
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
		internal abstract class VectorOutStreamWriter : IStreamWriter<T>
		{
			private readonly VectorOutStream<T> FStream;
			protected readonly int FDimension;
			protected readonly double* FUnmanagedArray;
			protected readonly int FUnmanagedLength;
			protected readonly int FUnderFlow;
			protected double* FPointer;
			protected int FPosition;
			
			public VectorOutStreamWriter(VectorOutStream<T> stream)
			{
				FStream = stream;
				Length = stream.Length;
				FUnmanagedArray = stream.FUnmanagedArray;
				FDimension = stream.FDimension;
				FUnmanagedLength = stream.FUnmanagedLength;
				FUnderFlow = stream.FUnderFlow;
				FPointer = FUnmanagedArray;
			}
			
			public bool Eos
			{
				get
				{
					return FPosition >= Length;
				}
			}
			
			public int Position
			{
				get
				{
					return FPosition;
				}
				set
				{
					FPosition = value;
					FPointer = FUnmanagedArray + value * FDimension;
				}
			}
			
			public int Length
			{
				get;
				private set;
			}
			
			public void Dispose()
			{
				FStream.FRefCount--;
			}
			
			public void Reset()
			{
				FPosition = 0;
				FPointer = FUnmanagedArray;
			}
			
			public abstract void Write(T value, int stride = 1);
			
			public abstract int Write(T[] buffer, int index, int length, int stride = 1);
			
			protected bool IsOutOfBounds(int numSlicesToWorkOn)
			{
				return (FUnderFlow > 0) && ((FPosition + numSlicesToWorkOn) > (Length - 1));
			}
		}
		
		private readonly Func<int, IntPtr> FResizeUnmanagedArrayFunc;
		protected readonly int FDimension;
		protected double* FUnmanagedArray;
		protected double* FWritePointer;
		protected int FUnmanagedLength;
		protected int FLength;
		protected int FUnderFlow;
		protected int FWritePosition;
		protected int FRefCount;
		
		public VectorOutStream(int dimension, Func<int, IntPtr> resizeUnmanagedArrayFunc)
		{
			FDimension = dimension;
			FResizeUnmanagedArrayFunc = resizeUnmanagedArrayFunc;
			Length = 1;
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
					FUnmanagedArray = (double*) FResizeUnmanagedArrayFunc(value).ToPointer();
					FLength = value;
					FUnmanagedLength = value * FDimension;
					FUnderFlow = 0;
				}
			}
		}
		
		public void Flush()
		{
			// No need. We write to the unmanaged array directly.
		}
		
		public abstract IStreamWriter<T> GetWriter();
	}
	
	unsafe class Vector2DOutStream : VectorOutStream<Vector2D>
	{
		class Vector2DOutStreamWriter : VectorOutStreamWriter
		{
			public Vector2DOutStreamWriter(Vector2DOutStream stream)
				: base(stream)
			{
				
			}
			
			public override int Write(Vector2D[] buffer, int index, int length, int stride)
			{
				int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector2D* source = buffer)
				{
					Vector2D* dst = (Vector2D*) FPointer;
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
						dst += stride;
					}
					
					if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
					{
						int i = FUnmanagedLength - FUnderFlow;
						FUnmanagedArray[i++ % FUnmanagedLength] = src->x;
						FUnmanagedArray[i++ % FUnmanagedLength] = src->y;
					}
				}
				
				FPosition += numSlicesToWrite * stride;
				FPointer += numSlicesToWrite * stride * FDimension;
				
				return numSlicesToWrite;
			}
			
			public override void Write(Vector2D value, int stride)
			{
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = value.x;
					FUnmanagedArray[i++ % FUnmanagedLength] = value.y;
				}
				else
				{
					*((Vector2D*) FPointer) = value;
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
			}
		}
		
		public Vector2DOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(2, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override IStreamWriter<Vector2D> GetWriter()
		{
			FRefCount++;
			return new Vector2DOutStreamWriter(this);
		}
	}
	
	unsafe class Vector3DOutStream : VectorOutStream<Vector3D>
	{
		class Vector3DOutStreamWriter : VectorOutStreamWriter
		{
			public Vector3DOutStreamWriter(Vector3DOutStream stream)
				: base(stream)
			{
				
			}
			
			public override int Write(Vector3D[] buffer, int index, int length, int stride)
			{
				int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector3D* source = buffer)
				{
					Vector3D* dst = (Vector3D*) FPointer;
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
						dst += stride;
					}
					
					if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
					{
						int i = FUnmanagedLength - FUnderFlow;
						FUnmanagedArray[i++ % FUnmanagedLength] = src->x;
						FUnmanagedArray[i++ % FUnmanagedLength] = src->y;
						FUnmanagedArray[i++ % FUnmanagedLength] = src->z;
					}
				}
				
				FPosition += numSlicesToWrite * stride;
				FPointer += numSlicesToWrite * stride * FDimension;
				
				return numSlicesToWrite;
			}
			
			public override void Write(Vector3D value, int stride)
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
					*((Vector3D*) FPointer) = value;
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
			}
		}
		
		public Vector3DOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(3, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override IStreamWriter<Vector3D> GetWriter()
		{
			FRefCount++;
			return new Vector3DOutStreamWriter(this);
		}
	}
	
	unsafe class Vector4DOutStream : VectorOutStream<Vector4D>
	{
		class Vector4DOutStreamWriter : VectorOutStreamWriter
		{
			public Vector4DOutStreamWriter(Vector4DOutStream stream)
				: base(stream)
			{
				
			}
			
			public override int Write(Vector4D[] buffer, int index, int length, int stride)
			{
				int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector4D* source = buffer)
				{
					Vector4D* dst = (Vector4D*) FPointer;
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
						dst += stride;
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
				
				FPosition += numSlicesToWrite * stride;
				FPointer += numSlicesToWrite * stride * FDimension;
				
				return numSlicesToWrite;
			}
			
			public override void Write(Vector4D value, int stride)
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
					*((Vector4D*) FPointer) = value;
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
			}
		}
		
		public Vector4DOutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(4, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override IStreamWriter<Vector4D> GetWriter()
		{
			FRefCount++;
			return new Vector4DOutStreamWriter(this);
		}
	}
	
	unsafe class Vector2OutStream : VectorOutStream<Vector2>
	{
		class Vector2OutStreamWriter : VectorOutStreamWriter
		{
			public Vector2OutStreamWriter(Vector2OutStream stream)
				: base(stream)
			{
				
			}
			
			public override int Write(Vector2[] buffer, int index, int length, int stride)
			{
				int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector2* source = buffer)
				{
					Vector2D* dst = (Vector2D*) FPointer;
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
						dst += stride;
					}
					
					if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
					{
						int i = FUnmanagedLength - FUnderFlow;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->X;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->Y;
					}
				}
				
				FPosition += numSlicesToWrite * stride;
				FPointer += numSlicesToWrite * stride * FDimension;
				
				return numSlicesToWrite;
			}
			
			public override void Write(Vector2 value, int stride)
			{
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.X;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.Y;
				}
				else
				{
					*((Vector2D*) FPointer) = value.ToVector2D();
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
			}
		}
		
		public Vector2OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(2, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override IStreamWriter<Vector2> GetWriter()
		{
			FRefCount++;
			return new Vector2OutStreamWriter(this);
		}
	}
	
	unsafe class Vector3OutStream : VectorOutStream<Vector3>
	{
		class Vector3OutStreamWriter : VectorOutStreamWriter
		{
			public Vector3OutStreamWriter(Vector3OutStream stream)
				: base(stream)
			{
				
			}
			
			public override int Write(Vector3[] buffer, int index, int length, int stride)
			{
				int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector3* source = buffer)
				{
					Vector3D* dst = (Vector3D*) FPointer;
					Vector3* src = source + index;
					
					int numSlicesToWriteAtFullSpeed = numSlicesToWrite;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToWrite))
					{
						numSlicesToWriteAtFullSpeed = Math.Max(numSlicesToWriteAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToWriteAtFullSpeed; i++)
					{
						*dst = (*(src++)).ToVector3D();
						dst += stride;
					}
					
					if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
					{
						int i = FUnmanagedLength - FUnderFlow;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->X;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->Y;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->Z;
					}
				}
				
				FPosition += numSlicesToWrite * stride;
				FPointer += numSlicesToWrite * stride * FDimension;
				
				return numSlicesToWrite;
			}
			
			public override void Write(Vector3 value, int stride)
			{
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.X;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.Y;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.Z;
				}
				else
				{
					*((Vector3D*) FPointer) = value.ToVector3D();
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
			}
		}
		
		public Vector3OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(3, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override IStreamWriter<Vector3> GetWriter()
		{
			FRefCount++;
			return new Vector3OutStreamWriter(this);
		}
	}
	
	unsafe class Vector4OutStream : VectorOutStream<Vector4>
	{
		class Vector4OutStreamWriter : VectorOutStreamWriter
		{
			public Vector4OutStreamWriter(Vector4OutStream stream)
				: base(stream)
			{
				
			}
			
			public override int Write(Vector4[] buffer, int index, int length, int stride)
			{
				int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector4* source = buffer)
				{
					Vector4D* dst = (Vector4D*) FPointer;
					Vector4* src = source + index;
					
					int numSlicesToWriteAtFullSpeed = numSlicesToWrite;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToWrite))
					{
						numSlicesToWriteAtFullSpeed = Math.Max(numSlicesToWriteAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToWriteAtFullSpeed; i++)
					{
						*dst = (*(src++)).ToVector4D();
						dst += stride;
					}
					
					if (numSlicesToWriteAtFullSpeed < numSlicesToWrite)
					{
						int i = FUnmanagedLength - FUnderFlow;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->X;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->Y;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->Z;
						FUnmanagedArray[i++ % FUnmanagedLength] = (double) src->W;
					}
				}
				
				FPosition += numSlicesToWrite * stride;
				FPointer += numSlicesToWrite * stride * FDimension;
				
				return numSlicesToWrite;
			}
			
			public override void Write(Vector4 value, int stride)
			{
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.X;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.Y;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.Z;
					FUnmanagedArray[i++ % FUnmanagedLength] = (double) value.W;
				}
				else
				{
					*((Vector4D*) FPointer) = value.ToVector4D();
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
			}
		}
		
		public Vector4OutStream(Func<int, IntPtr> resizeUnmanagedArrayFunc)
			: base(4, resizeUnmanagedArrayFunc)
		{
			
		}
		
		public override IStreamWriter<Vector4> GetWriter()
		{
			FRefCount++;
			return new Vector4OutStreamWriter(this);
		}
	}
}
