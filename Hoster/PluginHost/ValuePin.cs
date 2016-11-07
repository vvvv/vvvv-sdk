using System;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace Hoster
{
	public class TValuePin: TBasePin, IValueIn, IValueConfig, IValueFastIn, IValueOut
	{
		public double[] FValues;
		private double FMin, FMax, FDefault, FStepSize;
		private bool FIsInteger;
		private string[] FDimensionNames;
		
		public TValuePin(IPluginHost Parent, string PinName, int Dimension, string[] DimensionNames, TPinDirection PinDirection, TOnConfigurate Callback, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, Dimension, PinDirection, Callback, SliceMode, Visibility)
		{
			FDimension = Dimension; 
			FDimensionNames = DimensionNames; 
		}
		
		/* //indexed properties don't seem to work without overhead to native code
		public double this[int i]
		{
			get { return FValues[i % FSliceCount]; }
			set 
			{ 
				if (value != FValues[i])
				{
					FValues[i] = value;
					FPinIsChanged = true;
				}
			}
		}*/
		
		public void GetValue(int Index, out double Value)
		{
			Value = FValues[Index % FSliceCount]; 
		}
		
		public void GetValue2D(int Index, out double Value1, out double Value2)
		{
			int idx = (Index % FSliceCount) * 2;
			Value1 = FValues[idx]; 
			Value2 = FValues[idx+1]; 
		}

		public void GetValue3D(int Index, out double Value1, out double Value2, out double Value3)
		{
			int idx = (Index % FSliceCount) * 3;
			Value1 = FValues[idx]; 
			Value2 = FValues[idx+1]; 
			Value3 = FValues[idx+2]; 
		}
		
		public void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4)
		{
			int idx = (Index % FSliceCount) * 4;
			Value1 = FValues[idx]; 
			Value2 = FValues[idx+1]; 
			Value3 = FValues[idx+2]; 
			Value4 = FValues[idx+3]; 
		}
		
		public void SetValue(int Index, double Value)
		{
			if (Value != FValues[Index])
			{
				FValues[Index] = Math.Max(FMin, Math.Min(FMax, Value));
				FPinIsChanged = true;
			}		
			
			if ((FPinIsChanged) && (FOnConfigurate != null))
				FOnConfigurate(this);
		}
		
		public void SetValue2D(int Index, double Value1, double Value2)
		{
			if ((Value1 != FValues[Index]) || (Value2 != FValues[Index+1]))
			{
				int idx = Index*2;
				FValues[idx] = Math.Max(FMin, Math.Min(FMax, Value1));
				FValues[idx+1] = Math.Max(FMin, Math.Min(FMax, Value2));
				FPinIsChanged = true;
			}		
			
			if ((FPinIsChanged) && (FOnConfigurate != null))
				FOnConfigurate(this);
		}
		
		public void SetValue3D(int Index, double Value1, double Value2, double Value3)
		{
			if ((Value1 != FValues[Index]) || (Value2 != FValues[Index+1]) || (Value3 != FValues[Index+2]))
			{
				int idx = Index*3;
				FValues[idx] = Math.Max(FMin, Math.Min(FMax, Value1));
				FValues[idx+1] = Math.Max(FMin, Math.Min(FMax, Value2));
				FValues[idx+2] = Math.Max(FMin, Math.Min(FMax, Value3));
				FPinIsChanged = true;
			}		
			
			if ((FPinIsChanged) && (FOnConfigurate != null))
				FOnConfigurate(this);
		}
		
		public void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4)
		{
			if ((Value1 != FValues[Index]) || (Value2 != FValues[Index+1]) || (Value3 != FValues[Index+2]) || (Value4 != FValues[Index+3]))
			{
				int idx = Index*4;
				FValues[idx] = Math.Max(FMin, Math.Min(FMax, Value1));
				FValues[idx+1] = Math.Max(FMin, Math.Min(FMax, Value2));
				FValues[idx+2] = Math.Max(FMin, Math.Min(FMax, Value3));
				FValues[idx+3] = Math.Max(FMin, Math.Min(FMax, Value4));
				FPinIsChanged = true;
			}		
			
			if ((FPinIsChanged) && (FOnConfigurate != null))
				FOnConfigurate(this);
		}
		
		public void SetMatrix(int Index, Matrix4x4 Value)
		{
			
		}
		
		public void GetMatrix(int Index, out Matrix4x4 Value)
		{
			Value = new Matrix4x4();
		}
		
		//may only be used from the pluginhost
		public void SetDeltaValue(int Index, double Delta)
		{
			double val = FValues[Index] - Delta * FStepSize;
			SetValue(Index, val);
		}
		
		public void SetDeltaValue2D(int Index, double Delta1, double Delta2)
		{
			double val1 = FValues[Index] - Delta1 * FStepSize;
			double val2 = FValues[Index+1] - Delta2 * FStepSize;
			SetValue2D(Index, val1, val2);
		}
		
		override protected void ChangeSliceCount()
		{
			int oldCount = 0;
			if (FValues != null)
				oldCount = FValues.Length;
			double[] tmp = new double[oldCount];
			
			//save old values
			for (int i=0; i<oldCount; i++)
				tmp[i] = FValues[i];
			
			FValues = new double[FSliceCount * FDimension];
			
			//set old values to new array
			for (int i=0; i<Math.Min(FSliceCount * FDimension, oldCount); i++)
				FValues[i] = tmp[i];
			if (oldCount > 0)
			{
				for (int i=oldCount; i<FSliceCount * FDimension; i++)
					FValues[i] = tmp[oldCount-1];
			}
			else
				for (int i=oldCount; i<FSliceCount * FDimension; i++)
					FValues[i] = FDefault;
		}
		
		public void SetSubType(double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger)
		{
			FMin = Min;
			FMax = Max;
			FStepSize = StepSize;
			FDefault = Default;		
			FIsInteger = IsInteger;
		
			for (int i=0; i<FSliceCount*FDimension; i++)
				FValues[i] = Default;
				//SetValue(i, Default);
			
			FPinIsChanged = true;
		}

		public void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger)
		{
			SetSubType(Min, Max, StepSize, Default1, IsBang, IsToggle, IsInteger);

			//set defaults
			for (int i=0; i<FSliceCount; i++)
			{
				FValues[i*2] = Default1;
				FValues[i*2+1] = Default2;
				//SetValue2D(i, Default1, Default2);
			}
		}
		
		public void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger)
		{
			SetSubType(Min, Max, StepSize, Default1, IsBang, IsToggle, IsInteger);

			//set defaults
			for (int i=0; i<FSliceCount; i++)
			{
				FValues[i*3] = Default1;
				FValues[i*3+1] = Default2;
				FValues[i*3+2] = Default3;
				//SetValue3D(i, Default1, Default2, Default3);
			}
		}
		
		public void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger)
		{
			SetSubType(Min, Max, StepSize, Default1, IsBang, IsToggle, IsInteger);

			//set defaults
			for (int i=0; i<FSliceCount; i++)
			{
				FValues[i*4] = Default1;
				FValues[i*4+1] = Default2;
				FValues[i*4+2] = Default3;
				FValues[i*4+3] = Default4;
				//SetValue4D(i, Default1, Default2, Default3, Default4);
			}				
		}
		
		unsafe public void GetValuePointer(out int SliceCount, out double* ValueP)
		{
			fixed(double* p = &FValues[0])
			{ValueP = p;}
			SliceCount = FSliceCount;
		}
		
		unsafe public void GetValuePointer(out double* ValueP)
		{
			fixed(double* p = &FValues[0])
			{ValueP = p;}
		}
		
		unsafe public void GetValuePointer(out int* sliceCount, out double** ppDst)
		{
		  //TODO: not implemented
		  sliceCount = (int*)0;
		  ppDst = (double**)0;
		}
		
		unsafe public void GetValuePointer(out double** ppDst)
		{
		  //TODO: not implemented
		  ppDst = (double**)0;
		}

		override protected string AsString(int index)
		{
			if (FIsInteger)
			{
				return FValues[index].ToString();
			}
			else
			{
				string tmp = String.Format("{0:F4}", FValues[index]);
				return tmp.Replace(',', '.');
			}
		}
		
		override public void SetSpreadAsString(string Spread)
		{
			if (Spread == "")
			{
				if (FSliceCount > 0)
				{
					FSliceCountIsChanged = true;
					FPinIsChanged = true;
				}
				FSliceCount = 0;
			}
			else
			{
				char[] s = {','};
				string[] slices = Spread.Split(s);
				
				int newSliceCount = (int) slices.Length / FDimension;
				if (newSliceCount != FSliceCount)
				{
					FSliceCount = newSliceCount;
					FValues = new double[FSliceCount * FDimension];
					FSliceCountIsChanged = true;
				}
								
				System.Globalization.NumberFormatInfo nf = new System.Globalization.NumberFormatInfo();
				nf.NumberDecimalSeparator = ".";
				
				string tmp;
				double d;
				for (int i=0; i<slices.Length; i++)
				{
					tmp = slices[i];
					try
					{
						d = System.Convert.ToDouble(tmp, nf);
					}
					catch (System.OverflowException)
					{
						//could also be double.MinValue...for now assume max. suits: timeliner lastState.EndTime
						d = double.MaxValue;
					}
					
					if (FValues[i] != d)
						FPinIsChanged = true;
					FValues[i] = d;
				}
			}

			if (FOnConfigurate != null)
				FOnConfigurate(this);
		}
	}	
}
