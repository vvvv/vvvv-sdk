using System;
using System.Linq;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO
{
    class PinRegistry : IORegistryBase
    {
        public PinRegistry()
        {
            RegisterInput(typeof(ISpread<>), (factory, attribute, t) => {
                              ISpread spread = null;
                              if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                              {
                                  if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                  {
                                      var spreadType = typeof(InputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                      
                                      if (attribute.IsPinGroup)
                                      {
                                          spreadType = typeof(InputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                      }
                                      
                                      spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                      if (attribute.AutoValidate)
                                          return IOContainer.Create(factory, spread, null, p => p.Sync());
                                      else
                                          return IOContainer.Create(factory, spread, null);
                                  }
                              }
                              var ioHandler = factory.CreateIOContainer(typeof(IInStream<>).MakeGenericType(t), attribute, false);
                              var pinType = typeof(InputPin<>).MakeGenericType(t);
                              spread = Activator.CreateInstance(pinType, ioHandler.PluginIO, ioHandler.RawIOObject) as ISpread;
                              if (attribute.AutoValidate)
                                  return IOContainer.Create(factory, spread, ioHandler.PluginIO, p => p.Sync());
                              else
                                  return IOContainer.Create(factory, spread, ioHandler.PluginIO);
                          });
            
            RegisterInput(typeof(IDiffSpread<>),
                          (factory, attribute, t) =>
                          {
                              attribute.CheckIfChanged = true;
                              ISpread spread = null;
                              
                              if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                              {
                                  if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                  {
                                      var spreadType = typeof(DiffInputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                      
                                      if (attribute.IsPinGroup)
                                      {
                                          spreadType = typeof(DiffInputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                      }
                                      
                                      spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                      if (attribute.AutoValidate)
                                          return IOContainer.Create(factory, spread, null, p => p.Sync());
                                      else
                                          return IOContainer.Create(factory, spread, null);
                                  }
                              }
                              var ioBuilder = factory.CreateIOContainer(typeof(IInStream<>).MakeGenericType(t), attribute, false);
                              var pinType = typeof(DiffInputPin<>).MakeGenericType(t);
                              spread = Activator.CreateInstance(pinType, ioBuilder.PluginIO, ioBuilder.RawIOObject) as ISpread;
                              if (attribute.AutoValidate)
                                  return IOContainer.Create(factory, spread, ioBuilder.PluginIO, p => p.Sync());
                              else
                                  return IOContainer.Create(factory, spread, ioBuilder.PluginIO);
                          },
                          false);
            
            RegisterInput(typeof(IDXRenderStateIn), (factory, attribute, t) => {
                              var host = factory.PluginHost;
                              IDXRenderStateIn pin;
                              host.CreateRenderStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                              return IOContainer.Create(factory, pin, pin);
                          });
            
            RegisterInput(typeof(IDXSamplerStateIn), (factory, attribute, t) => {
                              var host = factory.PluginHost;
                              IDXSamplerStateIn pin;
                              host.CreateSamplerStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                              return IOContainer.Create(factory, pin, pin);
                          });
            
            
            RegisterOutput(typeof(IDXLayerIO), (factory, attribute, t) => {
                               var host = factory.PluginHost;
                               IDXLayerIO pin;
                               host.CreateLayerOutput(attribute.Name, (TPinVisibility)attribute.Visibility, out pin);
                               return IOContainer.Create(factory, pin, pin);
                           });
            
            RegisterOutput(typeof(IDXMeshOut), (factory, attribute, t) => {
                               var host = factory.PluginHost;
                               IDXMeshOut pin;
                               host.CreateMeshOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                               return IOContainer.Create(factory, pin, pin);
                           });
            
            RegisterOutput(typeof(IDXTextureOut), (factory, attribute, t) => {
                               var host = factory.PluginHost;
                               IDXTextureOut pin;
                               host.CreateTextureOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                               return IOContainer.Create(factory, pin, pin);
                           });
            
            RegisterOutput(typeof(ISpread<>), (factory, attribute, t) => {
                               if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                               {
                                   if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                   {
                                       var spreadType = typeof(OutputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                       
                                       if (attribute.IsPinGroup)
                                       {
                                           spreadType = typeof(OutputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                       }
                                       
                                       var stream = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                       return IOContainer.Create(factory, stream, null, null, p => p.Flush());
                                   }
                               }
                               var ioBuilder = factory.CreateIOContainer(typeof(IOutStream<>).MakeGenericType(t), attribute, false);
                               var pinType = typeof(OutputPin<>).MakeGenericType(t);
                               var pin = Activator.CreateInstance(pinType, ioBuilder.PluginIO, ioBuilder.RawIOObject) as ISpread;
                               return IOContainer.Create(factory, pin, ioBuilder.PluginIO, null, p => p.Flush());
                           });
            
            RegisterConfig(typeof(ISpread<>), (factory, attribute, t) => {
                               var ioBuilder = factory.CreateIOContainer(typeof(IIOStream<>).MakeGenericType(t), attribute, false);
                               var pinType = typeof(ConfigPin<>).MakeGenericType(t);
                               var spread = (ISpread) Activator.CreateInstance(pinType, ioBuilder.PluginIO, ioBuilder.RawIOObject);
                               return IOContainer.Create(factory, spread, ioBuilder.PluginIO, null, s => s.Flush(), p => p.Sync());
                           });
            
            RegisterConfig(typeof(IDiffSpread<>),
                           (factory, attribute, t) =>
                           {
                               var ioBuilder = factory.CreateIOContainer(typeof(IIOStream<>).MakeGenericType(t), attribute, false);
                               var pinType = typeof(ConfigPin<>).MakeGenericType(t);
                               var spread = (IDiffSpread) Activator.CreateInstance(pinType, ioBuilder.PluginIO, ioBuilder.RawIOObject);
                               return IOContainer.Create(factory, spread, ioBuilder.PluginIO, null, s => s.Flush(), p => p.Sync());
                           },
                           false);
        }
    }
}
