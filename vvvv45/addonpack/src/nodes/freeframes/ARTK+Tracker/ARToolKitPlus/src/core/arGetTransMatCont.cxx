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
* $Id: arGetTransMatCont.cxx 162 2006-04-19 21:28:10Z grabner $
* @file
 * ======================================================================== */

//static double arGetTransMatContSub( ARMarkerInfo *marker_info, double prev_conv[3][4],
//                                    double center[2], double width, double conv[3][4] );


namespace ARToolKitPlus {


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMatCont2(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4])
{
	return arGetTransMatCont(marker_info, conv, center, width, conv);
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMatCont(ARMarkerInfo *marker_info, ARFloat prev_conv[3][4], ARFloat center[2], ARFloat width, ARFloat conv[3][4])
{
    ARFloat  err1, err2;
    ARFloat wtrans[3][4];
    int     i, j;

    err1 = arGetTransMatContSub(marker_info, prev_conv, center, width, conv);
    if( err1 > AR_GET_TRANS_CONT_MAT_MAX_FIT_ERROR ) {
        err2 = arGetTransMat(marker_info, center, width, wtrans);
        if( err2 < err1 ) {
            for( j = 0; j < 3; j++ ) {
                for( i = 0; i < 4; i++ ) conv[j][i] = wtrans[j][i];
            }
            err1 = err2;
        }
    }

    return err1;
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arGetTransMatContSub(ARMarkerInfo *marker_info, ARFloat prev_conv[3][4], ARFloat center[2], ARFloat width, ARFloat conv[3][4])
{
    ARFloat  rot[3][3];
    ARFloat  ppos2d[4][2];
    ARFloat  ppos3d[4][2];
    int     dir;
    ARFloat  err;
    int     i, j;

    for( i = 0; i < 3; i++ ) {
        for( j = 0; j < 3; j++ ) {
            rot[i][j] = prev_conv[i][j];
        }
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
    return err;
}


}  // namespace ARToolKitPlus
