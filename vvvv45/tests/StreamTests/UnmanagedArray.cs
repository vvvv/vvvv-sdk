using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using VVVV.Utils;

namespace VVVV.Tests.StreamTests
{
    public unsafe class UnmanagedArray : IDisposable
    {
        private IntPtr FDataPtr;
        private readonly IntPtr FPDataPtr;
        private readonly IntPtr FPLength;
        private int FLength;
        private const int PREFIX = 4;
		private const int SUFFIX = 4;
        
        public UnmanagedArray(int length)
        {
            FLength = length;
            FDataPtr = Marshal.AllocHGlobal(new IntPtr((PREFIX + length + SUFFIX) * sizeof(double)));
            
            FPDataPtr = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(FPDataPtr, FDataPtr + PREFIX * sizeof(double));
            FPLength = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(FPLength, new IntPtr(FLength));
            
            FillBoundaries();
            FillBody();
            
            // Do some self tests
            CheckBoundaries();
            CheckData();
        }
        
        public double[] ToArray()
        {
            var result = new double[FLength];
            double* pData = (double*) FDataPtr.ToPointer();
            pData += PREFIX;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = pData[i];
            }
            return result;
        }
        
        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= FLength) throw new IndexOutOfRangeException();
                
                double* pData = (double*) FDataPtr.ToPointer();
                pData += PREFIX;
                return pData[index];
            }
            set
            {
                if (index < 0 || index >= FLength) throw new IndexOutOfRangeException();
                
                double* pData = (double*) FDataPtr.ToPointer();
                pData += PREFIX;
                pData[index] = value;
            }
        }
        
        public void Resize(int length)
        {
            FLength = length;
            FDataPtr = Marshal.ReAllocHGlobal(FDataPtr, new IntPtr((PREFIX + length + SUFFIX) * sizeof(double)));
            Marshal.WriteIntPtr(FPDataPtr, FDataPtr + PREFIX * sizeof(double));
            Marshal.WriteIntPtr(FPLength, new IntPtr(FLength));
            
            FillBoundaries();
            FillBody();
            CheckBoundaries();
        }
        
        public void CheckBoundaries()
        {
            double* pData = (double*) FDataPtr.ToPointer();
            for (int i = 0; i < PREFIX; i++)
            {
                Assert.AreEqual(-1000, pData[i]);
            }
            for (int i = PREFIX + FLength; i < PREFIX + FLength + SUFFIX; i++)
            {
                Assert.AreEqual(1000, pData[i]);
            }
        }
        
        private void FillBody()
        {
            double* pData = (double*) FDataPtr.ToPointer();
            pData += PREFIX;
            for (int i = 0; i < FLength; i++)
            {
                pData[i] = i;
            }
        }
        
        private void FillBoundaries()
        {
            double* pData = (double*) FDataPtr.ToPointer();
            for (int i = 0; i < PREFIX; i++)
            {
                pData[i] = -1000;
            }
            for (int i = PREFIX + FLength; i < PREFIX + FLength + SUFFIX; i++)
            {
                pData[i] = 1000;
            }
        }
        
        private void CheckData()
        {
            var arr = ToArray();
            double* pData = (double*) FDataPtr.ToPointer();
            pData += PREFIX;
            for (int i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(arr[i], pData[i]);
            }
        }
        
        public int Length
        {
            get
            {
                return FLength;
            }
        }
        
        public double** PPData
        {
            get
            {
                return (double**) FPDataPtr.ToPointer();
            }
        }
        
        public int* PLength
        {
            get
            {
                return (int*) FPLength.ToPointer();
            }
        }
    	
		public void Dispose()
		{
		    Marshal.FreeHGlobal(FDataPtr);
		    Marshal.FreeHGlobal(FPDataPtr);
		    Marshal.FreeHGlobal(FPLength);
		}
    }
}
