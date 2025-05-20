using HarmonyLib;
using MapLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace SmoothCoastlines
{
    [HarmonyPatch]
    public static class Patch
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GenMaps), nameof(GenMaps.GetOceanMapGen))]
        public static bool Prefix(ref MapLayerBase __result, GenMaps __instance, long seed, float landcover, int oceanMapScale, float oceanScaleMul, List<XZ> requireLandAt, bool requiresSpawnOffset)
        {
            __result = new AltMapLayerOceans(seed, SmoothCoastlinesModSystem.config);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GenStoryStructures), "TryAddStoryLocation")]
        public static void Postfix(GenStoryStructures __instance, WorldGenStoryStructure storyStructure)
        {

            //debug prints
            Console.WriteLine("Modifying spawn location of: " + storyStructure.Code + ": ");
            Console.WriteLine("CenterPos: " + __instance.storyStructureInstances[storyStructure.Code].CenterPos);
            Console.WriteLine("Location: " + __instance.storyStructureInstances[storyStructure.Code].Location);
            Console.WriteLine("LandformRadius: " + __instance.storyStructureInstances[storyStructure.Code].LandformRadius);
            Console.WriteLine("GenerationRadius: " + __instance.storyStructureInstances[storyStructure.Code].GenerationRadius);
            Console.WriteLine("DirX: " + __instance.storyStructureInstances[storyStructure.Code].DirX);
            Console.WriteLine("SkipGenerationFlags: " + __instance.storyStructureInstances[storyStructure.Code].SkipGenerationFlags);

            //TODO Get the voronoi cell the center pos is in, then calculate the voronoi point for this cell (Make sure to use the same seed that is later used in ocean map gen) and set the new center pos to that location (adjust other stuff like location accordingly). This should ensure the structure spawn at the center of a continent. For additional safety maybe check whether there is another story location at that point already, although this should only happen with extremely huge continents.
        }

    }
}
