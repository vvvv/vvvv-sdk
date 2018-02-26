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


#ifndef __FIXEDPOINT_HEADERFILE__
#define __FIXEDPOINT_HEADERFILE__


namespace ARToolKitPlus {


#define FIXED_Float_To_Fixed_n(x, n)       ((I32)(x * (float)(1 << n) + 0.5f))
#define FIXED_Fixed_n_To_Float(x, n)	((float)x / (float)(1 << n))	



#ifdef _USEGPP_
#  include <gpp.h>
#  define _USE_GPP_ARITHMETIC_
#  define _USE_GPP_TRIGONOMETRIC_
#  define _USE_GPP_VECTOR_
#  define _FIXEDPOINT_MATH_ACTIVATED_

// autolink against Intel GPP
#  ifdef DEBUG
#    pragma comment( lib, "gpp_WMMX40_d.lib" )
#  else
#    pragma comment( lib, "gpp_WMMX40_r.lib" )
#  endif
#endif


#ifdef _USEFIXED_
#  if defined(_WIN32) || defined(WIN32)
	typedef unsigned __int64	U64;
	typedef __int64				I64;
	typedef unsigned int U32;
	typedef int I32;
#  endif
#  define _USE_GENERIC_ARITHMETIC_
#  define _USE_GENERIC_TRIGONOMETRIC_
#  define _USE_GENERIC_VECTOR_
#  define _FIXEDPOINT_MATH_ACTIVATED_
#endif


#ifdef _USE_GPP_ARITHMETIC_

#define FIXED_MUL2(a,b, res, bits)  \
	gppMul_n_32s((a), (b), &res, bits);

#define FIXED_MUL3(a,b,c, res, bits)  \
	gppMul_n_32s((a), (b), &_tmp1, bits);  \
	gppMul_n_32s(_tmp1, (c), &res, bits);

#define FIXED_MUL4(a,b,c,d, res, bits)  \
	gppMul_n_32s((a), (b), &_tmp1, bits);  \
	gppMul_n_32s(_tmp1, (c), &_tmp2, bits);  \
	gppMul_n_32s(_tmp2, (d), &res, bits);

#define FIXED_DIV2(a,b, res, bits)  \
	gppDiv_n_32s((a), (b), &res, bits);

#endif //_USE_GPP_ARITHMETIC_


#ifdef _USE_GPP_TRIGONOMETRIC_

#define FIXED_SIN(theta, sin_theta, n) \
	gppSinHP_n_32s((theta), sin_theta, n);

#define FIXED_COS(theta, cos_theta, n) \
	gppCosHP_n_32s((theta), cos_theta, n);

#define FIXED_SINCOS(theta, sin_theta, cos_theta, n) \
	gppSinCosHP_n_32s((theta), sin_theta, cos_theta, n);

#endif // _USE_GPP_TRIGONOMETRIC_


#ifdef _USE_GPP_VECTOR_

typedef GPP_VEC3D FIXED_VEC3D;

#define FIXED_VEC3_DOT(vec1, vec2, res, n) \
	gppVec3DDot_n_32s((vec1), (vec2), (res), (n));

#define FIXED_VEC3_SUB(vec1, vec2, res) \
	gppVec3DSub_n_32s((vec1), (vec2), (res));

#define FIXED_VEC3_LENGTH_SQ(vec, res, n) \
	gppVec3DLengthSq_n_32s((vec), (res), (n));

#endif // _USE_GPP_VECTOR_



#ifdef _USE_GENERIC_ARITHMETIC_

#  define FIXED_MUL2(a,b, res, bits)  \
	res = ((I32) (((I64)a * (I64)b)  >> bits));

#  define FIXED_MUL3(a,b,c, res, bits)  \
	FIXED_MUL2((a), (b), (res), bits);  \
	FIXED_MUL2((res), (c), (res), bits);

#  define FIXED_MUL4(a,b,c,d, res, bits)  \
	FIXED_MUL2((a), (b), (res), bits);  \
	FIXED_MUL2((res), (c), (res), bits);  \
	FIXED_MUL2((res), (d), (res), bits);

#  define FIXED_DIV2(a,b, res, bits)  \
	res = (I32) ((((I64)a)<<bits)/(I64)b);

#endif //_USE_GENERIC_ARITHMETIC_


#ifdef _USE_GENERIC_TRIGONOMETRIC_

void Fixed28_Init();
void Fixed28_Deinit();

inline void Fixed28_SinCos(I32 phi, I32 &sin, I32 &cos);

#define FIXED_SINCOS(theta, sin_theta, cos_theta, n) \
	Fixed28_SinCos((theta), *(sin_theta), *(cos_theta));

#endif //_USE_GENERIC_TRIGONOMETRIC_


#ifdef _USE_GENERIC_VECTOR_

typedef struct{
	I32 	x, y, z;
} FIXED_VEC3D;

inline void FIXED_VEC3_DOT(FIXED_VEC3D* vec1, FIXED_VEC3D* vec2, I32* res, I32 n)
{
	I32 x,y,z;
	FIXED_MUL2(vec1->x, vec2->x, x, n);
	FIXED_MUL2(vec1->y, vec2->y, y, n);
	FIXED_MUL2(vec1->z, vec2->z, z, n);

	*res = x+y+z;
}

inline void FIXED_VEC3_SUB(FIXED_VEC3D* vec1, FIXED_VEC3D* vec2, FIXED_VEC3D* res)
{
	res->x = vec1->x - vec2->x;
	res->y = vec1->y - vec2->y;
	res->z = vec1->z - vec2->z;
}

inline void FIXED_VEC3_LENGTH_SQ(FIXED_VEC3D* vec, U32* res, I32 n)
{
	I32 x,y,z;
	FIXED_MUL2(vec->x, vec->x, x, n);
	FIXED_MUL2(vec->y, vec->y, y, n);
	FIXED_MUL2(vec->z, vec->z, z, n);

	*res = x+y+z;
}

#endif //_USE_GENERIC_VECTOR_


}  // namespace ARToolKitPlus


#endif //__FIXEDPOINT_HEADERFILE__
