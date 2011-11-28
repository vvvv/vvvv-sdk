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
			Action<TIOObject> afterEvalAction = null,
			Action<TIOObject> configAction = null
		)
		{
			return new IOHandler<TIOObject>(iOObject, metadata, beforeEvalAction, afterEvalAction, configAction);
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
		public readonly bool IsConfigActionEnabled;
		
		protected IOHandler(object rawIOObject, object metadata, bool before, bool after, bool config = false)
		{
			RawIOObject = rawIOObject;
			Metadata = metadata;
			IsBeforeEvalActionEnabled = before;
			IsAfterEvalActionEnabled = after;
			IsConfigActionEnabled = config;
		}
		
		public abstract void BeforeEvaluate();
		public abstract void AfterEvaluate();
		public abstract void Configurate();
	}
	
	public class IOHandler<TIOObject> : IOHandler
	{
		public readonly TIOObject IOObject;
		private readonly Action<TIOObject> BeforeEvaluateAction;
		private readonly Action<TIOObject> AfterEvaluateAction;
		private readonly Action<TIOObject> ConfigurateAction;
		
		public IOHandler(
			TIOObject iOObject,
			object metadata,
			Action<TIOObject> beforeEvalAction,
			Action<TIOObject> afterEvalAction,
			Action<TIOObject> configAction)
			: base(iOObject, metadata, beforeEvalAction != null, afterEvalAction != null, configAction != null)
		{
			IOObject = iOObject;
			BeforeEvaluateAction = beforeEvalAction;
			AfterEvaluateAction = afterEvalAction;
			ConfigurateAction = configAction;
		}
		
		public override void BeforeEvaluate()
		{
			BeforeEvaluateAction(IOObject);
		}
		
		public override void AfterEvaluate()
		{
			AfterEvaluateAction(IOObject);
		}
		
		public override void Configurate()
		{
			ConfigurateAction(IOObject);
		}
	}
	
	[ComVisible(false)]
	public class IORegistry
	{
		private readonly Dictionary<Type, Func<IOFactory, InputAttribute, Type, IOHandler>> FInputDelegates = new Dictionary<Type, Func<IOFactory, InputAttribute, Type, IOHandler>>();
		private readonly Dictionary<Type, Func<IOFactory, OutputAttribute, Type, IOHandler>> FOutputDelegates = new Dictionary<Type, Func<IOFactory, OutputAttribute, Type, IOHandler>>();
		private readonly Dictionary<Type, Func<IOFactory, ConfigAttribute, Type, IOHandler>> FConfigDelegates = new Dictionary<Type, Func<IOFactory, ConfigAttribute, Type, IOHandler>>();
		
		public IORegistry()
		{
			RegisterInput(typeof(IInStream<double>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
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
			
			RegisterInput(typeof(IInStream<float>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
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
			
			RegisterInput(typeof(IInStream<int>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
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
			
			RegisterInput(typeof(IInStream<bool>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
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

			RegisterInput(typeof(IInStream<Matrix4x4>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var transformIn = host.CreateTransformInput(attribute, t);
			              	var stream = new Matrix4x4InStream(GetMatrixPointerFunc(transformIn), GetValidateFunc(transformIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, transformIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, transformIn);
			              });
			
			RegisterInput(typeof(IInStream<Matrix>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var transformIn = host.CreateTransformInput(attribute, t);
			              	var stream = new MatrixInStream(GetMatrixPointerFunc(transformIn), GetValidateFunc(transformIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, transformIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, transformIn);
			              });

			RegisterInput(typeof(IInStream<Vector2D>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector2DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector3D>),(factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector3DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector4D>),(factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector4DInStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, valueFastIn);
			              });

			RegisterInput(typeof(IInStream<Vector2>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector2InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector3>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector3InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, valueFastIn);
			              });
			RegisterInput(typeof(IInStream<Vector4>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var valueFastIn = host.CreateValueFastInput(attribute, t);
			              	var stream = new Vector4InStream(GetFastValuePointerFunc(valueFastIn), GetValidateFunc(valueFastIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, valueFastIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, valueFastIn);
			              });

			RegisterInput(typeof(IInStream<string>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var stringIn = host.CreateStringInput(attribute, t);
			              	var stream = new StringInStream(stringIn);
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, stringIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, stringIn);
			              });
			
			RegisterInput(typeof(IInStream<RGBAColor>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var colorIn = host.CreateColorInput(attribute, t);
			              	var stream = new ColorInStream(GetColorPointerFunc(colorIn), GetValidateFunc(colorIn));
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, colorIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, colorIn);
			              });

			RegisterInput(typeof(IInStream<EnumEntry>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	var enumIn = host.CreateEnumInput(attribute, t);
			              	var stream = new DynamicEnumInStream(enumIn);
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(stream, enumIn, s => s.Sync(), null);
			              	else
			              		return IOHandler.Create(stream, enumIn);
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
			              				// TODO
			              			}
			              			
			              			var stream = Activator.CreateInstance(multiDimStreamType, factory, attribute.Clone()) as IInStream;
			              			if (attribute.AutoValidate)
			              				return IOHandler.Create(stream, null, s => s.Sync(), null);
			              			else
			              				return IOHandler.Create(stream, null);
			              		}
			              	}
			              	
			              	if (t.BaseType == typeof(Enum))
			              	{
			              		var enumIn = host.CreateEnumInput(attribute, t);
			              		var stream = Activator.CreateInstance(typeof(EnumInStream<>).MakeGenericType(t), new object[] { enumIn }) as IInStream;
			              		if (attribute.AutoValidate)
			              			return IOHandler.Create(stream, enumIn, s => s.Sync(), null);
			              		else
			              			return IOHandler.Create(stream, enumIn);
			              	}
			              	else
			              	{
			              		var nodeIn = host.CreateNodeInput(attribute, t);
			              		var stream = Activator.CreateInstance(typeof(NodeInStream<>).MakeGenericType(t), new object[] { nodeIn }) as IInStream;
			              		if (attribute.AutoValidate)
			              			return IOHandler.Create(stream, nodeIn, s => s.Sync(), null);
			              		else
			              			return IOHandler.Create(stream, nodeIn);
			              	}
			              });
			
			RegisterInput(typeof(ISpread<>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	if (t.IsGenericType)
			              	{
			              		if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
			              		{
			              			var spreadType = typeof(InputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
			              			
			              			if (attribute.IsPinGroup)
			              			{
			              				spreadType = typeof(InputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
			              			}
			              			
			              			var stream = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
			              			if (attribute.AutoValidate)
			              				return IOHandler.Create(stream, null, p => p.Sync());
			              			else
			              				return IOHandler.Create(stream, null);
			              		}
			              	}
			              	var ioBuilder = CreateIO(typeof(IInStream<>), typeof(IInStream<>).MakeGenericType(t), factory, attribute);
			              	var pinType = typeof(InputPin<>).MakeGenericType(t);
			              	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as ISpread;
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(pin, ioBuilder.Metadata, p => p.Sync());
			              	else
			              		return IOHandler.Create(pin, ioBuilder.Metadata);
			              });
			
			RegisterInput(typeof(IDiffSpread<>), (factory, attribute, t) => {
			              	var host = factory.PluginHost;
			              	attribute.CheckIfChanged = true;
			              	if (t.IsGenericType)
			              	{
			              		if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
			              		{
			              			var spreadType = typeof(DiffInputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
			              			
			              			if (attribute.IsPinGroup)
			              			{
			              				spreadType = typeof(DiffInputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
			              			}
			              			
			              			var stream = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
			              			if (attribute.AutoValidate)
			              				return IOHandler.Create(stream, null, p => p.Sync());
			              			else
			              				return IOHandler.Create(stream, null);
			              		}
			              	}
			              	var ioBuilder = CreateIO(typeof(IInStream<>), typeof(IInStream<>).MakeGenericType(t), factory, attribute);
			              	var pinType = typeof(DiffInputPin<>).MakeGenericType(t);
			              	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as ISpread;
			              	if (attribute.AutoValidate)
			              		return IOHandler.Create(pin, ioBuilder.Metadata, p => p.Sync());
			              	else
			              		return IOHandler.Create(pin, ioBuilder.Metadata);
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
			               	return IOHandler.Create(new DoubleOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<float>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new FloatOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<int>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new IntOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<bool>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new BoolOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });

			RegisterOutput(typeof(IOutStream<Matrix4x4>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var transformOut = host.CreateTransformOutput(attribute, t);
			               	return IOHandler.Create(new Matrix4x4OutStream(GetResizeMatrixArrayFunc(transformOut)), transformOut, null, null);
			               });
			
			RegisterOutput(typeof(IOutStream<Matrix>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var transformOut = host.CreateTransformOutput(attribute, t);
			               	return IOHandler.Create(new MatrixOutStream(GetResizeMatrixArrayFunc(transformOut)), transformOut, null, s => s.Flush());
			               });

			RegisterOutput(typeof(IOutStream<Vector2D>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector2DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });
			RegisterOutput(typeof(IOutStream<Vector3D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector3DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });
			RegisterOutput(typeof(IOutStream<Vector4D>),(factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector4DOutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });

			RegisterOutput(typeof(IOutStream<Vector2>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector2OutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });
			RegisterOutput(typeof(IOutStream<Vector3>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector3OutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });
			RegisterOutput(typeof(IOutStream<Vector4>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueOut = host.CreateValueOutput(attribute, t);
			               	return IOHandler.Create(new Vector4OutStream(GetResizeValueArrayFunc(valueOut)), valueOut, null, s => s.Flush());
			               });

			RegisterOutput(typeof(IOutStream<string>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var stringOut = host.CreateStringOutput(attribute, t);
			               	return IOHandler.Create(new StringOutStream(stringOut), stringOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IOutStream<RGBAColor>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var colorOut = host.CreateColorOutput(attribute, t);
			               	return IOHandler.Create(new ColorOutStream(GetResizeColorArrayFunc(colorOut)), colorOut, null, s => s.Flush());
			               });

			RegisterOutput(typeof(IOutStream<EnumEntry>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var enumOut = host.CreateEnumOutput(attribute, t);
			               	return IOHandler.Create(new DynamicEnumOutStream(enumOut), enumOut, null, s => s.Flush());
			               });
			
			RegisterOutput(typeof(IIOStream<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	if (t.IsGenericType)
			               	{
			               		if (typeof(IOutStream<>).MakeGenericType(t.GetGenericArguments()).IsAssignableFrom(t))
			               		{
			               			var multiDimStreamType = typeof(MultiDimOutStream<>).MakeGenericType(t.GetGenericArguments().First());
			               			if (attribute.IsPinGroup)
			               			{
			               				// TODO
			               			}
			               			
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
			               	var ioBuilder = CreateIO(typeof(IOutStream<>).MakeGenericType(t), typeof(IOutStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(OutputPin<>).MakeGenericType(t);
			               	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as ISpread;
			               	return IOHandler.Create(pin, ioBuilder.Metadata, null, p => p.Flush());
			               });
			
			RegisterConfig(typeof(IIOStream<double>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new DoubleInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new DoubleOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<double>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<float>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new FloatInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new FloatOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<float>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<int>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new IntInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
			               	var outStream = new IntOutStream(ResizeValueArrayFunc(valueConfig));
			               	return IOHandler.Create(new ConfigIOStream<int>(inStream, outStream), valueConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(IIOStream<bool>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var valueConfig = host.CreateValueConfig(attribute, t);
			               	var inStream = new BoolInStream(GetValuePointerFunc(valueConfig), GetValidateFunc(valueConfig));
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
			               	var inStream = new ColorInStream(GetColorPointerFunc(colorConfig), GetValidateFunc(colorConfig));
			               	var outStream = new ColorOutStream(ResizeColorArrayFunc(colorConfig));
			               	return IOHandler.Create(new ConfigIOStream<RGBAColor>(inStream, outStream), colorConfig, null, null, s => s.Sync());
			               });

			RegisterConfig(typeof(IIOStream<EnumEntry>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var enumConfig = host.CreateEnumConfig(attribute, t);
			               	return IOHandler.Create(new DynamicEnumConfigStream(enumConfig), enumConfig, null, null, s => s.Sync());
			               });
			
			RegisterConfig(typeof(ISpread<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var ioBuilder = CreateIO(typeof(IIOStream<>).MakeGenericType(t), typeof(IIOStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(ConfigPin<>).MakeGenericType(t);
			               	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as IInputPin;
			               	return IOHandler.Create(pin, ioBuilder.Metadata, null, null, p => p.Sync());
			               });
			
			RegisterConfig(typeof(IDiffSpread<>), (factory, attribute, t) => {
			               	var host = factory.PluginHost;
			               	var ioBuilder = CreateIO(typeof(IIOStream<>).MakeGenericType(t), typeof(IIOStream<>).MakeGenericType(t), factory, attribute);
			               	var pinType = typeof(ConfigPin<>).MakeGenericType(t);
			               	var pin = Activator.CreateInstance(pinType, host, ioBuilder.Metadata, ioBuilder.RawIOObject) as IInputPin;
			               	return IOHandler.Create(pin, ioBuilder.Metadata, null, null, p => p.Sync());
			               });
		}

		public void RegisterInput(Type ioType, Func<IOFactory, InputAttribute, Type, IOHandler> createInputFunc)
		{
			FInputDelegates[ioType] = createInputFunc;
		}
		
		public void RegisterOutput(Type ioType, Func<IOFactory, OutputAttribute, Type, IOHandler> createOutputFunc)
		{
			FOutputDelegates[ioType] = createOutputFunc;
		}
		
		public void RegisterConfig(Type ioType, Func<IOFactory, ConfigAttribute, Type, IOHandler> createConfigFunc)
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
		
		public IOHandler CreateIO(Type openIOType, Type closedIOType, IOFactory factory, IOAttribute attribute)
		{
			var ioDataType = closedIOType.GetGenericArguments().FirstOrDefault();
			var ioName = "IO";
			
			var inputAttribute = attribute as InputAttribute;
			if (inputAttribute != null)
			{
				ioName = "Input";
				if (FInputDelegates.ContainsKey(closedIOType))
					return FInputDelegates[closedIOType](factory, inputAttribute, ioDataType);
				else if (FInputDelegates.ContainsKey(openIOType))
					return FInputDelegates[openIOType](factory, inputAttribute, ioDataType);
			}
			
			var outputAttribute = attribute as OutputAttribute;
			if (outputAttribute != null)
			{
				ioName = "Output";
				if (FOutputDelegates.ContainsKey(closedIOType))
					return FOutputDelegates[closedIOType](factory, outputAttribute, ioDataType);
				else if (FOutputDelegates.ContainsKey(openIOType))
					return FOutputDelegates[openIOType](factory, outputAttribute, ioDataType);
			}
			
			var configAttribute = attribute as ConfigAttribute;
			if (configAttribute != null)
			{
				ioName = "Config";
				if (FConfigDelegates.ContainsKey(closedIOType))
					return FConfigDelegates[closedIOType](factory, configAttribute, ioDataType);
				else if (FConfigDelegates.ContainsKey(openIOType))
					return FConfigDelegates[openIOType](factory, configAttribute, ioDataType);
			}
			
			throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", ioName, closedIOType));
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
