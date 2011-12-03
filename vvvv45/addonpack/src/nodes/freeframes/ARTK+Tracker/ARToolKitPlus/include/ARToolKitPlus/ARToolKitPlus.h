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
* $Id: ARToolKitPlus.h 167 2006-05-18 17:00:58Z daniel $
* @file
* ======================================================================== */


#ifndef __ARTOOLKITPLUS_HEADERFILE__
#define __ARTOOLKITPLUS_HEADERFILE__


#include <ARToolKitPlus/Logger.h>


#ifdef ARTOOLKITPLUS_DLL
#  ifdef ARTOOLKITPLUS_EXPORTS
#    define ARTOOLKITPLUS_API __declspec(dllexport)
#  else
#    define ARTOOLKITPLUS_API __declspec(dllimport)
#  endif // ARTOOLKITPLUS_EXPORTS
#else
#  define ARTOOLKITPLUS_API
#endif // ARTOOLKITPLUS_DLL


#define ARTOOLKITPLUS_VERSION_MAJOR 2
#define ARTOOLKITPLUS_VERSION_MINOR 1


namespace ARToolKitPlus
{


enum {
	DEF_CAMWIDTH = 320,
	DEF_CAMHEIGHT = 240
};


enum PIXEL_FORMAT {
	PIXEL_FORMAT_ABGR = 1,
	PIXEL_FORMAT_BGRA = 2,
	PIXEL_FORMAT_BGR = 3,
	PIXEL_FORMAT_RGBA = 4,
	PIXEL_FORMAT_RGB = 5,
	PIXEL_FORMAT_RGB565 = 6,
	PIXEL_FORMAT_LUM = 7
};


enum UNDIST_MODE {
	UNDIST_NONE,
	UNDIST_STD,
	UNDIST_LUT
};


enum IMAGE_PROC_MODE {
	IMAGE_HALF_RES,
	IMAGE_FULL_RES
};


// ARToolKitPlus versioning
//
enum ARTKP_VERSION {
	VERSION_MAJOR = ARTOOLKITPLUS_VERSION_MAJOR,
	VERSION_MINOR = ARTOOLKITPLUS_VERSION_MINOR
};


//enum IDMARKER_MODE {
//	IDMARKER_SIMPLE,
//	IDMARKER_BCH
//};

enum MARKER_MODE {
	MARKER_TEMPLATE,
	MARKER_ID_SIMPLE,
	MARKER_ID_BCH,
	//MARKER_ID_BCH2		// upcomming, not implemented yet
};

enum POSE_ESTIMATOR {
	POSE_ESTIMATOR_ORIGINAL,			// original "normal" pose estimator
	POSE_ESTIMATOR_ORIGINAL_CONT,		// original "cont" pose estimator
	POSE_ESTIMATOR_RPP					// new "Robust Planar Pose" estimator
};


class TrackerSingleMarker;
class MemoryManager;


ARTOOLKITPLUS_API ARToolKitPlus::TrackerSingleMarker* createTrackerSingleMarker(int nWidth, int nHeight,
																				int nMarkerSizeX, int nMarkerSizeY, int nMarkerSampleNum,
																				ARToolKitPlus::PIXEL_FORMAT nPixelFormat = ARToolKitPlus::PIXEL_FORMAT_RGB);


ARTOOLKITPLUS_API void setMemoryManager(MemoryManager* nManager);

ARTOOLKITPLUS_API MemoryManager* getMemoryManager();


}  // namespace ARToolKitPlus



#endif //__ARTOOLKITPLUS_HEADERFILE__
