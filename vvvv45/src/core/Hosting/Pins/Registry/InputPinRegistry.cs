using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;

using SlimDX;

namespace VVVV.Hosting.Pins
{
    public class InputPinRegistry : PinTypeRegistry<InputAttribute>
    {
        public InputPinRegistry()
        {
            //Register default types
            this.RegisterType(typeof(double), delegate(IPluginHost host, InputAttribute attribute) { return new DoubleInputPin(host, attribute); });
            this.RegisterType(typeof(float), delegate(IPluginHost host, InputAttribute attribute) { return new FloatInputPin(host, attribute); });
            this.RegisterType(typeof(int), delegate(IPluginHost host, InputAttribute attribute) { return new IntInputPin(host, attribute); });
            this.RegisterType(typeof(bool), delegate(IPluginHost host, InputAttribute attribute) { return new BoolInputPin(host, attribute); });

            this.RegisterType(typeof(Matrix4x4), delegate(IPluginHost host, InputAttribute attribute) { return new Matrix4x4InputPin(host, attribute); });
            this.RegisterType(typeof(Matrix), delegate(IPluginHost host, InputAttribute attribute) { return new SlimDXMatrixInputPin(host, attribute); });

            this.RegisterType(typeof(Vector2D), delegate(IPluginHost host, InputAttribute attribute) { return new Vector2DInputPin(host, attribute); });
            this.RegisterType(typeof(Vector3D), delegate(IPluginHost host, InputAttribute attribute) { return new Vector3DInputPin(host, attribute); });
            this.RegisterType(typeof(Vector4D), delegate(IPluginHost host, InputAttribute attribute) { return new Vector4DInputPin(host, attribute); });

            this.RegisterType(typeof(Vector2), delegate(IPluginHost host, InputAttribute attribute) { return new Vector2InputPin(host, attribute); });
            this.RegisterType(typeof(Vector3), delegate(IPluginHost host, InputAttribute attribute) { return new Vector3InputPin(host, attribute); });
            this.RegisterType(typeof(Vector4), delegate(IPluginHost host, InputAttribute attribute) { return new Vector4InputPin(host, attribute); });

            this.RegisterType(typeof(string), delegate(IPluginHost host, InputAttribute attribute) { return new StringInputPin(host, attribute); });
            this.RegisterType(typeof(RGBAColor), delegate(IPluginHost host, InputAttribute attribute) { return new ColorInputPin(host, attribute); });

            this.RegisterType(typeof(EnumEntry), delegate(IPluginHost host, InputAttribute attribute) { return new DynamicEnumInputPin(host, attribute); });

        }
    }
}
