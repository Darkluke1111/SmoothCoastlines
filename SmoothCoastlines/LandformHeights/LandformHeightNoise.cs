using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;
using Vintagestory.ServerMods.NoObf;

namespace SmoothCoastlines.LandformHeights {

    public struct RequiredHeightPoints {
        public int x;
        public int z;
        public double minHeight;
        public double maxHeight;
        public double centerHeight;

        public RequiredHeightPoints(int x, int z, double minHeight, double maxHeight) {
            this.x = x;
            this.z = z;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.centerHeight = minHeight + ((maxHeight - minHeight) / 2);
        }

        public bool IsWithinRange(int X, int Z, int range) {
            return X > (x - range) && X < (x + range) && Z > (z - range) && Z < (z + range);
        }
    }

    public class LandformHeightNoise : NoiseBase {

        public static LandformsWorldProperty landforms;
        public static LandformsHeightsWorldProperty landformsHeights;

        protected WeightedNormalizedSimplexNoise heightNoise;
        protected List<ForceLandform> forcedLandforms;
        protected WorldGenConfig config;
        protected int fallbackParentLandformID;
        public float scale;

        public LandformHeightNoise(long seed, ICoreServerAPI api, float scale, List<ForceLandform> forcedLandforms, WorldGenConfig config) : base(seed) {
            this.scale = scale;
            this.config = config;
            this.forcedLandforms = forcedLandforms;

            int hOctaves = 4;
            float hScale = this.config.heightMapWobbleScale * this.config.heightMapNoiseScale;
            float hPersistance = 0.9f;
            heightNoise = new WeightedNormalizedSimplexNoise(hOctaves, hScale, hPersistance, seed, this.config.heightPointsOutForAverage);

            LoadLandforms(api);
            SetForcedHeightPoints();
            FindForcedLandformID();
        }

        public static void LoadLandforms(ICoreServerAPI api) {
            IAsset asset = api.Assets.Get("worldgen/landforms.json");
            landforms = asset.ToObject<LandformsWorldProperty>();

            IAsset heightsProperty = api.Assets.Get("terraprety:worldgen/landformheights.json");
            landformsHeights = heightsProperty.ToObject<LandformsHeightsWorldProperty>();

            int quantityMutations = 0;
            landformsHeights.LandformHeightsByIndex = new LandformGenHeight[landforms.Variants.Length];

            for (int i = 0; i < landforms.Variants.Length; i++) {
                LandformVariant variant = landforms.Variants[i];
                variant.index = i;
                variant.Init(api.WorldManager, i);

                LandformGenHeight varHeight = landformsHeights.Variants.FirstOrDefault(h => h.Code == variant.Code);
                varHeight.Init(i);
                landformsHeights.LandformHeightsByIndex[i] = varHeight; //Adding these in here since Mutations likely don't need separate heights from the parents?

                if (variant.Mutations != null) {
                    quantityMutations += variant.Mutations.Length;
                }
            }

            landforms.LandFormsByIndex = new LandformVariant[quantityMutations + landforms.Variants.Length];

            // Mutations get indices after the parent ones
            for (int i = 0; i < landforms.Variants.Length; i++) {
                landforms.LandFormsByIndex[i] = landforms.Variants[i];
            }

            int nextIndex = landforms.Variants.Length;
            for (int i = 0; i < landforms.Variants.Length; i++) {
                LandformVariant variant = landforms.Variants[i];
                if (variant.Mutations != null) {
                    for (int j = 0; j < variant.Mutations.Length; j++) {
                        LandformVariant variantMut = variant.Mutations[j];

                        if (variantMut.TerrainOctaves == null) {
                            variantMut.TerrainOctaves = variant.TerrainOctaves;
                        }
                        if (variantMut.TerrainOctaveThresholds == null) {
                            variantMut.TerrainOctaveThresholds = variant.TerrainOctaveThresholds;
                        }
                        if (variantMut.TerrainYKeyPositions == null) {
                            variantMut.TerrainYKeyPositions = variant.TerrainYKeyPositions;
                        }
                        if (variantMut.TerrainYKeyThresholds == null) {
                            variantMut.TerrainYKeyThresholds = variant.TerrainYKeyThresholds;
                        }


                        landforms.LandFormsByIndex[nextIndex] = variantMut;
                        variantMut.Init(api.WorldManager, nextIndex);
                        nextIndex++;
                    }
                }
            }
        }

