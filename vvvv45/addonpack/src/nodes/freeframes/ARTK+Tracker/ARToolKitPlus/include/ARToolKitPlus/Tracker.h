/* ========================================================================
* PROJECT: ARToolKitPlus
* ========================================================================
* This work is based on the original ARToolKit developed by
*   Hirokazu Kato
*   Mark Billinghurst
*   HITLab, University of Washington, Seattle
* http://www.hitl.washington.edu/artoolkit/
*
* Copyright of the derived and new portions of this work
*     (C) 2006 Graz University of Technology
*
* This framework is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This framework is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this framework; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
* For further information please contact 
*   Dieter Schmalstieg
*   <schmalstieg@icg.tu-graz.ac.at>
*   Graz University of Technology, 
*   Institut for Computer Graphics and Vision,
*   Inffeldgasse 16a, 8010 Graz, Austria.
* ========================================================================
** @author   Daniel Wagner
*
* $Id: Tracker.h 172 2006-07-25 14:05:47Z daniel $
* @file
* ======================================================================== */


#ifndef __ARTOOLKIT_TRACKER_HEADERFILE__
#define __ARTOOLKIT_TRACKER_HEADERFILE__


#include <ARToolKitPlus/ARToolKitPlus.h>
#include <ARToolKitPlus/ar.h>
#include <ARToolKitPlus/arMulti.h>
#include <ARToolKitPlus/Logger.h>
#include <ARToolKitPlus/extra/Profiler.h>
#include <ARToolKitPlus/Camera.h>


namespace ARToolKitPlus {


/// Tracker is the vision core of ARToolKit.
/**
 * Almost all original ARToolKit methods are included here.
 * Exceptions: matrix & vector.
 *
 * Tracker includes all methods that are needed to create a
 * basic ARToolKit application (e.g. the simple example
 * from the original ARToolKit package)
 *
 * Application developers should usually prefer using the
 * more high level classes:
 *  - TrackerSingleMarker
 *  - TrackerMultiMarker
 */
class Tracker
{
public:
	virtual ~Tracker()
	{}

	/// does final clean up (memory deallocation)
	virtual void cleanup() = 0;


	/// Sets the pixel format of the camera image
	/**
	 *  Default format is RGB888 (PIXEL_FORMAT_RGB)
	 */
	virtual bool setPixelFormat(PIXEL_FORMAT nFormat) = 0;


	/// Loads a camera calibration file and stores data internally
	/**
	*  To prevent memory leaks, this method internally deletes an existing camera.
	*  If you want to use more than one camera, retrieve the existing camera using getCamera()
	*  and call setCamera(NULL); before loading another camera file.
	*  On destruction, ARToolKitPlus will only destroy the currently set camera. All other
	*  cameras have to be destroyed manually.
	*/
	virtual bool loadCameraFile(const char* nCamParamFile, ARFloat nNearClip, ARFloat nFarClip) = 0;


	/// Set to true to try loading camera undistortion table from a cache file
	/**
	 *  On slow platforms (e.g. Smartphone) creation of the undistortion lookup-table
	 *  can take quite a while. Consequently caching will speedup the start phase.
	 *  If set to true and no cache file could be found a new one will be created.
	 *  The cache file will get the same name as the camera file with the added extension '.LUT'
	 */
	virtual void setLoadUndistLUT(bool nSet) = 0;


	/// sets an instance which implements the ARToolKit::Logger interface
	virtual void setLogger(ARToolKitPlus::Logger* nLogger) = 0;


	/// marker detection using tracking history
	virtual int arDetectMarker(ARUint8 *dataPtr, int thresh, ARMarkerInfo **marker_info, int *marker_num) = 0;


	/// marker detection without using tracking history
	virtual int arDetectMarkerLite(ARUint8 *dataPtr, int thresh, ARMarkerInfo **marker_info, int *marker_num) = 0;


	/// calculates the transformation matrix between camera and the given multi-marker config
	virtual ARFloat arMultiGetTransMat(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config) = 0;
	/// calculates the transformation matrix between camera and the given marker
	virtual ARFloat arGetTransMat(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4]) = 0;

	virtual ARFloat arGetTransMatCont(ARMarkerInfo *marker_info, ARFloat prev_conv[3][4], ARFloat center[2], ARFloat width, ARFloat conv[3][4]) = 0;

