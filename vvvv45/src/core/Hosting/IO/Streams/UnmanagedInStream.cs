
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
            private UnmanagedInStream<T> FStream;
            
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
            
            protected abstract void Copy(T[] destination, int destinationIndex, int length, int stride);
            
            public void Dispose()
            {
                FStream = null;
            }
            
            public void Reset()
            {
                Position = 0;
            }
        }
        
        protected readonly int* FPLength;
        protected readonly Func<bool> FValidateFunc;
        
        public UnmanagedInStream(int* pLength, Func<bool> validateFunc)
        {
            FPLength = pLength;
            FValidateFunc = validateFunc;
            IsChanged = true;
        }
        
        public int Length
        {
            get
            {
                return *FPLength;
            }
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
        
        public abstract object Clone();
        
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
            private readonly double* FPData;
            
            public DoubleInStreamReader(DoubleInStream stream, double* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override double Read(int stride)
            {
                Debug.Assert(Position < Length);
                var result = FPData[Position];
                Position += stride;
                return result;
            }
            
            protected override void Copy(double[] destination, int destinationIndex, int length, int stride)
            {
                fixed (double* destinationPtr = destination)
                {
                    switch (stride)
                    {
                        case 1:
                            //Marshal.Copy(new IntPtr(FPData + Position), destination, destinationIndex, length);
                            Memory.Copy(destinationPtr + destinationIndex, FPData + Position, (uint)length * sizeof(double));
                            break;
                        default:
                            double* dst = destinationPtr + destinationIndex;
                            double* src = FPData + Position;

                            for (int i = 0; i < length; i++)
                            {
                                *(dst++) = *src;
                                src += stride;
                            }
                            break;
                    }
                }
            }
        }
        
        private readonly double** FPPData;
        
        public DoubleInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<double> GetReader()
        {
            return new DoubleInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new DoubleInStream(FPLength, FPPData, FValidateFunc);
        }
    }
    
    unsafe class FloatInStream : UnmanagedInStream<float>
    {
        class FloatInStreamReader : UnmanagedInStreamReader
        {
            private readonly double* FPData;
            
            public FloatInStreamReader(FloatInStream stream, double* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override float Read(int stride)
            {
                Debug.Assert(Position < Length);
                var result = (float) FPData[Position];
                Position += stride;
                return result;
            }
            
            protected override void Copy(float[] destination, int destinationIndex, int length, int stride)
            {
                fixed (float* destinationPtr = destination)
                {
                    float* dst = destinationPtr + destinationIndex;
                    double* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *(dst++) = (float) *src;
                        src += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPData;
        
        public FloatInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<float> GetReader()
        {
            return new FloatInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new FloatInStream(FPLength, FPPData, FValidateFunc);
        }
    }

    unsafe class IntInStream : UnmanagedInStream<int>
    {
        class IntInStreamReader : UnmanagedInStreamReader
        {
            private readonly double* FPData;
            
            public IntInStreamReader(IntInStream stream, double* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override int Read(int stride)
            {
                Debug.Assert(Position < Length);
                var result = (int) Math.Round(FPData[Position]);
                Position += stride;
                return result;
            }
            
            protected override void Copy(int[] destination, int destinationIndex, int length, int stride)
            {
                fixed (int* destinationPtr = destination)
                {
                    int* dst = destinationPtr + destinationIndex;
                    double* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *(dst++) = (int) Math.Round(*src);
                        src += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPData;
        
        public IntInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<int> GetReader()
        {
            return new IntInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new IntInStream(FPLength, FPPData, FValidateFunc);
        }
    }

    unsafe class UIntInStream : UnmanagedInStream<uint>
    {
        class UIntInStreamReader : UnmanagedInStreamReader
        {
            private readonly double* FPData;
            
            public UIntInStreamReader(UIntInStream stream, double* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override uint Read(int stride)
            {
                Debug.Assert(Position < Length);
                var result = (uint) Math.Round(FPData[Position]);
                Position += stride;
                return result;
            }
            
            protected override void Copy(uint[] destination, int destinationIndex, int length, int stride)
            {
                fixed (uint* destinationPtr = destination)
                {
                    uint* dst = destinationPtr + destinationIndex;
                    double* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *(dst++) = (uint) Math.Round(*src);
                        src += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPData;
        
        public UIntInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<uint> GetReader()
        {
            return new UIntInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new UIntInStream(FPLength, FPPData, FValidateFunc);
        }
    }
    
    
    unsafe class BoolInStream : UnmanagedInStream<bool>
    {
        class BoolInStreamReader : UnmanagedInStreamReader
        {
            private readonly double* FPData;
            
            public BoolInStreamReader(BoolInStream stream, double* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override bool Read(int stride)
            {
                Debug.Assert(Position < Length, string.Format("Position: {0}, Length: {0}", Position, Length));
                var result = FPData[Position] >= 0.5;
                Position += stride;
                return result;
            }
            
            protected override void Copy(bool[] destination, int destinationIndex, int length, int stride)
            {
                fixed (bool* destinationPtr = destination)
                {
                    bool* dst = destinationPtr + destinationIndex;
                    double* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *(dst++) = *src >= 0.5;
                        src += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPData;
        
        public BoolInStream(int* pLength, double** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<bool> GetReader()
        {
            return new BoolInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new BoolInStream(FPLength, FPPData, FValidateFunc);
        }
    }
    
    unsafe class ColorInStream : UnmanagedInStream<RGBAColor>
    {
        class ColorInStreamReader : UnmanagedInStreamReader
        {
            private readonly RGBAColor* FPData;
            
            public ColorInStreamReader(ColorInStream stream, RGBAColor* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override RGBAColor Read(int stride)
            {
                Debug.Assert(Position < Length);
                var result = FPData[Position];
                Position += stride;
                return result;
            }
            
            protected override void Copy(RGBAColor[] destination, int destinationIndex, int length, int stride)
            {
                fixed (RGBAColor* destinationPtr = destination)
                {
                    RGBAColor* dst = destinationPtr + destinationIndex;
                    RGBAColor* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *(dst++) = *src;
                        src += stride;
                    }
                }
            }
        }
        
        private readonly RGBAColor** FPPData;
        
        public ColorInStream(int* pLength, RGBAColor** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<RGBAColor> GetReader()
        {
            return new ColorInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new ColorInStream(FPLength, FPPData, FValidateFunc);
        }
    }
    
    unsafe class SlimDXColorInStream : UnmanagedInStream<Color4>
    {
        class SlimDXColorInStreamReader : UnmanagedInStreamReader
        {
            private readonly RGBAColor* FPData;
            
            public SlimDXColorInStreamReader(SlimDXColorInStream stream, RGBAColor* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override Color4 Read(int stride)
            {
                Debug.Assert(Position < Length);
                RGBAColor* src = FPData + Position;
                Position += stride;
                return new Color4((float) src->A, (float) src->R, (float) src->G, (float) src->B);
            }
            
            protected override void Copy(Color4[] destination, int destinationIndex, int length, int stride)
            {
                fixed (Color4* destinationPtr = destination)
                {
                    Color4* dst = destinationPtr + destinationIndex;
                    RGBAColor* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        dst->Red = (float) src->R;
                        dst->Green = (float) src->G;
                        dst->Blue = (float) src->B;
                        dst->Alpha = (float) src->A;
                        dst++;
                        src += stride;
                    }
                }
            }
        }
        
        private readonly RGBAColor** FPPData;
        
        public SlimDXColorInStream(int* pLength, RGBAColor** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<Color4> GetReader()
        {
            return new SlimDXColorInStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new SlimDXColorInStream(FPLength, FPPData, FValidateFunc);
        }
    }

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

    unsafe class Matrix4x4InStream : UnmanagedInStream<Matrix4x4>
    {
        class Matrix4x4InStreamReader : UnmanagedInStreamReader
        {
            private readonly Matrix* FPData;
            
            public Matrix4x4InStreamReader(Matrix4x4InStream stream, Matrix* pData)
                : base(stream)
            {
                FPData = pData;
            }
            
            public override Matrix4x4 Read(int stride)
            {
                Debug.Assert(Position < Length);
                var result = FPData[Position].ToMatrix4x4();
                Position += stride;
                return result;
            }
            
            protected override void Copy(Matrix4x4[] destination, int destinationIndex, int length, int stride)
            {
                fixed (Matrix4x4* destinationPtr = destination)
                {
                    Matrix4x4* dst = destinationPtr + destinationIndex;
                    Matrix* src = FPData + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *(dst++) = (*src).ToMatrix4x4();
                        src += stride;
                    }
                }
            }
        }
        
        private readonly Matrix** FPPData;
        
        public Matrix4x4InStream(int* pLength, Matrix** ppData, Func<bool> validateFunc)
            : base(pLength, validateFunc)
        {
            FPPData = ppData;
        }
        
        public override IStreamReader<Matrix4x4> GetReader()
        {
            return new Matrix4x4InStreamReader(this, *FPPData);
        }
        
        public override object Clone()
        {
            return new Matrix4x4InStream(FPLength, FPPData, FValidateFunc);
        }
    }
}