        public void SetForcedHeightPoints() {
            List<RequiredHeightPoints> reqHeights = new List<RequiredHeightPoints>();
            foreach (var forcedLand in forcedLandforms) {
                int heightReqIndex = -1;
                var list = landforms.LandFormsByIndex;
                for (int i = 0; i < list.Length; i++) {
                    if (list[i].Code.Path == forcedLand.LandformCode) {
                        heightReqIndex = i;
                        break;
                    }
                }

                LandformGenHeight heights;
                if (heightReqIndex != -1) {
                    heights = landformsHeights.LandformHeightsByIndex[heightReqIndex];
                } else {
                    heights = new LandformGenHeight();
                }
                reqHeights.Add(new RequiredHeightPoints(forcedLand.CenterPos.X, forcedLand.CenterPos.Z, heights.minHeight, heights.maxHeight));
            }

            heightNoise.SetRequiredPoints(reqHeights);
        }

        public void FindForcedLandformID() {
            for (int i = 0; i < landforms.LandFormsByIndex.Length; i++) {
                if (landforms.LandFormsByIndex[i].Code == config.fallbackParentLandformCode) {
                    fallbackParentLandformID = i;
                    return;
                }
            }
        }

        public int GetLandformIndexAt(int unscaledXpos, int unscaledZpos, int temp, int rain) {
            float xpos = unscaledXpos / scale;
            float zpos = unscaledZpos / scale;

            int xposInt = (int)xpos;
            int zposInt = (int)zpos;

            int parentIndex = GetParentLandformIndexAt(xposInt, zposInt, temp, rain);

            LandformVariant[] mutations = landforms.Variants[parentIndex].Mutations;
            if (mutations != null && mutations.Length > 0) {
                InitPositionSeed(unscaledXpos / 2, unscaledZpos / 2);
                float chance = NextInt(101) / 100f;

                for (int i = 0; i < mutations.Length; i++) {
                    LandformVariant variantMut = mutations[i];

                    if (variantMut.UseClimateMap) {
                        int distRain = rain - GameMath.Clamp(rain, variantMut.MinRain, variantMut.MaxRain);
                        double distTemp = temp - GameMath.Clamp(temp, variantMut.MinTemp, variantMut.MaxTemp);
                        if (distRain != 0 || distTemp != 0) continue;
                    }

                    chance -= mutations[i].Chance;
                    if (chance <= 0) {
                        return mutations[i].index;
                    }
                }
            }
            return parentIndex;
        }


        public int GetParentLandformIndexAt(int xpos, int zpos, int temp, int rain) {
            InitPositionSeed(xpos, zpos);

            double weightSum = 0;
            double heightAtPoint = heightNoise.Height(xpos, zpos);
            int i;
            for (i = 0; i < landforms.Variants.Length; i++) {
                if (heightAtPoint < landformsHeights.LandformHeightsByIndex[i].minHeight || heightAtPoint > landformsHeights.LandformHeightsByIndex[i].maxHeight) {
                    continue; //If this landform doesn't match the height at this position, just skip it and go to the next.
                }

                double weight = landforms.Variants[i].Weight;

                if (landforms.Variants[i].UseClimateMap) {
                    int distRain = rain - GameMath.Clamp(rain, landforms.Variants[i].MinRain, landforms.Variants[i].MaxRain);
                    double distTemp = temp - GameMath.Clamp(temp, landforms.Variants[i].MinTemp, landforms.Variants[i].MaxTemp);
                    if (distRain != 0 || distTemp != 0) weight = 0;
                }

                landforms.Variants[i].WeightTmp = weight;
                weightSum += weight;
            }

            if (weightSum <= 0) {
                return landforms.Variants[fallbackParentLandformID].index;
            }

            double rand = weightSum * NextInt(10000) / 10000.0;

            for (i = 0; i < landforms.Variants.Length; i++) {
                rand -= landforms.Variants[i].WeightTmp;
                if (rand <= 0) return landforms.Variants[i].index;
            }

            return landforms.Variants[i].index;
        }
    }
}
