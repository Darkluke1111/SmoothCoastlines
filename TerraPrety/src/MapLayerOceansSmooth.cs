using System.Collections.Generic;
using TerraPrety;
using TerraPrety.Noise;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace MapLayer
{
    class MapLayerOceansSmooth : MapLayerBase
    {
        NormalizedSimplexNoise noisegenX;
        NormalizedSimplexNoise noisegenY;
        float wobbleIntensity;
        private WorldGenConfig config;
        VoronoiNoise voronoiNoise;
        Noise2D oceanNoise;
        Noise2D oceanAndCoastNoise;

        public static MapLayerOceansSmooth Instance;

        public float landFormHorizontalScale = 1f;

        public MapLayerOceansSmooth(long seed, WorldGenConfig config, List<XZ> requireLandAt) : base(seed)
        {
            this.config = config;

            voronoiNoise = new VoronoiNoise(seed + 2, config.noiseScale, requireLandAt);
            oceanNoise = new NoiseRemapper(voronoiNoise, config.remappingKeys, config.remappingValues);
            oceanAndCoastNoise = new NoiseRemapper(voronoiNoise, config.coastRemappingKeys, config.coastRemappingValues);
            Instance = this;

            int woctaves = 4;
            float wscale = config.oceanWobbleScale * config.noiseScale;
            float wpersistence = 0.9f;
            wobbleIntensity = config.oceanWobbleIntensity * config.noiseScale;
            noisegenX = NormalizedSimplexNoise.FromDefaultOctaves(woctaves, 1 / wscale, wpersistence, seed + 2);
            noisegenY = NormalizedSimplexNoise.FromDefaultOctaves(woctaves, 1 / wscale, wpersistence, seed + 1231296);
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
                    this.Wobble(nx, nz, out int unscaledXpos, out int unscaledZpos);
                    var oceanicity = oceanNoise.getValueAt(unscaledXpos, unscaledZpos);

                    result[z * sizeX + x] = (int)(oceanicity * 255);
                }
            }

            return result;
        }

        private void Wobble(int nx, int nz, out int offsetX, out int offsetZ)
        {
            double undistortedNoise = voronoiNoise.getValueAt(nx, nz);
            offsetX = nx + (int)(wobbleIntensity * noisegenX.Noise(nx, nz) * undistortedNoise);
            offsetZ = nz + (int)(wobbleIntensity * noisegenY.Noise(nx, nz) * undistortedNoise);
        }

        public double OceanOpacity(int ox, int oz)
        {
            this.Wobble(ox, oz, out int wx, out int wz);
            return oceanNoise.getValueAt(wx, wz);
        }

        public double CoastOpacity(int ox, int oz)
        {
            this.Wobble(ox, oz, out int wx, out int wz);
            return oceanAndCoastNoise.getValueAt(wx, wz);
        }

        public double ContinentalPosition(int ox, int oz)
        {
            this.Wobble(ox, oz, out int wx, out int wz);
            return voronoiNoise.getValueAt(wx, wz);
        }

        //Send this the coords of a single tile of the OceanMap to recieve the Oceanicity for that tile. Intended for use with far-reaching generation steps where this part of the map has not been generated yet, but the value is still needed. Use sparingly as this fully calculates it, if possible always just try accessing the saved region maps directly. (Used in River Gen currently)
        public int GetOceanicityAt(int xCoord, int zCoord) {
            var undestortedNoise = voronoiNoise.getValueAt(xCoord, zCoord); ;
            var offsetX = (int)(wobbleIntensity * noisegenX.Noise(xCoord, zCoord) * undestortedNoise);
            var offsetZ = (int)(wobbleIntensity * noisegenY.Noise(xCoord, zCoord) * undestortedNoise);
            var unscaledXpos = xCoord + offsetX;
            var unscaledZpos = zCoord + offsetZ;

            return (int)(oceanNoise.getValueAt(unscaledXpos, unscaledZpos) * 255);
        }

        public XZ GetContinentalCenter(int xCoord, int zCoord) {
            var undestortedNoise = voronoiNoise.getValueAt(xCoord, zCoord); ;
            var offsetX = (int)(wobbleIntensity * noisegenX.Noise(xCoord, zCoord) * undestortedNoise);
            var offsetZ = (int)(wobbleIntensity * noisegenY.Noise(xCoord, zCoord) * undestortedNoise);
            var unscaledXpos = xCoord + offsetX;
            var unscaledZpos = zCoord + offsetZ;

            return voronoiNoise.GetContinentalCenter(unscaledXpos, unscaledZpos);
        }
    }
}
