
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils;

namespace VVVV.Hosting.IO.Streams
{
    unsafe abstract class UnmanagedOutStream<T> : IOutStream<T>
    {
        internal abstract class UnmanagedOutWriter : IStreamWriter<T>
        {
            private UnmanagedOutStream<T> FStream;
            
            public UnmanagedOutWriter(UnmanagedOutStream<T> stream)
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
            
            public void Reset()
            {
                Position = 0;
            }
            
            public abstract void Write(T value, int stride);
            
            protected abstract void Copy(T[] source, int sourceIndex, int length, int stride);
            
            public int Write(T[] buffer, int index, int length, int stride)
            {
                int numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                
                switch (stride)
                {
                    case 0:
                        Write(buffer[index + numSlicesToWrite - 1], stride);
                        break;
                    default:
                        Debug.Assert(Position + numSlicesToWrite <= Length);
                        Copy(buffer, index, numSlicesToWrite, stride);
                        Position += numSlicesToWrite * stride;
                        break;
                }
                
                return numSlicesToWrite;
            }
            
            public void Dispose()
            {
                FStream = null;
            }
        }
        
        private readonly Action<int> FSetDstLengthAction;
        protected int FLength;
        
        public UnmanagedOutStream(Action<int> setDstLengthAction)
        {
            FSetDstLengthAction = setDstLengthAction;
            FSetDstLengthAction(FLength);
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
                }
            }
        }

        public void Flush(bool force = false)
        {
            // We write to the unmanaged array directly. Nothing to do here.
        }
        
        public object Clone()
        {
            throw new NotImplementedException();
        }
        
        public abstract IStreamWriter<T> GetWriter();
    }
    
    unsafe class DoubleOutStream : UnmanagedOutStream<double>
    {
        class DoubleOutWriter : UnmanagedOutWriter
        {
            private readonly double* FPDst;
            
            public DoubleOutWriter(DoubleOutStream stream, double* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(double value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = value;
                Position += stride;
            }
            
            protected override void Copy(double[] source, int sourceIndex, int length, int stride)
            {
                fixed (double* pSrc = source)
                {
                    switch (stride)
                    {
                        case 1:
                            //Marshal.Copy(source, sourceIndex, new IntPtr(FPDst + Position), length);
                            Memory.Copy(FPDst + Position, pSrc + sourceIndex, (uint)length * sizeof(double));
                            break;
                        default:
                            double* src = pSrc + sourceIndex;
                            double* dst = FPDst + Position;

                            for (int i = 0; i < length; i++)
                            {
                                *dst = *(src++);
                                dst += stride;
                            }
                            break;
                    }
                }
            }
        }
        
        private readonly double** FPPDst;
        
        public DoubleOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<double> GetWriter()
        {
            return new DoubleOutWriter(this, *FPPDst);
        }
    }
    
    unsafe class FloatOutStream : UnmanagedOutStream<float>
    {
        class FloatOutWriter : UnmanagedOutWriter
        {
            private readonly double* FPDst;
            
            public FloatOutWriter(FloatOutStream stream, double* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(float value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = (double) value;
                Position += stride;
            }
            
            protected override void Copy(float[] source, int sourceIndex, int length, int stride)
            {
                fixed (float* sourcePtr = source)
                {
                    float* src = sourcePtr + sourceIndex;
                    double* dst = FPDst + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *dst = (double) *(src++);
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPDst;
        
        public FloatOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<float> GetWriter()
        {
            return new FloatOutWriter(this, *FPPDst);
        }
    }

    unsafe class IntOutStream : UnmanagedOutStream<int>
    {
        class IntOutWriter : UnmanagedOutWriter
        {
            private readonly double* FPDst;
            
            public IntOutWriter(IntOutStream stream, double* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(int value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = (double) value;
                Position += stride;
            }
            
            protected override void Copy(int[] source, int sourceIndex, int length, int stride)
            {
                fixed (int* sourcePtr = source)
                {
                    int* src = sourcePtr + sourceIndex;
                    double* dst = FPDst + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *dst = (double) *(src++);
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPDst;
        
        public IntOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<int> GetWriter()
        {
            return new IntOutWriter(this, *FPPDst);
        }
    }
    
    unsafe class UIntOutStream : UnmanagedOutStream<uint>
    {
        class UIntOutWriter : UnmanagedOutWriter
        {
            private readonly double* FPDst;
            
            public UIntOutWriter(UIntOutStream stream, double* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(uint value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = (double) value;
                Position += stride;
            }
            
            protected override void Copy(uint[] source, int sourceIndex, int length, int stride)
            {
                fixed (uint* sourcePtr = source)
                {
                    uint* src = sourcePtr + sourceIndex;
                    double* dst = FPDst + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *dst = (double) *(src++);
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPDst;
        
        public UIntOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<uint> GetWriter()
        {
            return new UIntOutWriter(this, *FPPDst);
        }
    }

    unsafe class BoolOutStream : UnmanagedOutStream<bool>
    {
        class BoolOutWriter : UnmanagedOutWriter
        {
            private readonly double* FPDst;
            
            public BoolOutWriter(BoolOutStream stream, double* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(bool value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = value ? 1.0 : 0.0;
                Position += stride;
            }
            
            protected override void Copy(bool[] source, int sourceIndex, int length, int stride)
            {
                fixed (bool* sourcePtr = source)
                {
                    bool* src = sourcePtr + sourceIndex;
                    double* dst = FPDst + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *dst = *(src++) ? 1.0 : 0.0;
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly double** FPPDst;
        
        public BoolOutStream(double** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<bool> GetWriter()
        {
            return new BoolOutWriter(this, *FPPDst);
        }
    }
    
    unsafe class ColorOutStream : UnmanagedOutStream<RGBAColor>
    {
        class ColorOutWriter : UnmanagedOutWriter
        {
            private readonly RGBAColor* FPDst;
            
            public ColorOutWriter(ColorOutStream stream, RGBAColor* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(RGBAColor value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = value;
                Position += stride;
            }
            
            protected override void Copy(RGBAColor[] source, int sourceIndex, int length, int stride)
            {
                fixed (RGBAColor* sourcePtr = source)
                {
                    RGBAColor* src = sourcePtr + sourceIndex;
                    RGBAColor* dst = FPDst + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *dst = *(src++);
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly RGBAColor** FPPDst;
        
        public ColorOutStream(RGBAColor** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<RGBAColor> GetWriter()
        {
            return new ColorOutWriter(this, *FPPDst);
        }
    }
    
    unsafe class SlimDXColorOutStream : UnmanagedOutStream<Color4>
    {
        class SlimDXColorOutWriter : UnmanagedOutWriter
        {
            private readonly RGBAColor* FPDst;
            
            public SlimDXColorOutWriter(SlimDXColorOutStream stream, RGBAColor* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(Color4 value, int stride)
            {
                Debug.Assert(!Eos);
                RGBAColor* dst = FPDst + Position;
                dst->R = value.Red;
                dst->G = value.Green;
                dst->B = value.Blue;
                dst->A = value.Alpha;
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
                        dst->R = src->Red;
                        dst->G = src->Green;
                        dst->B = src->Blue;
                        dst->A = src->Alpha;
                        src++;
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly RGBAColor** FPPDst;
        
        public SlimDXColorOutStream(RGBAColor** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<Color4> GetWriter()
        {
            return new SlimDXColorOutWriter(this, *FPPDst);
        }
    }

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

    unsafe class Matrix4x4OutStream : UnmanagedOutStream<Matrix4x4>
    {
        class Matrix4x4OutWriter : UnmanagedOutWriter
        {
            private readonly Matrix* FPDst;
            
            public Matrix4x4OutWriter(Matrix4x4OutStream stream, Matrix* pDst)
                : base(stream)
            {
                FPDst = pDst;
            }
            
            public override void Write(Matrix4x4 value, int stride)
            {
                Debug.Assert(!Eos);
                FPDst[Position] = value.ToSlimDXMatrix();
                Position += stride;
            }
            
            protected override void Copy(Matrix4x4[] source, int sourceIndex, int length, int stride)
            {
                fixed (Matrix4x4* sourcePtr = source)
                {
                    Matrix4x4* src = sourcePtr + sourceIndex;
                    Matrix* dst = FPDst + Position;
                    
                    for (int i = 0; i < length; i++)
                    {
                        *dst = (*(src++)).ToSlimDXMatrix();
                        dst += stride;
                    }
                }
            }
        }
        
        private readonly Matrix** FPPDst;
        
        public Matrix4x4OutStream(Matrix** ppDst, Action<int> setDstLengthAction)
            : base(setDstLengthAction)
        {
            FPPDst = ppDst;
        }
        
        public override IStreamWriter<Matrix4x4> GetWriter()
        {
            return new Matrix4x4OutWriter(this, *FPPDst);
        }
    }
}
