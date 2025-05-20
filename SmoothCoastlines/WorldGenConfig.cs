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
        public float oceanWobbleIntensity = 0.5f;
        public float oceanWobbleScale = 2.0f;
        public float noiseScale = 20.0f;
        public double[] remappingKeys = { 0.0, 1.0 };
        public double[] remappingValues = {0.0, 1.0};
    }
}
