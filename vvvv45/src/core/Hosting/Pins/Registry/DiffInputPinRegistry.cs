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
            this.RegisterType(typeof(double), delegate(IPluginHost host, InputAttribute attribute) { return new DiffDoubleInputPin(host, attribute); });
            this.RegisterType(typeof(float), delegate(IPluginHost host, InputAttribute attribute) { return new DiffFloatInputPin(host, attribute); });
            this.RegisterType(typeof(int), delegate(IPluginHost host, InputAttribute attribute) { return new DiffIntInputPin(host, attribute); });
            this.RegisterType(typeof(bool), delegate(IPluginHost host, InputAttribute attribute) { return new DiffBoolInputPin(host, attribute); });

            this.RegisterType(typeof(Vector2D), delegate(IPluginHost host, InputAttribute attribute) { return new DiffVector2DInputPin(host, attribute); });
            this.RegisterType(typeof(Vector3D), delegate(IPluginHost host, InputAttribute attribute) { return new DiffVector3DInputPin(host, attribute); });
            this.RegisterType(typeof(Vector4D), delegate(IPluginHost host, InputAttribute attribute) { return new DiffVector4DInputPin(host, attribute); });

            this.RegisterType(typeof(Vector2), delegate(IPluginHost host, InputAttribute attribute) { return new DiffVector2InputPin(host, attribute); });
            this.RegisterType(typeof(Vector3), delegate(IPluginHost host, InputAttribute attribute) { return new DiffVector3InputPin(host, attribute); });
            this.RegisterType(typeof(Vector4), delegate(IPluginHost host, InputAttribute attribute) { return new DiffVector4InputPin(host, attribute); });

        }
    }
}
