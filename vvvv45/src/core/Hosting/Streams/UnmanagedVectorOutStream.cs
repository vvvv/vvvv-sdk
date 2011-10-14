
using System;
using SlimDX;
using VVVV.Utils.SlimDX;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams
{
	unsafe abstract class VectorOutStream<T> : IOutStream<T> where T : struct
	{
		private readonly Func<int, IntPtr> FResizeUnmanagedArrayFunc;
		protected readonly int FDimension;
		protected double* FUnmanagedArray;
		protected double* FWritePointer;
		protected int FUnmanagedLength;
		protected int FLength;
		protected int FUnderFlow;
		protected int FWritePosition;
		
		public VectorOutStream(int dimension, Func<int, IntPtr> resizeUnmanagedArrayFunc)
		{
			FDimension = dimension;
			FResizeUnmanagedArrayFunc = resizeUnmanagedArrayFunc;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public int Length
		{
			get
			{
				return FLength;
			}
			set
			{
				if (value != FLength)
				{
					FUnmanagedArray = (double*) FResizeUnmanagedArrayFunc(value * FDimension).ToPointer();
					FLength = value;
					FUnmanagedLength = value * FDimension;
					FUnderFlow = 0;
				}
			}
		}
		
		public int WritePosition
		{
			get
			{
				return FWritePosition;
			}
			set
			{
				FWritePosition = value;
				FWritePointer = FUnmanagedArray + value * FDimension;
			}
		}
		
		public bool Eof
		{
			get
			{
				return FWritePosition >= FLength;
			}
		}
		
		public abstract void Write(T value, int stepSize = 1);
		
		public abstract int Write(T[] buffer, int index, int length, int stepSize = 1);
		
		public void Reset()
		{
			FWritePosition = 0;
			FWritePointer = FUnmanagedArray;
		}
		
		public void Flush()
		{
			
		}
		
		protected bool IsOutOfBounds(int numSlicesToWorkOn)
		{
			return (FUnderFlow > 0) && ((FWritePosition + numSlicesToWorkOn) > (FLength - 1));
		}
	}
	
	unsafe class Vector2DOutStream : VectorOutStream<Vector2D>
	{
		public Vector2DOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(2, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override int Write(Vector2D[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			
			fixed (Vector2D* source = buffer)
			{
				Vector2D* dst = (Vector2D*) FWritePointer;
				Vector2D* src = source + index;
				
				int numSlicesToWriteAtFullSpeed = numSlicesToWrite;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToWrite))
				{
					numSlicesToWriteAtFullSpeed = Math.Max(numSlicesToWriteAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToWriteAtFullSpeed; i++)
				{
					*dst = *(src++);
					dst += stepSize;
				}
				
				if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->x;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->y;
				}
			}
			
			FWritePosition += numSlicesToWrite * stepSize;
			FWritePointer += numSlicesToWrite * stepSize * FDimension;
			
			return numSlicesToWrite;
		}
		
		public override void Write(Vector2D value, int stepSize)
		{
			if (IsOutOfBounds(1))
			{
				int i = FUnmanagedLength - FUnderFlow;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.x;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.y;
			}
			else
			{
				*((Vector2D*) FWritePointer) = value;
			}
			
			FWritePosition += stepSize;
			FWritePointer += stepSize * FDimension;
		}
	}
	
	unsafe class Vector3DOutStream : VectorOutStream<Vector3D>
	{
		public Vector3DOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(3, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override int Write(Vector3D[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			
			fixed (Vector3D* source = buffer)
			{
				Vector3D* dst = (Vector3D*) FWritePointer;
				Vector3D* src = source + index;
				
				int numSlicesToWriteAtFullSpeed = numSlicesToWrite;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToWrite))
				{
					numSlicesToWriteAtFullSpeed = Math.Max(numSlicesToWriteAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToWriteAtFullSpeed; i++)
				{
					*dst = *(src++);
					dst += stepSize;
				}
				
				if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->x;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->y;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->z;
				}
			}
			
			FWritePosition += numSlicesToWrite * stepSize;
			FWritePointer += numSlicesToWrite * stepSize * FDimension;
			
			return numSlicesToWrite;
		}
		
		public override void Write(Vector3D value, int stepSize)
		{
			if (IsOutOfBounds(1))
			{
				int i = FUnmanagedLength - FUnderFlow;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.x;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.y;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.z;
			}
			else
			{
				*((Vector3D*) FWritePointer) = value;
			}
			
			FWritePosition += stepSize;
			FWritePointer += stepSize * FDimension;
		}
	}
	
	unsafe class Vector4DOutStream : VectorOutStream<Vector4D>
	{
		public Vector4DOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(4, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override int Write(Vector4D[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			
			fixed (Vector4D* source = buffer)
			{
				Vector4D* dst = (Vector4D*) FWritePointer;
				Vector4D* src = source + index;
				
				int numSlicesToWriteAtFullSpeed = numSlicesToWrite;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToWrite))
				{
					numSlicesToWriteAtFullSpeed = Math.Max(numSlicesToWriteAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToWriteAtFullSpeed; i++)
				{
					*dst = *(src++);
					dst += stepSize;
				}
				
				if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->x;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->y;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->z;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->w;
				}
			}
			
			FWritePosition += numSlicesToWrite * stepSize;
			FWritePointer += numSlicesToWrite * stepSize * FDimension;
			
			return numSlicesToWrite;
		}
		
		public override void Write(Vector4D value, int stepSize)
		{
			if (IsOutOfBounds(1))
			{
				int i = FUnmanagedLength - FUnderFlow;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.x;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.y;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.z;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.w;
			}
			else
			{
				*((Vector4D*) FWritePointer) = value;
			}
			
			FWritePosition += stepSize;
			FWritePointer += stepSize * FDimension;
		}
	}
	
	unsafe class ColorOutStream : VectorOutStream<RGBAColor>
	{
		public ColorOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(4, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override int Write(RGBAColor[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			
			fixed (RGBAColor* source = buffer)
			{
				RGBAColor* dst = (RGBAColor*) FWritePointer;
				RGBAColor* src = source + index;
				
				int numSlicesToWriteAtFullSpeed = numSlicesToWrite;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToWrite))
				{
					numSlicesToWriteAtFullSpeed = Math.Max(numSlicesToWriteAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToWriteAtFullSpeed; i++)
				{
					*dst = *(src++);
					dst += stepSize;
				}
				
				if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->R;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->G;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->B;
					FUnmanagedArray[i++ % FUnmanagedLength] = src->A;
				}
			}
			
			FWritePosition += numSlicesToWrite * stepSize;
			FWritePointer += numSlicesToWrite * stepSize * FDimension;
			
			return numSlicesToWrite;
		}
		
		public override void Write(RGBAColor value, int stepSize)
		{
			if (IsOutOfBounds(1))
			{
				int i = FUnmanagedLength - FUnderFlow;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.R;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.G;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.B;
				FUnmanagedArray[i++ % FUnmanagedLength] = value.A;
			}
			else
			{
				*((RGBAColor*) FWritePointer) = value;
			}
			
			FWritePosition += stepSize;
			FWritePointer += stepSize * FDimension;
		}
	}
	
	unsafe class Vector2OutStream : VectorOutStream<Vector2>
	{
		public Vector2OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(2, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override int Write(Vector2[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			
			fixed (Vector2* source = buffer)
			{
				Vector2D* dst = (Vector2D*) FWritePointer;
				Vector2* src = source + index;
				
				int numSlicesToWriteAtFullSpeed = numSlicesToWrite;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToWrite))
				{
					numSlicesToWriteAtFullSpeed = Math.Max(numSlicesToWriteAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToWriteAtFullSpeed; i++)
				{
					*dst = (*(src++)).ToVector2D();
					dst += stepSize;
				}
				
				if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->X;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->Y;
				}
			}
			
			FWritePosition += numSlicesToWrite * stepSize;
			FWritePointer += numSlicesToWrite * stepSize * FDimension;
			
			return numSlicesToWrite;
		}
		
		public override void Write(Vector2 value, int stepSize)
		{
			if (IsOutOfBounds(1))
			{
				int i = FUnmanagedLength - FUnderFlow;
				FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.X;
				FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.Y;
			}
			else
			{
				*((Vector2D*) FWritePointer) = value.ToVector2D();
			}
			
			FWritePosition += stepSize;
			FWritePointer += stepSize * FDimension;
		}
	}
	
	unsafe class Vector3OutStream : VectorOutStream<Vector3>
	{
		public Vector3OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(3, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override int Write(Vector3[] buffer, int index, int length, int stepSize)
		{
			throw new NotImplementedException();
		}
		
		public override void Write(Vector3 value, int stepSize)
		{
			throw new NotImplementedException();
		}
	}
	
	unsafe class Vector4OutStream : VectorOutStream<Vector4>
	{
		public Vector4OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(4, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override int Write(Vector4[] buffer, int index, int length, int stepSize)
		{
			throw new NotImplementedException();
		}
		
		public override void Write(Vector4 value, int stepSize)
		{
			throw new NotImplementedException();
		}
	}
}
