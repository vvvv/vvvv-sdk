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


#ifndef __ARTKPFLOAT_FLOAT_HEADERFILE__
#define __ARTKPFLOAT_FLOAT_HEADERFILE__

#include <math.h>


// Floating point implementation of artkpFloat
// Although this class will be very slow on any hardware
// without an FPU the advantage is high precision.
//
template <typename BASETYPE_>
class artkpFloat_float
{
public:
	typedef BASETYPE_ BASETYPE;

	artkpFloat_float()									{}
	artkpFloat_float(int nV)							{  setInt(nV);  }
	artkpFloat_float(unsigned int nV)					{  setUnsignedInt(nV);  }
	artkpFloat_float(float nV)							{  setFloat(nV);  }
	artkpFloat_float(double nV)							{  setDouble(nV);  }
	artkpFloat_float(const artkpFloat_float& nOther)	{  v = nOther.v;  }

	// custom interface
	//
	bool isFixed()										{  return false;  };

	void setFixed(int nV)							{  v = nV/65536.0f;  }
	void setInt(int nV)								{  v = (BASETYPE)nV;  }
	void setUnsignedInt(int nV)						{  v = (BASETYPE)nV;  }
	void setFloat(float nV)							{  v = nV;  }
	void setDouble(double nV)						{  v = (BASETYPE)nV;  }
	void setArtkpFloat(const artkpFloat_float& nOther)	{  v = nOther.v;  }

	int getByte() const         {  return (int)(v*255.0f);  }
	int getFixed() const        {  return (int)(v*65536.0f);  }
	int getInt() const			{  return (int)v;  }
	float getFloat() const		{  return (float)v;  }
	double getDouble() const	{  return (double)v;  }

	operator double() const		{  return getDouble();  }

	void inverse(const artkpFloat_float& nOther)		{  v = BASETYPE(1)/nOther.v;  }
	void inverse()										{  v = BASETYPE(1)/v;  }

	void inverseSqrt(const artkpFloat_float& nOther)	{  v = BASETYPE(1)/(BASETYPE)sqrt(nOther.v);  }
	void inverseSqrt()								{  v = BASETYPE(1)/(BASETYPE)sqrt(v);  }

	void multiplyBy255()							{  v *= 255.0f;  }


	// some standard math.h routines applied to this class
	//
	friend inline const artkpFloat_float sqrt(const artkpFloat_float& nV)
		{  return ::sqrt(nV.v);  }
	friend inline const artkpFloat_float cos(const artkpFloat_float& nV)
		{  return ::cos(nV.v);  }
	friend inline const artkpFloat_float sin(const artkpFloat_float& nV)
		{  return ::sin(nV.v);  }
	friend inline const artkpFloat_float acos(const artkpFloat_float& nV)
		{  return ::acos(nV.v);  }
	friend inline const artkpFloat_float asin(const artkpFloat_float& nV)
		{  return ::asin(nV.v);  }
	friend inline const artkpFloat_float atan2(const artkpFloat_float& nVa, const artkpFloat_float& nVb)
		{  return ::atan2(nVa.v,nVb.v);  }
	friend inline const artkpFloat_float fabs(const artkpFloat_float& nV)
		{  return ::fabs(nV.v);  }
	friend inline const artkpFloat_float pow(const artkpFloat_float& nVa, const artkpFloat_float& nVb)
		{  return ::pow(nVa.v,nVb.v);  }
	friend inline const artkpFloat_float ceil(const artkpFloat_float& nV)
		{  return ::ceil(nV.v);  }

	// overloaded operators
	//
	artkpFloat_float& operator=(unsigned int nV)				{  setInt(nV);  return *this; }
	artkpFloat_float& operator=(int nV)							{  setInt(nV);  return *this; }
	artkpFloat_float& operator=(float nV)						{  setFloat(nV);  return *this; }
	artkpFloat_float& operator=(double nV)						{  setDouble(nV);  return *this; }
	artkpFloat_float& operator=(const artkpFloat_float& nOther)	{  v = nOther.v;  return *this;  }

	artkpFloat_float operator-() const  {  artkpFloat_float w;  w.v = -v;  return w;  }

	artkpFloat_float& operator+=(int nV)	{  v+=(BASETYPE)nV;  return *this;  }
	artkpFloat_float& operator-=(int nV)	{  v-=(BASETYPE)nV;  return *this;  }
	artkpFloat_float& operator*=(int nV)	{  v*=(BASETYPE)nV;  return *this;  }
	artkpFloat_float& operator/=(int nV)	{  v/=(BASETYPE)nV;  return *this;  }

