using HarmonyLib;
using LightsOut.Common;
using LightsOut.Patches.ModCompatibility;
using System.Reflection;
using UnityEngine;
using Verse;

namespace LightsOut.Boilerplate
{
    /// <summary>
    /// The actual mod object
    /// </summary>
    public class LightsOutMod : Mod
    {
        /// <summary>
        /// The identifier this mod uses to identify itself in-game
        /// </summary>
        public const string ModIdentifier = "LightsOut";

        /// <summary>
        /// Default constructor that sets the Harmony instance. 
        /// This should only be called by the game when it loads the mod.
        /// </summary>
        public LightsOutMod(ModContentPack content)
            : base(content)
        {
            Log.Message("Initializing LightsOut [" + typeof(LightsOutSettings).Assembly.GetName().Version + "]");
            Settings = GetSettings<LightsOutSettings>();
            HarmonyInstance = new Harmony(ModIdentifier);
            foreach (Assembly asm in content.assemblies.loadedAssemblies)
                HarmonyInstance.PatchAll(asm);
        }

        /// <summary>
        /// The Harmony instance associated with this mod
        /// </summary>
        public static Harmony HarmonyInstance { get; private set; }

        /// <summary>
        /// The settings for the mod
        /// </summary>
        public LightsOutSettings Settings { get; private set; }

        /// <summary>
        /// The function called when the game finishes loading defs
        /// </summary>
        public static void InitializeMod()
        {
            ModCompatibilityManager.LoadCompatibilityPatches();
        }

        /// <summary>
        /// How this mod should be shown in the settings menu
        /// </summary>
        /// <returns>LightsOut</returns>
        public override string SettingsCategory() => ModIdentifier;

        /// <summary>
        /// Renders the settings window
        /// </summary>
        /// <param name="inRect">The rectangle to render the settings in</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            Settings.DrawSettingsMenu(listingStandard);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
