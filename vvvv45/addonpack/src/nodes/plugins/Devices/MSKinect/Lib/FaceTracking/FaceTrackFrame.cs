// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceTrackFrame.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class represent face tracking results for a frame
    /// </summary>
    public sealed class FaceTrackFrame : IDisposable, ICloneable
    {
        private bool disposed;

        private IFTResult faceTrackingResultPtr;

        private WeakReference parentFaceTracker;

        internal FaceTrackFrame(IFTResult faceTrackResultPtr, FaceTracker parentTracker)
        {
            if (faceTrackResultPtr == null)
            {
                throw new InvalidOperationException("Cannot associate with a null native frame pointer");
            }

            this.faceTrackingResultPtr = faceTrackResultPtr;
            this.parentFaceTracker = new WeakReference(parentTracker, false);
        }

        private FaceTrackFrame()
        {
        }

        ~FaceTrackFrame()
        {
            this.InternalDispose();
        }

        /// <summary>
        /// Face rectangle in video frame coordinates
        /// </summary>
        public Rect FaceRect
        {
            get
            {
                this.CheckPtrAndThrow();
                Rect faceRect;
                this.faceTrackingResultPtr.GetFaceRect(out faceRect);
                return faceRect;
            }
        }

        /// <summary>
        /// Rotation around X, Y, Z axes
        /// </summary>
        public Vector3DF Rotation
        {
            get
            {
                this.CheckPtrAndThrow();
                float scale;
                Vector3DF rotationXyz;
                Vector3DF translationXyz;
                this.faceTrackingResultPtr.Get3DPose(out scale, out rotationXyz, out translationXyz);
                return rotationXyz;
            }
        }

        /// <summary>
        /// Returns a flag if the tracking was successful or not on last tracking call
        /// </summary>
        public bool TrackSuccessful
        {
            get
            {
                return this.Status == ErrorCode.Success;
            }
        }

        /// <summary>
        /// Translation in X, Y, Z axes
        /// </summary>
        public Vector3DF Translation
        {
            get
            {
                this.CheckPtrAndThrow();
                float scale;
                Vector3DF rotationXYZ;
                Vector3DF translationXYZ;
                this.faceTrackingResultPtr.Get3DPose(out scale, out rotationXYZ, out translationXYZ);
                return translationXYZ;
            }
        }

        internal IFTResult ResultPtr
        {
            get
            {
                return this.faceTrackingResultPtr;
            }
        }

        /// <summary>
        /// Returns face scale where 1.0 scale means that it is equal in size 
        /// to the loaded 3D model (in the model space)
        /// </summary>
        internal float Scale
        {
            get
            {
                this.CheckPtrAndThrow();
                float scale;
                Vector3DF rotationXyz;
                Vector3DF translationXyz;
                this.faceTrackingResultPtr.Get3DPose(out scale, out rotationXyz, out translationXyz);
                return scale;
            }
        }

        /// <summary>
        /// Error code associated with the frame if the tracking failed
        /// </summary>
        internal ErrorCode Status
        {
            get
            {
                this.CheckPtrAndThrow();
                return (ErrorCode)this.faceTrackingResultPtr.GetStatus();
            }
        }

        /// <summary>
        /// Creates a deep copy clone. Copies all data from this instance to another instance of FaceTrackFrame. 
        /// Both instances must be created by the same face tracker instance.
        /// </summary>
        /// <returns>
        /// The clone.
        /// </returns>
        public object Clone()
        {
            this.CheckPtrAndThrow();
            var faceTracker = this.parentFaceTracker.Target as FaceTracker;
            if (faceTracker == null)
            {
                throw new ObjectDisposedException("FaceTracker", "Underlying face object has been garbage collected. Cannot clone.");
            }

            int hr;
            FaceTrackFrame faceTrackFrame = faceTracker.CreateResult(out hr);
            if (faceTrackFrame == null || hr != 0)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, "Failed to create face tracking frame. Error code from native=0x{0:X}", hr));
            }

            hr = this.faceTrackingResultPtr.CopyTo(faceTrackFrame.ResultPtr);
            if (hr != 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture, "Failed to clone the source face tracking frame. Error code from native=0x{0:X}", hr));
            }

            return faceTrackFrame;
        }

        /// <summary>
        /// Disposes this instance and clears the native resources allocated
        /// </summary>
        public void Dispose()
        {
            this.InternalDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the 3D face model vertices transformed by the passed Shape Units, Animation Units, scale stretch, rotation and translation
        /// </summary>
        /// <returns>
        /// Returns 3D shape
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Analysis doesn't see these as arrays.  If this returned an actual array, we wouldn't see this warning.")]
        public EnumIndexableCollection<FeaturePoint, Vector3DF> Get3DShape()
        {
            var faceTracker = this.parentFaceTracker.Target as FaceTracker;
            if (faceTracker == null)
            {
                throw new ObjectDisposedException("FaceTracker", "Underlying face object has been garbage collected. Cannot copy.");
            }

            return new EnumIndexableCollection<FeaturePoint, Vector3DF>(faceTracker.FaceModel.Get3DShape(this));
        }

        /// <summary>
        /// Returns Animation Units (AUs) coefficients. These coefficients represent deformations 
        /// of the 3D mask caused by the moving parts of the face (mouth, eyebrows, etc). Use the 
        /// AnimationUnit enum to index these co-efficients
        /// </summary>
        /// <returns>
        /// The get animation unit coefficients.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Analysis doesn't see these as arrays.  If this returned an actual array, we wouldn't see this warning.")]
        public EnumIndexableCollection<AnimationUnit, float> GetAnimationUnitCoefficients()
        {
            this.CheckPtrAndThrow();
            IntPtr animUnitCoeffPtr;
            uint pointsCount;
            this.faceTrackingResultPtr.GetAUCoefficients(out animUnitCoeffPtr, out pointsCount);
            float[] animUnitCoeff = null;
            if (pointsCount > 0)
            {
                animUnitCoeff = new float[pointsCount];
                Marshal.Copy(animUnitCoeffPtr, animUnitCoeff, 0, animUnitCoeff.Length);
            }

            return new EnumIndexableCollection<AnimationUnit, float>(animUnitCoeff);
        }

        /// <summary>
        /// Returns the 3D face model vertices transformed by the passed Shape Units, Animation Units, scale stretch, rotation and translation and
        /// projected to the video frame
        /// </summary>
        /// <returns>
        /// Returns projected 3D shape
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Analysis doesn't see these as arrays.  If this returned an actual array, we wouldn't see this warning.")]
        public EnumIndexableCollection<FeaturePoint, PointF> GetProjected3DShape()
        {
            var faceTracker = this.parentFaceTracker.Target as FaceTracker;
            if (faceTracker == null)
            {
                throw new ObjectDisposedException("FaceTracker", "Underlying face object has been garbage collected. Cannot copy.");
            }

            return
                new EnumIndexableCollection<FeaturePoint, PointF>(
                    faceTracker.FaceModel.GetProjected3DShape(FaceTracker.DefaultZoomFactor, Point.Empty, this));
        }

        /// <summary>
        /// Get the Mesh triangles for the 3D Face Model
        /// </summary>
        /// <returns>
        /// Returns the face triangles
        /// </returns>
        public FaceTriangle[] GetTriangles()
        {
            var faceTracker = this.parentFaceTracker.Target as FaceTracker;
            if (faceTracker == null)
            {
                throw new ObjectDisposedException("FaceTracker", "Underlying face object has been garbage collected. Cannot copy.");
            }

            return faceTracker.FaceModel.GetTriangles();
        }

        private void CheckPtrAndThrow()
        {
            if (this.faceTrackingResultPtr == null)
            {
                throw new InvalidOperationException("Native frame pointer in invalid state.");
            }
        }

        private void InternalDispose()
        {
            if (!this.disposed)
            {
                if (this.faceTrackingResultPtr != null)
                {
                    Marshal.FinalReleaseComObject(this.faceTrackingResultPtr);
                    this.faceTrackingResultPtr = null;
                }

                // do not dispose parentFaceTracker
                this.parentFaceTracker = null;

                this.disposed = true;
            }
        }
    }
}