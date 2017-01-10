// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceTracker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Main class that instantiates the face tracking engine and tracks the faces of a single person
    /// retrieving various metrics like animation units, 3D points and triangles on the face.
    /// </summary>
    public class FaceTracker : IDisposable
    {
        /// <summary>
        /// A constant zoom factor is used for now, since Windows Kinect does not support
        /// different zoom levels.
        /// </summary>
        internal const float DefaultZoomFactor = 1.0f;

        private const string FaceTrackTraceSwitchName = "KinectForWindowsFaceTracking";

        private const string TraceCategory = "FTR";

        private const string TraceLogFileName = "TraceLogFile";

        private readonly Stopwatch copyStopwatch = new Stopwatch();

        private readonly ColorImageFormat initializationColorImageFormat;

        private readonly DepthImageFormat initializationDepthImageFormat;

        private readonly OperationMode operationMode = OperationMode.Kinect;

        private readonly KinectSensor sensor;

        private readonly Stopwatch startOrContinueTrackingStopwatch = new Stopwatch();

        private readonly Stopwatch trackStopwatch = new Stopwatch();

        private Image colorFaceTrackingImage;

        private CameraConfig depthCameraConfig;

        private Image depthFaceTrackingImage;

        private bool disposed;

        private FaceModel faceModel;

        private IFTFaceTracker faceTrackerInteropPtr;

        private FaceTrackFrame frame;

        private long lastSuccessTrackElapsedMs;

        private FaceTrackingRegisterDepthToColor registerDepthToColorDelegate;

        private long totalSuccessTrackMs;

        private int totalSuccessTracks;

        private int totalTracks;

        // trace settings
        private TraceLevel traceLevel = TraceLevel.Off;

        private bool trackSucceeded;

        private CameraConfig videoCameraConfig;

        static FaceTracker()
        {
            try
            {
                /*NameValueCollection appSettings = ConfigurationManager.AppSettings;
                string logFileName = appSettings[TraceLogFileName];
                if (!string.IsNullOrEmpty(logFileName))
                {
                    foreach (TraceListener tl in Trace.Listeners)
                    {
                        var defaultListener = tl as DefaultTraceListener;
                        if (defaultListener != null)
                        {
                            defaultListener.LogFileName = logFileName;
                            break;
                        }
                    }

                    DateTime cur = DateTime.Now;
                    Trace.WriteLine(
                        string.Format(
                            CultureInfo.InvariantCulture, "---------------------------------------------------------------------------"));
                    Trace.WriteLine(
                        string.Format(
                            CultureInfo.InvariantCulture, 
                            "Starting Trace. Time={0} {1}, Machine={2}, Processor={3}, OS={4}", 
                            cur.ToShortDateString(), 
                            cur.ToLongTimeString(), 
                            Environment.MachineName, 
                            Environment.Is64BitProcess ? "64bit" : "32bit", 
                            Environment.OSVersion));
                    Trace.WriteLine(
                        string.Format(
                            CultureInfo.InvariantCulture, "---------------------------------------------------------------------------"));
                }*/
            }
            catch (Exception ex)
            {
                Trace.WriteLine(
                    string.Format(CultureInfo.InvariantCulture, "Failed to set logfile for logging trace output. Exception={0}", ex));

                throw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the FaceTracker class from a reference of the Kinect device.
        /// <param name="sensor">Reference to kinect sensor instance</param>
        /// </summary>
        public FaceTracker(KinectSensor sensor)
        {
            if (sensor == null)
            {
                throw new ArgumentNullException("sensor");
            }

            if (!sensor.ColorStream.IsEnabled)
            {
                throw new InvalidOperationException("Color stream is not enabled yet.");
            }

            if (!sensor.DepthStream.IsEnabled)
            {
                throw new InvalidOperationException("Depth stream is not enabled yet.");
            }

            this.operationMode = OperationMode.Kinect;
            this.sensor = sensor;
            this.initializationColorImageFormat = sensor.ColorStream.Format;
            this.initializationDepthImageFormat = sensor.DepthStream.Format;

            var newColorCameraConfig = new CameraConfig(
                (uint)sensor.ColorStream.FrameWidth, 
                (uint)sensor.ColorStream.FrameHeight, 
                sensor.ColorStream.NominalFocalLengthInPixels, 
                FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_B8G8R8X8);
            var newDepthCameraConfig = new CameraConfig(
                (uint)sensor.DepthStream.FrameWidth, 
                (uint)sensor.DepthStream.FrameHeight, 
                sensor.DepthStream.NominalFocalLengthInPixels, 
                FaceTrackingImageFormat.FTIMAGEFORMAT_UINT16_D13P3);
            this.Initialize(newColorCameraConfig, newDepthCameraConfig, IntPtr.Zero, IntPtr.Zero, this.DepthToColorCallback);
        }

        /// <summary>
        /// Finalizes an instance of the FaceTracker class
        /// </summary>
        ~FaceTracker()
        {
            this.Dispose(false);
        }

        internal CameraConfig ColorCameraConfig
        {
            get
            {
                return this.videoCameraConfig;
            }
        }

        /// <summary>
        /// Returns reference to FaceModel class for the loaded face model.
        /// </summary>
        internal FaceModel FaceModel
        {
            get
            {
                this.CheckPtrAndThrow();
                if (this.faceModel == null)
                {
                    IFTModel faceTrackModelPtr;
                    this.faceTrackerInteropPtr.GetFaceModel(out faceTrackModelPtr);
                    this.faceModel = new FaceModel(this, faceTrackModelPtr);
                }

                return this.faceModel;
            }
        }

        internal IFTFaceTracker FaceTrackerPtr
        {
            get
            {
                return this.faceTrackerInteropPtr;
            }
        }

        /// <summary>
        /// Stopwatch associated with the tracker
        /// </summary>
        internal Stopwatch Stopwatch
        {
            get
            {
                return this.trackStopwatch;
            }
        }

        /// <summary>
        /// Total number of tracking operations handled by the tracker
        /// </summary>
        internal int TotalTracks
        {
            get
            {
                return this.totalTracks;
            }
        }

        /// <summary>
        /// Disposes of the face tracking engine
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Resets IFTFaceTracker instance to the clean state (like it is in right after the call 
        /// to Initialize() method)
        /// </summary>
        public void ResetTracking()
        {
            this.CheckPtrAndThrow();
            this.trackSucceeded = false;
            this.faceTrackerInteropPtr.Reset();
        }

        /// <summary>
        /// Starts face tracking from Kinect input data. Track() detects a face
        /// based on the passed parameters, then identifies characteristic
        /// points and begins tracking. The first call to this API is more
        /// expensive, but if the tracking succeeds then subsequent calls use
        /// the tracking information generated from first call and is faster,
        /// until a tracking failure happens. 
        /// </summary>
        /// <param name="colorImageFormat">format of the colorImage array</param>
        /// <param name="colorImage">Input color image frame retrieved from Kinect sensor</param>
        /// <param name="depthImageFormat">format of the depthImage array</param>
        /// <param name="depthImage">Input depth image frame retrieved from Kinect sensor</param>
        /// <param name="skeletonOfInterest">Input skeleton to track. Head and shoulder joints in the skeleton are used to calculate the head vector</param>
        /// <returns>Returns computed face tracking results for this image frame</returns>
        public FaceTrackFrame Track(
            ColorImageFormat colorImageFormat, 
            byte[] colorImage, 
            DepthImageFormat depthImageFormat, 
            short[] depthImage, 
            Skeleton skeletonOfInterest)
        {
            return this.Track(colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest, Rect.Empty);
        }

        /// <summary>
        /// Starts face tracking from Kinect input data. Track() detects a face
        /// based on the passed parameters, then identifies characteristic
        /// points and begins tracking. The first call to this API is more
        /// expensive, but if the tracking succeeds then subsequent calls use
        /// the tracking information generated from first call and is faster,
        /// until a tracking failure happens. 
        /// </summary>
        /// <param name="colorImageFormat">
        /// format of the colorImage array
        /// </param>
        /// <param name="colorImage">
        /// Input color image frame retrieved from Kinect sensor
        /// </param>
        /// <param name="depthImageFormat">
        /// format of the depthImage array
        /// </param>
        /// <param name="depthImage">
        /// Input depth image frame retrieved from Kinect sensor
        /// </param>
        /// <param name="regionOfInterest">
        /// Region of interest in the passed video frame where the face tracker should search for a face to initiate tracking. 
        /// Passing Rectangle.Empty (default) causes the entire frame to be searched.
        /// </param>
        /// <returns>
        /// Returns computed face tracking results for this image frame
        /// </returns>
        public FaceTrackFrame Track(
            ColorImageFormat colorImageFormat, 
            byte[] colorImage, 
            DepthImageFormat depthImageFormat, 
            short[] depthImage, 
            Rect regionOfInterest)
        {
            return this.Track(colorImageFormat, colorImage, depthImageFormat, depthImage, null, regionOfInterest);
        }

        /// <summary>
        /// Starts face tracking from Kinect input data. Track() detects a face
        /// based on the passed parameters, then identifies characteristic
        /// points and begins tracking. The first call to this API is more
        /// expensive, but if the tracking succeeds then subsequent calls use
        /// the tracking information generated from first call and is faster,
        /// until a tracking failure happens. 
        /// </summary>
        /// <param name="colorImageFormat">
        /// format of the colorImage array
        /// </param>
        /// <param name="colorImage">
        /// Input color image frame retrieved from Kinect sensor
        /// </param>
        /// <param name="depthImageFormat">
        /// format of the depthImage array
        /// </param>
        /// <param name="depthImage">
        /// Input depth image frame retrieved from Kinect sensor
        /// </param>
        /// <returns>
        /// Returns computed face tracking results for this image frame
        /// </returns>
        public FaceTrackFrame Track(
            ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage)
        {
            return this.Track(colorImageFormat, colorImage, depthImageFormat, depthImage, null, Rect.Empty);
        }

        /// <summary>
        /// Creates a frame object instance. Can be used for caching of the face tracking
        /// frame. FaceTrackFrame should be disposed after use.
        /// </summary>
        /// <returns>
        /// newly created frame object
        /// </returns>
        internal FaceTrackFrame CreateResult(out int hr)
        {
            IFTResult faceTrackResultPtr;
            FaceTrackFrame faceTrackFrame = null;

            this.CheckPtrAndThrow();
            hr = this.faceTrackerInteropPtr.CreateFTResult(out faceTrackResultPtr);
            if (faceTrackResultPtr != null)
            {
                faceTrackFrame = new FaceTrackFrame(faceTrackResultPtr, this);
            }

            return faceTrackFrame;
        }

        /// <summary>
        /// Allows calling dispose explicitly or from the finalizer
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                string traceStr = string.Format(
                    CultureInfo.InvariantCulture, 
                    "FaceTracker::Dispose() - TotalTracks={0}, TotalSuccessTracks={1}, TimePerTrack={2:F3}ms, TimePerSuccessTrack={3:F3}ms, TimePerDataCopy={4:F3}ms, TimePerStartOrContinueTracking={5:F3}ms", 
                    this.totalTracks, 
                    this.totalSuccessTracks, 
                    this.totalTracks > 0 ? (double)this.trackStopwatch.ElapsedMilliseconds / this.totalTracks : 0, 
                    this.totalSuccessTracks > 0 ? (double)this.totalSuccessTrackMs / this.totalSuccessTracks : 0, 
                    this.totalTracks > 0 ? (double)this.copyStopwatch.ElapsedMilliseconds / this.totalTracks : 0, 
                    this.totalTracks > 0 ? (double)this.startOrContinueTrackingStopwatch.ElapsedMilliseconds / this.totalTracks : 0);
#if DEBUG
                Debug.WriteLine(traceStr);
#else
                Trace.WriteLineIf(traceLevel >= TraceLevel.Info, traceStr);
#endif
                if (this.faceModel != null)
                {
                    this.faceModel.Dispose();
                    this.faceModel = null;
                }

                if (this.frame != null)
                {
                    this.frame.Dispose();
                    this.frame = null;
                }

                if (this.colorFaceTrackingImage != null)
                {
                    this.colorFaceTrackingImage.Dispose();
                    this.colorFaceTrackingImage = null;
                }

                if (this.depthFaceTrackingImage != null)
                {
                    this.depthFaceTrackingImage.Dispose();
                    this.depthFaceTrackingImage = null;
                }

                if (this.faceTrackerInteropPtr != null)
                {
                    Marshal.FinalReleaseComObject(this.faceTrackerInteropPtr);
                    this.faceTrackerInteropPtr = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Helper API to retrieve head points structure from a given skeleton instance
        /// </summary>
        /// <param name="skeletonOfInterest">
        /// skeleton from which head points are to be extracted
        /// </param>
        /// <returns>
        /// HeadPoints that can be passed to Start/Continue tracking APIs
        /// </returns>
        private static Vector3DF[] GetHeadPointsFromSkeleton(Skeleton skeletonOfInterest)
        {
            Vector3DF[] headPoints = null;

            if (skeletonOfInterest != null && skeletonOfInterest.TrackingState == SkeletonTrackingState.Tracked)
            {
                headPoints = new Vector3DF[2];

                SkeletonPoint sp0 = skeletonOfInterest.Joints[JointType.ShoulderCenter].Position;
                headPoints[0] = new Vector3DF(sp0.X, sp0.Y, sp0.Z);

                SkeletonPoint sp1 = skeletonOfInterest.Joints[JointType.Head].Position;
                headPoints[1] = new Vector3DF(sp1.X, sp1.Y, sp1.Z);
            }

            return headPoints;
        }

        private void CheckPtrAndThrow()
        {
            if (this.faceTrackerInteropPtr == null)
            {
                throw new InvalidOperationException("Native face tracker pointer in invalid state.");
            }
        }

        /// <summary>
        /// Callback to help with mapping depth pixel to color pixel data. Uses Kinect sensor's MapDepthToColorImagePoint to 
        /// do the conversion
        /// </summary>
        /// <returns>
        /// The depth to color callback.
        /// </returns>
        private int DepthToColorCallback(
            uint depthFrameWidth, 
            uint depthFrameHeight, 
            uint colorFrameWidth, 
            uint colorFrameHeight, 
            float zoomFactor, 
            Point viewOffset, 
            int depthX, 
            int depthY, 
            ushort depthZ, 
            out int colorX, 
            out int colorY)
        {
            int retCode = 0;
            colorX = 0;
            colorY = 0;

            if (this.sensor != null)
            {
                var colorPoint = new ColorImagePoint();
                try
                {
                    DepthImagePoint depthImagePoint = new DepthImagePoint()
                    {
                        X = depthX,
                        Y = depthY,
                        Depth = depthZ,
                    };

                    colorPoint = this.sensor.CoordinateMapper.MapDepthPointToColorPoint(
                        this.sensor.DepthStream.Format,
                        depthImagePoint,
                        this.sensor.ColorStream.Format);
                }
                catch (InvalidOperationException e)
                {
                    string traceStr = string.Format(
                        CultureInfo.CurrentCulture, 
                        "Exception on MapDepthToColorImagePoint while translating depth point({0},{1},{2}). Exception={3}", 
                        depthX, 
                        depthY, 
                        depthZ, 
                        e.Message);
                    Trace.WriteLineIf(this.traceLevel >= TraceLevel.Error, traceStr, TraceCategory);

                    retCode = -1;
                }

                colorX = colorPoint.X;
                colorY = colorPoint.Y;
            }
            else
            {
                retCode = -1;
            }

            return retCode;
        }

        /// <summary>
        /// Helper method that does the core instantiation & initialization of face tracking engine
        /// </summary>
        /// <param name="newColorCameraConfig">Color camera configuration</param>
        /// <param name="newDepthCameraConfig">Depth camera configuration</param>
        /// <param name="colorImagePtr">Allows face tracking engine to read color image from native memory pointer. 
        /// If set to IntPtr.Zero, image data needs to be provided for tracking to this instance. </param>
        /// <param name="depthImagePtr">Allows face tracking engine to read depth image from native memory pointer. 
        /// If set to IntPtr.Zero, image data needs to be provided for tracking to this instance.</param>
        /// <param name="newRegisterDepthToColorDelegate">Callback which maps of depth to color pixels</param>
        private void Initialize(
            CameraConfig newColorCameraConfig, 
            CameraConfig newDepthCameraConfig, 
            IntPtr colorImagePtr, 
            IntPtr depthImagePtr, 
            FaceTrackingRegisterDepthToColor newRegisterDepthToColorDelegate)
        {
            if (newColorCameraConfig == null)
            {
                throw new ArgumentNullException("newColorCameraConfig");
            }

            if (newDepthCameraConfig == null)
            {
                throw new ArgumentNullException("newDepthCameraConfig");
            }

            if (newRegisterDepthToColorDelegate == null)
            {
                throw new ArgumentNullException("newRegisterDepthToColorDelegate");
            }

            // initialize perf counters
            this.totalTracks = 0;
            this.trackStopwatch.Reset();

            // get configuration & trace settings
            this.traceLevel = new TraceSwitch(FaceTrackTraceSwitchName, FaceTrackTraceSwitchName).Level;

            this.videoCameraConfig = newColorCameraConfig;
            this.depthCameraConfig = newDepthCameraConfig;
            this.registerDepthToColorDelegate = newRegisterDepthToColorDelegate;

            this.faceTrackerInteropPtr = NativeMethods.FTCreateFaceTracker(IntPtr.Zero);
            if (this.faceTrackerInteropPtr == null)
            {
                throw new InsufficientMemoryException("Cannot create face tracker.");
            }

            IntPtr funcPtr = Marshal.GetFunctionPointerForDelegate(this.registerDepthToColorDelegate);
            if (funcPtr == IntPtr.Zero)
            {
                throw new InsufficientMemoryException("Cannot setup callback for retrieving color to depth pixel mapping");
            }

            int hr = this.faceTrackerInteropPtr.Initialize(this.videoCameraConfig, this.depthCameraConfig, funcPtr, null);
            if (hr != 0)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, "Failed to initialize face tracker - Error code from native=0x{0:X}", hr));
            }

            this.frame = this.CreateResult(out hr);
            if (this.frame == null || hr != 0)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, "Failed to create face tracking result. Error code from native=0x{0:X}", hr));
            }

            this.colorFaceTrackingImage = new Image();
            if (colorImagePtr == IntPtr.Zero)
            {
                this.colorFaceTrackingImage.Allocate(
                    this.videoCameraConfig.Width, this.videoCameraConfig.Height, this.videoCameraConfig.ImageFormat);
            }
            else
            {
                this.colorFaceTrackingImage.Attach(
                    this.videoCameraConfig.Width, 
                    this.videoCameraConfig.Height, 
                    colorImagePtr, 
                    this.videoCameraConfig.ImageFormat, 
                    this.videoCameraConfig.Stride);
            }

            this.depthFaceTrackingImage = new Image();
            if (depthImagePtr == IntPtr.Zero)
            {
                this.depthFaceTrackingImage.Allocate(
                    this.depthCameraConfig.Width, this.depthCameraConfig.Height, this.depthCameraConfig.ImageFormat);
            }
            else
            {
                this.depthFaceTrackingImage.Attach(
                    this.depthCameraConfig.Width, 
                    this.depthCameraConfig.Height, 
                    depthImagePtr, 
                    this.depthCameraConfig.ImageFormat, 
                    this.depthCameraConfig.Stride);
            }
        }

        /// <summary>
        /// Starts face tracking from Kinect input data. Track() detects a face
        /// based on the passed parameters, then identifies characteristic
        /// points and begins tracking. The first call to this API is more
        /// expensive, but if the tracking succeeds then subsequent calls use
        /// the tracking information generated from first call and is faster,
        /// until a tracking failure happens. 
        /// </summary>
        /// <param name="colorImageFormat">format of the colorImage array</param>
        /// <param name="colorImage">Input color image frame retrieved from Kinect sensor</param>
        /// <param name="depthImageFormat">format of the depthImage array</param>
        /// <param name="depthImage">Input depth image frame retrieved from Kinect sensor</param>
        /// <param name="skeletonOfInterest">Input skeleton to track. Head & shoulder joints in the skeleton are used to calculate the head vector</param>
        /// <param name="regionOfInterest">Region of interest in the passed video frame where the face tracker should search for a face to initiate tracking. 
        /// Passing Rectangle.Empty (default) causes the entire frame to be searched.</param>
        /// <returns>Returns computed face tracking results for this image frame</returns>
        private FaceTrackFrame Track(
            ColorImageFormat colorImageFormat, 
            byte[] colorImage, 
            DepthImageFormat depthImageFormat, 
            short[] depthImage, 
            Skeleton skeletonOfInterest, 
            Rect regionOfInterest)
        {
            this.totalTracks++;
            this.trackStopwatch.Start();

            if (this.operationMode != OperationMode.Kinect)
            {
                throw new InvalidOperationException(
                    "Cannot use Track with Kinect input types when face tracker is initialized for tracking videos/images");
            }

            if (colorImage == null)
            {
                throw new ArgumentNullException("colorImage");
            }

            if (depthImage == null)
            {
                throw new ArgumentNullException("depthImage");
            }

            if (colorImageFormat != this.initializationColorImageFormat)
            {
                throw new InvalidOperationException("Color image frame format different from initialization");
            }

            if (depthImageFormat != this.initializationDepthImageFormat)
            {
                throw new InvalidOperationException("Depth image frame format different from initialization");
            }

            if (colorImage.Length != this.videoCameraConfig.FrameBufferLength)
            {
                throw new ArgumentOutOfRangeException("colorImage", "Color image data size is needs to match initialization configuration.");
            }

            if (depthImage.Length != this.depthCameraConfig.FrameBufferLength)
            {
                throw new ArgumentOutOfRangeException("depthImage", "Depth image data size is needs to match initialization configuration.");
            }

            int hr;
            HeadPoints headPointsObj = null;
            Vector3DF[] headPoints = GetHeadPointsFromSkeleton(skeletonOfInterest);

            if (headPoints != null && headPoints.Length == 2)
            {
                headPointsObj = new HeadPoints { Points = headPoints };
            }

            this.copyStopwatch.Start();
            this.colorFaceTrackingImage.CopyFrom(colorImage);
            this.depthFaceTrackingImage.CopyFrom(depthImage);
            this.copyStopwatch.Stop();

            var sensorData = new SensorData(this.colorFaceTrackingImage, this.depthFaceTrackingImage, DefaultZoomFactor, Point.Empty);
            FaceTrackingSensorData faceTrackSensorData = sensorData.FaceTrackingSensorData;

            this.startOrContinueTrackingStopwatch.Start();
            if (this.trackSucceeded)
            {
                hr = this.faceTrackerInteropPtr.ContinueTracking(ref faceTrackSensorData, headPointsObj, this.frame.ResultPtr);
            }
            else
            {
                hr = this.faceTrackerInteropPtr.StartTracking(
                    ref faceTrackSensorData, ref regionOfInterest, headPointsObj, this.frame.ResultPtr);
            }

            this.startOrContinueTrackingStopwatch.Stop();

            this.trackSucceeded = hr == (int)ErrorCode.Success && this.frame.Status == ErrorCode.Success;
            this.trackStopwatch.Stop();

            if (this.trackSucceeded)
            {
                ++this.totalSuccessTracks;
                this.totalSuccessTrackMs += this.trackStopwatch.ElapsedMilliseconds - this.lastSuccessTrackElapsedMs;
                this.lastSuccessTrackElapsedMs = this.trackStopwatch.ElapsedMilliseconds;
            }

            return this.frame;
        }
    }
}