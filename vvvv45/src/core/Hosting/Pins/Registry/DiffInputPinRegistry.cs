using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins.Input;
using VVVV.Utils.VMath;

using SlimDX;

namespace VVVV.Hosting.Pins
{
    public class DiffInputPinRegistry : PinTypeRegistry<InputAttribute>
    {
        public DiffInputPinRegistry()
        {
            //Register default types
            this.RegisterType(typeof(double), (host, attribute) => new DiffDoubleInputPin(host, attribute));
            this.RegisterType(typeof(float), (host, attribute) => new DiffFloatInputPin(host, attribute));
            this.RegisterType(typeof(int), (host, attribute) => new DiffIntInputPin(host, attribute));
            this.RegisterType(typeof(bool), (host, attribute) => new DiffBoolInputPin(host, attribute));

            this.RegisterType(typeof(Vector2D), (host, attribute) => new DiffVector2DInputPin(host, attribute));
            this.RegisterType(typeof(Vector3D), (host, attribute) => new DiffVector3DInputPin(host, attribute));
            this.RegisterType(typeof(Vector4D), (host, attribute) => new DiffVector4DInputPin(host, attribute));

            this.RegisterType(typeof(Vector2), (host, attribute) => new DiffVector2InputPin(host, attribute));
            this.RegisterType(typeof(Vector3), (host, attribute) => new DiffVector3InputPin(host, attribute));
            this.RegisterType(typeof(Vector4), (host, attribute) => new DiffVector4InputPin(host, attribute));

        }
    }
}
