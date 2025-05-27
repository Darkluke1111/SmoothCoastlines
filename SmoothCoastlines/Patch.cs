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

            // We need the server api to access the world seed. Maybe there is a better way than accessing the private field via reflection?
            var sapi = (ICoreServerAPI) __instance.GetType().GetField("api", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            // Create new AltMapLayerOceans to find continent positions (We could also use the same object that is referenced in GenMaps?)
            var oceanLayer = new AltMapLayerOceans(sapi.WorldManager.Seed, SmoothCoastlinesModSystem.config);

            //Move structure center to a close continent center
            var structureCenter = __instance.storyStructureInstances[storyStructure.Code].CenterPos;
            var oldCenter = structureCenter.Copy();
            var newStructureCoordinates = oceanLayer.GetCloseContinentCenter(new Vec2i(structureCenter.X, structureCenter.Z));
            structureCenter.X = (int) newStructureCoordinates.X;
            structureCenter.Z = (int) newStructureCoordinates.Y;

            //Adjust Location attribute of structure
            __instance.storyStructureInstances[storyStructure.Code].Location.OffsetCopy(structureCenter.AsVec3i - oldCenter.AsVec3i);

            
        }

    }
}
