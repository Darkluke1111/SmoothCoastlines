using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SmoothCoastlines.LandformHeights {

    public class LandformGenHeight : WorldPropertyVariant {
        [JsonIgnore]
        public int index;

        [JsonProperty]
        public double minHeight = 0;

        [JsonProperty]
        public double maxHeight = 1;

        public void Init(int index) {
            this.index = index;
        }
    }
}
