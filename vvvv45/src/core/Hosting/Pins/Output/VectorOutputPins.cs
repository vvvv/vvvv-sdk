using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public abstract class VectorOutputPin<T> : VectorPin<T> where T: struct
	{
		protected IValueOut FValueOut;
		
		public VectorOutputPin(IPluginHost host, OutputAttribute attribute, int dimension, double minValue, double maxValue, double stepSize)
			: base(host, attribute, dimension, minValue, maxValue, stepSize)
		{
			host.CreateValueOutput(FName, FDimension, FDimensionNames, FSliceMode, FVisibility, out FValueOut);
			switch (FDimension)
			{
				case 2:
					FValueOut.SetSubType2D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FIsBang, FIsToggle, FIsInteger);
					break;
				case 3:
					FValueOut.SetSubType3D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FIsBang, FIsToggle, FIsInteger);
					break;
				case 4:
					FValueOut.SetSubType4D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FDefaultValues[3], FIsBang, FIsToggle, FIsInteger);
					break;
			}
			
			base.InitializeInternalPin(FValueOut);
		}
		
		unsafe protected abstract void CopyFromBuffer(T[] buffer, double* destination, int length);
		
		unsafe public override void Update()
		{
			base.Update();
			
			if (FAttribute.SliceMode != SliceMode.Single)
				FValueOut.SliceCount = FSliceCount;
			
			if (FSliceCount > 0)
			{
				double* destination;
				FValueOut.GetValuePointer(out destination);
				CopyFromBuffer(FBuffer, destination, FSliceCount * FDimension);
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector2DOutputPin : VectorOutputPin<Vector2D>
	{
		public Vector2DOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, 2, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(Vector2D[] buffer, double* destination, int length)
		{
			fixed (Vector2D* source = buffer)
			{
				Vector2D* src = source;
				Vector2D* dst = (Vector2D*) destination;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector3DOutputPin : VectorOutputPin<Vector3D>
	{
		public Vector3DOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, 3, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(Vector3D[] buffer, double* destination, int length)
		{
			fixed (Vector3D* source = buffer)
			{
				Vector3D* src = source;
				Vector3D* dst = (Vector3D*) destination;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector4DOutputPin : VectorOutputPin<Vector4D>
	{
		public Vector4DOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, 4, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(Vector4D[] buffer, double* destination, int length)
		{
			fixed (Vector4D* source = buffer)
			{
				Vector4D* src = source;
				Vector4D* dst = (Vector4D*) destination;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
			}
		}
	}
}
