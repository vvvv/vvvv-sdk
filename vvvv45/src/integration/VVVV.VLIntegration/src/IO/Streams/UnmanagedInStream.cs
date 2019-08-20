using Xenko.Core.Mathematics;
using System;
using System.Diagnostics;
using VVVV.Hosting.IO.Streams;
using VVVV.Utils.Streams;

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
                var result = FPData[Position];
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
                        *(dst++) = *src;
                        src += stride;
                    }
                }
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
