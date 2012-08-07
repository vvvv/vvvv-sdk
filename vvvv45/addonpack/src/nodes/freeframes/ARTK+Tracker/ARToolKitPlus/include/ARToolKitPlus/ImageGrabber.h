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
* $Id: ImageGrabber.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __ARTOOLKITIMAGEGRABBER_HEADERFILE__
#define __ARTOOLKITIMAGEGRABBER_HEADERFILE__


namespace ARToolKitPlus {


// This class is depricated
class ImageGrabber
{
public:
	// reads the camera and returns a 32-bit camera image
	// including pixel format conversion.
	// this method does all three steps below...
	//
	virtual const unsigned char* grabImage() = 0;

	// just reads the image, but does no conversion yet
	// the image returned by getImage() is not modified!
	//
	virtual void readImage() = 0;

	// converts the image and therefore modifies the buffer
	// returned by getImage()
	//
	virtual void convertImage() = 0;

	// returns the image buffer of the converted image
	// (format: RGBX8888)
	//
	virtual const unsigned char* getImage() = 0;
};


}  // namespace ARToolKitPlus


#endif //__ARTOOLKITIMAGEGRABBER_HEADERFILE__
