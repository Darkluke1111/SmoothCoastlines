using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods.NoObf;
using Vintagestory.ServerMods;
using System.Linq;
using System;

namespace SmoothCoastlines
{



    namespace Vintagestory.ServerMods
    {
        class NoiseAltLandforms : NoiseBase
        {

            public float scale;

            public NoiseAltLandforms(long seed, ICoreServerAPI api, float scale) : base(seed)
            {
                this.scale = scale;
            }



            public int GetLandformIndexAt(int unscaledXpos, int unscaledZpos, LandformsWorldProperty landforms, LandformAltitudeInfoWorldProperty altitudeInfo,  int temp, int rain, int altitude)
            {
                float xpos = (float)unscaledXpos / scale;
                float zpos = (float)unscaledZpos / scale;

                int xposInt = (int)xpos;
                int zposInt = (int)zpos;

                int parentIndex = GetParentLandformIndexAt(xposInt, zposInt, landforms, altitudeInfo, temp, rain, altitude);

                LandformVariant[] mutations = landforms.Variants[parentIndex].Mutations;
                if (mutations != null && mutations.Length > 0)
                {
                    InitPositionSeed(unscaledXpos / 2, unscaledZpos / 2);
                    float chance = NextInt(101) / 100f;

                    for (int i = 0; i < mutations.Length; i++)
                    {
                        LandformVariant variantMut = mutations[i];

                        if (variantMut.UseClimateMap)
                        {
                            int distRain = rain - GameMath.Clamp(rain, variantMut.MinRain, variantMut.MaxRain);
                            double distTemp = temp - GameMath.Clamp(temp, variantMut.MinTemp, variantMut.MaxTemp);
                            if (distRain != 0 || distTemp != 0) continue;
                        }


                        chance -= mutations[i].Chance;
                        if (chance <= 0)
                        {
                            return mutations[i].index;
                        }
                    }
                }

                return parentIndex;
            }


            public int GetParentLandformIndexAt(int xpos, int zpos, LandformsWorldProperty landforms, LandformAltitudeInfoWorldProperty altitudeInfoList, int temp, int rain, int altitude)
            {
                InitPositionSeed(xpos, zpos);

                double weightSum = 0;
                int i;
                for (i = 0; i < landforms.Variants.Length; i++)
                {
                    double weight = landforms.Variants[i].Weight;

                    var altitudeInfo = altitudeInfoList.GetInfo(landforms.Variants[i]);

                    if (altitudeInfo == null || altitude < altitudeInfo.MinAltitude * 255 || altitude > altitudeInfo.MaxAltitude * 255)
                    {
                        weight = 0;
                    }

                    if (landforms.Variants[i].UseClimateMap)
                    {
                        int distRain = rain - GameMath.Clamp(rain, landforms.Variants[i].MinRain, landforms.Variants[i].MaxRain);
                        double distTemp = temp - GameMath.Clamp(temp, landforms.Variants[i].MinTemp, landforms.Variants[i].MaxTemp);
                        if (distRain != 0 || distTemp != 0) weight = 0;
                    }

                    landforms.Variants[i].WeightTmp = weight;
                    weightSum += weight;
                }

                double rand = weightSum * NextInt(10000) / 10000.0;

                for (i = 0; i < landforms.Variants.Length; i++)
                {
                    rand -= landforms.Variants[i].WeightTmp;
                    if (rand <= 0) return landforms.Variants[i].index;
                }

                return landforms.Variants[i].index;
            }
        }
    }

}
