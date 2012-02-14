using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Config;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.PluginInterfaces.V1;
using SlimDX;

namespace VVVV.Hosting.Pins
{
    [ComVisible(false)]
    public class ConfigPinRegistry : PinTypeRegistry<ConfigAttribute>
    {
        public ConfigPinRegistry()
        {
            //Register default types
            this.RegisterType(typeof(double), (host, attribute, t) => new DoubleConfigPin(host, attribute));
            this.RegisterType(typeof(float), (host, attribute, t) => new FloatConfigPin(host, attribute));
            this.RegisterType(typeof(int), (host, attribute, t) => new IntConfigPin(host, attribute));
            this.RegisterType(typeof(uint), (host, attribute, t) => new UIntConfigPin(host, attribute));
            this.RegisterType(typeof(bool), (host, attribute, t) => new BoolConfigPin(host, attribute));

            this.RegisterType(typeof(Vector2D), (host, attribute, t) => new Vector2DConfigPin(host, attribute));
            this.RegisterType(typeof(Vector3D), (host, attribute, t) => new Vector3DConfigPin(host, attribute));
            this.RegisterType(typeof(Vector4D), (host, attribute, t) => new Vector4DConfigPin(host, attribute));

            this.RegisterType(typeof(Vector2), (host, attribute, t) => new Vector2ConfigPin(host, attribute));
            this.RegisterType(typeof(Vector3), (host, attribute, t) => new Vector3ConfigPin(host, attribute));
            this.RegisterType(typeof(Vector4), (host, attribute, t) => new Vector4ConfigPin(host, attribute));
            this.RegisterType(typeof(Quaternion), (host, attribute, t) => new QuaternionConfigPin(host, attribute));

            this.RegisterType(typeof(string), (host, attribute, t) => new StringConfigPin(host, attribute));

            this.RegisterType(typeof(RGBAColor), (host, attribute, t) => new ColorConfigPin(host, attribute));
            this.RegisterType(typeof(Color4), (host, attribute, t) => new SlimDXColorConfigPin(host, attribute));

            this.RegisterType(typeof(EnumEntry), (host, attribute, t) => new DynamicEnumConfigPin(host, attribute));

        }
    }
}
