using System;
using VVVV.Utils.Streams;
using Xenko.Core.Mathematics;
using VVVV.Hosting.IO.Streams;
using VVVV.Utils.VColor;

namespace VVVV.VL.Hosting.IO.Streams
{
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

    unsafe class Color4OutStream : UnmanagedOutStream<Color4>
    {
        class Color4OutWriter : UnmanagedOutWriter
        {
            private readonly RGBAColor* FPDst;

            public Color4OutWriter(Color4OutStream stream, RGBAColor* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }

            public override void Write(Color4 value, int stride)
            {
                RGBAColor* dst = FPDst + Position;
                dst->R = value.R;
                dst->G = value.G;
                dst->B = value.B;
                dst->A = value.A;
                Position += stride;
            }

            protected override void Copy(Color4[] source, int sourceIndex, int length, int stride)
            {
                fixed (Color4* sourcePtr = source)
                {
                    Color4* src = sourcePtr + sourceIndex;
                    RGBAColor* dst = FPDst + Position;

                    for (int i = 0; i < length; i++)
                    {
                        dst->R = src->R;
                        dst->G = src->G;
                        dst->B = src->B;
                        dst->A = src->A;
                        src++;
                        dst += stride;
                    }
                }
            }
        }

        private readonly RGBAColor** FPPDst;

        public Color4OutStream(RGBAColor** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }

        public override IStreamWriter<Color4> GetWriter()
        {
            return new Color4OutWriter(this, *FPPDst);
        }
    }

}
