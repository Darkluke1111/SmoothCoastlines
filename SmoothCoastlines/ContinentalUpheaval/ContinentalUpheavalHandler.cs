using HarmonyLib;
using SmoothCoastlines.LandformHeights;
using SmoothCoastlines.Rivers;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;
using Vintagestory.ServerMods.NoObf;

namespace SmoothCoastlines.ContinentalUpheaval {

    public class ContinentalUpheavalHandler {

        static Dictionary<int, LerpedWeightedIndex2DMap> landformMapRef;

        public static MapLayerBase GetTerraPretyUpheavalMap(long seed, int scale, List<XZ> requireLandAt) {
            return new MapLayerContinentalUpheaval(seed, SmoothCoastlinesModSystem.config, requireLandAt);
        }

        public static void PostGenMapsInitWorldGen(ICoreServerAPI sapi) { //This can actually serve as an injection point for allowing the init of other Maps after GenMaps is finished with it's Init.
            //var genRivers = sapi.ModLoader.GetModSystem<GenRivers>();
            //genRivers.InitWorldGenPostGenMaps(sapi);

            var terraPrety = sapi.ModLoader.GetModSystem<SmoothCoastlinesModSystem>();
            terraPrety.NoiseSizeCoast = sapi.WorldManager.RegionSize / TerraGenConfig.landformMapScale;
            terraPrety.NoiseSizeRivers = sapi.WorldManager.RegionSize / TerraGenConfig.landformMapScale;
            terraPrety.regionMapSize = (int)Math.Ceiling((double)sapi.WorldManager.MapSizeX / sapi.WorldManager.RegionSize);
            var genMaps = sapi.ModLoader.GetModSystem<GenMaps>();
            var genTerra = sapi.ModLoader.GetModSystem<GenTerra>();
            landformMapRef = Traverse.Create(genTerra).Field("LandformMapByRegion").GetValue<Dictionary<int, LerpedWeightedIndex2DMap>>();

            terraPrety.CoastMap = new CoastMap(sapi.WorldManager.Seed + 1873, terraPrety.NoiseSizeCoast, sapi); //Seed probably doesn't matter for this.
            //genRivers.RiverMap = new RiverMap(sapi.WorldManager.Seed + 1873, genRivers.NoiseSizeRivers, sapi, genMaps.requireLandAt);

            //var genTerra = sapi.ModLoader.GetModSystem<GenTerra>();
            terraPrety.landforms = LandformHeightNoise.landforms;
            float[][] terrainYThresholds = new float[terraPrety.landforms.LandFormsByIndex.Length][];
            for (int i = 0; i < terraPrety.landforms.LandFormsByIndex.Length; i++) terrainYThresholds[i] = terraPrety.landforms.LandFormsByIndex[i].TerrainYThresholds;

            float noiseScale = Math.Max(1, (sapi.WorldManager.MapSizeY - 64) / 256f);
            var riversTerrainNoise = NewNormalizedSimplexFractalNoise.FromDefaultOctaves(
                9, 0.0005 * NewSimplexNoiseLayer.OldToNewFrequency / noiseScale, 0.9, sapi.WorldManager.Seed
            );
            ((CoastMap)terraPrety.CoastMap).SetThresholdAndNoise(terrainYThresholds, riversTerrainNoise);
        }

