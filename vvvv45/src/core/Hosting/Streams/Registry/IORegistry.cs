using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;

using SlimDX;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams.Registry
{
	public abstract class IOHandler
	{
		public static IOHandler<TIOObject> Create<TIOObject>(
			TIOObject iOObject,
			object metadata, 
			Action<TIOObject> beforeEvalAction, 
			Action<TIOObject> afterEvalAction
		)
		{
			return new IOHandler<TIOObject>(iOObject, metadata, beforeEvalAction, afterEvalAction);
		}
		
		public static IOHandler<TIOObject> Create<TIOObject>(
			TIOObject iOObject,
			object metadata
		)
		{
			return Create<TIOObject>(iOObject, metadata, null, null);
		}
		
		public readonly object RawIOObject;
		public readonly object Metadata;
		public readonly bool IsBeforeEvalActionEnabled;
		public readonly bool IsAfterEvalActionEnabled;
		
		protected IOHandler(object rawIOObject, object metadata, bool before, bool after)
		{
			RawIOObject = rawIOObject;
			Metadata = metadata;
			IsBeforeEvalActionEnabled = before;
			IsAfterEvalActionEnabled = after;
		}
		
		public abstract void BeforeEvaluate();
		public abstract void AfterEvaluate();
	}
	
	public class IOHandler<TIOObject> : IOHandler
	{
		public readonly TIOObject IOObject;
		private readonly Action<TIOObject> BeforeEvaluateAction;
		private readonly Action<TIOObject> AfterEvaluateAction;
		
		public IOHandler(
			TIOObject iOObject, 
			object metadata, 
			Action<TIOObject> beforeEvalAction, 
			Action<TIOObject> afterEvalAction)
			: base(iOObject, metadata, beforeEvalAction != null, afterEvalAction != null)
		{
			IOObject = iOObject;
			BeforeEvaluateAction = beforeEvalAction;
			AfterEvaluateAction = afterEvalAction;
		}
		
		public override void BeforeEvaluate()
		{
			BeforeEvaluateAction(IOObject);
		}
		
		public override void AfterEvaluate()
		{
			AfterEvaluateAction(IOObject);
		}
	}
	
	[ComVisible(false)]
	public class IORegistry
	{
		private readonly Dictionary<Type, Func<IInternalPluginHost, InputAttribute, Type, IOHandler>> FInputDelegates = new Dictionary<Type, Func<IInternalPluginHost, InputAttribute, Type, IOHandler>>();
		private readonly Dictionary<Type, Func<IInternalPluginHost, OutputAttribute, Type, IOHandler>> FOutputDelegates = new Dictionary<Type, Func<IInternalPluginHost, OutputAttribute, Type, IOHandler>>();
		private readonly Dictionary<Type, Func<IInternalPluginHost, ConfigAttribute, Type, IOHandler>> FConfigDelegates = new Dictionary<Type, Func<IInternalPluginHost, ConfigAttribute, Type, IOHandler>>();
		
		public IORegistry()
		{
			RegisterInput(typeof(IInStream<double>), (host, attribute, t) => {
			             	if (attribute.CheckIfChanged)
			             	{
			             		var valueIn = host.CreateValueInput(attribute, t);
			             		var stream = new DoubleInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueIn);
			             	}
			             	else
			             	{
			             		var valueFastIn = host.CreateValueFastInput(attribute, t);
			             		var stream = new DoubleInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueFastIn);
			             	}
			             });
			
			RegisterInput(typeof(IInStream<float>), (host, attribute, t) => {
			             	if (attribute.CheckIfChanged)
			             	{
			             		var valueIn = host.CreateValueInput(attribute, t);
			             		var stream = new FloatInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueIn);
			             	}
			             	else
			             	{
			             		var valueFastIn = host.CreateValueFastInput(attribute, t);
			             		var stream = new FloatInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueFastIn);
			             	}
			             });
			
			RegisterInput(typeof(IInStream<int>), (host, attribute, t) => {
			             	if (attribute.CheckIfChanged)
			             	{
			             		var valueIn = host.CreateValueInput(attribute, t);
			             		var stream = new IntInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueIn);
			             	}
			             	else
			             	{
			             		var valueFastIn = host.CreateValueFastInput(attribute, t);
			             		var stream = new IntInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueFastIn);
			             	}
			             });
			
			RegisterInput(typeof(IInStream<bool>), (host, attribute, t) => {
			             	if (attribute.CheckIfChanged)
			             	{
			             		var valueIn = host.CreateValueInput(attribute, t);
			             		var stream = new BoolInStream(GetValuePointerFunc(valueIn), GetValidateFunc(valueIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueIn);
			             	}
			             	else
			             	{
			             		var valueFastIn = host.CreateValueFastInput(attribute, t);
			             		var stream = new BoolInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             		if (attribute.AutoValidate)
			             			return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             		else
			             			return IOHandler.Create(stream, valueFastIn);
			             	}
			             });

			RegisterInput(typeof(IInStream<Matrix4x4>), (host, attribute, t) => {
			             	var transformIn = host.CreateTransformInput(attribute, t);
			             	var stream = new Matrix4x4InStream(GetMatrixPointerFunc(transformIn), GetValidateFunc(transformIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, transformIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, transformIn);
			             });
			
			RegisterInput(typeof(IInStream<Matrix>), (host, attribute, t) => {
			             	var transformIn = host.CreateTransformInput(attribute, t);
			             	var stream = new MatrixInStream(GetMatrixPointerFunc(transformIn), GetValidateFunc(transformIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, transformIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, transformIn);
			             });

			RegisterInput(typeof(IInStream<Vector2D>), (host, attribute, t) => {
			             	var valueFastIn = host.CreateValueFastInput(attribute, t);
			             	var stream = new Vector2DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, valueFastIn);
			             });
			RegisterInput(typeof(IInStream<Vector3D>),(host, attribute, t) => {
			             	var valueFastIn = host.CreateValueFastInput(attribute, t);
			             	var stream = new Vector3DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, valueFastIn);
			             });
			RegisterInput(typeof(IInStream<Vector4D>),(host, attribute, t) => {
			             	var valueFastIn = host.CreateValueFastInput(attribute, t);
			             	var stream = new Vector4DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, valueFastIn);
			             });

			RegisterInput(typeof(IInStream<Vector2>), (host, attribute, t) => {
			             	var valueFastIn = host.CreateValueFastInput(attribute, t);
			             	var stream = new Vector2InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, valueFastIn);
			             });
			RegisterInput(typeof(IInStream<Vector3>), (host, attribute, t) => {
			             	var valueFastIn = host.CreateValueFastInput(attribute, t);
			             	var stream = new Vector3InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, valueFastIn);
			             });
			RegisterInput(typeof(IInStream<Vector4>), (host, attribute, t) => {
			             	var valueFastIn = host.CreateValueFastInput(attribute, t);
			             	var stream = new Vector4InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, valueFastIn);
			             });

			RegisterInput(typeof(IInStream<string>), (host, attribute, t) => {
			             	var stringIn = host.CreateStringInput(attribute, t);
			             	var stream = new StringInStream(stringIn);
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, stringIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, stringIn);
			             });
			
			RegisterInput(typeof(IInStream<RGBAColor>), (host, attribute, t) => {
			             	var colorIn = host.CreateColorInput(attribute, t);
			             	var stream = new ColorInStream(GetColorPointerFunc(colorIn), GetValidateFunc(colorIn));
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, colorIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, colorIn);
			             });

			RegisterInput(typeof(IInStream<EnumEntry>), (host, attribute, t) => {
			             	var enumIn = host.CreateEnumInput(attribute, t);
			             	var stream = new DynamicEnumInStream(enumIn);
			             	if (attribute.AutoValidate)
			             		return IOHandler.Create(stream, enumIn, s => s.Sync(), null);
			             	else
			             		return IOHandler.Create(stream, enumIn);
			             });
			
			RegisterInput(typeof(ISpread<>), (host, attribute, t) => {
			              	var ioBuilder = CreateIO(typeof(IInStream<>).MakeGenericType(t), typeof(IInStream<>).MakeGenericType(t), host, attribute);
			              	var pinType = typeof(InputPin<>).MakeGenericType(t);
			              	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as IInputPin;
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(pin, ioBuilder.Metadata, p => p.Sync(), null);
			              	else
			              		return IOHandler.Create(pin, ioBuilder.Metadata);
			              });
			
			RegisterInput(typeof(IDiffSpread<>), (host, attribute, t) => {
			              	attribute.CheckIfChanged = true;
			              	var ioBuilder = CreateIO(typeof(IInStream<>).MakeGenericType(t), typeof(IInStream<>).MakeGenericType(t), host, attribute);
			              	var pinType = typeof(InputPin<>).MakeGenericType(t);
			              	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as IInputPin;
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(pin, ioBuilder.Metadata, p => p.Sync(), null);
			              	else
			              		return IOHandler.Create(pin, ioBuilder.Metadata);
			              });
			
			RegisterInput(typeof(IDXRenderStateIn), (host, attribute, t) => {
			               	IDXRenderStateIn pin;
			               	host.CreateRenderStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterInput(typeof(IDXSamplerStateIn), (host, attribute, t) => {
			               	IDXSamplerStateIn pin;
			               	host.CreateSamplerStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterOutput(typeof(IOutStream<double>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new DoubleOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });
			
			RegisterOutput(typeof(IOutStream<float>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new FloatOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });
			
			RegisterOutput(typeof(IOutStream<int>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new IntOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });
			
			RegisterOutput(typeof(IOutStream<bool>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new BoolOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });

			RegisterOutput(typeof(IOutStream<Matrix4x4>), (host, attribute, t) => {
			             	var transformOut = host.CreateTransformOutput(attribute, t);
			             	return IOHandler.Create(new Matrix4x4OutStream(GetResizeMatrixArrayFunc(transformOut)), transformOut, s => s.Flush(), null);
			             });
			
			RegisterOutput(typeof(IOutStream<Matrix>), (host, attribute, t) => {
			             	var transformOut = host.CreateTransformOutput(attribute, t);
			             	return IOHandler.Create(new MatrixOutStream(GetResizeMatrixArrayFunc(transformOut)), transformOut, s => s.Flush(), null);
			             });

			RegisterOutput(typeof(IOutStream<Vector2D>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new Vector2DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });
			RegisterOutput(typeof(IOutStream<Vector3D>),(host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new Vector3DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });
			RegisterOutput(typeof(IOutStream<Vector4D>),(host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new Vector4DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });

			RegisterOutput(typeof(IOutStream<Vector2>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new Vector2OutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });
			RegisterOutput(typeof(IOutStream<Vector3>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new Vector3OutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });
			RegisterOutput(typeof(IOutStream<Vector4>), (host, attribute, t) => {
			             	var valueOut = host.CreateValueOutput(attribute, t);
			             	return IOHandler.Create(new Vector4OutStream(GetResizeValueArrayFunc(valueOut)), valueOut, s => s.Flush(), null);
			             });

			RegisterOutput(typeof(IOutStream<string>), (host, attribute, t) => {
			             	var stringOut = host.CreateStringOutput(attribute, t);
			             	return IOHandler.Create(new StringOutStream(stringOut), stringOut, s => s.Flush(), null);
			             });
			
			RegisterOutput(typeof(IOutStream<RGBAColor>), (host, attribute, t) => {
			             	var colorOut = host.CreateColorOutput(attribute, t);
			             	return IOHandler.Create(new ColorOutStream(GetResizeColorArrayFunc(colorOut)), colorOut, s => s.Flush(), null);
			             });

			RegisterOutput(typeof(IOutStream<EnumEntry>), (host, attribute, t) => {
			             	var enumOut = host.CreateEnumOutput(attribute, t);
			             	return IOHandler.Create(new DynamicEnumOutStream(enumOut), enumOut, s => s.Flush(), null);
			             });
			
			RegisterOutput(typeof(IDXLayerIO), (host, attribute, t) => {
			               	IDXLayerIO pin;
			               	host.CreateLayerOutput(attribute.Name, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterOutput(typeof(IDXMeshOut), (host, attribute, t) => {
			               	IDXMeshOut pin;
			               	host.CreateMeshOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterOutput(typeof(IDXTextureOut), (host, attribute, t) => {
			               	IDXTextureOut pin;
			               	host.CreateTextureOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
			               	return IOHandler.Create(pin, pin);
			               });
			
			RegisterOutput(typeof(ISpread<>), (host, attribute, t) => {
			              	var ioBuilder = CreateIO(typeof(IOutStream<>).MakeGenericType(t), typeof(IOutStream<>).MakeGenericType(t), host, attribute);
			              	var pinType = typeof(OutputPin<>).MakeGenericType(t);
			              	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as IOutputPin;
			              	return IOHandler.Create(pin, ioBuilder.Metadata, p => p.Flush(), null);
			              });
			
			RegisterConfig(typeof(IIOStream<double>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new DoubleInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new DoubleOutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<double>(inStream, outStream), valueConfig);
			             });
			
			RegisterConfig(typeof(IIOStream<float>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new FloatInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new FloatOutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<float>(inStream, outStream), valueConfig);
			             });
			
			RegisterConfig(typeof(IIOStream<int>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new IntInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new IntOutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<int>(inStream, outStream), valueConfig);
			             });
			
			RegisterConfig(typeof(IIOStream<bool>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new BoolInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new BoolOutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<bool>(inStream, outStream), valueConfig);
			             });

			RegisterConfig(typeof(IIOStream<Vector2D>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new Vector2DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new Vector2DOutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<Vector2D>(inStream, outStream), valueConfig);
			             });
			RegisterConfig(typeof(IIOStream<Vector3D>),(host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new Vector3DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new Vector3DOutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<Vector3D>(inStream, outStream), valueConfig);
			             });
			RegisterConfig(typeof(IIOStream<Vector4D>),(host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new Vector4DInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new Vector4DOutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<Vector4D>(inStream, outStream), valueConfig);
			             });

			RegisterConfig(typeof(IIOStream<Vector2>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new Vector2InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new Vector2OutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<Vector2>(inStream, outStream), valueConfig);
			             });
			RegisterConfig(typeof(IIOStream<Vector3>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new Vector3InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new Vector3OutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<Vector3>(inStream, outStream), valueConfig);
			             });
			RegisterConfig(typeof(IIOStream<Vector4>), (host, attribute, t) => {
			             	var valueConfig = host.CreateValueConfig(attribute, t);
			             	var inStream = new Vector4InStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			             	var outStream = new Vector4OutStream(ResizeValueArrayFunc(valueConfig));
			             	return IOHandler.Create(new ConfigIOStream<Vector4>(inStream, outStream), valueConfig);
			             });

			RegisterConfig(typeof(IIOStream<string>), (host, attribute, t) => {
			             	var stringConfig = host.CreateStringConfig(attribute, t);
			             	return IOHandler.Create(new StringConfigStream(stringConfig), stringConfig);
			             });
			
			RegisterConfig(typeof(IIOStream<RGBAColor>), (host, attribute, t) => {
			             	var colorConfig = host.CreateColorConfig(attribute, t);
			             	var inStream = new ColorInStream(GetColorPointerFunc(colorConfig), GetValidateFunc(colorConfig));
			             	var outStream = new ColorOutStream(ResizeColorArrayFunc(colorConfig));
			             	return IOHandler.Create(new ConfigIOStream<RGBAColor>(inStream, outStream), colorConfig);
			             });

			RegisterConfig(typeof(IIOStream<EnumEntry>), (host, attribute, t) => {
			             	var enumConfig = host.CreateEnumConfig(attribute, t);
			             	return IOHandler.Create(new DynamicEnumConfigStream(enumConfig), enumConfig);
			             });
			
			RegisterConfig(typeof(ISpread<>), (host, attribute, t) => {
			              	var ioBuilder = CreateIO(typeof(IIOStream<>).MakeGenericType(t), typeof(IIOStream<>).MakeGenericType(t), host, attribute);
			              	var pinType = typeof(ConfigPin<>).MakeGenericType(t);
			              	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject);
			              	return IOHandler.Create(pin, ioBuilder.Metadata);
			              });
		}

		public void RegisterInput(Type ioType, Func<IInternalPluginHost, InputAttribute, Type, IOHandler> createInputFunc)
		{
			FInputDelegates[ioType] = createInputFunc;
		}
		
		public void RegisterOutput(Type ioType, Func<IInternalPluginHost, OutputAttribute, Type, IOHandler> createOutputFunc)
		{
			FOutputDelegates[ioType] = createOutputFunc;
		}
		
		public void RegisterConfig(Type ioType, Func<IInternalPluginHost, ConfigAttribute, Type, IOHandler> createConfigFunc)
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
		
		public IOHandler CreateIO(Type openIOType, Type closedIOType, IInternalPluginHost pluginHost, IOAttribute attribute)
		{
			var ioDataType = closedIOType.GetGenericArguments().FirstOrDefault();
			
			var inputAttribute = attribute as InputAttribute;
			if (inputAttribute != null)
			{
				return FInputDelegates[openIOType](pluginHost, inputAttribute, ioDataType);
			}
			
			var outputAttribute = attribute as OutputAttribute;
			if (outputAttribute != null)
			{
				return FOutputDelegates[openIOType](pluginHost, outputAttribute, ioDataType);
			}
			
			var configAttribute = attribute as ConfigAttribute;
			if (configAttribute != null)
			{
				return FConfigDelegates[openIOType](pluginHost, configAttribute, ioDataType);
			}
			
			throw new NotSupportedException(string.Format("Can't create IO of type '{0}'.", closedIOType));
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
		
		static private Func<bool> GetValidateFunc(IValueIn valueIn)
		{
			// TODO: check this
			return () => { return valueIn.PinIsChanged; };
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
		
		static private Func<bool> GetValidateFunc(IValueFastIn valueFastIn)
		{
			// TODO: check this
			return () => { return true; };
		}
		
		static private Func<bool> GetValidateFunc(IColorIn colorIn)
		{
			// TODO: check this
			return () => { return colorIn.PinIsChanged; };
		}
		
		static private Func<bool> GetValidateFunc(ITransformIn transformIn)
		{
			// TODO: check this
			return () => { return transformIn.PinIsChanged; };
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
