using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
    [ComVisible(false)]
    public class OutputPinRegistry : PinTypeRegistry<OutputAttribute>
    {
        public OutputPinRegistry()
        {
            //Register default types
            this.RegisterType(typeof(double), (host, attribute, t) => new DoubleOutputPin(host, attribute));
            this.RegisterType(typeof(float), (host, attribute, t) => new FloatOutputPin(host, attribute));
            this.RegisterType(typeof(int), (host, attribute, t) => new IntOutputPin(host, attribute));
            this.RegisterType(typeof(uint), (host, attribute, t) => new UIntOutputPin(host, attribute));
            this.RegisterType(typeof(bool), (host, attribute, t) => new BoolOutputPin(host, attribute));

            this.RegisterType(typeof(Matrix4x4), (host, attribute, t) => new Matrix4x4OutputPin(host, attribute));
            this.RegisterType(typeof(Matrix), (host, attribute, t) => new SlimDXMatrixOutputPin(host, attribute));

            this.RegisterType(typeof(Vector2D), (host, attribute, t) => new Vector2DOutputPin(host, attribute));
            this.RegisterType(typeof(Vector3D), (host, attribute, t) => new Vector3DOutputPin(host, attribute));
            this.RegisterType(typeof(Vector4D), (host, attribute, t) => new Vector4DOutputPin(host, attribute));

            this.RegisterType(typeof(Vector2), (host, attribute, t) => new Vector2OutputPin(host, attribute));
            this.RegisterType(typeof(Vector3), (host, attribute, t) => new Vector3OutputPin(host, attribute));
            this.RegisterType(typeof(Vector4), (host, attribute, t) => new Vector4OutputPin(host, attribute));
            this.RegisterType(typeof(Quaternion), (host, attribute, t) => new QuaternionOutputPin(host, attribute));

            this.RegisterType(typeof(string), (host, attribute, t) => new StringOutputPin(host, attribute));
            
            this.RegisterType(typeof(RGBAColor), (host, attribute, t) => new ColorOutputPin(host, attribute));
            this.RegisterType(typeof(Color4), (host, attribute, t) => new SlimDXColorOutputPin(host, attribute));

            this.RegisterType(typeof(EnumEntry), (host, attribute, t) => new DynamicEnumOutputPin(host, attribute));
            
            this.RegisterType(typeof(DXResource<,>), 
                              (host, attribute, t) =>
                              {
                                  var genericArguments = t.GetGenericArguments();
                                  var resourceType = genericArguments[0];
                                  var metadataType = genericArguments[1];
                                  
                                  if (resourceType == typeof(Texture))
                                  {
                                    var textureOutPinType = typeof(TextureOutputPin<,>);
                                    textureOutPinType = textureOutPinType.MakeGenericType(t, metadataType);
                                    return Activator.CreateInstance(textureOutPinType, host, attribute);
                                  }
                                  else
                                  {
                                      throw new NotImplementedException();
                                  }
                              });
        }
    }
}
