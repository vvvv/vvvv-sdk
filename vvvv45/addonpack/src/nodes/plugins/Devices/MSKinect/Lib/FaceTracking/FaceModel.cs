// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceModel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// FaceModel interface provides a way to read 3D model parameters or to transform loaded 3D model used by the 
    /// face tracker from the model space to the camera/world space
    /// </summary>
    internal class FaceModel : IDisposable
    {
        private readonly FaceTracker faceTracker;

        private bool disposed;

        private IFTModel faceTrackingModelPtr;

        internal FaceModel(FaceTracker faceTracker, IFTModel faceModelPtr)
        {
            if (faceTracker == null || faceModelPtr == null)
            {
                throw new InvalidOperationException("Cannot associate face model with null face tracker or native face model reference");
            }

            this.faceTrackingModelPtr = faceModelPtr;
            this.faceTracker = faceTracker;
        }

        private FaceModel()
        {
        }

        ~FaceModel()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Count of 3D vertices and 2d projected vertices
        /// </summary>
        public uint VertexCount
        {
            get
            {
                this.CheckPtrAndThrow();
                return this.faceTrackingModelPtr.GetVertexCount();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the 3D shape associated with the result
        /// </summary>
        /// <param name="faceTrackFrame">
        /// result object
        /// </param>
        /// <returns>
        /// the 3d shape
        /// </returns>
        public Vector3DF[] Get3DShape(FaceTrackFrame faceTrackFrame)
        {
            IntPtr shapeUnitCoeffPtr;
            uint shapeUnitCount = 0;
            IntPtr animUnitCoeffPtr;
            uint animUnitPointsCount;
            bool hasSuConverged;
            float scale;

            faceTrackFrame.ResultPtr.GetAUCoefficients(out animUnitCoeffPtr, out animUnitPointsCount);
            this.faceTracker.FaceTrackerPtr.GetShapeUnits(out scale, out shapeUnitCoeffPtr, ref shapeUnitCount, out hasSuConverged);

            return this.Get3DShape(
                shapeUnitCoeffPtr, 
                shapeUnitCount, 
                animUnitCoeffPtr, 
                animUnitPointsCount, 
                faceTrackFrame.Scale, 
                faceTrackFrame.Rotation, 
                faceTrackFrame.Translation);
        }

        public PointF[] GetProjected3DShape(float zoomFactor, Point viewOffset, FaceTrackFrame faceTrackFrame)
        {
            this.CheckPtrAndThrow();
            IntPtr shapeUnitCoeffPtr;
            uint shapeUnitCount = 0;
            IntPtr animUnitCoeffPtr;
            uint animUnitPointsCount;
            bool hasSuConverged;
            float scale;

            faceTrackFrame.ResultPtr.GetAUCoefficients(out animUnitCoeffPtr, out animUnitPointsCount);
            this.faceTracker.FaceTrackerPtr.GetShapeUnits(out scale, out shapeUnitCoeffPtr, ref shapeUnitCount, out hasSuConverged);

            return this.GetProjected3DShape(
                this.faceTracker.ColorCameraConfig, 
                zoomFactor, 
                viewOffset, 
                shapeUnitCoeffPtr, 
                shapeUnitCount, 
                animUnitCoeffPtr, 
                animUnitPointsCount, 
                faceTrackFrame.Scale, 
                faceTrackFrame.Rotation, 
                faceTrackFrame.Translation);
        }

        /// <summary>
        /// Get the Mesh triangles
        /// </summary>
        /// <returns>
        /// The face mesh triangles
        /// </returns>
        public FaceTriangle[] GetTriangles()
        {
            IntPtr trianglesPtr;
            uint trianglesCount;
            this.CheckPtrAndThrow();
            this.faceTrackingModelPtr.GetTriangles(out trianglesPtr, out trianglesCount);
            FaceTriangle[] triangles = null;
            if (trianglesCount > 0)
            {
                triangles = new FaceTriangle[trianglesCount];
                for (int i = 0; i < trianglesCount; i++)
                {
                    triangles[i] = new FaceTriangle();
                    IntPtr trianglesIthPtr;
                    if (IntPtr.Size == 8)
                    {
                        // 64bit
                        trianglesIthPtr = new IntPtr(trianglesPtr.ToInt64() + (i * Marshal.SizeOf(typeof(FaceTriangle))));
                    }
                    else
                    {
                        // 32bit
                        trianglesIthPtr = new IntPtr(trianglesPtr.ToInt32() + (i * Marshal.SizeOf(typeof(FaceTriangle))));
                    }

                    triangles[i] = (FaceTriangle)Marshal.PtrToStructure(trianglesIthPtr, typeof(FaceTriangle));
                }
            }

            return triangles;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (this.faceTrackingModelPtr != null)
                {
                    Marshal.FinalReleaseComObject(this.faceTrackingModelPtr);
                    this.faceTrackingModelPtr = null;
                }

                this.disposed = true;
            }
        }

        private void CheckPtrAndThrow()
        {
            if (this.faceTrackingModelPtr == null)
            {
                throw new InvalidOperationException("Native face model pointer in invalid state.");
            }
        }

        private Vector3DF[] Get3DShape(
            IntPtr shapeUnitCoeffPtr, 
            uint shapeUnitCoeffCount, 
            IntPtr animUnitCoeffPtr, 
            uint animUnitCoeffCount, 
            float scale, 
            Vector3DF rotation, 
            Vector3DF translation)
        {
            this.CheckPtrAndThrow();
            Vector3DF[] faceModel3DShape = null;
            uint vertexCount = this.VertexCount;
            IntPtr faceModel3DVerticesPtr = IntPtr.Zero;

            if (shapeUnitCoeffPtr == IntPtr.Zero || shapeUnitCoeffCount == 0)
            {
                throw new ArgumentException("Invalid shape unit co-efficients", "shapeUnitCoeffPtr");
            }

            if (animUnitCoeffPtr == IntPtr.Zero || animUnitCoeffCount == 0)
            {
                throw new ArgumentException("Invalid animation unit co-efficients", "animUnitCoeffPtr");
            }

            if (vertexCount > 0)
            {
                try
                {
                    faceModel3DVerticesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Vector3DF)) * (int)vertexCount);
                    this.faceTrackingModelPtr.Get3DShape(
                        shapeUnitCoeffPtr, 
                        shapeUnitCoeffCount, 
                        animUnitCoeffPtr, 
                        animUnitCoeffCount, 
                        scale, 
                        ref rotation, 
                        ref translation, 
                        faceModel3DVerticesPtr, 
                        vertexCount);
                    faceModel3DShape = new Vector3DF[vertexCount];
                    for (int i = 0; i < (int)vertexCount; i++)
                    {
                        IntPtr faceModel3DVerticesIthPtr;
                        if (IntPtr.Size == 8)
                        {
                            // 64bit
                            faceModel3DVerticesIthPtr =
                                new IntPtr(faceModel3DVerticesPtr.ToInt64() + (i * Marshal.SizeOf(typeof(Vector3DF))));
                        }
                        else
                        {
                            // 32bit
                            faceModel3DVerticesIthPtr =
                                new IntPtr(faceModel3DVerticesPtr.ToInt32() + (i * Marshal.SizeOf(typeof(Vector3DF))));
                        }

                        faceModel3DShape[i] = (Vector3DF)Marshal.PtrToStructure(faceModel3DVerticesIthPtr, typeof(Vector3DF));
                    }
                }
                finally
                {
                    if (faceModel3DVerticesPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(faceModel3DVerticesPtr);
                    }
                }
            }

            return faceModel3DShape;
        }

        private PointF[] GetProjected3DShape(
            CameraConfig videoCameraConfig, 
            float zoomFactor, 
            Point viewOffset, 
            IntPtr shapeUnitCoeffPtr, 
            uint shapeUnitCoeffCount, 
            IntPtr animUnitCoeffPtr, 
            uint animUnitCoeffCount, 
            float scale, 
            Vector3DF rotation, 
            Vector3DF translation)
        {
            this.CheckPtrAndThrow();
            PointF[] faceModelProjected3DShape = null;
            uint vertexCount = this.VertexCount;
            IntPtr faceModel3DVerticesPtr = IntPtr.Zero;

            if (shapeUnitCoeffPtr == IntPtr.Zero || shapeUnitCoeffCount == 0)
            {
                throw new ArgumentException("Invalid shape unit co-efficients", "shapeUnitCoeffPtr");
            }

            if (animUnitCoeffPtr == IntPtr.Zero || animUnitCoeffCount == 0)
            {
                throw new ArgumentException("Invalid animation unit co-efficients", "animUnitCoeffPtr");
            }

            if (vertexCount > 0)
            {
                try
                {
                    faceModel3DVerticesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Vector3DF)) * (int)vertexCount);
                    this.faceTrackingModelPtr.GetProjectedShape(
                        videoCameraConfig, 
                        zoomFactor, 
                        viewOffset, 
                        shapeUnitCoeffPtr, 
                        shapeUnitCoeffCount, 
                        animUnitCoeffPtr, 
                        animUnitCoeffCount, 
                        scale, 
                        ref rotation, 
                        ref translation, 
                        faceModel3DVerticesPtr, 
                        vertexCount);
                    faceModelProjected3DShape = new PointF[vertexCount];
                    for (int i = 0; i < (int)vertexCount; i++)
                    {
                        IntPtr faceModel3DVerticesIthPtr;
                        if (IntPtr.Size == 8)
                        {
                            // 64bit
                            faceModel3DVerticesIthPtr = new IntPtr(faceModel3DVerticesPtr.ToInt64() + (i * Marshal.SizeOf(typeof(PointF))));
                        }
                        else
                        {
                            // 32bit
                            faceModel3DVerticesIthPtr = new IntPtr(faceModel3DVerticesPtr.ToInt32() + (i * Marshal.SizeOf(typeof(PointF))));
                        }

                        faceModelProjected3DShape[i] = (PointF)Marshal.PtrToStructure(faceModel3DVerticesIthPtr, typeof(PointF));
                    }
                }
                finally
                {
                    if (faceModel3DVerticesPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(faceModel3DVerticesPtr);
                    }
                }
            }

            return faceModelProjected3DShape;
        }
    }
}