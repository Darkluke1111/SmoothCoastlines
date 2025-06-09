using Cairo;
using HarmonyLib;
using MapLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        //This patch switches the vanilla MapLandformGen class for the custom MapOceanGenSmooth class
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GenMaps), nameof(GenMaps.GetLandformMapGen))]
        public static bool Prefix(ref MapLayerBase __result, long seed, NoiseClimate climateNoise, ICoreServerAPI api, float landformScale)
        {
            new MapLayerLandforms(seed,climateNoise, api, landformScale); // Unused but it initializes static fields in NoiseLandforms that are used by a lot of vanilla code
            __result = new MapLayerAltLandforms(seed, api, climateNoise, landformScale);
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

        
    }
}
