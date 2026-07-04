namespace TerraPrety
{
    public class WorldGenConfig
    {
        public float noiseScale = 300.0f;
        public float heightMapNoiseScale = 32.0f;
        public string fallbackParentLandformCode = "ultraflats"; //This is just in case it somehow rolls a height value with no valid Landforms that would fit it, it will use this one instead.

        public bool Delicate_configs_below__alter_at_your_own_peril = false;

        public float oceanWobbleScale = 2.0f;
        public float oceanWobbleIntensity = 1.0f;
        public double[] remappingKeys = { 0.115, 0.285 };
        public double[] remappingValues = {0.0, 1.0 };

        public double[] coastRemappingKeys = { 0.00, 0.285 }; // Starts at 0 opacity, ends at max ocean opacity. Keep the 2nd element the same as remappingKeys's 2nd element so it matches the ocean.
        public double[] coastRemappingValues = { 0.0, 1.0 }; // Don't touch. Basically the coastmap opacities at no coast and full coast.
        public double coastTargetLandformHeight = 0.3; // In coastal areas, move the landform height down towards this value.
        public double coastMinOpacity = 0.15; // Areas below this coastal opacity dont have the coast lowering the landform height.
        public double coastFullOpacity = 0.35; // Areas above this coastal opacity dont get their landform height above coastTargetLandformHeight.

        public int heightMapOctaves = 1;
        public float heightMapPersistance = 0.1f;

        public double[] midHeightKeys = { 0.0, 0.05, 0.33333, 1.0 };
        public double[] midHeightValues = { 1.0, 0.9, 0.0, 0.0 };
        public float chanceForMidZone = 1.0f;
        public float targetMidLevel = 0.2f;
        public float lowThreshForMidZone = 0.2f;

        public float mountainRangesPullsHeightMapTowards = 1.0f; // The mountain ranges drags the landform heightmap towards this value
        public float mountainRangeOceanFadeStrength = 1.0f;

        // Inland mountain ranges gets its own wobble noise, coastal mountain ranges follow the continents, so they use the continental wobble
        public float inlandMountainRangeWobbleScale = 2.5f;
        public float inlandMountainRangeWobbleIntensity = 1.5f;
        public int inlandMountainRangeWobbleOctaves = 2;
        public float inlandMountainRangeWobblePersistence = 0.9f;

        public float inlandMountainRangeScale = 4.0f; // Size of the whole inland mountain range pattern on the map
        public double[] inlandMountainRangeKeys = { 0.915, 0.975 }; // Shape of the inland mountain range
        public double[] inlandMountainRangeValues = { 0.0, 1.0 }; // Don't touch. Basically the inland mountain range opacities at no mountain range and the center of the mountain range

        public float inlandMountainRangeApertureMaskScale = 15.0f; // Larger for larger mountain ranges
        public float inlandMountainRangeApertureMaskThreshold = 0.8f;  // Near 1 gives less mountain ranges, lower gives more
        public float inlandMountainRangeApertureMaskSharpness = 0.1f; // Near 0 gives a smooth mountainrange fadein, higher is sharper

        public double coastalMountainRangeBandPositionInContinent = 0.2; // Near 1 is the center of the continent, near 0 is the ocean
        public double coastalMountainRangeBandBaseWidth = 0.1; // Base width of the coastal mountain range
        public double[] coastalMountainRangeKeys = { 0.6, 0.85 }; // Shape of the coastal mountain range within the band's base width
        public double[] coastalMountainRangeValues = { 0.0, 1.0 }; // Don't touch. Basically the coastal mountain range opacities at the beginning of the band and in the center of the band

        public float coastalMountainRangeApertureMaskScale = 15.0f; // Larger for larger mountain ranges
        public float coastalMountainRangeApertureMaskThreshold = 0.8f;  // Near 1 gives less mountain ranges, lower gives more
        public float coastalMountainRangeApertureMaskSharpness = 0.1f; // Near 0 gives a smooth mountainrange fadein, higher is sharper

        public float radiusMultOutwardsForSmoothing = 6.0f;

        public int hardMinimumCoastalOceanicity = 1;
        public int softMinimumCoastalOceanicity = 30;
        public int maximumCoastalOceanicity = 256;

        // -- Rivers related settings follow! --

        public float chanceForRiver = 0.2f; //The chance for each valid region found, should it contain the start of a river?
        public int minimumRiverOceanicity = 5;
        public int maximumRiverOceanicity = 100;
        public float maxHeightForRiverSink = 0.25f; //Based on the LandformHeightMap heights, not actual y-heights.
        public float chanceToFork = 0.02f;

        public float minimumRiverFlowStrength = 0.25f;
        public float maximumRiverFlowStrength = 1.5f;

        public int maxPointsPerRiverSegment = 10; //Aim to generate a full segment's worth of points before adding them all to the segment and then to the region.
        public float primaryRiverHeightStepFlex = 0.025f; //A primary river step must have a LandformHeightMap value that is only this distance from the current to be considered valid.
        public float tributaryRiverHeightStepFlex = 0.02f; //Mainly just the downward flexibility, since upwards is not really capped for Tributaries, just a desired target for a step.
        public float tributaryDesiredHeightStepUp = 0.04f; //Try to aim for a step that would be at least this amount higher then the current HeightMap value.
        public int riverOceanicityStepFlexibility = 5; //SLIGHT amount of leeway to allow the river to still somewhat travel to the sides, but trend inland.

        public float flowLossPerRiverSegment = 0.03f; //This serves as a hard-stop for a River to cease expanding if the flow gets below 0. Lower Value means longer rivers, generally, unless something else stops it first.

        /*public double terrainNoiseFrequencyMult = 1.0;
        public double terrainNoisePersistance = 0.9;
        public bool enableEdgeLandformSmoothing = false;
        public int landformSmoothingRadius = 3;
        public int landformMapPadding = 4;*/
    }
}