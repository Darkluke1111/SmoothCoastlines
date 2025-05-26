using System;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace SmoothCoastLines.Noise
{
    interface Noise2D
    {
        double getValueAt(int unscaledXpos, int unscaledZpos);
    }
    class VoronoiNoise: NoiseBase, Noise2D
    {


        double scale;
        const double maxDistanceConstant = 1.41421356237309505; //Square Root of 2

        public VoronoiNoise(long seed, double scale) : base(seed)
        {
            this.scale = scale;
        }

        public double getValueAt(int unscaledXpos, int unscaledZpos)
        {
            double xpos_full = unscaledXpos / scale;
            double zpos_full = unscaledZpos / scale;

            int xCell = (int)xpos_full;
            int zCell = (int)zpos_full;

            double xFrac = xpos_full - xCell;
            double zFrac = zpos_full - zCell;

            double[] random_X = new double[3 * 3];
            double[] random_Z = new double[3 * 3];

            double min_distance = Double.MaxValue;

            for (int dx = 0; dx < 3; dx++)
            {
                for(int dz = 0; dz < 3; dz++)
                {
                    InitPositionSeed(xCell - 1 + dx, zCell - 1 + dz);
                    double pointPosX = (NextInt(10000) / 10000.0) - 1 + dx;
                    double pointPosZ = (NextInt(10000) / 10000.0) - 1 + dz;



                    double distance = GameMath.Sqrt((xFrac - pointPosX) * (xFrac - pointPosX) + (zFrac - pointPosZ) * (zFrac - pointPosZ));

                    min_distance = Double.Min(min_distance, distance);
                }
            }

            return min_distance / maxDistanceConstant;
        }

        public Vec2i GetVoronoiCellPoint(Vec2i unscaledPosition)
        {
            //TODO Calculate the "voronoi point" of the cell that the unscaled Position is in and return it
            throw new NotImplementedException();
        }

    }
}
