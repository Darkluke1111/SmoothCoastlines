
using SmoothCoastlines;
using SmoothCoastLines.Noise;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace MapLayer
{
    class AltMapLayerOceans : MapLayerBase
    {
        NormalizedSimplexNoise noisegenX;
        NormalizedSimplexNoise noisegenY;
        float wobbleIntensity;
        private WorldGenConfig config;
        Noise2D oceanNoise;

        public float landFormHorizontalScale = 1f;
        /// <summary>
        /// This is related to ServerSystemSupplyChunks.LoadWorldgenHandlerAndSpawnChunks
        /// </summary>
        private readonly bool requiresSpawnOffset;

        public AltMapLayerOceans(long seed, WorldGenConfig config) : base(seed)
        {
            this.config = config;

            oceanNoise = new NoiseRemapper(new VoronoiNoise(seed + 2, config.noiseScale), config.remappingKeys, config.remappingValues);

            int woctaves = 4;
            float wscale = config.oceanWobbleScale * config.noiseScale;
            float wpersistence = 0.9f;
            wobbleIntensity = config.oceanWobbleIntensity * config.noiseScale;
            noisegenX = NormalizedSimplexNoise.FromDefaultOctaves(woctaves, 1 / wscale, wpersistence, seed + 2);
            noisegenY = NormalizedSimplexNoise.FromDefaultOctaves(woctaves, 1 / wscale, wpersistence, seed + 1231296);

        }

        public XZ GetNoiseOffsetAt(int xCoord, int zCoord)
        {
            int offsetX = (int)(wobbleIntensity * noisegenX.Noise(xCoord, zCoord) * 1.2f);
            int offsetY = (int)(wobbleIntensity * noisegenY.Noise(xCoord, zCoord) * 1.2f);
            return new XZ(offsetX, offsetY);
        }

        public override int[] GenLayer(int xCoord, int zCoord, int sizeX, int sizeZ)
        {
            var result = new int[sizeX * sizeZ];
            for (var x = 0; x < sizeX; x++)
            {
                for (var z = 0; z < sizeZ; z++)
                {
                    var nx = xCoord + x;
                    var nz = zCoord + z;
                    var offsetX = (int)(wobbleIntensity * noisegenX.Noise(nx, nz));
                    var offsetZ = (int)(wobbleIntensity * noisegenY.Noise(nx, nz));
                    var unscaledXpos = nx + offsetX;
                    var unscaledZpos = nz + offsetZ;
                    var oceanicity = oceanNoise.getValueAt(unscaledXpos, unscaledZpos);

                    result[z * sizeX + x] = (int) (oceanicity * 255);
                }
            }

            return result;
        }
    }
}
