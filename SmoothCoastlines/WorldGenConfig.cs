﻿using System;
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
        public double[] remappingValues = {0.0, 1.0 };

        public float[] heightThresholdsForOceanicityComp = { 0.2f, 0.5f, 0.7f, 1.0f };
        public float[] heightMultsAtThresholdsForOceanicityComp = { 0.0f, 40.0f, 70.0f, 90.0f };
        public float[] heightFlatsAtThresholdsForOceanicityComp = { 4.2f, 1f, 2f, 4f };
        public float heightAboveWhichToWatchOceanicity = 0.5f;
        public float highHeightLowOceanicityMin = 6.15f;
        public float highHeightLowOceanicityMax = 24.6f;
        public float heightMidAboveWhichToWatchOceanicity = 0.2f;
        public float midHeightMidOceanicityMin = 4.1f;
        public float midHeightMidOceanicityMax = 16.4f; //These values are the oceanicity at the spot multiplied by the OceanicityFactor, this is what it recieves so it makes it easier to calculate them

        public int heightMapOctaves = 1;
        public float heightMapNoiseScale = 66.0f;
        public float heightMapPersistance = 0.1f;
        public double[] heightmapRemapKeys = { 0.05, 0.2, 0.5, 0.8, 0.95 };
        public double[] heightmapRemapValues = { 0.05, 0.35, 0.5, 0.65, 0.95 };
        public float radiusMultOutwardsForSmoothing = 6.0f;
        public string fallbackParentLandformCode = "ultraflats"; //This is just in case it somehow rolls a height value with no valid Landforms that would fit it, it will use this one instead.
    }
}