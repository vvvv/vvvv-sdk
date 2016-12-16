using System;
using System.Collections;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using SlimDX;

namespace Hoster
{
	public class TTransformInPin: TBasePin, ITransformIn, IPluginConfig
	{
		public double[] FValues;
		private double FMin, FMax, FDefault, FStepSize;
		private bool FIsInteger;
		
		public TTransformInPin(IPluginHost Parent, string PinName, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, TPinDirection.Input, null, SliceMode, Visibility)
		{
		}
		
		public void GetMatrix(int Index, out Matrix4x4 Value)
		{
			Value = VMath.IdentityMatrix;
		}
		
		public void SetRenderSpace()
		{
			
		}

		public void GetRenderWorldMatrix(int Index, out Matrix4x4 Value)
		{
			Value = VMath.IdentityMatrix;
		}
		
		public void GetRenderWorldMatrix(int Index, [Out, MarshalAs(UnmanagedType.Struct)] out Matrix Value)
		{
			Value = Matrix.Identity;
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
			
			FPinIsChanged = true;
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
				FSliceCount = 0;
			}
			else
			{
				char[] s = {','};
				string[] slices = Spread.Split(s);
				
				FSliceCount = (int) slices.Length / FDimension;
				FValues = new double[FSliceCount * FDimension];
				
				
				System.Globalization.NumberFormatInfo nf = new System.Globalization.NumberFormatInfo();
				nf.NumberDecimalSeparator = ".";
				
				string tmp;
				for (int i=0; i<slices.Length; i++)
				{
					tmp = slices[i];
					//tmp = tmp.Replace('.', ',');
					double d = System.Convert.ToDouble(tmp, nf);
					FValues[i] = d;
					//SetValue(i, System.Convert.ToDouble(tmp));
				}
			}
			
			FSliceCountIsChanged = true;
			FPinIsChanged = true;
			
			if (FOnConfigurate != null)
				FOnConfigurate(this);
		}
		
		unsafe public void GetMatrixPointer(out int SliceCount, out float* ValueP)
		{
			//todo: return sth meaningfull
			var f = (float) FValues[0];
			//fixed(float* p = &f)
			ValueP = &f;
			SliceCount = 0;
		}
		
		unsafe public void GetMatrixPointer(out int* sliceCount, out float** ValueP)
		{
			//TODO: not implemented
			sliceCount = (int*)0;
			ValueP = (float**)0;
		}
	}	
	
	
	public class TTransformOutPin: TBasePin, ITransformOut, IPluginConfig
	{
		public double[] FValues;
		private double FMin, FMax, FDefault, FStepSize;
		private bool FIsInteger;
		
		public TTransformOutPin(IPluginHost Parent, string PinName, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, TPinDirection.Output, null, SliceMode, Visibility)
		{
		}
		
		public void SetMatrix(int Index, Matrix4x4 Value)
		{
			
		}
		
		public void GetMatrix(int Index, out Matrix4x4 Value)
		{
			Value = new Matrix4x4();
		}
		
		unsafe public void GetMatrixPointer(out float** ValueP)
		{
			//TODO: not implemented
			ValueP = (float**)0;
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

			FPinIsChanged = true;
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
				FSliceCount = 0;
			}
			else
			{
				char[] s = {','};
				string[] slices = Spread.Split(s);
				
				FSliceCount = (int) slices.Length / FDimension;
				FValues = new double[FSliceCount * FDimension];
				
				
				System.Globalization.NumberFormatInfo nf = new System.Globalization.NumberFormatInfo();
				nf.NumberDecimalSeparator = ".";
				
				string tmp;
				for (int i=0; i<slices.Length; i++)
				{
					tmp = slices[i];
					//tmp = tmp.Replace('.', ',');
					double d = System.Convert.ToDouble(tmp, nf);
					FValues[i] = d;
					//SetValue(i, System.Convert.ToDouble(tmp));
				}
			}
			
			FSliceCountIsChanged = true;
			FPinIsChanged = true;
			
			if (FOnConfigurate != null)
				FOnConfigurate(this);
		}
		
		unsafe public void GetMatrixPointer(out float* ValueP)
		{
			//todo: return sth meaningfull
			var f = (float) FValues[0];
			//fixed(float* p = &f)
			ValueP = &f;
			SliceCount = 0;
		}
	}	
}
