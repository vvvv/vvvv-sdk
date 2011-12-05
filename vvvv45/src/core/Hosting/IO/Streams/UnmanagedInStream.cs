
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SlimDX;
using VVVV.Utils;
using VVVV.Utils.SlimDX;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO.Streams
{
	unsafe abstract class UnmanagedInStream<T> : IInStream<T>
	{
		internal abstract class UnmanagedInStreamReader : IStreamReader<T>
		{
			private readonly UnmanagedInStream<T> FStream;
			
			public UnmanagedInStreamReader(UnmanagedInStream<T> stream)
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
			
			public int Read(T[] buffer, int index, int length, int stride)
			{
				int slicesAhead = Length - Position;
				
				if (stride > 1)
				{
					int r = 0;
					slicesAhead = Math.DivRem(slicesAhead, stride, out r);
					if (r > 0)
						slicesAhead++;
				}
				
				int numSlicesToRead = Math.Max(Math.Min(length, slicesAhead), 0);
				
				switch (numSlicesToRead)
				{
					case 0:
						return 0;
					case 1:
						buffer[index] = Read(stride);
						return 1;
					default:
						switch (stride)
						{
							case 0:
								if (index == 0 && numSlicesToRead == buffer.Length)
									buffer.Init(Read(stride)); // Slightly faster
								else
									buffer.Fill(index, numSlicesToRead, Read(stride));
								break;
							default:
								Debug.Assert(Position + numSlicesToRead <= Length);
								Copy(buffer, index, numSlicesToRead, stride);
								Position += numSlicesToRead * stride;
								break;
						}
						break;
				}
				
				return numSlicesToRead;
			}
			
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
			
			protected abstract void Copy(T[] destination, int destinationIndex, int length, int stride);
			
			public void Dispose()
			{
				FStream.FRefCount--;
			}
			
			public void Reset()
			{
				Position = 0;
			}
		}
		
		private readonly Func<Tuple<IntPtr, int>> FGetUnmanagedArrayFunc;
		private readonly Func<bool> FValidateFunc;
		protected int FRefCount;
		protected IntPtr FUnmanagedArrayPtr;
		
		public UnmanagedInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
		{
			FGetUnmanagedArrayFunc = getUnmanagedArrayFunc;
			FValidateFunc = validateFunc;
		}
		
		public int RefCount
		{
			get
			{
				return FRefCount;
			}
		}
		
		public int Length
		{
			get;
			private set;
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
		
		public object Clone()
		{
			return UnmanagedInStream.Create<T>(FGetUnmanagedArrayFunc, FValidateFunc);
		}
		
		protected abstract void Synced(IntPtr unmanagedArray, int unmanagedArrayLength);
		
		public abstract IStreamReader<T> GetReader();
		
		public IEnumerator<T> GetEnumerator()
		{
			return GetReader();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	
	unsafe class DoubleInStream : UnmanagedInStream<double>
	{
		class DoubleInStreamReader : UnmanagedInStreamReader
		{
			private readonly double* FUnmanagedArray;
			
			public DoubleInStreamReader(DoubleInStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override double Read(int stride)
			{
				Debug.Assert(Position < Length);
				var result = FUnmanagedArray[Position];
				Position += stride;
				return result;
			}
			
			protected override void Copy(double[] destination, int destinationIndex, int length, int stride)
			{
				switch (stride)
				{
					case 1:
						Marshal.Copy(new IntPtr(FUnmanagedArray + Position), destination, destinationIndex, length);
						break;
					default:
						fixed (double* destinationPtr = destination)
						{
							double* dst = destinationPtr + destinationIndex;
							double* src = FUnmanagedArray + Position;
							
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
		
		private double* FUnmanagedArray;
		
		public DoubleInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override IStreamReader<double> GetReader()
		{
			FRefCount++;
			return new DoubleInStreamReader(this);
		}
	}
	
	unsafe class FloatInStream : UnmanagedInStream<float>
	{
		class FloatInStreamReader : UnmanagedInStreamReader
		{
			private readonly double* FUnmanagedArray;
			
			public FloatInStreamReader(FloatInStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override float Read(int stride)
			{
				Debug.Assert(Position < Length);
				var result = (float) FUnmanagedArray[Position];
				Position += stride;
				return result;
			}
			
			protected override void Copy(float[] destination, int destinationIndex, int length, int stride)
			{
				fixed (float* destinationPtr = destination)
				{
					float* dst = destinationPtr + destinationIndex;
					double* src = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*(dst++) = (float) *src;
						src += stride;
					}
				}
			}
		}
		
		private double* FUnmanagedArray;
		
		public FloatInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override IStreamReader<float> GetReader()
		{
			FRefCount++;
			return new FloatInStreamReader(this);
		}
	}

	unsafe class IntInStream : UnmanagedInStream<int>
	{
		class IntInStreamReader : UnmanagedInStreamReader
		{
			private readonly double* FUnmanagedArray;
			
			public IntInStreamReader(IntInStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override int Read(int stride)
			{
				Debug.Assert(Position < Length);
				var result = (int) Math.Round(FUnmanagedArray[Position]);
				Position += stride;
				return result;
			}
			
			protected override void Copy(int[] destination, int destinationIndex, int length, int stride)
			{
				fixed (int* destinationPtr = destination)
				{
					int* dst = destinationPtr + destinationIndex;
					double* src = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*(dst++) = (int) Math.Round(*src);
						src += stride;
					}
				}
			}
		}
		
		private double* FUnmanagedArray;
		
		public IntInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override IStreamReader<int> GetReader()
		{
			FRefCount++;
			return new IntInStreamReader(this);
		}
	}

	unsafe class BoolInStream : UnmanagedInStream<bool>
	{
		class BoolInStreamReader : UnmanagedInStreamReader
		{
			private readonly double* FUnmanagedArray;
			
			public BoolInStreamReader(BoolInStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override bool Read(int stride)
			{
				Debug.Assert(Position < Length, string.Format("Position: {0}, Length: {0}", Position, Length));
				var result = FUnmanagedArray[Position] >= 0.5;
				Position += stride;
				return result;
			}
			
			protected override void Copy(bool[] destination, int destinationIndex, int length, int stride)
			{
				fixed (bool* destinationPtr = destination)
				{
					bool* dst = destinationPtr + destinationIndex;
					double* src = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*(dst++) = *src >= 0.5;
						src += stride;
					}
				}
			}
		}
		
		private double* FUnmanagedArray;
		
		public BoolInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override IStreamReader<bool> GetReader()
		{
			FRefCount++;
			return new BoolInStreamReader(this);
		}
	}
	
	unsafe class ColorInStream : UnmanagedInStream<RGBAColor>
	{
		class ColorInStreamReader : UnmanagedInStreamReader
		{
			private readonly RGBAColor* FUnmanagedArray;
			
			public ColorInStreamReader(ColorInStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override RGBAColor Read(int stride)
			{
				Debug.Assert(Position < Length);
				var result = FUnmanagedArray[Position];
				Position += stride;
				return result;
			}
			
			protected override void Copy(RGBAColor[] destination, int destinationIndex, int length, int stride)
			{
				fixed (RGBAColor* destinationPtr = destination)
				{
					RGBAColor* dst = destinationPtr + destinationIndex;
					RGBAColor* src = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*(dst++) = *src;
						src += stride;
					}
				}
			}
		}
		
		private RGBAColor* FUnmanagedArray;
		
		public ColorInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (RGBAColor*) unmanagedArray.ToPointer();
		}
		
		public override IStreamReader<RGBAColor> GetReader()
		{
			FRefCount++;
			return new ColorInStreamReader(this);
		}
	}

	unsafe class MatrixInStream : UnmanagedInStream<Matrix>
	{
		class MatrixInStreamReader : UnmanagedInStreamReader
		{
			private readonly Matrix* FUnmanagedArray;
			
			public MatrixInStreamReader(MatrixInStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override Matrix Read(int stride)
			{
				Debug.Assert(Position < Length);
				var result = FUnmanagedArray[Position];
				Position += stride;
				return result;
			}
			
			protected override void Copy(Matrix[] destination, int destinationIndex, int length, int stride)
			{
				fixed (Matrix* destinationPtr = destination)
				{
					Matrix* dst = destinationPtr + destinationIndex;
					Matrix* src = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*(dst++) = *src;
						src += stride;
					}
				}
			}
		}
		
		private Matrix* FUnmanagedArray;
		
		public MatrixInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override IStreamReader<Matrix> GetReader()
		{
			FRefCount++;
			return new MatrixInStreamReader(this);
		}
	}

	unsafe class Matrix4x4InStream : UnmanagedInStream<Matrix4x4>
	{
		class Matrix4x4InStreamReader : UnmanagedInStreamReader
		{
			private readonly Matrix* FUnmanagedArray;
			
			public Matrix4x4InStreamReader(Matrix4x4InStream stream)
				: base(stream)
			{
				FUnmanagedArray = stream.FUnmanagedArray;
			}
			
			public override Matrix4x4 Read(int stride)
			{
				Debug.Assert(Position < Length);
				var result = FUnmanagedArray[Position].ToMatrix4x4();
				Position += stride;
				return result;
			}
			
			protected override void Copy(Matrix4x4[] destination, int destinationIndex, int length, int stride)
			{
				fixed (Matrix4x4* destinationPtr = destination)
				{
					Matrix4x4* dst = destinationPtr + destinationIndex;
					Matrix* src = FUnmanagedArray + Position;
					
					for (int i = 0; i < length; i++)
					{
						*(dst++) = (*src).ToMatrix4x4();
						src += stride;
					}
				}
			}
		}
		
		private Matrix* FUnmanagedArray;
		
		public Matrix4x4InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Func<bool> validateFunc)
			: base(getUnmanagedArrayFunc, validateFunc)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override IStreamReader<Matrix4x4> GetReader()
		{
			FRefCount++;
			return new Matrix4x4InStreamReader(this);
		}
	}
}
