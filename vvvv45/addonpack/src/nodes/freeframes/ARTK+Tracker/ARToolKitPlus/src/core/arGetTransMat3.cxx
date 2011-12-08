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
 * $Id: arGetTransMat3.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */

#define CHECK_CALC 0

//#include <stdlib.h>
//#include <math.h>
//#include <ARToolKitPlus/Tracker.h>
//#include <ARToolKitPlus/matrix.h>


namespace ARToolKitPlus {


#define MD_PI         3.14159265358979323846

static int  check_rotation( ARFloat rot[2][3] );
static int  check_dir( ARFloat dir[3], ARFloat st[2], ARFloat ed[2],
                       ARFloat cpara[3][4] );

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetAngle( ARFloat rot[3][3], ARFloat *wa, ARFloat *wb, ARFloat *wc )
{
	PROFILE_BEGINSEC(profiler, GETANGLE)
	
	ARFloat      a, b, c;
    ARFloat      sina, cosa, sinb, cosb, sinc, cosc;
#if CHECK_CALC
ARFloat   w[3];
int      i;
for(i=0;i<3;i++) w[i] = rot[i][0];
for(i=0;i<3;i++) rot[i][0] = rot[i][1];
for(i=0;i<3;i++) rot[i][1] = rot[i][2];
for(i=0;i<3;i++) rot[i][2] = w[i];
#endif

    if( rot[2][2] > 1.0 ) {
        /* printf("cos(beta) = %f\n", rot[2][2]); */
        rot[2][2] = 1.0;
    }
    else if( rot[2][2] < -1.0 ) {
        /* printf("cos(beta) = %f\n", rot[2][2]); */
        rot[2][2] = -1.0;
    }
    cosb = rot[2][2];
    b = (ARFloat)acos( cosb );
    sinb = (ARFloat)sin( b );
    if( b >= 0.000001 || b <= -0.000001) {
        cosa = rot[0][2] / sinb;
        sina = rot[1][2] / sinb;
        if( cosa > 1.0 ) {
            /* printf("cos(alph) = %f\n", cosa); */
            cosa = 1.0;
            sina = 0.0;
        }
        if( cosa < -1.0 ) {
            /* printf("cos(alph) = %f\n", cosa); */
            cosa = -1.0;
            sina =  0.0;
        }
        if( sina > 1.0 ) {
            /* printf("sin(alph) = %f\n", sina); */
            sina = 1.0;
            cosa = 0.0;
        }
        if( sina < -1.0 ) {
            /* printf("sin(alph) = %f\n", sina); */
            sina = -1.0;
            cosa =  0.0;
        }
        a = (ARFloat)acos( cosa );
        if( sina < 0 ) a = -a;

        sinc =  (rot[2][1]*rot[0][2]-rot[2][0]*rot[1][2])
              / (rot[0][2]*rot[0][2]+rot[1][2]*rot[1][2]);
        cosc =  -(rot[0][2]*rot[2][0]+rot[1][2]*rot[2][1])
               / (rot[0][2]*rot[0][2]+rot[1][2]*rot[1][2]);
        if( cosc > 1.0 ) {
            /* printf("cos(r) = %f\n", cosc); */
            cosc = 1.0;
            sinc = 0.0;
        }
        if( cosc < -1.0 ) {
            /* printf("cos(r) = %f\n", cosc); */
            cosc = -1.0;
            sinc =  0.0;
        }
        if( sinc > 1.0 ) {
            /* printf("sin(r) = %f\n", sinc); */
            sinc = 1.0;
            cosc = 0.0;
        }
        if( sinc < -1.0 ) {
            /* printf("sin(r) = %f\n", sinc); */
            sinc = -1.0;
            cosc =  0.0;
        }
        c = (ARFloat)acos( cosc );
        if( sinc < 0 ) c = -c;
    }
    else {
        a = b = 0.0;
        cosa = cosb = 1.0;
        sina = sinb = 0.0;
        cosc = rot[0][0];
        sinc = rot[1][0];
        if( cosc > 1.0 ) {
            /* printf("cos(r) = %f\n", cosc); */
            cosc = 1.0;
            sinc = 0.0;
        }
        if( cosc < -1.0 ) {
            /* printf("cos(r) = %f\n", cosc); */
            cosc = -1.0;
            sinc =  0.0;
        }
        if( sinc > 1.0 ) {
            /* printf("sin(r) = %f\n", sinc); */
            sinc = 1.0;
            cosc = 0.0;
        }
        if( sinc < -1.0 ) {
            /* printf("sin(r) = %f\n", sinc); */
            sinc = -1.0;
            cosc =  0.0;
        }
        c = (ARFloat)acos( cosc );
        if( sinc < 0 ) c = -c;
    }

    *wa = a;
    *wb = b;
    *wc = c;

	PROFILE_ENDSEC(profiler, GETANGLE)
    return 0;
}


#ifndef _FIXEDPOINT_MATH_ACTIVATED_


// Non-FixedPoint version of arGetRot
//
AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetRot( ARFloat a, ARFloat b, ARFloat c, ARFloat rot[3][3] )
{
	PROFILE_BEGINSEC(profiler, GETROT)

    ARFloat   sina, sinb, sinc;
    ARFloat   cosa, cosb, cosc;
#if CHECK_CALC
    ARFloat   w[3];
    int      i;
#endif

    sina = (ARFloat)sin(a); cosa = (ARFloat)cos(a);
    sinb = (ARFloat)sin(b); cosb = (ARFloat)cos(b);
    sinc = (ARFloat)sin(c); cosc = (ARFloat)cos(c);
    rot[0][0] = cosa*cosa*cosb*cosc+sina*sina*cosc+sina*cosa*cosb*sinc-sina*cosa*sinc;
    rot[0][1] = -cosa*cosa*cosb*sinc-sina*sina*sinc+sina*cosa*cosb*cosc-sina*cosa*cosc;
    rot[0][2] = cosa*sinb;
    rot[1][0] = sina*cosa*cosb*cosc-sina*cosa*cosc+sina*sina*cosb*sinc+cosa*cosa*sinc;
    rot[1][1] = -sina*cosa*cosb*sinc+sina*cosa*sinc+sina*sina*cosb*cosc+cosa*cosa*cosc;
    rot[1][2] = sina*sinb;
    rot[2][0] = -cosa*sinb*cosc-sina*sinb*sinc;
    rot[2][1] = cosa*sinb*sinc-sina*sinb*cosc;
    rot[2][2] = cosb;

#if CHECK_CALC
    for(i=0;i<3;i++) w[i] = rot[i][2];
    for(i=0;i<3;i++) rot[i][2] = rot[i][1];
    for(i=0;i<3;i++) rot[i][1] = rot[i][0];
    for(i=0;i<3;i++) rot[i][0] = w[i];
#endif

	PROFILE_ENDSEC(profiler, GETROT)
    return 0;
}

#endif //_FIXEDPOINT_MATH_ACTIVATED_


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetNewMatrix(ARFloat a, ARFloat b, ARFloat c,
						ARFloat trans[3], ARFloat trans2[3][4],
						ARFloat cpara[3][4], ARFloat ret[3][4])
{
    ARFloat   cpara2[3][4];
    ARFloat   rot[3][3];
    int      i, j;

	PROFILE_BEGINSEC(profiler, GETNEWMATRIX)

    arGetRot( a, b, c, rot );

    if( trans2 != NULL ) {
        for( j = 0; j < 3; j++ ) {
            for( i = 0; i < 4; i++ ) {
                cpara2[j][i] = cpara[j][0] * trans2[0][i]
                             + cpara[j][1] * trans2[1][i]
                             + cpara[j][2] * trans2[2][i];
            }
        }
    }
    else {
        for( j = 0; j < 3; j++ ) {
            for( i = 0; i < 4; i++ ) {
                cpara2[j][i] = cpara[j][i];
            }
        }
    }

    for( j = 0; j < 3; j++ ) {
        for( i = 0; i < 3; i++ ) {
            ret[j][i] = cpara2[j][0] * rot[0][i]
                      + cpara2[j][1] * rot[1][i]
                      + cpara2[j][2] * rot[2][i];
        }
        ret[j][3] = cpara2[j][0] * trans[0]
                  + cpara2[j][1] * trans[1]
                  + cpara2[j][2] * trans[2]
                  + cpara2[j][3];
    }

	PROFILE_ENDSEC(profiler, GETNEWMATRIX)
    return(0);
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetInitRot( ARMarkerInfo *marker_info, ARFloat cpara[3][4], ARFloat rot[3][3] )
{
    ARFloat  wdir[3][3];
    ARFloat  w, w1, w2, w3;
    int     dir;
    int     j;

	PROFILE_BEGINSEC(profiler, GETINITROT)

    dir = marker_info->dir;

    for( j = 0; j < 2; j++ ) {
        w1 = marker_info->line[(4-dir+j)%4][0] * marker_info->line[(6-dir+j)%4][1]
           - marker_info->line[(6-dir+j)%4][0] * marker_info->line[(4-dir+j)%4][1];
        w2 = marker_info->line[(4-dir+j)%4][1] * marker_info->line[(6-dir+j)%4][2]
           - marker_info->line[(6-dir+j)%4][1] * marker_info->line[(4-dir+j)%4][2];
        w3 = marker_info->line[(4-dir+j)%4][2] * marker_info->line[(6-dir+j)%4][0]
           - marker_info->line[(6-dir+j)%4][2] * marker_info->line[(4-dir+j)%4][0];

        wdir[j][0] =  w1*(cpara[0][1]*cpara[1][2]-cpara[0][2]*cpara[1][1])
                   +  w2*cpara[1][1]
                   -  w3*cpara[0][1];
        wdir[j][1] = -w1*cpara[0][0]*cpara[1][2]
                   +  w3*cpara[0][0];
        wdir[j][2] =  w1*cpara[0][0]*cpara[1][1];
        w = (ARFloat)sqrt( wdir[j][0]*wdir[j][0]
                         + wdir[j][1]*wdir[j][1]
                         + wdir[j][2]*wdir[j][2] );
        wdir[j][0] /= w;
        wdir[j][1] /= w;
        wdir[j][2] /= w;
    }

    if( check_dir(wdir[0], marker_info->vertex[(4-dir)%4],
                  marker_info->vertex[(5-dir)%4], cpara) < 0 )
	{
		PROFILE_ENDSEC(profiler, GETINITROT)
		return -1;
	}

	if( check_dir(wdir[1], marker_info->vertex[(7-dir)%4],
                  marker_info->vertex[(4-dir)%4], cpara) < 0 )
	{
		PROFILE_ENDSEC(profiler, GETINITROT)
		return -1;
	}

    if( check_rotation(wdir) < 0 )
	{
		PROFILE_ENDSEC(profiler, GETINITROT)
		return -1;
	}

    wdir[2][0] = wdir[0][1]*wdir[1][2] - wdir[0][2]*wdir[1][1];
    wdir[2][1] = wdir[0][2]*wdir[1][0] - wdir[0][0]*wdir[1][2];
    wdir[2][2] = wdir[0][0]*wdir[1][1] - wdir[0][1]*wdir[1][0];
    w = (ARFloat)sqrt( wdir[2][0]*wdir[2][0]
                     + wdir[2][1]*wdir[2][1]
                     + wdir[2][2]*wdir[2][2] );
    wdir[2][0] /= w;
    wdir[2][1] /= w;
    wdir[2][2] /= w;
/*
    if( wdir[2][2] < 0 ) {
        wdir[2][0] /= -w;
        wdir[2][1] /= -w;
        wdir[2][2] /= -w;
    }
    else {
        wdir[2][0] /= w;
        wdir[2][1] /= w;
        wdir[2][2] /= w;
    }
*/

    rot[0][0] = wdir[0][0];
    rot[1][0] = wdir[0][1];
    rot[2][0] = wdir[0][2];
    rot[0][1] = wdir[1][0];
    rot[1][1] = wdir[1][1];
    rot[2][1] = wdir[1][2];
    rot[0][2] = wdir[2][0];
    rot[1][2] = wdir[2][1];
    rot[2][2] = wdir[2][2];

	PROFILE_ENDSEC(profiler, GETINITROT)
    return 0;
}


//////////////////////////////////////////////////////////////
//
//             POCKETPC specific code starts here
//


#ifdef _FIXEDPOINT_MATH_ACTIVATED_

#define BITS 28

// FixedPoint version of arGetRot - n is 28
//
// TODO: we can further optimize this routine
//       by storing products
//

#pragma message(">>> Using arGetRot_28()")
int arGetRot_28(I32 _a, I32 _b, I32 _c, FIXED_VEC3D _rot[3])
{
	I32 _sina, _sinb, _sinc, _cosa, _cosb, _cosc;
	I32 _tmp1=0, _tmp2=0;
	I32 _res1,_res2,_res3,_res4,  _res5,_res6;

	FIXED_SINCOS(_a, &_sina, &_cosa, BITS);
	FIXED_SINCOS(_b, &_sinb, &_cosb, BITS);
	FIXED_SINCOS(_c, &_sinc, &_cosc, BITS);

	//FIXED_SIN(_a, &_sina, BITS);
	//FIXED_SIN(_b, &_sinb, BITS);
	//FIXED_SIN(_c, &_sinc, BITS);
	//FIXED_COS(_a, &_cosa, BITS);
	//FIXED_COS(_b, &_cosb, BITS);
	//FIXED_COS(_c, &_cosc, BITS);

	I32 _cosa2, _sina2, _sina_cosa, _cosb_cosc,
		_sina_sinb, _cosa_sinb, _cosb_sinc;

	// some intermediate results that can be used more than once later
	//
	FIXED_MUL2(_cosa,_cosa, _cosa2, BITS);
	FIXED_MUL2(_sina,_sina, _sina2, BITS);
	FIXED_MUL2(_sina,_cosa, _sina_cosa, BITS);
	FIXED_MUL2(_cosb,_cosc, _cosb_cosc, BITS);
	FIXED_MUL2(_sina,_sinb, _sina_sinb, BITS);
	FIXED_MUL2(_cosa,_sinb, _cosa_sinb, BITS);
	FIXED_MUL2(_cosb,_sinc, _cosb_sinc, BITS);



	// rot[0][0]
	FIXED_MUL2(_cosa2,	_cosb_cosc,	_res1,BITS);
	FIXED_MUL2(_sina2,	_cosc, _res2,BITS);
	FIXED_MUL2(_sina_cosa,	_cosb_sinc, _res3,BITS);
	FIXED_MUL2(_sina_cosa,	_sinc, _res5,BITS);					//store for later use
	_rot[0].x = _res1+_res2+_res3-_res5;


	// rot[0][1]
	FIXED_MUL2(_cosa2,	_cosb_sinc, _res1,BITS);
	FIXED_MUL2(_sina2,	_sinc, _res2,BITS);
	FIXED_MUL2(_sina_cosa,	_cosb_cosc, _res3,BITS);
	FIXED_MUL2(_sina_cosa,	_cosc, _res6,BITS);					//store for later use
	_rot[0].y = -_res1-_res2+_res3-_res6;


	// rot[0][2]
	_rot[0].z = _cosa_sinb;


	// rot[1][0]
	FIXED_MUL2(_sina_cosa, _cosb_cosc, _res1,BITS);
	FIXED_MUL2(_sina2, _cosb_sinc, _res3,BITS);
	FIXED_MUL2(_cosa2, _sinc, _res4,BITS);
	_rot[1].x = _res1-_res6+_res3+_res4;


	// rot[1][1]
	FIXED_MUL2(_sina_cosa,	_cosb_sinc, _res1,BITS);
	FIXED_MUL2(_sina2,	_cosb_cosc, _res3,BITS);
	FIXED_MUL2(_cosa2,	_cosc, _res4,BITS);
	_rot[1].y = -_res1+_res5+_res3+_res4;


	// rot[1][2]
	_rot[1].z = _sina_sinb;


	// rot[2][0]
	FIXED_MUL2(_cosa_sinb,	_cosc, _res1,BITS);
	FIXED_MUL2(_sina_sinb,	_sinc, _res2,BITS);
	_rot[2].x = -_res1-_res2;


	// rot[2][1]
	FIXED_MUL2(_cosa_sinb,	_sinc, _res1,BITS);
	FIXED_MUL2(_sina_sinb,	_cosc, _res2,BITS);
	_rot[2].y = _res1-_res2;


	// rot[2][2]
	_rot[2].z = _cosb;


    return 0;
}


int arGetRot_28_old(I32 _a, I32 _b, I32 _c, FIXED_VEC3D _rot[3])
{
	I32 _sina, _sinb, _sinc, _cosa, _cosb, _cosc;
	I32 _tmp1=0, _tmp2=0;
	I32 _res1,_res2,_res3,_res4;

	FIXED_SINCOS(_a, &_sina, &_cosa, BITS);
	FIXED_SINCOS(_b, &_sinb, &_cosb, BITS);
	FIXED_SINCOS(_c, &_sinc, &_cosc, BITS);

	//FIXED_SIN(_a, &_sina, BITS);
	//FIXED_SIN(_b, &_sinb, BITS);
	//FIXED_SIN(_c, &_sinc, BITS);
	//FIXED_COS(_a, &_cosa, BITS);
	//FIXED_COS(_b, &_cosb, BITS);
	//FIXED_COS(_c, &_cosc, BITS);

	// rot[0][0]
	FIXED_MUL4(_cosa,_cosa,_cosb,_cosc, _res1, BITS)			// cosa*cosa*cosb*cosc
	FIXED_MUL3(_sina,_sina,_cosc, _res2, BITS)					// sina*sina*cosc
	FIXED_MUL4(_sina,_cosa,_cosb,_sinc, _res3, BITS)			// sina*cosa*cosb*sinc
	FIXED_MUL3(_sina,_cosa,_sinc, _res4, BITS)					// sina*cosa*sinc
	_rot[0].x = _res1+_res2+_res3-_res4;

	// rot[0][1]
	FIXED_MUL4(_cosa,_cosa,_cosb,_sinc, _res1, BITS)			// cosa*cosa*cosb*sinc
	FIXED_MUL3(_sina,_sina,_sinc, _res2, BITS)					// sina*sina*sinc
	FIXED_MUL4(_sina,_cosa,_cosb,_cosc, _res3, BITS)			// sina*cosa*cosb*cosc
	FIXED_MUL3(_sina,_cosa,_cosc, _res4, BITS)					// sina*cosa*cosc
	_rot[0].y = -_res1-_res2+_res3-_res4;

	// rot[0][2]
	FIXED_MUL2(_cosa,_sinb, _res1, BITS);						// cosa*sinb
	_rot[0].z = _res1;

	// rot[1][0]
	FIXED_MUL4(_sina,_cosa,_cosb,_cosc, _res1, BITS)			// sina*cosa*cosb*cosc
	FIXED_MUL3(_sina,_cosa,_cosc, _res2, BITS)					// sina*cosa*cosc
	FIXED_MUL4(_sina,_sina,_cosb,_sinc, _res3, BITS)			// sina*sina*cosb*sinc
	FIXED_MUL3(_cosa,_cosa,_sinc, _res4, BITS)					// cosa*cosa*sinc
	_rot[1].x = _res1-_res2+_res3+_res4;

	// rot[1][1]
	FIXED_MUL4(_sina,_cosa,_cosb,_sinc, _res1, BITS)			// sina*cosa*cosb*sinc
	FIXED_MUL3(_sina,_cosa,_sinc, _res2, BITS)					// sina*cosa*sinc
	FIXED_MUL4(_sina,_sina,_cosb,_cosc, _res3, BITS)			// sina*sina*cosb*cosc
	FIXED_MUL3(_cosa,_cosa,_cosc, _res4, BITS)					// cosa*cosa*cosc
	_rot[1].y = -_res1+_res2+_res3+_res4;

	// rot[1][2]
	FIXED_MUL2(_sina,_sinb, _res1, BITS);						// sina*sinb
	_rot[1].z = _res1;

	// rot[2][0]
	FIXED_MUL3(_cosa,_sinb,_cosc, _res1, BITS)					// cosa*sinb*cosc
	FIXED_MUL3(_sina,_sinb,_sinc, _res2, BITS)					// sina*sinb*sinc
	_rot[2].x = -_res1-_res2;

	// rot[2][1]
	FIXED_MUL3(_cosa,_sinb,_sinc, _res1, BITS)					// cosa*sinb*sinc
	FIXED_MUL3(_sina,_sinb,_cosc, _res2, BITS)					// sina*sinb*cosc
	_rot[2].y = _res1-_res2;

	// rot[2][2]
	_rot[2].z = _cosb;

	return 0;
}


// FixedPoint version of arGetRot with std interface
//
AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetRot( ARFloat a, ARFloat b, ARFloat c, ARFloat rot[3][3] )
{
#if CHECK_CALC
    ARFloat   w[3];
    int      i;
#endif


	I32 _a, _b, _c;
	I32 _sina, _sinb, _sinc, _cosa, _cosb, _cosc;
	I32 _tmp1=0, _tmp2=0;
	I32 _res1,_res2,_res3,_res4;

	_a = FIXED_Float_To_Fixed_n(a, BITS);
	_b = FIXED_Float_To_Fixed_n(b, BITS);
	_c = FIXED_Float_To_Fixed_n(c, BITS);

	FIXED_SINCOS(_a, &_sina, &_cosa, BITS);
	FIXED_SINCOS(_b, &_sinb, &_cosb, BITS);
	FIXED_SINCOS(_c, &_sinc, &_cosc, BITS);

	//FIXED_SIN(_a, &_sina, BITS);
	//FIXED_SIN(_b, &_sinb, BITS);
	//FIXED_SIN(_c, &_sinc, BITS);
	//FIXED_COS(_a, &_cosa, BITS);
	//FIXED_COS(_b, &_cosb, BITS);
	//FIXED_COS(_c, &_cosc, BITS);

	// rot[0][0]
	FIXED_MUL4(_cosa,_cosa,_cosb,_cosc, _res1, BITS)			// cosa*cosa*cosb*cosc
	FIXED_MUL3(_sina,_sina,_cosc, _res2, BITS)					// sina*sina*cosc
	FIXED_MUL4(_sina,_cosa,_cosb,_sinc, _res3, BITS)			// sina*cosa*cosb*sinc
	FIXED_MUL3(_sina,_cosa,_sinc, _res4, BITS)					// sina*cosa*sinc
	rot[0][0] = FIXED_Fixed_n_To_Float((_res1+_res2+_res3-_res4), BITS);

	// rot[0][1]
	FIXED_MUL4(_cosa,_cosa,_cosb,_sinc, _res1, BITS)			// cosa*cosa*cosb*sinc
	FIXED_MUL3(_sina,_sina,_sinc, _res2, BITS)					// sina*sina*sinc
	FIXED_MUL4(_sina,_cosa,_cosb,_cosc, _res3, BITS)			// sina*cosa*cosb*cosc
	FIXED_MUL3(_sina,_cosa,_cosc, _res4, BITS)					// sina*cosa*cosc
	rot[0][1] = FIXED_Fixed_n_To_Float((-_res1-_res2+_res3-_res4), BITS);

	// rot[0][2]
	FIXED_MUL2(_cosa,_sinb, _res1, BITS);						// cosa*sinb
	rot[0][2] = FIXED_Fixed_n_To_Float(_res1, BITS);

	// rot[1][0]
	FIXED_MUL4(_sina,_cosa,_cosb,_cosc, _res1, BITS)			// sina*cosa*cosb*cosc
	FIXED_MUL3(_sina,_cosa,_cosc, _res2, BITS)					// sina*cosa*cosc
	FIXED_MUL4(_sina,_sina,_cosb,_sinc, _res3, BITS)			// sina*sina*cosb*sinc
	FIXED_MUL3(_cosa,_cosa,_sinc, _res4, BITS)					// cosa*cosa*sinc
	rot[1][0] = FIXED_Fixed_n_To_Float((_res1-_res2+_res3+_res4), BITS);

	// rot[1][1]
	FIXED_MUL4(_sina,_cosa,_cosb,_sinc, _res1, BITS)			// sina*cosa*cosb*sinc
	FIXED_MUL3(_sina,_cosa,_sinc, _res2, BITS)					// sina*cosa*sinc
	FIXED_MUL4(_sina,_sina,_cosb,_cosc, _res3, BITS)			// sina*sina*cosb*cosc
	FIXED_MUL3(_cosa,_cosa,_cosc, _res4, BITS)					// cosa*cosa*cosc
	rot[1][1] = FIXED_Fixed_n_To_Float((-_res1+_res2+_res3+_res4), BITS);

	// rot[1][2]
	FIXED_MUL2(_sina,_sinb, _res1, BITS)						// sina*sinb
	rot[1][2] = FIXED_Fixed_n_To_Float(_res1, BITS);

	// rot[2][0]
	FIXED_MUL3(_cosa,_sinb,_cosc, _res1, BITS)					// cosa*sinb*cosc
	FIXED_MUL3(_sina,_sinb,_sinc, _res2, BITS)					// sina*sinb*sinc
	rot[2][0] = FIXED_Fixed_n_To_Float((-_res1-_res2), BITS);

	// rot[2][1]
	FIXED_MUL3(_cosa,_sinb,_sinc, _res1, BITS)					// cosa*sinb*sinc
	FIXED_MUL3(_sina,_sinb,_cosc, _res2, BITS)					// sina*sinb*cosc
	rot[2][1] = FIXED_Fixed_n_To_Float((_res1-_res2), BITS);

	// rot[2][2]
	rot[2][2] = FIXED_Fixed_n_To_Float((_cosb), BITS);


#if CHECK_CALC
    for(i=0;i<3;i++) w[i] = rot[i][2];
    for(i=0;i<3;i++) rot[i][2] = rot[i][1];
    for(i=0;i<3;i++) rot[i][1] = rot[i][0];
    for(i=0;i<3;i++) rot[i][0] = w[i];
#endif

    return 0;
}


#ifdef BITS
#undef BITS
#endif
#define BITS 12


#pragma message(">>> Using arGetNewMatrix12()")

// optimized arGetNewMatrix() version which completely
// works with intel's fixed point library
// (n is 12)
int arGetNewMatrix12(I32 _a, I32 _b, I32 _c,
                    FIXED_VEC3D _trans, ARFloat trans2[3][4],
                    FIXED_VEC3D _cpara2[3], I32 _cpara32[3], FIXED_VEC3D ret[3], I32 _ret3[3], Profiler& nProfiler)
{
    int      j;

	const unsigned int dbits = 16;
	const unsigned int conv = 1<<dbits;

	// all this only works if trans2 is NULL (no stereo)
	//
	FIXED_VEC3D	_rot[3], _rot2[3];

	PROFILE_BEGINSEC(nProfiler, GETROT)
	arGetRot_28(_a*conv, _b*conv, _c*conv, _rot2);
	//arGetRot_28_old(_a*conv, _b*conv, _c*conv, _rot2);
	PROFILE_ENDSEC(nProfiler, GETROT)

	// fill _rot mirrored for better access (vector dot-methods)
	//
	_rot[0].x = _rot2[0].x >> dbits;
	_rot[0].y = _rot2[1].x >> dbits;
	_rot[0].z = _rot2[2].x >> dbits;
	_rot[1].x = _rot2[0].y >> dbits;
	_rot[1].y = _rot2[1].y >> dbits;
	_rot[1].z = _rot2[2].y >> dbits;
	_rot[2].x = _rot2[0].z >> dbits;
	_rot[2].y = _rot2[1].z >> dbits;
	_rot[2].z = _rot2[2].z >> dbits;

	for(j=0; j<3; j++)
	{
		FIXED_VEC3_DOT(_cpara2+j, _rot+0, &ret[j].x, BITS);
		FIXED_VEC3_DOT(_cpara2+j, _rot+1, &ret[j].y, BITS);
		FIXED_VEC3_DOT(_cpara2+j, _rot+2, &ret[j].z, BITS);

		FIXED_VEC3_DOT(_cpara2+j, &_trans, _ret3+j, BITS);
		_ret3[j] += _cpara32[j];
	}

    return(0);
}


#ifdef BITS
#undef BITS
#endif


#endif //_FIXEDPOINT_MATH_ACTIVATED_


//
//             POCKETPC specific code ends here
//
//////////////////////////////////////////////////////////////



static int
check_dir( ARFloat dir[3], ARFloat st[2], ARFloat ed[2],
                      ARFloat cpara[3][4] )
{
    ARMat     *mat_a;
    ARFloat    world[2][3];
    ARFloat    camera[2][2];
    ARFloat    v[2][2];
    ARFloat    h;
    int       i, j;

    mat_a = Matrix::alloc( 3, 3 );
    for(j=0;j<3;j++) for(i=0;i<3;i++) mat_a->m[j*3+i] = cpara[j][i];
    Matrix::selfInv( mat_a );
    world[0][0] = mat_a->m[0]*st[0]*(ARFloat)10.0
                + mat_a->m[1]*st[1]*(ARFloat)10.0
                + mat_a->m[2]*(ARFloat)10.0;
    world[0][1] = mat_a->m[3]*st[0]*(ARFloat)10.0
                + mat_a->m[4]*st[1]*(ARFloat)10.0
                + mat_a->m[5]*(ARFloat)10.0;
    world[0][2] = mat_a->m[6]*st[0]*(ARFloat)10.0
                + mat_a->m[7]*st[1]*(ARFloat)10.0
                + mat_a->m[8]*(ARFloat)10.0;
    Matrix::free( mat_a );
    world[1][0] = world[0][0] + dir[0];
    world[1][1] = world[0][1] + dir[1];
    world[1][2] = world[0][2] + dir[2];

    for( i = 0; i < 2; i++ ) {
        h = cpara[2][0] * world[i][0]
          + cpara[2][1] * world[i][1]
          + cpara[2][2] * world[i][2];
        if( h == 0.0 ) return -1;
        camera[i][0] = (cpara[0][0] * world[i][0]
                      + cpara[0][1] * world[i][1]
                      + cpara[0][2] * world[i][2]) / h;
        camera[i][1] = (cpara[1][0] * world[i][0]
                      + cpara[1][1] * world[i][1]
                      + cpara[1][2] * world[i][2]) / h;
    }

    v[0][0] = ed[0] - st[0];
    v[0][1] = ed[1] - st[1];
    v[1][0] = camera[1][0] - camera[0][0];
    v[1][1] = camera[1][1] - camera[0][1];

    if( v[0][0]*v[1][0] + v[0][1]*v[1][1] < 0 ) {
        dir[0] = -dir[0];
        dir[1] = -dir[1];
        dir[2] = -dir[2];
    }

    return 0;
}

static int
check_rotation( ARFloat rot[2][3] )
{
    ARFloat  v1[3], v2[3], v3[3];
    ARFloat  ca, cb, k1, k2, k3, k4;
    ARFloat  a, b, c, d;
    ARFloat  p1, q1, r1;
    ARFloat  p2, q2, r2;
    ARFloat  p3, q3, r3;
    ARFloat  p4, q4, r4;
    ARFloat  w;
    ARFloat  e1, e2, e3, e4;
    int     f;

    v1[0] = rot[0][0];
    v1[1] = rot[0][1];
    v1[2] = rot[0][2];
    v2[0] = rot[1][0];
    v2[1] = rot[1][1];
    v2[2] = rot[1][2];
    v3[0] = v1[1]*v2[2] - v1[2]*v2[1];
    v3[1] = v1[2]*v2[0] - v1[0]*v2[2];
    v3[2] = v1[0]*v2[1] - v1[1]*v2[0];
    w = (ARFloat)sqrt( v3[0]*v3[0]+v3[1]*v3[1]+v3[2]*v3[2] );
    if( w == 0.0 ) return -1;
    v3[0] /= w;
    v3[1] /= w;
    v3[2] /= w;

    cb = v1[0]*v2[0] + v1[1]*v2[1] + v1[2]*v2[2];
    if( cb < 0 ) cb *= -1.0;
    ca = ((ARFloat)sqrt(cb+1.0) + (ARFloat)sqrt(1.0-cb)) * (ARFloat)0.5;

    if( v3[1]*v1[0] - v1[1]*v3[0] != 0.0 ) {
        f = 0;
    }
    else {
        if( v3[2]*v1[0] - v1[2]*v3[0] != 0.0 ) {
            w = v1[1]; v1[1] = v1[2]; v1[2] = w;
            w = v3[1]; v3[1] = v3[2]; v3[2] = w;
            f = 1;
        }
        else {
            w = v1[0]; v1[0] = v1[2]; v1[2] = w;
            w = v3[0]; v3[0] = v3[2]; v3[2] = w;
            f = 2;
        }
    }
    if( v3[1]*v1[0] - v1[1]*v3[0] == 0.0 ) return -1;
    k1 = (v1[1]*v3[2] - v3[1]*v1[2]) / (v3[1]*v1[0] - v1[1]*v3[0]);
    k2 = (v3[1] * ca) / (v3[1]*v1[0] - v1[1]*v3[0]);
    k3 = (v1[0]*v3[2] - v3[0]*v1[2]) / (v3[0]*v1[1] - v1[0]*v3[1]);
    k4 = (v3[0] * ca) / (v3[0]*v1[1] - v1[0]*v3[1]);

    a = k1*k1 + k3*k3 + 1;
    b = k1*k2 + k3*k4;
    c = k2*k2 + k4*k4 - 1;

    d = b*b - a*c;
    if( d < 0 ) return -1;
    r1 = (-b + (ARFloat)sqrt(d))/a;
    p1 = k1*r1 + k2;
    q1 = k3*r1 + k4;
    r2 = (-b - (ARFloat)sqrt(d))/a;
    p2 = k1*r2 + k2;
    q2 = k3*r2 + k4;
    if( f == 1 ) {
        w = q1; q1 = r1; r1 = w;
        w = q2; q2 = r2; r2 = w;
        w = v1[1]; v1[1] = v1[2]; v1[2] = w;
        w = v3[1]; v3[1] = v3[2]; v3[2] = w;
        f = 0;
    }
    if( f == 2 ) {
        w = p1; p1 = r1; r1 = w;
        w = p2; p2 = r2; r2 = w;
        w = v1[0]; v1[0] = v1[2]; v1[2] = w;
        w = v3[0]; v3[0] = v3[2]; v3[2] = w;
        f = 0;
    }

    if( v3[1]*v2[0] - v2[1]*v3[0] != 0.0 ) {
        f = 0;
    }
    else {
        if( v3[2]*v2[0] - v2[2]*v3[0] != 0.0 ) {
            w = v2[1]; v2[1] = v2[2]; v2[2] = w;
            w = v3[1]; v3[1] = v3[2]; v3[2] = w;
            f = 1;
        }
        else {
            w = v2[0]; v2[0] = v2[2]; v2[2] = w;
            w = v3[0]; v3[0] = v3[2]; v3[2] = w;
            f = 2;
        }
    }
    if( v3[1]*v2[0] - v2[1]*v3[0] == 0.0 ) return -1;
    k1 = (v2[1]*v3[2] - v3[1]*v2[2]) / (v3[1]*v2[0] - v2[1]*v3[0]);
    k2 = (v3[1] * ca) / (v3[1]*v2[0] - v2[1]*v3[0]);
    k3 = (v2[0]*v3[2] - v3[0]*v2[2]) / (v3[0]*v2[1] - v2[0]*v3[1]);
    k4 = (v3[0] * ca) / (v3[0]*v2[1] - v2[0]*v3[1]);

    a = k1*k1 + k3*k3 + 1;
    b = k1*k2 + k3*k4;
    c = k2*k2 + k4*k4 - 1;

    d = b*b - a*c;
    if( d < 0 ) return -1;
    r3 = (-b + (ARFloat)sqrt(d))/a;
    p3 = k1*r3 + k2;
    q3 = k3*r3 + k4;
    r4 = (-b - (ARFloat)sqrt(d))/a;
    p4 = k1*r4 + k2;
    q4 = k3*r4 + k4;
    if( f == 1 ) {
        w = q3; q3 = r3; r3 = w;
        w = q4; q4 = r4; r4 = w;
        w = v2[1]; v2[1] = v2[2]; v2[2] = w;
        w = v3[1]; v3[1] = v3[2]; v3[2] = w;
        f = 0;
    }
    if( f == 2 ) {
        w = p3; p3 = r3; r3 = w;
        w = p4; p4 = r4; r4 = w;
        w = v2[0]; v2[0] = v2[2]; v2[2] = w;
        w = v3[0]; v3[0] = v3[2]; v3[2] = w;
        f = 0;
    }

    e1 = p1*p3+q1*q3+r1*r3; if( e1 < 0 ) e1 = -e1;
    e2 = p1*p4+q1*q4+r1*r4; if( e2 < 0 ) e2 = -e2;
    e3 = p2*p3+q2*q3+r2*r3; if( e3 < 0 ) e3 = -e3;
    e4 = p2*p4+q2*q4+r2*r4; if( e4 < 0 ) e4 = -e4;
    if( e1 < e2 ) {
        if( e1 < e3 ) {
            if( e1 < e4 ) {
                rot[0][0] = p1;
                rot[0][1] = q1;
                rot[0][2] = r1;
                rot[1][0] = p3;
                rot[1][1] = q3;
                rot[1][2] = r3;
            }
            else {
                rot[0][0] = p2;
                rot[0][1] = q2;
                rot[0][2] = r2;
                rot[1][0] = p4;
                rot[1][1] = q4;
                rot[1][2] = r4;
            }
        }
        else {
            if( e3 < e4 ) {
                rot[0][0] = p2;
                rot[0][1] = q2;
                rot[0][2] = r2;
                rot[1][0] = p3;
                rot[1][1] = q3;
                rot[1][2] = r3;
            }
            else {
                rot[0][0] = p2;
                rot[0][1] = q2;
                rot[0][2] = r2;
                rot[1][0] = p4;
                rot[1][1] = q4;
                rot[1][2] = r4;
            }
        }
    }
    else {
        if( e2 < e3 ) {
            if( e2 < e4 ) {
                rot[0][0] = p1;
                rot[0][1] = q1;
                rot[0][2] = r1;
                rot[1][0] = p4;
                rot[1][1] = q4;
                rot[1][2] = r4;
            }
            else {
                rot[0][0] = p2;
                rot[0][1] = q2;
                rot[0][2] = r2;
                rot[1][0] = p4;
                rot[1][1] = q4;
                rot[1][2] = r4;
            }
        }
        else {
            if( e3 < e4 ) {
                rot[0][0] = p2;
                rot[0][1] = q2;
                rot[0][2] = r2;
                rot[1][0] = p3;
                rot[1][1] = q3;
                rot[1][2] = r3;
            }
            else {
                rot[0][0] = p2;
                rot[0][1] = q2;
                rot[0][2] = r2;
                rot[1][0] = p4;
                rot[1][1] = q4;
                rot[1][2] = r4;
            }
        }
    }

    return 0;
}


}  // namespace ARToolKitPlus
