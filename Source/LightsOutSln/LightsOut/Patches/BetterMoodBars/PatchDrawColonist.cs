//************************************************
// Fix the yeah
//************************************************

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace LightsOut.Patches.BetterMoodBars
{
    //[HarmonyPatch(typeof(ColonistBarColonistDrawer))]
    //[HarmonyPatch(nameof(ColonistBarColonistDrawer.DrawColonist))]
    public class PatchDrawColonist
    {
        public static Texture2D BadTex = null;
        public static Texture2D OkayTex = null;
        public static Texture2D GoodTex = null;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            MethodInfo DrawTexture = typeof(GUI).GetMethod(nameof(GUI.DrawTexture),new Type[] { typeof(Rect), typeof(Texture2D) });
            int startIndex = -1;

            for(int index = 0; index < codes.Count; ++index)
            {
                if(codes[index].Calls(DrawTexture))
                {
                    startIndex = index - 1;
                    break;
                }
            }

            if (startIndex != -1)
            {
                // load in the pawn
                codes[startIndex].opcode = OpCodes.Ldarg_2;
                codes[startIndex].operand = null;

                // point it at our custom method
                MethodInfo DrawBg = typeof(PatchDrawColonist).GetMethod(nameof(DrawBackground));
                codes[startIndex + 1].operand = DrawBg;
            }

            return codes.AsEnumerable();
        }

        public static void DrawBackground(Rect rect, Pawn pawn)
        {
            if(BadTex is null || BadTex.width != rect.width)
            {
                Color color = Color.red;
                BadTex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);

                for (int y = 0; y < rect.height; ++y)
                    for (int x = 0; x < rect.width; ++x)
                        BadTex.SetPixel(x, y, color);
            }

            if (OkayTex is null || OkayTex.width != rect.width)
            {
                Color color = Color.yellow;
                OkayTex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);

                for (int y = 0; y < rect.height; ++y)
                    for (int x = 0; x < rect.width; ++x)
                        OkayTex.SetPixel(x, y, color);
            }

            if (GoodTex is null || GoodTex.width != rect.width)
            {
                Color color = Color.green;
                GoodTex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);

                for (int y = 0; y < rect.height; ++y)
                    for (int x = 0; x < rect.width; ++x)
                        GoodTex.SetPixel(x, y, color);
            }

            float curLevel = pawn.needs.mood.CurLevel;
            float mentalBreak = pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true);

            if (curLevel <= mentalBreak)
                GUI.DrawTexture(rect, BadTex);
            else if(curLevel <= 0.65f)
                GUI.DrawTexture(rect, OkayTex);
            else
                GUI.DrawTexture(rect, GoodTex);
        }
    }
}