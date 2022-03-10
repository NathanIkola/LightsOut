//************************************************
// Holds all of the common light operations
//************************************************

using LightsOut.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Common
{
    [StaticConstructorOnStartup]
    public static class Lights
    {
        //****************************************
        // Does the hard work of enabling a light
        //****************************************
        public static void EnableLight(ThingComp glower)
        {
            // imagine if I'd spent like half an hour
            // troubleshooting why I was getting a nullex
            // here after already solving it in the disable
            // function haha... :(
            if (glower is null) return;

            Common.Glowers.SetCanGlow(glower, true);
        }

        //****************************************
        // Does the hard work of disabling a light
        //****************************************
        public static void DisableLight(ThingComp glower)
        {
            if (glower is null || !ModSettings.FlickLights) return;
            ThingWithComps parent = glower.parent;

            if (Rooms.GetRoom((parent as Building)).OutdoorsForWork)
                return;

            // acknowledge the keep on setting
            KeepOnComp comp = null;
            if (KeepOnComps.ContainsKey(parent))
                comp = KeepOnComps[parent];
            else
            {
                comp = parent.TryGetComp<KeepOnComp>();
                KeepOnComps.Add(parent, comp);
            }
            if (comp?.KeepOn == true) return;

            Common.Glowers.SetCanGlow(glower, false);
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
                    ThingComp glower = GetGlower(thing);

                    if (glower is null || !Rooms.IsInRoom(thing, room)) continue;

                    DisableLight(glower);
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
                    ThingComp glower = GetGlower(thing);

                    if (glower is null || !Rooms.IsInRoom(thing, room)) continue;

                    EnableLight(glower);
                }
            }
        }

        //****************************************
        // Check if a light has a comp on the
        // disallowed list
        //****************************************
        public static ThingComp GetGlower(Building thing)
        {
            if (Glowers.ContainsKey(thing))
                return Glowers[thing];

            if (thing is null || Tables.IsTable(thing))
            {
                Glowers.Add(thing, null);
                return null;
            }

            // use our light whitelist to cull out anything that doesn't claim to be a light
            if (!CanBeLight(thing))
            {
                Glowers.Add(thing, null);
                return null;
            }

            ThingComp glower = Common.Glowers.GetGlower(thing);

            if (glower is null)
            {
                Glowers.Add(thing, null);
                return null;
            }

            Glowers.Add(thing, glower);
            return glower;
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
                return Rooms.AllPawnsSleeping(room, pawn);

            // otherwise only check for pawns in the room
            return Rooms.RoomIsEmpty(room, pawn);
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

        // whitelist for things that can be lights
        public static List<string> LightNamesMustInclude { get; } = new List<string>()
        {
            "light",
            "lamp",
            "illuminated"
        };

        private static Dictionary<Building, bool> MemoizedCanBeLight { get; } = new Dictionary<Building, bool>();
        private static Dictionary<Building, ThingComp> Glowers { get; } = new Dictionary<Building, ThingComp>();
        private static Dictionary<ThingWithComps, KeepOnComp> KeepOnComps { get; } = new Dictionary<ThingWithComps, KeepOnComp>();
    }
}