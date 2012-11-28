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
* $Id: TrackerSingleMarker.h 164 2006-05-02 11:29:10Z daniel $
* @file
* ======================================================================== */


#ifndef __TRACKERSINGLEMARKER_HEADERFILE__
#define __TRACKERSINGLEMARKER_HEADERFILE__


#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/Logger.h>


namespace ARToolKitPlus
{


/// Defines a simple interface for single-marker tracking with ARToolKitPlus
/**
 *  ARToolKitPlus::TrackerSingleMarker provides all methods to access ARToolKit for
 *  single marker tracking without needing to mess around with it low level methods directly.
 *
 *  A current restriction is that only the best detected marker is returned.
 *  If you need multi-marker tracking use TrackerMultiMarker.
 */
class TrackerSingleMarker : public Tracker
{
public:
	virtual ~TrackerSingleMarker()
	{}


	/// initializes TrackerSingleMarker
	/**
	 *  nCamParamFile is the name of the camera parameter file
	 *  nLogger is an instance which implements the ARToolKit::Logger interface
	 */
	virtual bool init(const char* nCamParamFile, ARFloat nNearClip, ARFloat nFarClip, ARToolKitPlus::Logger* nLogger=NULL) = 0;


	/// adds a pattern to ARToolKit
	/**
	 *  pass the patterns filename
	 */
	virtual int addPattern(const char* nFileName) = 0;

	/// calculates the transformation matrix
	/**
	 *	pass the image as RGBX (32-bits) in 320x240 pixels.
	 *  if nPattern is not -1 then only this pattern is accepted
	 *  otherwise any found pattern will be used.
	 */
	virtual int calc(const unsigned char* nImage, int nPattern=-1, bool nUpdateMatrix=true,
			 ARMarkerInfo** nMarker_info=NULL, int* nNumMarkers=NULL) = 0;

	/// Sets the width and height of the patterns.
	virtual void setPatternWidth(ARFloat nWidth) = 0;

	/// Provides access to ARToolKit' patt_trans matrix
	/**
	*  This method is primarily for compatibility issues with code previously using
	*  ARToolKit rather than ARToolKitPlus. patt_trans is the original transformation
	*  matrix ARToolKit calculates rather than the OpenGL style version of this matrix
	*  that can be retrieved via getModelViewMatrix().
	*/
	virtual void getARMatrix(ARFloat nMatrix[3][4]) const = 0;

	/// Returns the confidence value of the currently best detected marker.
	virtual ARFloat getConfidence() const = 0;
};


};	// namespace ARToolKitPlus


#endif //__TRACKERSINGLEMARKER_HEADERFILE__
