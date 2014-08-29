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
 * $Id$
 * @file
 * ======================================================================== */


#ifndef __ARTKPFIXEDBASE_GPP_HEADERFILE__
#define __ARTKPFIXEDBASE_GPP_HEADERFILE__


#include <gpp.h>
#ifdef DEBUG
  #pragma comment( lib, "gpp_WMMX40_d.lib" )
#else
  #pragma comment( lib, "gpp_WMMX40_r.lib" )
#endif //DEBUG


template <int PBITS_>
class artkpFixedBase_gpp
{
public:
	enum {
		PBITS = PBITS_,
		CHECK = 0
	};

	static float floatFromFixed(int nFixed)
	{
		return nFixed/(float)(1 << PBITS);
	}

	static double doubleFromFixed(int nFixed)
	{
		return nFixed/(double)(1 << PBITS);
	}

	static int fixedFromInt(int nV)
	{
		return nV<<PBITS;
	}

	static int fixedFromFloat(float nV)
	{
		return (int)(nV *  (float)(1 << PBITS) + 0.5f);
	}

	static int fixedFromDouble(double nV)
	{
		return (int)(nV * (double)(1 << PBITS) + 0.5f);
	}

	static int inverse(int nFixed)
	{
		int ret=0;
		gppInvHP_n_32s(nFixed, &ret, PBITS);
		return ret;
	}

	static int multiply(int nLeftFixed, int nRightFixed)
	{
		int ret;
		gppMul_n_32s(nLeftFixed, nRightFixed, &ret, PBITS);
		return ret;
	}

	static int divide(int nLeftFixed, int nRightFixed)
	{
		int ret;
		gppDiv_n_32s(nLeftFixed, nRightFixed, &ret, PBITS);
		return ret;
	}

	static int cos(int nFixed)
	{
		int ret;
		gppCosHP_n_32s(nFixed, &ret, PBITS);
		return ret;
	}

	static int sin(int nFixed)
	{
		int ret;
		gppSinHP_n_32s(nFixed, &ret, PBITS);
		return ret;
	}

	static int fabs(int nFixed)
	{
		return nFixed<0 ? -nFixed : nFixed;
	}

	static int sqrt(int nFixed)
	{
		unsigned int ret;
		gppSqrtHP_n_32s(nFixed, &ret, PBITS);
		return (int)ret;
	}

	static int inverseSqrt(int nFixed)
	{
		unsigned int ret;
		gppInvSqrtHP_n_32s(nFixed, &ret, PBITS);
		return (int)ret;
	}

	static int ceil(int nFixed)
	{
		int ret = (nFixed>>PBITS)<<PBITS;

		if(nFixed>=0 && ret<nFixed)
			ret += fixedFromInt(1);

		return ret;
	}
};


#endif //__ARTKPFIXEDBASE_GPP_HEADERFILE__
