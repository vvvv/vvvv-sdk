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
 * $Id: paramDistortion.cxx 172 2006-07-25 14:05:47Z daniel $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <math.h>
#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/Camera.h>
#include <ARToolKitPlus/param.h>


namespace ARToolKitPlus {


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arParamObserv2Ideal_std(Camera* pCam, ARFloat ox, ARFloat oy, ARFloat *ix, ARFloat *iy)
{
	pCam->observ2Ideal(ox,oy,ix,iy);
	return(0);
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arParamIdeal2Observ_std(Camera* pCam, ARFloat ix, ARFloat iy, ARFloat *ox, ARFloat *oy)
{
	pCam->ideal2Observ(ix,iy,ox,oy);
	return(0);
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arParamObserv2Ideal_none(Camera* pCam, ARFloat ox, ARFloat oy, ARFloat *ix, ARFloat *iy)
{
	*ix = ox;
	*iy = oy;
	return(0);
}

//
//  these functions store 2 values (x & y) in a single 32-bit unsigned integer
//  each value is stored as 11.5 fixed point
//

inline
void floatToFixed(ARFloat nX, ARFloat nY, unsigned int &nFixed)
{
	short sx = (short)(nX * 32);
	short sy = (short)(nY * 32);

	unsigned short *ux = (unsigned short*)&sx;
	unsigned short *uy = (unsigned short*)&sy;

	nFixed = (*ux << 16) | *uy;
}


inline
void fixedToFloat(unsigned int nFixed, ARFloat& nX, ARFloat& nY)
{
	unsigned short ux = (nFixed >> 16);
	unsigned short uy = (nFixed & 0xffff);

	short *sx = (short*)&ux;
	short *sy = (short*)&uy;

	nX = (*sx)/32.0f;
	nY = (*sy)/32.0f;
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arParamObserv2Ideal_LUT(Camera* pCam, ARFloat ox, ARFloat oy, ARFloat *ix, ARFloat *iy)
{
	if(!undistO2ITable)
		buildUndistO2ITable(pCam);

	int x=(int)ox, y=(int)oy;

	fixedToFloat(undistO2ITable[x+y*arImXsize], *ix,*iy);
	return 0;
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::buildUndistO2ITable(Camera* pCam)
{
	int x,y;
	ARFloat cx,cy, ox,oy;
	unsigned int fixed;
	char* cachename = NULL;
	bool loaded = false;

	if(loadCachedUndist)
	{
		assert(pCam->getFileName());
		cachename = new char[strlen(pCam->getFileName())+5];
		strcpy(cachename, pCam->getFileName());
		strcat(cachename, ".LUT");
	}

	// we have to take care here when using a memory manager that can not free memory
	// (usually this lookup table should only be built once - unless we change camera resolution)
	//
	if(undistO2ITable)
		//delete undistO2ITable;
		artkp_Free(undistO2ITable);

	//undistO2ITable = new unsigned int [arImXsize*arImYsize];
	undistO2ITable = artkp_Alloc<unsigned int>(arImXsize*arImYsize);

	if(loadCachedUndist)
	{
		if(FILE* fp = fopen(cachename, "rb"))
		{
			size_t numBytes = fread(undistO2ITable, 1, arImXsize*arImYsize*sizeof(unsigned int), fp);
			fclose(fp);

			if(numBytes == arImXsize*arImYsize*sizeof(unsigned int))
				loaded = true;
		}
	}

	if(!loaded)
	{
		for(x=0; x<arImXsize; x++)
		{
			for(y=0; y<arImYsize; y++)
			{
				arParamObserv2Ideal_std(pCam, (ARFloat)x, (ARFloat)y, &cx, &cy);
				floatToFixed(cx,cy, fixed);
				fixedToFloat(fixed, ox,oy);
				undistO2ITable[x+y*arImXsize] = fixed;
			}
		}

		if(loadCachedUndist)
			if(FILE* fp = fopen(cachename, "wb"))
			{
				fwrite(undistO2ITable, 1, arImXsize*arImYsize*sizeof(unsigned int), fp);
				fclose(fp);
			}
	}

	delete cachename;
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arParamObserv2Ideal(Camera *pCam, ARFloat ox, ARFloat oy, ARFloat *ix, ARFloat *iy)
{
	pCam->observ2Ideal(ox,oy,ix,iy);
	return(0);
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arParamIdeal2Observ(Camera *pCam, ARFloat ix, ARFloat iy, ARFloat *ox, ARFloat *oy)
{
	pCam->ideal2Observ(ix,iy,ox,oy);
	return(0);
}


}  // namespace ARToolKitPlus


