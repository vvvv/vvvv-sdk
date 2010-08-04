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
			Name = name;
			Visibility = TPinVisibility.True;
			SliceMode = TSliceMode.Dynamic;
			
			IsFilename = false;
			MinValue = DefaultMinValue;
			MaxValue = DefaultMaxValue;
			StepSize = DefaultStepSize;
			
			DefaultValues = new double[] { 0.0, 0.0, 0.0, 0.0 };
			
			EnumName = "Empty";
			DefaultEnumEntry = "";
		}
		
		public string Name
		{
			get;
			set;
		}
		
		public TPinVisibility Visibility
		{
			get;
			set;
		}
		
		public TSliceMode SliceMode
		{
			get;
			set;
		}
		
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
		
		public string DefaultString
		{
			get;
			set;
		}
		
		public bool IsFilename
		{
			get;
			set;
		}
		
		public double[] DefaultValues
		{
			get;
			set;
		}
		
		public bool HasAlpha
		{
			get;
			set;
		}
		
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
	}
}
