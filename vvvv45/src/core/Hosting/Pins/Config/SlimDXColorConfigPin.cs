using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

using SlimDX;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
    public class SlimDXColorConfigPin : ConfigPin<Color4>
    {
        protected IColorConfig FColorConfig;

        public SlimDXColorConfigPin(IPluginHost host, ConfigAttribute attribute)
            : base(host, attribute)
        {
            host.CreateColorConfig(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FColorConfig);
            FColorConfig.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);

            base.Initialize(FColorConfig);
        }

        public override Color4 this[int index]
        {
            get
            {
                RGBAColor value;
                FColorConfig.GetColor(index, out value);
                return new Color4((float)value.A, (float)value.R, (float)value.G, (float)value.B);
            }
            set
            {
                FColorConfig.SetColor(index, new RGBAColor(value.Red, value.Green, value.Blue, value.Alpha));
            }
        }
    }
}
