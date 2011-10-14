
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams
{
	/// <summary>
	/// Description of UnmanagedOutStream.
	/// </summary>
	unsafe abstract class UnmanagedOutStream<T> : IIOStream<T>
	{
		private readonly Func<int, IntPtr> FResizeUnmanagedArrayFunc;
		private readonly IInStream<T> FInStream;
		protected IntPtr FUnmanagedArrayPtr;
		protected int FLength;
		protected int FWritePosition;
		
		public UnmanagedOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
		{
			FResizeUnmanagedArrayFunc = resizeUnmanagedArrayFunc;
			FInStream = UnmanagedInStream<T>.Create(() => Tuple.Create(FUnmanagedArrayPtr, FLength), () => {});
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
			}
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
					FUnmanagedArrayPtr = FResizeUnmanagedArrayFunc(value);
					FLength = value;
					FInStream.Sync();
					Resized(FUnmanagedArrayPtr, FLength);
				}
			}
		}
		
		public bool Eof
		{
			get
			{
				return FWritePosition >= FLength || FInStream.Eof;
			}
		}
		
		public abstract void Write(T value, int stepSize);
		
		protected abstract void Copy(T[] source, int sourceIndex, int length, int stepSize);
		
		protected abstract void Resized(IntPtr unmanagedArray, int unmanagedArrayLength);
		
		public int Write(T[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			
			switch (stepSize)
			{
				case 0:
					Write(buffer[index + numSlicesToWrite - 1], stepSize);
					break;
				default:
					Debug.Assert(FWritePosition + numSlicesToWrite <= Length);
					Copy(buffer, index, numSlicesToWrite, stepSize);
					FWritePosition += numSlicesToWrite * stepSize;
					break;
			}
			
			return numSlicesToWrite;
		}
		
		public void Flush()
		{
			// We write to the unmanaged array directly. Nothing to do here.
		}
		
		public void Reset()
		{
			FWritePosition = 0;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public int ReadPosition
		{
			get
			{
				return FInStream.ReadPosition;
			}
			set
			{
				FInStream.ReadPosition = value;
			}
		}
		
		public T Read(int stepSize)
		{
			return FInStream.Read(stepSize);
		}
		
		public int Read(T[] buffer, int index, int length, int stepSize)
		{
			return FInStream.Read(buffer, index, length, stepSize);
		}
		
		public void ReadCyclic(T[] buffer, int index, int length, int stepSize)
		{
			StreamUtils.ReadCyclic(this, buffer, index, length, stepSize);
		}
		
		public void Sync()
		{
			// We read what the user wrote. So no need to sync here with external data.
		}
	}
	
	unsafe class DoubleOutStream : UnmanagedOutStream<double>
	{
		private double* FUnmanagedArray;
		
		public DoubleOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override void Write(double value, int stepSize)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = value;
			FWritePosition += stepSize;
		}
		
		protected override void Copy(double[] source, int sourceIndex, int length, int stepSize)
		{
			switch (stepSize)
			{
				case 1:
					Marshal.Copy(source, sourceIndex, FUnmanagedArrayPtr + FWritePosition * sizeof(double), length);
					break;
				default:
					fixed (double* sourcePtr = source)
					{
						double* src = sourcePtr + sourceIndex;
						double* dst = FUnmanagedArray + FWritePosition;
						
						for (int i = 0; i < length; i++)
						{
							*dst = *(src++);
							dst += stepSize;
						}
					}
					break;
			}
		}
	}
	
	unsafe class FloatOutStream : UnmanagedOutStream<float>
	{
		private double* FUnmanagedArray;
		
		public FloatOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override void Write(float value, int stepSize)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = (double) value;
			FWritePosition += stepSize;
		}
		
		protected override void Copy(float[] source, int sourceIndex, int length, int stepSize)
		{
			fixed (float* sourcePtr = source)
			{
				float* src = sourcePtr + sourceIndex;
				double* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = (double) *(src++);
					dst += stepSize;
				}
			}
		}
	}

	unsafe class IntOutStream : UnmanagedOutStream<int>
	{
		private double* FUnmanagedArray;
		
		public IntOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override void Write(int value, int stepSize)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = (double) value;
			FWritePosition += stepSize;
		}
		
		protected override void Copy(int[] source, int sourceIndex, int length, int stepSize)
		{
			fixed (int* sourcePtr = source)
			{
				int* src = sourcePtr + sourceIndex;
				double* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = (double) *(src++);
					dst += stepSize;
				}
			}
		}
	}

	unsafe class BoolOutStream : UnmanagedOutStream<bool>
	{
		private double* FUnmanagedArray;
		
		public BoolOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override void Write(bool value, int stepSize)
		{
			FUnmanagedArray[FWritePosition] = value ? 1.0 : 0.0;
			FWritePosition += stepSize;
		}
		
		protected override void Copy(bool[] source, int sourceIndex, int length, int stepSize)
		{
			fixed (bool* sourcePtr = source)
			{
				bool* src = sourcePtr + sourceIndex;
				double* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = *(src++) ? 1.0 : 0.0;
					dst += stepSize;
				}
			}
		}
	}

	unsafe class MatrixOutStream : UnmanagedOutStream<Matrix>
	{
		private Matrix* FUnmanagedArray;
		
		public MatrixOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override void Write(Matrix value, int stepSize)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = value;
			FWritePosition += stepSize;
		}
		
		protected override void Copy(Matrix[] source, int sourceIndex, int length, int stepSize)
		{
			fixed (Matrix* sourcePtr = source)
			{
				Matrix* src = sourcePtr + sourceIndex;
				Matrix* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = *(src++);
					dst += stepSize;
				}
			}
		}
	}

	unsafe class Matrix4x4OutStream : UnmanagedOutStream<Matrix4x4>
	{
		private Matrix* FUnmanagedArray;
		
		public Matrix4x4OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override void Write(Matrix4x4 value, int stepSize)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = value.ToSlimDXMatrix();
			FWritePosition += stepSize;
		}
		
		protected override void Copy(Matrix4x4[] source, int sourceIndex, int length, int stepSize)
		{
			fixed (Matrix4x4* sourcePtr = source)
			{
				Matrix4x4* src = sourcePtr + sourceIndex;
				Matrix* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = (*(src++)).ToSlimDXMatrix();
					dst += stepSize;
				}
			}
		}
	}
}
