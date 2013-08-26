// -----------------------------------------------------------------------
// <copyright file="FtInterop.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A declaration of the pointer to the function used for mapping Kinect's depth frame coordinates to video frame coordinates.
    /// By default the face tracking engine uses Kinect's depth to color frame mapping function when users do not pass a pointer to their custom version.
    /// </summary>
    /// <param name="depthFrameWidth">depth frame width</param>
    /// <param name="depthFrameHeight">depth frame height</param>
    /// <param name="colorFrameWidth">color frame width</param>
    /// <param name="colorFrameHeight">color frame height</param>
    /// <param name="zoomFactor">video frame zoom factor</param>
    /// <param name="viewOffset">video frame view offset in the native video camera frame defined by its left-top corner X,Y coordinates</param>
    /// <param name="depthX">X coordinate of the depth point to convert</param>
    /// <param name="depthY">Y coordinate of the depth point to convert</param>
    /// <param name="depthZ">distance in millimeters to the depth frame point defined by (depthX, depthY)</param>
    /// <param name="colorX">returned video frame X coordinate</param>
    /// <param name="colorY">returned video frame Y coordinate</param>
    /// <returns>S_OK if the method succeeds. E_INVALIDARG, E_POINTER if the method fails.</returns>
#if INTERNAL 
    public 
#else
    internal
