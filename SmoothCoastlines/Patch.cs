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
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common;
using Vintagestory.GameContent;
using Vintagestory.Server;
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
            new MapLayerLandforms(seed + 12, climateNoise, api, landformScale); //Luke pointed out this is a much better place to initialize this, since it SHOULD be the same as in the SmoothLandforms version, this should be fine! Both init them the same way, Smooth just also inits the heights as well.
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
        [HarmonyPatch(typeof(GenMaps), "OnMapRegionGen")]
        public static IEnumerable<CodeInstruction> OnMapRegionGenTranspiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);

            int indexOfInjectPoint = -1;

            for (int i = 0; i < codes.Count; i++) {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand as string == "forceLandform") {
                    indexOfInjectPoint = i - 3;
                    break;
                }
            }

            var addHeightmapToRegionMethod = AccessTools.Method(typeof(Patch), "AddHeightmapToRegionData", new Type[1] { typeof(IMapRegion) });

            var addHeightmapToRegionData = new List<CodeInstruction> {
                CodeInstruction.LoadArgument(1),
                new CodeInstruction(OpCodes.Call, addHeightmapToRegionMethod)
            };

            if (indexOfInjectPoint > -1) {
                codes.InsertRange(indexOfInjectPoint, addHeightmapToRegionData);
            } else {
                SmoothCoastlinesModSystem.Logger.Warning("Could not locate the forceLandform string in OnMapRegionGen. Will not be able to save the Heightmap to Region Data.");
            }

            return codes.AsEnumerable();
        }

        private static void AddHeightmapToRegionData(IMapRegion region) {
            ((MapLayerLandformsSmooth)SmoothCoastlinesModSystem.Sapi.ModLoader.GetModSystem<GenMaps>().landformsGen)?.AddHeightmapToRegion(region);
        }
    }
}
