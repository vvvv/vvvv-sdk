using Xenko.Core.Mathematics;
using System;
using VVVV.Hosting.IO.Streams;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;

namespace VVVV.VL.Hosting.IO.Streams
{
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

    unsafe class Color4InStream : UnmanagedInStream<Color4>
    {
        class Color4InStreamReader : UnmanagedInStreamReader
        {
            private readonly RGBAColor* FPData;

            public Color4InStreamReader(Color4InStream stream, RGBAColor* pData)
                : base(stream)
            {
                FPData = pData;
            }

            public override Color4 Read(int stride)
            {
                RGBAColor* src = FPData + Position;
                Position += stride;
                return new Color4((float)src->R, (float)src->G, (float)src->B, (float)src->A);
            }

            protected override void Copy(Color4[] destination, int destinationIndex, int length, int stride)
            {
                fixed (Color4* destinationPtr = destination)
                {
                    Color4* dst = destinationPtr + destinationIndex;
                    RGBAColor* src = FPData + Position;

                    for (int i = 0; i < length; i++)
                    {
                        dst->R = (float)src->R;
                        dst->G = (float)src->G;
                        dst->B = (float)src->B;
                        dst->A = (float)src->A;
                        dst++;
                        src += stride;
                    }
                }
            }
        }

        private readonly RGBAColor** FPPData;

        public Color4InStream(int* pLength, RGBAColor** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }

        public override IStreamReader<Color4> GetReader()
        {
            return new Color4InStreamReader(this, *FPPData);
        }

        public override object Clone()
        {
            return new Color4InStream(FPLength, FPPData, FValidateFunc);
        }
    }
}
