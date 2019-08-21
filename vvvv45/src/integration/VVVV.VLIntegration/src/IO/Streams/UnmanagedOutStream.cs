using System;
using System.Diagnostics;
using VVVV.Utils.Streams;
using Xenko.Core.Mathematics;
using VVVV.Hosting.IO.Streams;
using System.Runtime.CompilerServices;

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
                TransposeCopy(&value, FPDst + Position);
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
                        TransposeCopy(src, dst);
                        src++;
                        dst += stride;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void TransposeCopy(Matrix* src, Matrix* dst)
            {
                dst->M11 = src->M11; dst->M21 = src->M12; dst->M31 = src->M13; dst->M41 = src->M14;
                dst->M12 = src->M21; dst->M22 = src->M22; dst->M32 = src->M23; dst->M42 = src->M24;
                dst->M13 = src->M31; dst->M23 = src->M32; dst->M33 = src->M33; dst->M43 = src->M34;
                dst->M14 = src->M41; dst->M24 = src->M42; dst->M34 = src->M43; dst->M44 = src->M44;
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
