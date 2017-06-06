using System;
using System.Linq;
using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Hosting.IO.Streams;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;

namespace VVVV.Hosting.IO
{
    unsafe class StreamRegistry : IORegistryBase
    {
        public StreamRegistry()
        {
            int* pLength;
            double** ppDoubleData;
            float** ppFloatData;
            Func<bool> validateFunc;

            
            
            RegisterInput(typeof(IInStream<double>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new DoubleInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput(typeof(IInStream<float>), (factory, context) => 
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new FloatInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<byte>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new ByteInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<sbyte>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new SByteInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<int>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new IntInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput(typeof(IInStream<uint>), (factory, context) => 
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new UIntInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });


            RegisterInput(typeof(IInStream<long>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new LongInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<ulong>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new ULongInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput(typeof(IInStream<bool>), (factory, context) => 
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new BoolInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<Matrix4x4>), (factory, context) => 
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformIn)));
                var transformIn = container.RawIOObject as ITransformIn;
                transformIn.GetMatrixPointer(out pLength, out ppFloatData);
                var stream = new Matrix4x4InStream(pLength, (Matrix**) ppFloatData, GetValidateFunc(transformIn, context.IOAttribute.AutoValidate));
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput(typeof(IInStream<Matrix>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformIn)));
                var transformIn = container.RawIOObject as ITransformIn;
                transformIn.GetMatrixPointer(out pLength, out ppFloatData);
                var stream = new MatrixInStream(pLength, (Matrix**) ppFloatData, GetValidateFunc(transformIn, context.IOAttribute.AutoValidate));
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<Vector2D>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new Vector2DInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            RegisterInput(typeof(IInStream<Vector3D>),(factory, context) => 
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new Vector3DInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            RegisterInput(typeof(IInStream<Vector4D>),(factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new Vector4DInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<Vector2>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new Vector2InStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            RegisterInput(typeof(IInStream<Vector3>), (factory, context) => 
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new Vector3InStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            RegisterInput(typeof(IInStream<Vector4>), (factory, context) =>
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new Vector4InStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput(typeof(IInStream<Quaternion>), (factory, context) => 
            {
                var container = GetValueContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new QuaternionInStream(pLength, ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput(typeof(IInStream<RGBAColor>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorIn)));
                var colorIn = container.RawIOObject as IColorIn;
                colorIn.GetColorPointer(out pLength, out ppDoubleData);
                var stream = new ColorInStream(pLength, (RGBAColor**) ppDoubleData, GetValidateFunc(colorIn, context.IOAttribute.AutoValidate));
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput(typeof(IInStream<Color4>), (factory, context) => 
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorIn)));
                var colorIn = container.RawIOObject as IColorIn;
                colorIn.GetColorPointer(out pLength, out ppDoubleData);
                var stream = new SlimDXColorInStream(pLength, (RGBAColor**) ppDoubleData, GetValidateFunc(colorIn, context.IOAttribute.AutoValidate));
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });
            
            RegisterInput<MemoryIOStream<string>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IStringIn)));
                    var stringIn = container.RawIOObject as IStringIn;
                    var stream = new StringInStream(stringIn);
                    // Using MemoryIOStream -> needs to be synced on managed side.
                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });

