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
 * $Id: arUtil.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <math.h>

#ifdef _WIN32

#ifndef _WIN32_WCE
#include <sys/timeb.h>
#endif

#include <windows.h>
#else
#include <sys/time.h>
#endif

#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/param.h>
#include <ARToolKitPlus/matrix.h>


namespace ARToolKitPlus {


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arInitCparam(Camera *pCam)
{
	// if the camera parameters change, the undistortion LUT has to be rebuilt.
	// (this is done automatically in arParamObserv2Ideal_LUT or arParamIdeal2Observ_LUT)
	//
	if(undistO2ITable && (arImXsize!=pCam->xsize || arImYsize!=pCam->ysize))
	{
		artkp_Free(undistO2ITable);
		undistO2ITable = NULL;
	}

	arImXsize = pCam->xsize;
	arImYsize = pCam->ysize;

    return(0);
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetLine(int x_coord[], int y_coord[], int coord_num, int vertex[], ARFloat line[4][3], ARFloat v[4][2])
{
    //return arGetLine2( x_coord, y_coord, coord_num, vertex, line, v, arParam.dist_factor );
	return arGetLine2( x_coord, y_coord, coord_num, vertex, line, v, arCamera );
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetLine2(int x_coord[], int y_coord[], int coord_num,
                    int vertex[], ARFloat line[4][3], ARFloat v[4][2], Camera *pCam) 
{
    ARMat    *input, *evec;
    ARVec    *ev, *mean;
    ARFloat   w1;
    int      st, ed, n;
    int      i, j;

    ev     = Vector::alloc( 2 );
    mean   = Vector::alloc( 2 );
    evec   = Matrix::alloc( 2, 2 );
    for( i = 0; i < 4; i++ ) {
        w1 = (ARFloat)(vertex[i+1]-vertex[i]+1) * (ARFloat)0.05 + (ARFloat)0.5;
        st = (int)(vertex[i]   + w1);
        ed = (int)(vertex[i+1] - w1);
        n = ed - st + 1;
        input  = Matrix::alloc( n, 2 );
        for( j = 0; j < n; j++ ) {
			// does not work
            (this->*arParamObserv2Ideal_func)( pCam, (ARFloat)x_coord[st+j], (ARFloat)y_coord[st+j], &(input->m[j*2+0]), &(input->m[j*2+1]) );
			//
        }
        if( arMatrixPCA(input, evec, ev, mean) < 0 ) {
            Matrix::free( input );
            Matrix::free( evec );
            Vector::free( mean );
            Vector::free( ev );
            return(-1);
        }
        line[i][0] =  evec->m[1];
        line[i][1] = -evec->m[0];
        line[i][2] = -(line[i][0]*mean->v[0] + line[i][1]*mean->v[1]);
        Matrix::free( input );
    }
    Matrix::free( evec );
    Vector::free( mean );
    Vector::free( ev );

    for( i = 0; i < 4; i++ ) {
        w1 = line[(i+3)%4][0] * line[i][1] - line[i][0] * line[(i+3)%4][1];
        if( w1 == 0.0 ) return(-1);
        v[i][0] = (  line[(i+3)%4][1] * line[i][2]
                   - line[i][1] * line[(i+3)%4][2] ) / w1;
        v[i][1] = (  line[i][0] * line[(i+3)%4][2]
                   - line[(i+3)%4][0] * line[i][2] ) / w1;
    }

    return(0);
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arUtilMatMul( ARFloat s1[3][4], ARFloat s2[3][4], ARFloat d[3][4] )
{
    int     i, j;

    for( j = 0; j < 3; j++ ) {
        for( i = 0; i < 4; i++) {
            d[j][i] = s1[j][0] * s2[0][i]
                    + s1[j][1] * s2[1][i]
                    + s1[j][2] * s2[2][i];
        }
        d[j][3] += s1[j][3];
    }

    return 0;
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arUtilMatInv(ARFloat s[3][4], ARFloat d[3][4])
{
    ARMat       *mat;
    int         i, j;

    mat = Matrix::alloc( 4, 4 );
    for( j = 0; j < 3; j++ ) {
        for( i = 0; i < 4; i++ ) {
            mat->m[j*4+i] = s[j][i];
        }
    }
    mat->m[3*4+0] = 0; mat->m[3*4+1] = 0;
    mat->m[3*4+2] = 0; mat->m[3*4+3] = 1;
    Matrix::selfInv( mat );
    for( j = 0; j < 3; j++ ) {
        for( i = 0; i < 4; i++ ) {
            d[j][i] = mat->m[j*4+i];
        }
    }
    Matrix::free( mat );

    return 0;
}


}  // namespace ARToolKitPlus
