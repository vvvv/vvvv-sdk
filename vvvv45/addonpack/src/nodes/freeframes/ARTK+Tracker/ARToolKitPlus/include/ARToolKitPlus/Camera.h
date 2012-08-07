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
** @author   Thomas Pintaric
*
* $Id$
* @file
* ======================================================================== */


#ifndef __ARTOOLKIT_CAMERA_HEADERFILE__
#define __ARTOOLKIT_CAMERA_HEADERFILE__

#include <ARToolKitPlus/config.h>
#include <ARToolKitPlus/param.h>
#include <ARToolKitPlus/Logger.h>

namespace ARToolKitPlus {

class Camera : public ARParam
{
public:
	Camera()
	{  fileName = NULL;  }

	virtual ~Camera()
	{  delete fileName;  }

	virtual void observ2Ideal(ARFloat ox, ARFloat oy, ARFloat *ix, ARFloat *iy) = 0;
	virtual void ideal2Observ(ARFloat ix, ARFloat iy, ARFloat *ox, ARFloat *oy) = 0;
	virtual bool loadFromFile(const char* filename) = 0;
	virtual Camera* clone() = 0;
	virtual bool changeFrameSize(const int frameWidth, const int frameHeight) = 0;
	virtual void logSettings(Logger* p_log) = 0;

	char* getFileName() const  {  return fileName;  }

protected:
	void setFileName(const char* filename)
	{
		if(fileName)
			delete fileName;
		fileName = new char[strlen(filename)+1];
		strcpy(fileName, filename);
	}

	char* fileName;
};

}  // namespace ARToolKitPlus

#endif // __ARTOOLKIT_CAMERA_HEADERFILE__
