using System;

namespace VVVV.Utils.Animation
{
	/// <summary>
	/// Basic 1-pole IIR filter with threshold.
	/// </summary>
	public struct IIRFilter
	{
		public double Value;
		public double Alpha;
		public double Thresh;
		
		/// <summary>
		/// Update filter state. If the difference between 
		/// the current filter value and the new value is greater than 
		/// the threshold then filter value is set to new value.
		/// </summary>
		/// <param name="newValue">Target value</param>
		/// <returns>Filtered target value</returns>
		public double Update(double newValue)
		{
			var diff = Math.Abs(newValue - Value);
			Value = diff > Thresh ? newValue : newValue * (1-Alpha) + Value * Alpha;
			return Value;
		}
	}
}
