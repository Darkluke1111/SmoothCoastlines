using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.ServerMods.NoObf;

namespace SmoothCoastlines
{
    public class LandformAltitudeInfoWorldProperty : WorldProperty<LandformAltitudeInfo>
    {

        

        public LandformAltitudeInfo GetInfo(LandformVariant landform)
        {
            return this.Variants.FirstOrDefault(info => landform.index == info.LandformIndex, null);
        }

        internal void InitIndices(LandformsWorldProperty landforms)
        {
            foreach (var info in this.Variants)
            {
                var landform = landforms.Variants.FirstOrDefault(landform => landform.Code.Path == info.Code.Path);
                if(landform != null)
                {
                    info.LandformIndex = landform.index;
                } else
                {
                    throw new Exception("Couldn't find a landform that was referenced in the AltitudeInfo!");
                }
            }
        }
    }
}
