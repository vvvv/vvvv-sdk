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
* $Id: TrackerMultiMarkerImpl.h 172 2006-07-25 14:05:47Z daniel $
* @file
* ======================================================================== */


#ifndef __ARTOOLKITPLUS_TRACKERMULTIMARKERIMPL_HEADERFILE__
#define __ARTOOLKITPLUS_TRACKERMULTIMARKERIMPL_HEADERFILE__


#include <ARToolKitPlus/TrackerMultiMarker.h>
#include <ARToolKitPlus/TrackerImpl.h>
#include <ARToolKitPlus/Logger.h>


#define ARMM_TEMPL_FUNC template <int __PATTERN_SIZE_X, int __PATTERN_SIZE_Y, int __PATTERN_SAMPLE_NUM, int __MAX_LOAD_PATTERNS, int __MAX_IMAGE_PATTERNS>
#define ARMM_TEMPL_TRACKER TrackerMultiMarkerImpl<__PATTERN_SIZE_X, __PATTERN_SIZE_Y, __PATTERN_SAMPLE_NUM, __MAX_LOAD_PATTERNS, __MAX_IMAGE_PATTERNS>


namespace ARToolKitPlus
{


/// TrackerMultiMarkerImpl implements the TrackerMultiMarker interface
/**
 *  __PATTERN_SIZE_X describes the pattern image width (16 by default).
 *  __PATTERN_SIZE_Y describes the pattern image height (16 by default).
 *  __PATTERN_SAMPLE_NUM describes the maximum resolution at which a pattern is sampled from the camera image
 *  (64 by default, must a a multiple of __PATTERN_SIZE_X and __PATTERN_SIZE_Y).
 *  __MAX_LOAD_PATTERNS describes the maximum number of pattern files that can be loaded.
 *  __MAX_IMAGE_PATTERNS describes the maximum number of patterns that can be analyzed in a camera image.
 *  Reduce __MAX_LOAD_PATTERNS and __MAX_IMAGE_PATTERNS to reduce memory footprint.
 */
template <int __PATTERN_SIZE_X, int __PATTERN_SIZE_Y, int __PATTERN_SAMPLE_NUM, int __MAX_LOAD_PATTERNS=32, int __MAX_IMAGE_PATTERNS=32>
class TrackerMultiMarkerImpl : public TrackerMultiMarker, protected TrackerImpl<__PATTERN_SIZE_X,__PATTERN_SIZE_Y, __PATTERN_SAMPLE_NUM, __MAX_LOAD_PATTERNS, __MAX_IMAGE_PATTERNS>
{
public:
	TrackerMultiMarkerImpl(int nWidth=320, int nHeight=240);
	~TrackerMultiMarkerImpl();

	/// initializes ARToolKit
	/// initializes TrackerSingleMarker
	/**
	 *  nCamParamFile is the name of the camera parameter file
	 *  nNearClip & nFarClip are near and far clipping values for the OpenGL projection matrix
	 *  nLogger is an instance which implements the ARToolKit::Logger interface
	 */
	virtual bool init(const char* nCamParamFile, const char* nMultiFile, ARFloat nNearClip, ARFloat nFarClip,
			  ARToolKitPlus::Logger* nLogger=NULL);

	/// calculates the transformation matrix
	/**
	 *	pass the image as RGBX (32-bits) in 320x240 pixels.
	 */
	virtual int calc(const unsigned char* nImage);

	/// Returns the number of detected markers used for multi-marker tracking
	virtual int getNumDetectedMarkers() const  {  return numDetected;  }

	/// Enables usage of arDetectMarkerLite. Otherwise arDetectMarker is used
	/**
	 * In general arDetectMarker is more powerful since it keeps history about markers.
	 * In some cases such as very low camera refresh rates it is advantegous to change this.
	 * Using the non-lite version treats each image independent.
	 */
	virtual void setUseDetectLite(bool nEnable)  {  useDetectLite = nEnable;  }

	virtual void getDetectedMarkers(int*& nMarkerIDs);

	virtual const ARMarkerInfo& getDetectedMarker(int nWhich) const  {  return detectedMarkers[nWhich];  }