	// RPP integration -- [t.pintaric]
	virtual ARFloat rppMultiGetTransMat(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config) = 0;
	virtual ARFloat rppGetTransMat(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4]) = 0;


	/// loads a pattern from a file
	virtual int arLoadPatt(char *filename) = 0;


	/// frees a pattern from memory
	virtual int arFreePatt(int patno) = 0;

	/// frees a multimarker config from memory
	virtual int arMultiFreeConfig( ARMultiMarkerInfoT *config ) = 0;

	/// reads a standard artoolkit multimarker config file
	virtual ARMultiMarkerInfoT *arMultiReadConfigFile(const char *filename) = 0;

	/// activates binary markers
	/**
	 *  markers are converted to pure black/white during loading
	 */
	virtual void activateBinaryMarker(int nThreshold) = 0;

	/// activate the usage of id-based markers rather than template based markers
	/**
	 *  Template markers are the classic marker type used in ARToolKit.
	 *  Id-based markers directly encode the marker id in the image.
	 *  Simple markers use 3-times redundancy to increase robustness, while
	 *  BCH markers use an advanced CRC algorithm to detect and repair marker damages.
	 *  See arBitFieldPattern.h for more information.
	 *  In order to use id-based markers, the marker size has to be 6x6, 12x12 or 18x18.
	 */
	virtual void setMarkerMode(MARKER_MODE nMarkerMode) = 0;


	/// activates the complensation of brightness falloff in the corners of the camera image
	/**
	 *  some cameras have a falloff in brightness at the border of the image, which creates
	 *  problems with thresholding the image. use this function to set a (linear) adapted
	 *  threshold value. the threshold value will stay exactly the same at the center but
	 *  will deviate near to the border. all values specify a difference, not absolute values!
	 *  nCorners define the falloff a all four corners. nLeftRight defines the falloff
	 *  at the half y-position at the left and right side of the image. nTopBottom defines the falloff
	 *  at the half x-position at the top and bottom side of the image.
	 *  all values between these 9 points (center, 4 corners, left, right, top, bottom) will
	 *  be interpolated.
	 */
	virtual void activateVignettingCompensation(bool nEnable, int nCorners=0, int nLeftRight=0, int nTopBottom=0) = 0;

	
	/// changes the resolution of the camera after the camerafile was already loaded
	virtual void changeCameraSize(int nWidth, int nHeight) = 0;


	/// Changes the undistortion mode
	/**
	 * Default value is UNDIST_STD which means that
	 * artoolkit's standard undistortion method is used.
	 */
	virtual void setUndistortionMode(UNDIST_MODE nMode) = 0;

	/// Changes the Pose Estimation Algorithm
	/**
	* POSE_ESTIMATOR_ORIGINAL (default): arGetTransMat()
	* POSE_ESTIMATOR_RPP: "Robust Pose Estimation from a Planar Target"
	*/
	virtual bool setPoseEstimator(POSE_ESTIMATOR nMethod) = 0;

	/// Sets a new relative border width. ARToolKit's default value is 0.25
	/**
	 * Take caution that the markers need of course really have thiner borders.
	 * Values other than 0.25 have not been tested for regular pattern-based matching,
	 * but only for id-encoded markers. It might be that the pattern creation process
	 * needs to be updated too.
	 */
	virtual void setBorderWidth(ARFloat nFraction) = 0;


	/// Sets the threshold value that is used for black/white conversion
	virtual void setThreshold(int nValue) = 0;


	/// Returns the current threshold value.
	virtual int getThreshold() const = 0;


	/// Enables or disables automatic threshold calculation
	virtual void activateAutoThreshold(bool nEnable) = 0;

	
	/// Returns true if automatic threshold calculation is activated
	virtual bool isAutoThresholdActivated() const = 0;


	/// Sets the number of times the threshold is randomized in case no marker was visible (Default: 2)
	/**
	 *  Autothreshold requires a visible marker to estime the optimal thresholding value. If
	 *  no marker is visible ARToolKitPlus randomizes the thresholding value until a marker is
	 *  found. This function sets the number of times ARToolKitPlus will randomize the threshold
	 *  value and research for a marker per calc() invokation until it gives up.
	 *  A value of 2 means that ARToolKitPlus will analyze the image a second time with an other treshold value
	 *  if it does not find a marker the first time. Each unsuccessful try uses less processing power
	 *  than a single full successful position estimation.
	 */
	virtual void setNumAutoThresholdRetries(int nNumRetries) = 0;


	/// Sets an image processing mode (half or full resolution)
	/**
	 *  Half resolution is faster but less accurate. When using
	 *  full resolution smaller markers will be detected at a
	 *  higher accuracy (or even detected at all).
	 */
	virtual void setImageProcessingMode(IMAGE_PROC_MODE nMode) = 0;


	/// Returns an opengl-style modelview transformation matrix
	virtual const ARFloat* getModelViewMatrix() const = 0;


	/// Returns an opengl-style projection transformation matrix
	virtual const ARFloat* getProjectionMatrix() const = 0;


	/// Returns a short description with compiled-in settings
	virtual const char* getDescription() = 0;


	/// Returns the compiled-in pixel format
	virtual PIXEL_FORMAT getPixelFormat() const = 0;


	/// Returns the number of bits required to store a single pixel
	virtual int getBitsPerPixel() const = 0;


	/// Returns the maximum number of patterns that can be loaded
	/**
	 *  This maximum number of loadable patterns can be set via the
	 *  __MAX_LOAD_PATTERNS template parameter
	 */
	virtual int getNumLoadablePatterns() const = 0;


	/// Returns the current camera
	virtual Camera* getCamera() = 0;


	/// Sets a new camera without specifying new near and far clip values
	virtual void setCamera(Camera* nCamera) = 0;


	/// Sets a new camera including specifying new near and far clip values
	virtual void setCamera(Camera* nCamera, ARFloat nNearClip, ARFloat nFarClip) = 0;


	/// Calculates the OpenGL transformation matrix for a specific marker info
	virtual ARFloat calcOpenGLMatrixFromMarker(ARMarkerInfo* nMarkerInfo, ARFloat nPatternCenter[2], ARFloat nPatternSize, ARFloat *nOpenGLMatrix) = 0;


	/// Returns the internal profiler object
	virtual Profiler& getProfiler() = 0;


	/// Calls the pose estimator set with setPoseEstimator() for single marker tracking
	virtual ARFloat executeSingleMarkerPoseEstimator(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4]) = 0;

	/// Calls the pose estimator set with setPoseEstimator() for multi marker tracking
	virtual ARFloat executeMultiMarkerPoseEstimator(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config) = 0;
};


}	// namespace ARToolKitPlus


#endif //__ARTOOLKIT_TRACKER_HEADERFILE__
