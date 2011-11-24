//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using SlimDX;
//using VVVV.Hosting.Interfaces;
//using VVVV.Hosting.Pins.Config;
//using VVVV.Hosting.Pins.Input;
//using VVVV.Hosting.Pins.Output;
//using VVVV.Hosting.Streams;
//using VVVV.PluginInterfaces.V1;
//using VVVV.PluginInterfaces.V2;
//using VVVV.Utils.VColor;
//using VVVV.Utils.VMath;
//
//namespace VVVV.Hosting.Pins
//{
//	/// <summary>
//	/// Builds objects of type Pin<T>.
//	/// </summary>
//	[ComVisible(false)]
//	public class PinFactory : IDisposable
//	{
//		private readonly IOFactory FStreamFactory;
//		private readonly IInternalPluginHost FPluginHost;
//        
//        public PinFactory(IOFactory streamFactory, IInternalPluginHost pluginHost)
//        {
//        	FStreamFactory = streamFactory;
//        	FPluginHost = pluginHost;
//        }
//
//		public Pin<T> CreatePin<T>(Attribute attribute)
//		{
//			return CreatePin(attribute, typeof(T)) as Pin<T>;
//		}
//		
//		public DiffInputPin<T> CreateDiffPin<T>(Attribute attribute)
//		{
//			return CreateDiffPin(attribute, typeof(T)) as DiffInputPin<T>;
//		}
//		
//		public ISpread<T> CreateSpread<T>(Attribute attribute)
//		{
//			return CreatePin(attribute, typeof(T)) as ISpread<T>;
//		}
//		
//		public IDiffSpread<T> CreateDiffSpread<T>(Attribute attribute)
//		{
//			return CreateDiffPin(attribute, typeof(T)) as IDiffSpread<T>;
//		}
//		
//		public object CreatePin(Attribute attribute, Type type)
//		{
//			if (attribute is InputAttribute)
//				return CreatePin(attribute as InputAttribute, type);
//			else if (attribute is OutputAttribute)
//				return CreatePin(attribute as OutputAttribute, type);
//			else if (attribute is ConfigAttribute)
//				return CreatePin(attribute as ConfigAttribute, type);
//			else
//				throw new ArgumentException(string.Format("Unknown pin attribute '{0}'.", attribute));
//		}
//		
//		public object CreateDiffPin(Attribute attribute, Type type)
//		{
//			if (attribute is InputAttribute)
//				return CreateDiffPin(attribute as InputAttribute, type);
//			if (attribute is ConfigAttribute)
//				return CreatePin(attribute as ConfigAttribute, type);
//			else
//				throw new ArgumentException(string.Format("Either attribute '{0}' can't be used for type IDiffSpread<{1}> or unknown.", attribute, type));
//		}
//		
//		public object CreatePin(InputAttribute attribute, Type type)
//		{
//			Debug.WriteLine(string.Format("Creating input pin '{0}' at position {1}.", attribute.Name, attribute.Order));
//			
//			if (type.IsGenericType)
//			{
//				var subSpreadType = type.GetGenericArguments()[0];
//				if (typeof(ISpread<>).MakeGenericType(subSpreadType).IsAssignableFrom(type))
//				{
//					// type == ISpread<T>
//					var pinType = typeof(InputBinSpread<>).MakeGenericType(subSpreadType);
//					if(attribute.IsPinGroup)
//						pinType = typeof(InputSpreadList<>).MakeGenericType(subSpreadType);
//					
//					return Activator.CreateInstance(pinType, new object[] { FPluginHost, attribute });
//				}
//				else
//				{
//					var openGenericType = type.GetGenericTypeDefinition();
//					if (FInputPinFactory.ContainsType(openGenericType))
//					{
//						var stream = FInputPinFactory.CreatePin(openGenericType, FPluginHost, type, attribute);
//						var pinType = typeof(InputPin<>).MakeGenericType(subSpreadType);
////						new InputPin<T>(
//					}
//				}
//			}
//
//            if (FInputPinFactory.ContainsType(type))
//            {
//                return FInputPinFactory.CreatePin(type, FPluginHost, type, attribute);
//            }
//            else if (type.BaseType == typeof(Enum))
//            {
//                var pinType = typeof(DiffInputPin<>).MakeGenericType(type);
//                var enumIn = FPluginHost.CreateEnumInput(attribute, type);
//                var stream = Activator.CreateInstance(typeof(EnumInStream<>).MakeGenericType(type), new object[] { enumIn });
//                return Activator.CreateInstance(pinType, new object[] { FPluginHost, enumIn, stream });
//            }
//            else
//            {
//                var pinType = typeof(DiffInputPin<>).MakeGenericType(type);
//                var nodeIn = FPluginHost.CreateNodeInput(attribute, type);
//                var stream = Activator.CreateInstance(typeof(NodeInStream<>).MakeGenericType(type), new object[] { nodeIn });
//                return Activator.CreateInstance(pinType, new object[] { FPluginHost, nodeIn, stream });
//            }
//		}
//		
//		public object CreateDiffPin(InputAttribute attribute, Type type)
//		{
//			Debug.WriteLine(string.Format("Creating diff input pin '{0}' as position {1}.", attribute.Name, attribute.Order));
//			
//			if (type.IsGenericType)
//			{
//				// Test if type == ISpread<T>
//				var subSpreadType = type.GetGenericArguments()[0];
//				if (typeof(ISpread<>).MakeGenericType(subSpreadType).IsAssignableFrom(type))
//				{
//					var pinType = typeof(DiffInputBinSpread<>).MakeGenericType(subSpreadType);
//					if(attribute.IsPinGroup)
//						pinType = typeof(DiffInputSpreadList<>).MakeGenericType(subSpreadType);
//					
//					return Activator.CreateInstance(pinType, new object[] { FPluginHost, attribute });
//				}
//				else
//				{
//					var openGenericType = type.GetGenericTypeDefinition();
//					if (FDiffInputPinFactory.ContainsType(openGenericType))
//					{
//						return FDiffInputPinFactory.CreatePin(openGenericType, FPluginHost, type, attribute);
//					}
//				}
//			}
//
//            if (FDiffInputPinFactory.ContainsType(type))
//            {
//                return FDiffInputPinFactory.CreatePin(type, FPluginHost, type, attribute);
//            }
//            else
//            {
//                return CreatePin(FPluginHost, attribute, type);
//            }
//		}
//		
//		public object CreatePin(OutputAttribute attribute, Type type)
//		{
//			Debug.WriteLine(string.Format("Creating output pin '{0}' as position {1}.", attribute.Name, attribute.Order));
//			
//			if (type.IsGenericType)
//			{
//				// Test if type == ISpread<T>
//				var subSpreadType = type.GetGenericArguments()[0];
//				if (typeof(ISpread<>).MakeGenericType(subSpreadType).IsAssignableFrom(type))
//				{
//					var pinType = typeof(OutputBinSpread<>).MakeGenericType(subSpreadType);
//					if(attribute.IsPinGroup)
//						pinType = typeof(OutputSpreadList<>).MakeGenericType(subSpreadType);
//					
//					return Activator.CreateInstance(pinType, new object[] { FPluginHost, attribute });
//				}
//				else
//				{
//					var openGenericType = type.GetGenericTypeDefinition();
//					if (FOutputPinFactory.ContainsType(openGenericType))
//					{
//						return FOutputPinFactory.CreatePin(openGenericType, FPluginHost, type, attribute);
//					}
//				}
//			}
//			
//            if (FOutputPinFactory.ContainsType(type))
//            {
//                return FOutputPinFactory.CreatePin(type, FPluginHost, type, attribute);
//            }
//            else if (type.BaseType == typeof(Enum))
//            {
//                var pinType = typeof(Pin<>).MakeGenericType(type);
//                var enumOut = FPluginHost.CreateEnumOutput(attribute, type);
//                var stream = Activator.CreateInstance(typeof(EnumOutStream<>).MakeGenericType(type), new object[] { enumOut });
//                return Activator.CreateInstance(pinType, new object[] { FPluginHost, enumOut, stream });
//            }
//            else
//            {
//                var pinType = typeof(Pin<>).MakeGenericType(type);
//                var nodeOut = FPluginHost.CreateNodeOutput(attribute, type);
//                var stream = Activator.CreateInstance(typeof(NodeOutStream<>).MakeGenericType(type), new object[] { nodeOut });
//                return Activator.CreateInstance(pinType, new object[] { FPluginHost, nodeOut, stream });
//            }
//		}
//		
//		public object CreatePin(ConfigAttribute attribute, Type type)
//		{
//			Debug.WriteLine(string.Format("Creating config pin '{0}'.", attribute.Name));
//			
//            if (FConfigPinFactory.ContainsType(type))
//            {
//                return FConfigPinFactory.CreatePin(type, FPluginHost, type, attribute);
//            }
//             else if (type.BaseType == typeof(Enum))
//            {
//                var pinType = typeof(ConfigPin<>).MakeGenericType(type);
//                var enumConfig = FPluginHost.CreateEnumConfig(attribute, type);
//                var stream = Activator.CreateInstance(typeof(EnumConfigStream<>).MakeGenericType(type), new object[] { enumConfig });
//                return Activator.CreateInstance(pinType, new object[] { FPluginHost, enumConfig, stream });
//            }
//            else
//                throw new NotImplementedException(string.Format("ConfigPin of type '{0}' not supported.", type));
//		}
//		
//		// TODO: Find way to integrate DXLayerIO in ISpread<T>
//		public IDXLayerIO CreateLayerOutput(OutputAttribute outputAttribute)
//		{
//			IDXLayerIO pin;
//			FPluginHost.CreateLayerOutput(outputAttribute.Name, (TPinVisibility)outputAttribute.Visibility, out pin);
//			return pin;
//		}
//		
//		// TODO: Find way to integrate IDXMeshOut in ISpread<T>
//		public IDXMeshOut CreateMeshOutput(OutputAttribute outputAttribute)
//		{
//			IDXMeshOut pin;
//			FPluginHost.CreateMeshOutput(outputAttribute.Name, (TSliceMode)outputAttribute.SliceMode, (TPinVisibility)outputAttribute.Visibility, out pin);
//			return pin;
//		}
//		
//		// TODO: Find way to integrate IDXTextureOut in ISpread<T>
//		public IDXTextureOut CreateTextureOutput(OutputAttribute outputAttribute)
//		{
//			IDXTextureOut pin;
//			FPluginHost.CreateTextureOutput(outputAttribute.Name, (TSliceMode)outputAttribute.SliceMode, (TPinVisibility)outputAttribute.Visibility, out pin);
//			return pin;
//		}
//		
//		// TODO: Find way to integrate IDXRenderStateIn in ISpread<T>
//		public IDXRenderStateIn CreateRenderStateInput(InputAttribute inputAttribute)
//		{
//			IDXRenderStateIn pin;
//			FPluginHost.CreateRenderStateInput((TSliceMode)inputAttribute.SliceMode, (TPinVisibility)inputAttribute.Visibility, out pin);
//			return pin;
//		}
//		
//		// TODO: Find way to integrate IDXSamplerStateIn in ISpread<T>
//		public IDXSamplerStateIn CreateSamplerStateInput(InputAttribute inputAttribute)
//		{
//			IDXSamplerStateIn pin;
//			FPluginHost.CreateSamplerStateInput((TSliceMode)inputAttribute.SliceMode, (TPinVisibility)inputAttribute.Visibility, out pin);
//			return pin;
//		}
//		
//		public void Dispose()
//		{
//			
//		}
//	}
//}
