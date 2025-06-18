using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace SmoothCoastlines.LandformHeights {
    public class WeightedNormalizedSimplexNoise { 

        private NormalizedSimplexNoise SimplexNoise;
        private List<RequiredHeightPoints> RequiredPoints; //Any point here has a specific height for the Landform it is expecting, and this holds that min-max height.
        private float PointsOutwardsNeedingAverage; //This many steps outwards from a required point will be adjusted towards the required height.
        private double[] remappingKeys;
        private double[] remappingValues;

        public WeightedNormalizedSimplexNoise(int quantityOctaves, double baseFrequency, double persistance, long seed, float pointsOutForAverage, double[] remapKeys, double[] remapValues) {
            SimplexNoise = NormalizedSimplexNoise.FromDefaultOctaves(quantityOctaves, baseFrequency, persistance, seed);
            PointsOutwardsNeedingAverage = pointsOutForAverage;
            remappingKeys = remapKeys;
            remappingValues = remapValues;
        }

        public void SetRequiredPoints(List<RequiredHeightPoints> reqPoints) {
            RequiredPoints = reqPoints;
        }

        public double Height(int x, int z) {
            RequiredHeightPoints foundPoint = new RequiredHeightPoints(0,0,100,0,1); //This should never be accessed unless it's actually properly replaced.
            bool wasWithinRange = false;
            int scaledRadius;
            if (RequiredPoints != null && RequiredPoints.Count > 0) {
                foreach (var p in RequiredPoints) {
                    if (p.x == x && p.z == z) { //If the polled point actually is the weighted point, just return the center height and we are good to go.
                        return p.centerHeight;
                    }
                    scaledRadius = (int)(PointsOutwardsNeedingAverage * p.radius);
                    scaledRadius += scaledRadius / 2;
                    if (p.IsWithinRange(x, z, scaledRadius)) {
                        foundPoint = p;
                        wasWithinRange = true;
                        break;
                    }
                }
            }

            var height = SimplexNoise.Noise(x, z); //First grab the height.
            height = RemapHeight(height);

            if (wasWithinRange) { //If the point was within the range of the required heights...
                //Handle the smoothing here. foundPoint is set.
                scaledRadius = (int)(PointsOutwardsNeedingAverage * foundPoint.radius);
                var centerHeightWeight = GetAdjustmentFromGaussian(scaledRadius, foundPoint, x, z); //This SHOULD return a double from 0 - 1, which is how strong of a 'pull' should the center point have over the current height
                var adjustedHeight = GameMath.Lerp(height, foundPoint.centerHeight, centerHeightWeight);

                return adjustedHeight;
            }

            return height;
        }

        public double RemapHeight(double baseHeight) {
            if (remappingKeys == null || remappingValues == null || remappingKeys.Length != remappingValues.Length) {
                return baseHeight;
            }

            var keyCount = remappingKeys.Length;
            var currentKey = 0.0;
            var currentValue = 0.0;

            for (int i = 0; i < keyCount + 1; i++) {
                var nextKey = i < keyCount ? remappingKeys[i] : 1.0;
                var nextValue = i < keyCount ? remappingValues[i] : 1.0;

                if (nextKey > baseHeight) {
                    var t = (baseHeight - currentKey) / (nextKey - currentKey);
                    return GameMath.Lerp(currentValue, nextValue, t);
                }

                currentKey = nextKey;
                currentValue = nextValue;
            }

            return remappingValues[keyCount];
        }

        public double GetAdjustmentFromGaussian(int radius, RequiredHeightPoints foundPoint, int x, int z) {
            float radiusSquare = MathF.Pow(radius/2, 2); //Gaussian Function to find the percentage to adjust the height by!
            var dx = foundPoint.x - x;
            var dz = foundPoint.z - z;

            return Math.Exp(-((Math.Pow(dx, 2) / (2 * radiusSquare)) + (Math.Pow(dz, 2) / (2 * radiusSquare))));
        }
    }
}