        public static void PostGenMapsOnMapRegionGen(IMapRegion mapRegion, int regionX, int regionZ) {
            var sapi = SmoothCoastlinesModSystem.Sapi;
            var genMaps = sapi.ModLoader.GetModSystem<GenMaps>();
            var terraPrety = sapi.ModLoader.GetModSystem<SmoothCoastlinesModSystem>();
            //var genRivers = sapi.ModLoader.GetModSystem<GenRivers>();

            ((MapLayerLandformsSmooth)genMaps.landformsGen)?.AddHeightmapToRegion(mapRegion);
            //genRivers.OnMapRegionGenPostGenMaps(mapRegion, regionX, regionZ);

            var coastPad = 1;
            //var riverPad = 1;
            var oceanMap = mapRegion.OceanMap;
            var oceanPad = oceanMap.BottomRightPadding;
            var landformMap = mapRegion.LandformMap;
            //var genTerraPrety = Sapi.ModLoader.GetModSystem<GenTerraPrety>(); //This is currently a problem for Rivers and how they were being done for now. With the removal of GenTerraPrety, will have to get the LandLerpMap some other way. Or similar way?
            var landLerpMap = GetOrLoadLerpedLandformMapFromRegion(mapRegion, regionX, regionZ, terraPrety);

            var heightMap = mapRegion.ModMaps["LandformHeightMap"];

            var CoastalRegion = new IntDataMap2D { //It seems any map with a pad of 1 just doesn't set the TopLeft padding in GenMaps. Sticking with that?
                //Data = coastData,
                Size = terraPrety.NoiseSizeCoast + 1,
                //TopLeftPadding = coastPad,
                BottomRightPadding = coastPad
            };
            ((CoastMap)terraPrety.CoastMap).SetCoastAndLandformMaps(oceanMap, landLerpMap, landformMap.InnerSize, CoastalRegion.InnerSize);
            var coastData = ((CoastMap)terraPrety.CoastMap).GenLayerAndAddToCache(regionX * terraPrety.NoiseSizeCoast, regionZ * terraPrety.NoiseSizeCoast, terraPrety.NoiseSizeCoast + coastPad, terraPrety.NoiseSizeCoast + coastPad);
            CoastalRegion.Data = coastData;

            mapRegion.ModMaps["TerraPretyCoastMap"] = CoastalRegion;
        }

        public static LerpedWeightedIndex2DMap GetOrLoadLerpedLandformMapFromRegion(IMapRegion mapregion, int regionX, int regionZ, SmoothCoastlinesModSystem terraPrety) {
            // 1. Load?
            landformMapRef.TryGetValue(regionZ * terraPrety.regionMapSize + regionX, out LerpedWeightedIndex2DMap map);
            if (map != null) return map;

            IntDataMap2D lmap = mapregion.LandformMap;
            // 2. Create
            map = landformMapRef[regionZ * terraPrety.regionMapSize + regionX]
                = new LerpedWeightedIndex2DMap(lmap.Data, lmap.Size, TerraGenConfig.landFormSmoothingRadius, lmap.TopLeftPadding, lmap.BottomRightPadding);

            return map;
        }

        public static float GetContinentalAndMountainUpheavalForArea(IMapChunk mapChunk, int rlX, int rlZ, int lX, int lZ) { //Did I use this anywhere...? I don't think so since it had the old ModMaps and that was never actually set or used.
            float retVal = 0;
            var contUpheavalMap = mapChunk.MapRegion.ModMaps["ContinentalUpheavalMap"];
            int regionChunkSize = SmoothCoastlinesModSystem.Sapi.WorldManager.RegionSize / GlobalConstants.ChunkSize;
            const float chunkBlockDelta = 1.0f / GlobalConstants.ChunkSize;

            if (contUpheavalMap != null && contUpheavalMap.Data.Length > 0) {
                float ofac = (float)contUpheavalMap.InnerSize / regionChunkSize;
                int contUpheavalUpLeft = contUpheavalMap.GetUnpaddedInt((int)(rlX * ofac), (int)(rlZ * ofac));
                int contUpheavalUpRight = contUpheavalMap.GetUnpaddedInt((int)(rlX * ofac + ofac), (int)(rlZ * ofac));
                int contUpheavalBotLeft = contUpheavalMap.GetUnpaddedInt((int)(rlX * ofac), (int)(rlZ * ofac + ofac));
                int contUpheavalBotRight = contUpheavalMap.GetUnpaddedInt((int)(rlX * ofac + ofac), (int)(rlZ * ofac + ofac));

                float continentalLift = GameMath.BiLerp(contUpheavalUpLeft, contUpheavalUpRight, contUpheavalBotLeft, contUpheavalBotRight, lX * chunkBlockDelta, lZ * chunkBlockDelta);
                retVal += continentalLift;
            }

            return retVal;
        }
    }
}
