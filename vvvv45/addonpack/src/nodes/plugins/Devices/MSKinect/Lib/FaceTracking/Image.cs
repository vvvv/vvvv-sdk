// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Image.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Image class represents an image used by the face tracked. Users can
    /// either attach this class to their image buffers or to fill this
    /// reference with image data (in which case it owns the data buffer memory)
    /// </summary>
    internal class Image : IDisposable
    {
        /// <summary>
        /// buffer management policy
        /// </summary>
        private BufferManagement bufferManagement = BufferManagement.None;

        /// <summary>
        /// have we been disposed or not
        /// </summary>
        private bool disposed;

        /// <summary>
        /// native interop interface
        /// </summary>
        private IFTImage faceTrackingImagePtr;

        /// <summary>
        /// Initializes a new instance of the Image class
        /// </summary>
        public Image()
        {
            this.faceTrackingImagePtr = NativeMethods.FTCreateImage();
            if (this.faceTrackingImagePtr == null)
            {
                throw new InvalidOperationException("Cannot create image instance");
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Image"/> class. 
        /// </summary>
        ~Image()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Buffer management policies
        /// </summary>
        private enum BufferManagement
        {
            /// <summary>
            /// Not set
            /// </summary>
            None, 

            /// <summary>
            /// We own the memory.
            /// </summary>
            LocalNativeImage, 

            /// <summary>
            /// The memory is handled externally
            /// </summary>
            External
        }

        /// <summary>
        /// Pointer to native memory to access raw data
        /// </summary>
        public IntPtr BufferPtr
        {
            get
            {
                this.CheckPtrAndThrow();
                return this.faceTrackingImagePtr.GetBuffer();
            }
        }

        /// <summary>
        /// Size of the image data
        /// </summary>
        public uint BufferSize
        {
            get
            {
                this.CheckPtrAndThrow();
                return this.faceTrackingImagePtr.GetBufferSize();
            }
        }

        /// <summary>
        /// Native image pointer
        /// </summary>
        internal IFTImage ImagePtr
        {
            get
            {
                return this.faceTrackingImagePtr;
            }
        }

        /// <summary>Get the bytes per pixel count for a FaceTrackingImageFormat</summary>
        /// <param name="format">The format.</param>
        /// <returns>the size</returns>
        /// <exception cref="ArgumentException">invalid format</exception>
        public static uint FormatToSize(FaceTrackingImageFormat format)
        {
            switch (format)
            {
                case FaceTrackingImageFormat.FTIMAGEFORMAT_INVALID:
                    return 0;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_GR8:
                    return 1;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_R8G8B8:
                    return 3;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_X8R8G8B8:
                    return 4;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_A8R8G8B8:
                    return 4;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_B8G8R8X8:
                    return 4;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_B8G8R8A8:
                    return 4;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT16_D16:
                    return 2;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT16_D13P3:
                    return 2;
                default:
                    throw new ArgumentException("Invalid image format specified");
            }
        }

        /// <summary>
        /// Allocates memory for the provided image width, height and format.
        /// The memory is owned by this instance and is released when the
        /// instance is disposed or when another Allocate() call happens.
        /// This method deallocates currently allocated memory if its internal
        /// buffers are not enough to fit new image data. If its internal
        /// buffers are big enough, no new allocation happens
        /// </summary>
        /// <param name="width">
        /// image width in pixels 
        /// </param>
        /// <param name="height">
        /// image height in pixels 
        /// </param>
        /// <param name="format">
        /// image format 
        /// </param>
        public void Allocate(uint width, uint height, FaceTrackingImageFormat format)
        {
            this.CheckPtrAndThrow();

            if (this.bufferManagement != BufferManagement.None)
            {
                throw new InvalidOperationException("Cannot Allocate again. Image already allocated buffer in native image.");
            }

            this.bufferManagement = BufferManagement.LocalNativeImage;
            this.faceTrackingImagePtr.Allocate(width, height, format);
        }

        /// <summary>
        /// Attaches this instance to external native memory pointed to by
        /// imageData, which is assumed to be sufficiently large to contain
        /// an image of the given size and format. The memory referenced by
        /// imageData will not be deallocated when this instance is released.
        /// The caller owns the image buffer in this case and is responsible
        /// for its lifetime management.
        /// </summary>
        /// <param name="width">
        /// image width in pixels 
        /// </param>
        /// <param name="height">
        /// image height in pixels 
        /// </param>
        /// <param name="imageDataPtr">
        /// external image buffer 
        /// </param>
        /// <param name="format">
        /// image format 
        /// </param>
        /// <param name="stride">
        /// stride of the image 
        /// </param>
        public void Attach(uint width, uint height, IntPtr imageDataPtr, FaceTrackingImageFormat format, uint stride)
        {
            this.CheckPtrAndThrow();

            if (this.bufferManagement != BufferManagement.None)
            {
                throw new InvalidOperationException("Cannot Attach again. Image already attached to external buffer.");
            }

            this.bufferManagement = BufferManagement.External;
            this.faceTrackingImagePtr.Attach(width, height, imageDataPtr, format, stride);
        }

        /// <summary>
        /// Copies the data from input source array to the native memory
        /// </summary>
        /// <typeparam name="T">
        /// Input data type - can be short(Depth) or byte(color) 
        /// </typeparam>
        /// <param name="srcData">
        /// Input source data. The size of the buffer should not be greater than
        /// native buffer size allocated 
        /// </param>
        public void CopyFrom<T>(T[] srcData) where T : struct
        {
            if (this.bufferManagement == BufferManagement.External)
            {
                throw new InvalidOperationException("Cannot copy data as buffer managed externally.");
            }

            if (this.bufferManagement == BufferManagement.LocalNativeImage)
            {
                IntPtr bufferPtr = this.BufferPtr;
                uint bufferSize = this.BufferSize;

                if (srcData.Length * Marshal.SizeOf(typeof(T)) > bufferSize)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.CurrentCulture, 
                            "Array size for src buffer ({0}) should be less than native image buffer ({1})", 
                            srcData.Length, 
                            bufferSize));
                }

                if (typeof(T).Equals(typeof(byte)))
                {
                    var srcDataByte = srcData as byte[];
                    Marshal.Copy(srcDataByte, 0, bufferPtr, srcDataByte.Length);
                }
                else if (typeof(T).Equals(typeof(short)))
                {
                    var srcDataShort = srcData as short[];
                    Marshal.Copy(srcDataShort, 0, bufferPtr, srcDataShort.Length);
                }
                else
                {
                    throw new InvalidOperationException("Invalid type of data specified. Only byte & short supported");
                }
            }
            else
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.CurrentCulture, "Unsupported buffer management {0} encountered", this.bufferManagement));
            }
        }

        /// <summary>
        /// Disposes the instance and cleans up native references
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (this.faceTrackingImagePtr != null)
                {
                    Marshal.FinalReleaseComObject(this.faceTrackingImagePtr);
                    this.faceTrackingImagePtr = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Makes sure we have a valid image pointer.
        /// </summary>
        /// <exception cref="InvalidOperationException">internal pointer was null</exception>
        private void CheckPtrAndThrow()
        {
            if (this.faceTrackingImagePtr == null)
            {
                throw new InvalidOperationException("Native image pointer in invalid state.");
            }
        }
    }
}