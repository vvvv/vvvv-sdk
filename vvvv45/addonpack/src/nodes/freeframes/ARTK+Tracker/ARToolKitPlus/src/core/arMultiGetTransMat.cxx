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
 * $Id: arMultiGetTransMat.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/matrix.h>


namespace ARToolKitPlus {


//#define  THRESH_1            2.0
// chan
#define  THRESH_1            3.0
#define  THRESH_2           20.0
#define  THRESH_3           10.0
#define  AR_MULTI_GET_TRANS_MAT_MAX_LOOP_COUNT   2
#define  AR_MULTI_GET_TRANS_MAT_MAX_FIT_ERROR    10.0

typedef struct {
    ARFloat   pos[4][2];
    ARFloat   thresh;
    ARFloat   err;
    int      marker;
    int      dir;
} arMultiEachMarkerInternalInfoT;


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arMultiGetTransMat(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config)
{
    ARFloat                *pos2d, *pos3d;
    ARFloat                rot[3][3], trans1[3][4], trans2[3][4];
    ARFloat                err = 0, err2;
    int                   max, max_area = 0, max_marker, vnum;
    int                   dir;
    int                   i, j, k;

    if( config->prevF ) {
        verify_markers( marker_info, marker_num, config );
    }

    max = -1;
    vnum = 0;
    for( i = 0; i < config->marker_num; i++ ) {
        k = -1;
        for( j = 0; j < marker_num; j++ ) {
            if( marker_info[j].id != config->marker[i].patt_id ) continue;
            if( marker_info[j].cf < 0.70 ) continue;

            if( k == -1 ) k = j;
            else if( marker_info[k].cf < marker_info[j].cf ) k = j;
        }
        if( (config->marker[i].visible=k) == -1) continue;

		// Changed by Daniel: use the selected pose estimator for this now. i'm though not sure if
		//                    it is wise to use arGetTransMatCont for multi-marker tracking...
		//
        //err = arGetTransMat(&marker_info[k], config->marker[i].center, config->marker[i].width, trans1);
		//err = (this->*poseEstimator_func)(&marker_info[k], config->marker[i].center, config->marker[i].width, trans1);
		err = executeSingleMarkerPoseEstimator(&marker_info[k], config->marker[i].center, config->marker[i].width, trans1);


#ifdef ARTK_DEBUG
	printf("##err = %10.5f %d %10.5f %10.5f\n", err, marker_info[k].dir, marker_info[k].pos[0], marker_info[k].pos[1]);
#endif
        if( err > THRESH_1 ) {
            config->marker[i].visible = -1;
            continue;
        }

        vnum++;
        if( max == -1 
         || marker_info[k].area > max_area ) {
            max = i;
            max_marker = k;
            max_area   = marker_info[k].area;
            for( j = 0; j < 3; j++ ) {
                for( k = 0; k < 4; k++ ) {
                    trans2[j][k] = trans1[j][k];
                }
            }
        }
    }
    if( max == -1 ) {
        config->prevF = 0;
        return -1;
    }

    arMalloc(pos2d, ARFloat, vnum*4*2);
    arMalloc(pos3d, ARFloat, vnum*4*3);

    j = 0;
    for( i = 0; i < config->marker_num; i++ ) {
        if( (k=config->marker[i].visible) < 0 ) continue;

        dir = marker_info[k].dir;
        pos2d[j*8+0] = marker_info[k].vertex[(4-dir)%4][0];
        pos2d[j*8+1] = marker_info[k].vertex[(4-dir)%4][1];
        pos2d[j*8+2] = marker_info[k].vertex[(5-dir)%4][0];
        pos2d[j*8+3] = marker_info[k].vertex[(5-dir)%4][1];
        pos2d[j*8+4] = marker_info[k].vertex[(6-dir)%4][0];
        pos2d[j*8+5] = marker_info[k].vertex[(6-dir)%4][1];
        pos2d[j*8+6] = marker_info[k].vertex[(7-dir)%4][0];
        pos2d[j*8+7] = marker_info[k].vertex[(7-dir)%4][1];
        pos3d[j*12+0] = config->marker[i].pos3d[0][0];
        pos3d[j*12+1] = config->marker[i].pos3d[0][1];
        pos3d[j*12+2] = config->marker[i].pos3d[0][2];
        pos3d[j*12+3] = config->marker[i].pos3d[1][0];
        pos3d[j*12+4] = config->marker[i].pos3d[1][1];
        pos3d[j*12+5] = config->marker[i].pos3d[1][2];
        pos3d[j*12+6] = config->marker[i].pos3d[2][0];
        pos3d[j*12+7] = config->marker[i].pos3d[2][1];
        pos3d[j*12+8] = config->marker[i].pos3d[2][2];
        pos3d[j*12+9] = config->marker[i].pos3d[3][0];
        pos3d[j*12+10] = config->marker[i].pos3d[3][1];
        pos3d[j*12+11] = config->marker[i].pos3d[3][2];
        j++;
    }

    if( config->prevF ) {
        for( j = 0; j < 3; j++ ) {
            for( i = 0; i < 3; i++ ) {
                rot[j][i] = config->trans[j][i];
            }
        }
        for( i = 0; i < AR_MULTI_GET_TRANS_MAT_MAX_LOOP_COUNT; i++ ) {
            err = arGetTransMat4( rot, (ARFloat (*)[2])pos2d,
                                       (ARFloat (*)[3])pos3d,
                                        vnum*4, config->trans );
            if( err < AR_MULTI_GET_TRANS_MAT_MAX_FIT_ERROR ) break;
        }

        if( err < THRESH_2 ) {
            config->prevF = 1;
            free(pos3d);
            free(pos2d);
            return err;
        }
    }

    arUtilMatMul( trans2, config->marker[max].itrans, trans1 );
    for( j = 0; j < 3; j++ ) {
        for( i = 0; i < 3; i++ ) {
            rot[j][i] = trans1[j][i];
        }
    }

    for( i = 0; i < AR_MULTI_GET_TRANS_MAT_MAX_LOOP_COUNT; i++ ) {
        err2 = arGetTransMat4( rot, (ARFloat (*)[2])pos2d, (ARFloat (*)[3])pos3d,
                              vnum*4, trans2 );
        if( err2 < AR_MULTI_GET_TRANS_MAT_MAX_FIT_ERROR ) break;
    }

    if( config->prevF == 0 || err2 < err ) {
        for( j = 0; j < 3; j++ ) {
            for( i = 0; i < 4; i++ ) {
                config->trans[j][i] = trans2[j][i];
            }
        }
        err = err2;
    }

    if( err < THRESH_3 ) {
        config->prevF = 1;
    }
    else {
        config->prevF = 0;
    }

    free(pos3d);
    free(pos2d);
    return err;
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::verify_markers(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config)
{
    arMultiEachMarkerInternalInfoT *winfo;
    ARFloat                         wtrans[3][4];
    ARFloat                         pos3d[4][2];
    ARFloat                         wx, wy, wz, hx, hy, h;
    int                            dir1, dir2 = 0, marker2;
    ARFloat                         err, err1 = 0, err2;
    ARFloat                         x1 = 0, x2 = 0, y1 = 0, y2 = 0;
    int                            w1, w2;
    int                            i, j, k;

    arMalloc(winfo,arMultiEachMarkerInternalInfoT,config->marker_num);

    for( i = 0; i < config->marker_num; i++ ) {
		arUtilMatMul(config->trans, config->marker[i].trans, wtrans);
        pos3d[0][0] = config->marker[i].center[0] - config->marker[i].width*0.5f;
        pos3d[0][1] = config->marker[i].center[1] + config->marker[i].width*0.5f;
        pos3d[1][0] = config->marker[i].center[0] + config->marker[i].width*0.5f;
        pos3d[1][1] = config->marker[i].center[1] + config->marker[i].width*0.5f;
        pos3d[2][0] = config->marker[i].center[0] + config->marker[i].width*0.5f;
        pos3d[2][1] = config->marker[i].center[1] - config->marker[i].width*0.5f;
        pos3d[3][0] = config->marker[i].center[0] - config->marker[i].width*0.5f;
        pos3d[3][1] = config->marker[i].center[1] - config->marker[i].width*0.5f;
        for( j = 0; j < 4; j++ ) {
            wx = wtrans[0][0] * pos3d[j][0]
               + wtrans[0][1] * pos3d[j][1]
               + wtrans[0][3];
            wy = wtrans[1][0] * pos3d[j][0]
               + wtrans[1][1] * pos3d[j][1]
               + wtrans[1][3];
            wz = wtrans[2][0] * pos3d[j][0]
               + wtrans[2][1] * pos3d[j][1]
               + wtrans[2][3];
            hx = arCamera->mat[0][0] * wx
               + arCamera->mat[0][1] * wy
               + arCamera->mat[0][2] * wz
               + arCamera->mat[0][3];
            hy = arCamera->mat[1][0] * wx
               + arCamera->mat[1][1] * wy
               + arCamera->mat[1][2] * wz
               + arCamera->mat[1][3];
            h  = arCamera->mat[2][0] * wx
               + arCamera->mat[2][1] * wy
               + arCamera->mat[2][2] * wz
               + arCamera->mat[2][3];
            winfo[i].pos[j][0] = hx / h;
            winfo[i].pos[j][1] = hy / h;

            if(j ==0) {x1=x2=winfo[i].pos[j][0]; y1=y2=winfo[i].pos[j][1];}
            else {
                if( winfo[i].pos[j][0] < x1 ) x1 = winfo[i].pos[j][0];
                if( winfo[i].pos[j][0] > x2 ) x2 = winfo[i].pos[j][0];
                if( winfo[i].pos[j][1] < y1 ) y1 = winfo[i].pos[j][1];
                if( winfo[i].pos[j][1] > y2 ) y2 = winfo[i].pos[j][1];
            }
        }
        winfo[i].thresh = (x2 - x1 + 1)*(y2 - y1 + 1) / 2;
    }

#ifdef ARTK_DEBUG
printf("\n");
printf("================================================================\n");
for( i = 0; i < config->marker_num; i++) {
printf("%3d: ", i+1);
for( j = 0; j < 4; j++ ) {
    printf("(%5.1f %5.1f) ", winfo[i].pos[j][0], winfo[i].pos[j][1]);
}
printf("\n");
}
printf("--------\n");
for( i = 0; i < marker_num; i++) {
printf("%3d: ", i+1);
for( j = 0; j < 4; j++ ) {
    printf("(%5.1f %5.1f) ", marker_info[i].vertex[j][0], marker_info[i].vertex[j][1]);
}
printf("\n");
}
#endif

    w1 = w2 = 0;
    for( i = 0; i < config->marker_num; i++ ) {
        marker2 = -1;
        err2 = winfo[i].thresh;
        for( j = 0; j < marker_num; j++ ) {
            if( marker_info[j].id != -1
             && marker_info[j].id != config->marker[i].patt_id
             && marker_info[j].cf > 0.7 ) continue;

            dir1 = -1;
            for( k = 0; k < 4; k++ ) {
                err = (winfo[i].pos[0][0] - marker_info[j].vertex[(k+0)%4][0])
                    * (winfo[i].pos[0][0] - marker_info[j].vertex[(k+0)%4][0])
                    + (winfo[i].pos[0][1] - marker_info[j].vertex[(k+0)%4][1])
                    * (winfo[i].pos[0][1] - marker_info[j].vertex[(k+0)%4][1])
                    + (winfo[i].pos[1][0] - marker_info[j].vertex[(k+1)%4][0])
                    * (winfo[i].pos[1][0] - marker_info[j].vertex[(k+1)%4][0])
                    + (winfo[i].pos[1][1] - marker_info[j].vertex[(k+1)%4][1])
                    * (winfo[i].pos[1][1] - marker_info[j].vertex[(k+1)%4][1])
                    + (winfo[i].pos[2][0] - marker_info[j].vertex[(k+2)%4][0])
                    * (winfo[i].pos[2][0] - marker_info[j].vertex[(k+2)%4][0])
                    + (winfo[i].pos[2][1] - marker_info[j].vertex[(k+2)%4][1])
                    * (winfo[i].pos[2][1] - marker_info[j].vertex[(k+2)%4][1])
                    + (winfo[i].pos[3][0] - marker_info[j].vertex[(k+3)%4][0])
                    * (winfo[i].pos[3][0] - marker_info[j].vertex[(k+3)%4][0])
                    + (winfo[i].pos[3][1] - marker_info[j].vertex[(k+3)%4][1])
                    * (winfo[i].pos[3][1] - marker_info[j].vertex[(k+3)%4][1]);
                if( dir1 == -1 || err < err1 ) {
                    err1 = err;
                    dir1 = k;
                }
            }
#ifdef ARTK_DEBUG
	printf("%f\n", err1);
#endif
            if( err1 < err2 ) {
                err2 = err1;
                dir2 = dir1;
                marker2 = j;
            }
        }

#ifdef ARTK_DEBUG
	printf("%3d<=>%3d, err = %f(%f)\n", i+1, marker2+1, err2, winfo[i].thresh);
#endif
        if( marker2 != -1 ) {
            winfo[i].marker = marker2;
            winfo[i].dir    = dir2;
            winfo[i].err    = err2;

            if( marker_info[marker2].id == config->marker[i].patt_id ) w1++;
            else if( marker_info[marker2].id != -1 )                   w2++;
        }
        else {
            winfo[i].marker = -1;
        }
    }
#ifdef ARTK_DEBUG
	printf("w1,w2 = %d,%d\n", w1, w2);
#endif
    if( w2 >= w1 ) {
        free(winfo);
        return -1;
    }

    for( i = 0; i < config->marker_num; i++ ) {
        for( j = 0; j < marker_num; j++ ) {
                if( marker_info[j].id == config->marker[i].patt_id ) marker_info[j].id = -1;
        }
        if( winfo[i].marker != -1 ) {
            marker_info[winfo[i].marker].id  = config->marker[i].patt_id;
            marker_info[winfo[i].marker].dir = (4-winfo[i].dir)%4;
            marker_info[winfo[i].marker].cf  = 1.0;
        }
    }

    free(winfo);

    return 0;
}


}  // namespace ARToolKitPlus
