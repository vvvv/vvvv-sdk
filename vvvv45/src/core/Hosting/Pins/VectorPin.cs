using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// Base class for VectorInputPin and VectorOutputPin
	/// </summary>
	[ComVisible(false)]
	public abstract class VectorPin<T> : ValuePin<T> where T: struct
	{
		protected int FDimension;
		protected double[] FDefaultValues;
		protected string[] FDimensionNames;
		
		public VectorPin(IPluginHost host, PinAttribute attribute, int dimension, double minValue, double maxValue, double stepSize)
			: base(host, attribute, minValue, maxValue, stepSize)
		{
			FDimension = dimension;
			
			FDefaultValues = FAttribute.DefaultValues;
			if(FDefaultValues.Length < dimension)
			{
				var newDefaults = new double[dimension];
				for (int i = 0; i < FDefaultValues.Length; i++)
					newDefaults[i] = FAttribute.DefaultValues[i];
				
				FDefaultValues = newDefaults;
			}
			
			FDimensionNames = FAttribute.DimensionNames;
			if (FDimensionNames != null)
			{
				if(FDimensionNames.Length < dimension)
				{
					var newNames = new string[dimension];
					for (int i=0; i<dimension; i++)
					{
						if (i < FDimensionNames.Length)
							newNames[i] = FDimensionNames[i];
						else
							newNames[i] = "";
					}
					
					FDimensionNames = newNames;
				}
			}
		}
	}
	
	/// <summary>
	/// Base class for DiffVectorInputPin and VectorConfigPin
	/// </summary>
	[ComVisible(false)]
	public abstract class DiffVectorPin<T> : DiffValuePin<T> where T: struct
	{
		protected int FDimension;
		protected double[] FDefaultValues;
		protected string[] FDimensionNames;
		
		public DiffVectorPin(IPluginHost host, PinAttribute attribute, int dimension, double minValue, double maxValue, double stepSize)
			: base(host, attribute, minValue, maxValue, stepSize)
		{
			FDimension = dimension;
			
			FDefaultValues = FAttribute.DefaultValues;
			if(FDefaultValues.Length < dimension)
			{
				var newDefaults = new double[dimension];
				for (int i = 0; i < FDefaultValues.Length; i++)
					newDefaults[i] = FAttribute.DefaultValues[i];
				
				FDefaultValues = newDefaults;
			}
			
			FDimensionNames = FAttribute.DimensionNames;
			if (FDimensionNames != null)
			{
				if(FDimensionNames.Length < dimension)
				{
					var newNames = new string[dimension];
					for (int i=0; i<dimension; i++)
					{
						if (i < FDimensionNames.Length)
							newNames[i] = FDimensionNames[i];
						else
							newNames[i] = "";
					}
					
					FDimensionNames = newNames;
				}
			}
		}
	}
}
