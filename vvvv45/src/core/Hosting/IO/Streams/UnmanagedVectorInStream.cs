
using System;
using SlimDX;
using VVVV.Utils;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO.Streams
{
    public unsafe abstract class VectorInStream<T> : IInStream<T> where T : struct
    {
        public abstract class VectorInStreamReader : IStreamReader<T>
        {
            private readonly VectorInStream<T> FStream;
            protected readonly int FDimension;
            protected readonly double* FPSrc;
            protected readonly int FSrcLength;
            protected readonly int FUnderFlow;
            
            public VectorInStreamReader(VectorInStream<T> stream, int srcLength, double* pSrc)
            {
                FStream = stream;
                FDimension = stream.FDimension;
                Length = stream.Length;
                // Underflow is set after Length property was read.
                FUnderFlow = stream.FUnderFlow;
                FSrcLength = srcLength;
                FPSrc = pSrc;
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
            
            protected int Read(double* pDst, int index, int length, int stride)
            {
                double* src = FPSrc + Position * FDimension;
                double* dst = pDst + index * FDimension;
                
                int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                int numSlicesToReadAtFullSpeed = numSlicesToRead;
                
                if (IsOutOfBounds(numSlicesToRead))
                {
                    numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
                }
                
                for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
                {
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = *(src + j);
                    }
                    src += FDimension * stride;
                }
                
                if (numSlicesToReadAtFullSpeed < numSlicesToRead)
                {
                    int i = FSrcLength - FUnderFlow;
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = FPSrc[i++ % FSrcLength];
                    }
                }
                
                Position += numSlicesToRead * stride;
                return numSlicesToRead;
            }
            
            protected void Read(double* pDst, int stride)
            {
                double* src = FPSrc + Position * FDimension;
                double* dst = pDst;
                
                if (IsOutOfBounds(1))
                {
                    int i = FSrcLength - FUnderFlow;
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = FPSrc[i++ % FSrcLength];
                    }
                }
                else
                {
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = *(src++);
                    }
                }
                
                Position += stride;
            }
            
            protected int Read(float* pDst, int index, int length, int stride)
            {
                double* src = FPSrc + Position * FDimension;
                float* dst = pDst + index * FDimension;
                
                int numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                int numSlicesToReadAtFullSpeed = numSlicesToRead;
                
                if (IsOutOfBounds(numSlicesToRead))
                {
                    numSlicesToReadAtFullSpeed = Math.Max(numSlicesToReadAtFullSpeed - 1, 0);
                }
                
                for (int i = 0; i < numSlicesToReadAtFullSpeed; i++)
                {
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = (float) *(src + j);
                    }
                    src += FDimension * stride;
                }
                
                if (numSlicesToReadAtFullSpeed < numSlicesToRead)
                {
                    int i = FSrcLength - FUnderFlow;
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = (float) FPSrc[i++ % FSrcLength];
                    }
                }
                
                Position += numSlicesToRead * stride;
                return numSlicesToRead;
            }
            
            protected void Read(float* pDst, int stride)
            {
                double* src = FPSrc + Position * FDimension;
                float* dst = pDst;
                
                if (IsOutOfBounds(1))
                {
                    int i = FSrcLength - FUnderFlow;
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = (float) FPSrc[i++ % FSrcLength];
                    }
                }
                else
                {
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst++) = (float) *(src++);
                    }
                }
                
                Position += stride;
            }
            
            public abstract T Read(int stride = 1);
            
            public abstract int Read(T[] buffer, int index, int length, int stride);
            
            public void Dispose()
            {
                
            }
            
            public void Reset()
            {
                Position = 0;
            }
            
            protected bool IsOutOfBounds(int numSlicesToWorkOn)
            {
                return (FUnderFlow > 0) && ((Position + numSlicesToWorkOn) > (Length - 1));
            }
        }
        
        private readonly Func<bool> FValidateFunc;
        protected readonly int FDimension;
        protected readonly int* FPLength;
        protected readonly double** FPPSrc;
        protected int FUnderFlow;
        
        public VectorInStream(int dimension, int* pLength, double** ppSrc, Func<bool> validateFunc)
        {
            FDimension = dimension;
            FPLength = pLength;
            FPPSrc = ppSrc;
            FValidateFunc = validateFunc;
            IsChanged = true;
        }
        
        public object Clone()
        {
            throw new NotImplementedException();
        }
        
        public bool Sync()
		{
			IsChanged = FValidateFunc();
			return IsChanged;
		}
		
		public bool IsChanged
		{
		    get;
		    private set;
		}
        
        public int Length
        {
            get
            {
                int length = Math.DivRem(*FPLength, FDimension, out FUnderFlow);
                return FUnderFlow > 0 ? length + 1 : length;
            }
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
            public Vector2DInStreamReader(Vector2DInStream stream, int srcLength, double* pSrc)
                : base(stream, srcLength, pSrc)
            {
                
            }
            
            public override int Read(Vector2D[] buffer, int index, int length, int stride)
            {
                fixed (Vector2D* destination = buffer)
                {
                    return Read((double*) destination, index, length, stride);
                }
            }
            
            public override Vector2D Read(int stride)
            {
                Vector2D result;
                Read((double*) &result, stride);
                return result;
            }
        }
        
        public Vector2DInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(2, pLength, ppData, validateFunc)
        {
            
        }
        
        public override IStreamReader<Vector2D> GetReader()
        {
            return new Vector2DInStreamReader(this, *FPLength, *FPPSrc);
        }
    }
    
    unsafe class Vector3DInStream : VectorInStream<Vector3D>
    {
        class Vector3DInStreamReader : VectorInStreamReader
        {
            public Vector3DInStreamReader(Vector3DInStream stream, int srcLength, double* pSrc)
                : base(stream, srcLength, pSrc)
            {
                
            }
            
            public override int Read(Vector3D[] buffer, int index, int length, int stride)
            {
                fixed (Vector3D* destination = buffer)
                {
                    return Read((double*) destination, index, length, stride);
                }
            }
            
            public override Vector3D Read(int stride)
            {
                Vector3D result;
                Read((double*) &result, stride);
                return result;
            }
        }
        
        public Vector3DInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(3, pLength, ppData, validateFunc)
        {
            
        }
        
        public override IStreamReader<Vector3D> GetReader()
        {
            return new Vector3DInStreamReader(this, *FPLength, *FPPSrc);
        }
    }
    
    unsafe class Vector4DInStream : VectorInStream<Vector4D>
    {
        class Vector4DInStreamReader : VectorInStreamReader
        {
            public Vector4DInStreamReader(Vector4DInStream stream, int srcLength, double* pSrc)
                : base(stream, srcLength, pSrc)
            {
                
            }
            
            public override int Read(Vector4D[] buffer, int index, int length, int stride)
            {
                fixed (Vector4D* destination = buffer)
                {
                    return Read((double*) destination, index, length, stride);
                }
            }
            
            public override Vector4D Read(int stride)
            {
                Vector4D result;
                Read((double*) &result, stride);
                return result;
            }
        }
        
        public Vector4DInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(4, pLength, ppData, validateFunc)
        {
            
        }
        
        public override IStreamReader<Vector4D> GetReader()
        {
            return new Vector4DInStreamReader(this, *FPLength, *FPPSrc);
        }
    }
    
    unsafe class Vector2InStream : VectorInStream<Vector2>
    {
        class Vector2InStreamReader : VectorInStreamReader
        {
            public Vector2InStreamReader(Vector2InStream stream, int srcLength, double* pSrc)
                : base(stream, srcLength, pSrc)
            {
                
            }
            
            public override int Read(Vector2[] buffer, int index, int length, int stride)
            {
                fixed (Vector2* destination = buffer)
                {
                    return Read((float*) destination, index, length, stride);
                }
            }
            
            public override Vector2 Read(int stride)
            {
                Vector2 result;
                Read((float*) &result, stride);
                return result;
            }
        }
        
        public Vector2InStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(2, pLength, ppData, validateFunc)
        {
            
        }
        
        public override IStreamReader<Vector2> GetReader()
        {
            return new Vector2InStreamReader(this, *FPLength, *FPPSrc);
        }
    }
    
    unsafe class Vector3InStream : VectorInStream<Vector3>
    {
        class Vector3InStreamReader : VectorInStreamReader
        {
            public Vector3InStreamReader(Vector3InStream stream, int srcLength, double* pSrc)
                : base(stream, srcLength, pSrc)
            {
                
            }
            
            public override int Read(Vector3[] buffer, int index, int length, int stride)
            {
                fixed (Vector3* destination = buffer)
                {
                    return Read((float*) destination, index, length, stride);
                }
            }
            
            public override Vector3 Read(int stride)
            {
                Vector3 result;
                Read((float*) &result, stride);
                return result;
            }
        }
        
        public Vector3InStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(3, pLength, ppData, validateFunc)
        {
            
        }
        
        public override IStreamReader<Vector3> GetReader()
        {
            return new Vector3InStreamReader(this, *FPLength, *FPPSrc);
        }
    }
    
    unsafe class Vector4InStream : VectorInStream<Vector4>
    {
        class Vector4InStreamReader : VectorInStreamReader
        {
            public Vector4InStreamReader(Vector4InStream stream, int srcLength, double* pSrc)
                : base(stream, srcLength, pSrc)
            {
                
            }
            
            public override int Read(Vector4[] buffer, int index, int length, int stride)
            {
                fixed (Vector4* destination = buffer)
                {
                    return Read((float*) destination, index, length, stride);
                }
            }
            
            public override Vector4 Read(int stride)
            {
                Vector4 result;
                Read((float*) &result, stride);
                return result;
            }
        }
        
        public Vector4InStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(4, pLength, ppData, validateFunc)
        {
            
        }
        
        public override IStreamReader<Vector4> GetReader()
        {
            return new Vector4InStreamReader(this, *FPLength, *FPPSrc);
        }
    }
    
    unsafe class QuaternionInStream : VectorInStream<Quaternion>
    {
        class QuaternionInStreamReader : VectorInStreamReader
        {
            public QuaternionInStreamReader(QuaternionInStream stream, int srcLength, double* pSrc)
                : base(stream, srcLength, pSrc)
            {
                
            }
            
            public override int Read(Quaternion[] buffer, int index, int length, int stride)
            {
                fixed (Quaternion* destination = buffer)
                {
                    return Read((float*) destination, index, length, stride);
                }
            }
            
            public override Quaternion Read(int stride)
            {
                Quaternion result;
                Read((float*) &result, stride);
                return result;
            }
        }
        
        public QuaternionInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(4, pLength, ppData, validateFunc)
        {
            
        }
        
        public override IStreamReader<Quaternion> GetReader()
        {
            return new QuaternionInStreamReader(this, *FPLength, *FPPSrc);
        }
    }
}
