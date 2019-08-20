using System;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using Xenko.Core.Mathematics;
using System.Collections.Generic;
using VVVV.Utils.VColor;
using VL.Lib.Basics.Resources;
using System.IO;
using Path = VL.Lib.IO.Path;
using VL.Lib.IO;

namespace VVVV.VL.Hosting.IO.Streams
{
    public unsafe class StreamRegistry : IORegistryBase
    {
        public StreamRegistry()
        {
            int* pLength;
            double** ppDoubleData;
            float** ppFloatData;
            Func<bool> validateFunc;

            PluginHostExtensions.RegisterPinAttributeConfigForType(typeof(Vector2), float.MinValue, float.MaxValue, 0.01, false, 2);
            PluginHostExtensions.RegisterPinAttributeConfigForType(typeof(Vector3), float.MinValue, float.MaxValue, 0.01, false, 3);
            PluginHostExtensions.RegisterPinAttributeConfigForType(typeof(Vector4), float.MinValue, float.MaxValue, 0.01, false, 4);
            PluginHostExtensions.RegisterPinAttributeConfigForType(typeof(Quaternion), float.MinValue, float.MaxValue, 0.01, false, 4);
            PluginHostExtensions.RegisterPinAttributeConfigForType(typeof(Color4), 0, 1, 0.01, false, 1);

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

            RegisterInput(typeof(IInStream<Color4>), (factory, context) =>
            {
                var container = GetColorContainer(factory, context, out pLength, out ppDoubleData, out validateFunc);
                var stream = new Color4InStream(pLength, (RGBAColor**)ppDoubleData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput(typeof(IInStream<Matrix>), (factory, context) =>
            {
                var container = GetMatrixContainer(factory, context, out pLength, out ppFloatData, out validateFunc);
                var stream = new MatrixInStream(pLength, (Matrix**)ppFloatData, validateFunc);
                if (context.IOAttribute.AutoValidate && context.IOAttribute.CheckIfChanged)
                    // In order to manage the IsChanged flag on the stream
                    return IOContainer.Create(context, stream, container, s => s.Sync());
                else
                    return IOContainer.Create(context, stream, container);
            });

            RegisterInput<IInStream<Path>>((factory, context) =>
            {
                var attribute = context.IOAttribute;
                var stream = new PathInStream(factory, attribute);
                if (attribute.AutoValidate)
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync(), s => s.Flush());
                else
                    return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
            });

            RegisterInput<IInStream<IMouse>>((factory, context) =>
            {
                var attribute = context.IOAttribute;
                var stream = new MouseInStream(factory, attribute);
                if (attribute.AutoValidate)
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync(), s => s.Flush());
                else
                    return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
            });
            RegisterInput<IInStream<IKeyboard>>((factory, context) =>
            {
                var attribute = context.IOAttribute;
                var stream = new KeyboardInStream(factory, attribute);
                if (attribute.AutoValidate)
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync(), s => s.Flush());
                else
                    return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
            });
            RegisterInput<IInStream<IGestureDevice>>((factory, context) =>
            {
                var attribute = context.IOAttribute;
                var stream = new GestureInStream(factory, attribute);
                if (attribute.AutoValidate)
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync(), s => s.Flush());
                else
                    return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
            });
            RegisterInput<IInStream<ITouchDevice>>((factory, context) =>
            {
                var attribute = context.IOAttribute;
                var stream = new TouchInStream(factory, attribute);
                if (attribute.AutoValidate)
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync(), s => s.Flush());
                else
                    return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
            });

