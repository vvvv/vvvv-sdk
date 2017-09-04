
using System;
using SlimDX;
using VVVV.Utils.SlimDX;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO.Streams
{
    public unsafe abstract class VectorOutStream<T> : IOutStream<T> where T : struct
    {
        public abstract class VectorOutStreamWriter : IStreamWriter<T>
        {
            private readonly VectorOutStream<T> FStream;
            private readonly int FDimension;
            private readonly double* FPDst;
            private readonly int FDstLength;
            
            public VectorOutStreamWriter(VectorOutStream<T> stream, double* pDst)
            {
                FStream = stream;
                Length = stream.Length;
                FPDst = pDst;
                FDimension = stream.FDimension;
                FDstLength = Length * FDimension;
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
            
            public void Dispose()
            {
                
            }
            
            public void Reset()
            {
                Position = 0;
            }
            
            protected int Write(double* pSrc, int index, int length, int stride)
            {
                double* src = pSrc + index * FDimension;
                double* dst = FPDst + Position * FDimension;
                
                int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                
                for (int i = 0; i < numSlicesToWrite; i++)
                {
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst + j) = *(src++);
                    }
                    dst += FDimension * stride;
                }
                
                Position += numSlicesToWrite * stride;
                return numSlicesToWrite;
            }
            
            protected void Write(double* pSrc, int stride)
            {
                double* src = pSrc;
                double* dst = FPDst + Position * FDimension;
                
                for (int j = 0; j < FDimension; j++)
                {
                    *(dst++) = *(src++);
                }
                
                Position += stride;
            }
            
            protected int Write(float* pSrc, int index, int length, int stride)
            {
                float* src = pSrc + index * FDimension;
                double* dst = FPDst + Position * FDimension;
                
                int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                
                for (int i = 0; i < numSlicesToWrite; i++)
                {
                    for (int j = 0; j < FDimension; j++)
                    {
                        *(dst + j) = (double) *(src++);
                    }
                    dst += FDimension * stride;
                }
                
                Position += numSlicesToWrite * stride;
                return numSlicesToWrite;
            }
            
            protected void Write(float* pSrc, int stride)
            {
                float* src = pSrc;
                double* dst = FPDst + Position * FDimension;
                
                for (int j = 0; j < FDimension; j++)
                {
                    *(dst++) = (double) *(src++);
                }
                
                Position += stride;
            }
            
            public abstract void Write(T value, int stride = 1);
            
            public abstract int Write(T[] buffer, int index, int length, int stride = 1);
        }
        
        private readonly Action<int> FSetDstLengthAction;
        protected readonly int FDimension;
        protected readonly double** FPPDst;
        protected int FDstLength;
        protected int FLength;
        
        public VectorOutStream(int dimension, double** ppDst, Action<int> setDstLengthAction)
        {
            FDimension = dimension;
            FPPDst = ppDst;
            FSetDstLengthAction = setDstLengthAction;
            FSetDstLengthAction(FLength);
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
                    FSetDstLengthAction(value);
                    FLength = value;
                    FDstLength = value * FDimension;
                }
            }
        }

        public void Flush(bool force = false)
        {
            // No need. We write to the unmanaged array directly.
        }
        
        public abstract IStreamWriter<T> GetWriter();
    }
    
    unsafe class Vector2DOutStream : VectorOutStream<Vector2D>
    {
        class Vector2DOutStreamWriter : VectorOutStreamWriter
        {
            public Vector2DOutStreamWriter(Vector2DOutStream stream, double* pDst)
                : base(stream, pDst)
            {
                
            }
            
            public override int Write(Vector2D[] buffer, int index, int length, int stride)
            {
                fixed (Vector2D* source = buffer)
                {
                    return Write((double*) source, index, length, stride);
                }
            }
            
            public override void Write(Vector2D value, int stride)
            {
                Write((double*) &value, stride);
            }
        }
        
        public Vector2DOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(2, ppDst, setDstLengthAction)
        {
            
        }
        
        public override IStreamWriter<Vector2D> GetWriter()
        {
            return new Vector2DOutStreamWriter(this, *FPPDst);
        }
    }
    
    unsafe class Vector3DOutStream : VectorOutStream<Vector3D>
    {
        class Vector3DOutStreamWriter : VectorOutStreamWriter
        {
            public Vector3DOutStreamWriter(Vector3DOutStream stream, double* pDst)
                : base(stream, pDst)
            {
                
            }
            
            public override int Write(Vector3D[] buffer, int index, int length, int stride)
            {
                fixed (Vector3D* source = buffer)
                {
                    return Write((double*) source, index, length, stride);
                }
            }
            
            public override void Write(Vector3D value, int stride)
            {
                Write((double*) &value, stride);
            }
        }
        
        public Vector3DOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(3, ppDst, setDstLengthAction)
        {
            
        }
        
        public override IStreamWriter<Vector3D> GetWriter()
        {
            return new Vector3DOutStreamWriter(this, *FPPDst);
        }
    }
    
    unsafe class Vector4DOutStream : VectorOutStream<Vector4D>
    {
        class Vector4DOutStreamWriter : VectorOutStreamWriter
        {
            public Vector4DOutStreamWriter(Vector4DOutStream stream, double* pDst)
                : base(stream, pDst)
            {
                
            }
            
            public override int Write(Vector4D[] buffer, int index, int length, int stride)
            {
                fixed (Vector4D* source = buffer)
                {
                    return Write((double*) source, index, length, stride);
                }
            }
            
            public override void Write(Vector4D value, int stride)
            {
                Write((double*) &value, stride);
            }
        }
        
        public Vector4DOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(4, ppDst, setDstLengthAction)
        {
            
        }
        
        public override IStreamWriter<Vector4D> GetWriter()
        {
            return new Vector4DOutStreamWriter(this, *FPPDst);
        }
    }
    
    unsafe class Vector2OutStream : VectorOutStream<Vector2>
    {
        class Vector2OutStreamWriter : VectorOutStreamWriter
        {
            public Vector2OutStreamWriter(Vector2OutStream stream, double* pDst)
                : base(stream, pDst)
            {
                
            }
            
            public override int Write(Vector2[] buffer, int index, int length, int stride)
            {
                fixed (Vector2* source = buffer)
                {
                    return Write((float*) source, index, length, stride);
                }
            }
            
            public override void Write(Vector2 value, int stride)
            {
                Write((float*) &value, stride);
            }
        }
        
        public Vector2OutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(2, ppDst, setDstLengthAction)
        {
            
        }
        
        public override IStreamWriter<Vector2> GetWriter()
        {
            return new Vector2OutStreamWriter(this, *FPPDst);
        }
    }
    
    unsafe class Vector3OutStream : VectorOutStream<Vector3>
    {
        class Vector3OutStreamWriter : VectorOutStreamWriter
        {
            public Vector3OutStreamWriter(Vector3OutStream stream, double* pDst)
                : base(stream, pDst)
            {
                
            }
            
            public override int Write(Vector3[] buffer, int index, int length, int stride)
            {
                fixed (Vector3* source = buffer)
                {
                    return Write((float*) source, index, length, stride);
                }
            }
            
            public override void Write(Vector3 value, int stride)
            {
                Write((float*) &value, stride);
            }
        }
        
        public Vector3OutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(3, ppDst, setDstLengthAction)
        {
            
        }
        
        public override IStreamWriter<Vector3> GetWriter()
        {
            return new Vector3OutStreamWriter(this, *FPPDst);
        }
    }
    
    unsafe class Vector4OutStream : VectorOutStream<Vector4>
    {
        class Vector4OutStreamWriter : VectorOutStreamWriter
        {
            public Vector4OutStreamWriter(Vector4OutStream stream, double* pDst)
                : base(stream, pDst)
            {
                
            }
            
            public override int Write(Vector4[] buffer, int index, int length, int stride)
            {
                fixed (Vector4* source = buffer)
                {
                    return Write((float*) source, index, length, stride);
                }
            }
            
            public override void Write(Vector4 value, int stride)
            {
                Write((float*) &value, stride);
            }
        }
        
        public Vector4OutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(4, ppDst, setDstLengthAction)
        {
            
        }
        
        public override IStreamWriter<Vector4> GetWriter()
        {
            return new Vector4OutStreamWriter(this, *FPPDst);
        }
    }
    
    unsafe class QuaternionOutStream : VectorOutStream<Quaternion>
    {
        class QuaternionOutStreamWriter : VectorOutStreamWriter
        {
            public QuaternionOutStreamWriter(QuaternionOutStream stream, double* pDst)
                : base(stream, pDst)
            {
                
            }
            
            public override int Write(Quaternion[] buffer, int index, int length, int stride)
            {
                fixed (Quaternion* source = buffer)
                {
                    return Write((float*) source, index, length, stride);
                }
            }
            
            public override void Write(Quaternion value, int stride)
            {
                Write((float*) &value, stride);
            }
        }
        
        public QuaternionOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(4, ppDst, setDstLengthAction)
        {
            
        }
        
        public override IStreamWriter<Quaternion> GetWriter()
        {
            return new QuaternionOutStreamWriter(this, *FPPDst);
        }
    }
}
