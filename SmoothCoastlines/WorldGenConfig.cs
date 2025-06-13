using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace SmoothCoastlines
{
    public class WorldGenConfig
    {
        public float oceanWobbleIntensity = 0.75f;
        public float oceanWobbleScale = 1.5f;
        public float noiseScale = 256.0f;
        public double[] remappingKeys = { 0.125, 0.45 };
        public double[] remappingValues = {0.0, 1.0};

        public float heightCompOceanicityMult = 100.0f;
        public int heightCompOceanicityFlatFactor = 0;
        public float lowRangeHeightForOceanicityComp = 0.2f;
        public float highRangeHeightForOceanicityComp = 0.5f;
        public int highRangeFlatFactor = 5;

        public int heightMapOctaves = 1;
        public float heightMapNoiseScale = 85.0f;
        public float heightMapPersistance = 0.1f;
        public float radiusMultOutwardsForSmoothing = 3.0f;
        public string fallbackParentLandformCode = "ultraflats"; //This is just in case it somehow rolls a height value with no valid Landforms that would fit it, it will use this one instead.
    }
}