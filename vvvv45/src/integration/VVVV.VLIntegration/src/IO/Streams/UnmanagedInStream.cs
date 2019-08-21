using Xenko.Core.Mathematics;
using System;
using System.Diagnostics;
using VVVV.Hosting.IO.Streams;
using VVVV.Utils.Streams;
using System.Runtime.CompilerServices;

namespace VVVV.VL.Hosting.IO.Streams
{
    // TODO: Matrix4 needs to be transposed I guess.
    unsafe class MatrixInStream : UnmanagedInStream<Matrix>
    {
        class MatrixInStreamReader : UnmanagedInStreamReader
        {
            private readonly Matrix* FPData;
            
            public MatrixInStreamReader(MatrixInStream stream, Matrix* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override Matrix Read(int stride)
            {
                Debug.Assert(Position < Length);
                Matrix result;
                TransposeCopy(FPData + Position, &result);
                Position += stride;
                return result;
            }
            
            protected override void Copy(Matrix[] destination, int destinationIndex, int length, int stride)
            {
                fixed (Matrix* destinationPtr = destination)
                {
                    Matrix* dst = destinationPtr + destinationIndex;
                    Matrix* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        TransposeCopy(src, dst);
                        dst++;
                        src += stride;
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
        
        private readonly Matrix** FPPData;
        
        public MatrixInStream(int* pLength, Matrix** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<Matrix> GetReader()
        {
            return new MatrixInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new MatrixInStream(FPLength, FPPData, FValidateFunc);
        }
    }
}
