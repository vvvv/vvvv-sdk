
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams
{
	static class UnmanagedOutStream
	{
		private static readonly Dictionary<Type, Func<Func<int, IntPtr>, object>> FStreamCreators;
		
		static UnmanagedOutStream()
		{
			FStreamCreators = new Dictionary<Type, Func<Func<int, IntPtr>, object>>();
			FStreamCreators[typeof(double)] = (resizeUnmanagedArrayFunc) =>
			{
				return new DoubleOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(float)] = (resizeUnmanagedArrayFunc) =>
			{
				return new FloatOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(int)] = (resizeUnmanagedArrayFunc) =>
			{
				return new IntOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(bool)] = (resizeUnmanagedArrayFunc) =>
			{
				return new BoolOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Vector2)] = (resizeUnmanagedArrayFunc) =>
			{
				return new Vector2OutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Vector3)] = (resizeUnmanagedArrayFunc) =>
			{
				return new Vector3OutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Vector4)] = (resizeUnmanagedArrayFunc) =>
			{
				return new Vector4OutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Vector2D)] = (resizeUnmanagedArrayFunc) =>
			{
				return new Vector2DOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Vector3D)] = (resizeUnmanagedArrayFunc) =>
			{
				return new Vector3DOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Vector4D)] = (resizeUnmanagedArrayFunc) =>
			{
				return new Vector4DOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Matrix)] = (resizeUnmanagedArrayFunc) =>
			{
				return new MatrixOutStream(resizeUnmanagedArrayFunc);
			};
			FStreamCreators[typeof(Matrix4x4)] = (resizeUnmanagedArrayFunc) =>
			{
				return new Matrix4x4OutStream(resizeUnmanagedArrayFunc);
			};
		}
		
		public static IOutStream<T> Create<T>(Func<int, IntPtr> resizeUnmanagedArrayFunc)
		{
			Func<Func<int, IntPtr>, object> streamCreator = null;
			
			if (FStreamCreators.TryGetValue(typeof(T), out streamCreator))
			{
				return streamCreator(resizeUnmanagedArrayFunc) as IOutStream<T>;
			}
			
			throw new NotSupportedException(string.Format("UnmanagedOutStream of type '{0}' is not supported.", typeof(T)));
		}
		
		public static bool CanCreate(Type type)
		{
			return FStreamCreators.ContainsKey(type);
		}
	}
	
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
			FInStream = UnmanagedInStream.Create<T>(() => Tuple.Create(FUnmanagedArrayPtr, FLength), () => { return true; });
			Resize(1);
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
					Resize(value);
				}
			}
		}
		
		private void Resize(int newSize)
		{
			FUnmanagedArrayPtr = FResizeUnmanagedArrayFunc(newSize);
			FLength = newSize;
			FInStream.Sync();
			Resized(FUnmanagedArrayPtr, FLength);
		}
		
		public bool Eof
		{
			get
			{
				return FWritePosition >= FLength || FInStream.Eof;
			}
		}
		
		public abstract void Write(T value, int stride);
		
		protected abstract void Copy(T[] source, int sourceIndex, int length, int stride);
		
		protected abstract void Resized(IntPtr unmanagedArray, int unmanagedArrayLength);
		
		public int Write(T[] buffer, int index, int length, int stride)
		{
			int numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stride);
			
			switch (stride)
			{
				case 0:
					Write(buffer[index + numSlicesToWrite - 1], stride);
					break;
				default:
					Debug.Assert(FWritePosition + numSlicesToWrite <= Length);
					Copy(buffer, index, numSlicesToWrite, stride);
					FWritePosition += numSlicesToWrite * stride;
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
		
		public T Read(int stride)
		{
			return FInStream.Read(stride);
		}
		
		public int Read(T[] buffer, int index, int length, int stride)
		{
			return FInStream.Read(buffer, index, length, stride);
		}
		
		public void ReadCyclic(T[] buffer, int index, int length, int stride)
		{
			StreamUtils.ReadCyclic(this, buffer, index, length, stride);
		}
		
		public bool Sync()
		{
			// We read what the user wrote. So no need to sync here with external data.
			return true;
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
		
		public override void Write(double value, int stride)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = value;
			FWritePosition += stride;
		}
		
		protected override void Copy(double[] source, int sourceIndex, int length, int stride)
		{
			switch (stride)
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
							dst += stride;
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
		
		public override void Write(float value, int stride)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = (double) value;
			FWritePosition += stride;
		}
		
		protected override void Copy(float[] source, int sourceIndex, int length, int stride)
		{
			fixed (float* sourcePtr = source)
			{
				float* src = sourcePtr + sourceIndex;
				double* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = (double) *(src++);
					dst += stride;
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
		
		public override void Write(int value, int stride)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = (double) value;
			FWritePosition += stride;
		}
		
		protected override void Copy(int[] source, int sourceIndex, int length, int stride)
		{
			fixed (int* sourcePtr = source)
			{
				int* src = sourcePtr + sourceIndex;
				double* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = (double) *(src++);
					dst += stride;
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
		
		public override void Write(bool value, int stride)
		{
			FUnmanagedArray[FWritePosition] = value ? 1.0 : 0.0;
			FWritePosition += stride;
		}
		
		protected override void Copy(bool[] source, int sourceIndex, int length, int stride)
		{
			fixed (bool* sourcePtr = source)
			{
				bool* src = sourcePtr + sourceIndex;
				double* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = *(src++) ? 1.0 : 0.0;
					dst += stride;
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
		
		public override void Write(Matrix value, int stride)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = value;
			FWritePosition += stride;
		}
		
		protected override void Copy(Matrix[] source, int sourceIndex, int length, int stride)
		{
			fixed (Matrix* sourcePtr = source)
			{
				Matrix* src = sourcePtr + sourceIndex;
				Matrix* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = *(src++);
					dst += stride;
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
		
		public override void Write(Matrix4x4 value, int stride)
		{
			Debug.Assert(FWritePosition < FLength);
			FUnmanagedArray[FWritePosition] = value.ToSlimDXMatrix();
			FWritePosition += stride;
		}
		
		protected override void Copy(Matrix4x4[] source, int sourceIndex, int length, int stride)
		{
			fixed (Matrix4x4* sourcePtr = source)
			{
				Matrix4x4* src = sourcePtr + sourceIndex;
				Matrix* dst = FUnmanagedArray + FWritePosition;
				
				for (int i = 0; i < length; i++)
				{
					*dst = (*(src++)).ToSlimDXMatrix();
					dst += stride;
				}
			}
		}
	}
}
