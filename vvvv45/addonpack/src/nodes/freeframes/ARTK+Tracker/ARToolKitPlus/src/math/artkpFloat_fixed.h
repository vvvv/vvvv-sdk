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


#ifndef __ARTKPFLOAT_FIXED_HEADERFILE__
#define __ARTKPFLOAT_FIXED_HEADERFILE__

#include <math.h>
#include <assert.h>

#pragma warning ( push )
#pragma warning ( disable: 4293 )		// fix warning about shifting in conversion constructor

// fixed point implementation of artkpFloat
//
// MATHBASE provides concrete methods for
// internal usage. while artkpFloat_fixed is platform
// independent, MATHBASE is the correct place
// to implement hardware specific optimizations.
// (see artkpFixedBase_generic & artkpFixedBase_gpp)
//

template<typename MATHBASE>
class artkpFloat_fixed
{
public:
	enum {
		PBITS = MATHBASE::PBITS,
		CHECK = MATHBASE::CHECK
	};

	artkpFloat_fixed()									{}
	artkpFloat_fixed(float nV)							{  setFloat(nV);  }
	artkpFloat_fixed(double nV)							{  setDouble(nV);  }
	artkpFloat_fixed(const artkpFloat_fixed& nOther)	{  v = nOther.v;  }

	// conversion constructor. allows assigning values of different precision types
	template <typename OTHER_MATHBASE>
	artkpFloat_fixed(const artkpFloat_fixed<OTHER_MATHBASE>& nOther)
	{
		if(OTHER_MATHBASE::PBITS<PBITS)
		{
			if(CHECK)
			{
				// check if the conversion resulted in an overflow
				__int64 v64 = ((__int64)nOther.getFixed()) << (PBITS-OTHER_MATHBASE::PBITS);
				if(v64 != int(v))
				{
					assert(false && "Fixed-Point to Fixed-Point conversion failed: the target's range was overflowed");
				}
			}
			v = nOther.getFixed() << (PBITS-OTHER_MATHBASE::PBITS);
		}
		else
		{
			v = nOther.getFixed() >> (OTHER_MATHBASE::PBITS-PBITS);
		}
	}

	// custom interface
	//
	bool isFixed()										{  return true;  }

	void setFixed(int nV)								{  v = nV;  }
	void setInt(int nV)									{  v = MATHBASE::fixedFromInt(nV);  }
	void setFloat(float nV)								{  v = MATHBASE::fixedFromFloat(nV);  }
	void setDouble(double nV)							{  v = MATHBASE::fixedFromDouble(nV);  }
	void setArtkpFloat(const artkpFloat_fixed& nOther)	{  v = nOther.v;  }

	int getByte() const									{  return v>255<<(PBITS-8) ? 255 : v>>(PBITS-8);  }
	int getFixed() const        						{  return v;  }
	int getInt() const									{  return v>>PBITS;  }
	float getFloat() const								{  return MATHBASE::floatFromFixed(v);  }
	double getDouble() const							{  return MATHBASE::doubleFromFixed(v);  }

	void inverse(const artkpFloat_fixed& nOther)		{  v = MATHBASE::inverse(nOther.v);  }
	void inverse()										{  inverse(*this);  }

	void inverseSqrt(const artkpFloat_fixed& nOther)	{  v = MATHBASE::inverseSqrt(nOther.v);  }
	void inverseSqrt()									{  inverseSqrt(*this);  }

	void multiplyBy255()								{  int vOld = v;   v<<=8;  v-=vOld;  }


	// some standard math.h routines applied to this class
	//
	friend inline artkpFloat_fixed cos(const artkpFloat_fixed& nV)
		{  return MATHBASE::cos(nV.v);  }


	// overloaded operators
	//
	artkpFloat_fixed& operator=(int nV)							{  setInt(nV);  return *this; }
	artkpFloat_fixed& operator=(float nV)						{  setFloat(nV);  return *this; }
	artkpFloat_fixed& operator=(double nV)						{  setDouble(nV);  return *this; }
	artkpFloat_fixed& operator=(const artkpFloat_fixed& nOther)	{  v = nOther.v;  return *this;  }

	artkpFloat_fixed& operator+=(int nV)	{  v += MATHBASE::fixedFromInt(nV);  return *this;  }
	artkpFloat_fixed& operator-=(int nV)	{  v -= MATHBASE::fixedFromInt(nV);  return *this;  }
	artkpFloat_fixed& operator*=(int nV)	{  v = MATHBASE::multiply(v, MATHBASE::fixedFromInt(nV));  return *this;  }
	artkpFloat_fixed& operator/=(int nV)	{  v = MATHBASE::divide(v, MATHBASE::fixedFromInt(nV));  return *this;  }

	artkpFloat_fixed& operator+=(float nV)	{  v += MATHBASE::fixedFromFloat(nV);  return *this;  }
	artkpFloat_fixed& operator-=(float nV)	{  v -= MATHBASE::fixedFromFloat(nV);  return *this;  }
	artkpFloat_fixed& operator*=(float nV)	{  v = MATHBASE::multiply(v, MATHBASE::fixedFromFloat(nV));  return *this;  }
	artkpFloat_fixed& operator/=(float nV)	{  v = MATHBASE::divide(v, MATHBASE::fixedFromFloat(nV));  return *this;  }

