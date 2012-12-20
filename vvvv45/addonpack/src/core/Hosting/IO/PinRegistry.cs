using System;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Hosting.IO
{
    class PinRegistry : IORegistryBase
    {
        public PinRegistry()
        {
            RegisterInput(typeof(Pin<>), (factory, context) => {
                              var attribute = context.IOAttribute;
                              var container = factory.CreateIOContainer(typeof(IInStream<>).MakeGenericType(context.DataType), attribute, false);
                              var pinType = typeof(InputPin<>).MakeGenericType(context.DataType);
                              var spread = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                              if (attribute.AutoValidate)
                                  return IOContainer.Create(context, spread, container, p => p.Sync());
                              else
                                  return IOContainer.Create(context, spread, container);
                          },
                          false);
            
            RegisterOutput(typeof(Pin<>), (factory, context) => {
                               var attribute = context.IOAttribute;
                               var container = factory.CreateIOContainer(typeof(IOutStream<>).MakeGenericType(context.DataType), attribute, false);
                               var pinType = typeof(OutputPin<>).MakeGenericType(context.DataType);
                               var pin = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                               return IOContainer.Create(context, pin, container, null, p => p.Flush());
                           },
                           false);
            
            RegisterConfig(typeof(Pin<>), (factory, context) => {
                               var attribute = context.IOAttribute;
                               var container = factory.CreateIOContainer(typeof(IIOStream<>).MakeGenericType(context.DataType), attribute, false);
                               var pinType = typeof(ConfigPin<>).MakeGenericType(context.DataType);
                               var spread = (ISpread) Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject);
                               return IOContainer.Create(context, spread, container, null, s => s.Flush(), p => p.Sync());
                           },
                           false);
        }
    }
}
