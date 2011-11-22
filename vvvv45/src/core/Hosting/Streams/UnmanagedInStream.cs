
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX;
using VVVV.Utils;
using VVVV.Utils.SlimDX;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Streams
{
	unsafe abstract class UnmanagedInStream<T> : IInStream<T>
	{
		public static IInStream<T> Create(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
		{
			object result = null;
			
			var type = typeof(T);
			if (type == typeof(double))
				result = new DoubleInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(float))
				result = new FloatInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(int))
				result = new IntInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(bool))
				result = new BoolInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Vector2))
				result = new Vector2InStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Vector3))
				result = new Vector3InStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Vector4))
				result = new Vector4InStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Vector2D))
				result = new Vector2DInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Vector3D))
				result = new Vector3DInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Vector4D))
				result = new Vector4DInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Matrix))
				result = new MatrixInStream(getUnmanagedArrayFunc, validateFunc);
			else if (type == typeof(Matrix4x4))
				result = new Matrix4x4InStream(getUnmanagedArrayFunc, validateFunc);
			else
				throw new NotSupportedException(string.Format("UnmanagedInStream of type '{0}' is not supported.", type));
			
			return result as IInStream<T>;
		}
		
		private readonly Func<Tuple<IntPtr, int>> FGetUnmanagedArrayFunc;
		private readonly Func<bool> FValidateFunc;
		protected IntPtr FUnmanagedArrayPtr;
		
		public UnmanagedInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
		{
			FGetUnmanagedArrayFunc = getUnmanagedArrayFunc;
			FValidateFunc = validateFunc;
		}
		
		public int ReadPosition
		{
			get;
			set;
		}
		
		public int Length
		{
			get;
			private set;
		}
		
		public bool Eof
		{
			get
			{
				return ReadPosition >= Length;
			}
		}
		
		public abstract T Read(int stride);
		
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
					if (Eof) Reset();
					
					if (index == 0 && length == buffer.Length)
						buffer.Init(Read(stride)); // Slightly faster
					else
						buffer.Fill(index, length, Read(stride));
					break;
				default:
					int numSlicesRead = 0;
					
					// Read till end
					while ((numSlicesRead < length) && (ReadPosition %= Length) > 0)
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
						numSlicesRead += Read(buffer, index + numSlicesRead, length - numSlicesRead, stride);
					}
					
					break;
			}
		}
		
		protected abstract void Copy(T[] destination, int destinationIndex, int length, int stride);
		
		protected abstract void Synced(IntPtr unmanagedArray, int unmanagedArrayLength);
		
		public int Read(T[] buffer, int index, int length, int stride)
		{
			int slicesAhead = Length - ReadPosition;
			
			if (stride > 0)
			{
				int r = 0;
				slicesAhead = Math.DivRem(slicesAhead, stride, out r);
				if (r > 0)
					slicesAhead++;
			}
			
			int numSlicesToRead = Math.Max(Math.Min(length, slicesAhead), 0);
			
			switch (stride)
			{
				case 0:
					if (index == 0 && numSlicesToRead == buffer.Length)
						buffer.Init(Read(stride)); // Slightly faster
					else
						buffer.Fill(index, numSlicesToRead, Read(stride));
					break;
				default:
					Debug.Assert(ReadPosition + numSlicesToRead <= Length);
					Copy(buffer, index, numSlicesToRead, stride);
					ReadPosition += numSlicesToRead * stride;
					break;
			}
			
			return numSlicesToRead;
		}
		
		public bool Sync()
		{
			var changed = FValidateFunc();
			
			var result = FGetUnmanagedArrayFunc();
			FUnmanagedArrayPtr = result.Item1;
			Length = result.Item2;
			Synced(FUnmanagedArrayPtr, Length);
			
			return changed;
		}
		
		public void Reset()
		{
			ReadPosition = 0;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
	}
	
	unsafe class DoubleInStream : UnmanagedInStream<double>
	{
		private double* FUnmanagedArray;
		
		public DoubleInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override double Read(int stride)
		{
			Debug.Assert(ReadPosition < Length);
			var result = FUnmanagedArray[ReadPosition];
			ReadPosition += stride;
			return result;
		}
		
		protected override void Copy(double[] destination, int destinationIndex, int length, int stride)
		{
			switch (stride)
			{
				case 1:
					Marshal.Copy(FUnmanagedArrayPtr + ReadPosition * sizeof(double), destination, destinationIndex, length);
					break;
				default:
					fixed (double* destinationPtr = destination)
					{
						double* dst = destinationPtr + destinationIndex;
						double* src = FUnmanagedArray + ReadPosition;
						
						for (int i = 0; i < length; i++)
						{
							*(dst++) = *src;
							src += stride;
						}
					}
					break;
			}
		}
	}
	
	unsafe class FloatInStream : UnmanagedInStream<float>
	{
		private double* FUnmanagedArray;
		
		public FloatInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override float Read(int stride)
		{
			Debug.Assert(ReadPosition < Length);
			var result = (float) FUnmanagedArray[ReadPosition];
			ReadPosition += stride;
			return result;
		}
		
		protected override void Copy(float[] destination, int destinationIndex, int length, int stride)
		{
			fixed (float* destinationPtr = destination)
			{
				float* dst = destinationPtr + destinationIndex;
				double* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = (float) *src;
					src += stride;
				}
			}
		}
	}

	unsafe class IntInStream : UnmanagedInStream<int>
	{
		private double* FUnmanagedArray;
		
		public IntInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override int Read(int stride)
		{
			Debug.Assert(ReadPosition < Length);
			var result = (int) Math.Round(FUnmanagedArray[ReadPosition]);
			ReadPosition += stride;
			return result;
		}
		
		protected override void Copy(int[] destination, int destinationIndex, int length, int stride)
		{
			fixed (int* destinationPtr = destination)
			{
				int* dst = destinationPtr + destinationIndex;
				double* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = (int) Math.Round(*src);
					src += stride;
				}
			}
		}
	}

	unsafe class BoolInStream : UnmanagedInStream<bool>
	{
		private double* FUnmanagedArray;
		
		public BoolInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override bool Read(int stride)
		{
			Debug.Assert(ReadPosition < Length, string.Format("ReadPosition: {0}, Length: {0}", ReadPosition, Length));
			var result = FUnmanagedArray[ReadPosition] >= 0.5;
			ReadPosition += stride;
			return result;
		}
		
		protected override void Copy(bool[] destination, int destinationIndex, int length, int stride)
		{
			fixed (bool* destinationPtr = destination)
			{
				bool* dst = destinationPtr + destinationIndex;
				double* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = *src >= 0.5;
					src += stride;
				}
			}
		}
	}

	unsafe class MatrixInStream : UnmanagedInStream<Matrix>
	{
		private Matrix* FUnmanagedArray;
		
		public MatrixInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override Matrix Read(int stride)
		{
			Debug.Assert(ReadPosition < Length);
			var result = FUnmanagedArray[ReadPosition];
			ReadPosition += stride;
			return result;
		}
		
		protected override void Copy(Matrix[] destination, int destinationIndex, int length, int stride)
		{
			fixed (Matrix* destinationPtr = destination)
			{
				Matrix* dst = destinationPtr + destinationIndex;
				Matrix* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = *src;
					src += stride;
				}
			}
		}
	}

	unsafe class Matrix4x4InStream : UnmanagedInStream<Matrix4x4>
	{
		private Matrix* FUnmanagedArray;
		
		public Matrix4x4InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override Matrix4x4 Read(int stride)
		{
			Debug.Assert(ReadPosition < Length);
			var result = FUnmanagedArray[ReadPosition].ToMatrix4x4();
			ReadPosition += stride;
			return result;
		}
		
		protected override void Copy(Matrix4x4[] destination, int destinationIndex, int length, int stride)
		{
			fixed (Matrix4x4* destinationPtr = destination)
			{
				Matrix4x4* dst = destinationPtr + destinationIndex;
				Matrix* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = (*src).ToMatrix4x4();
					src += stride;
				}
			}
		}
	}
}
