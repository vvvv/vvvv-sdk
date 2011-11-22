using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{


	/// <summary>
	/// Builds objects of type Pin<T>.
	/// </summary>
	[ComVisible(false)]
	public static class PinFactory
	{
        private static InputPinRegistry inputPinFactory = new InputPinRegistry();
        private static DiffInputPinRegistry diffInputPinFactory = new DiffInputPinRegistry();
        private static ConfigPinRegistry configPinFactory = new ConfigPinRegistry();
        private static OutputPinRegistry outputPinFactory = new OutputPinRegistry();

        public static void RegisterCustomInputPinType(Type t, InputPinRegistry.PinCreateDelegate creator)
        {
            inputPinFactory.RegisterType(t, creator);
        }

        public static void RegisterCustomDiffInputPinType(Type t, DiffInputPinRegistry.PinCreateDelegate creator)
        {
            diffInputPinFactory.RegisterType(t, creator);
        }

        public static void RegisterCustomOutputPinType(Type t, OutputPinRegistry.PinCreateDelegate creator)
        {
            outputPinFactory.RegisterType(t, creator);
        }

		public static Pin<T> CreatePin<T>(IPluginHost host, Attribute attribute)
		{
			return CreatePin(host, attribute, typeof(T)) as Pin<T>;
		}
		
		public static DiffInputPin<T> CreateDiffPin<T>(IPluginHost host, Attribute attribute)
		{
			return CreateDiffPin(host, attribute, typeof(T)) as DiffInputPin<T>;
		}
		
		public static ISpread<T> CreateSpread<T>(IPluginHost host, Attribute attribute)
		{
			return CreatePin(host, attribute, typeof(T)) as ISpread<T>;
		}
		
		public static IDiffSpread<T> CreateDiffSpread<T>(IPluginHost host, Attribute attribute)
		{
			return CreateDiffPin(host, attribute, typeof(T)) as IDiffSpread<T>;
		}
		
		public static object CreatePin(IPluginHost host, Attribute attribute, Type type)
		{
			if (attribute is InputAttribute)
				return CreatePin(host, attribute as InputAttribute, type);
			else if (attribute is OutputAttribute)
				return CreatePin(host, attribute as OutputAttribute, type);
			else if (attribute is ConfigAttribute)
				return CreatePin(host, attribute as ConfigAttribute, type);
			else
				throw new ArgumentException(string.Format("Unknown pin attribute '{0}'.", attribute));
		}
		
		public static object CreateDiffPin(IPluginHost host, Attribute attribute, Type type)
		{
			if (attribute is InputAttribute)
				return CreateDiffPin(host, attribute as InputAttribute, type);
			if (attribute is ConfigAttribute)
				return CreatePin(host, attribute as ConfigAttribute, type);
			else
				throw new ArgumentException(string.Format("Either attribute '{0}' can't be used for type IDiffSpread<{1}> or unknown.", attribute, type));
		}
		
		public static object CreatePin(IPluginHost host, InputAttribute attribute, Type type)
		{
			Debug.WriteLine(string.Format("Creating input pin '{0}' at position {1}.", attribute.Name, attribute.Order));
			
			if (type.IsGenericType)
			{
				var subSpreadType = type.GetGenericArguments()[0];
				if (typeof(ISpread<>).MakeGenericType(subSpreadType).IsAssignableFrom(type))
				{
					// type == ISpread<T>
					var pinType = typeof(InputBinSpread<>).MakeGenericType(subSpreadType);
					if(attribute.IsPinGroup)
						pinType = typeof(InputSpreadList<>).MakeGenericType(subSpreadType);
					
					return Activator.CreateInstance(pinType, new object[] { host, attribute });
				}
				else
				{
					var openGenericType = type.GetGenericTypeDefinition();
					if (inputPinFactory.ContainsType(openGenericType))
					{
						var stream = inputPinFactory.CreatePin(openGenericType, host, type, attribute);
						var pinType = typeof(InputPin<>).MakeGenericType(subSpreadType);
//						new InputPin<T>(
					}
				}
			}

            if (inputPinFactory.ContainsType(type))
            {
                return inputPinFactory.CreatePin(type, host, type, attribute);
            }
            else if (type.BaseType == typeof(Enum))
            {
                var pinType = typeof(DiffInputPin<>).MakeGenericType(type);
                var enumIn = host.CreateEnumInput(attribute, type);
                var stream = Activator.CreateInstance(typeof(EnumInStream<>).MakeGenericType(type), new object[] { enumIn });
                return Activator.CreateInstance(pinType, new object[] { host, enumIn, stream });
            }
            else
            {
                var pinType = typeof(DiffInputPin<>).MakeGenericType(type);
                var nodeIn = host.CreateNodeInput(attribute, type);
                var stream = Activator.CreateInstance(typeof(NodeInStream<>).MakeGenericType(type), new object[] { nodeIn });
                return Activator.CreateInstance(pinType, new object[] { host, nodeIn, stream });
            }
		}
		
		public static object CreateDiffPin(IPluginHost host, InputAttribute attribute, Type type)
		{
			Debug.WriteLine(string.Format("Creating diff input pin '{0}' as position {1}.", attribute.Name, attribute.Order));
			
			if (type.IsGenericType)
			{
				// Test if type == ISpread<T>
				var subSpreadType = type.GetGenericArguments()[0];
				if (typeof(ISpread<>).MakeGenericType(subSpreadType).IsAssignableFrom(type))
				{
					var pinType = typeof(DiffInputBinSpread<>).MakeGenericType(subSpreadType);
					if(attribute.IsPinGroup)
						pinType = typeof(DiffInputSpreadList<>).MakeGenericType(subSpreadType);
					
					return Activator.CreateInstance(pinType, new object[] { host, attribute });
				}
				else
				{
					var openGenericType = type.GetGenericTypeDefinition();
					if (diffInputPinFactory.ContainsType(openGenericType))
					{
						return diffInputPinFactory.CreatePin(openGenericType, host, type, attribute);
					}
				}
			}

            if (diffInputPinFactory.ContainsType(type))
            {
                return diffInputPinFactory.CreatePin(type, host, type, attribute);
            }
            else
            {
                return CreatePin(host, attribute, type);
            }
		}
		
		public static object CreatePin(IPluginHost host, OutputAttribute attribute, Type type)
		{
			Debug.WriteLine(string.Format("Creating output pin '{0}' as position {1}.", attribute.Name, attribute.Order));
			
			if (type.IsGenericType)
			{
				// Test if type == ISpread<T>
				var subSpreadType = type.GetGenericArguments()[0];
				if (typeof(ISpread<>).MakeGenericType(subSpreadType).IsAssignableFrom(type))
				{
					var pinType = typeof(OutputBinSpread<>).MakeGenericType(subSpreadType);
					if(attribute.IsPinGroup)
						pinType = typeof(OutputSpreadList<>).MakeGenericType(subSpreadType);
					
					return Activator.CreateInstance(pinType, new object[] { host, attribute });
				}
				else
				{
					var openGenericType = type.GetGenericTypeDefinition();
					if (outputPinFactory.ContainsType(openGenericType))
					{
						return outputPinFactory.CreatePin(openGenericType, host, type, attribute);
					}
				}
			}
			
            if (outputPinFactory.ContainsType(type))
            {
                return outputPinFactory.CreatePin(type, host, type, attribute);
            }
            else if (type.BaseType == typeof(Enum))
            {
                var pinType = typeof(Pin<>).MakeGenericType(type);
                var enumOut = host.CreateEnumOutput(attribute, type);
                var stream = Activator.CreateInstance(typeof(EnumOutStream<>).MakeGenericType(type), new object[] { enumOut });
                return Activator.CreateInstance(pinType, new object[] { host, enumOut, stream });
            }
            else
            {
                var pinType = typeof(Pin<>).MakeGenericType(type);
                var nodeOut = host.CreateNodeOutput(attribute, type);
                var stream = Activator.CreateInstance(typeof(NodeOutStream<>).MakeGenericType(type), new object[] { nodeOut });
                return Activator.CreateInstance(pinType, new object[] { host, nodeOut, stream });
            }
		}
		
		public static object CreatePin(IPluginHost host, ConfigAttribute attribute, Type type)
		{
			Debug.WriteLine(string.Format("Creating config pin '{0}'.", attribute.Name));
			
            if (configPinFactory.ContainsType(type))
            {
                return configPinFactory.CreatePin(type, host, type, attribute);
            }
             else if (type.BaseType == typeof(Enum))
            {
                var pinType = typeof(ConfigPin<>).MakeGenericType(type);
                var enumConfig = host.CreateEnumConfig(attribute, type);
                var stream = Activator.CreateInstance(typeof(EnumConfigStream<>).MakeGenericType(type), new object[] { enumConfig });
                return Activator.CreateInstance(pinType, new object[] { host, enumConfig, stream });
            }
            else
                throw new NotImplementedException(string.Format("ConfigPin of type '{0}' not supported.", type));
		}
	}
}
