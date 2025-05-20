using SmoothCoastLines.Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace SmoothCoastlines
{
    class NoiseRemapper : Noise2D
    {
        Noise2D baseNoise;
        private readonly double[] keys;
        private readonly double[] thresholds;

        public NoiseRemapper(Noise2D baseNoise, double[] keys, double[] thresholds)
        {
            this.baseNoise = baseNoise;
            this.keys = keys;
            this.thresholds = thresholds;
        }

        public double getValueAt(int unscaledXpos, int unscaledZpos) {
            var baseValue = baseNoise.getValueAt(unscaledXpos, unscaledZpos);

            if(keys == null || thresholds == null || keys.Length != thresholds.Length)
            {
                return baseValue;
            }

            var keyAmount = keys.Length;

            var currentKey = 0.0;
            var currentThreshold = 0.0;

            for ( int i = 0; i < keyAmount + 1; i++)
            {
                var nextKey = i < keyAmount ? keys[i] : 1.0;
                var nextThreshold = i < keyAmount ? thresholds[i] : 1.0;

                if (nextKey > baseValue)
                {
                    var t = (baseValue - currentKey) / (nextKey - currentKey);
                    return GameMath.Lerp(currentThreshold, nextThreshold, t);
                }

                currentKey = nextKey;
                currentThreshold = nextThreshold;
            }
            return thresholds[keyAmount];
        }
    }
}
