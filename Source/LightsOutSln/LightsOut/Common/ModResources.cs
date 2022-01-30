//************************************************
// Goulash class of various resources
//************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using System.Reflection;
using LightsOut.Patches.ModCompatibility;
using LightsOut.Patches.ModCompatibility.WallLights;
using LightsOut.Patches.ModCompatibility.Androids;
using ModSettings = LightsOut.Boilerplate.ModSettings;
using LightsOut.ThingComps;
using LightsOut.Patches.ModCompatibility.VEWallLights;

namespace LightsOut.Common
{
    using LightObject = KeyValuePair<CompPowerTrader, ThingComp>;

    public static class ModResources
    {
        //****************************************
        // Does the hard work of disabling
        // a worktable
        //****************************************
        public static void DisableTable(Building table)
        {
            if (table is null) return;
            SetConsumesPower(table.PowerComp as CompPowerTrader, false);
            ThingComp glower = GetGlower(table);
            if (!(glower is null) && ModSettings.StandbyPowerDrawRate == 0f)
                SetCanGlow(glower, false);
        }

        //****************************************
        // Does the hard work of enabling
        // a worktable
        //****************************************
        public static void EnableTable(Building table)
        {
            if (table is null) return;
            SetConsumesPower(table.PowerComp as CompPowerTrader, true);
            ThingComp glower = GetGlower(table);
            if (!(glower is null) && ModSettings.StandbyPowerDrawRate == 0f)
                SetCanGlow(glower, true);
        }

        //****************************************
        // Does the hard work of enabling a light
        //****************************************
        public static void EnableLight(LightObject? light)
        {
            // imagine if I'd spent like half an hour
            // troubleshooting why I was getting a nullex
            // here after already solving it in the disable
            // function haha... :(
            if (light is null) return;

            CompPowerTrader powerTrader = light?.Key;
            ThingComp glower = light?.Value;

            SetCanGlow(glower, true);
            SetConsumesPower(powerTrader, true);
        }

        //****************************************
        // Does the hard work of disabling a light
        //****************************************
        public static void DisableLight(LightObject? light)
        {
            if (light is null || !ModSettings.FlickLights) return;

            CompPowerTrader powerTrader = light?.Key;
            ThingComp glower = light?.Value;

            if (GetRoom((glower.parent as Building)).OutdoorsForWork)
                return;

            // acknowledge the keep on setting
            KeepOnComp comp = null;
            if (KeepOnComps.ContainsKey(glower.parent))
                comp = KeepOnComps[glower.parent];
            else
            {
                comp = glower.parent.TryGetComp<KeepOnComp>();
                KeepOnComps.Add(glower.parent, comp);
            }
            if (comp?.KeepOn == true) return;

            SetCanGlow(glower, false);
            SetConsumesPower(powerTrader, false);
        }

