
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Reflection;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	[ComVisible(false)]
	public static class PluginHostExtensions
	{
		// bool, byte, sbyte, int, uint, short, ushort, long, ulong, float, double
		private static Dictionary<Type, Tuple<double, double, double, bool, int>> FDefaultValues = new Dictionary<Type, Tuple<double, double, double, bool, int>>()
		{
			{ typeof(bool), Tuple.Create(0.0, 1.0, 1.0, false, 1) },
			{ typeof(byte), Tuple.Create((double) byte.MinValue, (double) byte.MaxValue, 1.0, true, 1) },
			{ typeof(sbyte), Tuple.Create((double) sbyte.MinValue, (double) sbyte.MaxValue, 1.0, true, 1) },
			{ typeof(int), Tuple.Create((double) int.MinValue, (double) int.MaxValue, 1.0, true, 1) },
			{ typeof(uint), Tuple.Create((double) uint.MinValue, (double) uint.MaxValue, 1.0, true, 1) },
			{ typeof(short), Tuple.Create((double) short.MinValue, (double) short.MaxValue, 1.0, true, 1) },
			{ typeof(ushort), Tuple.Create((double) ushort.MinValue, (double) ushort.MaxValue, 1.0, true, 1) },
			{ typeof(long), Tuple.Create((double) long.MinValue, (double) long.MaxValue, 1.0, true, 1) },
			{ typeof(ulong), Tuple.Create((double) ulong.MinValue, (double) ulong.MaxValue, 1.0, true, 1) },
			{ typeof(float), Tuple.Create((double) float.MinValue, (double) float.MaxValue, 0.01, false, 1) },
			{ typeof(double), Tuple.Create(double.MinValue, double.MaxValue, 0.01, false, 1) },
			{ typeof(Vector2), Tuple.Create((double) float.MinValue, (double) float.MaxValue, 0.01, false, 2) },
			{ typeof(Vector3), Tuple.Create((double) float.MinValue, (double) float.MaxValue, 0.01, false, 3) },
			{ typeof(Vector4), Tuple.Create((double) float.MinValue, (double) float.MaxValue, 0.01, false, 4) },
            { typeof(Quaternion), Tuple.Create((double) float.MinValue, (double) float.MaxValue, 0.01, false, 4) },
			{ typeof(Matrix), Tuple.Create((double) float.MinValue, (double) float.MaxValue, 0.01, false, 1) },
			{ typeof(Vector2D), Tuple.Create(double.MinValue, double.MaxValue, 0.01, false, 2) },
			{ typeof(Vector3D), Tuple.Create(double.MinValue, double.MaxValue, 0.01, false, 3) },
			{ typeof(Vector4D), Tuple.Create(double.MinValue, double.MaxValue, 0.01, false, 4) },
			{ typeof(Matrix4x4), Tuple.Create(double.MinValue, double.MaxValue, 0.01, false, 1) },
		};

        public static void RegisterPinAttributeConfigForType(Type type, double minValue, double maxValue, double stepSize, bool isInteger, int dimensions)
        {
            FDefaultValues.Add(type, Tuple.Create(minValue, maxValue, stepSize, isInteger, dimensions));
        }
		
		private static T NormalizePinAttribute<T>(T attribute, Type type) where T : IOAttribute
		{
			attribute = attribute.Clone() as T;

            if (type == null) { return attribute; }
			
			if (attribute.MinValue == IOAttribute.DefaultMinValue && FDefaultValues.ContainsKey(type))
				attribute.MinValue = FDefaultValues[type].Item1;
			
			if (attribute.MaxValue == IOAttribute.DefaultMaxValue && FDefaultValues.ContainsKey(type))
				attribute.MaxValue = FDefaultValues[type].Item2;
			
			if (attribute.StepSize == IOAttribute.DefaultStepSize && FDefaultValues.ContainsKey(type))
				attribute.StepSize = FDefaultValues[type].Item3;
			
			if (attribute.AsInt)
				attribute.StepSize = 1.0;
			
			var isBool = type == typeof(bool);
			var isInteger = FDefaultValues.ContainsKey(type) && FDefaultValues[type].Item4;
			
			attribute.IsBang = isBool && attribute.IsBang;
			attribute.IsToggle = isBool && !attribute.IsBang;
			attribute.AsInt = isInteger || attribute.AsInt;
			
			if (attribute.IsToggle && attribute.DefaultBoolean)
				attribute.DefaultValue = 1.0;
			
			attribute.Dimension = FDefaultValues.ContainsKey(type) ? FDefaultValues[type].Item5 : attribute.Dimension;
			var defaultValues = attribute.DefaultValues;
			Array.Resize(ref defaultValues, attribute.Dimension);
			attribute.DefaultValues = defaultValues;
			
			if (attribute.DimensionNames != null)
			{
				if(attribute.DimensionNames.Length < attribute.Dimension)
				{
					var newNames = new string[attribute.Dimension];
					for (int i = 0; i < attribute.Dimension; i++)
					{
						if (i < attribute.DimensionNames.Length)
							newNames[i] = attribute.DimensionNames[i];
						else
							newNames[i] = "";
					}
					
					attribute.DimensionNames = newNames;
				}
			}
			
			return attribute;
		}
		
		public static IValueConfig CreateValueConfig(this IPluginHost host, ConfigAttribute attribute, Type type)
		{
			attribute = NormalizePinAttribute(attribute, type);
			
			IValueConfig result = null;
			host.CreateValueConfig(attribute.Name, attribute.Dimension, attribute.DimensionNames, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			switch (attribute.Dimension)
			{
				case 2:
					result.SetSubType2D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 3:
					result.SetSubType3D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 4:
					result.SetSubType4D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				default:
					result.SetSubType(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValue, attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
					
			}
			result.Order = attribute.Order;
			return result;
		}
		
		public static IValueIn CreateValueInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			attribute = NormalizePinAttribute(attribute, type);
			
			IValueIn result = null;
			host.CreateValueInput(attribute.Name, attribute.Dimension, attribute.DimensionNames, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			switch (attribute.Dimension)
			{
				case 2:
					result.SetSubType2D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 3:
					result.SetSubType3D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 4:
					result.SetSubType4D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				default:
					result.SetSubType(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValue, attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
					
			}
            SetInputProperties(result, attribute);
			return result;
		}
		
		public static IValueFastIn CreateValueFastInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			attribute = NormalizePinAttribute(attribute, type);
			
			IValueFastIn result = null;
			host.CreateValueFastInput(attribute.Name, attribute.Dimension, attribute.DimensionNames, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			switch (attribute.Dimension)
			{
				case 2:
					result.SetSubType2D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 3:
					result.SetSubType3D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 4:
					result.SetSubType4D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				default:
					result.SetSubType(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValue, attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
					
			}
            SetInputProperties(result, attribute);
			return result;
		}
		
		public static IValueOut CreateValueOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
			attribute = NormalizePinAttribute(attribute, type);
			
			IValueOut result = null;
			host.CreateValueOutput(attribute.Name, attribute.Dimension, attribute.DimensionNames, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			switch (attribute.Dimension)
			{
				case 2:
					result.SetSubType2D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 3:
					result.SetSubType3D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				case 4:
					result.SetSubType4D(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValues[0], attribute.DefaultValues[1], attribute.DefaultValues[2], attribute.DefaultValues[3], attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
				default:
					result.SetSubType(attribute.MinValue, attribute.MaxValue, attribute.StepSize, attribute.DefaultValue, attribute.IsBang, attribute.IsToggle, attribute.AsInt);
					break;
					
			}
            SetOutputProperties(result, attribute);
			return result;
		}
		
		public static IStringConfig CreateStringConfig(this IPluginHost host, ConfigAttribute attribute, Type type)
		{
            if (type == typeof(char))
                attribute.MaxChars = 1;
            IStringConfig result = null;
			host.CreateStringConfig(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			result.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType) attribute.StringType);
			result.Order = attribute.Order;
			return result;
		}
		
		public static IStringIn CreateStringInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
            if (type == typeof(char))
                attribute.MaxChars = 1;
            IStringIn result = null;
			host.CreateStringInput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			result.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType) attribute.StringType);
            SetInputProperties(result, attribute);
			return result;
		}
		
		public static IStringOut CreateStringOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
            if (type == typeof(char))
                attribute.MaxChars = 1;
            IStringOut result = null;
			host.CreateStringOutput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			result.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType) attribute.StringType);
            SetOutputProperties(result, attribute);
			return result;
		}
		
		public static IColorConfig CreateColorConfig(this IPluginHost host, ConfigAttribute attribute, Type type)
		{
			IColorConfig result = null;
			host.CreateColorConfig(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			result.SetSubType(new RGBAColor(attribute.DefaultColor), attribute.HasAlpha);
			result.Order = attribute.Order;
			return result;
		}
		
		public static IColorIn CreateColorInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			IColorIn result = null;
			host.CreateColorInput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			result.SetSubType(new RGBAColor(attribute.DefaultColor), attribute.HasAlpha);
            SetInputProperties(result, attribute);
			return result;
		}
		
		public static IColorOut CreateColorOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
			IColorOut result = null;
			host.CreateColorOutput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			result.SetSubType(new RGBAColor(attribute.DefaultColor), attribute.HasAlpha);
            SetOutputProperties(result, attribute);
			return result;
		}
		
		public static IEnumConfig CreateEnumConfig(this IPluginHost host, ConfigAttribute attribute, Type type)
		{
		    //this creates and sets the default for a .NET enum
			if (!typeof(EnumEntry).IsAssignableFrom(type))
			{
				var entries = Enum.GetNames(type);
				host.UpdateEnum(type.FullName, entries[0], entries);
			}
			
			IEnumConfig result = null;
			host.CreateEnumConfig(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out result);
						
			if(!string.IsNullOrWhiteSpace(attribute.DefaultEnumEntry))
                result.SetDefaultEntry(attribute.DefaultEnumEntry);
			
			if (!typeof(EnumEntry).IsAssignableFrom(type))
				result.SetSubType(type.FullName);
			else
				result.SetSubType(attribute.EnumName);
			result.Order = attribute.Order;

			return result;
		}
		
		public static IEnumIn CreateEnumInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			//this creates and sets the default for a .NET enum
			if (!typeof(EnumEntry).IsAssignableFrom(type))
			{
				var entries = Enum.GetNames(type);
				host.UpdateEnum(type.FullName, entries[0], entries);
			}

			IEnumIn result = null;
			host.CreateEnumInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out result);
			
			if(!string.IsNullOrWhiteSpace(attribute.DefaultEnumEntry))
                result.SetDefaultEntry(attribute.DefaultEnumEntry);
			
			if (!typeof(EnumEntry).IsAssignableFrom(type))
				result.SetSubType(type.FullName);
			else
				result.SetSubType(attribute.EnumName);
			
            SetInputProperties(result, attribute);

            return result;
		}
		
		public static IEnumOut CreateEnumOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
		    
		    //this creates and sets the default for a .NET enum
			if (!typeof(EnumEntry).IsAssignableFrom(type))
			{
				var entries = Enum.GetNames(type);
				host.UpdateEnum(type.FullName, entries[0], entries);
			}
			
			IEnumOut result = null;
			host.CreateEnumOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out result);
			if (!typeof(EnumEntry).IsAssignableFrom(type))
				result.SetSubType(type.FullName);
			else
				result.SetSubType(attribute.EnumName);		
			
            SetOutputProperties(result, attribute);
			return result;
		}
		
		public static ITransformIn CreateTransformInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			ITransformIn result = null;
			host.CreateTransformInput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
            SetInputProperties(result, attribute);
			return result;
		}
		
		public static ITransformOut CreateTransformOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
			ITransformOut result = null;
			host.CreateTransformOutput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
            SetOutputProperties(result, attribute);
			return result;
		}
		
		public static INodeIn CreateNodeInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			INodeIn result = null;
			host.CreateNodeInput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
            if (type != null)
                result.SetSubType2(type, new Guid[] { type.GUID }, type.GetCSharpName());
            else
                result.SetSubType(new Guid[] { }, "Variant");
            SetInputProperties(result, attribute);
			return result;
		}
		
		public static INodeOut CreateNodeOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
			INodeOut result = null;
			host.CreateNodeOutput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
			
            if (type != null)
            {
                // Register all implemented interfaces and inherited classes of T
                // to support the assignment of ISpread<Apple> output to ISpread<Fruit> input.
                var guids = new List<Guid>();

                RegisterID(host, guids, type);

                foreach (var interf in type.GetInterfaces())
                    RegisterID(host, guids, interf);

                var t = type.BaseType;
                while (t != null)
                {
                    RegisterID(host, guids, t);
                    t = t.BaseType;
                }

                result.SetSubType2(type, guids.ToArray(), type.GetCSharpName());
            }
            else
                result.SetSubType(new Guid[] { }, "Variant");

            SetOutputProperties(result, attribute);
			return result;
		}

        private static void RegisterID(IPluginHost host, List<Guid> guids, Type t)
        {
            Guid id = t.GUID;
            guids.Add(id);
            host.RegisterType(id, t.GetCSharpName());
        }

        public static IRawIn CreateRawInput(this IPluginHost host, InputAttribute attribute)
        {
            IRawIn result = null;
            host.CreateRawInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out result);
            SetInputProperties(result, attribute);
            return result;
        }

        public static IRawOut CreateRawOutput(this IPluginHost host, OutputAttribute attribute)
        {
            IRawOut result = null;
            host.CreateRawOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out result);
            SetOutputProperties(result, attribute);
            return result;
        }
		
		public static IDXMeshOut CreateMeshOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
			IDXMeshOut result = null;
			host.CreateMeshOutput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
            SetOutputProperties(result, attribute);
			return result;
		}
		
		public static IDXTextureOut CreateTextureOutput(this IPluginHost host, OutputAttribute attribute, Type type)
		{
			IDXTextureOut result = null;
			host.CreateTextureOutput(attribute.Name, (TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
            SetOutputProperties(result, attribute);
			return result;
		}
		
		public static IDXRenderStateIn CreateRenderStateInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			IDXRenderStateIn result = null;
			host.CreateRenderStateInput((TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
            SetInputProperties(result, attribute);
			return result;
		}
		
		public static IDXSamplerStateIn CreateSamplerStateInput(this IPluginHost host, InputAttribute attribute, Type type)
		{
			IDXSamplerStateIn result = null;
			host.CreateSamplerStateInput((TSliceMode) attribute.SliceMode, (TPinVisibility) attribute.Visibility, out result);
            SetInputProperties(result, attribute);
			return result;
		}

        private static void SetIOProperties(IPluginIO pin, IOAttribute attribute)
        {
            if (attribute.Order != pin.Order)
                pin.Order = attribute.Order;
        }

        private static void SetInputProperties(IPluginIn input, InputAttribute attribute)
        {
            SetIOProperties(input, attribute);
            input.AutoValidate = attribute.AutoValidate;
        }

        private static void SetInputProperties(IPluginFastIn input, InputAttribute attribute)
        {
            SetIOProperties(input, attribute);
            input.AutoValidate = attribute.AutoValidate;
        }

        private static void SetOutputProperties(IPluginOut output, OutputAttribute attribute)
        {
            SetIOProperties(output, attribute);
            output.AllowFeedback = attribute.AllowFeedback;
        }
	}
}