	artkpFloat_fixed& operator+=(double nV)	{  v += MATHBASE::fixedFromDouble(nV);  return *this;  }
	artkpFloat_fixed& operator-=(double nV)	{  v -= MATHBASE::fixedFromDouble(nV);  return *this;  }
	artkpFloat_fixed& operator*=(double nV)	{  v = MATHBASE::multiply(v, fixedFromDouble(nV));  return *this;  }
	artkpFloat_fixed& operator/=(double nV)	{  v = MATHBASE::divide(v, fixedFromDouble(nV));  return *this;  }

	artkpFloat_fixed& operator+=(const artkpFloat_fixed& nOther)	{  v+=nOther.v;  return *this;  }
	artkpFloat_fixed& operator-=(const artkpFloat_fixed& nOther)	{  v-=nOther.v;  return *this;  }
	artkpFloat_fixed& operator*=(const artkpFloat_fixed& nOther)	{  v = MATHBASE::multiply(v, nOther.v);  return *this;  }
	artkpFloat_fixed& operator/=(const artkpFloat_fixed& nOther)	{  v = MATHBASE::divide(v, nOther.v);  return *this;  }

	artkpFloat_fixed& operator>>=(int nBits)	{  v>>=nBits;  return *this;  }
	artkpFloat_fixed& operator<<=(int nBits)	{  v<<=nBits;  return *this;  }

	bool operator==(const artkpFloat_fixed& nOther) const		{  return v==nOther.v;  }
	bool operator!=(const artkpFloat_fixed& nOther) const		{  return v!=nOther.v;  }
	bool operator<=(const artkpFloat_fixed& nOther) const		{  return v<=nOther.v;  }
	bool operator>=(const artkpFloat_fixed& nOther) const		{  return v>=nOther.v;  }
	bool operator<(const artkpFloat_fixed& nOther) const		{  return v<nOther.v;  }
	bool operator>(const artkpFloat_fixed& nOther) const		{  return v>nOther.v;  }

	bool operator==(int nOther) const		{  return v == MATHBASE::fixedFromInt(nOther);  }
	bool operator!=(int nOther) const		{  return v != MATHBASE::fixedFromInt(nOther);  }
	bool operator<=(int nOther) const		{  return v <= MATHBASE::fixedFromInt(nOther);  }
	bool operator>=(int nOther) const		{  return v >= MATHBASE::fixedFromInt(nOther);  }
	bool operator<(int nOther) const		{  return v <  MATHBASE::fixedFromInt(nOther);  }
	bool operator>(int nOther) const		{  return v >  MATHBASE::fixedFromInt(nOther);  }

	bool operator==(float nOther) const		{  return v == MATHBASE::fixedFromFloat(nOther);  }
	bool operator!=(float nOther) const		{  return v != MATHBASE::fixedFromFloat(nOther);  }
	bool operator<=(float nOther) const		{  return v <= MATHBASE::fixedFromFloat(nOther);  }
	bool operator>=(float nOther) const		{  return v >= MATHBASE::fixedFromFloat(nOther);  }
	bool operator<(float nOther) const		{  return v <  MATHBASE::fixedFromFloat(nOther);  }
	bool operator>(float nOther) const		{  return v >  MATHBASE::fixedFromFloat(nOther);  }

	friend artkpFloat_fixed operator+ (const artkpFloat_fixed& left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(left.v+right.v);  }
	friend artkpFloat_fixed operator+ (const artkpFloat_fixed& left, float right)
		{  return artkpFloat_fixed(left.v+MATHBASE::fixedFromFloat(right));  }
	friend artkpFloat_fixed operator+ (float left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(MATHBASE::fixedFromFloat(left)+right.v);  }

	friend artkpFloat_fixed operator- (const artkpFloat_fixed& left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(left.v-right.v);  }
	friend artkpFloat_fixed operator- (const artkpFloat_fixed& left, float right)
		{  return artkpFloat_fixed(left.v-MATHBASE::fixedFromFloat(right));  }
	friend artkpFloat_fixed operator- (float left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(MATHBASE::fixedFromFloat(left)-right.v);  }

	friend artkpFloat_fixed operator* (const artkpFloat_fixed& left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(MATHBASE::multiply(left.v, right.v));  }
	friend artkpFloat_fixed operator* (const artkpFloat_fixed& left, float right)
		{  return artkpFloat_fixed(MATHBASE::multiply(left.v, MATHBASE::fixedFromFloat(right)));  }
	friend artkpFloat_fixed operator* (float left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(MATHBASE::multiply(MATHBASE::fixedFromFloat(left), right.v));  }

	friend artkpFloat_fixed operator/ (const artkpFloat_fixed& left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(MATHBASE::divide(left.v, right.v));  }
	friend artkpFloat_fixed operator/ (const artkpFloat_fixed& left, float right)
		{  return artkpFloat_fixed(MATHBASE::divide(left.v, MATHBASE::fixedFromFloat(right)));  }
	friend artkpFloat_fixed operator/ (float left, const artkpFloat_fixed& right)
		{  return artkpFloat_fixed(MATHBASE::divide(MATHBASE::fixedFromFloat(left), right.v));  }

	artkpFloat_fixed operator-() const
		{  artkpFloat_fixed w;  w.v = -v;  return w;  }

protected:
	artkpFloat_fixed(int nV)		{  v = nV;  }		// constructor with fixed point parameter

	int v;
};


#pragma warning ( pop )


#endif //__ARTKPFLOAT_FIXED_HEADERFILE__
