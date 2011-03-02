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
            this.RegisterType(typeof(double), delegate(IPluginHost host, ConfigAttribute attribute) { return new DoubleConfigPin(host, attribute); });
            this.RegisterType(typeof(float), delegate(IPluginHost host, ConfigAttribute attribute) { return new FloatConfigPin(host, attribute); });
            this.RegisterType(typeof(int), delegate(IPluginHost host, ConfigAttribute attribute) { return new IntConfigPin(host, attribute); });
            this.RegisterType(typeof(bool), delegate(IPluginHost host, ConfigAttribute attribute) { return new BoolConfigPin(host, attribute); });

            this.RegisterType(typeof(Vector2D), delegate(IPluginHost host, ConfigAttribute attribute) { return new Vector2DConfigPin(host, attribute); });
            this.RegisterType(typeof(Vector3D), delegate(IPluginHost host, ConfigAttribute attribute) { return new Vector3DConfigPin(host, attribute); });
            this.RegisterType(typeof(Vector4D), delegate(IPluginHost host, ConfigAttribute attribute) { return new Vector4DConfigPin(host, attribute); });

            this.RegisterType(typeof(Vector2), delegate(IPluginHost host, ConfigAttribute attribute) { return new Vector2ConfigPin(host, attribute); });
            this.RegisterType(typeof(Vector3), delegate(IPluginHost host, ConfigAttribute attribute) { return new Vector3ConfigPin(host, attribute); });
            this.RegisterType(typeof(Vector4), delegate(IPluginHost host, ConfigAttribute attribute) { return new Vector4ConfigPin(host, attribute); });

            this.RegisterType(typeof(string), delegate(IPluginHost host, ConfigAttribute attribute) { return new StringConfigPin(host, attribute); });
            this.RegisterType(typeof(RGBAColor), delegate(IPluginHost host, ConfigAttribute attribute) { return new ColorConfigPin(host, attribute); });

            this.RegisterType(typeof(EnumEntry), delegate(IPluginHost host, ConfigAttribute attribute) { return new DynamicEnumConfigPin(host, attribute); });

        }
    }
}
