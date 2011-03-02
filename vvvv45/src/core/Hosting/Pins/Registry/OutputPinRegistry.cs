using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins.Output;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;

using SlimDX;


namespace VVVV.Hosting.Pins
{
    public class OutputPinRegistry : PinTypeRegistry<OutputAttribute>
    {
        public OutputPinRegistry()
        {
            //Register default types
            this.RegisterType(typeof(double), delegate(IPluginHost host, OutputAttribute attribute) { return new DoubleOutputPin(host, attribute); });
            this.RegisterType(typeof(float), delegate(IPluginHost host, OutputAttribute attribute) { return new FloatOutputPin(host, attribute); });
            this.RegisterType(typeof(int), delegate(IPluginHost host, OutputAttribute attribute) { return new IntOutputPin(host, attribute); });
            this.RegisterType(typeof(bool), delegate(IPluginHost host, OutputAttribute attribute) { return new BoolOutputPin(host, attribute); });

            this.RegisterType(typeof(Matrix4x4), delegate(IPluginHost host, OutputAttribute attribute) { return new Matrix4x4OutputPin(host, attribute); });
            this.RegisterType(typeof(Matrix), delegate(IPluginHost host, OutputAttribute attribute) { return new SlimDXMatrixOutputPin(host, attribute); });

            this.RegisterType(typeof(Vector2D), delegate(IPluginHost host, OutputAttribute attribute) { return new Vector2DOutputPin(host, attribute); });
            this.RegisterType(typeof(Vector3D), delegate(IPluginHost host, OutputAttribute attribute) { return new Vector3DOutputPin(host, attribute); });
            this.RegisterType(typeof(Vector4D), delegate(IPluginHost host, OutputAttribute attribute) { return new Vector4DOutputPin(host, attribute); });

            this.RegisterType(typeof(Vector2), delegate(IPluginHost host, OutputAttribute attribute) { return new Vector2OutputPin(host, attribute); });
            this.RegisterType(typeof(Vector3), delegate(IPluginHost host, OutputAttribute attribute) { return new Vector3OutputPin(host, attribute); });
            this.RegisterType(typeof(Vector4), delegate(IPluginHost host, OutputAttribute attribute) { return new Vector4OutputPin(host, attribute); });

            this.RegisterType(typeof(string), delegate(IPluginHost host, OutputAttribute attribute) { return new StringOutputPin(host, attribute); });
            this.RegisterType(typeof(RGBAColor), delegate(IPluginHost host, OutputAttribute attribute) { return new ColorOutputPin(host, attribute); });

            this.RegisterType(typeof(EnumEntry), delegate(IPluginHost host, OutputAttribute attribute) { return new DynamicEnumOutputPin(host, attribute); });

        }
    }
}
