using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public abstract class VectorInputPin<T> : VectorPin<T> where T: struct
	{
		protected IValueFastIn FValueFastIn;
		
		public VectorInputPin(IPluginHost host, InputAttribute attribute, int dimension, double minValue, double maxValue, double stepSize)
			: base(host, attribute, dimension, minValue, maxValue, stepSize)
		{
			host.CreateValueFastInput(FName, FDimension, FDimensionNames, FSliceMode, FVisibility, out FValueFastIn);
			switch (FDimension)
			{
				case 2:
					FValueFastIn.SetSubType2D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FIsBang, FIsToggle, FIsInteger);
					break;
				case 3:
					FValueFastIn.SetSubType3D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FIsBang, FIsToggle, FIsInteger);
					break;
				case 4:
					FValueFastIn.SetSubType4D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FDefaultValues[3], FIsBang, FIsToggle, FIsInteger);
					break;
			}
			
			base.InitializeInternalPin(FValueFastIn);
		}
		
		unsafe protected abstract void CopyToBuffer(T[] buffer, double* source, int length, int underFlow);
		
		unsafe public override void Update()
		{
			int length;
			double* source;
			
			FValueFastIn.GetValuePointer(out length, out source);
			
			var underFlow = length % FDimension;
			if (underFlow != 0)
				SliceCount = length / FDimension + 1;
			else
				SliceCount = length / FDimension;
			
			if (FSliceCount > 0)
				CopyToBuffer(FBuffer, source, length, underFlow);
			
			base.Update();
		}
	}
	
	[ComVisible(false)]
	public abstract class DiffVectorInputPin<T> : DiffVectorPin<T> where T: struct
	{
		protected IValueIn FValueIn;
		
		public DiffVectorInputPin(IPluginHost host, InputAttribute attribute, int dimension, double minValue, double maxValue, double stepSize)
			: base(host, attribute, dimension, minValue, maxValue, stepSize)
		{
			host.CreateValueInput(FName, FDimension, FDimensionNames, FSliceMode, FVisibility, out FValueIn);
			switch (FDimension)
			{
				case 2:
					FValueIn.SetSubType2D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FIsBang, FIsToggle, FIsInteger);
					break;
				case 3:
					FValueIn.SetSubType3D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FIsBang, FIsToggle, FIsInteger);
					break;
				case 4:
					FValueIn.SetSubType4D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FDefaultValues[3], FIsBang, FIsToggle, FIsInteger);
					break;
			}
			
			base.InitializeInternalPin(FValueIn);
		}
		
		unsafe protected abstract void CopyToBuffer(T[] buffer, double* source, int length, int underFlow);
		
		unsafe protected override void DoUpdate()
		{
			int length;
			double* source;
			
			FValueIn.GetValuePointer(out length, out source);
			
			var underFlow = length % FDimension;
			if (underFlow != 0)
				SliceCount = length / FDimension + 1;
			else
				SliceCount = length / FDimension;
			
			if (FSliceCount > 0)
				CopyToBuffer(FBuffer, source, length, underFlow);
		}
		
		protected override bool IsInternalPinChanged
		{
			get
			{
				return FValueIn.PinIsChanged;
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector2DInputPin : VectorInputPin<Vector2D>
	{
		public Vector2DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 2, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector2D[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector2D* destination = buffer)
			{
				Vector2D* dst = destination;
				Vector2D* src = (Vector2D*) source;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->x = *(source + i++ % length);
					dst->y = *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector3DInputPin : VectorInputPin<Vector3D>
	{
		public Vector3DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 3, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector3D[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector3D* destination = buffer)
			{
				Vector3D* dst = destination;
				Vector3D* src = (Vector3D*) source;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->x = *(source + i++ % length);
					dst->y = *(source + i++ % length);
					dst->z = *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector4DInputPin : VectorInputPin<Vector4D>
	{
		public Vector4DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 4, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector4D[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector4D* destination = buffer)
			{
				Vector4D* dst = destination;
				Vector4D* src = (Vector4D*) source;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->x = *(source + i++ % length);
					dst->y = *(source + i++ % length);
					dst->z = *(source + i++ % length);
					dst->w = *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffVector2DInputPin : DiffVectorInputPin<Vector2D>
	{
		public DiffVector2DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 2, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector2D[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector2D* destination = buffer)
			{
				Vector2D* dst = destination;
				Vector2D* src = (Vector2D*) source;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->x = *(source + i++ % length);
					dst->y = *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffVector3DInputPin : DiffVectorInputPin<Vector3D>
	{
		public DiffVector3DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 3, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector3D[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector3D* destination = buffer)
			{
				Vector3D* dst = destination;
				Vector3D* src = (Vector3D*) source;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->x = *(source + i++ % length);
					dst->y = *(source + i++ % length);
					dst->z = *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffVector4DInputPin : DiffVectorInputPin<Vector4D>
	{
		public DiffVector4DInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 4, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector4D[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector4D* destination = buffer)
			{
				Vector4D* dst = destination;
				Vector4D* src = (Vector4D*) source;
				
				for (int i = 0; i < length / FDimension; i++)
					*(dst++) = *(src++);
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->x = *(source + i++ % length);
					dst->y = *(source + i++ % length);
					dst->z = *(source + i++ % length);
					dst->w = *(source + i++ % length);
				}
			}
		}
	}
}
