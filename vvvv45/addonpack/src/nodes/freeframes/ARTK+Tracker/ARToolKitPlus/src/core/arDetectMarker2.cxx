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
 * $Id: arDetectMarker2.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/Tracker.h>


namespace ARToolKitPlus {

static int get_vertex( int x_coord[], int y_coord[], int st,  int ed,
                       ARFloat thresh, int vertex[], int *vnum);


AR_TEMPL_FUNC ARMarkerInfo2*
AR_TEMPL_TRACKER::arDetectMarker2(ARInt16 *limage, int label_num, int *label_ref,
                    int *warea, ARFloat *wpos, int *wclip,
                    int area_max, int area_min, ARFloat factor, int *marker_num)
{
    ARMarkerInfo2     *pm;
    int               xsize, ysize;
    int               marker_num2;
    int               i, j, ret;
    ARFloat            d;

	PROFILE_BEGINSEC(profiler, DETECTMARKER2)

    if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
        area_min /= 4;
        area_max /= 4;
        xsize = arImXsize / 2;
        ysize = arImYsize / 2;
    }
    else {
        xsize = arImXsize;
        ysize = arImYsize;
    }
    marker_num2 = 0;
    for(i=0; i<label_num; i++ ) {
        if( warea[i] < area_min || warea[i] > area_max ) continue;
        if( wclip[i*4+0] == 1 || wclip[i*4+1] == xsize-2 ) continue;
        if( wclip[i*4+2] == 1 || wclip[i*4+3] == ysize-2 ) continue;

        ret = arGetContour( limage, label_ref, i+1,
                            &(wclip[i*4]), &(marker_infoTWO[marker_num2]));
        if( ret < 0 ) continue;

        ret = check_square( warea[i], &(marker_infoTWO[marker_num2]), factor );
        if( ret < 0 ) continue;

        marker_infoTWO[marker_num2].area   = warea[i];
        marker_infoTWO[marker_num2].pos[0] = wpos[i*2+0];
        marker_infoTWO[marker_num2].pos[1] = wpos[i*2+1];
        marker_num2++;
        if(marker_num2==MAX_IMAGE_PATTERNS)
			break;
    }

    for( i=0; i < marker_num2; i++ ) {
        for( j=i+1; j < marker_num2; j++ ) {
            d = (marker_infoTWO[i].pos[0] - marker_infoTWO[j].pos[0])
              * (marker_infoTWO[i].pos[0] - marker_infoTWO[j].pos[0])
              + (marker_infoTWO[i].pos[1] - marker_infoTWO[j].pos[1])
              * (marker_infoTWO[i].pos[1] - marker_infoTWO[j].pos[1]);
            if( marker_infoTWO[i].area > marker_infoTWO[j].area ) {
                if( d < marker_infoTWO[i].area / 4 ) {
                    marker_infoTWO[j].area = 0;
                }
            }
            else {
                if( d < marker_infoTWO[j].area / 4 ) {
                    marker_infoTWO[i].area = 0;
                }
            }
        }
    }
    for( i=0; i < marker_num2; i++ ) {
        if( marker_infoTWO[i].area == 0.0 ) {
            for( j=i+1; j < marker_num2; j++ ) {
                marker_infoTWO[j-1] = marker_infoTWO[j];
            }
            marker_num2--;
        }
    }

