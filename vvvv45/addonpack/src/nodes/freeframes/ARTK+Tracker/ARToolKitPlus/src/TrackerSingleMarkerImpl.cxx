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
 * $Id: TrackerSingleMarkerImpl.cxx 172 2006-07-25 14:05:47Z daniel $
 * @file
 * ======================================================================== */


//#pragma message ( "Compiling TrackerSingleMarkerImpl.cxx" )


#ifndef __ARTOOLKITPLUS_TRACKERSINGLEMARKERIMPL_HEADERFILE__
#error ARToolKitPlus/TrackerSingleMarkerImpl.cxx should not be compiled directly, but only if included from ARToolKitPlus/TrackerSingleMarkerImpl.h
#endif


namespace ARToolKitPlus
{


ARSM_TEMPL_FUNC
ARSM_TEMPL_TRACKER::TrackerSingleMarkerImpl(int nWidth, int nHeight)
{
	this->logger = NULL;

	this->screenWidth = nWidth;
	this->screenHeight = nHeight;

	this->thresh = 100;

	patt_width     = 80;
	patt_center[0] = patt_center[1] = 0.0;
}


ARSM_TEMPL_FUNC
ARSM_TEMPL_TRACKER::~TrackerSingleMarkerImpl()
{
	cleanup();
}


ARSM_TEMPL_FUNC bool
ARSM_TEMPL_TRACKER::init(const char* nCamParamFile, ARFloat nNearClip, ARFloat nFarClip, ARToolKitPlus::Logger* nLogger)
{
	if(nLogger)
		this->logger = nLogger;

	if(!this->checkPixelFormat())
	{	
		if(this->logger)
			this->logger->artLog("ARToolKitPlus: Invalid Pixel Format!");
		return false;
	}

	// init some "static" members from artoolkit
	// (some systems don't like such large global members
	// so we allocate this manually)
	//
	if(this->marker_infoTWO==NULL)
		this->marker_infoTWO = artkp_Alloc<ARMarkerInfo2>(__MAX_IMAGE_PATTERNS);

	//initialize applications
	if(nCamParamFile)
		return loadCameraFile(nCamParamFile, nNearClip, nFarClip);
	else
		return true;
}


ARSM_TEMPL_FUNC int
ARSM_TEMPL_TRACKER::calc(const unsigned char* nImage, int nPattern, bool nUpdateMatrix,
						  ARMarkerInfo** nMarker_info, int* nNumMarkers)
{
    ARMarkerInfo    *marker_info;
    int             marker_num;

	if(nImage==0)
		return 0;

	PROFILE_BEGINSEC(profiler, SINGLEMARKER_OVERALL)

	confidence = 0.0f;

    // detect the markers in the video frame
	//
    if(arDetectMarker(const_cast<unsigned char*>(nImage), this->thresh, &marker_info, &marker_num) < 0)
	{
		PROFILE_ENDSEC(profiler, SINGLEMARKER_OVERALL)
        return -1;
	}

    // find best visible marker
    int j, k = -1;
    for(j = 0; j < marker_num; j++)
        if(marker_info[j].id!=-1 && (nPattern==-1 || nPattern==marker_info[j].id))
		{
            if(k == -1)
				k = j;
			else
			if(marker_info[k].cf < marker_info[j].cf)
				k = j;
        }

	if(nMarker_info)
		*nMarker_info = marker_info;
	if(nNumMarkers)
		*nNumMarkers = marker_num;

	// nothing found ?
	//
    if(k == -1)
	{
		PROFILE_ENDSEC(profiler, SINGLEMARKER_OVERALL)
        return -1;
	}

	confidence = marker_info[k].cf;


	/////////////////////////////////////////////////////////////////////////
	//
	//       corner refinement begin
	//
/*	if(false)
	{
		const unsigned int roi_radius = 4;
		for(unsigned int i=0; i<4; i++)
		{
			ARFloat edge_x, edge_y;
			int c_ret = refineCorner(edge_x, edge_y,
				marker_info[k].vertex[i][0],
				marker_info[k].vertex[i][1],
				roi_radius, (void*) nImage,
				arCamera->xsize, arCamera->ysize);

			if(c_ret == 1)
			{
				marker_info[k].vertex[i][0] = edge_x;
				marker_info[k].vertex[i][1] = edge_y;
			}
		}
	}*/
	//
	//       corner refinement end
	//
	/////////////////////////////////////////////////////////////////////////



    // get the transformation between the marker and the real camera
	//
	if(nUpdateMatrix)
	{
		executeSingleMarkerPoseEstimator(&marker_info[k], patt_center, patt_width, patt_trans);
		convertTransformationMatrixToOpenGLStyle(patt_trans, this->gl_para);
	}

	PROFILE_ENDSEC(profiler, SINGLEMARKER_OVERALL)
	return marker_info[k].id;
}


ARSM_TEMPL_FUNC int
ARSM_TEMPL_TRACKER::addPattern(const char* nFileName)
{
	int patt_id = arLoadPatt(const_cast<char*>(nFileName));

    if(patt_id<0)
	{
		if(this->logger)
			this->logger->artLogEx("ARToolKitPlus: error loading pattern '%s'", nFileName);
	}

	return patt_id;
}


ARSM_TEMPL_FUNC void
ARSM_TEMPL_TRACKER::getARMatrix(ARFloat nMatrix[3][4]) const
{
	for(int i=0; i<3; i++)
		for(int j=0; j<4; j++)
			nMatrix[i][j] = patt_trans[i][j];
}


ARSM_TEMPL_FUNC void*
ARSM_TEMPL_TRACKER::operator new(size_t size)
{
#ifndef _ARTKP_NO_MEMORYMANAGER_
	if(memManager)
		return memManager->getMemory(size);
	else
#endif //_ARTKP_NO_MEMORYMANAGER_
		return malloc(size);
}


ARSM_TEMPL_FUNC void
ARSM_TEMPL_TRACKER::operator delete(void *rawMemory)
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


ARSM_TEMPL_FUNC size_t
ARSM_TEMPL_TRACKER::getMemoryRequirements()
{
	size_t size = sizeof(ARSM_TEMPL_TRACKER);

	size += AR_TEMPL_TRACKER::getDynamicMemoryRequirements();

	return size;
}


}	// namespace ARToolKitPlus
