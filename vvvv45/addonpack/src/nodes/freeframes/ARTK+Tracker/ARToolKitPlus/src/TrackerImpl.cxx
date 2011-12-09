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
 * $Id: TrackerImpl.cxx 172 2006-07-25 14:05:47Z daniel $
 * @file
 * ======================================================================== */


//#include <ARToolKitPlus/Tracker.h>


//#if AR_PATT_SIZE_X!=16 || AR_PATT_SIZE_Y!=16
//#pragma message("CAUTION: AR_PATT_SIZE_X or AR_PATT_SIZE_Y are not standard size")
//#endif

//#pragma message ( "Compiling TrackerImpl.cxx" )


namespace ARToolKitPlus {


AR_TEMPL_FUNC 
AR_TEMPL_TRACKER::TrackerImpl()
{
	int i;

#ifdef _USE_GENERIC_TRIGONOMETRIC_
#  ifdef WIN32
#    pragma message(">>> using SinCos LUT")
#  endif
	Fixed28_Init();
#  else
#    ifdef WIN32
#      pragma message(">>> not using SinCos LUT")
#  endif
#endif

	// set default value to RGB888
	//
	pixelFormat = PIXEL_FORMAT_RGB;
	pixelSize = 3;

	binaryMarkerThreshold = -1;

	autoThreshold.enable = false;
	autoThreshold.numRandomRetries = 2;

	wmarker_num = 0;
	prev_num = 0;
	sprev_num[0] = sprev_num[1] = 0;

	marker_infoTWO = NULL;

	pattern_num = -1;
	for(i=0; i<MAX_LOAD_PATTERNS; i++)
		patf[i] = 0;
	evecf = 0;
	evecBWf = 0;

	// we allocate all large data dynamically
	//
	l_imageL = NULL;
	l_imageL_size = 0;

	workL = artkp_Alloc<int>(WORK_SIZE);
	work2L = artkp_Alloc<int>(WORK_SIZE*7);
	wareaL = artkp_Alloc<int>(WORK_SIZE);
	wclipL = artkp_Alloc<int>(WORK_SIZE*4);
	wposL = artkp_Alloc<ARFloat>(WORK_SIZE*2);

	//workL = new int[WORK_SIZE];
	//work2L = new int[WORK_SIZE*7];
	//wareaL = new int[WORK_SIZE];
	//wclipL = new int[WORK_SIZE*4];
	//wposL = new ARFloat[WORK_SIZE*2];


	// set all right side structures to NULL
	l_imageR = NULL;
	workR = NULL;
	work2R = NULL;
	wareaR = NULL;
	wclipR = NULL;
	wposR = NULL;

	//arDebug                 = 0;
	//arImage                 = NULL;
	arFittingMode           = DEFAULT_FITTING_MODE;
	arImageProcMode         = DEFAULT_IMAGE_PROC_MODE;
	
	arCamera                = NULL;
	loadCachedUndist        = false;
	//arParam;
	arImXsize = arImYsize	= 0;
	arTemplateMatchingMode  = DEFAULT_TEMPLATE_MATCHING_MODE;
	arMatchingPCAMode       = DEFAULT_MATCHING_PCA_MODE;
	arImageL                = NULL;
	//arImageR                = NULL;

	markerMode = MARKER_TEMPLATE;

	RGB565_to_LUM8_LUT = NULL;

	relBorderWidth = 0.25f;

	// undistortion addon by Daniel
	//
	undistMode = UNDIST_STD;
	undistO2ITable = NULL;
	//undistI2OTable = NULL;
	arParamObserv2Ideal_func = &AR_TEMPL_TRACKER::arParamObserv2Ideal_std;
	//arParamIdeal2Observ_func = arParamIdeal2Observ_std;

	vignetting.enabled = false;
	vignetting.corners = 
	vignetting.leftright = 
	vignetting.bottomtop = 0;

	bchProcessor = NULL;

	// RPP integration -- [t.pintaric]
	poseEstimator = POSE_ESTIMATOR_ORIGINAL;
	//poseEstimator_func = &AR_TEMPL_TRACKER::arGetTransMat;
	//multiPoseEstimator_func = &AR_TEMPL_TRACKER::arMultiGetTransMat;

	descriptionString = new char[512];

	profiler.reset();
}


AR_TEMPL_FUNC 
AR_TEMPL_TRACKER::~TrackerImpl()
{
	if(arCamera)
		delete arCamera;
	arCamera = NULL;

	if(bchProcessor)
		delete bchProcessor;
	bchProcessor = NULL;

	if(l_imageL)
		artkp_Free(l_imageL);
	l_imageL = NULL;

	if(workL)
		artkp_Free(workL);
	workL = NULL;

	if(work2L)
		artkp_Free(work2L);
	work2L = NULL;

	if(wareaL)
		artkp_Free(wareaL);
	wareaL = NULL;

	if(wclipL)
		artkp_Free(wclipL);
	wclipL = NULL;

	if(wposL)
		artkp_Free(wposL);
	wposL = NULL;

	if(RGB565_to_LUM8_LUT)
		artkp_Free(RGB565_to_LUM8_LUT);
	RGB565_to_LUM8_LUT = NULL;

	if(undistO2ITable)
		artkp_Free(undistO2ITable);
	undistO2ITable = NULL;

	if(descriptionString)
		delete [] descriptionString;
	descriptionString = NULL;
}


AR_TEMPL_FUNC bool
AR_TEMPL_TRACKER::setPixelFormat(PIXEL_FORMAT nFormat)
{
	PIXEL_FORMAT oldFormat = pixelFormat;

	switch(pixelFormat = nFormat)
	{
	case PIXEL_FORMAT_LUM:
		pixelSize=1;
		return true;

	case PIXEL_FORMAT_RGB565:
		pixelSize=2;
		return true;

	case PIXEL_FORMAT_BGR:
	case PIXEL_FORMAT_RGB:
		pixelSize=3;
		return true;

	case PIXEL_FORMAT_ABGR:
	case PIXEL_FORMAT_BGRA:
	case PIXEL_FORMAT_RGBA:
		pixelSize=4;
		return true;

	default:
		pixelFormat = oldFormat;
		return false;
	}
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::checkImageBuffer()
{
	// we have to take care here when using a memory manager that can not free memory
	// (usually this image buffer should only be built once - unless we change camera resolution)
	//

	int newSize = screenWidth*screenHeight;

	if(newSize==l_imageL_size)
		return;

	if(l_imageL)
		//delete l_imageL;
		artkp_Free(l_imageL);

	l_imageL_size = newSize;

	//l_imageL = new ARInt16[newSize];
	l_imageL = artkp_Alloc<ARInt16>(newSize);
}


AR_TEMPL_FUNC bool
AR_TEMPL_TRACKER::checkPixelFormat()
{
	switch(pixelFormat)
	{
	case PIXEL_FORMAT_LUM:
		return pixelSize==1;

	case PIXEL_FORMAT_RGB565:
		return pixelSize==2;

	case PIXEL_FORMAT_BGR:
	case PIXEL_FORMAT_RGB:
		return pixelSize==3;

	case PIXEL_FORMAT_ABGR:
	case PIXEL_FORMAT_BGRA:
	case PIXEL_FORMAT_RGBA:
		return pixelSize==4;

	default:
		return false;
	}
}


AR_TEMPL_FUNC bool
AR_TEMPL_TRACKER::loadCameraFile(const char* nCamParamFile, ARFloat nNearClip, ARFloat nFarClip)
{
	CameraFactory cf;
	Camera* c_ptr = cf.createCamera(nCamParamFile);
	if(c_ptr == NULL)
	{
		if(logger)
			logger->artLog("ARToolKitPlus: Camera parameter load error!\n");
		return false;
	}

	if(arCamera)
		delete arCamera;
	arCamera = NULL;

	setCamera(c_ptr, nNearClip,nFarClip);
	return true;
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::setCamera(Camera* nCamera)
{
	arCamera = nCamera;

	if(arCamera)
	{
		arCamera->changeFrameSize(screenWidth,screenHeight);
		arInitCparam(arCamera);
		arCamera->logSettings(logger);

		// Comment out if you want to get the matrix for camera calibration
		// printf("MAT=\n");
		// printf("[ %f %f %f;\n",arCamera->mat[0][0],arCamera->mat[0][1],arCamera->mat[0][2]);
		// printf("%f %f %f;\n",arCamera->mat[1][0],arCamera->mat[1][1],arCamera->mat[1][2]);
		// printf("%f %f %f ]\n",arCamera->mat[2][0],arCamera->mat[2][1],arCamera->mat[2][2]);

		buildUndistO2ITable(arCamera);
	}
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::setCamera(Camera* nCamera, ARFloat nNearClip, ARFloat nFarClip)
{
	setCamera(nCamera);

	ARParam gCparam = *((ARParam*)arCamera);

	for(int i = 0; i < 4; i++)
		gCparam.mat[1][i] = (gCparam.ysize-1)*(gCparam.mat[2][i]) - gCparam.mat[1][i];
	convertProjectionMatrixToOpenGLStyle(&gCparam, nNearClip,nFarClip, gl_cpara);
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::calcOpenGLMatrixFromMarker(ARMarkerInfo* nMarkerInfo, ARFloat nPatternCenter[2], ARFloat nPatternSize, ARFloat *nOpenGLMatrix)
{
	ARFloat tmpTrans[3][4];

	ARFloat err = executeSingleMarkerPoseEstimator(nMarkerInfo, nPatternCenter, nPatternSize, tmpTrans);
	convertTransformationMatrixToOpenGLStyle(tmpTrans, nOpenGLMatrix);

	return err;
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::convertTransformationMatrixToOpenGLStyle(ARFloat para[3][4], ARFloat gl_para[16])
{
    int     i, j;

    for( j = 0; j < 3; j++ ) {
        for( i = 0; i < 4; i++ ) {
            gl_para[i*4+j] = para[j][i];
        }
    }
    gl_para[0*4+3] = gl_para[1*4+3] = gl_para[2*4+3] = 0.0;
    gl_para[3*4+3] = 1.0;
}


AR_TEMPL_FUNC bool
AR_TEMPL_TRACKER::convertProjectionMatrixToOpenGLStyle( ARParam *param, ARFloat gnear, ARFloat gfar, ARFloat m[16] )
{
	return convertProjectionMatrixToOpenGLStyle2( param->mat, param->xsize, param->ysize, gnear, gfar, m );
}


AR_TEMPL_FUNC bool
AR_TEMPL_TRACKER::convertProjectionMatrixToOpenGLStyle2( ARFloat cparam[3][4], int width, int height, ARFloat gnear, ARFloat gfar, ARFloat m[16] )
{
    ARFloat   icpara[3][4];
    ARFloat   trans[3][4];
    ARFloat   p[3][3], q[4][4];
    int      i, j;

    if(arParamDecompMat(cparam, icpara, trans) < 0) {
        printf("gConvGLcpara: Parameter error!!\n");
        return false;
    }

    for( i = 0; i < 3; i++ ) {
        for( j = 0; j < 3; j++ ) {
            p[i][j] = icpara[i][j] / icpara[2][2];
        }
    }
    q[0][0] = (2.0f * p[0][0] / width);
    q[0][1] = (2.0f * p[0][1] / width);
    q[0][2] = ((2.0f * p[0][2] / width)  - 1.0f);
    q[0][3] = 0.0f;

    q[1][0] = 0.0f;
    q[1][1] = (2.0f * p[1][1] / height);
    q[1][2] = ((2.0f * p[1][2] / height) - 1.0f);
    q[1][3] = 0.0f;

    q[2][0] = 0.0f;
    q[2][1] = 0.0f;
    q[2][2] = (gfar + gnear)/(gfar - gnear);
    q[2][3] = -2.0f * gfar * gnear / (gfar - gnear);

    q[3][0] = 0.0f;
    q[3][1] = 0.0f;
    q[3][2] = 1.0f;
    q[3][3] = 0.0f;

    for( i = 0; i < 4; i++ ) {
        for( j = 0; j < 3; j++ ) {
            m[i+j*4] = q[i][0] * trans[0][j]
                     + q[i][1] * trans[1][j]
                     + q[i][2] * trans[2][j];
        }
        m[i+3*4] = q[i][0] * trans[0][3]
                 + q[i][1] * trans[1][3]
                 + q[i][2] * trans[2][3]
                 + q[i][3];
    }

	return true;
}


AR_TEMPL_FUNC bool
AR_TEMPL_TRACKER::calcCameraMatrix(const char* nCamParamFile, int nWidth, int nHeight, ARFloat nNear, ARFloat nFar, ARFloat *nMatrix)
{
	CameraFactory cf;
	Camera* pCam = cf.createCamera(nCamParamFile);
	if(pCam == NULL)
	{
		return(false);
	}

	pCam->changeFrameSize(AR_TEMPL_TRACKER::screenWidth,AR_TEMPL_TRACKER::screenHeight);

	int i;
    for(i = 0; i < 4; i++ )
        pCam->mat[1][i] = (pCam->ysize-1)*(pCam->mat[2][i]) - pCam->mat[1][i];

	ARFloat glcpara[16];

    if(!convertProjectionMatrixToOpenGLStyle((ARParam*)pCam, nNear,nFar, glcpara))
		return false;

	// convert to float (in case of ARFloat is def'ed to doubled
	for(i = 0; i < 16; i++ )
		nMatrix[i] = (ARFloat)glcpara[i];

	return(true);
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::changeCameraSize(int nWidth, int nHeight)
{
	screenWidth = nWidth;
	screenHeight = nHeight;

	arCamera->changeFrameSize(nWidth,nHeight);
	arInitCparam(arCamera);

	if(logger)
		logger->artLogEx("ARToolKitPlus: Changed CamSize %d, %d", arCamera->xsize, arCamera->ysize);
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::setUndistortionMode(UNDIST_MODE nMode)
{
	undistMode = nMode;
	switch(undistMode)
	{
	case UNDIST_NONE:
		arParamObserv2Ideal_func = &AR_TEMPL_TRACKER::arParamObserv2Ideal_none;
		//arParamIdeal2Observ_func = arParamIdeal2Observ_none;
		break;

	case UNDIST_STD:
		arParamObserv2Ideal_func = &AR_TEMPL_TRACKER::arParamObserv2Ideal_std;
		//arParamIdeal2Observ_func = arParamIdeal2Observ_std;
		break;

	case UNDIST_LUT:
		arParamObserv2Ideal_func = &AR_TEMPL_TRACKER::arParamObserv2Ideal_LUT;
		//arParamIdeal2Observ_func = arParamIdeal2Observ_LUT;
		break;
	}
}


AR_TEMPL_FUNC bool
AR_TEMPL_TRACKER::setPoseEstimator(POSE_ESTIMATOR nMode)
{
	poseEstimator = nMode;
/*	switch(poseEstimator)
	{
	case POSE_ESTIMATOR_ORIGINAL:
		poseEstimator_func = &AR_TEMPL_TRACKER::arGetTransMat;
		multiPoseEstimator_func = &AR_TEMPL_TRACKER::arMultiGetTransMat;		// will use arGetTransMat internally
		return true;

	case POSE_ESTIMATOR_ORIGINAL_CONT:
		poseEstimator_func = &AR_TEMPL_TRACKER::arGetTransMatCont2;
		multiPoseEstimator_func = &AR_TEMPL_TRACKER::arMultiGetTransMat;		// will use arGetTransMatCont2 internally
		return true;

	case POSE_ESTIMATOR_RPP:
		if(rppSupportAvailabe())
		{
			poseEstimator_func = &AR_TEMPL_TRACKER::rppGetTransMat;
			multiPoseEstimator_func = &AR_TEMPL_TRACKER::rppMultiGetTransMat;
			return true;
		}

		if(logger)
			logger->artLog("ARToolKitPlus: Failed to set RPP pose estimator - RPP disabled during build\n");
		return false;
	}

	return false;*/

	return true;
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::executeSingleMarkerPoseEstimator(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4])
{
	switch(poseEstimator)
	{
	case POSE_ESTIMATOR_ORIGINAL:
		return arGetTransMat(marker_info, center, width, conv);

	case POSE_ESTIMATOR_ORIGINAL_CONT:
		return arGetTransMatCont2(marker_info, center, width, conv);

	case POSE_ESTIMATOR_RPP:
		if(rppSupportAvailabe())
		{
			return rppGetTransMat(marker_info, center, width, conv);
		}
		if(logger)
			logger->artLog("ARToolKitPlus: Failed to set RPP pose estimator - RPP disabled during build\n");
		return -1.0f;
	}

	return -1.0f;
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::executeMultiMarkerPoseEstimator(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config)
{
	switch(poseEstimator)
	{
	case POSE_ESTIMATOR_ORIGINAL:
		return arMultiGetTransMat(marker_info, marker_num, config);

	case POSE_ESTIMATOR_ORIGINAL_CONT:
		return arMultiGetTransMat(marker_info, marker_num, config);

	case POSE_ESTIMATOR_RPP:
		if(rppSupportAvailabe())
		{
			return rppMultiGetTransMat(marker_info, marker_num, config);
		}
		if(logger)
			logger->artLog("ARToolKitPlus: Failed to set RPP pose estimator - RPP disabled during build\n");
		return -1.0f;
	}

	return -1.0f;
}



AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::activateVignettingCompensation(bool nEnable, int nCorners, int nLeftRight, int nTopBottom)
{
	vignetting.enabled = nEnable;
	vignetting.corners = nCorners;
	vignetting.leftright = nLeftRight;
	vignetting.bottomtop = nTopBottom;
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::setMarkerMode(MARKER_MODE nMarkerMode)
{
	markerMode = nMarkerMode;
}


static void
convertPixel16To24(unsigned short nPixel, unsigned char& nRed, unsigned char& nGreen, unsigned char& nBlue)
{
	const unsigned short RED_MASK    = 0x1F << 11;
	const unsigned short GREEN_MASK  = 0x3F << 5;
	const unsigned short BLUE_MASK   = 0x1F;

	nRed =   (unsigned char)((nPixel&RED_MASK) >> 8);
	nGreen = (unsigned char)((nPixel&GREEN_MASK) >> 3);
	nBlue =  (unsigned char)((nPixel&BLUE_MASK) << 3);
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::checkRGB565LUT()
{
	int i;

	if(RGB565_to_LUM8_LUT)
		return;

	//RGB565_to_LUM8_LUT = new unsigned char[LUM_TABLE_SIZE];
	RGB565_to_LUM8_LUT = artkp_Alloc<unsigned char>(LUM_TABLE_SIZE);


#ifdef SMALL_LUM8_TABLE
	for(i=0; i<(LUM_TABLE_SIZE >> 6); i++)
	{
		unsigned char red,green,blue;

		convertPixel16To24(i<<6, red, green, blue);
		RGB565_to_LUM8_LUT[i] =  (unsigned char)(((red<<1) + (green<<2) + green + blue)>>3);
	}
#else
	for(i=0; i<LUM_TABLE_SIZE; i++)
	{
		unsigned char red,green,blue;

		convertPixel16To24(i, red, green, blue);
		RGB565_to_LUM8_LUT[i] =  (unsigned char)(((red<<1) + (green<<2) + green + blue)>>3);
	}
#endif //SMALL_LUM8_TABLE
}


// cleanup function called when program exits
//
AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::cleanup()
{
	if(marker_infoTWO)
		//delete [] marker_infoTWO;
		artkp_Free(marker_infoTWO);
	marker_infoTWO = NULL;
}


AR_TEMPL_FUNC const char*
AR_TEMPL_TRACKER::getDescription()
{
	char* pixelformats[] = { "NONE", "ABGR", "BGRA", "BGR", "RGBA", "RGB", "RGB565", "LUM"  };
	int f = getPixelFormat();

	char *compilerstr = new char[256];

#ifdef __INTEL_COMPILER
	sprintf(compilerstr, "Intel C++ v%d.%d", __INTEL_COMPILER/100, __INTEL_COMPILER%100);
#   pragma message ( ">> ARToolKitPlus: compiling with Intel Compiler" )
#elif _MSC_VER
	sprintf(compilerstr, "MS C++ v%d.%d", _MSC_VER/100, _MSC_VER%100);
//#   pragma message ( ">> ARToolKitPlus: compiling with Microsoft Compiler" )
#elif __GNUC__
	sprintf(compilerstr, "GCC %d.%d.%d", __GNUC__, __GNUC_MINOR__,  __GNUC_PATCHLEVEL__);
#else
	sprintf(compilerstr, "Unknown Compiler");
#endif

	assert(strlen(compilerstr)<256);

//
// Under WinCE it is very important to use the Intel XScale compiler.
// This will double the speed of ARToolKit
//
#if defined(_WIN32_WCE) && defined(NDEBUG)

#  ifdef __INTEL_COMPILER
#    pragma message ( ">>>    WinCE Targeted with Intel Compiler" )
#  else
#    pragma message("  ")
#    pragma message("  >>>    WinCE PERFORMANCE WARNING: Release builds of ARToolKitPlus should be done with the Intel XScale Compiler !!!")
#    pragma message("  ")
#  endif

//#  ifndef _USEGPP_
//#    pragma message("  ")
//#    pragma message("  >>>    PERFORMANCE WARNING: Release builds of ARToolKitPlus should be done with the Intel GPP !!!")
//#    pragma message("  ")
//#  else
//#   pragma message ( ">> ARToolKitPlus: compiling with Intel GPP" )
//#  endif

#endif //defined(_WIN32_WCE) && defined(NDEBUG)

	sprintf(descriptionString,
			"ARToolKitPlus v%d.%d: built %s %s (%s); %s; %s precision; %dx%d marker; %s pixelformat; %scustom memory manager; RPP support %savailable.",
			VERSION_MAJOR, VERSION_MINOR,
			__DATE__, __TIME__,
			compilerstr,
#if defined(_USEFIXED_) || defined(_USEGPP_)
			"fixed-point",
#else
			"floating-point",
#endif
			usesSinglePrecision() ? "single" : "double",
			PATTERN_WIDTH,PATTERN_HEIGHT,
			f<=PIXEL_FORMAT_LUM ? pixelformats[f] : pixelformats[0],
#ifdef _ARTKP_NO_MEMORYMANAGER_
			"no ",
#else
			"",
#endif
			rppSupportAvailabe() ? "" : "not "
			);

	delete [] compilerstr;

	assert(strlen(descriptionString)<512);
	return descriptionString;
}


static bool usesSinglePrecision()
{
	return sizeof(ARFloat)==4;
}


AR_TEMPL_FUNC void*
AR_TEMPL_TRACKER::operator new(size_t size)
{
#ifndef _ARTKP_NO_MEMORYMANAGER_
	if(memManager)
		return memManager->getMemory(size);
	else
#endif //_ARTKP_NO_MEMORYMANAGER_
		return malloc(size);
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::operator delete(void *rawMemory)
{
	if(!rawMemory)
		return;

#ifndef _ARTKP_NO_MEMORYMANAGER_
	if(memManager)
		memManager->releaseMemory(rawMemory);
	else
#endif //_ARTKP_NO_MEMORYMANAGER_
		free(rawMemory);
}


AR_TEMPL_FUNC size_t
AR_TEMPL_TRACKER::getDynamicMemoryRequirements()
{
	// requirements for allocations in the constructor
	//
	size_t size = sizeof(unsigned int)*(WORK_SIZE +			// workL = new int[WORK_SIZE];
										WORK_SIZE*7 +		// work2L = new int[WORK_SIZE*7];
										WORK_SIZE +			// wareaL = new int[WORK_SIZE];
										WORK_SIZE*4 +		// wclipL = new int[WORK_SIZE*4];
										WORK_SIZE*2);		// wposL = new ARFloat[WORK_SIZE*2];

	// requirements for the image buffer (arImageL)
	//
	size += sizeof(ARUint8)*MAX_BUFFER_WIDTH*MAX_BUFFER_HEIGHT;


	// requirements for allocation of marker_infoTWO
	//
	size += sizeof(ARMarkerInfo2)*MAX_IMAGE_PATTERNS;


	// requirements for allocation of l_imageL
	//
	size += sizeof(ARInt16)*MAX_BUFFER_WIDTH*MAX_BUFFER_HEIGHT;


	// requirements for the lens undistortion table (undistO2ITable)
	//
	size += sizeof(unsigned int)*MAX_BUFFER_WIDTH*MAX_BUFFER_HEIGHT;


	// requirements for the RGB565 to gray table RGB565_to_LUM8_LUT
	//
	size += sizeof(unsigned char)*LUM_TABLE_SIZE;


	return size;
}


}  // namespace ARToolKitPlus