        //****************************************
        // Disable all lights in a room
        //****************************************
        public static void DisableAllLights(Room room)
        {
            if (room is null || room.OutdoorsForWork || !ModSettings.FlickLights
                || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return;

            bool done = false;
            uint attempts = 0;
            Thing[] things = null;
            while (!done)
            {
                try
                {
                    things = room.ContainedAndAdjacentThings.ToArray();
                    done = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) done = true;
                    }
                    else
                    {
                        Log.Warning($"[LightsOut](DisableAllLights): InvalidOperationException: {e.Message}");
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](DisableAllLights): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");

            // now actually go through the collection
            foreach (Thing t in things)
            {
                if (t is Building thing)
                {
                    LightObject? light = ModResources.GetLightResources(thing);

                    if (light is null || !ModResources.IsInRoom(thing, room)) continue;

                    DisableLight(light);
                }
            }
        }

        //****************************************
        // Enable all lights in a room
        //****************************************
        public static void EnableAllLights(Room room)
        {
            if (room is null || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return;

            bool done = false;
            uint attempts = 0;
            Thing[] things = null;
            while (!done)
            {
                try
                {
                    things = room.ContainedAndAdjacentThings.ToArray();
                    done = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) done = true;
                    }
                    else
                    {
                        Log.Warning($"[LightsOut](EnableAllLights): InvalidOperationException: {e.Message}");
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](EnableAllLights): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");

            // now actually go through the collection
            foreach (Thing t in things)
            {
                if (t is Building thing)
                {
                    LightObject? light = ModResources.GetLightResources(thing);

                    if (light is null || !ModResources.IsInRoom(thing, room)) continue;

                    EnableLight(light);
                }
            }
        }

        //****************************************
        // Add a CompPower to the power-consumable
        // dictionary
        //
        // Returns: the previous value or null
        // if it was not in the dictionary before
        //****************************************
        public static bool? SetConsumesPower(CompPower powerComp, bool? consumesPower)
        {
            if (powerComp == null) return null;
            bool? previous = CanConsumePower(powerComp);
            BuildingStatus[powerComp.parent] = consumesPower;

            // make sure to reset the power status
            if (!(consumesPower is null) && powerComp is CompPowerTrader trader)
                trader.PowerOutput = -trader.Props.basePowerConsumption;

            return previous;
        }

        //****************************************
        // Check if a CompPower is able to
        // consume power
        //
        // Returns: whether or not the CompPower
        // can consume power, or null if it is
        // not in the dictionary
        //****************************************
        public static bool? CanConsumePower(CompPower powerComp)
        {
            if (powerComp == null) return null;
            if (IsCharged(powerComp.parent)) return false;
            return BuildingStatus.TryGetValue(powerComp.parent, null);
        }

        //****************************************
        // Check if a power consumer is a 
        // rechargeable building
        //****************************************
        public static bool IsCharged(ThingWithComps thing)
        {
            if (thing is null) return false;
            CompRechargeable rechargeable = null;
            if (CompRechargeables.ContainsKey(thing))
                rechargeable = CompRechargeables[thing];
            else
            {
                rechargeable = thing.GetComp<CompRechargeable>();
                CompRechargeables.Add(thing, rechargeable);
            }

            if (rechargeable is null) return false;
            return rechargeable.Charged;
        }

        //****************************************
        // Add a glower to the glowable dictionary
        //
        // Returns: the previous value or null
        // if it was not in the dictionary before
        //****************************************
        public static bool? SetCanGlow(ThingComp glower, bool? canGlow)
        {
            if (glower is null) return false;
            bool? previous = CanGlow(glower);
            BuildingStatus[glower.parent] = canGlow;

            // only update if the state has changed
            if (previous is null || canGlow is null || previous != canGlow)
                UpdateGlower(glower);

            return previous;
        }

        //****************************************
        // Add a glower to the glowable dictionary
        //
        // Returns: whether or not the glower
        // is able to glow, or null if it is
        // not in the dictionary
        //****************************************
        public static bool? CanGlow(ThingComp glower)
        {
            if (glower is null) return false;
            return BuildingStatus.TryGetValue(glower.parent, null);
        }

        //****************************************
        // Checks if a building can even be a
        // light in the first place
        //****************************************
        public static bool CanBeLight(Building thing)
        {
            if (thing is null) return false;
            if (MemoizedCanBeLight.ContainsKey(thing))
                return MemoizedCanBeLight[thing];

            if (HasBlacklistedLightComp(thing))
            {
                MemoizedCanBeLight.Add(thing, false);
                return false;
            }

            // make sure it has one of the light kewords in its def name
            string defName = thing.def.defName.ToLower();
            foreach (string keyword in LightNamesMustInclude)
                if (defName.Contains(keyword))
                {
                    MemoizedCanBeLight.Add(thing, true);
                    return true;
                }

            MemoizedCanBeLight.Add(thing, false);
            return false;
        }

        //****************************************
        // Get the glower (if present) from a
        // building, or return null if not present
        //****************************************
        public static ThingComp GetGlower(Building thing)
        {
            if (thing is null) return null;
            if (Glowers.ContainsKey(thing))
                return Glowers[thing];

            try
            {
                ThingComp glower = thing.AllComps.First(x => CompGlowers.Contains(x.GetType()));
                Glowers.Add(thing, glower);
                return glower;
            }
            catch (Exception) { return null; }
        }

        //****************************************
        // Check if a light has a comp on the
        // disallowed list
        //****************************************
        public static LightObject? GetLightResources(Building thing)
        {
            if (LightObjects.ContainsKey(thing))
                return LightObjects[thing];

            if (thing is null || IsTable(thing))
            {
                LightObjects.Add(thing, null);
                return null;
            }

            // use our light whitelist to cull out anything that doesn't claim to be a light
            if (!CanBeLight(thing))
            {
                LightObjects.Add(thing, null);
                return null;
            }

            CompPowerTrader powerTrader = thing.PowerComp as CompPowerTrader;
            ThingComp glower = GetGlower(thing);

            if (glower is null || powerTrader is null || powerTrader.powerOutputInt > 0)
            {
                LightObjects.Add(thing, null);
                return null;
            }

            LightObject? light = new LightObject(powerTrader, glower);
            LightObjects.Add(thing, light);
            return light;
        }

        //****************************************
        // Check if a table has a comp on the
        // disallowed list
        //****************************************
        public static bool IsTable(Building thing)
        {
            if (MemoizedIsTable.ContainsKey(thing))
                return MemoizedIsTable[thing];

            if (thing is null)
            {
                MemoizedIsTable.Add(thing, false);
                return false;
            }

            if (HasBlacklistedTableComp(thing))
            {
                MemoizedIsTable.Add(thing, false);
                return false;
            }

            bool isTable = ((thing is Building_WorkTable || thing is Building_ResearchBench)
                && thing.PowerComp as CompPowerTrader != null);
            MemoizedIsTable.Add(thing, isTable);
            return isTable;
        }

        //****************************************
        // Get whether something is rechargeable
        //****************************************
        public static bool IsRechargeable(Building thing)
        {
            if (thing is null) return false;
            if (CompRechargeables.ContainsKey(thing))
                return CompRechargeables[thing] != null;

            CompRechargeable rechargeable = thing.GetComp<CompRechargeable>();
            CompRechargeables.Add(thing, rechargeable);
            return rechargeable != null;
        }

        //****************************************
        // Detect if any pawns are in the room
        // or if the room is outdoors
        //****************************************
        public static bool RoomIsEmpty(Room room, Pawn pawn)
        {
            if (room is null || room.OutdoorsForWork || room.IsDoorway
                || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return false;

            bool done = false;
            uint attempts = 0;
            Thing[] things = null;
            while (!done)
            {
                try
                {
                    things = room.ContainedAndAdjacentThings.ToArray();
                    done = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) done = true;
                    }
                    else
                    {
                        Log.Warning($"[LightsOut](RoomIsEmpty): InvalidOperationException: {e.Message}");
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](RoomIsEmpty): collection was unexpectedly updated {attempts} time(s). If this number is big please report it.");

            // now actually go through the collection
            foreach (Thing thing in things)
                if (thing is Pawn otherPawn && otherPawn.RaceProps.Humanlike && otherPawn != pawn
                    // what if two pawns were both leaving the room at the same time haha... unless?
                    && (otherPawn.pather.nextCell.GetEdifice(otherPawn.Map) as Building_Door) == null
                    // what if a pawn is entering while another pawn is leaving haha... unless??
                    && (otherPawn.Position.GetEdifice(otherPawn.Map) as Building_Door) == null)
                {
                    return false;
                }

            return true;
        }

        //****************************************
        // Detect if all pawns in a room are
        // currently sleeping except the one
        // passed in
        //****************************************
        public static bool AllPawnsSleeping(Room room, Pawn pawn)
        {
            if (room is null || room.OutdoorsForWork || room.IsDoorway
                || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return false;

            bool done = false;
            uint attempts = 0;
            Thing[] things = null;
            while (!done)
            {
                try
                {
                    things = room.ContainedAndAdjacentThings.ToArray();
                    done = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) done = true;
                    }
                    else
                    {
                        Log.Warning($"[LightsOut](AllPawnsSleeping): InvalidOperationException: {e.Message}");
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](AllPawnsSleeping): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");

            // now actually go through the collection
            foreach (Thing thing in things)
                if (thing is Pawn otherPawn && otherPawn.RaceProps.Humanlike && otherPawn != pawn
                    // what if two pawns were both leaving the room at the same time haha... unless?
                    && (otherPawn.pather.nextCell.GetEdifice(otherPawn.Map) as Building_Door) == null
                    // what if a pawn is entering while another pawn is leaving haha... unless??
                    && (otherPawn.Position.GetEdifice(otherPawn.Map) as Building_Door) == null)
                {
                    if (otherPawn.jobs?.curDriver?.asleep != true)
                        return false;
                }

            return true;
        }

        //****************************************
        // We only need to check a room once
        // but we currently do it twice, which
        // is really sad, so stop it
        //****************************************
        public static bool ShouldTurnOffAllLights(Room room, Pawn pawn)
        {
            // never turn off the lights if we aren't supposed to
            if (!ModSettings.FlickLights)
                return false;

            // if pawns are allowed to turn off the lights at night
            // then only check if all pawns are asleep (which intrinsically
            // also checks for pawn presence)
            if (!ModSettings.NightLights)
                return AllPawnsSleeping(room, pawn);

            // otherwise only check for pawns in the room
            return RoomIsEmpty(room, pawn);
        }

        //****************************************
        // Check if a building is in a room
        //****************************************
        public static bool IsInRoom(Building building, Room room)
        {
            if (building is null || room is null || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return false;
            return GetRoom(building)?.ID == room.ID;
        }

        //****************************************
        // Returns the room a particular
        // building is a part of
        //****************************************
        public static Room GetRoom(Building building)
        {
            if (building is null) return null;
            if (!(building.Map?.regionAndRoomUpdater?.Enabled ?? false))
                return null;
            return building.GetRoom();
        }

        //****************************************
        // Update any glower
        //****************************************
        private static void UpdateGlower(ThingComp glower)
        {
            if (glower is null) return;
            if (glower is CompGlower compGlower)
                compGlower.UpdateLit(compGlower.parent.Map);
            else
                TryUpdateGenericGlower(glower);
        }

        //****************************************
        // Try to update a generic glower
        //****************************************
        private static void TryUpdateGenericGlower(ThingComp glower)
        {
            if (glower is null) return;
            ThingWithComps thing = glower.parent;
            MethodInfo updateLit = glower.GetType().GetMethod("UpdateLit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateLit != null)
            {
                try
                {
                    if (updateLit.GetParameters().Length == 1 && updateLit.GetParameters()[0].ParameterType == typeof(Map))
                        updateLit.Invoke(glower, new object[] { thing.Map });
                    else if (updateLit.GetParameters().Length == 0)
                        updateLit.Invoke(glower, null);
                }
                catch (Exception e) 
                {
                    Log.Warning($"[LightsOut] Having trouble updating a generic glower: {glower.GetType()}: {e}");
                }
            }
        }

        // keep track of all disabled Things
        public static Dictionary<ThingWithComps, bool?> BuildingStatus { get; } = new Dictionary<ThingWithComps, bool?>();

        // finally, time to start memoizing things
        private static Dictionary<Building, bool> MemoizedIsTable { get; } = new Dictionary<Building, bool>();
        private static Dictionary<Building, bool> MemoizedCanBeLight { get; } = new Dictionary<Building, bool>();
        private static Dictionary<ThingWithComps, CompRechargeable> CompRechargeables { get; } = new Dictionary<ThingWithComps, CompRechargeable>();
        private static Dictionary<ThingWithComps, KeepOnComp> KeepOnComps { get; } = new Dictionary<ThingWithComps, KeepOnComp>();
        private static Dictionary<Building, ThingComp> Glowers { get; } = new Dictionary<Building, ThingComp>();
        private static Dictionary<Building, LightObject?> LightObjects { get; } = new Dictionary<Building, LightObject?>();

        //****************************************
        // Detects blacklisted comps on lights
        //
        // Used to be a list, but that has much
        // worse performance implications
        //****************************************
        public static bool HasBlacklistedLightComp(ThingWithComps thing)
        {
            // if all of the blacklisted comps are null
            if (
                thing.TryGetComp<CompPowerPlant>() is null
                && thing.TryGetComp<CompHeatPusher>() is null
                && thing.TryGetComp<CompSchedule>() is null
                && thing.TryGetComp<CompTempControl>() is null
                && thing.TryGetComp<CompShipLandingBeacon>() is null
                )
            { 
                // then this did not have a blacklisted comp
                return false;
            }
            // otherwise it did have a blacklisted comp
            else return true;
        }

        //****************************************
        // Detects blacklisted comps on tables
        //
        // Used to be a list, but that has much
        // worse performance implications
        //****************************************
        public static bool HasBlacklistedTableComp(ThingWithComps thing)
        {

            return false;
        }

        // whitelist for things that can be lights
        public static List<string> LightNamesMustInclude { get; } = new List<string>()
        {
            "light",
            "lamp",
            "illuminated"
        };

        // compatibility patches to apply
        public static List<IModCompatibilityPatch> CompatibilityPatches { get; } = new List<IModCompatibilityPatch>()
        {
            new WallLightCompatibilityPatch(),
            new AndroidsCompatibilityPatch(),
            new VEWallLightCompatibilityPatch()
        };

        public static List<Type> CompGlowers { get; } = new List<Type>() { typeof(CompGlower) };
    }
}