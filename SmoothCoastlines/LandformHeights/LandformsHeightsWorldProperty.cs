using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.ServerMods.NoObf;

namespace SmoothCoastlines.LandformHeights {
    public class LandformsHeightsWorldProperty : WorldProperty<LandformGenHeight> {
        [JsonIgnore]
        public LandformGenHeight[] LandformHeightsByIndex;
    }
}
