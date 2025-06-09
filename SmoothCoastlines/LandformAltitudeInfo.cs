using Newtonsoft.Json;
using Vintagestory.API.Common;

namespace SmoothCoastlines
{
    public class LandformAltitudeInfo: WorldPropertyVariant
    {
        [JsonIgnore]
        public int LandformIndex;

        [JsonProperty]
        public double MinAltitude = 0;

        [JsonProperty]
        public double MaxAltitude = 1;
    }
}
