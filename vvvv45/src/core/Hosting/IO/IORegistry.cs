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
			              	valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              	var stream = new Vector2DInStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector3D>),(factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              	var stream = new Vector3DInStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector4D>),(factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              	var stream = new Vector4DInStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });

			RegisterInput(typeof(IInStream<Vector2>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              	var stream = new Vector2InStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector3>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              	var stream = new Vector3InStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
			              	return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector4>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	valueFastIn.GetValuePointer(out pLength, out ppDoubleData);
			              	var stream = new Vector4InStream(pLength, ppDoubleData, GetValidateFunc(valueFastIn));
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
			              	if (t.IsGenericType && t.GetGenericArguments().Length == 1)
			              	{
			              		if (typeof(IInStream<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
			              		{
			              			var multiDimStreamType = typeof(MultiDimInStream<>).MakeGenericType(t.GetGenericArguments().First());
			              			if (attribute.IsPinGroup)
			              			{
			              				multiDimStreamType = typeof(GroupInStream<>).MakeGenericType(t.GetGenericArguments().First());
			              			}
			              			
			              			var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IInStream;
			              			
			              			// PinGroup implementation doesn't need to get synced on managed side.
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
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(spread, ioHandler.Metadata, p => p.Sync());
			              	else
			              		return IOHandler.Create(spread, ioHandler.Metadata);
			              });
			
			RegisterInput(typeof(IDiffSpread<>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
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
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new DoubleOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });
			
			RegisterOutput(typeof(IOutStream<float>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new FloatOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });
			
			RegisterOutput(typeof(IOutStream<int>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new IntOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });
			
			RegisterOutput(typeof(IOutStream<bool>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new BoolOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });

			RegisterOutput(typeof(IOutStream<Matrix4x4>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var transformOut = host.CreateTransformOutput(attribute, t);
			               	transformOut.GetMatrixPointer(out ppFloatData);
			               	return IOHandler.Create(new Matrix4x4OutStream((Matrix**) ppFloatData, GetSetMatrixLengthAction(transformOut)), transformOut);
			               });
			
			RegisterOutput(typeof(IOutStream<Matrix>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var transformOut = host.CreateTransformOutput(attribute, t);
			               	transformOut.GetMatrixPointer(out ppFloatData);
			               	return IOHandler.Create(new MatrixOutStream((Matrix**) ppFloatData, GetSetMatrixLengthAction(transformOut)), transformOut);
			               });

			RegisterOutput(typeof(IOutStream<Vector2D>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new Vector2DOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector3D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new Vector3DOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector4D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new Vector4DOutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });

			RegisterOutput(typeof(IOutStream<Vector2>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new Vector2OutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector3>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new Vector3OutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });
			RegisterOutput(typeof(IOutStream<Vector4>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	valueOut.GetValuePointer(out ppDoubleData);
			               	return IOHandler.Create(new Vector4OutStream(ppDoubleData, GetSetValueLengthAction(valueOut)), valueOut);
			               });

			RegisterOutput(typeof(IOutStream<string>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var stringOut = host.CreateStringOutput(attribute, t);
			               	return IOHandler.Create(new StringOutStream(stringOut), stringOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<RGBAColor>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var colorOut = host.CreateColorOutput(attribute, t);
			               	colorOut.GetColorPointer(out ppDoubleData);
			               	return IOHandler.Create(new ColorOutStream((RGBAColor**) ppDoubleData, GetSetColorLengthAction(colorOut)), colorOut);
			               });

			RegisterOutput(typeof(IOutStream<EnumEntry>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var enumOut = host.CreateEnumOutput(attribute, t);
			               	return IOHandler.Create(new DynamicEnumOutStream(enumOut), enumOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	if (t.IsGenericType && t.GetGenericArguments().Length == 1)
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
			               			return IOHandler.Create(stream, null, null, p => p.Flush());
			               		}
			               	}
			               	var ioBuilder = CreateIOHandler(typeof(IOutStream<>), typeof(IOutStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(OutputPin<>).MakeGenericType(t);
			               	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as ISpread;
			               	return IOHandler.Create(pin, ioBuilder.Metadata, null, p => p.Flush());
			               });
			
			RegisterConfig(typeof(IIOStream<string>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var stringConfig = host.CreateStringConfig(attribute, t);
			               	return IOHandler.Create(new StringConfigStream(stringConfig), stringConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<RGBAColor>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var colorConfig = host.CreateColorConfig(attribute, t);
			               	var stream = new ColorConfigStream(colorConfig);
			               	return IOHandler.Create(stream, colorConfig, null, null, s => s.Sync());
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
			               		var streamType = typeof(EnumConfigStream<>).MakeGenericType(t);
			               		var stream = Activator.CreateInstance(streamType, new object[] { enumConfig }) as IIOStream;
			               		return IOHandler.Create(stream, enumConfig, null, null, s => s.Sync());
			               	}
			               	else if (t.IsPrimitive)
			               	{
			               	    var valueConfig = host.CreateValueConfig(attribute, t);
			               	    var streamType = typeof(ValueConfigStream<>).MakeGenericType(t);
			               	    var stream = Activator.CreateInstance(streamType, new object[] { valueConfig }) as IIOStream;
			               	    return IOHandler.Create(stream, valueConfig, null, null, s => s.Sync());
			               	}
			               	throw new NotSupportedException(string.Format("Config pin of type '{0}' is not supported.", t));
			               });
			
			RegisterConfig(typeof(ISpread<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var ioBuilder = CreateIOHandler(typeof(IIOStream<>), typeof(IIOStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(ConfigPin<>).MakeGenericType(t);
			               	var spread = (ISpread) Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject);
			               	return IOHandler.Create(spread, ioBuilder.Metadata, null, null, p => p.Sync());
			               });
			
			RegisterConfig(typeof(IDiffSpread<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var ioBuilder = CreateIOHandler(typeof(IIOStream<>), typeof(IIOStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(ConfigPin<>).MakeGenericType(t);
			               	var spread = (IDiffSpread) Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject);
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
		
		static private Func<bool> GetValidateFunc(IPluginIn pluginIn)
		{
			return () => { return pluginIn.Validate(); };
		}
		
		static private Func<bool> GetValidateFunc(IPluginFastIn pluginFastIn)
		{
			return () => { return pluginFastIn.Validate(); };
		}
		
		static unsafe private Action<int> GetSetValueLengthAction(IValueOut valueOut)
		{
			return (newLength) =>
			{
				valueOut.SliceCount = newLength;
			};
		}
		
		static unsafe private Action<int> GetSetColorLengthAction(IColorOut colorOut)
		{
			return (newLength) =>
			{
				colorOut.SliceCount = newLength;
			};
		}
		
		static unsafe private Action<int> GetSetMatrixLengthAction(ITransformOut transformOut)
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
		
		static unsafe private Action<int> GetSetValueLengthAction(IValueConfig valueConfig)
		{
			return (int newLength) => {
				valueConfig.SliceCount = newLength;
			};
		}
		
		static unsafe private Action<int> GetSetColorLengthAction(IColorConfig colorConfig)
		{
			return (int newLength) => {
				colorConfig.SliceCount = newLength;
			};
		}
	}
}