	artkpFloat_float& operator+=(float nV)	{  v+=nV;  return *this;  }
	artkpFloat_float& operator-=(float nV)	{  v-=nV;  return *this;  }
	artkpFloat_float& operator*=(float nV)	{  v*=nV;  return *this;  }
	artkpFloat_float& operator/=(float nV)	{  v/=nV;  return *this;  }

	artkpFloat_float& operator+=(double nV)	{  v+=(BASETYPE)nV;  return *this;  }
	artkpFloat_float& operator-=(double nV)	{  v-=(BASETYPE)nV;  return *this;  }
	artkpFloat_float& operator*=(double nV)	{  v*=(BASETYPE)nV;  return *this;  }
	artkpFloat_float& operator/=(double nV)	{  v/=(BASETYPE)nV;  return *this;  }

	artkpFloat_float& operator+=(const artkpFloat_float& nOther)	{  v+=nOther.v;  return *this;  }
	artkpFloat_float& operator-=(const artkpFloat_float& nOther)	{  v-=nOther.v;  return *this;  }
	artkpFloat_float& operator*=(const artkpFloat_float& nOther)	{  v*=nOther.v;  return *this;  }
	artkpFloat_float& operator/=(const artkpFloat_float& nOther)	{  v/=nOther.v;  return *this;  }

	artkpFloat_float& operator>>=(int nBits)	{  int tmp=1<<nBits;	v/=tmp;  return *this;  }
	artkpFloat_float& operator<<=(int nBits)	{  int tmp=1<<nBits;	v*=tmp;  return *this;  }

	bool operator==(const artkpFloat_float& nOther) const		{  return v==nOther.v;  }
	bool operator!=(const artkpFloat_float& nOther) const		{  return v!=nOther.v;  }
	bool operator<=(const artkpFloat_float& nOther) const		{  return v<=nOther.v;  }
	bool operator>=(const artkpFloat_float& nOther) const		{  return v>=nOther.v;  }
	bool operator<(const artkpFloat_float& nOther) const		{  return v<nOther.v;  }
	bool operator>(const artkpFloat_float& nOther) const		{  return v>nOther.v;  }

	bool operator==(int nOther) const		{  return v==(BASETYPE)nOther;  }
	bool operator!=(int nOther) const		{  return v!=(BASETYPE)nOther;  }
	bool operator<=(int nOther) const		{  return v<=(BASETYPE)nOther;  }
	bool operator>=(int nOther) const		{  return v>=(BASETYPE)nOther;  }
	bool operator<(int nOther) const		{  return v< (BASETYPE)nOther;  }
	bool operator>(int nOther) const		{  return v> (BASETYPE)nOther;  }

	bool operator==(float nOther) const		{  return v==nOther;  }
	bool operator!=(float nOther) const		{  return v!=nOther;  }
	bool operator<=(float nOther) const		{  return v<=nOther;  }
	bool operator>=(float nOther) const		{  return v>=nOther;  }
	bool operator<(float nOther) const		{  return v<nOther;  }
	bool operator>(float nOther) const		{  return v>nOther;  }

	friend inline const artkpFloat_float operator+(const artkpFloat_float& left, const artkpFloat_float& right)
		{	return left.v+right.v;	}
	friend inline const artkpFloat_float operator+(const artkpFloat_float& left, float right)
		{	return left.v+right;	}
	friend inline const artkpFloat_float operator+(float left, const artkpFloat_float& right)
		{	return left+right.v;	}

	friend inline const artkpFloat_float operator-(const artkpFloat_float& left, const artkpFloat_float& right)
	{	return left.v-right.v;	}
	friend inline const artkpFloat_float operator-(const artkpFloat_float& left, float right)
	{	return left.v-right;	}
	friend inline const artkpFloat_float operator-(float left, const artkpFloat_float& right)
	{	return left-right.v;	}

	friend inline const artkpFloat_float operator*(const artkpFloat_float& left, const artkpFloat_float& right)
	{	return left.v*right.v;	}
	friend inline const artkpFloat_float operator*(const artkpFloat_float& left, float right)
	{	return left.v*right;	}
	friend inline const artkpFloat_float operator*(float left, const artkpFloat_float& right)
	{	return left*right.v;	}

