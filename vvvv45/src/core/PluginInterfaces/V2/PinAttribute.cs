using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class PinAttribute : ImportAttribute
	{
		public static readonly double DefaultMinValue = double.MinValue;
		public static readonly double DefaultMaxValue = double.MaxValue;
		public static readonly double DefaultStepSize = 1.0;
		
		public PinAttribute(string name)
		{
			//pin
			Name = name;
			Visibility = PinVisibility.True;
			SliceMode = SliceMode.Dynamic;
			
			//string
			StringType = StringType.String;
			FileMask = "All Files (*.*)|*.*";
			MaxChar = -1;
			DefaultString = "";
			
			//value
			MinValue = DefaultMinValue;
			MaxValue = DefaultMaxValue;
			StepSize = DefaultStepSize;
			DefaultValues = new double[] { 0.0, 0.0, 0.0, 0.0 };
			
			//color
			HasAlpha = true;
			DefaultColor = new double[] { 0.0, 1.0, 0.0, 1.0 };
			
			//enum
			EnumName = "Empty";
		}
		
		//pin
		public string Name
		{
			get;
			set;
		}
		
		public PinVisibility Visibility
		{
			get;
			set;
		}
		
		public SliceMode SliceMode
		{
			get;
			set;
		}
		
		//value
		public double MinValue
		{
			get;
			set;
		}
		
		public double MaxValue
		{
			get;
			set;
		}
		
		public double StepSize
		{
			get;
			set;
		}
		
		public double DefaultValue
		{
			get;
			set;
		}
		
		public double[] DefaultValues
		{
			get;
			set;
		}
		
		public string[] DimensionNames
		{
			get;
			set;
		}
		
		public bool IsBang
		{
			get;
			set;
		}
		
		public bool AsInt
		{
			get;
			set;
		}
		
		//string
		public string DefaultString
		{
			get;
			set;
		}
		
		public StringType StringType
		{
			get;
			set;
		}
		
		public string FileMask
		{
			get;
			set;
		}
		
		public int MaxChar
		{
			get;
			set;
		}
		
		//color
		public bool HasAlpha
		{
			get;
			set;
		}
		
		public double[] DefaultColor
		{
			get;
			set;
		}
		
		//enum
		public string EnumName
		{
			get;
			set;
		}
		
		public string DefaultEnumEntry
		{
			get;
			set;
		}
		
		//pin group
		public bool IsPinGroup
		{
			get;
			set;	
		}
	}
}
