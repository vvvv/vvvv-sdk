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
            this.RegisterType(typeof(double), (host, attribute) => new DoubleInputPin(host, attribute));
            this.RegisterType(typeof(float), (host, attribute) => new FloatInputPin(host, attribute));
            this.RegisterType(typeof(int), (host, attribute) => new IntInputPin(host, attribute));
            this.RegisterType(typeof(bool), (host, attribute) => new BoolInputPin(host, attribute));

            this.RegisterType(typeof(Matrix4x4), (host, attribute) => new Matrix4x4InputPin(host, attribute));
            this.RegisterType(typeof(Matrix), (host, attribute) => new SlimDXMatrixInputPin(host, attribute));

            this.RegisterType(typeof(Vector2D), (host, attribute) => new Vector2DInputPin(host, attribute));
            this.RegisterType(typeof(Vector3D), (host, attribute) => new Vector3DInputPin(host, attribute));
            this.RegisterType(typeof(Vector4D), (host, attribute) => new Vector4DInputPin(host, attribute));

            this.RegisterType(typeof(Vector2), (host, attribute) => new Vector2InputPin(host, attribute));
            this.RegisterType(typeof(Vector3), (host, attribute) => new Vector3InputPin(host, attribute));
            this.RegisterType(typeof(Vector4), (host, attribute) => new Vector4InputPin(host, attribute));

            this.RegisterType(typeof(string), (host, attribute) => new StringInputPin(host, attribute));
            this.RegisterType(typeof(RGBAColor), (host, attribute) => new ColorInputPin(host, attribute));

            this.RegisterType(typeof(EnumEntry), (host, attribute) => new DynamicEnumInputPin(host, attribute));
        }
    }
}
