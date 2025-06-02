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

        private (double, Vec2d) calcNoiseAt(int unscaledXpos, int unscaledZpos)
        {
            double xpos_full = unscaledXpos / scale;
            double zpos_full = unscaledZpos / scale;

            int xCell = (int)xpos_full;
            int zCell = (int)zpos_full;

            double xFrac = xpos_full - xCell;
            double zFrac = zpos_full - zCell;

            double min_distance = Double.MaxValue;
            Vec2d closestPoint = new Vec2d();

            for (int dx = 0; dx < 3; dx++)
            {
                for (int dz = 0; dz < 3; dz++)
                {
                    InitPositionSeed(xCell - 1 + dx, zCell - 1 + dz);
                    double pointPosX = (NextInt(10000) / 10000.0) - 1 + dx;
                    double pointPosZ = (NextInt(10000) / 10000.0) - 1 + dz;



                    double distance = GameMath.Sqrt((xFrac - pointPosX) * (xFrac - pointPosX) + (zFrac - pointPosZ) * (zFrac - pointPosZ));

                    if(min_distance > distance)
                    {
                        min_distance = distance;
                        closestPoint = new Vec2d((xCell -1 + dx)* scale + pointPosX * scale, (zCell -1 + dz) * scale + pointPosZ * scale);
                    }

                }
            }

            return ((min_distance / maxDistanceConstant), closestPoint);
        }

        public double getValueAt(int unscaledXpos, int unscaledZpos)
        {
            return calcNoiseAt(unscaledXpos, unscaledZpos).Item1;
        }

        public Vec2d getVoronoiCellPoint(Vec2i unscaledPosition)
        {
            return calcNoiseAt(unscaledPosition.X, unscaledPosition.Y).Item2;
        }

    }
}
