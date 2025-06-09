using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace SmoothCoastlines
{
    class AddGenMaps : ModSystem
    {
        ICoreServerAPI sapi;

        int noiseSizeAltitude;

        MapLayerBase altitudeGen;

        public override double ExecuteOrder()
        {
            return 0.05;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.sapi = api;

            api.Event.InitWorldGenerator(initWorldGen, "standard");

            api.Event.MapRegionGeneration(OnMapRegionGen, "standard");
        }

        public void initWorldGen()
        {
            long seed = sapi.WorldManager.Seed;
            noiseSizeAltitude = sapi.WorldManager.RegionSize / SmoothCoastlinesModSystem.config.altitudeMapScale;
            altitudeGen = GetAltitudeMapGen(seed + 4);

        }

        public void OnMapRegionGen(IMapRegion mapRegion, int regionX, int regionZ, ITreeAttribute chunkGenParams = null)
        {
            int pad = 0;
            IntDataMap2D dataMap = IntDataMap2D.CreateEmpty();
            dataMap.Size = noiseSizeAltitude;
            dataMap.BottomRightPadding = pad;
            dataMap.TopLeftPadding = pad;
            dataMap.Data = altitudeGen.GenLayer(regionX * noiseSizeAltitude - pad,
                regionZ * noiseSizeAltitude - pad,
                noiseSizeAltitude + 2 * pad,
                noiseSizeAltitude + 2 * pad);
            
            mapRegion.ModMaps.Add("AltitudeMap", dataMap);

            
                
        }

        public static MapLayerBase GetAltitudeMapGen(long seed)
        {
            return new MapLayerAltitude(seed);
        }
    }
}
