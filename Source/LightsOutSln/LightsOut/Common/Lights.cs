using LightsOut.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Common
{
    /// <summary>
    /// Holds common light operations
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Lights
    {
        /// <summary>
        /// Enables a light
        /// </summary>
        /// <param name="light">The <see cref="Building"/> to enable</param>
        public static void EnableLight(ThingWithComps light)
        {
            DebugLogger.AssertFalse(light is null, "EnableLight called with a null light");
            if (light is null) return;

            DebugLogger.LogInfo($"Enabling light with ID: {light.ThingID} on map: {light.Map.uniqueID}", DebugMessageKeys.Lights);

            ThingComp glower = Glowers.GetGlower(light);
            DebugLogger.AssertFalse(glower is null, $"EnableLight called on a building without an approved glower: {light.def.defName}");
            if (glower is null) return;

            Glowers.EnableGlower(glower);
        }

        /// <summary>
        /// Disables a light
        /// </summary>
        /// <param name="light">The <see cref="Building"/> to disable</param>
        /// <param name="delay">The delay (in ticks) to keep the light on for</param>
        public static void DisableLight(ThingWithComps light, int? delay = 0)
        {
            DebugLogger.AssertFalse(light is null, "DisableLight called with a null light");
            if (light is null || !ModSettings.FlickLights) return; 

            DebugLogger.LogInfo($"Disabling light with ID: {light.ThingID} on map: {light.Map.uniqueID}", DebugMessageKeys.Lights);

            if (Rooms.GetRoom(light).OutdoorsForWork) return;

            // acknowledge the keep on setting
            if (KeepLightOn(light)) return;

            ThingComp glower = Glowers.GetGlower(light);
            DebugLogger.AssertFalse(glower is null, $"DisableLight called on a building without an approved glower: {light.def.defName}");
            if (glower is null) return;

            Glowers.DisableGlower(glower, delay);
        }

        /// <summary>
        /// Check if a light's KeepOnComp is keeping it on
        /// </summary>
        /// <param name="light">The light to check</param>
        /// <returns>
        /// <see langword="true"/> if the KeepOnComp is keeping
        /// the light on, <see langword="false"/> otherwise
        /// </returns>
        public static bool KeepLightOn(ThingWithComps light)
        {
            DebugLogger.AssertFalse(light is null, "KeepLightOn called on a null light");
            if (light is null) return false;

            // acknowledge the keep on setting
            KeepOnComp comp;
            if (KeepOnComps.ContainsKey(light))
                comp = KeepOnComps[light];
            else
            {
                comp = light.TryGetComp<KeepOnComp>();
                KeepOnComps.Add(light, comp);
            }

            DebugLogger.AssertFalse(comp is null, $"Found a light that doesn't have a KeepOnComp: {light.def.defName}", true);
            return comp?.KeepOn ?? false;
        }

        /// <summary>
        /// Goes through a room and disables all lights in it
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to disable the lights in</param>
        /// <param name="respectDelay">Whether to respect the delay or not</param>
        public static void DisableAllLights(Room room, bool respectDelay = true)
        {
            DebugLogger.AssertFalse(room is null, "DisableAllLights called on a null room");
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
                        DebugLogger.LogWarning($"(DisableAllLights): InvalidOperationException: {e.Message}", DebugMessageKeys.Lights);
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                DebugLogger.LogWarning($"(DisableAllLights): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.", DebugMessageKeys.Lights);

            // now actually go through the collection
            foreach (Thing thing in things)
            {
                if (thing is ThingWithComps building && CanBeLight(building) && Rooms.IsInRoom(building, room))
                    DisableLight(building, respectDelay ? ModSettings.DelayTicks : 0);
            }
        }

        /// <summary>
        /// Goes through a room and enables all lights in it
        /// </summary>
        /// <param name="room">The <see cref="Room"/> to enables the lights in</param>
        public static void EnableAllLights(Room room)
        {
            DebugLogger.AssertFalse(room is null, "EnableAllLights called on a null room");
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
                        DebugLogger.LogWarning($"(EnableAllLights): InvalidOperationException: {e.Message}", DebugMessageKeys.Lights);
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                DebugLogger.LogWarning($"(EnableAllLights): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.", DebugMessageKeys.Lights);

            // now actually go through the collection
            foreach (Thing thing in things)
            {
                if (thing is ThingWithComps building && CanBeLight(building) && Rooms.IsInRoom(building, room))
                    EnableLight(building);
            }
        }

        /// <summary>
        /// Check if the mod should turn off all of the
        /// lights in the specified <paramref name="room"/>
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <param name="excludedPawn">A pawn to disregard when 
        /// checking (i.e the pawn leaving the room)</param>
        /// <returns><see langword="true"/> if the mod should shut
        /// off the lights, <see langword="false"/> otherwise</returns>
        public static bool ShouldTurnOffAllLights(Room room, Pawn excludedPawn)
        {
            // if pawns are allowed to turn off the lights at night
            // then only check if all pawns are asleep (which intrinsically
            // also checks for pawn presence)
            if (!ModSettings.NightLights)
                return Rooms.AllPawnsSleeping(room, excludedPawn);

            // otherwise, if we aren't allowed to turn off the lights in normal rooms then bail out
            if (!ModSettings.FlickLights)
                return false;

            // otherwise only check for pawns in the room
            return Rooms.RoomIsEmpty(room, excludedPawn);
        }

        /// <summary>
        /// Check if something could possibly be a light
        /// </summary>
        /// <param name="building">The building to check</param>
        /// <returns><see langword="true"/> if <paramref name="building"/> could
        /// be a light, <see langword="false"/> otherwise</returns>
        public static bool CanBeLight(ThingWithComps building)
        {
            DebugLogger.AssertFalse(building is null, "CanBeLight called on a null building");
            if (building is null) return false;
            if (building is MinifiedThing) return false;
            if (Resources.MemoizedThings.ContainsKey(building))
                return Resources.MemoizedThings[building] == Resources.ThingType.Light;

            if (HasDisallowedCompForLights(building))
                return false;

            // double check that it actually has a glower
            if (Glowers.GetGlower(building) is null)
                return false;

            // make sure it doesn't have a disallowed name
            string defName = building.def.defName.ToLower();
            foreach (string keyword in LightNamesMustNotInclude)
                if (defName.Contains(keyword.ToLower()))
                    return false;

            // make sure it has one of the light kewords in its def name
            foreach (string keyword in LightNamesMustInclude)
                if (defName.Contains(keyword.ToLower()))
                {
                    Resources.MemoizedThings.Add(building, Resources.ThingType.Light);
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Checks to see if a <see cref="ThingWithComps"/> has any
        /// comps that are in the disallow list for a light
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if <paramref name="thing"/> has
        /// a comp in the disallowed list, <see langword="false"/> otherwise</returns>
        public static bool HasDisallowedCompForLights(ThingWithComps thing)
        {
            // Check against the list of disallowed comps
            // use AND logic here because we don't have access to
            // the "is not null" construct. It's clunky but it works
            if (
                thing.TryGetComp<CompPowerPlant>() is null
                && thing.TryGetComp<CompHeatPusher>() is null
                && thing.TryGetComp<CompSchedule>() is null
                && thing.TryGetComp<CompTempControl>() is null
                && thing.TryGetComp<CompShipLandingBeacon>() is null
                )
            {
                return false;
            }

            // otherwise it did have a disallowed comp
            return true;
        }

        /// <summary>
        /// Adds a new requirement that a light's def name can include
        /// </summary>
        /// <param name="nameMustInclude">A string that a light's def name should be able to include</param>
        /// <remarks>Case insensitive. Light def names must include at least ONE of these required terms</remarks>
        public static void AddLightNameRequirement(string nameMustInclude)
        {
            DebugLogger.AssertFalse(string.IsNullOrWhiteSpace(nameMustInclude), "AddLightNameRequirement called with a null argument");
            LightNamesMustInclude.Add(nameMustInclude);
        }

        /// <summary>
        /// Adds a new string that a light's def name should NOT include
        /// </summary>
        /// <param name="nameMustNotInclude">The string that no light's def name should include</param>
        /// <remarks>Case insensitive. Light def names CANNOT include any of these terms</remarks>
        public static void AddIllegalLightName(string nameMustNotInclude)
        {
            DebugLogger.AssertFalse(string.IsNullOrWhiteSpace(nameMustNotInclude), "AddIllegalLightName called with a null argument");
            LightNamesMustNotInclude.Add(nameMustNotInclude);
        }

        /// <summary>
        /// A helper method to remove the cached data about a light
        /// </summary>
        /// <param name="thing">The thing to remove</param>
        public static void RemoveLight(ThingWithComps thing)
        {
            if (thing is null) return;
            KeepOnComps.Remove(thing);
        }

        /// <summary>
        /// List of things that a light name MUST include to be considered
        /// </summary>
        private static List<string> LightNamesMustInclude { get; } = new List<string>()
        {
            "light",
            "lamp",
            "illuminated"
        };

        /// <summary>
        /// List of things that a light name MUST NOT include to be considered
        /// </summary>
        private static List<string> LightNamesMustNotInclude { get; } = new List<string>() { };
        
        /// <summary>
        /// A cached list of KeepOnComps to prevent repeated comp lookups
        /// </summary>
        private static Dictionary<ThingWithComps, KeepOnComp> KeepOnComps { get; } = new Dictionary<ThingWithComps, KeepOnComp>();
    }
}