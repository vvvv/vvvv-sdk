using System;
using System.Linq;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;
using SlimDX;

namespace VVVV.Hosting.IO
{
    class SpreadRegistry : IORegistryBase
    {
        public SpreadRegistry()
        {
            RegisterInput(typeof(ISpread<>), (factory, context) => {
                              var attribute = context.IOAttribute;
                              var t = context.DataType;
                              ISpread spread = null;
                              if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                              {
                                  if (attribute.IsBinSizeEnabled && typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                  {
                                      var spreadType = attribute.CheckIfChanged
                                          ? typeof(DiffInputBinSpread<>).MakeGenericType(t.GetGenericArguments().First())
                                          : typeof(InputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                      
                                      if (attribute.IsPinGroup)
                                      {
                                          spreadType = attribute.CheckIfChanged 
                                              ? typeof(DiffInputSpreadList<>).MakeGenericType(t.GetGenericArguments().First())
                                              : typeof(InputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                      }

                                      if (context.BinSizeIOContainer != null)
                                          spread = Activator.CreateInstance(spreadType, factory, attribute.Clone(), context.BinSizeIOContainer) as ISpread;
                                      else
                                          spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                      if (attribute.CheckIfChanged)
                                      {
                                          if (attribute.AutoValidate)
                                              return GenericIOContainer.Create(context, factory, spread, s => s.Sync(), s => s.Flush());
                                          else
                                              return GenericIOContainer.Create(context, factory, spread, null, s => s.Flush());
                                      }
                                      else
                                      {
                                          if (attribute.AutoValidate)
                                              return GenericIOContainer.Create(context, factory, spread, s => s.Sync());
                                          else
                                              return GenericIOContainer.Create(context, factory, spread);
                                      }
                                  }
                              }
                              var container = factory.CreateIOContainer(typeof(IInStream<>).MakeGenericType(context.DataType), attribute, false);
                              var pinType = typeof(InputPin<>).MakeGenericType(context.DataType);
                              spread = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                              if (attribute.AutoValidate)
                                  return IOContainer.Create(context, spread, container, p => p.Sync());
                              else
                                  return IOContainer.Create(context, spread, container);
                          });
            
            RegisterInput(typeof(IDiffSpread<>), (factory, context) => {
                              var attribute = context.IOAttribute;
                              var t = context.DataType;
                              attribute.CheckIfChanged = true;
                              ISpread spread = null;
                              
                              if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                              {
                                  if (attribute.IsBinSizeEnabled && typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                  {
                                      var spreadType = typeof(DiffInputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                      
                                      if (attribute.IsPinGroup)
                                      {
                                          spreadType = typeof(DiffInputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                      }

                                      if (context.BinSizeIOContainer != null)
                                          spread = Activator.CreateInstance(spreadType, factory, attribute.Clone(), context.BinSizeIOContainer) as ISpread;
                                      else
                                          spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                      if (attribute.AutoValidate)
                                          return GenericIOContainer.Create(context, factory, spread, s => s.Sync(), s => s.Flush());
                                      else
                                          return GenericIOContainer.Create(context, factory, spread, null, s => s.Flush());
                                  }
                              }
                              
                              var container = factory.CreateIOContainer(typeof(IInStream<>).MakeGenericType(context.DataType), attribute, false);
                              var pinType = typeof(DiffInputPin<>).MakeGenericType(context.DataType);
                              if (context.DataType == typeof(Matrix4x4)) //special diff pin for transform pins until IsChanged gets fixed in delphi
                                  spread = new DiffInputPin<Matrix4x4>(factory, container.GetPluginIO() as IPluginIn, new BufferedInputMatrix4x4IOStream(container.RawIOObject as IInStream<Matrix4x4>));
                              else if (context.DataType == typeof(Matrix))
                                  spread = new DiffInputPin<Matrix>(factory, container.GetPluginIO() as IPluginIn, new BufferedInputMatrixIOStream(container.RawIOObject as IInStream<Matrix>));
                              else	
                                  spread = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                              if (attribute.AutoValidate)
                                  return IOContainer.Create(context, spread, container, s => s.Sync());
                              else
                                  return IOContainer.Create(context, spread, container);
                          },
                          false);
            
            RegisterOutput(typeof(ISpread<>), (factory, context) => {
                               var attribute = context.IOAttribute;
                               var t = context.DataType;
                               if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                               {
                                   if (attribute.IsBinSizeEnabled && typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                   {
                                       var spreadType = typeof(OutputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                       
                                       if (attribute.IsPinGroup)
                                       {
                                           spreadType = typeof(OutputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                       }

                                       ISpread spread;
                                       if (context.BinSizeIOContainer != null)
                                           spread = Activator.CreateInstance(spreadType, factory, attribute.Clone(), context.BinSizeIOContainer) as ISpread;
                                       else
                                           spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                       if (context.IOAttribute.AutoFlush)
                                           return GenericIOContainer.Create(context, factory, spread, null, s => s.Flush());
                                       else
                                           return GenericIOContainer.Create(context, factory, spread);
                                   }
                               }
                               var container = factory.CreateIOContainer(typeof(IOutStream<>).MakeGenericType(context.DataType), attribute, false);
                               var pinType = typeof(OutputPin<>).MakeGenericType(context.DataType);
                               var pin = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                               if (context.IOAttribute.AutoFlush)
                                   return IOContainer.Create(context, pin, container, null, p => p.Flush());
                               else
                                   return IOContainer.Create(context, pin, container);
                           });
            
            RegisterConfig(typeof(IDiffSpread<>),
                           (factory, context) =>
                           {
                               var attribute = context.IOAttribute;
                               var container = factory.CreateIOContainer(typeof(IIOStream<>).MakeGenericType(context.DataType), attribute, false);
                               var pinType = typeof(ConfigPin<>).MakeGenericType(context.DataType);
                               var spread = (IDiffSpread) Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject);
                               return IOContainer.Create(context, spread, container, null, s => s.Flush(), p => p.Sync());
                           },
                           true);
        }
    }
}
