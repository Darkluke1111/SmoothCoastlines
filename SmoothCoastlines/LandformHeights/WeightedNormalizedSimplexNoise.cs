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

        public WeightedNormalizedSimplexNoise(int quantityOctaves, double baseFrequency, double persistance, long seed, float pointsOutForAverage) {
            SimplexNoise = NormalizedSimplexNoise.FromDefaultOctaves(quantityOctaves, baseFrequency, persistance, seed);
            PointsOutwardsNeedingAverage = pointsOutForAverage;
        }

        public void SetRequiredPoints(List<RequiredHeightPoints> reqPoints) {
            RequiredPoints = reqPoints;
        }

        public double Height(int x, int z) {
            RequiredHeightPoints foundPoint = new RequiredHeightPoints(0,0,100,0,1); //This should never be accessed unless it's actually properly replaced.
            bool wasWithinRange = false;
            if (RequiredPoints != null && RequiredPoints.Count > 0) {
                foreach (var p in RequiredPoints) {
                    var scaledRadius = (int)(PointsOutwardsNeedingAverage * p.radius);
                    if (p.IsWithinRange(x, z, scaledRadius)) {
                        foundPoint = p;
                        wasWithinRange = true;
                        break;
                    }
                }
            }

            var height = SimplexNoise.Noise(x, z); //First grab the height.

            if (wasWithinRange) { //If the point was within the range of the required heights...
                //Handle the smoothing here. foundPoint is set.
                //Find the percentage adjustment from the current to the centerpoint of the found point. Is it above or below?
                var isAbove = false;
                var scaledRadius = (int)(PointsOutwardsNeedingAverage * foundPoint.radius);
                if (height > foundPoint.centerHeight) {
                    isAbove = true;
                }

                float pointsOutSquare = scaledRadius * scaledRadius; //Gaussian Function to find the percentage to adjust the height by!
                double percentAdjust = Math.Exp(-((Math.Pow((x-foundPoint.x), 2) / (2 * pointsOutSquare)) + (Math.Pow((z - foundPoint.z), 2) / (2 * pointsOutSquare))));
                var adjustedHeight = height;
                if (isAbove) {
                    adjustedHeight -= (height - foundPoint.maxHeight) * percentAdjust;
                } else {
                    adjustedHeight += (foundPoint.minHeight - height) * percentAdjust;
                }

                return adjustedHeight;
            }

            return height;
        }
    }
}
