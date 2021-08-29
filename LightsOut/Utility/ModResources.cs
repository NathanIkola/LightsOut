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

            SetConsumesPower(powerTrader, true);
            SetCanGlow(glower, true);
        }

        //****************************************
        // Does the hard work of disabling a light
        //****************************************
        public static void DisableLight(LightObject? light)
        {
            CompPowerTrader powerTrader = light?.Key;
            ThingComp glower = light?.Value;

            SetConsumesPower(powerTrader, false);
            SetCanGlow(glower, false);
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
            return BuildingStatus.TryGetValue(powerComp.parent, null);
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
            // make sure that this is not on the blacklist
            foreach (ThingComp comp in thing.AllComps)
                if (LightCompBlacklist.Any(badComp => badComp.IsAssignableFrom(comp.GetType())))
                    return false;

            // make sure it has one of the light kewords in its def name
            string defName = thing.def.defName.ToLower();
            foreach (string keyword in LightNamesMustInclude)
                if (defName.Contains(keyword))
                    return true;

            return false;
        }

        //****************************************
        // Get the glower (if present) from a
        // building, or return null if not present
        //****************************************
        public static ThingComp GetGlower(Building thing)
        {
            return thing.AllComps.First(x => CompGlowers.Contains(x.GetType())); ;
        }

        //****************************************
        // Check if a light has a comp on the
        // disallowed list
        //****************************************
        public static LightObject? GetLightResources(Building thing)
        {
            if (thing is null || IsTable(thing)) return null;

            // use our light whitelist to cull out anything that doesn't claim to be a light
            if (!CanBeLight(thing)) return null;

            CompPowerTrader powerTrader = thing.PowerComp as CompPowerTrader;
            ThingComp glower = GetGlower(thing);

            if (glower is null || powerTrader is null || powerTrader.powerOutputInt > 0)
                return null;
            return new LightObject(powerTrader, glower);
        }

        //****************************************
        // Check if a table has a comp on the
        // disallowed list
        //****************************************
        public static bool IsTable(Building thing)
        {
            if (thing is null) return false;

            foreach(ThingComp comp in thing.AllComps)
            {
                if (TableCompBlacklist.Any(x => x.IsAssignableFrom(comp.GetType())))
                    return false;
            }
            return ((thing is Building_WorkTable || thing is Building_ResearchBench) && thing.PowerComp != null);
        }

        //****************************************
        // Detect if any pawns are in the room
        // or if the room is outdoors
        //****************************************
        public static bool RoomIsEmpty(Room room, Pawn pawn)
        {
            if (room is null || room.PsychologicallyOutdoors) return false;

            foreach (Thing thing in room.ContainedAndAdjacentThings)
                if (thing is Pawn otherPawn && otherPawn.RaceProps.Humanlike && otherPawn != pawn
                    // what if two pawns were both leaving the room at the same time haha... unless?
                    && (otherPawn.pather.nextCell.GetEdifice(otherPawn.Map) as Building_Door) == null)
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
            if (room is null || room.PsychologicallyOutdoors) return false;

            foreach(Thing thing in room.ContainedAndAdjacentThings)
                if(thing is Pawn otherPawn && otherPawn.RaceProps.Humanlike && otherPawn != pawn
                    // what if two pawns were both leaving the room at the same time haha... unless?
                    && (otherPawn.pather.nextCell.GetEdifice(otherPawn.Map) as Building_Door) == null)
                {
                    if (otherPawn.Awake()) return false;
                }
            return true;
        }

        //****************************************
        // Get the list of pawns in the room
        //****************************************
        public static List<Pawn> GetPawnsInRoom(Room room)
        {
            List<Pawn> pawns = new List<Pawn>();
            if (room is null || room.PsychologicallyOutdoors) return pawns;

            // just get all of the humans in the room
            foreach (Thing thing in room.ContainedAndAdjacentThings)
                if (thing is Pawn pawn && pawn.RaceProps.Humanlike)
                    pawns.Add(pawn);

            return pawns;
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
            if (!building.Map.regionAndRoomUpdater.Enabled)
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
            typeof(CompSchedule)
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
            "lamp"
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