#endif
 delegate int FaceTrackingRegisterDepthToColor(
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
        out int colorY);

    /// <summary>
    /// Face Tracking Native Error Codes
    /// </summary>
    internal enum ErrorCode
    {
        Success = 0,                            // S_OK - No Error. Success.
        InvalidModels = unchecked((int)0x8fac0001),   // FT_ERROR_INVALID_MODELS - Returned when the face tracking models loaded by the tracking engine have incorrect format
        InvalidInputImage = unchecked((int)0x8fac0002),   // FT_ERROR_INVALID_INPUT_IMAGE - Returned when passed input image is invalid
        FaceDetectorFailed = unchecked((int)0x8fac0003),   // FT_ERROR_FACE_DETECTOR_FAILED - Returned when face tracking fails due to face detection errors
        ActiveAppearanceModelFailed = unchecked((int)0x8fac0004),   // FT_ERROR_AAM_FAILED - Returned when face tracking fails due to errors in tracking individual face parts
        NeuralNetworkFailed = unchecked((int)0x8fac0005),   // FT_ERROR_NN_FAILED - Returned when face tracking fails due to inability of the Neural Network to find nose, mouth corners and eyes
        FaceTrackerUninitialized = unchecked((int)0x8fac0006),   // FT_ERROR_UNINITIALIZED - Returned when uninitialized face tracker is used
        InvalidModelPath = unchecked((int)0x8fac0007),   // FT_ERROR_INVALID_MODEL_PATH - Returned when a file path to the face model files is invalid or when the model files could not be located
        EvaluationFailed = unchecked((int)0x8fac0008),   // FT_ERROR_EVAL_FAILED - Returned when face tracking worked but later evaluation found that the quality of the results was poor
        InvalidCameraConfig = unchecked((int)0x8fac0009),   // FT_ERROR_INVALID_CAMERA_CONFIG - Returned when the passed camera configuration is invalid
        Invalid3DHint = unchecked((int)0x8fac000a),   // FT_ERROR_INVALID_3DHINT - Returned when the passed 3D hint vectors contain invalid values (for example out of range)
        HeadSearchFailed = unchecked((int)0x8fac000b),   // FT_ERROR_HEAD_SEARCH_FAILED - Returned when the system cannot find the head area in the passed data based on passed 3D hint vectors or region of interest rectangle
        UserLost = unchecked((int)0x8fac000c),   // FT_ERROR_USER_LOST - Returned when the user ID of the subject being tracked is switched or lost so we should call StartTracking on next call for tracking face
        KinectDllLoadFailed = unchecked((int)0x8fac000d)    // FT_ERROR_KINECT_DLL_FAILED - Returned when Kinect DLL failed to load
    }

    /// <summary>
    /// Face tracking can run in three modes of operation. The Kinect mode allows you to
    /// seamlessly use the library with Windows Kinect, while image mode is for 
    /// tracking videos & images
    /// </summary>
    internal enum OperationMode
    {
        Kinect,                 // The image data will be retrieved from the Kinect sensor frames
        ImageLocalBuffer,       // The image data will be passed to tracking on each call to Start/Continue tracking
        ImageExternalBuffer     // The image pointers are associated with the tracker and image data management will be managed outside of the tracker
    }

    /// <summary>
    /// Image formats that the face tracker supports
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal enum FaceTrackingImageFormat
    {
        FTIMAGEFORMAT_INVALID = 0,    // Invalid format
        FTIMAGEFORMAT_UINT8_GR8 = 1,    // Grayscale image where each pixel is 1 byte (or 8 bits). 
        FTIMAGEFORMAT_UINT8_R8G8B8 = 2,    // RGB image (same as ARGB but without an alpha channel).
        FTIMAGEFORMAT_UINT8_X8R8G8B8 = 3,    // Same as ARGB (the alpha channel byte is present but not used). 
        FTIMAGEFORMAT_UINT8_A8R8G8B8 = 4,    // ARGB format (the first byte is the alpha transparency channel; remaining bytes are 8-bit red, green, and blue channels). 
        FTIMAGEFORMAT_UINT8_B8G8R8X8 = 5,    // Same as BGRA (the alpha channel byte is present but not used). 
        FTIMAGEFORMAT_UINT8_B8G8R8A8 = 6,    // BGRA format (the last byte is the alpha transparency channel; remaining bytes are 8-bit red, green, and blue channels). 
        FTIMAGEFORMAT_UINT16_D16 = 7,    // 16-bit per pixel depth data that represents the distance to a pixel in millimeters. 
        FTIMAGEFORMAT_UINT16_D13P3 = 8     // 16-bit per pixel depth data that represents the distance to a pixel in millimeters. The last three bits represent the user ID (Kinect's depth data format).
    }

    /// <summary>
    /// IFTImage is a helper interface that can wrap various image buffers
    /// </summary>
    [ComImport, Guid("1A00A7BC-C217-11E0-AC90-0024811441FD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFTImage
    {
        /// <summary>
        /// Allocates memory for the image of passed width, height and format. The memory is owned by this interface and is released when the interface is released or 
        /// when another Allocate() call happens. Allocate() deallocates currently allocated memory if its internal buffers are not big enough to fit new image data. 
        /// If its internal buffers are big enough, no new allocation occurs. 
        /// </summary>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="format">Image format.</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be E_INVALIDARG, E_OUTOFMEMORY.
        /// STDMETHOD(Allocate)(THIS_ UINT width, UINT height, FaceTrackingImageFormat format) PURE;
        void Allocate(uint width, uint height, FaceTrackingImageFormat format);

        /// <summary>
        /// Attaches this interface to external memory pointed to by pData, which is assumed to be sufficiently large to contain an image of the given size and format. 
        /// The memory referenced by pData is not deallocated when this interface is released. The caller owns the image buffer in this case and is responsible for its lifetime management. 
        /// </summary>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="dataPtr">External image buffer.</param>
        /// <param name="format">Image format.</param>
        /// <param name="stride">Number of bytes between the beginning of two image rows (the image buffer could be aligned, so stride could be more than width*pixelSizeInBytes).</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be E_INVALIDARG, E_POINTER, E_OUTOFMEMORY.
        /// STDMETHOD(Attach)(THIS_ UINT width, UINT height, void* pData, FaceTrackingImageFormat format, UINT stride) PURE;
        void Attach(uint width, uint height, IntPtr dataPtr, FaceTrackingImageFormat format, uint stride);

        /// <summary>
        /// Frees internal memory and sets this image to the empty state (0 size)
        /// </summary>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be E_UNEXPECTED, E_POINTER, E_OUTOFMEMORY.
        /// STDMETHOD(Reset)(THIS) PURE;
        void Reset();

        /// <summary>Accessor to width of the image.</summary>
        /// <returns>Width of the image in pixel.</returns>
        /// STDMETHOD_(UINT, GetWidth)(THIS) PURE;
        [PreserveSig]
        uint GetWidth();

        /// <summary>Accessor to height of the image.</summary>
        /// <returns>Height of the image in pixel.</returns>
        /// STDMETHOD_(UINT, GetHeight)(THIS) PURE;
        [PreserveSig]
        uint GetHeight();

        /// <summary>Accessor to stride of the image.</summary>
        /// <returns>Stride of the image.</returns>
        /// STDMETHOD_(UINT, GetStride)(THIS) PURE;
        [PreserveSig]
        uint GetStride();

        /// <summary>Accessor to bytes per pixel of the image.</summary>
        /// <returns>Bytes per pixel of the image.</returns>
        /// STDMETHOD_(UINT, GetBytesPerPixel)(THIS) PURE;
        [PreserveSig]
        uint GetBytesPerPixel();

        /// <summary>Accessor to buffer size of the image.</summary>
        /// <returns>Size in bytes of the internal image buffer</returns>
        /// STDMETHOD_(UINT, GetBufferSize)(THIS) PURE;
        [PreserveSig]
        uint GetBufferSize();

        /// <summary>Accessor to format of the image.</summary>
        /// <returns>Format of the image.</returns>
        /// STDMETHOD_(FaceTrackingImageFormat, GetFormat)(THIS) PURE;
        FaceTrackingImageFormat GetFormat();

        /// <summary>Accessor to format of the image.</summary>
        /// <returns>BYTE pointer to buffer.</returns>
        /// STDMETHOD_(BYTE*, GetBuffer)(THIS) PURE;
        [PreserveSig]
        IntPtr GetBuffer();

        /// <summary>
        /// Accessor to the image buffer ownership state.
        /// </summary>
        /// True if this interface is attached to the external buffer, false - otherwise.
        /// STDMETHOD_(BOOL, IsAttached)(THIS) PURE;
        /// <returns>The attachment status.</returns>
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsAttached();

        /// <summary>
        /// Non-allocating copy method. It copies this image data to pDestImage. It fails, if pDestImage doesn't have the right size or format. 
        /// If pDestImage has a different format, then this method attempts to convert pixels to pDestImage image format (if possible and supported).
        /// </summary>
        /// <param name="destImage">Destination image to copy data to.</param>
        /// <param name="srcRect">Source rectangle to copy data from in the source image (to support cut and paste operation). If NULL, the whole image gets copied.</param>
        /// <param name="destRow">Destination location (row) of the image data (to support cut and paste operation).</param>
        /// <param name="destColumn">Destination location (column) of the image data (to support cut and paste operation).</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be E_INVALIDARG, E_POINTER, E_OUTOFMEMORY.
        /// STDMETHOD(CopyTo)(THIS_ IFTImage* pDestImage, const RECT* pSrcRect, UINT destRow, UINT destColumn) PURE;
        void CopyTo([In] IFTImage destImage, [In] ref Rect srcRect, uint destRow, uint destColumn);

        /// <summary>
        /// Draws a line on the image.
        /// </summary>
        /// <param name="startPoint">Start point in image coordinates.</param>
        /// <param name="endPoint">End  point in image coordinates.</param>
        /// <param name="color">Line color in ARGB format (first byte is not used, second byte is red channel, third is green, fourth is blue)</param>
        /// <param name="lineWidthPx">Line width in pixels.</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be E_INVALIDARG.
        /// STDMETHOD(DrawLine)(THIS_ POINT startPoint, POINT endPoint, uint color, UINT lineWidthPx) PURE;
        void DrawLine(Point startPoint, Point endPoint, uint color, uint lineWidthPx);
    }

    [ComImport, Guid("1A00A7BB-C217-11E0-AC90-0024811441FD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFTResult
    {
        // STDMETHOD(Reset)(THIS) PURE;
        void Reset();

        // STDMETHOD(CopyTo)(THIS_ IFTResult* pFTResultDst) PURE;
        [PreserveSig]
        int CopyTo([In] IFTResult destResult);

        // STDMETHOD(GetStatus)(THIS) PURE;
        [PreserveSig]
        int GetStatus();

        // STDMETHOD(GetFaceRect)(THIS_ RECT* pRect) PURE;
        void GetFaceRect(out Rect rect);

        // STDMETHOD(Get2DShapePoints)(THIS_ FT_VECTOR2D** ppPoints, UINT* pPointCount) PURE;
        void Get2DShapePoints(out IntPtr pointsPtr, out uint pointCount);

        // STDMETHOD(Get3DPose)(THIS_ FLOAT* pScale, FLOAT rotationXYZ[3], FLOAT translationXYZ[3]) PURE;
        void Get3DPose(out float scale, out Vector3DF rotationXYZ, out Vector3DF translationXYZ);

        // STDMETHOD(GetAUCoefficients)(THIS_ FLOAT** ppCoefficients, UINT* pAUCount) PURE;
        void GetAUCoefficients(out IntPtr animUnitCoeffPtr, out uint animUnitCount);
    }

    [ComImport, Guid("1A00A7BD-C217-11E0-AC90-0024811441FD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFTModel
    {
        // STDMETHOD_(UINT, GetSUCount)(THIS) PURE;
        [PreserveSig]
        uint GetSUCount();

        // STDMETHOD_(UINT, GetAUCount)(THIS) PURE;
        [PreserveSig]
        uint GetAUCount();

        // STDMETHOD(GetTriangles)(THIS_ FT_TRIANGLE** ppTriangles, UINT* pTriangleCount) PURE;
        void GetTriangles(out IntPtr trianglesPtr, out uint triangleCount);

        // STDMETHOD_(UINT, GetVertexCount)(THIS) PURE;
        [PreserveSig]
        uint GetVertexCount();

        // STDMETHOD(Get3DShape)(THIS_ const FLOAT* pSUCoefs, UINT suCount, const FLOAT* pAUCoefs, UINT auCount, FLOAT scale, const FLOAT roationXYZ[3], const FLOAT translationXYZ[3], 
        //    FT_VECTOR3D* pVertices, UINT vertexCount) PURE;
        void Get3DShape(IntPtr shapeUnitCoeffsPtr, uint shapeUnitCount, IntPtr animUnitCoeffPtr, uint animUnitCount, float scale, ref Vector3DF rotationXYZ, ref Vector3DF translationXYZ, IntPtr vertices, uint vertexCount);

        // STDMETHOD(GetProjectedShape)(THIS_ const FT_CAMERA_CONFIG* pCameraConfig, FLOAT zoomFactor, POINT viewOffset, const FLOAT* pSUCoefs, UINT suCount, const FLOAT* pAUCoefs, UINT auCount, FLOAT scale, const FLOAT rotationXYZ[3], const FLOAT translationXYZ[3], 
        //    FT_VECTOR2D* pVertices, UINT vertexCount) PURE;
        void GetProjectedShape(CameraConfig cameraConfig, float zoomFactor, Point viewOffset, IntPtr shapeUnitCoeffPtr, uint shapeUnitCount, IntPtr animUnitCoeffsPtr, uint animUnitCount, float scale, ref Vector3DF rotationXYZ, ref Vector3DF translationXYZ, IntPtr vertices, uint vertexCount);
    }

    /// <summary>
    /// IFTFaceTracker is the main interface used for face tracking. An IFTFaceTracking object is created by using the FTCreateFaceTracker Function.
    /// </summary>
    [ComImport, Guid("1A00A7BA-C217-11E0-AC90-0024811441FD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFTFaceTracker
    {
        /// <summary>Initializes an IFTFaceTracker instance.</summary>
        /// <param name="videoCameraConfig">Video camera configuration.</param>
        /// <param name="depthCameraConfig">Optional. Depth camera configuration. Not used if NULL is passed.</param>
        /// <param name="depthToColorMappingFunc">A pointer to the depth-to-color coordinates mapping function.</param>
        /// <param name="modelPath">Path to a directory that contains face model. This is optional parameter and if it's not set, default model will be used (this is default use case).</param>
        /// <returns>If the method succeeds, the return value is S_OK. If the method fails, the return value can be E_INVALIDARG, E_POINTER, FT_ERROR_INVALID_MODEL_PATH, FT_ERROR_INVALID_MODELS, E_OUTOFMEMORY</returns>
        /// STDMETHOD(Initialize)(THIS_ const FT_CAMERA_CONFIG* pVideoCameraConfig, const FT_CAMERA_CONFIG* pDepthCameraConfig, 
        /// FaceTrackingRegisterDepthToColor depthToColorMappingFunc, PCWSTR pszModelPath) PURE;
        [PreserveSig]
        int Initialize(CameraConfig videoCameraConfig, CameraConfig depthCameraConfig, IntPtr depthToColorMappingFunc, string modelPath);

        /// <summary>
        /// Resets the IFTFaceTracker instance to a clean state (the same state that exists after calling the Initialize() method).
        /// </summary>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED.
        /// STDMETHOD(Reset)(THIS) PURE;
        void Reset();

        /// <summary>
        /// Creates a result object instance and returns its IFTResult interface. The returned interface refcount is incremented, 
        /// so after you use it, you must release it by calling Release(). 
        /// </summary>
        /// <param name="faceTrackResult">A returned interface pointer if successful; otherwise, NULL if it cannot create this instance.</param>
        /// <returns>If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED, E_POINTER.</returns>
        /// STDMETHOD(CreateFTResult)(THIS_ IFTResult** ppFTResult) PURE;
        [PreserveSig]
        int CreateFTResult(out IFTResult faceTrackResult);

        /// <summary>
        /// Sets shape units (SUs) that the face tracker uses for passed values. 
        /// </summary>
        /// <param name="scale">Head scale. Defined as headSize/averageHeadSize. Must be positive.</param>
        /// <param name="shapeUnitCoeffsPtr">Float array of SU coefficients.</param>
        /// <param name="shapeUnitCount">Number of elements in the pSUCoefs array.</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED, E_INVALIDARG, E_POINTER.
        /// STDMETHOD(SetShapeUnits)(THIS_ FLOAT scale, const FLOAT* pSUCoefs, UINT suCount) PURE;
        void SetShapeUnits(float scale, float[] shapeUnitCoeffsPtr, uint shapeUnitCount);

        /// <summary>
        /// Returns shape units (SUs) that the face tracker is using. If the passed ppSUCoefs parameter is NULL, it returns number of SUs used in the loaded face model.
        /// </summary>
        /// <param name="scale">A pointer to a head scale variable.</param>
        /// <param name="shapeUnitCoeffsPtr">A pointer to a float array of shape unit coefficients. The array must be large enough to contain all of the SUs for the loaded face model.</param>
        /// <param name="shapeUnitCount">Number of returned shape unit coefficients. This parameter is IN/OUT and must be initialized to the size of the *ppSUCoefs array when passed in.</param>
        /// <param name="haveConverged">true if shape unit coefficients converged to realistic values; otherwise, false (the SU coefficients are still converging).</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED, E_INVALIDARG, E_POINTER.
        /// STDMETHOD(GetShapeUnits)(THIS_ FLOAT* pScale, FLOAT** ppSUCoefs, UINT* pSUCount, BOOL* pHaveConverged) PURE;
        void GetShapeUnits(out float scale, out IntPtr shapeUnitCoeffsPtr, [In, Out] ref uint shapeUnitCount, [MarshalAs(UnmanagedType.Bool)] out bool haveConverged);

        /// <summary>
        /// Sets the shape unit (SU) computational state. This method allows you to enable or disable 3D-shape computation in the face tracker. If enabled, the face tracker will continue to refine SUs. 
        /// </summary>
        /// <param name="isEnabled">true to enable SU computation, false to disable SU computation.</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED.
        /// STDMETHOD(SetShapeComputationState)(THIS_ BOOL isEnabled) PURE;
        void SetShapeComputationState([MarshalAs(UnmanagedType.Bool)] bool isEnabled);

        /// <summary>
        /// Returns whether the shape unit (SU) computational state is enabled or disabled. If enabled, the face tracker continues refining the SUs. 
        /// </summary>
        /// <param name="isEnabled">A pointer to a variable that receives the returned value.</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED, E_POINTER.
        /// STDMETHOD(GetShapeComputationState)(THIS_ BOOL* pIsEnabled) PURE;
        void GetComputationState([MarshalAs(UnmanagedType.Bool)] out bool isEnabled);

        /// <summary>
        /// Returns an IFTModel Interface interface to the loaded face model. The returned interface refcount is incremented, so after you use it, 
        /// you must release it by calling Release(). 
        /// </summary>
        /// <param name="model">A returned interface pointer if successful; otherwise, NULL if it cannot create this instance.</param>
        /// If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED, E_POINTER.
        /// STDMETHOD(GetFaceModel)(THIS_ IFTModel** ppModel) PURE;
        void GetFaceModel(out IFTModel model);

        /// <summary>
        /// Starts face tracking. StartTracking() detects a face based on the passed parameters, then identifies characteristic points and begins tracking. 
        /// This process is more expensive than simply tracking (done by calling ContinueTracking()), but more robust. Therefore, if running at a high frame rate 
        /// you should only use StartTracking() to initiate the tracking process, and then you should use ContinueTracking() to continue tracking. 
        /// If the frame rate is low and the face tracker cannot keep up with fast face and head movement (or if there is too much motion blur), 
        /// you can use StartTracking() solely (instead of the usual sequence of StartTracking(), ContinueTracking(), ContinueTracking(), and so on). 
        /// </summary>
        /// <param name="sensorData">Input from the video and depth cameras (depth is optional).</param>
        /// <param name="roi">Optional, NULL if not provided. Region of interest in the passed video frame where the face tracker should search for a face to initiate tracking.</param>
        /// <param name="headPoints">
        /// Optional, NULL if not provided. Array that contains two 3D points in camera space if known (for example, from a Kinect skeleton). 
        /// The first element is the neck position and the second element is the head center position. 
        /// The camera space is defined as: right handed, the origin at the camera optical center; Y points up; units are in meters. 
        /// </param>
        /// <param name="faceTrackResult">IFTResult Interface pointer that receives computed face tracking results.</param>
        /// <returns>If the method succeeds, the return value is S_OK. If the method fails due to programmatic errors, the return value can be FT_ERROR_UNINITIALIZED, E_INVALIDARG, E_POINTER.
        /// To check if the face tracking was successful, you should call IFTResult::GetStatus() method.
        /// </returns>
        /// STDMETHOD(StartTracking)(THIS_ const FaceTrackingSensorData* pSensorData, const RECT* pRoi, const FT_VECTOR3D headPoints[2], IFTResult* pFTResult) PURE;
        [PreserveSig]
        int StartTracking(ref FaceTrackingSensorData sensorData, ref Rect roi, HeadPoints headPoints, IFTResult faceTrackResult);

        /// <summary>
        /// Continues the face tracking process that was initiated by StartTracking(). This method is faster than StartTracking() and is used only for tracking. 
        /// If the face being tracked moves too far from the previous location (for example, when the input frame rate is low), this method fails. 
        /// </summary>
        /// <param name="sensorData">Input from the video and depth cameras (depth is optional).</param>
        /// <param name="headPoints">
        /// Optional, NULL if not provided. Array that contains two 3D points in camera space if known (for example, from a Kinect skeleton). 
        /// The first element is the neck position and the second element is the head center position. 
        /// The camera space is defined as: right handed, the origin at the camera optical center; Y points up; units are in meters. 
        /// </param>
        /// <param name="faceTrackResult">IFTResult Interface pointer that receives computed face tracking results.</param>
        /// <returns>If the method succeeds, the return value is S_OK. If the method fails due to programmatic errors, the return value can be FT_ERROR_UNINITIALIZED, E_INVALIDARG, E_POINTER.
        /// To check if the face tracking was successful, you should call IFTResult::GetStatus() method.
        /// </returns>
        /// STDMETHOD(ContinueTracking)(THIS_ const FaceTrackingSensorData* pSensorData, const FT_VECTOR3D headPoints[2], IFTResult* pFTResult) PURE;
        [PreserveSig]
        int ContinueTracking(ref FaceTrackingSensorData sensorData, HeadPoints headPoints, IFTResult faceTrackResult);

        /// <summary>
        /// Detects faces in the provided video frame. It returns an array of faces and the detection confidence level for each face. 
        /// The confidence level is a value between 0 and 1 (where 0 is the lowest and 1 is highest). 
        /// </summary>
        /// <param name="sensorData">Input from the video and depth cameras (currently, depth input is ignored).</param>
        /// <param name="roi">
        /// Optional, NULL if not provided. Region of interest in the video frame where the detector must look for faces. 
        /// If NULL, the detector uses the full frame.
        /// </param>
        /// <param name="faces">Returned array of weighted face rectangles (where weight is a detection confidence level).</param>
        /// <param name="facesCount">On input, it must have a size of the pFaces array. On output, it contains the number of faces detected and returned in pFaces.</param>
        /// <returns>If the method succeeds, the return value is S_OK. If the method fails, the return value can be FT_ERROR_UNINITIALIZED, E_INVALIDARG, E_POINTER, or FT_ERROR_FACE_DETECTOR_FAILED.</returns>
        /// STDMETHOD(DetectFaces)(THIS_ const FaceTrackingSensorData* pSensorData, const RECT* pRoi, FT_WEIGHTED_RECT* pFaces, UINT* pFaceCount) PURE;
        [PreserveSig]
        int DetectFaces(ref FaceTrackingSensorData sensorData, ref Rect roi, IntPtr faces, ref uint facesCount);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FaceTrackingSensorData
    {
        public IFTImage VideoFrame;  // video frame from a video camera
        public IFTImage DepthFrame;  // depth frame (optional) from the depth camera
        public float ZoomFactor;   // video frame zoom factor (it is 1.0f if there is no zoom)
        public Point ViewOffset;   // X, Y coordinates of the top-left corner of the view area in the camera video frame (hardware resolution could be higher than what is being processed by this API)
    }

    internal static class FtInterop
    {
        internal const string FaceTrackLibDll = "FaceTrackLib.dll";
    }

    /// <summary>
    /// P/Invoke methods to instantiate native objects of face tracking engine
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport(FtInterop.FaceTrackLibDll, CharSet = CharSet.Unicode)]
        public static extern IFTFaceTracker FTCreateFaceTracker(IntPtr reserved);

        [DllImport(FtInterop.FaceTrackLibDll, CharSet = CharSet.Unicode)]
        public static extern IFTImage FTCreateImage();
    }

    /// <summary>
    /// Contains the video or depth camera configuration parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class CameraConfig
    {
        /// <summary>
        /// Max width or height of the camera input frames in pixels supported by FT API. This allows to use cameras up to 256 megapixels. 
        /// </summary>
        public const uint MaxResolution = 16384;

        // Note that camera pixels should be square
        private readonly uint width;            // in pixels, allowed range - 1-UINT_MAX
        private readonly uint height;           // in pixels, allowed range - 1-UINT_MAX
        private readonly float focalLength;      // in pixels, allowed range - 0-FLOAT_MAX, where 0 value means - use an average focal length for modern video cameras
        private readonly FaceTrackingImageFormat imageFormat;
        private readonly uint bytesPerPixel;
        private readonly uint stride;
        private readonly uint frameBufferLength;

        public CameraConfig(uint width, uint height, float focalLength, FaceTrackingImageFormat imageFormat)
        {
            this.width = width;
            this.height = height;
            this.focalLength = focalLength;
            this.imageFormat = imageFormat;
            this.bytesPerPixel = Image.FormatToSize(this.imageFormat);
            this.stride = this.width * this.bytesPerPixel;

            switch (this.imageFormat)
            {
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_GR8:
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_R8G8B8:
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_X8R8G8B8:
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_A8R8G8B8:
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_B8G8R8X8:
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT8_B8G8R8A8:
                    this.frameBufferLength = this.height * this.stride;
                    break;
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT16_D16:
                case FaceTrackingImageFormat.FTIMAGEFORMAT_UINT16_D13P3:
                    this.frameBufferLength = this.height * this.width;
                    break;
                default:
                    throw new ArgumentException("Invalid image format specified");
            }
        }

        /// <summary>
        /// Width in pixels, allowed range - 1-MaxResolution
        /// </summary>
        public uint Width
        {
            get { return this.width; }
        }

        /// <summary>
        /// Height in pixels, allowed range - 1-MaxResolution
        /// </summary>
        public uint Height
        {
            get { return this.height; }
        }

        public FaceTrackingImageFormat ImageFormat
        {
            get { return this.imageFormat; }
        }

        public uint Stride
        {
            get { return this.stride; }
        }

        public uint FrameBufferLength
        {
            get { return this.frameBufferLength; }
        }
    }

    /// <summary>
    /// Contains input data for a face tracking operation.
    /// </summary>
    internal class SensorData
    {
        private readonly Image videoFrame;
        private readonly Image depthFrame;
        private readonly float zoomFactor;
        private readonly Point viewOffset;

        public SensorData(Image videoFrame, Image depthFrame, float zoomFactor, Point viewOffset)
        {
            this.videoFrame = videoFrame;
            this.depthFrame = depthFrame;
            this.zoomFactor = zoomFactor;
            this.viewOffset = viewOffset;
        }

        internal FaceTrackingSensorData FaceTrackingSensorData
        {
            get
            {
                var faceTrackSensorData = new FaceTrackingSensorData
                    {
                        VideoFrame = this.videoFrame != null ? this.videoFrame.ImagePtr : null,
                        DepthFrame = this.depthFrame != null ? this.depthFrame.ImagePtr : null,
                        ZoomFactor = this.zoomFactor,
                        ViewOffset = this.viewOffset
                    };

                return faceTrackSensorData;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class HeadPoints
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        private Vector3DF[] points;

        public Vector3DF[] Points
        {
            set { this.points = value; }
        }
    }
}