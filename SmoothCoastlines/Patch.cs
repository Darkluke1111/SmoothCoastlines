using Cairo;
using HarmonyLib;
using MapLayer;
using SmoothCoastlines.LandformHeights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;
using Vintagestory.ServerMods.NoObf;

namespace SmoothCoastlines
{
    [HarmonyPatch]
    public static class Patch
    {
        //This patch switches the vanilla MapOceanGen class for the custom MapOceanGenSmooth class
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GenMaps), nameof(GenMaps.GetOceanMapGen))]
        public static bool Prefix(ref MapLayerBase __result, long seed, float landcover, int oceanMapScale, float oceanScaleMul, List<XZ> requireLandAt, bool requiresSpawnOffset)
        {
            __result = new MapLayerOceansSmooth(seed, SmoothCoastlinesModSystem.config, requireLandAt);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GenMaps), nameof(GenMaps.GetLandformMapGen))]
        public static bool Prefix(ref MapLayerBase __result, long seed, NoiseClimate climateNoise, ICoreServerAPI api, float landformScale) {
            MapLayerLandformsSmooth mapLayerLandformsSmooth = new MapLayerLandformsSmooth(seed + 12, climateNoise, api, landformScale, SmoothCoastlinesModSystem.config);
            mapLayerLandformsSmooth.DebugDrawBitmap(DebugDrawMode.LandformRGB, 0, 0, "Height-Based Landforms");
            __result = mapLayerLandformsSmooth;
            
            return false;
        }

        //This patch changes the way that GenMaps generates the list of areas where the ocean map is forced to have land (for spawn and story structures) because MapOceanGenSmooth does this stuff differently than the vanilla one.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GenMaps), "ForceRandomLandArea")]
        public static bool Prefix(GenMaps __instance, int positionX, int positionZ, int radius)
        {
            var sapi = (ICoreServerAPI)__instance.GetType().GetField("sapi", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            double regionSize = sapi.WorldManager.RegionSize;
            var factor =  __instance.noiseSizeOcean / regionSize;
            __instance.requireLandAt.Add(new XZ((int) (positionX * factor), (int) (positionZ * factor)));
            
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GenMaps), nameof(GenMaps.ForceLandformAt))]
        public static bool Prefix(GenMaps __instance, ForceLandform landform) {
            if (__instance.landformsGen is MapLayerLandformsSmooth) {
                ((MapLayerLandformsSmooth)__instance.landformsGen).AddForcedLandform(landform);
            }

            return true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GenMaps), nameof(GenMaps.ForceLandformAt))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator) {
            var codes = new List<CodeInstruction>(instructions);

            var updateNoiseLandform = new List<CodeInstruction> { //Attempt to copy over and assign the Landforms file from HeightNoise into the vanilla NoiseLandforms so vanilla calls can still pull from it properly.
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field("LandformHeightNoise:landforms")),
                new CodeInstruction(OpCodes.Stsfld, AccessTools.Field("NoiseLandforms:landforms"))
            }; //Will this actually set it? I HAVE NO IDEA! It feels TOO simple to work like this... It worked? Hah! Yay.

            codes.InsertRange(0, updateNoiseLandform);

            return codes.AsEnumerable();
        }
    }
}
