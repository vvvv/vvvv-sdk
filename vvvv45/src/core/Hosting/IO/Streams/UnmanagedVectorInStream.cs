
using System;
using SlimDX;
using VVVV.Utils;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO.Streams
{
	unsafe abstract class VectorInStream<T> : IInStream<T> where T : struct
	{
		internal abstract class VectorInStreamReader : IStreamReader<T>
		{
			private readonly VectorInStream<T> FStream;
			protected readonly int FDimension;
			protected readonly double* FUnmanagedArray;
			protected readonly int FUnmanagedLength;
			protected readonly int FUnderFlow;
			protected double* FPointer;
			protected int FPosition;
			
			public VectorInStreamReader(VectorInStream<T> stream)
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
			
			public T Current
			{
				get;
				private set;
			}
			
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}
			
			public bool MoveNext()
			{
				var result = !Eos;
				if (result)
				{
					Current = Read();
				}
				return result;
			}
			
			public abstract T Read(int stride = 1);
			
			public abstract int Read(T[] buffer, int index, int length, int stride);
			
			public void ReadCyclic(T[] buffer, int index, int length, int stride)
			{
				// Exception handling
				if (Length == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
				
				// Normalize the stride
				stride %= Length;
				
				switch (Length)
				{
					case 1:
						// Special treatment for streams of length one
						if (Eos) Reset();
						
						if (index == 0 && length == buffer.Length)
							buffer.Init(Read(stride)); // Slightly faster
						else
							buffer.Fill(index, length, Read(stride));
						break;
					default:
						int numSlicesRead = 0;
						
						// Read till end
						while ((numSlicesRead < length) && (Position %= Length) > 0)
						{
							numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
						}
						
						// Save start of possible block
						int startIndex = index + numSlicesRead;
						
						// Read one block
						while (numSlicesRead < length)
						{
							numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
							// Exit the loop once ReadPosition is back at beginning
							if ((Position %= Length) == 0) break;
						}
						
						// Save end of possible block
						int endIndex = index + numSlicesRead;
						
						// Calculate block size
						int blockSize = endIndex - startIndex;
						
						// Now see if the block can be replicated to fill up the buffer
						if (blockSize > 0)
						{
							int times = (length - numSlicesRead) / blockSize;
							buffer.Replicate(startIndex, endIndex, times);
							numSlicesRead += blockSize * times;
						}
						
						// Read the rest
						while (numSlicesRead < length)
						{
							if (Eos) Position %= Length;
							numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
						}
						
						break;
				}
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
			
			protected bool IsOutOfBounds(int numSlicesToWorkOn)
			{
				return (FUnderFlow > 0) && ((FPosition + numSlicesToWorkOn) > (Length - 1));
			}
		}
		
		private readonly Func<Tuple<IntPtr, int>> FGetUnmanagedArrayFunc;
		private readonly Func<bool> FValidateFunc;
		protected readonly int FDimension;
		protected int FRefCount;
		protected double* FUnmanagedArray;
		protected double* FReadPointer;
		protected int FUnmanagedLength;
		protected int FUnderFlow;
		protected int FReadPosition;
		
		public VectorInStream(int dimension, Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
		{
			FDimension = dimension;
			FGetUnmanagedArrayFunc = getUnmanagedArrayFunc;
			FValidateFunc = validateFunc;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public bool Sync()
		{
			var changed = FValidateFunc();
			
			var result = FGetUnmanagedArrayFunc();
			FUnmanagedArray = (double*) result.Item1.ToPointer();
			FUnmanagedLength = result.Item2;
			Length = Math.DivRem(FUnmanagedLength, FDimension, out FUnderFlow);
			if (FUnderFlow > 0)
				Length++;
			
			return changed;
		}
		
		public int Length
		{
			get;
			private set;
		}
		
		public abstract IStreamReader<T> GetReader();
		
		public System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return GetReader();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	
	unsafe class Vector2DInStream : VectorInStream<Vector2D>
	{
		class Vector2DInStreamReader : VectorInStreamReader
		{
			public Vector2DInStreamReader(Vector2DInStream stream)
				: base(stream)
			{
				
			}
			
			public override int Read(Vector2D[] buffer, int index, int length, int stride)
			{
				int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector2D* destination = buffer)
				{
					Vector2D* dst = destination + index;
					Vector2D* src = (Vector2D*) FPointer;
					
					int numSlicesToReadAtFullSpeed = numSlicesToRead;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToRead))
					{
						numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
					{
						*(dst++) = *src;
						src += stride;
					}
					
					if (numSlicesToReadAtFullSpeed < numSlicesToRead)
					{
						int i = FUnmanagedLength - FUnderFlow;
						dst->x = FUnmanagedArray[i++ % FUnmanagedLength];
						dst->y = FUnmanagedArray[i++ % FUnmanagedLength];
					}
				}
				
				FPosition += numSlicesToRead * stride;
				FPointer += numSlicesToRead * stride * FDimension;
				
				return numSlicesToRead;
			}
			
			public override Vector2D Read(int stride)
			{
				Vector2D result;
				
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					result.x = FUnmanagedArray[i++ % FUnmanagedLength];
					result.y = FUnmanagedArray[i++ % FUnmanagedLength];
				}
				else
				{
					result = *((Vector2D*) FPointer);
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
				
				return result;
			}
		}
		
		public Vector2DInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(2, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override IStreamReader<Vector2D> GetReader()
		{
			FRefCount++;
			return new Vector2DInStreamReader(this);
		}
	}
	
	unsafe class Vector3DInStream : VectorInStream<Vector3D>
	{
		class Vector3DInStreamReader : VectorInStreamReader
		{
			public Vector3DInStreamReader(Vector3DInStream stream)
				: base(stream)
			{
				
			}
			
			public override int Read(Vector3D[] buffer, int index, int length, int stride)
			{
				int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector3D* destination = buffer)
				{
					Vector3D* dst = destination + index;
					Vector3D* src = (Vector3D*) FPointer;
					
					int numSlicesToReadAtFullSpeed = numSlicesToRead;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToRead))
					{
						numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
					{
						*(dst++) = *src;
						src += stride;
					}
					
					if (numSlicesToReadAtFullSpeed < numSlicesToRead)
					{
						int i = FUnmanagedLength - FUnderFlow;
						dst->x = FUnmanagedArray[i++ % FUnmanagedLength];
						dst->y = FUnmanagedArray[i++ % FUnmanagedLength];
						dst->z = FUnmanagedArray[i++ % FUnmanagedLength];
					}
				}
				
				FPosition += numSlicesToRead * stride;
				FPointer += numSlicesToRead * stride * FDimension;
				
				return numSlicesToRead;
			}
			
			public override Vector3D Read(int stride)
			{
				Vector3D result;
				
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					result.x = FUnmanagedArray[i++ % FUnmanagedLength];
					result.y = FUnmanagedArray[i++ % FUnmanagedLength];
					result.z = FUnmanagedArray[i++ % FUnmanagedLength];
				}
				else
				{
					result = *((Vector3D*) FPointer);
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
				
				return result;
			}
		}
		
		public Vector3DInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(3, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override IStreamReader<Vector3D> GetReader()
		{
			FRefCount++;
			return new Vector3DInStreamReader(this);
		}
	}
	
	unsafe class Vector4DInStream : VectorInStream<Vector4D>
	{
		class Vector4DInStreamReader : VectorInStreamReader
		{
			public Vector4DInStreamReader(Vector4DInStream stream)
				: base(stream)
			{
				
			}
			
			public override int Read(Vector4D[] buffer, int index, int length, int stride)
			{
				int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector4D* destination = buffer)
				{
					Vector4D* dst = destination + index;
					Vector4D* src = (Vector4D*) FPointer;
					
					int numSlicesToReadAtFullSpeed = numSlicesToRead;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToRead))
					{
						numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
					{
						*(dst++) = *src;
						src += stride;
					}
					
					if (numSlicesToReadAtFullSpeed < numSlicesToRead)
					{
						int i = FUnmanagedLength - FUnderFlow;
						dst->x = FUnmanagedArray[i++ % FUnmanagedLength];
						dst->y = FUnmanagedArray[i++ % FUnmanagedLength];
						dst->z = FUnmanagedArray[i++ % FUnmanagedLength];
						dst->w = FUnmanagedArray[i++ % FUnmanagedLength];
					}
				}
				
				FPosition += numSlicesToRead * stride;
				FPointer += numSlicesToRead * stride * FDimension;
				
				return numSlicesToRead;
			}
			
			public override Vector4D Read(int stride)
			{
				Vector4D result;
				
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					result.x = FUnmanagedArray[i++ % FUnmanagedLength];
					result.y = FUnmanagedArray[i++ % FUnmanagedLength];
					result.z = FUnmanagedArray[i++ % FUnmanagedLength];
					result.w = FUnmanagedArray[i++ % FUnmanagedLength];
				}
				else
				{
					result = *((Vector4D*) FPointer);
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
				
				return result;
			}
		}
		
		public Vector4DInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(4, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override IStreamReader<Vector4D> GetReader()
		{
			FRefCount++;
			return new Vector4DInStreamReader(this);
		}
	}
	
	unsafe class Vector2InStream : VectorInStream<Vector2>
	{
		class Vector2InStreamReader : VectorInStreamReader
		{
			public Vector2InStreamReader(Vector2InStream stream)
				: base(stream)
			{
				
			}
			
			public override int Read(Vector2[] buffer, int index, int length, int stride)
			{
				int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector2* destination = buffer)
				{
					Vector2* dst = destination + index;
					Vector2D* src = (Vector2D*) FPointer;
					
					int numSlicesToReadAtFullSpeed = numSlicesToRead;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToRead))
					{
						numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
					{
						*(dst++) = (*src).ToSlimDXVector();
						src += stride;
					}
					
					if (numSlicesToReadAtFullSpeed < numSlicesToRead)
					{
						int i = FUnmanagedLength - FUnderFlow;
						dst->X = (float) FUnmanagedArray[i++ % FUnmanagedLength];
						dst->Y = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					}
				}
				
				FPosition += numSlicesToRead * stride;
				FPointer += numSlicesToRead * stride * FDimension;
				
				return numSlicesToRead;
			}
			
			public override Vector2 Read(int stride)
			{
				Vector2 result;
				
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					result.X = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					result.Y = (float) FUnmanagedArray[i++ % FUnmanagedLength];
				}
				else
				{
					result = (*((Vector2D*) FPointer)).ToSlimDXVector();
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
				
				return result;
			}
		}
		
		public Vector2InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(2, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override IStreamReader<Vector2> GetReader()
		{
			FRefCount++;
			return new Vector2InStreamReader(this);
		}
	}
	
	unsafe class Vector3InStream : VectorInStream<Vector3>
	{
		class Vector3InStreamReader : VectorInStreamReader
		{
			public Vector3InStreamReader(Vector3InStream stream)
				: base(stream)
			{
				
			}
			
			public override int Read(Vector3[] buffer, int index, int length, int stride)
			{
				int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector3* destination = buffer)
				{
					Vector3* dst = destination + index;
					Vector3D* src = (Vector3D*) FPointer;
					
					int numSlicesToReadAtFullSpeed = numSlicesToRead;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToRead))
					{
						numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
					{
						*(dst++) = (*src).ToSlimDXVector();
						src += stride;
					}
					
					if (numSlicesToReadAtFullSpeed < numSlicesToRead)
					{
						int i = FUnmanagedLength - FUnderFlow;
						dst->X = (float) FUnmanagedArray[i++ % FUnmanagedLength];
						dst->Y = (float) FUnmanagedArray[i++ % FUnmanagedLength];
						dst->Z = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					}
				}
				
				FPosition += numSlicesToRead * stride;
				FPointer += numSlicesToRead * stride * FDimension;
				
				return numSlicesToRead;
			}
			
			public override Vector3 Read(int stride)
			{
				Vector3 result;
				
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					result.X = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					result.Y = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					result.Z = (float) FUnmanagedArray[i++ % FUnmanagedLength];
				}
				else
				{
					result = (*((Vector3D*) FPointer)).ToSlimDXVector();
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
				
				return result;
			}
		}
		
		public Vector3InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(3, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override IStreamReader<Vector3> GetReader()
		{
			FRefCount++;
			return new Vector3InStreamReader(this);
		}
	}
	
	unsafe class Vector4InStream : VectorInStream<Vector4>
	{
		class Vector4InStreamReader : VectorInStreamReader
		{
			public Vector4InStreamReader(Vector4InStream stream)
				: base(stream)
			{
				
			}
			
			public override int Read(Vector4[] buffer, int index, int length, int stride)
			{
				int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				
				fixed (Vector4* destination = buffer)
				{
					Vector4* dst = destination + index;
					Vector4D* src = (Vector4D*) FPointer;
					
					int numSlicesToReadAtFullSpeed = numSlicesToRead;
					
					// Check if we would read too much (for example unmanaged array is of size 7).
					if (IsOutOfBounds(numSlicesToRead))
					{
						numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
					}
					
					for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
					{
						*(dst++) = (*src).ToSlimDXVector();
						src += stride;
					}
					
					if (numSlicesToReadAtFullSpeed < numSlicesToRead)
					{
						int i = FUnmanagedLength - FUnderFlow;
						dst->X = (float) FUnmanagedArray[i++ % FUnmanagedLength];
						dst->Y = (float) FUnmanagedArray[i++ % FUnmanagedLength];
						dst->Z = (float) FUnmanagedArray[i++ % FUnmanagedLength];
						dst->W = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					}
				}
				
				FPosition += numSlicesToRead * stride;
				FPointer += numSlicesToRead * stride * FDimension;
				
				return numSlicesToRead;
			}
			
			public override Vector4 Read(int stride)
			{
				Vector4 result;
				
				if (IsOutOfBounds(1))
				{
					int i = FUnmanagedLength - FUnderFlow;
					result.X = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					result.Y = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					result.Z = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					result.W = (float) FUnmanagedArray[i++ % FUnmanagedLength];
				}
				else
				{
					result = (*((Vector4D*) FPointer)).ToSlimDXVector();
				}
				
				FPosition += stride;
				FPointer += stride * FDimension;
				
				return result;
			}
		}
		
		public Vector4InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(4, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override IStreamReader<Vector4> GetReader()
		{
			FRefCount++;
			return new Vector4InStreamReader(this);
		}
	}
}
