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
 * $Id: TrackerMultiMarkerImpl.cxx 164 2006-05-02 11:29:10Z daniel $
 * @file
 * ======================================================================== */


#ifndef __ARTOOLKITPLUS_TRACKERMULTIMARKERIMPL_HEADERFILE__
#error ARToolKitPlus/TrackerMultiMarkerImpl.cxx should not be compiled directly, but only if included from ARToolKitPlus/TrackerMultiMarkerImpl.h
#endif


namespace ARToolKitPlus
{


ARMM_TEMPL_FUNC
ARMM_TEMPL_TRACKER::TrackerMultiMarkerImpl(int nWidth, int nHeight)
{
	this->logger = NULL;

	this->screenWidth = nWidth;
	this->screenHeight = nHeight;

	useDetectLite = true;
	numDetected = 0;

	config = 0;

	this->thresh = 150;
}


ARMM_TEMPL_FUNC
ARMM_TEMPL_TRACKER::~TrackerMultiMarkerImpl()
{
	cleanup();
	if(config)
		arMultiFreeConfig(config);
}


ARMM_TEMPL_FUNC bool
ARMM_TEMPL_TRACKER::init(const char* nCamParamFile, const char* nMultiFile, ARFloat nNearClip, ARFloat nFarClip,
						 ARToolKitPlus::Logger* nLogger)
{
	// init some "static" from TrackerMultiMarker
	//
	if(this->marker_infoTWO==NULL)
		this->marker_infoTWO = artkp_Alloc<ARMarkerInfo2>(AR_TEMPL_TRACKER::MAX_IMAGE_PATTERNS);

	this->logger = nLogger;

	if(!loadCameraFile(nCamParamFile, nNearClip, nFarClip))
		return false;

	if(config)
		arMultiFreeConfig(config);

    if((config = arMultiReadConfigFile(nMultiFile)) == NULL )
        return false;

	if(this->logger)
		this->logger->artLogEx("INFO: %d markers loaded from config file", config->marker_num);

    return true;
}


ARMM_TEMPL_FUNC int
ARMM_TEMPL_TRACKER::calc(const unsigned char* nImage)
{
	numDetected = 0;
	int				tmpNumDetected;
    ARMarkerInfo    *tmp_markers;

	if(useDetectLite)
	{
		if(arDetectMarkerLite(const_cast<unsigned char*>(nImage), this->thresh, &tmp_markers, &tmpNumDetected) < 0)
			return 0;
	}
	else
	{
		if(arDetectMarker(const_cast<unsigned char*>(nImage), this->thresh, &tmp_markers, &tmpNumDetected) < 0)
			return 0;
	}

	for(int i=0; i<tmpNumDetected; i++)
		if(tmp_markers[i].id!=-1)
		{
			detectedMarkers[numDetected] = tmp_markers[i];
			detectedMarkerIDs[numDetected++] = tmp_markers[i].id;
			if(numDetected>=__MAX_IMAGE_PATTERNS)							// increase this value if more markers should be possible to be detected in one image...
				break;
		}

	if(executeMultiMarkerPoseEstimator(tmp_markers, tmpNumDetected, config) < 0)
		return 0;

	convertTransformationMatrixToOpenGLStyle(config->trans, this->gl_para);
	return numDetected;
}


ARMM_TEMPL_FUNC void
ARMM_TEMPL_TRACKER::getDetectedMarkers(int*& nMarkerIDs)
{
	nMarkerIDs = detectedMarkerIDs;
}


ARMM_TEMPL_FUNC void
ARMM_TEMPL_TRACKER::getARMatrix(ARFloat nMatrix[3][4]) const
{
	for(int i=0; i<3; i++)
		for(int j=0; j<4; j++)
			nMatrix[i][j] = config->trans[i][j];
}



ARMM_TEMPL_FUNC void*
ARMM_TEMPL_TRACKER::operator new(size_t size)
{
#ifndef _ARTKP_NO_MEMORYMANAGER_
	if(memManager)
		return memManager->getMemory(size);
	else
#endif //_ARTKP_NO_MEMORYMANAGER_
		return malloc(size);
}


ARMM_TEMPL_FUNC void
ARMM_TEMPL_TRACKER::operator delete(void *rawMemory)
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


ARMM_TEMPL_FUNC size_t
ARMM_TEMPL_TRACKER::getMemoryRequirements()
{
	size_t size = sizeof(ARMM_TEMPL_TRACKER);

	size += AR_TEMPL_TRACKER::getDynamicMemoryRequirements();

	return size;
}


}	// namespace ARToolKitPlus
