
using System;
using SlimDX;
using VVVV.Utils;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams
{
	unsafe abstract class VectorInStream<T> : IInStream<T> where T : struct
	{
		private readonly Func<Tuple<IntPtr, int>> FGetUnmanagedArrayFunc;
		private readonly Action FValidateFunc;
		protected readonly int FDimension;
		protected double* FUnmanagedArray;
		protected double* FReadPointer;
		protected int FUnmanagedLength;
		protected int FUnderFlow;
		protected int FReadPosition;
		
		public VectorInStream(int dimension, Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
		{
			FDimension = dimension;
			FGetUnmanagedArrayFunc = getUnmanagedArrayFunc;
			FValidateFunc = validateFunc;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public int Length
		{
			get;
			private set;
		}
		
		public int ReadPosition
		{
			get
			{
				return FReadPosition;
			}
			set
			{
				FReadPosition = value;
				FReadPointer = FUnmanagedArray + value * FDimension;
			}
		}
		
		public bool Eof
		{
			get
			{
				return FReadPosition >= Length;
			}
		}
		
		public abstract T Read(int stepSize = 1);
		
		public abstract int Read(T[] buffer, int index, int length, int stepSize);
		
		public void ReadCyclic(T[] buffer, int index, int length, int stepSize)
		{
			// Exception handling
			if (Length == 0) throw new ArgumentOutOfRangeException("Can't read from an empty stream.");
			
			// Normalize the stride
			stepSize %= Length;
			
			switch (Length)
			{
				case 1:
					// Special treatment for streams of length one
					if (Eof) Reset();
					
					if (index == 0 && length == buffer.Length)
						buffer.Init(Read(stepSize)); // Slightly faster
					else
						buffer.Fill(index, length, Read(stepSize));
					break;
				default:
					int numSlicesRead = 0;
					
					// Read till end
					while ((numSlicesRead < length) && (ReadPosition %= Length) > 0)
					{
						numSlicesRead += Read(buffer, index, length, stepSize);
					}
					
					// Save start of possible block
					int startIndex = index + numSlicesRead;
					
					// Read one block
					while (numSlicesRead < length)
					{
						numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stepSize);
						// Exit the loop once ReadPosition is back at beginning
						if ((ReadPosition %= Length) == 0) break;
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
						if (Eof) ReadPosition %= Length;
						numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stepSize);
					}
					
					break;
			}
		}
		
		public void Reset()
		{
			FReadPosition = 0;
			FReadPointer = FUnmanagedArray;
		}
		
		public void Sync()
		{
			FValidateFunc();
			
			var result = FGetUnmanagedArrayFunc();
			FUnmanagedArray = (double*) result.Item1.ToPointer();
			FUnmanagedLength = result.Item2;
			Length = Math.DivRem(FUnmanagedLength, FDimension, out FUnderFlow);
			if (FUnderFlow > 0)
				Length++;
		}
		
		protected bool IsOutOfBounds(int numSlicesToWorkOn)
		{
			return (FUnderFlow > 0) && ((FReadPosition + numSlicesToWorkOn) > (Length - 1));
		}
	}
	
	unsafe class Vector2DInStream : VectorInStream<Vector2D>
	{
		public Vector2DInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
			: base(2, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override int Read(Vector2D[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			
			fixed (Vector2D* destination = buffer)
			{
				Vector2D* dst = destination + index;
				Vector2D* src = (Vector2D*) FReadPointer;
				
				int numSlicesToReadAtFullSpeed = numSlicesToRead;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToRead))
				{
					numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
				{
					*(dst++) = *src;
					src += stepSize;
				}
				
				if (numSlicesToReadAtFullSpeed < numSlicesToRead)
				{
					int i = FUnmanagedLength - FUnderFlow;
					dst->x = FUnmanagedArray[i++ % FUnmanagedLength];
					dst->y = FUnmanagedArray[i++ % FUnmanagedLength];
				}
			}
			
			FReadPosition += numSlicesToRead * stepSize;
			FReadPointer += numSlicesToRead * stepSize * FDimension;
			
			return numSlicesToRead;
		}
		
		public override Vector2D Read(int stepSize)
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
				result = *((Vector2D*) FReadPointer);
			}
			
			FReadPosition += stepSize;
			FReadPointer += stepSize * FDimension;
			
			return result;
		}
	}
	
	unsafe class Vector3DInStream : VectorInStream<Vector3D>
	{
		public Vector3DInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
			: base(3, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override int Read(Vector3D[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			
			fixed (Vector3D* destination = buffer)
			{
				Vector3D* dst = destination + index;
				Vector3D* src = (Vector3D*) FReadPointer;
				
				int numSlicesToReadAtFullSpeed = numSlicesToRead;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToRead))
				{
					numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
				{
					*(dst++) = *src;
					src += stepSize;
				}
				
				if (numSlicesToReadAtFullSpeed < numSlicesToRead)
				{
					int i = FUnmanagedLength - FUnderFlow;
					dst->x = FUnmanagedArray[i++ % FUnmanagedLength];
					dst->y = FUnmanagedArray[i++ % FUnmanagedLength];
					dst->z = FUnmanagedArray[i++ % FUnmanagedLength];
				}
			}
			
			FReadPosition += numSlicesToRead * stepSize;
			FReadPointer += numSlicesToRead * stepSize * FDimension;
			
			return numSlicesToRead;
		}
		
		public override Vector3D Read(int stepSize)
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
				result = *((Vector3D*) FReadPointer);
			}
			
			FReadPosition += stepSize;
			FReadPointer += stepSize * FDimension;
			
			return result;
		}
	}
	
	unsafe class Vector4DInStream : VectorInStream<Vector4D>
	{
		public Vector4DInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
			: base(4, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override int Read(Vector4D[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			
			fixed (Vector4D* destination = buffer)
			{
				Vector4D* dst = destination + index;
				Vector4D* src = (Vector4D*) FReadPointer;
				
				int numSlicesToReadAtFullSpeed = numSlicesToRead;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToRead))
				{
					numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
				{
					*(dst++) = *src;
					src += stepSize;
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
			
			FReadPosition += numSlicesToRead * stepSize;
			FReadPointer += numSlicesToRead * stepSize * FDimension;
			
			return numSlicesToRead;
		}
		
		public override Vector4D Read(int stepSize)
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
				result = *((Vector4D*) FReadPointer);
			}
			
			FReadPosition += stepSize;
			FReadPointer += stepSize * FDimension;
			
			return result;
		}
	}
	
	unsafe class ColorInStream : VectorInStream<RGBAColor>
	{
		public ColorInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
			: base(4, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override int Read(RGBAColor[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			
			fixed (RGBAColor* destination = buffer)
			{
				RGBAColor* dst = destination + index;
				RGBAColor* src = (RGBAColor*) FReadPointer;
				
				int numSlicesToReadAtFullSpeed = numSlicesToRead;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToRead))
				{
					numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
				{
					*(dst++) = *src;
					src += stepSize;
				}
				
				if (numSlicesToReadAtFullSpeed < numSlicesToRead)
				{
					int i = FUnmanagedLength - FUnderFlow;
					dst->R = FUnmanagedArray[i++ % FUnmanagedLength];
					dst->G = FUnmanagedArray[i++ % FUnmanagedLength];
					dst->B = FUnmanagedArray[i++ % FUnmanagedLength];
					dst->A = FUnmanagedArray[i++ % FUnmanagedLength];
				}
			}
			
			FReadPosition += numSlicesToRead * stepSize;
			FReadPointer += numSlicesToRead * stepSize * FDimension;
			
			return numSlicesToRead;
		}
		
		public override RGBAColor Read(int stepSize)
		{
			RGBAColor result;
			
			if (IsOutOfBounds(1))
			{
				int i = FUnmanagedLength - FUnderFlow;
				result.R = FUnmanagedArray[i++ % FUnmanagedLength];
				result.G = FUnmanagedArray[i++ % FUnmanagedLength];
				result.B = FUnmanagedArray[i++ % FUnmanagedLength];
				result.A = FUnmanagedArray[i++ % FUnmanagedLength];
			}
			else
			{
				result = *((RGBAColor*) FReadPointer);
			}
			
			FReadPosition += stepSize;
			FReadPointer += stepSize * FDimension;
			
			return result;
		}
	}
	
	unsafe class Vector2InStream : VectorInStream<Vector2>
	{
		public Vector2InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
			: base(2, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override int Read(Vector2[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			
			fixed (Vector2* destination = buffer)
			{
				Vector2* dst = destination + index;
				Vector2D* src = (Vector2D*) FReadPointer;
				
				int numSlicesToReadAtFullSpeed = numSlicesToRead;
				
				// Check if we would read too much (for example unmanaged array is of size 7).
				if (IsOutOfBounds(numSlicesToRead))
				{
					numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
				}
				
				for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
				{
					*(dst++) = (*src).ToSlimDXVector();
					src += stepSize;
				}
				
				if (numSlicesToReadAtFullSpeed < numSlicesToRead)
				{
					int i = FUnmanagedLength - FUnderFlow;
					dst->X = (float) FUnmanagedArray[i++ % FUnmanagedLength];
					dst->Y = (float) FUnmanagedArray[i++ % FUnmanagedLength];
				}
			}
			
			FReadPosition += numSlicesToRead * stepSize;
			FReadPointer += numSlicesToRead * stepSize * FDimension;
			
			return numSlicesToRead;
		}
		
		public override Vector2 Read(int stepSize)
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
				result = (*((Vector2D*) FReadPointer)).ToSlimDXVector();
			}
			
			FReadPosition += stepSize;
			FReadPointer += stepSize * FDimension;
			
			return result;
		}
	}
	
	unsafe class Vector3InStream : VectorInStream<Vector3>
	{
		public Vector3InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
			: base(3, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override int Read(Vector3[] buffer, int index, int length, int stepSize)
		{
			throw new NotImplementedException();
		}
		
		public override Vector3 Read(int stepSize)
		{
			throw new NotImplementedException();
		}
	}
	
	unsafe class Vector4InStream : VectorInStream<Vector4>
	{
		public Vector4InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateFunc)
			: base(4, getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		public override int Read(Vector4[] buffer, int index, int length, int stepSize)
		{
			throw new NotImplementedException();
		}
		
		public override Vector4 Read(int stepSize)
		{
			throw new NotImplementedException();
		}
	}
}
