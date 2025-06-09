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

        public int altitudeMapScale = 32;
        public double altitudeScale = 100.0;
    }
}
