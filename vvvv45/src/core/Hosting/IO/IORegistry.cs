using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO.Streams;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO
{
	[ComVisible(false)]
	unsafe class IORegistry : IIORegistry
	{
		private readonly Dictionary<Type, Func<IIOFactory, InputAttribute, Type, IOHandler>> FInputDelegates = new Dictionary<Type, Func<IIOFactory, InputAttribute, Type, IOHandler>>();
		private readonly Dictionary<Type, Func<IIOFactory, OutputAttribute, Type, IOHandler>> FOutputDelegates = new Dictionary<Type, Func<IIOFactory, OutputAttribute, Type, IOHandler>>();
		private readonly Dictionary<Type, Func<IIOFactory, ConfigAttribute, Type, IOHandler>> FConfigDelegates = new Dictionary<Type, Func<IIOFactory, ConfigAttribute, Type, IOHandler>>();
		
		public IORegistry()
		{
			int* pLength;
			double** ppDoubleData;
			float** ppFloatData;
			
			RegisterInput(typeof(IInStream<double>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	if (attribute.CheckIfChanged)
			              	{
			              		var valueIn = host.CreateValueInput(attribute, t);
			              		valueIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new DoubleInStream(pLength, ppDoubleData, GetValidateFunc(valueIn));
			              		return IOHandler.Create(stream, valueIn);
			              	}
			              	else
			              	{
			              		var valueFastIn = host.CreateValueFastInput(attribute, t);
			              		valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new DoubleInStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              		return IOHandler.Create(stream, valueFastIn);
			              	}
			              });
			
			RegisterInput(typeof(IInStream<float>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	if (attribute.CheckIfChanged)
			              	{
			              		var valueIn = host.CreateValueInput(attribute, t);
			              		valueIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new FloatInStream(pLength, ppDoubleData, GetValidateFunc(valueIn));
			              		return IOHandler.Create(stream, valueIn);
			              	}
			              	else
			              	{
			              		var valueFastIn = host.CreateValueFastInput(attribute, t);
			              		valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new FloatInStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              		return IOHandler.Create(stream, valueFastIn);
			              	}
			              });
			
			RegisterInput(typeof(IInStream<int>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	if (attribute.CheckIfChanged)
			              	{
			              		var valueIn = host.CreateValueInput(attribute, t);
			              		valueIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new IntInStream(pLength, ppDoubleData, GetValidateFunc(valueIn));
			              		return IOHandler.Create(stream, valueIn);
			              	}
			              	else
			              	{
			              		var valueFastIn = host.CreateValueFastInput(attribute, t);
			              		valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new IntInStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              		return IOHandler.Create(stream, valueFastIn);
			              	}
			              });
			
			RegisterInput(typeof(IInStream<bool>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	if (attribute.CheckIfChanged)
			              	{
			              		var valueIn = host.CreateValueInput(attribute, t);
			              		valueIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new BoolInStream(pLength, ppDoubleData, GetValidateFunc(valueIn));
			              		return IOHandler.Create(stream, valueIn);
			              	}
			              	else
			              	{
			              		var valueFastIn = host.CreateValueFastInput(attribute, t);
			              		valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              		var stream = new BoolInStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              		return IOHandler.Create(stream, valueFastIn);
			              	}
			              });

			RegisterInput(typeof(IInStream<Matrix4x4>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var transformIn = host.CreateTransformInput(attribute, t);
			              	transformIn.GetMatrixPointer(out pLength, out ppFloatData);
			              	var stream = new Matrix4x4InStream(pLength, (Matrix**) ppFloatData, GetValidateFunc(transformIn));
			              	return IOHandler.Create(stream, transformIn);
			              });
			
			RegisterInput(typeof(IInStream<Matrix>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var transformIn = host.CreateTransformInput(attribute, t);
			              	transformIn.GetMatrixPointer(out pLength, out ppFloatData);
			              	var stream = new MatrixInStream(pLength, (Matrix**) ppFloatData, GetValidateFunc(transformIn));
			              	return IOHandler.Create(stream, transformIn);
			              });

			RegisterInput(typeof(IInStream<Vector2D>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector2DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector3D>),(factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector3DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector4D>),(factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector4DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });

			RegisterInput(typeof(IInStream<Vector2>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector2InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector3>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector3InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector4>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector4InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });

			RegisterInput(typeof(IInStream<string>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var stringIn = host.CreateStringInput(attribute, t);
			              	var stream = new StringInStream(stringIn);
			              	// Using ManagedIOStream -> needs to be synced on managed side.
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, stringIn, s => s.Sync());
			              	else
			              		return IOHandler.Create(stream, stringIn);
			              });
			
			RegisterInput(typeof(IInStream<RGBAColor>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var colorIn = host.CreateColorInput(attribute, t);
			              	colorIn.GetColorPointer(out pLength, out ppDoubleData);
			              	var stream = new ColorInStream(pLength, (RGBAColor**) ppDoubleData, GetValidateFunc(colorIn));
			              	return IOHandler.Create(stream, colorIn);
			              });

			RegisterInput(typeof(IInStream<EnumEntry>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var enumIn = host.CreateEnumInput(attribute, t);
			              	var stream = new DynamicEnumInStream(enumIn);
			              	return IOHandler.Create(stream, enumIn);
			              });
			
			RegisterInput(typeof(IIOStream<>), (factory, attribute, t) => {
			              	var inStreamType = typeof(IInStream<>).MakeGenericType(t);
			              	var ioStreamType = typeof(InputIOStream<>).MakeGenericType(t);
			              	var inStream = factory.CreateIO(inStreamType, attribute);
			              	var ioStream = (IIOStream) Activator.CreateInstance(ioStreamType, inStream);
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(ioStream, inStream, s => s.Sync(), s => s.Flush());
			              	else
			              		return IOHandler.Create(ioStream, inStream, null, s => s.Flush());
			              });
			
			RegisterInput(typeof(IInStream<>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	if (t.IsGenericType)
			              	{
			              		if (typeof(IInStream<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
			              		{
			              			var multiDimStreamType = typeof(MultiDimInStream<>).MakeGenericType(t.GetGenericArguments().First());
			              			if (attribute.IsPinGroup)
			              			{
			              				multiDimStreamType = typeof(GroupInStream<>).MakeGenericType(t.GetGenericArguments().First());
			              			}
			              			
			              			var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IInStream;
			              			
			              			// PinGroup impementation doesn't need to get synced on managed side.
			              			if (!attribute.IsPinGroup && attribute.AutoValidate)
			              				return IOHandler.Create(stream, null, s => s.Sync());
			              			else
			              				return IOHandler.Create(stream, null);
			              		}
			              	}
			              	
			              	if (t.BaseType == typeof(Enum))
			              	{
			              		var enumIn = host.CreateEnumInput(attribute, t);
			              		var stream = Activator.CreateInstance(typeof(EnumInStream<>).MakeGenericType(t), new object[] { enumIn }) as IInStream;
			              		return IOHandler.Create(stream, enumIn);
			              	}
			              	else
			              	{
			              		var nodeIn = host.CreateNodeInput(attribute, t);
			              		var stream = Activator.CreateInstance(typeof(NodeInStream<>).MakeGenericType(t), new object[] { nodeIn }) as IInStream;
			              		return IOHandler.Create(stream, nodeIn);
			              	}
			              });
			
			RegisterInput(typeof(ISpread<>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	ISpread spread = null;
			              	if (t.IsGenericType)
			              	{
			              		if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
			              		{
			              			var spreadType = typeof(InputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
			              			
			              			if (attribute.IsPinGroup)
			              			{
			              				spreadType = typeof(InputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
			              			}
			              			
			              			spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
			              			spread.Sync();
			              			if (attribute.AutoValidate)
			              				return IOHandler.Create(spread, null, p => p.Sync());
			              			else
			              				return IOHandler.Create(spread, null);
			              		}
			              	}
			              	// Disable auto validation for stream as spread will do it.
			              	var streamAttribute = attribute.Clone() as InputAttribute;
			              	streamAttribute.AutoValidate = false;
			              	var ioHandler = CreateIOHandler(typeof(IInStream<>), typeof(IInStream<>).MakeGenericType(t), factory, streamAttribute);
			              	var pinType = typeof(InputPin<>).MakeGenericType(t);
			              	spread = Activator.CreateInstance(pinType, host, ioHandler.Metadata, ioHandler.RawIOObject) as ISpread;
			              	spread.Sync();
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(spread, ioHandler.Metadata, p => p.Sync());
			              	else
			              		return IOHandler.Create(spread, ioHandler.Metadata);
			              });
			
			RegisterInput(typeof(IDiffSpread<>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	attribute.CheckIfChanged = true;
			              	ISpread spread = null;
			              	
			              	if (t.IsGenericType)
			              	{
			              		if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
			              		{
			              			var spreadType = typeof(DiffInputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
			              			
			              			if (attribute.IsPinGroup)
			              			{
			              				spreadType = typeof(DiffInputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
			              			}
			              			
			              			spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
			              			spread.Sync();
			              			if (attribute.AutoValidate)
			              				return IOHandler.Create(spread, null, p => p.Sync());
			              			else
			              				return IOHandler.Create(spread, null);
			              		}
			              	}
			              	// Disable auto validation for stream as spread will do it.
			              	var streamAttribute = attribute.Clone() as InputAttribute;
			              	streamAttribute.AutoValidate = false;
			              	var ioBuilder = CreateIOHandler(typeof(IInStream<>), typeof(IInStream<>).MakeGenericType(t), factory, streamAttribute);
			              	var pinType = typeof(DiffInputPin<>).MakeGenericType(t);
			              	spread = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as ISpread;
			              	spread.Sync();
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(spread, ioBuilder.Metadata, p => p.Sync());
			              	else
			              		return IOHandler.Create(spread, ioBuilder.Metadata);
			              });
			
			RegisterInput(typeof(IDXRenderStateIn), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	IDXRenderStateIn pin;
			              	host.CreateRenderStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			              	return IOHandler.Create(pin, pin);
			              });
			
			RegisterInput(typeof(IDXSamplerStateIn), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	IDXSamplerStateIn pin;
			              	host.CreateSamplerStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			              	return IOHandler.Create(pin, pin);
			              });
			
			RegisterOutput(typeof(IOutStream<double>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new DoubleOutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });
			
			RegisterOutput(typeof(IOutStream<float>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new FloatOutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });
			
			RegisterOutput(typeof(IOutStream<int>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new IntOutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });
			
			RegisterOutput(typeof(IOutStream<bool>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new BoolOutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });

			RegisterOutput(typeof(IOutStream<Matrix4x4>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var transformOut = host.CreateTransformOutput(attribute, t);
			               	return IOHandler.Create(new Matrix4x4OutStream(GetResizeMatrixArrayFunc(transformOut)), transformOut);
			               });
			
			RegisterOutput(typeof(IOutStream<Matrix>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var transformOut = host.CreateTransformOutput(attribute, t);
			               	return IOHandler.Create(new MatrixOutStream(GetResizeMatrixArrayFunc(transformOut)), transformOut);
			               });

			RegisterOutput(typeof(IOutStream<Vector2D>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector2DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector3D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector3DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector4D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector4DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });

			RegisterOutput(typeof(IOutStream<Vector2>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector2OutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector3>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector3OutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector4>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector4OutStream(GetResizeValueArrayFunc(valueOut)), valueOut);
			               });

			RegisterOutput(typeof(IOutStream<string>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var stringOut = host.CreateStringOutput(attribute, t);
			               	return IOHandler.Create(new StringOutStream(stringOut), stringOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<RGBAColor>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var colorOut = host.CreateColorOutput(attribute, t);
			               	return IOHandler.Create(new ColorOutStream(GetResizeColorArrayFunc(colorOut)), colorOut);
			               });

			RegisterOutput(typeof(IOutStream<EnumEntry>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var enumOut = host.CreateEnumOutput(attribute, t);
			               	return IOHandler.Create(new DynamicEnumOutStream(enumOut), enumOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	if (t.IsGenericType)
			               	{
			               		if (typeof(IInStream<>).MakeGenericType(t.GetGenericArguments()).IsAssignableFrom(t))
			               		{
			               			var multiDimStreamType = typeof(MultiDimOutStream<>).MakeGenericType(t.GetGenericArguments().First());
			               			var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IOutStream;
			               			return IOHandler.Create(stream, null, null, s => s.Flush());
			               		}
			               	}
			               	
			               	if (t.BaseType == typeof(Enum))
			               	{
			               		var enumOut = host.CreateEnumOutput(attribute, t);
			               		var stream = Activator.CreateInstance(typeof(EnumOutStream<>).MakeGenericType(t), new object[] { enumOut }) as IOutStream;
			               		return IOHandler.Create(stream, enumOut, null, s => s.Flush());
			               	}
			               	else
			               	{
			               		var nodeOut = host.CreateNodeOutput(attribute, t);
			               		var stream = Activator.CreateInstance(typeof(NodeOutStream<>).MakeGenericType(t), new object[] { nodeOut }) as IOutStream;
			               		return IOHandler.Create(stream, nodeOut, null, s => s.Flush());
			               	}
			               });
			
			RegisterOutput(typeof(IInStream<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	if (t.IsGenericType)
			               	{
			               		if (typeof(IOutStream<>).MakeGenericType(t.GetGenericArguments()).IsAssignableFrom(t))
			               		{
			               			var multiDimStreamType = typeof(GroupOutStream<>).MakeGenericType(t.GetGenericArguments().First());
			               			if (!attribute.IsPinGroup)
			               			{
			               				throw new NotSupportedException("IInStream<IOutStream<T>> can only be used as a pin group.");
			               			}
			               			
			               			var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IInStream;
			               			return IOHandler.Create(stream, null);
			               		}
			               	}
			               	
			               	return null; // IOFactory will throw a NotSupportedException with a few more details.
			               });
			
			RegisterOutput(typeof(IDXLayerIO), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	IDXLayerIO pin;
			               	host.CreateLayerOutput(attribute.Name, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterOutput(typeof(IDXMeshOut), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	IDXMeshOut pin;
			               	host.CreateMeshOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterOutput(typeof(IDXTextureOut), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	IDXTextureOut pin;
			               	host.CreateTextureOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterOutput(typeof(ISpread<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	if (t.IsGenericType)
			               	{
			               		if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
			               		{
			               			var spreadType = typeof(OutputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
			               			
			               			if (attribute.IsPinGroup)
			               			{
			               				spreadType = typeof(OutputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
			               			}
			               			
			               			var stream = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
			               			return IOHandler.Create(stream, null, null, p => p.Flush());
			               		}
			               	}
			               	var ioBuilder = CreateIOHandler(typeof(IOutStream<>), typeof(IOutStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(OutputPin<>).MakeGenericType(t);
			               	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as ISpread;
			               	return IOHandler.Create(pin, ioBuilder.Metadata, null, p => p.Flush());
			               });
			
			RegisterConfig(typeof(IIOStream<double>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	valueConfig.GetValuePointer(out pLength, out ppDoubleData);
			               	var inStream = new DoubleInStream(pLength, ppDoubleData, GetValidateFunc(valueConfig));
			               	var outStream = new DoubleOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<double>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<float>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	valueConfig.GetValuePointer(out pLength, out ppDoubleData);
			               	var inStream = new FloatInStream(pLength, ppDoubleData, GetValidateFunc(valueConfig));
			               	var outStream = new FloatOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<float>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<int>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	valueConfig.GetValuePointer(out pLength, out ppDoubleData);
			               	var inStream = new IntInStream(pLength, ppDoubleData, GetValidateFunc(valueConfig));
			               	var outStream = new IntOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<int>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<bool>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	valueConfig.GetValuePointer(out pLength, out ppDoubleData);
			               	var inStream = new BoolInStream(pLength, ppDoubleData, GetValidateFunc(valueConfig));
			               	var outStream = new BoolOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<bool>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });

			RegisterConfig(typeof(IIOStream<Vector2D>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new Vector2DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new Vector2DOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<Vector2D>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			RegisterConfig(typeof(IIOStream<Vector3D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new Vector3DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new Vector3DOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<Vector3D>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			RegisterConfig(typeof(IIOStream<Vector4D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new Vector4DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new Vector4DOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<Vector4D>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });

			RegisterConfig(typeof(IIOStream<Vector2>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new Vector2InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new Vector2OutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<Vector2>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			RegisterConfig(typeof(IIOStream<Vector3>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new Vector3InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new Vector3OutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<Vector3>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			RegisterConfig(typeof(IIOStream<Vector4>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new Vector4InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new Vector4OutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<Vector4>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });

			RegisterConfig(typeof(IIOStream<string>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var stringConfig = host.CreateStringConfig(attribute, t);
			               	return IOHandler.Create(new StringConfigStream(stringConfig), stringConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<RGBAColor>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var colorConfig = host.CreateColorConfig(attribute, t);
			               	colorConfig.GetColorPointer(out pLength, out ppDoubleData);
			               	var inStream = new ColorInStream(pLength, (RGBAColor**) ppDoubleData, GetValidateFunc(colorConfig));
			               	var outStream = new ColorOutStream(ResizeColorArrayFunc(colorConfig));
			               	return IOHandler.Create(new ConfigIOStream<RGBAColor>(inStream, outStream), colorConfig, null, null, s => s.Sync());
			               });

			RegisterConfig(typeof(IIOStream<EnumEntry>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var enumConfig = host.CreateEnumConfig(attribute, t);
			               	return IOHandler.Create(new DynamicEnumConfigStream(enumConfig), enumConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	if (t.BaseType == typeof(Enum))
			               	{
			               		var enumConfig = host.CreateEnumConfig(attribute, t);
			               		var stream = Activator.CreateInstance(typeof(EnumConfigStream<>).MakeGenericType(t), new object[] { enumConfig }) as IIOStream;
			               		return IOHandler.Create(stream, enumConfig, null, null, s => s.Flush());
			               	}
			               	throw new NotSupportedException();
			               });
			
			RegisterConfig(typeof(ISpread<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var ioBuilder = CreateIOHandler(typeof(IIOStream<>), typeof(IIOStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(ConfigPin<>).MakeGenericType(t);
			               	var spread = (ISpread) Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject);
			               	spread.Sync();
			               	return IOHandler.Create(spread, ioBuilder.Metadata, null, null, p => p.Sync());
			               });
			
			RegisterConfig(typeof(IDiffSpread<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var ioBuilder = CreateIOHandler(typeof(IIOStream<>), typeof(IIOStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(ConfigPin<>).MakeGenericType(t);
			               	var spread = (IDiffSpread) Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject);
			               	spread.Sync();
			               	return IOHandler.Create(spread, ioBuilder.Metadata, null, null, p => p.Sync());
			               });
		}

		public void RegisterInput(Type ioType, Func<IIOFactory, InputAttribute, Type, IOHandler> createInputFunc)
		{
			FInputDelegates[ioType] = createInputFunc;
		}
		
		public void RegisterOutput(Type ioType, Func<IIOFactory, OutputAttribute, Type, IOHandler> createOutputFunc)
		{
			FOutputDelegates[ioType] = createOutputFunc;
		}
		
		public void RegisterConfig(Type ioType, Func<IIOFactory, ConfigAttribute, Type, IOHandler> createConfigFunc)
		{
			FConfigDelegates[ioType] = createConfigFunc;
		}
		
		public bool CanCreate(Type ioType, IOAttribute attribute)
		{
			var inputAttribute = attribute as InputAttribute;
			if (inputAttribute != null)
			{
				return FInputDelegates.ContainsKey(ioType);
			}
			
			var outputAttribute = attribute as OutputAttribute;
			if (outputAttribute != null)
			{
				return FOutputDelegates.ContainsKey(ioType);
			}
			
			var configAttribute = attribute as ConfigAttribute;
			if (configAttribute != null)
			{
				return FConfigDelegates.ContainsKey(ioType);
			}
			
			return false;
		}
		
		public IOHandler CreateIOHandler(Type openIOType, Type closedIOType, IIOFactory factory, IOAttribute attribute)
		{
			var ioDataType = closedIOType.GetGenericArguments().FirstOrDefault();
			
			var inputAttribute = attribute as InputAttribute;
			if (inputAttribute != null)
			{
				if (FInputDelegates.ContainsKey(closedIOType))
					return FInputDelegates[closedIOType](factory, inputAttribute, ioDataType);
				else if (FInputDelegates.ContainsKey(openIOType))
					return FInputDelegates[openIOType](factory, inputAttribute, ioDataType);
			}
			
			var outputAttribute = attribute as OutputAttribute;
			if (outputAttribute != null)
			{
				if (FOutputDelegates.ContainsKey(closedIOType))
					return FOutputDelegates[closedIOType](factory, outputAttribute, ioDataType);
				else if (FOutputDelegates.ContainsKey(openIOType))
					return FOutputDelegates[openIOType](factory, outputAttribute, ioDataType);
			}
			
			var configAttribute = attribute as ConfigAttribute;
			if (configAttribute != null)
			{
				if (FConfigDelegates.ContainsKey(closedIOType))
					return FConfigDelegates[closedIOType](factory, configAttribute, ioDataType);
				else if (FConfigDelegates.ContainsKey(openIOType))
					return FConfigDelegates[openIOType](factory, configAttribute, ioDataType);
			}
			
			throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", attribute, closedIOType));
		}
		
		static void SyncSpread(ISpread spread)
		{
			spread.Sync();
		}
		
		static private Func<bool> GetValidateFunc(IPluginIn pluginIn)
		{
			return () => { return pluginIn.Validate(); };
		}
		
		static private Func<bool> GetValidateFunc(IPluginFastIn pluginFastIn)
		{
			return () => { return pluginFastIn.Validate(); };
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueIn valueIn)
		{
			return () => {
				int length;
				double* ptr;
				valueIn.GetValuePointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetFastValuePointerFunc(IValueFastIn valueFastIn)
		{
			return () => {
				int length;
				double* ptr;
				valueFastIn.GetValuePointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetMatrixPointerFunc(ITransformIn transformIn)
		{
			return () => {
				int length;
				float* ptr;
				transformIn.GetMatrixPointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorIn colorIn)
		{
			return () => {
				int length;
				double* ptr;
				colorIn.GetColorPointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueOut valueFastOut)
		{
			return () => {
				double* ptr;
				valueFastOut.GetValuePointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), valueFastOut.SliceCount);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetMatrixPointerFunc(ITransformOut transformOut)
		{
			return () => {
				float* ptr;
				transformOut.GetMatrixPointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), transformOut.SliceCount);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorOut colorOut)
		{
			return () => {
				double* ptr;
				colorOut.GetColorPointer(out ptr);
				return Tuple.Create(new IntPtr(ptr), colorOut.SliceCount);
			};
		}
		
		static unsafe private Func<int, IntPtr> GetResizeValueArrayFunc(IValueOut valueOut)
		{
			return (newLength) =>
			{
				double* ptr;
				valueOut.SliceCount = newLength;
				valueOut.GetValuePointer(out ptr);
				return new IntPtr(ptr);
			};
		}
		
		static unsafe private Func<int, IntPtr> GetResizeColorArrayFunc(IColorOut colorOut)
		{
			return (newLength) =>
			{
				double* ptr;
				colorOut.SliceCount = newLength;
				colorOut.GetColorPointer(out ptr);
				return new IntPtr(ptr);
			};
		}
		
		static unsafe private Func<int, IntPtr> GetResizeMatrixArrayFunc(ITransformOut transformOut)
		{
			return (newLength) =>
			{
				float* ptr;
				transformOut.SliceCount = newLength;
				transformOut.GetMatrixPointer(out ptr);
				return new IntPtr(ptr);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetValuePointerFunc(IValueConfig valueConfig)
		{
			return () => {
				int length;
				double* ptr;
				valueConfig.GetValuePointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static private Func<bool> GetValidateFunc(IValueConfig valueConfig)
		{
			// TODO: check this
			return () => { return valueConfig.PinIsChanged; };
		}
		
		static private Func<bool> GetValidateFunc(IColorConfig colorConfig)
		{
			// TODO: check this
			return () => { return colorConfig.PinIsChanged; };
		}
		
		static unsafe private Func<int, IntPtr> ResizeValueArrayFunc(IValueConfig valueConfig)
		{
			return (int newLength) => {
				int length;
				double* ptr;
				valueConfig.SliceCount = newLength;
				valueConfig.GetValuePointer(out length, out ptr);
				return new IntPtr(ptr);
			};
		}
		
		static unsafe private Func<Tuple<IntPtr, int>> GetColorPointerFunc(IColorConfig colorConfig)
		{
			return () => {
				int length;
				double* ptr;
				colorConfig.GetColorPointer(out length, out ptr);
				return Tuple.Create(new IntPtr(ptr), length);
			};
		}
		
		static unsafe private Func<int, IntPtr> ResizeColorArrayFunc(IColorConfig colorConfig)
		{
			return (int newLength) => {
				int length;
				double* ptr;
				colorConfig.SliceCount = newLength;
				colorConfig.GetColorPointer(out length, out ptr);
				return new IntPtr(ptr);
			};
		}
	}
}