	virtual const ARMultiMarkerInfoT* getMultiMarkerConfig() const  {  return config;  }

	/// Provides access to ARToolKit' internal version of the transformation matrix
	/**
	*  This method is primarily for compatibility issues with code previously using
	*  ARToolKit rather than ARToolKitPlus. This is the original transformation
	*  matrix ARToolKit calculates rather than the OpenGL style version of this matrix
	*  that can be retrieved via getModelViewMatrix().
	*/
	virtual void getARMatrix(ARFloat nMatrix[3][4]) const;


	//
	// reimplement TrackerImpl into TrackerSingleMarker interface
	//
	// TODO: something like 'using cleanup;' would be nicer but does seem to work...
	//
	void cleanup()  {  AR_TEMPL_TRACKER::cleanup();  }
	bool setPixelFormat(PIXEL_FORMAT nFormat)  {  return AR_TEMPL_TRACKER::setPixelFormat(nFormat);  }
	bool loadCameraFile(const char* nCamParamFile, ARFloat nNearClip, ARFloat nFarClip)  {  return AR_TEMPL_TRACKER::loadCameraFile(nCamParamFile, nNearClip, nFarClip);  }
	void setLoadUndistLUT(bool nSet)  {  AR_TEMPL_TRACKER::setLoadUndistLUT(nSet);  }
	void setLogger(ARToolKitPlus::Logger* nLogger)  {  AR_TEMPL_TRACKER::setLogger(nLogger);  }
	int arDetectMarker(ARUint8 *dataPtr, int thresh, ARMarkerInfo **marker_info, int *marker_num)  {  return AR_TEMPL_TRACKER::arDetectMarker(dataPtr, thresh, marker_info, marker_num);  }
	int arDetectMarkerLite(ARUint8 *dataPtr, int thresh, ARMarkerInfo **marker_info, int *marker_num)  {  return AR_TEMPL_TRACKER::arDetectMarkerLite(dataPtr, thresh, marker_info, marker_num);  }
	ARFloat arMultiGetTransMat(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config)  {  return AR_TEMPL_TRACKER::arMultiGetTransMat(marker_info, marker_num, config);  }
	ARFloat arGetTransMat(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4])  {  return AR_TEMPL_TRACKER::arGetTransMat(marker_info, center, width, conv);  }
	ARFloat arGetTransMatCont(ARMarkerInfo *marker_info, ARFloat prev_conv[3][4], ARFloat center[2], ARFloat width, ARFloat conv[3][4])  {  return AR_TEMPL_TRACKER::arGetTransMatCont(marker_info, prev_conv, center, width, conv);  }
	ARFloat rppMultiGetTransMat(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config)  {  return AR_TEMPL_TRACKER::rppMultiGetTransMat(marker_info, marker_num, config);  }
	ARFloat rppGetTransMat(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4])  {  return AR_TEMPL_TRACKER::rppGetTransMat(marker_info, center, width, conv);  }
	int arLoadPatt(char *filename)  {  return AR_TEMPL_TRACKER::arLoadPatt(filename);  }
	int arFreePatt(int patno)  {  return AR_TEMPL_TRACKER::arFreePatt(patno);  }
	int arMultiFreeConfig(ARMultiMarkerInfoT *config)  {  return AR_TEMPL_TRACKER::arMultiFreeConfig(config);  }
	ARMultiMarkerInfoT *arMultiReadConfigFile(const char *filename)  {  return AR_TEMPL_TRACKER::arMultiReadConfigFile(filename);  }
	void activateBinaryMarker(int nThreshold)  {  AR_TEMPL_TRACKER::activateBinaryMarker(nThreshold);  }
	void setMarkerMode(MARKER_MODE nMarkerMode)  {  AR_TEMPL_TRACKER::setMarkerMode(nMarkerMode);  }
	void activateVignettingCompensation(bool nEnable, int nCorners=0, int nLeftRight=0, int nTopBottom=0)  {  AR_TEMPL_TRACKER::activateVignettingCompensation(nEnable, nCorners, nLeftRight, nTopBottom);  }
	void changeCameraSize(int nWidth, int nHeight)  {  AR_TEMPL_TRACKER::changeCameraSize(nWidth, nHeight);  }
	void setUndistortionMode(UNDIST_MODE nMode)  {  AR_TEMPL_TRACKER::setUndistortionMode(nMode);  }
	bool setPoseEstimator(POSE_ESTIMATOR nMethod) {  return AR_TEMPL_TRACKER::setPoseEstimator(nMethod);  }
	void setBorderWidth(ARFloat nFraction)  {  AR_TEMPL_TRACKER::setBorderWidth(nFraction);  }
	void setThreshold(int nValue)  {  AR_TEMPL_TRACKER::setThreshold(nValue);  }
	int getThreshold() const  {  return AR_TEMPL_TRACKER::getThreshold();  }
	void activateAutoThreshold(bool nEnable)  {  AR_TEMPL_TRACKER::activateAutoThreshold(nEnable);  }
	bool isAutoThresholdActivated() const  {  return AR_TEMPL_TRACKER::isAutoThresholdActivated();  }
	void setNumAutoThresholdRetries(int nNumRetries)  {  AR_TEMPL_TRACKER::setNumAutoThresholdRetries(nNumRetries);  }
	const ARFloat* getModelViewMatrix() const  {  return AR_TEMPL_TRACKER::getModelViewMatrix();  }
	const ARFloat* getProjectionMatrix() const  {  return AR_TEMPL_TRACKER::getProjectionMatrix();  }
	const char* getDescription()  {  return AR_TEMPL_TRACKER::getDescription();  }
	PIXEL_FORMAT getPixelFormat() const  {  return static_cast<PIXEL_FORMAT>(AR_TEMPL_TRACKER::getPixelFormat());  }
	int getBitsPerPixel() const  {  return static_cast<PIXEL_FORMAT>(AR_TEMPL_TRACKER::getBitsPerPixel());  }
	int getNumLoadablePatterns() const  {  return AR_TEMPL_TRACKER::getNumLoadablePatterns();  }
	void setImageProcessingMode(IMAGE_PROC_MODE nMode)  {  AR_TEMPL_TRACKER::setImageProcessingMode(nMode);  }
	Profiler& getProfiler()  {  return AR_TEMPL_TRACKER::getProfiler();  }
	Camera* getCamera()  {  return AR_TEMPL_TRACKER::getCamera();  }
	void setCamera(Camera* nCamera)  {  AR_TEMPL_TRACKER::setCamera(nCamera);  }
	void setCamera(Camera* nCamera, ARFloat nNearClip, ARFloat nFarClip)  {  AR_TEMPL_TRACKER::setCamera(nCamera, nNearClip, nFarClip);  }
	ARFloat calcOpenGLMatrixFromMarker(ARMarkerInfo* nMarkerInfo, ARFloat nPatternCenter[2], ARFloat nPatternSize, ARFloat *nOpenGLMatrix)  {  return AR_TEMPL_TRACKER::calcOpenGLMatrixFromMarker(nMarkerInfo, nPatternCenter, nPatternSize, nOpenGLMatrix);  }
	ARFloat executeSingleMarkerPoseEstimator(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4])  {  return AR_TEMPL_TRACKER::executeSingleMarkerPoseEstimator(marker_info, center, width, conv);  }
	ARFloat executeMultiMarkerPoseEstimator(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config)  {  return AR_TEMPL_TRACKER::executeMultiMarkerPoseEstimator(marker_info, marker_num, config);  }

	static void* operator new(size_t size);

	static void operator delete(void *rawMemory);

	static size_t getMemoryRequirements();

protected:
	int				numDetected;
	bool			useDetectLite;

	ARMultiMarkerInfoT  *config;

	int				detectedMarkerIDs[AR_TEMPL_TRACKER::MAX_IMAGE_PATTERNS];
	ARMarkerInfo	detectedMarkers[AR_TEMPL_TRACKER::MAX_IMAGE_PATTERNS];
};


};	// namespace ARToolKitPlus


#include "../src/TrackerMultiMarkerImpl.cxx"


#endif //__ARTOOLKITPLUS_TRACKERMULTIMARKERIMPL_HEADERFILE__
