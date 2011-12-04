
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
	
	unsafe abstract class UnmanagedOutStream<T> : IOutStream<T>
	{
		internal abstract class UnmanagedOutWriter : IStreamWriter<T>
		{
			private readonly UnmanagedOutStream<T> FStream;
			
			public UnmanagedOutWriter(UnmanagedOutStream<T> stream)
			{
				FStream = stream;
				Length = stream.Length;
			}
			
			public bool Eos
			{
				get
				{
					return Position >= Length;
				}
			}
			
			public int Position
			{
				get;
				set;
			}
			
			public int Length
			{
				get;
				private set;
			}
			
			public void Reset()
			{
				Position = 0;
			}
			
			public abstract void Write(T value, int stride);
			
			protected abstract void Copy(T[] source, int sourceIndex, int length, int stride);
			
			public int Write(T[] buffer, int index, int length, int stride)
			{
				int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				switch (stride)
				{
					case 0:
						Write(buffer[index + numSlicesToWrite - 1], stride);
						break;
					default:
						Debug.Assert(Position + numSlicesToWrite <= Length);
						Copy(buffer, index, numSlicesToWrite, stride);
						Position += numSlicesToWrite * stride;
						break;
				}
				
				return numSlicesToWrite;
			}
			
			public void Dispose()
			{
				FStream.FRefCount--;
			}
		}
		
		private readonly Func<int, IntPtr> FResizeUnmanagedArrayFunc;
		protected int FRefCount;
		protected IntPtr FUnmanagedArrayPtr;
		protected int FLength;
		protected int FWritePosition;
		
		public UnmanagedOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
		{
			FResizeUnmanagedArrayFunc = resizeUnmanagedArrayFunc;
			Resize(1);
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
			Resized(FUnmanagedArrayPtr, newSize);
		}
		
		protected abstract void Resized(IntPtr unmanagedArray, int unmanagedArrayLength);
		
		public void Flush()
		{
			// We write to the unmanaged array directly. Nothing to do here.
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public abstract IStreamWriter<T> GetWriter();
	}
	
	unsafe class DoubleOutStream : UnmanagedOutStream<double>
	{
		class DoubleOutWriter : UnmanagedOutWriter
		{
			private readonly double* FUnmanagedArray;
			
			public DoubleOutWriter(DoubleOutStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override void Write(double value, int stride)
			{
				Debug.Assert(!Eos);
				FUnmanagedArray[Position] = value;
				Position += stride;
			}
			
			protected override void Copy(double[] source, int sourceIndex, int length, int stride)
			{
				switch (stride)
				{
					case 1:
						Marshal.Copy(source, sourceIndex, new IntPtr(FUnmanagedArray + Position), length);
						break;
					default:
						fixed (double* sourcePtr = source)
						{
							double* src = sourcePtr + sourceIndex;
							double* dst = FUnmanagedArray + Position;
							
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
		
		private double* FUnmanagedArray;
		
		public DoubleOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override IStreamWriter<double> GetWriter()
		{
			FRefCount++;
			return new DoubleOutWriter(this);
		}
	}
	
	unsafe class FloatOutStream : UnmanagedOutStream<float>
	{
		class FloatOutWriter : UnmanagedOutWriter
		{
			private readonly double* FUnmanagedArray;
			
			public FloatOutWriter(FloatOutStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override void Write(float value, int stride)
			{
				Debug.Assert(!Eos);
				FUnmanagedArray[Position] = (double) value;
				Position += stride;
			}
			
			protected override void Copy(float[] source, int sourceIndex, int length, int stride)
			{
				fixed (float* sourcePtr = source)
				{
					float* src = sourcePtr + sourceIndex;
					double* dst = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*dst = (double) *(src++);
						dst += stride;
					}
				}
			}
		}
		
		private double* FUnmanagedArray;
		
		public FloatOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override IStreamWriter<float> GetWriter()
		{
			FRefCount++;
			return new FloatOutWriter(this);
		}
	}

	unsafe class IntOutStream : UnmanagedOutStream<int>
	{
		class IntOutWriter : UnmanagedOutWriter
		{
			private readonly double* FUnmanagedArray;
			
			public IntOutWriter(IntOutStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override void Write(int value, int stride)
			{
				Debug.Assert(!Eos);
				FUnmanagedArray[Position] = (double) value;
				Position += stride;
			}
			
			protected override void Copy(int[] source, int sourceIndex, int length, int stride)
			{
				fixed (int* sourcePtr = source)
				{
					int* src = sourcePtr + sourceIndex;
					double* dst = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*dst = (double) *(src++);
						dst += stride;
					}
				}
			}
		}
		
		private double* FUnmanagedArray;
		
		public IntOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		
		
		public override IStreamWriter<int> GetWriter()
		{
			FRefCount++;
			return new IntOutWriter(this);
		}
	}

	unsafe class BoolOutStream : UnmanagedOutStream<bool>
	{
		class BoolOutWriter : UnmanagedOutWriter
		{
			private readonly double* FUnmanagedArray;
			
			public BoolOutWriter(BoolOutStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override void Write(bool value, int stride)
			{
				Debug.Assert(!Eos);
				FUnmanagedArray[Position] = value ? 1.0 : 0.0;
				Position += stride;
			}
			
			protected override void Copy(bool[] source, int sourceIndex, int length, int stride)
			{
				fixed (bool* sourcePtr = source)
				{
					bool* src = sourcePtr + sourceIndex;
					double* dst = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*dst = *(src++) ? 1.0 : 0.0;
						dst += stride;
					}
				}
			}
		}
		
		private double* FUnmanagedArray;
		
		public BoolOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		
		
		public override IStreamWriter<bool> GetWriter()
		{
			FRefCount++;
			return new BoolOutWriter(this);
		}
	}

	unsafe class MatrixOutStream : UnmanagedOutStream<Matrix>
	{
		class MatrixOutWriter : UnmanagedOutWriter
		{
			private readonly Matrix* FUnmanagedArray;
			
			public MatrixOutWriter(MatrixOutStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override void Write(Matrix value, int stride)
			{
				Debug.Assert(!Eos);
				FUnmanagedArray[Position] = value;
				Position += stride;
			}
			
			protected override void Copy(Matrix[] source, int sourceIndex, int length, int stride)
			{
				fixed (Matrix* sourcePtr = source)
				{
					Matrix* src = sourcePtr + sourceIndex;
					Matrix* dst = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*dst = *(src++);
						dst += stride;
					}
				}
			}
		}
		
		private Matrix* FUnmanagedArray;
		
		public MatrixOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override IStreamWriter<Matrix> GetWriter()
		{
			FRefCount++;
			return new MatrixOutWriter(this);
		}
	}

	unsafe class Matrix4x4OutStream : UnmanagedOutStream<Matrix4x4>
	{
		class Matrix4x4OutWriter : UnmanagedOutWriter
		{
			private readonly Matrix* FUnmanagedArray;
			
			public Matrix4x4OutWriter(Matrix4x4OutStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override void Write(Matrix4x4 value, int stride)
			{
				Debug.Assert(!Eos);
				FUnmanagedArray[Position] = value.ToSlimDXMatrix();
				Position += stride;
			}
			
			protected override void Copy(Matrix4x4[] source, int sourceIndex, int length, int stride)
			{
				fixed (Matrix4x4* sourcePtr = source)
				{
					Matrix4x4* src = sourcePtr + sourceIndex;
					Matrix* dst = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*dst = (*(src++)).ToSlimDXMatrix();
						dst += stride;
					}
				}
			}
		}
		
		private Matrix* FUnmanagedArray;
		
		public Matrix4x4OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(resizeUnmanagedArrayFunc)
		{
			
		}
		
		protected override void Resized(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		
		
		public override IStreamWriter<Matrix4x4> GetWriter()
		{
			FRefCount++;
			return new Matrix4x4OutWriter(this);
		}
	}
}