            RegisterInput<MemoryIOStream<char>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IStringIn)));
                    var stringIn = container.RawIOObject as IStringIn;
                    var stream = new CharInStream(stringIn);
                    // Using MemoryIOStream -> needs to be synced on managed side.
                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });

            RegisterInput<MemoryIOStream<System.IO.Stream>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IRawIn)));
                    var rawIn = container.RawIOObject as IRawIn;
                    var stream = new RawInStream(rawIn);
                    // Using MemoryIOStream -> needs to be synced on managed side.
                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });

            RegisterInput<MemoryIOStream<EnumEntry>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumIn)));
                    var enumIn = container.RawIOObject as IEnumIn;
                    var stream = new DynamicEnumInStream(enumIn, context.IOAttribute.EnumName);
                    // Using MemoryIOStream -> needs to be synced on managed side.
                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });
            
            // InputIOStream can fullfill this contract a little more memory efficient than BufferedIOStream
            RegisterInput(typeof(IIOStream<>),
                          (factory, context) =>
                          {
                              var inStreamType = typeof(IInStream<>).MakeGenericType(context.DataType);
                              var ioStreamType = typeof(InputIOStream<>).MakeGenericType(context.DataType);
                              var container = factory.CreateIOContainer(context.ReplaceIOType(inStreamType));
                              var ioStream = (IIOStream) Activator.CreateInstance(ioStreamType, container.RawIOObject);
                              if (context.IOAttribute.AutoValidate)
                                  return IOContainer.Create(context, ioStream, container, s => s.Sync(), s => s.Flush());
                              else
                                  return IOContainer.Create(context, ioStream, container, null, s => s.Flush());
                          },
                          false);
            
            RegisterInput(typeof(IInStream<>), (factory, context) => {
                              var t = context.DataType;
                              var attribute = context.IOAttribute;
                              if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                              {
                                  if (attribute.IsBinSizeEnabled && typeof(IInStream<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                  {
                                      var multiDimStreamType = typeof(MultiDimInStream<>).MakeGenericType(t.GetGenericArguments().First());
                                      if (attribute.IsPinGroup)
                                      {
                                          multiDimStreamType = typeof(GroupInStream<>).MakeGenericType(t.GetGenericArguments().First());
                                      }
                                      
                                      var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IInStream;
                                      
                                      if (attribute.AutoValidate)
                                          return GenericIOContainer.Create(context, factory, stream, s => s.Sync());
                                      else
                                          return GenericIOContainer.Create(context, factory, stream);
                                  }
                              }
                              
                              {
                                  IIOContainer container;
                                  IInStream stream;
                                  if (context.DataType == typeof(MouseState))
                                  {
                                      context = context.ReplaceDataType(typeof(Mouse));
                                      container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeIn)));
                                      stream = new MouseToMouseStateInStream(factory, container.GetPluginIO() as INodeIn);
                                  }
                                  else if (context.DataType == typeof(KeyboardState))
                                  {
                                      context = context.ReplaceDataType(typeof(Keyboard));
                                      container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeIn)));
                                      stream = new KeyboardToKeyboardStateInStream(factory, container.GetPluginIO() as INodeIn);
                                  }
                                  else
                                  {
                                      container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeIn)));
                                      var dataType = context.DataType;
                                      var uncheckedDefaultValue = context.IOAttribute.DefaultNodeValue;
                                      var defaultValue = uncheckedDefaultValue != null && dataType.IsAssignableFrom(uncheckedDefaultValue.GetType()) ? uncheckedDefaultValue : dataType.IsValueType ? Activator.CreateInstance(dataType) : null;
                                      stream = Activator.CreateInstance(typeof(NodeInStream<>).MakeGenericType(context.DataType), container.RawIOObject, null, defaultValue) as IInStream;
                                  }
                                  // Using MemoryIOStream -> needs to be synced on managed side.
                                  if (attribute.AutoValidate)
                                      return IOContainer.Create(context, stream, container, s => s.Sync());
                                  else
                                      return IOContainer.Create(context, stream, container);
                              }
                          });
            
            RegisterOutput(typeof(IOutStream<double>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new DoubleOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });
            
            RegisterOutput(typeof(IOutStream<float>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new FloatOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });

            RegisterOutput(typeof(IOutStream<byte>), (factory, context) => {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                var valueOut = container.RawIOObject as IValueOut;
                valueOut.GetValuePointer(out ppDoubleData);
                return IOContainer.Create(context, new ByteOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
            });

            RegisterOutput(typeof(IOutStream<sbyte>), (factory, context) => {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                var valueOut = container.RawIOObject as IValueOut;
                valueOut.GetValuePointer(out ppDoubleData);
                return IOContainer.Create(context, new SByteOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
            });

            RegisterOutput(typeof(IOutStream<int>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new IntOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });
            
            RegisterOutput(typeof(IOutStream<uint>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new UIntOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });

            RegisterOutput(typeof(IOutStream<long>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                var valueOut = container.RawIOObject as IValueOut;
                valueOut.GetValuePointer(out ppDoubleData);
                return IOContainer.Create(context, new LongOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
            });

            RegisterOutput(typeof(IOutStream<ulong>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                var valueOut = container.RawIOObject as IValueOut;
                valueOut.GetValuePointer(out ppDoubleData);
                return IOContainer.Create(context, new ULongOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
            });
            
            RegisterOutput(typeof(IOutStream<bool>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new BoolOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });

            RegisterOutput(typeof(IOutStream<Matrix4x4>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformOut)));
                               var transformOut = container.RawIOObject as ITransformOut;
                               transformOut.GetMatrixPointer(out ppFloatData);
                               return IOContainer.Create(context, new Matrix4x4OutStream((Matrix**) ppFloatData, GetSetMatrixLengthAction(transformOut)), container);
                           });
            
            RegisterOutput(typeof(IOutStream<Matrix>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformOut)));
                               var transformOut = container.RawIOObject as ITransformOut;
                               transformOut.GetMatrixPointer(out ppFloatData);
                               return IOContainer.Create(context, new MatrixOutStream((Matrix**) ppFloatData, GetSetMatrixLengthAction(transformOut)), container);
                           });

            RegisterOutput(typeof(IOutStream<Vector2D>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new Vector2DOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });
            RegisterOutput(typeof(IOutStream<Vector3D>),(factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new Vector3DOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });
            RegisterOutput(typeof(IOutStream<Vector4D>),(factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new Vector4DOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });

            RegisterOutput(typeof(IOutStream<Vector2>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new Vector2OutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });
            RegisterOutput(typeof(IOutStream<Vector3>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new Vector3OutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });
            RegisterOutput(typeof(IOutStream<Vector4>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new Vector4OutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });
            
            RegisterOutput(typeof(IOutStream<Quaternion>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueOut)));
                               var valueOut = container.RawIOObject as IValueOut;
                               valueOut.GetValuePointer(out ppDoubleData);
                               return IOContainer.Create(context, new QuaternionOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), container);
                           });

            RegisterOutput(typeof(IOutStream<RGBAColor>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorOut)));
                               var colorOut = container.RawIOObject as IColorOut;
                               colorOut.GetColorPointer(out ppDoubleData);
                               return IOContainer.Create(context, new ColorOutStream((RGBAColor**) ppDoubleData, GetSetColorLengthAction(colorOut)), container);
                           });
            
            RegisterOutput(typeof(IOutStream<Color4>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorOut)));
                               var colorOut = container.RawIOObject as IColorOut;
                               colorOut.GetColorPointer(out ppDoubleData);
                               return IOContainer.Create(context, new SlimDXColorOutStream((RGBAColor**) ppDoubleData, GetSetColorLengthAction(colorOut)), container);
                           });

            RegisterOutput(typeof(IOutStream<EnumEntry>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumOut)));
                               var enumOut = container.RawIOObject as IEnumOut;
                               if (context.IOAttribute.AutoFlush)
                                   return IOContainer.Create(context, new DynamicEnumOutStream(enumOut), container, null, s => s.Flush());
                               else
                                   return IOContainer.Create(context, new DynamicEnumOutStream(enumOut), container);
                           });

            RegisterOutput<IOutStream<System.IO.Stream>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IRawOut)));
                    var rawOut = container.RawIOObject as IRawOut;
                    if (context.IOAttribute.AutoFlush)
                        return IOContainer.Create(context, new RawOutStream(rawOut), container, null, s => s.Flush());
                    else
                        return IOContainer.Create(context, new RawOutStream(rawOut), container);
                });

            RegisterOutput<MemoryIOStream<string>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IStringOut)));
                    var stringOut = container.RawIOObject as IStringOut;
                    if (context.IOAttribute.AutoFlush)
                        return IOContainer.Create(context, new StringOutStream(stringOut), container, null, s => s.Flush());
                    else
                        return IOContainer.Create(context, new StringOutStream(stringOut), container);
                });

            RegisterOutput<MemoryIOStream<char>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IStringOut)));
                    var stringOut = container.RawIOObject as IStringOut;
                    if (context.IOAttribute.AutoFlush)
                        return IOContainer.Create(context, new CharOutStream(stringOut), container, null, s => s.Flush());
                    else
                        return IOContainer.Create(context, new CharOutStream(stringOut), container);
                });

            RegisterOutput(typeof(IOutStream<>), (factory, context) => {
                               var host = factory.PluginHost;
                               var t = context.DataType;
                               var attribute = context.IOAttribute;
                               if (t.IsGenericType)
                               {
                                   var genericArguments = t.GetGenericArguments();
                                   Type streamType = null;
                                   switch (genericArguments.Length) {
                                       case 1:
                                           if (attribute.IsBinSizeEnabled && typeof(IInStream<>).MakeGenericType(genericArguments).IsAssignableFrom(t))
                                           {
                                               var multiDimStreamType = typeof(MultiDimOutStream<>).MakeGenericType(t.GetGenericArguments().First());
                                               var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IOutStream;
                                               if (context.IOAttribute.AutoFlush)
                                                   return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
                                               else
                                                   return GenericIOContainer.Create(context, factory, stream);
                                           }
                                           try
                                           {
                                               if (typeof(TextureResource<>).MakeGenericType(genericArguments).IsAssignableFrom(t))
                                               {
                                                    var metadataType = genericArguments[0];
                                                    streamType = typeof(TextureOutStream<,>);
                                                    streamType = streamType.MakeGenericType(t, metadataType);
                                               }
                                           }
                                           catch (ArgumentException)
                                           {
                                               // Type constraints weren't satisfied.
                                               streamType = null;
                                           }
                                           try
                                           {
                                               if (typeof(MeshResource<>).MakeGenericType(genericArguments).IsAssignableFrom(t))
                                               {
                                                   var metadataType = genericArguments[0];
                                                   streamType = typeof(MeshOutStream<,>);
                                                   streamType = streamType.MakeGenericType(t, metadataType);
                                               }
                                           }
                                           catch (ArgumentException)
                                           {
                                               // Type constraints weren't satisfied.
                                               streamType = null;
                                           }
                                           if (streamType != null)
                                           {
                                               var stream = Activator.CreateInstance(streamType, host, attribute) as IOutStream;
                                               if (context.IOAttribute.AutoFlush)
                                                   return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
                                               else
                                                   return GenericIOContainer.Create(context, factory, stream);
                                           }
                                           break;
                                       case 2:
                                           try 
                                           {
                                               if (typeof(DXResource<,>).MakeGenericType(genericArguments).IsAssignableFrom(t))
                                               {
                                                   var resourceType = genericArguments[0];
                                                   var metadataType = genericArguments[1];
                                                   if (resourceType == typeof(Texture))
                                                   {
                                                       streamType = typeof(TextureOutStream<,>);
                                                       streamType = streamType.MakeGenericType(t, metadataType);
                                                   }
                                                   else if (resourceType == typeof(Mesh))
                                                   {
                                                       streamType = typeof(MeshOutStream<,>);
                                                       streamType = streamType.MakeGenericType(t, metadataType);
                                                   }
                                                   if (streamType != null)
                                                   {
                                                       var stream = Activator.CreateInstance(streamType, host, attribute) as IOutStream;
                                                       if (context.IOAttribute.AutoFlush)
                                                           return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
                                                       else
                                                           return GenericIOContainer.Create(context, factory, stream);
                                                   }
                                                   else
                                                   {
                                                       throw new NotImplementedException();
                                                   }
                                               }
                                           }
                                           catch (Exception)
                                           {
                                               // Type constraints weren't satisfied.
                                           }
                                           break;
                                   }
                               }
                               
                               {
                                   IOutStream stream;
                                   IIOContainer container;
                                   if (context.DataType == typeof(MouseState))
                                   {
                                       context = context.ReplaceDataType(typeof(Mouse));
                                       container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeOut)));
                                       stream = Activator.CreateInstance(typeof(MouseStateToMouseOutStream), container.RawIOObject) as IOutStream;
                                   }
                                   else if (context.DataType == typeof(KeyboardState))
                                   {
                                       context = context.ReplaceDataType(typeof(Keyboard));
                                       container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeOut)));
                                       stream = Activator.CreateInstance(typeof(KeyboardStateToKeyboardOutStream), container.RawIOObject) as IOutStream;
                                   }
                                   else
                                   {
                                       container = factory.CreateIOContainer(context.ReplaceIOType(typeof(INodeOut)));
                                       stream = Activator.CreateInstance(typeof(NodeOutStream<>).MakeGenericType(context.DataType), container.RawIOObject) as IOutStream;
                                   }
                                   if (context.IOAttribute.AutoFlush)
                                       return IOContainer.Create(context, stream, container, null, s => s.Flush());
                                   else
                                       return IOContainer.Create(context, stream, container);
                               }
                           });
            
            RegisterOutput(typeof(IInStream<>), (factory, context) => {
                               var t = context.DataType;
                               var attribute = context.IOAttribute;
                               if (t.IsGenericType)
                               {
                                   if (typeof(IOutStream<>).MakeGenericType(t.GetGenericArguments()).IsAssignableFrom(t))
                                   {
                                       var multiDimStreamType = typeof(GroupOutStream<>).MakeGenericType(t.GetGenericArguments().First());
                                       if (!attribute.IsPinGroup)
                                       {
                                           throw new NotSupportedException("IInStream<IOutStream<T>> can only be used as a pin group.");
                                       }
                                       
                                       var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IFlushable;
                                       if (context.IOAttribute.AutoFlush)
                                           return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
                                       else
                                           return GenericIOContainer.Create(context, factory, stream);
                                   }
                               }
                               
                               return null; // IOFactory will throw a NotSupportedException with a few more details.
                           });
            
            RegisterOutput(typeof(IIOStream<>), (factory, context) => {
                               var outStreamType = typeof(IOutStream<>).MakeGenericType(context.DataType);
                               var ioStreamType = typeof(BufferedOutputIOStream<>).MakeGenericType(context.DataType);
                               var container = factory.CreateIOContainer(outStreamType, context.IOAttribute, false);
                               var ioStream = (IIOStream) Activator.CreateInstance(ioStreamType, container.RawIOObject);
                               if (context.IOAttribute.AutoFlush)
                                   return IOContainer.Create(context, ioStream, container, null, s => s.Flush());
                               else
                                   return IOContainer.Create(context, ioStream, container);
                           },
                           false);

            RegisterConfig(typeof(MemoryIOStream<Vector2>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                var valueConfig = container.RawIOObject as IValueConfig;
                var stream = new Vector2ConfigStream(valueConfig);
                return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
            });
            RegisterConfig(typeof(MemoryIOStream<Vector3>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                var valueConfig = container.RawIOObject as IValueConfig;
                var stream = new Vector3ConfigStream(valueConfig);
                return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
            });
            RegisterConfig(typeof(MemoryIOStream<Vector4>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                var valueConfig = container.RawIOObject as IValueConfig;
                var stream = new Vector4ConfigStream(valueConfig);
                return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
            });



            RegisterConfig(typeof(MemoryIOStream<Quaternion>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                var valueConfig = container.RawIOObject as IValueConfig;
                var stream = new QuaternionConfigStream(valueConfig);
                return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
            });



            RegisterConfig(typeof(MemoryIOStream<Vector2D>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                var valueConfig = container.RawIOObject as IValueConfig;
                var stream = new Vector2DConfigStream(valueConfig);
                return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
            });
            RegisterConfig(typeof(MemoryIOStream<Vector3D>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                var valueConfig = container.RawIOObject as IValueConfig;
                var stream = new Vector3DConfigStream(valueConfig);
                return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
            });
            RegisterConfig(typeof(MemoryIOStream<Vector4D>), (factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                var valueConfig = container.RawIOObject as IValueConfig;
                var stream = new Vector4DConfigStream(valueConfig);
                return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
            });
            
            RegisterConfig(typeof(MemoryIOStream<string>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IStringConfig)));
                               var stringConfig = container.RawIOObject as IStringConfig;
                               var stream = new StringConfigStream(stringConfig);
                               return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
                           });
            
            RegisterConfig(typeof(MemoryIOStream<RGBAColor>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorConfig)));
                               var colorConfig = container.RawIOObject as IColorConfig;
                               var stream = new ColorConfigStream(colorConfig);
                               return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
                           });
            
            RegisterConfig(typeof(MemoryIOStream<Color4>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorConfig)));
                               var colorConfig = container.RawIOObject as IColorConfig;
                               var stream = new SlimDXColorConfigStream(colorConfig);
                               return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
                           });

            RegisterConfig(typeof(MemoryIOStream<EnumEntry>), (factory, context) => {
                               var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumConfig)));
                               var enumConfig = container.RawIOObject as IEnumConfig;
                               var stream = new DynamicEnumConfigStream(enumConfig,context.IOAttribute.EnumName);
                               return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
                           });        
            RegisterConfig(typeof(MemoryIOStream<>), (factory, context) => {
                               var t = context.DataType;
                               if (t.IsPrimitive)
                               {
                                   var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueConfig)));
                                   var valueConfig = container.RawIOObject as IValueConfig;
                                   var streamType = typeof(ValueConfigStream<>).MakeGenericType(context.DataType);
                                   var stream = Activator.CreateInstance(streamType, new object[] { valueConfig }) as IIOStream;
                                   return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
                               }
                               throw new NotSupportedException(string.Format("Config pin of type '{0}' is not supported.", t));
                           });
        }

        private IIOContainer GetValueContainer(IIOFactory factory, IOBuildContext<InputAttribute> context, out int* pLength, out double** ppDoubleData, out Func<bool> validateFunc)
        {
            IIOContainer container;
            if (context.IOAttribute.CheckIfChanged)
            {
                container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueIn)));
                var valueIn = container.RawIOObject as IValueIn;
                valueIn.GetValuePointer(out pLength, out ppDoubleData);
                validateFunc = GetValidateFunc(valueIn, context.IOAttribute.AutoValidate);
            }
            else
            {
                container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IValueFastIn)));
                var valueFastIn = container.RawIOObject as IValueFastIn;
                valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
                validateFunc = GetValidateFunc(valueFastIn, context.IOAttribute.AutoValidate);
            }
            return container;
        }
        
        static private Func<bool> GetValidateFunc(IPluginIn pluginIn, bool autoValidate)
        {
            if (autoValidate)
            {
                return () => { return pluginIn.PinIsChanged; };
            }
            return () => { return pluginIn.Validate(); };
        }
        
        static private Func<bool> GetValidateFunc(IPluginFastIn pluginFastIn, bool autoValidate)
        {
            if (autoValidate)
            {
                return () => { return true; };
            }
            // Fast value pins always return false for PinIsChanged, we therefor need to return true here manually.
            return () => { pluginFastIn.Validate(); return true; };
        }
        
        static private Action<int> GetSetValueLengthAction(IValueOut valueOut)
        {
            return (newLength) =>
            {
                valueOut.SliceCount = newLength;
            };
        }
        
        static private Action<int> GetSetColorLengthAction(IColorOut colorOut)
        {
            return (newLength) =>
            {
                colorOut.SliceCount = newLength;
            };
        }
        
        static private Action<int> GetSetMatrixLengthAction(ITransformOut transformOut)
        {
            return (newLength) =>
            {
                transformOut.SliceCount = newLength;
            };
        }
        
        static private Func<bool> GetValidateFunc(IValueConfig valueConfig)
        {
            return () => { return valueConfig.PinIsChanged; };
        }
        
        static private Func<bool> GetValidateFunc(IColorConfig colorConfig)
        {
            return () => { return colorConfig.PinIsChanged; };
        }
        
        static private Action<int> GetSetValueLengthAction(IValueConfig valueConfig)
        {
            return (int newLength) => {
                valueConfig.SliceCount = newLength;
            };
        }
        
        static private Action<int> GetSetColorLengthAction(IColorConfig colorConfig)
        {
            return (int newLength) => {
                colorConfig.SliceCount = newLength;
            };
        }
    }
}
