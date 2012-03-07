using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using SlimDX;

namespace VVVV.Hosting.Pins
{
    [ComVisible(false)]
    public class InputPinRegistry : PinTypeRegistry<InputAttribute>
    {
        public InputPinRegistry()
        {
            //Register default types
            this.RegisterType(typeof(double), (host, attribute, t) => new DoubleInputPin(host, attribute));
            this.RegisterType(typeof(float), (host, attribute, t) => new FloatInputPin(host, attribute));
            this.RegisterType(typeof(int), (host, attribute, t) => new IntInputPin(host, attribute));
            this.RegisterType(typeof(uint), (host, attribute, t) => new UIntInputPin(host, attribute));
            this.RegisterType(typeof(bool), (host, attribute, t) => new BoolInputPin(host, attribute));

            this.RegisterType(typeof(Matrix4x4), (host, attribute, t) => new Matrix4x4InputPin(host, attribute));
            this.RegisterType(typeof(Matrix), (host, attribute, t) => new SlimDXMatrixInputPin(host, attribute));

            this.RegisterType(typeof(Vector2D), (host, attribute, t) => new Vector2DInputPin(host, attribute));
            this.RegisterType(typeof(Vector3D), (host, attribute, t) => new Vector3DInputPin(host, attribute));
            this.RegisterType(typeof(Vector4D), (host, attribute, t) => new Vector4DInputPin(host, attribute));

            this.RegisterType(typeof(Vector2), (host, attribute, t) => new Vector2InputPin(host, attribute));
            this.RegisterType(typeof(Vector3), (host, attribute, t) => new Vector3InputPin(host, attribute));
            this.RegisterType(typeof(Vector4), (host, attribute, t) => new Vector4InputPin(host, attribute));
            this.RegisterType(typeof(Quaternion), (host, attribute, t) => new QuaternionInputPin(host, attribute));

            this.RegisterType(typeof(string), (host, attribute, t) => new StringInputPin(host, attribute));

            this.RegisterType(typeof(RGBAColor), (host, attribute, t) => new ColorInputPin(host, attribute));
            this.RegisterType(typeof(Color4), (host, attribute, t) => new SlimDXColorInputPin(host, attribute));

            this.RegisterType(typeof(EnumEntry), (host, attribute, t) => new DynamicEnumInputPin(host, attribute));
        }
    }
}