            RegisterInput<IInStream<IResourceProvider<Stream>>>(
                (factory, context) =>
                {
                    var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IRawIn)));
                    var rawIn = container.RawIOObject as IRawIn;
                    var stream = new ResourceProviderInStream(rawIn);
                    // Using MemoryIOStream -> needs to be synced on managed side.
                    if (context.IOAttribute.AutoValidate)
                        return IOContainer.Create(context, stream, container, s => s.Sync());
                    else
                        return IOContainer.Create(context, stream, container);
                });

            // IInStream<IEnumerable<T>> and IInStream<Spread<T>>
            RegisterInput(typeof(IInStream<>), (factory, context) => 
            {
                var t = context.DataType;
                var attribute = context.IOAttribute;
                var genericArgument = t.GetGenericArguments()[0];
                var multiDimStreamType = (t.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? typeof(MultiDimEnumerableInStream<>) : typeof(MultiDimSpreadInStream<>))
                    .MakeGenericType(genericArgument);
                var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IInStream;
                if (attribute.AutoValidate)
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync());
                else
                    return GenericIOContainer.Create(context, factory, stream);
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
            RegisterOutput(typeof(IOutStream<Color4>), (factory, context) => {
                                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorOut)));
                                var colorOut = container.RawIOObject as IColorOut;
                                colorOut.GetColorPointer(out ppDoubleData);
                                return IOContainer.Create(context, new Color4OutStream((RGBAColor**)ppDoubleData, GetSetColorLengthAction(colorOut)), container);
                            });
            RegisterOutput(typeof(IOutStream<Matrix>), (factory, context) => {
                                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformOut)));
                                var transformOut = container.RawIOObject as ITransformOut;
                                transformOut.GetMatrixPointer(out ppFloatData);
                                return IOContainer.Create(context, new MatrixOutStream((Matrix**)ppFloatData, GetSetMatrixLengthAction(transformOut)), container);
                            });

            RegisterOutput<IOutStream<Path>>((factory, context) =>
            {
                var attribute = context.IOAttribute;
                var stream = new PathOutStream(factory, attribute);
                if (attribute.AutoFlush)
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync(), s => s.Flush());
                else
                    return GenericIOContainer.Create(context, factory, stream, s => s.Sync());
            });

            RegisterOutput<IOutStream<IResourceProvider<Stream>>>((factory, context) =>
            {
                var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IRawOut)));
                var rawOut = container.RawIOObject as IRawOut;
                var stream = new ResourceProviderOutStream(rawOut);
                return IOContainer.Create(context, stream, container, null, s => s.Flush());
            });

            // IOutStream<Spread<T>>
            RegisterOutput(typeof(IOutStream<>), (factory, context) => 
            {
                var host = factory.PluginHost;
                var t = context.DataType;
                var attribute = context.IOAttribute;
                var genericArgument = t.GetGenericArguments()[0];
                var multiDimStreamType = typeof(MultiDimOutStream<>).MakeGenericType(genericArgument);
                var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IOutStream;
                if (context.IOAttribute.AutoFlush)
                    return GenericIOContainer.Create(context, factory, stream, null, s => s.Flush());
                else
                    return GenericIOContainer.Create(context, factory, stream);
            });
        }

        public override bool CanCreate(IOBuildContext context)
        {
            // In case of multidimensional streams we're only interested in:
            // * IInStream<IEnumerable<T>>, IInStream<Spread<T>>
            // * IOutStream<Spread<T>>
            var ioType = context.IOType;
            if (ioType.IsGenericType)
            {
                var genericArgument = ioType.GetGenericArguments()[0];
                if (genericArgument.IsGenericType)
                {
                    var attribute = context.IOAttribute;
                    var openIOType = ioType.GetGenericTypeDefinition();
                    if (openIOType == typeof(IInStream<>) && attribute.IsBinSizeEnabled)
                    {
                        var genericTypeDefinition = genericArgument.GetGenericTypeDefinition();
                        return genericTypeDefinition == typeof(IEnumerable<>)
                            || genericTypeDefinition == typeof(global::VL.Lib.Collections.Spread<>)
                            || genericArgument == typeof(IResourceProvider<Stream>);
                    }
                    if (openIOType == typeof(IOutStream<>) && attribute.IsBinSizeEnabled)
                    {
                        var genericTypeDefinition = genericArgument.GetGenericTypeDefinition();
                        return genericTypeDefinition == typeof(global::VL.Lib.Collections.Spread<>)
                            || genericArgument == typeof(IResourceProvider<Stream>);
                    }
                    return false;
                }
                else
                {
                    // We do not want IInStream<> and IOutStream<> to match.
                    switch (context.Direction)
                    {
                        case PinDirection.Input:
                            return FInputDelegates.ContainsKey(ioType);
                        case PinDirection.Output:
                            return FOutputDelegates.ContainsKey(ioType);
                        default:
                            return false;
                    }
                }
            }
            return false;
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

        private IIOContainer GetColorContainer(IIOFactory factory, IOBuildContext<InputAttribute> context, out int* pLength, out double** ppDoubleData, out Func<bool> validateFunc)
        {
            IIOContainer container;

            container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IColorIn)));
            var colorIn = container.RawIOObject as IColorIn;
            colorIn.GetColorPointer(out pLength, out ppDoubleData);
            validateFunc = GetValidateFunc(colorIn, context.IOAttribute.AutoValidate);

            return container;
        }

        private IIOContainer GetMatrixContainer(IIOFactory factory, IOBuildContext<InputAttribute> context, out int* pLength, out float** ppMatrixData, out Func<bool> validateFunc)
        {
            IIOContainer container;

            container = factory.CreateIOContainer(context.ReplaceIOType(typeof(ITransformIn)));
            var matrixIn = container.RawIOObject as ITransformIn;
            matrixIn.GetMatrixPointer(out pLength, out ppMatrixData);
            validateFunc = GetValidateFunc(matrixIn, context.IOAttribute.AutoValidate);

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
    }
}
