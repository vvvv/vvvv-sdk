
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
		public static IInStream<T> Create(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
		{
			object result = null;
			
			var type = typeof(T);
			if (type == typeof(double))
				result = new DoubleInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(float))
				result = new FloatInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(int))
				result = new IntInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(bool))
				result = new BoolInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Vector2))
				result = new Vector2InStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Vector3))
				result = new Vector3InStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Vector4))
				result = new Vector4InStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Vector2D))
				result = new Vector2DInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Vector3D))
				result = new Vector3DInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Vector4D))
				result = new Vector4DInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Matrix))
				result = new MatrixInStream(getUnmanagedArrayFunc, validateAction);
			else if (type == typeof(Matrix4x4))
				result = new Matrix4x4InStream(getUnmanagedArrayFunc, validateAction);
			else
				throw new NotSupportedException(string.Format("UnmanagedInStream of type '{0}' is not supported.", type));
			
			return result as IInStream<T>;
		}
		
		private readonly Func<Tuple<IntPtr, int>> FGetUnmanagedArrayFunc;
		private readonly Action FValidateAction;
		protected IntPtr FUnmanagedArrayPtr;
		
		public UnmanagedInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
		{
			FGetUnmanagedArrayFunc = getUnmanagedArrayFunc;
			FValidateAction = validateAction;
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
		
		public abstract T Read(int stepSize);
		
		public void ReadCyclic(T[] buffer, int index, int length, int stepSize)
		{
			StreamUtils.ReadCyclic(this, buffer, index, length, stepSize);
		}
		
		protected abstract void Copy(T[] destination, int destinationIndex, int length, int stepSize);
		
		protected abstract void Synced(IntPtr unmanagedArray, int unmanagedArrayLength);
		
		public int Read(T[] buffer, int index, int length, int stepSize)
		{
			int numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			
			switch (stepSize)
			{
				case 0:
					if (index == 0 && numSlicesToRead == buffer.Length)
						buffer.Init(Read(stepSize)); // Slightly faster
					else
						buffer.Fill(index, numSlicesToRead, Read(stepSize));
					break;
				default:
					Debug.Assert(ReadPosition + numSlicesToRead <= Length);
					Copy(buffer, index, numSlicesToRead, stepSize);
					ReadPosition += numSlicesToRead * stepSize;
					break;
			}
			
			return numSlicesToRead;
		}
		
		public void Sync()
		{
			FValidateAction();
			
			var result = FGetUnmanagedArrayFunc();
			FUnmanagedArrayPtr = result.Item1;
			Length = result.Item2;
			
			Synced(FUnmanagedArrayPtr, Length);
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
		
		public DoubleInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
			: base(getUnmanagedArrayFunc, validateAction)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override double Read(int stepSize)
		{
			Debug.Assert(ReadPosition < Length);
			var result = FUnmanagedArray[ReadPosition];
			ReadPosition += stepSize;
			return result;
		}
		
		protected override void Copy(double[] destination, int destinationIndex, int length, int stepSize)
		{
			switch (stepSize)
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
							src += stepSize;
						}
					}
					break;
			}
		}
	}
	
	unsafe class FloatInStream : UnmanagedInStream<float>
	{
		private double* FUnmanagedArray;
		
		public FloatInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
			: base(getUnmanagedArrayFunc, validateAction)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override float Read(int stepSize)
		{
			Debug.Assert(ReadPosition < Length);
			var result = (float) FUnmanagedArray[ReadPosition];
			ReadPosition += stepSize;
			return result;
		}
		
		protected override void Copy(float[] destination, int destinationIndex, int length, int stepSize)
		{
			fixed (float* destinationPtr = destination)
			{
				float* dst = destinationPtr + destinationIndex;
				double* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = (float) *src;
					src += stepSize;
				}
			}
		}
	}

	unsafe class IntInStream : UnmanagedInStream<int>
	{
		private double* FUnmanagedArray;
		
		public IntInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
			: base(getUnmanagedArrayFunc, validateAction)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override int Read(int stepSize)
		{
			Debug.Assert(ReadPosition < Length);
			var result = (int) Math.Round(FUnmanagedArray[ReadPosition]);
			ReadPosition += stepSize;
			return result;
		}
		
		protected override void Copy(int[] destination, int destinationIndex, int length, int stepSize)
		{
			fixed (int* destinationPtr = destination)
			{
				int* dst = destinationPtr + destinationIndex;
				double* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = (int) Math.Round(*src);
					src += stepSize;
				}
			}
		}
	}

	unsafe class BoolInStream : UnmanagedInStream<bool>
	{
		private double* FUnmanagedArray;
		
		public BoolInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
			: base(getUnmanagedArrayFunc, validateAction)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (double*) unmanagedArray.ToPointer();
		}
		
		public override bool Read(int stepSize)
		{
			Debug.Assert(ReadPosition < Length);
			var result = FUnmanagedArray[ReadPosition] >= 0.5;
			ReadPosition += stepSize;
			return result;
		}
		
		protected override void Copy(bool[] destination, int destinationIndex, int length, int stepSize)
		{
			fixed (bool* destinationPtr = destination)
			{
				bool* dst = destinationPtr + destinationIndex;
				double* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = *src >= 0.5;
					src += stepSize;
				}
			}
		}
	}

	unsafe class MatrixInStream : UnmanagedInStream<Matrix>
	{
		private Matrix* FUnmanagedArray;
		
		public MatrixInStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
			: base(getUnmanagedArrayFunc, validateAction)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override Matrix Read(int stepSize)
		{
			Debug.Assert(ReadPosition < Length);
			var result = FUnmanagedArray[ReadPosition];
			ReadPosition += stepSize;
			return result;
		}
		
		protected override void Copy(Matrix[] destination, int destinationIndex, int length, int stepSize)
		{
			fixed (Matrix* destinationPtr = destination)
			{
				Matrix* dst = destinationPtr + destinationIndex;
				Matrix* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = *src;
					src += stepSize;
				}
			}
		}
	}

	unsafe class Matrix4x4InStream : UnmanagedInStream<Matrix4x4>
	{
		private Matrix* FUnmanagedArray;
		
		public Matrix4x4InStream(Func<Tuple<IntPtr, int>> getUnmanagedArrayFunc, Action validateAction)
			: base(getUnmanagedArrayFunc, validateAction)
		{
			
		}
		
		protected override void Synced(IntPtr unmanagedArray, int unmanagedArrayLength)
		{
			FUnmanagedArray = (Matrix*) unmanagedArray.ToPointer();
		}
		
		public override Matrix4x4 Read(int stepSize)
		{
			Debug.Assert(ReadPosition < Length);
			var result = FUnmanagedArray[ReadPosition].ToMatrix4x4();
			ReadPosition += stepSize;
			return result;
		}
		
		protected override void Copy(Matrix4x4[] destination, int destinationIndex, int length, int stepSize)
		{
			fixed (Matrix4x4* destinationPtr = destination)
			{
				Matrix4x4* dst = destinationPtr + destinationIndex;
				Matrix* src = FUnmanagedArray + ReadPosition;
				
				for (int i = 0; i < length; i++)
				{
					*(dst++) = (*src).ToMatrix4x4();
					src += stepSize;
				}
			}
		}
	}
}
