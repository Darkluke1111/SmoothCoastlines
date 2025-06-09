using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace SmoothCoastlines
{
    class MapLayerAltitude : MapLayerBase
    {
        NormalizedSimplexNoise noiseAltitude;
        public MapLayerAltitude(long seed) : base(seed)
        {
            noiseAltitude = NormalizedSimplexNoise.FromDefaultOctaves(1, 1 / SmoothCoastlinesModSystem.config.altitudeScale, 0.5, seed);
        }

        public override int[] GenLayer(int xCoord, int zCoord, int sizeX, int sizeZ)
        {
            int[] result = new int[sizeX * sizeZ];

            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    double finalX = (xCoord + x);
                    double finalZ = (zCoord + z);

                    double altitude = noiseAltitude.Noise(
                        finalX,
                        finalZ
                    );

                    result[z * sizeX + x] = (int)  (altitude * 255);
                }
            }
            return result;
        }
    }
}
