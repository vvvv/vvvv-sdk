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
* $Id: Profiler.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __ARTOOLKITPLUS_PROFILER_HEADERFILE__
#define __ARTOOLKITPLUS_PROFILER_HEADERFILE__


#if defined(WIN32) || defined(_WIN32_WCE)
#  define _ARTKP_IS_WINDOWS_
#endif


#ifdef _ARTKP_IS_WINDOWS_
#  include <windows.h>
#endif


namespace ARToolKitPlus {


class Profiler
{
public:
	enum MES {
		SINGLEMARKER_OVERALL,
			LABELING,
			DETECTMARKER2,
			GETMARKERINFO,

			GETTRANSMAT,
				GETINITROT,
				GETTRANSMAT3,
					GETTRANSMATSUB,
						MODIFYMATRIX_LOOP,
							MODIFYMATRIX,
								GETNEWMATRIX,
									GETROT,

		GETANGLE
	};

	struct Measurement {
#ifdef _ARTKP_IS_WINDOWS_
		LARGE_INTEGER secBegin, secEnd, sum;
#endif
		void reset();
	};

	Measurement _SINGLEMARKER_OVERALL, _LABELING, _DETECTMARKER2, _GETMARKERINFO, _GETTRANSMAT,
				_GETINITROT, _GETTRANSMAT3, _GETTRANSMATSUB, _MODIFYMATRIX_LOOP, _MODIFYMATRIX, _GETNEWMATRIX,
				_GETROT, _GETANGLE;

	void reset();
	void beginSection(Measurement& nM);
	void endSection(Measurement& nM);

	float getFraction(const Measurement& nNom, const Measurement& nDenom) const;
	float getFraction(MES nNom, MES nDenom) const;
	float getTime(MES nMes) const;

	void writeReport(const char* nFileName, unsigned int nNumRuns=1) const;

	static bool isProfilingEnabled();

protected:
	const Measurement* getMes(MES nMes) const;
};


}  // namespace ARToolKitPlus


//
// we use macros to simplify calls to the profiler
// and to turn profiling on/off
// define _USE_PROFILING_ in the project settings
// to activate profiling. note that this can reduce
// overall performance a lot when used too deeply
//
#ifdef _USE_PROFILING_
  #define PROFILE_BEGINSEC(obj, mes)            obj.beginSection(obj._##mes);
  #define PROFILE_ENDSEC(obj, mes)              obj.endSection(obj._##mes);

  #define PROFILEPTR_BEGINSEC(obj, mes)  if(obj) obj->beginSection(obj->_##mes);
  #define PROFILEPTR_ENDSEC(obj, mes)    if(obj) obj->endSection(obj->_##mes);
#else
  #define PROFILE_BEGINSEC(obj, mes);
  #define PROFILE_ENDSEC(obj, mes);

  #define PROFILEPTR_BEGINSEC(obj, mes)
  #define PROFILEPTR_ENDSEC(obj, mes)
#endif //_USE_PROFILING_


#endif //__ARTOOLKITPLUS_PROFILER_HEADERFILE__
