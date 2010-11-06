using System;
using System.Diagnostics;
using SlimDX;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// Builds objects of type Pin<T>.
	/// </summary>
	public static class PinFactory
	{
		public static Pin<T> CreatePin<T>(IPluginHost host, Attribute attribute)
		{
			return CreatePin(host, attribute, typeof(T)) as Pin<T>;
		}
		
		public static DiffPin<T> CreateDiffPin<T>(IPluginHost host, Attribute attribute)
		{
			return CreateDiffPin(host, attribute, typeof(T)) as DiffPin<T>;
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
			Debug.WriteLine(string.Format("Creating input pin '{0}'.", attribute.Name));
			
			if (type.IsGenericType)
			{
				// Test if type == ISpread<T>
				var subSpreadType = type.GetGenericArguments()[0];
				if (typeof(ISpread<>).MakeGenericType(subSpreadType).IsAssignableFrom(type))
				{
					var pinType = typeof(InputBinSpread<>).MakeGenericType(subSpreadType);
					if(attribute.IsPinGroup)
						pinType = typeof(InputSpreadList<>).MakeGenericType(subSpreadType);
					
					return Activator.CreateInstance(pinType, new object[] { host, attribute });
				}
			}
			
			if (type == typeof(double))
				return new DoubleInputPin(host, attribute);
			else if (type == typeof(float))
				return new FloatInputPin(host, attribute);
			else if (type == typeof(int))
				return new IntInputPin(host, attribute);
			else if (type == typeof(bool))
				return new BoolInputPin(host, attribute);
			else if (type == typeof(string))
				return new StringInputPin(host, attribute);
			else if (type == typeof(RGBAColor))
				return new ColorInputPin(host, attribute);
			else if (type == typeof(Matrix4x4))
				return new Matrix4x4InputPin(host, attribute);
			else if (type == typeof(Matrix))
				return new SlimDXMatrixInputPin(host, attribute);
			else if (type == typeof(Vector2D))
				return new Vector2DInputPin(host, attribute);
			else if (type == typeof(Vector3D))
				return new Vector3DInputPin(host, attribute);
			else if (type == typeof(Vector4D))
				return new Vector4DInputPin(host, attribute);
			else if (type == typeof(Vector2))
				return new Vector2InputPin(host, attribute);
			else if (type == typeof(Vector3))
				return new Vector3InputPin(host, attribute);
			else if (type == typeof(Vector4))
				return new Vector4InputPin(host, attribute);
			else if (type.BaseType == typeof(Enum))
			{
				var pinType = typeof(EnumInputPin<>).MakeGenericType(type);
				return Activator.CreateInstance(pinType, new object[] { host, attribute });
			}
			else if (type == typeof(EnumEntry))
				return new DynamicEnumInputPin(host, attribute);
			else
			{
				var pinType = typeof(GenericInputPin<>).MakeGenericType(type);
				return Activator.CreateInstance(pinType, new object[] { host, attribute });
			}
		}
		
		public static object CreateDiffPin(IPluginHost host, InputAttribute attribute, Type type)
		{
			Debug.WriteLine(string.Format("Creating diff input pin '{0}'.", attribute.Name));
			
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
			}
			
			if (type == typeof(double))
				return new DiffDoubleInputPin(host, attribute);
			else if (type == typeof(float))
				return new DiffFloatInputPin(host, attribute);
			else if (type == typeof(int))
				return new DiffIntInputPin(host, attribute);
			else if (type == typeof(bool))
				return new DiffBoolInputPin(host, attribute);
			else if (type == typeof(Vector2D))
				return new DiffVector2DInputPin(host, attribute);
			else if (type == typeof(Vector3D))
				return new DiffVector3DInputPin(host, attribute);
			else if (type == typeof(Vector4D))
				return new DiffVector4DInputPin(host, attribute);
			else if (type == typeof(Vector2))
				return new DiffVector2InputPin(host, attribute);
			else if (type == typeof(Vector3))
				return new DiffVector3InputPin(host, attribute);
			else if (type == typeof(Vector4))
				return new DiffVector4InputPin(host, attribute);
			else
				return CreatePin(host, attribute, type);
		}
		
		public static object CreatePin(IPluginHost host, OutputAttribute attribute, Type type)
		{
			Debug.WriteLine(string.Format("Creating output pin '{0}'.", attribute.Name));
			
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
			}
			
			if (type == typeof(double))
				return new DoubleOutputPin(host, attribute);
			else if (type == typeof(float))
				return new FloatOutputPin(host, attribute);
			else if (type == typeof(int))
				return new IntOutputPin(host, attribute);
			else if (type == typeof(bool))
				return new BoolOutputPin(host, attribute);
			else if (type == typeof(string))
				return new StringOutputPin(host, attribute);
			else if (type == typeof(RGBAColor))
				return new ColorOutputPin(host, attribute);
			else if (type == typeof(Matrix4x4))
				return new Matrix4x4OutputPin(host, attribute);
			else if (type == typeof(Matrix))
				return new SlimDXMatrixOutputPin(host, attribute);
			else if (type == typeof(Vector2D))
				return new Vector2DOutputPin(host, attribute);
			else if (type == typeof(Vector3D))
				return new Vector3DOutputPin(host, attribute);
			else if (type == typeof(Vector4D))
				return new Vector4DOutputPin(host, attribute);
			else if (type == typeof(Vector2))
				return new Vector2OutputPin(host, attribute);
			else if (type == typeof(Vector3))
				return new Vector3OutputPin(host, attribute);
			else if (type == typeof(Vector4))
				return new Vector4OutputPin(host, attribute);
			else if (type.BaseType == typeof(Enum))
			{
				var pinType = typeof(EnumOutputPin<>).MakeGenericType(type);
				return Activator.CreateInstance(pinType, new object[] { host, attribute });
			}
			else if (type == typeof(EnumEntry))
				return new DynamicEnumOutputPin(host, attribute);
			else
			{
				var pinType = typeof(GenericOutputPin<>).MakeGenericType(type);
				return Activator.CreateInstance(pinType, new object[] { host, attribute });
			}
		}
		
		public static object CreatePin(IPluginHost host, ConfigAttribute attribute, Type type)
		{
			Debug.WriteLine(string.Format("Creating config pin '{0}'.", attribute.Name));
			
			if (type == typeof(double))
				return new DoubleConfigPin(host, attribute);
			else if (type == typeof(float))
				return new FloatConfigPin(host, attribute);
			else if (type == typeof(int))
				return new IntConfigPin(host, attribute);
			else if (type == typeof(bool))
				return new BoolConfigPin(host, attribute);
			else if (type == typeof(string))
				return new StringConfigPin(host, attribute);
			else if (type == typeof(RGBAColor))
				return new ColorConfigPin(host, attribute);
			else if (type == typeof(Vector2D))
				return new Vector2DConfigPin(host, attribute);
			else if (type == typeof(Vector3D))
				return new Vector3DConfigPin(host, attribute);
			else if (type == typeof(Vector4D))
				return new Vector4DConfigPin(host, attribute);
			else if (type == typeof(Vector2))
				return new Vector2ConfigPin(host, attribute);
			else if (type == typeof(Vector3))
				return new Vector3ConfigPin(host, attribute);
			else if (type == typeof(Vector4))
				return new Vector4ConfigPin(host, attribute);
			else if (type.BaseType == typeof(Enum))
			{
				var pinType = typeof(EnumConfigPin<>).MakeGenericType(type);
				return Activator.CreateInstance(pinType, new object[] { host, attribute });
			}
			else if (type == typeof(EnumEntry))
				return new DynamicEnumConfigPin(host, attribute);
			else
				throw new NotImplementedException(string.Format("ConfigPin of type '{0}' not supported.", type));
		}
	}
}
