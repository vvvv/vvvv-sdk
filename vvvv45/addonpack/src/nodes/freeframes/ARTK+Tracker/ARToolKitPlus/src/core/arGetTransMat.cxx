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
 * $Id: arGetTransMat.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdlib.h>
#include <math.h>

#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/matrix.h>


namespace ARToolKitPlus {


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMat(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4])
{
    ARFloat  rot[3][3];
    ARFloat  ppos2d[4][2];
    ARFloat  ppos3d[4][2];
    int     dir;
    ARFloat  err;
    int     i;

	PROFILE_BEGINSEC(profiler, GETTRANSMAT)

	if( arGetInitRot( marker_info, arCamera->mat, rot ) < 0 )
	{
		PROFILE_ENDSEC(profiler, GETTRANSMAT)
		return -1;
	}

    dir = marker_info->dir;
    ppos2d[0][0] = marker_info->vertex[(4-dir)%4][0];
    ppos2d[0][1] = marker_info->vertex[(4-dir)%4][1];
    ppos2d[1][0] = marker_info->vertex[(5-dir)%4][0];
    ppos2d[1][1] = marker_info->vertex[(5-dir)%4][1];
    ppos2d[2][0] = marker_info->vertex[(6-dir)%4][0];
    ppos2d[2][1] = marker_info->vertex[(6-dir)%4][1];
    ppos2d[3][0] = marker_info->vertex[(7-dir)%4][0];
    ppos2d[3][1] = marker_info->vertex[(7-dir)%4][1];
    ppos3d[0][0] = center[0] - width*(ARFloat)0.5;
    ppos3d[0][1] = center[1] + width*(ARFloat)0.5;
    ppos3d[1][0] = center[0] + width*(ARFloat)0.5;
    ppos3d[1][1] = center[1] + width*(ARFloat)0.5;
    ppos3d[2][0] = center[0] + width*(ARFloat)0.5;
    ppos3d[2][1] = center[1] - width*(ARFloat)0.5;
    ppos3d[3][0] = center[0] - width*(ARFloat)0.5;
    ppos3d[3][1] = center[1] - width*(ARFloat)0.5;

    for( i = 0; i < AR_GET_TRANS_MAT_MAX_LOOP_COUNT; i++ ) {
		err = arGetTransMat3( rot, ppos2d, ppos3d, 4, conv, arCamera);
        if( err < AR_GET_TRANS_MAT_MAX_FIT_ERROR ) break;
    }

	PROFILE_ENDSEC(profiler, GETTRANSMAT)
    return err;
}

AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMat2(ARFloat rot[3][3], ARFloat ppos2d[][2], ARFloat ppos3d[][2], int num, ARFloat conv[3][4])
{
	return arGetTransMat3( rot, ppos2d, ppos3d, num, conv, arCamera);
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMat3(ARFloat rot[3][3], ARFloat ppos2d[][2],
                   ARFloat ppos3d[][2], int num, ARFloat conv[3][4],
				   Camera *pCam )
                   //ARFloat *dist_factor, ARFloat cpara[3][4] )
{
    ARFloat  off[3], pmax[3], pmin[3];
    ARFloat  ret;
    int     i;

	PROFILE_BEGINSEC(profiler, GETTRANSMAT3)

    pmax[0]=pmax[1]=pmax[2] = -10000000000.0;
    pmin[0]=pmin[1]=pmin[2] =  10000000000.0;
    for( i = 0; i < num; i++ ) {
        if( ppos3d[i][0] > pmax[0] ) pmax[0] = ppos3d[i][0];
        if( ppos3d[i][0] < pmin[0] ) pmin[0] = ppos3d[i][0];
        if( ppos3d[i][1] > pmax[1] ) pmax[1] = ppos3d[i][1];
        if( ppos3d[i][1] < pmin[1] ) pmin[1] = ppos3d[i][1];
/*
        if( ppos3d[i][2] > pmax[2] ) pmax[2] = ppos3d[i][2];
        if( ppos3d[i][2] < pmin[2] ) pmin[2] = ppos3d[i][2];
*/
    }
    off[0] = -(pmax[0] + pmin[0])  * (ARFloat)0.5;
    off[1] = -(pmax[1] + pmin[1])  * (ARFloat)0.5;
    off[2] = -(pmax[2] + pmin[2])  * (ARFloat)0.5;
    for( i = 0; i < num; i++ ) {
        pos3d[i][0] = ppos3d[i][0] + off[0];
        pos3d[i][1] = ppos3d[i][1] + off[1];
/*
        pos3d[i][2] = ppos3d[i][2] + off[2];
*/
        pos3d[i][2] = 0.0;
    }

    ret = arGetTransMatSub( rot, ppos2d, pos3d, num, conv, pCam);
                            //dist_factor, cpara );

    conv[0][3] = conv[0][0]*off[0] + conv[0][1]*off[1] + conv[0][2]*off[2] + conv[0][3];
    conv[1][3] = conv[1][0]*off[0] + conv[1][1]*off[1] + conv[1][2]*off[2] + conv[1][3];
    conv[2][3] = conv[2][0]*off[0] + conv[2][1]*off[1] + conv[2][2]*off[2] + conv[2][3];

	PROFILE_ENDSEC(profiler, GETTRANSMAT3)
    return ret;
}

AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMat4(ARFloat rot[3][3], ARFloat ppos2d[][2], ARFloat ppos3d[][3], int num, ARFloat conv[3][4])
{
    return arGetTransMat5( rot, ppos2d, ppos3d, num, conv, arCamera);
//                           arParam.dist_factor, arParam.mat );
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMat5(ARFloat rot[3][3], ARFloat ppos2d[][2],
				   ARFloat ppos3d[][3], int num, ARFloat conv[3][4],
				   Camera *pCam)
				   //ARFloat *dist_factor, ARFloat cpara[3][4])
{
    ARFloat  off[3], pmax[3], pmin[3];
    ARFloat  ret;
    int     i;

    pmax[0]=pmax[1]=pmax[2] = -10000000000.0;
    pmin[0]=pmin[1]=pmin[2] =  10000000000.0;
    for( i = 0; i < num; i++ ) {
        if( ppos3d[i][0] > pmax[0] ) pmax[0] = ppos3d[i][0];
        if( ppos3d[i][0] < pmin[0] ) pmin[0] = ppos3d[i][0];
        if( ppos3d[i][1] > pmax[1] ) pmax[1] = ppos3d[i][1];
        if( ppos3d[i][1] < pmin[1] ) pmin[1] = ppos3d[i][1];
        if( ppos3d[i][2] > pmax[2] ) pmax[2] = ppos3d[i][2];
        if( ppos3d[i][2] < pmin[2] ) pmin[2] = ppos3d[i][2];
    }
    off[0] = -(pmax[0] + pmin[0])  * (ARFloat)0.5;
    off[1] = -(pmax[1] + pmin[1])  * (ARFloat)0.5;
    off[2] = -(pmax[2] + pmin[2])  * (ARFloat)0.5;
    for( i = 0; i < num; i++ ) {
        pos3d[i][0] = ppos3d[i][0] + off[0];
        pos3d[i][1] = ppos3d[i][1] + off[1];
        pos3d[i][2] = ppos3d[i][2] + off[2];
    }

    ret = arGetTransMatSub( rot, ppos2d, pos3d, num, conv, pCam);
                            //dist_factor, cpara );

    conv[0][3] = conv[0][0]*off[0] + conv[0][1]*off[1] + conv[0][2]*off[2] + conv[0][3];
    conv[1][3] = conv[1][0]*off[0] + conv[1][1]*off[1] + conv[1][2]*off[2] + conv[1][3];
    conv[2][3] = conv[2][0]*off[0] + conv[2][1]*off[1] + conv[2][2]*off[2] + conv[2][3];

    return ret;
}

AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMatSub(ARFloat rot[3][3], ARFloat ppos2d[][2],
                     ARFloat pos3d[][3], int num, ARFloat conv[3][4],
					 Camera *pCam )
                     //ARFloat *dist_factor, ARFloat cpara[3][4] )
{
    ARMat   *mat_a, *mat_b, *mat_c, *mat_d, *mat_e, *mat_f;
    ARFloat  trans[3];
    ARFloat  wx, wy, wz;
    ARFloat  ret;
    int     i, j;

	PROFILE_BEGINSEC(profiler, GETTRANSMATSUB)

    mat_a = Matrix::alloc( num*2, 3 );
    mat_b = Matrix::alloc( 3, num*2 );
    mat_c = Matrix::alloc( num*2, 1 );
    mat_d = Matrix::alloc( 3, 3 );
    mat_e = Matrix::alloc( 3, 1 );
    mat_f = Matrix::alloc( 3, 1 );

    if( arFittingMode == AR_FITTING_TO_INPUT ) {
        for( i = 0; i < num; i++ ) {
            arParamIdeal2Observ_std(pCam, ppos2d[i][0], ppos2d[i][1], &pos2d[i][0], &pos2d[i][1]);
        }
    }
    else {
        for( i = 0; i < num; i++ ) {
            pos2d[i][0] = ppos2d[i][0];
            pos2d[i][1] = ppos2d[i][1];
        }
    }

    for( j = 0; j < num; j++ ) {
        wx = rot[0][0] * pos3d[j][0]
           + rot[0][1] * pos3d[j][1]
           + rot[0][2] * pos3d[j][2];
        wy = rot[1][0] * pos3d[j][0]
           + rot[1][1] * pos3d[j][1]
           + rot[1][2] * pos3d[j][2];
        wz = rot[2][0] * pos3d[j][0]
           + rot[2][1] * pos3d[j][1]
           + rot[2][2] * pos3d[j][2];
        mat_a->m[j*6+0] = mat_b->m[num*0+j*2] = pCam->mat[0][0];
        mat_a->m[j*6+1] = mat_b->m[num*2+j*2] = pCam->mat[0][1];
        mat_a->m[j*6+2] = mat_b->m[num*4+j*2] = pCam->mat[0][2] - pos2d[j][0];
        mat_c->m[j*2+0] = wz * pos2d[j][0]
               - pCam->mat[0][0]*wx - pCam->mat[0][1]*wy - pCam->mat[0][2]*wz;
        mat_a->m[j*6+3] = mat_b->m[num*0+j*2+1] = 0.0;
        mat_a->m[j*6+4] = mat_b->m[num*2+j*2+1] = pCam->mat[1][1];
        mat_a->m[j*6+5] = mat_b->m[num*4+j*2+1] = pCam->mat[1][2] - pos2d[j][1];
        mat_c->m[j*2+1] = wz * pos2d[j][1]
               - pCam->mat[1][1]*wy - pCam->mat[1][2]*wz;
    }
    Matrix::mul( mat_d, mat_b, mat_a );
    Matrix::mul( mat_e, mat_b, mat_c );
    Matrix::selfInv( mat_d );
    Matrix::mul( mat_f, mat_d, mat_e );
    trans[0] = mat_f->m[0];
    trans[1] = mat_f->m[1];
    trans[2] = mat_f->m[2];

	/*trans[0] = 3.96559f;
	trans[1] = 27.0546f;
	trans[2] = 274.627f;*/

	{
		ARFloat a,b,c;
		arGetAngle( rot, &a, &b, &c );

		//trans[0] = -13.5f;
		//trans[1] = 45.7f;
		//trans[2] = 303.0f;
		//arGetRot( -90.5f*3.1415f/180.0f, 120.3f*3.1415f/180.0f, 31.2f*3.1415f/180.0f, rot );

		ret = arModifyMatrix( rot, trans, pCam->mat, pos3d, pos2d, num );

		arGetAngle( rot, &a, &b, &c );
		a=a;
	}

	// double begin
	//
    /*for( j = 0; j < num; j++ ) {
        wx = rot[0][0] * pos3d[j][0]
           + rot[0][1] * pos3d[j][1]
           + rot[0][2] * pos3d[j][2];
        wy = rot[1][0] * pos3d[j][0]
           + rot[1][1] * pos3d[j][1]
           + rot[1][2] * pos3d[j][2];
        wz = rot[2][0] * pos3d[j][0]
           + rot[2][1] * pos3d[j][1]
           + rot[2][2] * pos3d[j][2];
        mat_a->m[j*6+0] = mat_b->m[num*0+j*2] = pCam->mat[0][0];
        mat_a->m[j*6+1] = mat_b->m[num*2+j*2] = pCam->mat[0][1];
        mat_a->m[j*6+2] = mat_b->m[num*4+j*2] = pCam->mat[0][2] - pos2d[j][0];
        mat_c->m[j*2+0] = wz * pos2d[j][0]
               - pCam->mat[0][0]*wx - pCam->mat[0][1]*wy - pCam->mat[0][2]*wz;
        mat_a->m[j*6+3] = mat_b->m[num*0+j*2+1] = 0.0;
        mat_a->m[j*6+4] = mat_b->m[num*2+j*2+1] = pCam->mat[1][1];
        mat_a->m[j*6+5] = mat_b->m[num*4+j*2+1] = pCam->mat[1][2] - pos2d[j][1];
        mat_c->m[j*2+1] = wz * pos2d[j][1]
               - pCam->mat[1][1]*wy - pCam->mat[1][2]*wz;
    }
    Matrix::mul( mat_d, mat_b, mat_a );
    Matrix::mul( mat_e, mat_b, mat_c );
    Matrix::selfInv( mat_d );
    Matrix::mul( mat_f, mat_d, mat_e );
    trans[0] = mat_f->m[0];
    trans[1] = mat_f->m[1];
    trans[2] = mat_f->m[2];

    ret = arModifyMatrix( rot, trans, pCam->mat, pos3d, pos2d, num );*/
	//
	// double end

    Matrix::free( mat_a );
    Matrix::free( mat_b );
    Matrix::free( mat_c );
    Matrix::free( mat_d );
    Matrix::free( mat_e );
    Matrix::free( mat_f );

    for( j = 0; j < 3; j++ ) {
        for( i = 0; i < 3; i++ ) conv[j][i] = rot[j][i];
        conv[j][3] = trans[j];
    }

	PROFILE_ENDSEC(profiler, GETTRANSMATSUB)
    return ret;
}


}  // namespace ARToolKitPlus
