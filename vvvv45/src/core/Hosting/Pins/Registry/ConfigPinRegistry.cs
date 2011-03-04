using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Config;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.PluginInterfaces.V1;

using SlimDX;



namespace VVVV.Hosting.Pins
{
    public class ConfigPinRegistry : PinTypeRegistry<ConfigAttribute>
    {
        public ConfigPinRegistry()
        {
            //Register default types
            this.RegisterType(typeof(double), (host, attribute) => new DoubleConfigPin(host, attribute));
            this.RegisterType(typeof(float), (host, attribute) => new FloatConfigPin(host, attribute));
            this.RegisterType(typeof(int), (host, attribute) => new IntConfigPin(host, attribute));
            this.RegisterType(typeof(bool), (host, attribute) => new BoolConfigPin(host, attribute));

            this.RegisterType(typeof(Vector2D), (host, attribute) => new Vector2DConfigPin(host, attribute));
            this.RegisterType(typeof(Vector3D), (host, attribute) => new Vector3DConfigPin(host, attribute));
            this.RegisterType(typeof(Vector4D), (host, attribute) => new Vector4DConfigPin(host, attribute));

            this.RegisterType(typeof(Vector2), (host, attribute) => new Vector2ConfigPin(host, attribute));
            this.RegisterType(typeof(Vector3), (host, attribute) => new Vector3ConfigPin(host, attribute));
            this.RegisterType(typeof(Vector4), (host, attribute) => new Vector4ConfigPin(host, attribute));

            this.RegisterType(typeof(string), (host, attribute) => new StringConfigPin(host, attribute));
            this.RegisterType(typeof(RGBAColor), (host, attribute) => new ColorConfigPin(host, attribute));

            this.RegisterType(typeof(EnumEntry), (host, attribute) => new DynamicEnumConfigPin(host, attribute));

        }
    }
}
