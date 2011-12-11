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
* $Id: Logger.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __ARTKLOGGER_HEADERFILE__
#define __ARTKLOGGER_HEADERFILE__


#include <stdio.h>
#include <stdarg.h>
#include <string.h>


namespace ARToolKitPlus
{


/// ARToolKit::Logger specifies the interface for a logging application
/**
 *  Several classes of the ARToolKitPlus library use the Logger interface to
 *  pass messages to the calling instance. An application should derive from
 *  this interface class and implement artLog() in order to get error
 *  and success messages from artoolkit.
 */
class Logger {
public:
	virtual ~Logger() {}

	/// Passes a simple string to the implementing instance
	virtual void artLog(const char* nStr) = 0;


	/// Passes an sprintf like string plus ellipsis to the implementing instance
	/**
	 *  A default implementation is provided which should usually be sufficient.
	 *  Only in rare cases it will make sense to override this method and
	 *  create a custom implementation.
	 */
	virtual void artLogEx(const char* nStr, ...)
	{
		char tmpString[512];
		va_list marker;

		va_start(marker, nStr);
		vsprintf(tmpString, nStr, marker);

		//if(tmpString[strlen(tmpString)-1] == '\n')  // was bringt das?
		//	tmpString[strlen(tmpString)-1] = 0;

		artLog(tmpString);
	}
};


};	// namespace PN


#endif //__ARTKLOGGER_HEADERFILE__
