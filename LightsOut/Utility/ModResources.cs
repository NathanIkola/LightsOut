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

namespace LightsOut.Utility
{
    public static class ModResources
    {
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
        // Check if a light has a comp on the
        // disallowed list
        //****************************************
        public static KeyValuePair<CompPowerTrader, ThingComp>? GetLightResources(Building thing)
        {
            if (thing is null || IsTable(thing)) return null;

            // use our light whitelist to cull out anything that
            // doesn't claim to be a light
            bool isLight = false;
            foreach(string term in LightNamesMustInclude)
            {
                if(thing.def.defName.ToLower().Contains(term))
                {
                    isLight = true;
                    break;
                }
            }
            if (!isLight) return null;

            foreach(ThingComp comp in thing.AllComps)
            {
                if (LightCompBlacklist.Any(x => x.IsAssignableFrom(comp.GetType())))
                    return null;
            }

            CompPowerTrader powerTrader = thing.PowerComp as CompPowerTrader;
            ThingComp glower = thing.TryGetComp<CompGlower>();

            if (glower is null)
                foreach (ThingComp comp in thing.AllComps)
                    if (comp.GetType().ToString().ToLower().Contains("glower"))
                        glower = comp;

            if (glower is null || powerTrader is null || powerTrader.powerOutputInt > 0)
                return null;
            return new KeyValuePair<CompPowerTrader, ThingComp>(powerTrader, glower);
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
                    return false;
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
    }
}