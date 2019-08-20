using System;
using System.Diagnostics;
using VVVV.Utils.Streams;
using Xenko.Core.Mathematics;
using VVVV.Hosting.IO.Streams;

namespace VVVV.VL.Hosting.IO.Streams
{
    unsafe class MatrixOutStream : UnmanagedOutStream<Matrix>
    {
        class MatrixOutWriter : UnmanagedOutWriter
        {
            private readonly Matrix* FPDst;
            
            public MatrixOutWriter(MatrixOutStream stream, Matrix* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(Matrix value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = value;
                Position += stride;
            }
            
            protected override void Copy(Matrix[] source, int sourceIndex, int length, int stride)
            {
                fixed (Matrix* sourcePtr = source)
                {
                    Matrix* src = sourcePtr + sourceIndex;
                    Matrix* dst = FPDst + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *dst = *(src++);
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly Matrix** FPPDst;
        
        public MatrixOutStream(Matrix** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<Matrix> GetWriter()
        {
            return new MatrixOutWriter(this, *FPPDst);
        }
    }
}
