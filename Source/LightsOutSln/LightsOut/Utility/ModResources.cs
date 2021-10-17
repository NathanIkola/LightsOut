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
using Verse.AI;
using LightsOut.ThingComps;

namespace LightsOut.Utility
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
            if (!ModSettings.FlickLights) return;

            CompPowerTrader powerTrader = light?.Key;
            ThingComp glower = light?.Value;

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
            if (room is null || !ModSettings.FlickLights) return;

            bool done = false;
            uint attempts = 0;
            while (!done)
            {
                try
                {
                    foreach (Thing t in room.ContainedAndAdjacentThings)
                    {
                        if (t is Building thing)
                        {
                            LightObject? light = ModResources.GetLightResources(thing);

                            if (light is null || !ModResources.IsInRoom(thing, room)) continue;

                            ModResources.DisableLight(light);
                        }
                    }
                    done = true;
                }
                catch (InvalidOperationException) { ++attempts; }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](DisableAllLights): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");
        }

        //****************************************
        // Enable all lights in a room
        //****************************************
        public static void EnableAllLights(Room room)
        {
            if (room is null) return;

            bool done = false;
            uint attempts = 0;
            while (!done)
            {
                try
                {
                    foreach (Thing t in room.ContainedAndAdjacentThings)
                    {
                        if (t is Building thing)
                        {
                            LightObject? light = ModResources.GetLightResources(thing);

                            if (light is null || !ModResources.IsInRoom(thing, room)) continue;

                            ModResources.EnableLight(light);
                        }
                    }
                    done = true;
                }
                catch (InvalidOperationException) { ++attempts; }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](EnableAllLights): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");
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
            bool? previous = CanConsumePower(powerComp);
            BuildingStatus[powerComp.parent] = consumesPower;

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
            if (powerComp == null) return false;
            if (IsCharged(powerComp.parent)) return false;
            return BuildingStatus.TryGetValue(powerComp.parent, null);
        }

        //****************************************
        // Check if a power consumer is a 
        // rechargeable building
        //****************************************
        public static bool IsCharged(ThingWithComps thing)
        {
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
            bool? previous = CanGlow(glower);
            BuildingStatus[glower.parent] = canGlow;

            // only update if the state has changed
            if(previous is null || canGlow is null || previous != canGlow)
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
            return BuildingStatus.TryGetValue(glower.parent, null);
        }

        //****************************************
        // Checks if a building can even be a
        // light in the first place
        //****************************************
        public static bool CanBeLight(Building thing)
        {
            if (MemoizedCanBeLight.ContainsKey(thing))
                return MemoizedCanBeLight[thing];

            // make sure that this is not on the blacklist
            foreach (ThingComp comp in thing.AllComps)
                if (LightCompBlacklist.Any(badComp => badComp.IsAssignableFrom(comp.GetType())))
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
        // Detect if a pawn is ACTUALLY asleep
        //****************************************
        public static bool Sleeping(this Pawn pawn)
        {
            return (pawn?.CurJob?.GetCachedDriver(pawn) is JobDriver_LayDownResting);
        }

        //****************************************
        // Get the glower (if present) from a
        // building, or return null if not present
        //****************************************
        public static ThingComp GetGlower(Building thing)
        {
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

            foreach(ThingComp comp in thing.AllComps)
            {
                if (TableCompBlacklist.Any(x => x.IsAssignableFrom(comp.GetType())))
                {
                    MemoizedIsTable.Add(thing, false);
                    return false;
                }
            }

            bool isTable = ((thing is Building_WorkTable || thing is Building_ResearchBench) 
                && thing.PowerComp != null);
            MemoizedIsTable.Add(thing, isTable);
            return isTable;
        }

        //****************************************
        // Get whether something is rechargeable
        //****************************************
        public static bool IsRechargeable(Building thing)
        {
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
            if (room is null || room.OutdoorsForWork || room.IsDoorway) return false;

            bool done = false;
            uint attempts = 0;
            while(!done)
            {
                try
                {
                    foreach (Thing thing in room.ContainedAndAdjacentThings)
                        if (thing is Pawn otherPawn && otherPawn.RaceProps.Humanlike && otherPawn != pawn
                            // what if two pawns were both leaving the room at the same time haha... unless?
                            && (otherPawn.pather.nextCell.GetEdifice(otherPawn.Map) as Building_Door) == null
                            // what if a pawn is entering while another pawn is leaving haha... unless??
                            && (otherPawn.Position.GetEdifice(otherPawn.Map) as Building_Door) == null)
                        {
                            if (attempts > 1)
                                Log.Warning($"[LightsOut](RoomIsEmpty): collection was unexpectedly updated {attempts} time(s). If this number is big please report it.");
                            return false;
                        }
                    done = true;
                }
                catch(InvalidOperationException) { ++attempts; }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](RoomIsEmpty): collection was unexpectedly updated {attempts} time(s). If this number is big please report it.");
            
            return true;
        }

        //****************************************
        // Detect if all pawns in a room are
        // currently sleeping except the one
        // passed in
        //****************************************
        public static bool AllPawnsSleeping(Room room, Pawn pawn)
        {
            if (room is null || room.OutdoorsForWork || room.IsDoorway) return false;

            bool done = false;
            uint attempts = 0;
            while(!done)
            {
                try
                {
                    foreach (Thing thing in room.ContainedAndAdjacentThings)
                        if (thing is Pawn otherPawn && otherPawn.RaceProps.Humanlike && otherPawn != pawn)
                        {
                            if (otherPawn.jobs?.curDriver?.asleep == false)
                                return false;
                        }
                    done = true;
                }
                catch(InvalidOperationException) { ++attempts; }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](AllPawnsSleeping): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");
            
            return true;
        }

        //****************************************
        // Check if a building is in a room
        //****************************************
        public static bool IsInRoom(Building building, Room room)
        {
            if (building is null || room is null) return false;
            return GetRoom(building)?.ID == room.ID;
        }

        //****************************************
        // Returns the room a particular
        // building is a part of
        //****************************************
        public static Room GetRoom(Building building)
        {
            if (!(bool)(building?.Map?.regionAndRoomUpdater?.Enabled))
                return null;
            return building.GetRoom();
        }

        //****************************************
        // Update any glower
        //****************************************
        private static void UpdateGlower(ThingComp glower)
        {
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
            ThingWithComps thing = glower.parent;
            MethodInfo updateLit = glower.GetType().GetMethod("UpdateLit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if(updateLit != null)
            {
                try
                {
                    if (updateLit.GetParameters().Length == 1 && updateLit.GetParameters()[0].ParameterType == typeof(Map))
                        updateLit.Invoke(glower, new object[] { thing.Map });
                    else if (updateLit.GetParameters().Length == 0)
                        updateLit.Invoke(glower, null);
                }
                catch (Exception) { }
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

        // list of ThingComp types that differentiates things that
        // look like lights from things that are actually lights
        // can be changed at runtime!
        public static List<Type> LightCompBlacklist { get; } = new List<Type>()
        { 
            // ignore the landing beacon
            typeof(CompShipLandingBeacon),
            // ignore generators
            typeof(CompPowerPlant),
            // ignore grow lights
            typeof(CompHeatPusher),
            typeof(CompSchedule),
            typeof(CompTempControl),
        };

        // list of ThingComp types that differentiate things that
        // look like benches from things that are actually benches
        // can be changed at runtime!
        public static List<Type> TableCompBlacklist { get; } = new List<Type>()
        {

        };

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
            new AndroidsCompatibilityPatch()
        };

        public static List<Type> CompGlowers { get; } = new List<Type>() { typeof(CompGlower) };
    }
}