    if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
        pm = &(marker_infoTWO[0]);
        for( i = 0; i < marker_num2; i++ ) {
            pm->area *= 4;
            pm->pos[0] *= 2.0;
            pm->pos[1] *= 2.0;
            for( j = 0; j< pm->coord_num; j++ ) {
                pm->x_coord[j] *= 2;
                pm->y_coord[j] *= 2;
            }
            pm++;
        }
    }

	PROFILE_ENDSEC(profiler, DETECTMARKER2)

    *marker_num = marker_num2;
    return( &(marker_infoTWO[0]) );
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetContour(ARInt16 *limage, int *label_ref, int label, int clip[4], ARMarkerInfo2 *marker_infoTWO)
{
    static const int      xdir[8] = { 0, 1, 1, 1, 0,-1,-1,-1};
    static const int      ydir[8] = {-1,-1, 0, 1, 1, 1, 0,-1};
    //static int      wx[AR_CHAIN_MAX];
    //static int      wy[AR_CHAIN_MAX];
    ARInt16         *p1;
    int             xsize, ysize;
    int             sx, sy, dir;
    int             dmax, d, v1 = 0;
    int             i, j;

    if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
        xsize = arImXsize / 2;
        ysize = arImYsize / 2;
    }
    else {
        xsize = arImXsize;
        ysize = arImYsize;
    }
    j = clip[2];
    p1 = &(limage[j*xsize+clip[0]]);
    for( i = clip[0]; i <= clip[1]; i++, p1++ ) {
        if( *p1 > 0 && label_ref[(*p1)-1] == label ) {
            sx = i; sy = j; break;
        }
    }
    if( i > clip[1] ) {
        printf("??? 1\n"); return(-1);
    }

    marker_infoTWO->coord_num = 1;
    marker_infoTWO->x_coord[0] = sx;
    marker_infoTWO->y_coord[0] = sy;
    dir = 5;
    for(;;) {
        p1 = &(limage[marker_infoTWO->y_coord[marker_infoTWO->coord_num-1] * xsize
                    + marker_infoTWO->x_coord[marker_infoTWO->coord_num-1]]);
        dir = (dir+5)%8;
        for(i=0;i<8;i++) {
            if( p1[ydir[dir]*xsize+xdir[dir]] > 0 ) break;
            dir = (dir+1)%8;
        }
        if( i == 8 ) {
            printf("??? 2\n"); return(-1);
        }
        marker_infoTWO->x_coord[marker_infoTWO->coord_num]
            = marker_infoTWO->x_coord[marker_infoTWO->coord_num-1] + xdir[dir];
        marker_infoTWO->y_coord[marker_infoTWO->coord_num]
            = marker_infoTWO->y_coord[marker_infoTWO->coord_num-1] + ydir[dir];
        if( marker_infoTWO->x_coord[marker_infoTWO->coord_num] == sx
         && marker_infoTWO->y_coord[marker_infoTWO->coord_num] == sy ) break;
        marker_infoTWO->coord_num++;
        if( marker_infoTWO->coord_num == AR_CHAIN_MAX-1 ) {
            printf("??? 3\n"); return(-1);
        }
    }

    dmax = 0;
    for(i=1;i<marker_infoTWO->coord_num;i++) {
        d = (marker_infoTWO->x_coord[i]-sx)*(marker_infoTWO->x_coord[i]-sx)
          + (marker_infoTWO->y_coord[i]-sy)*(marker_infoTWO->y_coord[i]-sy);
        if( d > dmax ) {
            dmax = d;
            v1 = i;
        }
    }

    for(i=0;i<v1;i++) {
        arGetContour_wx[i] = marker_infoTWO->x_coord[i];
        arGetContour_wy[i] = marker_infoTWO->y_coord[i];
    }
    for(i=v1;i<marker_infoTWO->coord_num;i++) {
        marker_infoTWO->x_coord[i-v1] = marker_infoTWO->x_coord[i];
        marker_infoTWO->y_coord[i-v1] = marker_infoTWO->y_coord[i];
    }
    for(i=0;i<v1;i++) {
        marker_infoTWO->x_coord[i-v1+marker_infoTWO->coord_num] = arGetContour_wx[i];
        marker_infoTWO->y_coord[i-v1+marker_infoTWO->coord_num] = arGetContour_wy[i];
    }
    marker_infoTWO->x_coord[marker_infoTWO->coord_num] = marker_infoTWO->x_coord[0];
    marker_infoTWO->y_coord[marker_infoTWO->coord_num] = marker_infoTWO->y_coord[0];
    marker_infoTWO->coord_num++;

    return 0;
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::check_square(int area, ARMarkerInfo2 *marker_infoTWO, ARFloat factor)
{
    int             sx, sy;
    int             dmax, d, v1;
    int             vertex[10], vnum;
    int             wv1[10], wvnum1, wv2[10], wvnum2, v2;
    ARFloat          thresh;
    int             i;


    dmax = 0;
    v1 = 0;
    sx = marker_infoTWO->x_coord[0];
    sy = marker_infoTWO->y_coord[0];
    for(i=1;i<marker_infoTWO->coord_num-1;i++) {
        d = (marker_infoTWO->x_coord[i]-sx)*(marker_infoTWO->x_coord[i]-sx)
          + (marker_infoTWO->y_coord[i]-sy)*(marker_infoTWO->y_coord[i]-sy);
        if( d > dmax ) {
            dmax = d;
            v1 = i;
        }
    }

    thresh = (ARFloat)(area/0.75) * (ARFloat)0.01 * factor;
    vnum = 1;
    vertex[0] = 0;
    wvnum1 = 0;
    wvnum2 = 0;
    if( get_vertex(marker_infoTWO->x_coord, marker_infoTWO->y_coord, 0,  v1,
                   thresh, wv1, &wvnum1) < 0 ) {
        return(-1);
    }
    if( get_vertex(marker_infoTWO->x_coord, marker_infoTWO->y_coord,
                   v1,  marker_infoTWO->coord_num-1, thresh, wv2, &wvnum2) < 0 ) {
        return(-1);
    }

    if( wvnum1 == 1 && wvnum2 == 1 ) {
        vertex[1] = wv1[0];
        vertex[2] = v1;
        vertex[3] = wv2[0];
    }
    else if( wvnum1 > 1 && wvnum2 == 0 ) {
        v2 = v1 / 2;
        wvnum1 = wvnum2 = 0;
        if( get_vertex(marker_infoTWO->x_coord, marker_infoTWO->y_coord,
                       0,  v2, thresh, wv1, &wvnum1) < 0 ) {
            return(-1);
        }
        if( get_vertex(marker_infoTWO->x_coord, marker_infoTWO->y_coord,
                       v2,  v1, thresh, wv2, &wvnum2) < 0 ) {
            return(-1);
        }
        if( wvnum1 == 1 && wvnum2 == 1 ) {
            vertex[1] = wv1[0];
            vertex[2] = wv2[0];
            vertex[3] = v1;
        }
        else {
            return(-1);
        }
    }
    else if( wvnum1 == 0 && wvnum2 > 1 ) {
        v2 = (v1 + marker_infoTWO->coord_num-1) / 2;
        wvnum1 = wvnum2 = 0;
        if( get_vertex(marker_infoTWO->x_coord, marker_infoTWO->y_coord,
                   v1, v2, thresh, wv1, &wvnum1) < 0 ) {
            return(-1);
        }
        if( get_vertex(marker_infoTWO->x_coord, marker_infoTWO->y_coord,
                   v2, marker_infoTWO->coord_num-1, thresh, wv2, &wvnum2) < 0 ) {
            return(-1);
        }
        if( wvnum1 == 1 && wvnum2 == 1 ) {
            vertex[1] = v1;
            vertex[2] = wv1[0];
            vertex[3] = wv2[0];
        }
        else {
            return(-1);
        }
    }
    else {
        return(-1);
    }

    marker_infoTWO->vertex[0] = vertex[0];
    marker_infoTWO->vertex[1] = vertex[1];
    marker_infoTWO->vertex[2] = vertex[2];
    marker_infoTWO->vertex[3] = vertex[3];
    marker_infoTWO->vertex[4] = marker_infoTWO->coord_num-1;

    return(0);
}

static int
get_vertex( int x_coord[], int y_coord[], int st,  int ed, ARFloat thresh, int vertex[], int *vnum)
{
    ARFloat   d, dmax;
    ARFloat   a, b, c;
    int      i, v1 = 0;

    a = (ARFloat)(y_coord[ed] - y_coord[st]);
    b = (ARFloat)(x_coord[st] - x_coord[ed]);
    c = (ARFloat)(x_coord[ed]*y_coord[st] - y_coord[ed]*x_coord[st]);
    dmax = 0;
    for(i=st+1;i<ed;i++) {
        d = a*x_coord[i] + b*y_coord[i] + c;
        if( d*d > dmax ) {
            dmax = d*d;
            v1 = i;
        }
    }
    if( dmax/(a*a+b*b) > thresh ) {
        if( get_vertex(x_coord, y_coord, st,  v1, thresh, vertex, vnum) < 0 )
            return(-1);

        if( (*vnum) > 5 ) return(-1);
        vertex[(*vnum)] = v1;
        (*vnum)++;

        if( get_vertex(x_coord, y_coord, v1,  ed, thresh, vertex, vnum) < 0 )
            return(-1);
    }

    return(0);
}


}  // namespace ARToolKitPlus