	friend inline const artkpFloat_float operator/(const artkpFloat_float& left, const artkpFloat_float& right)
	{	return left.v/right.v;	}
	friend inline const artkpFloat_float operator/(const artkpFloat_float& left, float right)
	{	return left.v/right;	}
	friend inline const artkpFloat_float operator/(float left, const artkpFloat_float& right)
	{	return left/right.v;	}

protected:
	BASETYPE v;
};


/*
// binary operator +
//
template <typename BT>
inline const artkpFloat_float<typename BT>
operator+(const artkpFloat_float<typename BT>& left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left.v+right.v);
}

template <typename BT>
inline const artkpFloat_float<typename BT>
operator+(const artkpFloat_float<typename BT>& left, float right)
{
	return artkpFloat_float<BT>(left.v+right);
}

template <typename BT>
inline const artkpFloat_float<typename BT>
operator+(float left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left+right.v);
}



// binary operator -
//
template <typename BT>
inline const artkpFloat_float<typename BT>
operator-(const artkpFloat_float<typename BT>& left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left.v-right.v);
}

template <typename BT>
inline const artkpFloat_float<typename BT>
operator-(const artkpFloat_float<typename BT>& left, float right)
{
	return artkpFloat_float<BT>(left.v-right);
}

template <typename BT>
inline const artkpFloat_float<typename BT>
operator-(float left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left-right.v);
}


// binary operator *
//
template <typename BT>
inline const artkpFloat_float<typename BT>
operator*(const artkpFloat_float<typename BT>& left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left.v*right.v);
}

template <typename BT>
inline const artkpFloat_float<typename BT>
operator*(const artkpFloat_float<typename BT>& left, float right)
{
	return artkpFloat_float<BT>(left.v*right);
}

template <typename BT>
inline const artkpFloat_float<typename BT>
operator*(float left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left*right.v);
}


// binary operator /
//
template <typename BT>
inline const artkpFloat_float<typename BT>
operator/(const artkpFloat_float<typename BT>& left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left.v/right.v);
}


template <typename BT>
inline const artkpFloat_float<typename BT>
operator/(const artkpFloat_float<typename BT>& left, float right)
{
	return artkpFloat_float<BT>(left.v/right);
}

template <typename BT>
inline const artkpFloat_float<typename BT>
operator/(float left, const artkpFloat_float<typename BT>& right)
{
	return artkpFloat_float<BT>(left/right.v);
}
*/


/*
// math.h methods
//
template <typename BT>
inline const artkpFloat_float<typename BT>
cos(const artkpFloat_float<typename BT>& nV)
{
	return artkpFloat_float<BT>(::cos(nV.v));
}


template <typename BT>
inline const artkpFloat_float<typename BT>
sin(const artkpFloat_float<typename BT>& nV)
{
	return artkpFloat_float<BT>(::sin(nV.v));
}

template <typename BT>
inline const artkpFloat_float<typename BT>
acos(const artkpFloat_float<typename BT>& nV)
{
	return artkpFloat_float<BT>(::acos(nV.v));
}

template <typename BT>
inline const artkpFloat_float<typename BT>
asin(const artkpFloat_float<typename BT>& nV)
{
	return artkpFloat_float<BT>(::asin(nV.v));
}

template <typename BT>
inline const artkpFloat_float<typename BT>
atan2(const artkpFloat_float<typename BT>& nVa, const artkpFloat_float<typename BT>& nVb)
{
	return artkpFloat_float<BT>(::atan2(nVa.v, nVb.v));
}

template <typename BT>
inline const artkpFloat_float<typename BT>
fabs(const artkpFloat_float<typename BT>& nV)
{
	return artkpFloat_float<BT>(::fabs(nV.v));
}

template <typename BT>
inline const artkpFloat_float<typename BT>
pow(const artkpFloat_float<typename BT>& nVa, const artkpFloat_float<typename BT>& nVb)
{
	return artkpFloat_float<BT>(::pow(nVa.v, nVb.v));
}

template <typename BT>
inline const artkpFloat_float<typename BT>
sqrt(const artkpFloat_float<typename BT>& nV)
{
	return artkpFloat_float<BT>(::sqrt(nV.v));
}

template <typename BT>
inline const artkpFloat_float<typename BT>
ceil(const artkpFloat_float<typename BT>& nV)
{
	return artkpFloat_float<BT>(::ceil(nV.v));
}
*/

#endif //__ARTKPFLOAT_FLOAT_HEADERFILE__
