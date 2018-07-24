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
 * $Id: arDetectMarker.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/Tracker.h>
#include <stdio.h>


namespace ARToolKitPlus {


// marker detection using tracking history
//
AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arDetectMarker(ARUint8 *dataPtr, int _thresh, ARMarkerInfo **marker_info, int *marker_num)
{
    ARInt16                *limage=NULL;
    int                    label_num;
    int                    *area, *clip, *label_ref;
    ARFloat                 *pos;
    ARFloat                 rarea, rlen, rlenmin;
    ARFloat                 diff, diffmin;
    int                    cid, cdir;
    int                    i, j, k;

	autoThreshold.reset();
	checkImageBuffer();

//	FILE* fp = fopen("imgdump.raw", "wb");
//	fwrite(dataPtr, 1, 320*240*2, fp);
//	fclose(fp);

    *marker_num = 0;

	for(int numTries = 0;;)
	{
		limage = arLabeling(dataPtr, _thresh, &label_num, &area, &pos, &clip, &label_ref);
		if(limage)
		{
			marker_info2 = arDetectMarker2(limage, label_num, label_ref, area, pos, clip, AR_AREA_MAX, AR_AREA_MIN, 1.0, &wmarker_num);
			assert(wmarker_num <= MAX_IMAGE_PATTERNS);
			if(marker_info2)
			{
				wmarker_info = arGetMarkerInfo(dataPtr, marker_info2, &wmarker_num, _thresh);
				assert(wmarker_num <= MAX_IMAGE_PATTERNS);
				if(wmarker_info && wmarker_num>0)
					break;
			}
		}

		if(!autoThreshold.enable)
			break;
		else
		{
			_thresh = thresh = (rand() % 230) + 10;
			if(++numTries>autoThreshold.numRandomRetries)
				break;
		}

	}

	if(!limage || !marker_info2 || !wmarker_info)
		return -1;

    for( i = 0; i < prev_num; i++ ) {
        rlenmin = 10.0;
        cid = -1;
        for( j = 0; j < wmarker_num; j++ ) {
            rarea = (ARFloat)prev_info[i].marker.area / (ARFloat)wmarker_info[j].area;
            if( rarea < 0.7 || rarea > 1.43 ) continue;
            rlen = ( (wmarker_info[j].pos[0] - prev_info[i].marker.pos[0])
                   * (wmarker_info[j].pos[0] - prev_info[i].marker.pos[0])
                   + (wmarker_info[j].pos[1] - prev_info[i].marker.pos[1])
                   * (wmarker_info[j].pos[1] - prev_info[i].marker.pos[1]) ) / wmarker_info[j].area;
            if( rlen < 0.5 && rlen < rlenmin ) {
                rlenmin = rlen;
                cid = j;
            }
        }
        if( cid >= 0 && wmarker_info[cid].cf < prev_info[i].marker.cf ) {
            wmarker_info[cid].cf = prev_info[i].marker.cf;
            wmarker_info[cid].id = prev_info[i].marker.id;
            diffmin = 10000.0 * 10000.0;
            cdir = -1;
            for( j = 0; j < 4; j++ ) {
                diff = 0;
                for( k = 0; k < 4; k++ ) {
                    diff += (prev_info[i].marker.vertex[k][0] - wmarker_info[cid].vertex[(j+k)%4][0])
                          * (prev_info[i].marker.vertex[k][0] - wmarker_info[cid].vertex[(j+k)%4][0])
                          + (prev_info[i].marker.vertex[k][1] - wmarker_info[cid].vertex[(j+k)%4][1])
                          * (prev_info[i].marker.vertex[k][1] - wmarker_info[cid].vertex[(j+k)%4][1]);
                }
                if( diff < diffmin ) {
                    diffmin = diff;
                    cdir = (prev_info[i].marker.dir - j + 4) % 4;
                }
            }
            wmarker_info[cid].dir = cdir;
        }
    }

    for( i = 0; i < wmarker_num; i++ ) {
        if( wmarker_info[i].cf < 0.5 ) wmarker_info[i].id = -1;
   }


/*------------------------------------------------------------*/

    for( i = j = 0; i < prev_num; i++ ) {
        prev_info[i].count++;
        if( prev_info[i].count < 4 ) {
            prev_info[j] = prev_info[i];
            j++;
        }
    }
    prev_num = j;

    for( i = 0; i < wmarker_num; i++ ) {
        if( wmarker_info[i].id < 0 )
			continue;

        for( j = 0; j < prev_num; j++ ) {
            if( prev_info[j].marker.id == wmarker_info[i].id )
				break;
        }
		if(j<MAX_IMAGE_PATTERNS)
		{
			prev_info[j].marker = wmarker_info[i];
			prev_info[j].count  = 1;
			if( j == prev_num )
				prev_num++;
		}
    }

    for( i = 0; i < prev_num; i++ ) {
        for( j = 0; j < wmarker_num; j++ ) {
            rarea = (ARFloat)prev_info[i].marker.area / (ARFloat)wmarker_info[j].area;
            if( rarea < 0.7 || rarea > 1.43 ) continue;
            rlen = ( (wmarker_info[j].pos[0] - prev_info[i].marker.pos[0])
                   * (wmarker_info[j].pos[0] - prev_info[i].marker.pos[0])
                   + (wmarker_info[j].pos[1] - prev_info[i].marker.pos[1])
                   * (wmarker_info[j].pos[1] - prev_info[i].marker.pos[1]) ) / wmarker_info[j].area;
            if( rlen < 0.5 ) break;
        }
        if(j==wmarker_num && wmarker_num<MAX_IMAGE_PATTERNS) {
            wmarker_info[wmarker_num] = prev_info[i].marker;
            wmarker_num++;
			assert(wmarker_num <= MAX_IMAGE_PATTERNS);
        }
    }


    *marker_num  = wmarker_num;
    *marker_info = wmarker_info;

	assert(*marker_num <= MAX_IMAGE_PATTERNS);

	if(autoThreshold.enable)
		thresh = autoThreshold.calc();

    return 0;
}


// marker detection without using tracking history
//
AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arDetectMarkerLite(ARUint8 *dataPtr, int _thresh, ARMarkerInfo **marker_info, int *marker_num)
{
    ARInt16                *limage = NULL;
    int                    label_num;
    int                    *area, *clip, *label_ref;
    ARFloat                 *pos;
    int                    i;

	autoThreshold.reset();
	checkImageBuffer();

    *marker_num = 0;

	for(int numTries = 0;;)
	{
		limage = arLabeling(dataPtr, _thresh, &label_num, &area, &pos, &clip, &label_ref);
		if(limage)
		{
			marker_info2 = arDetectMarker2(limage, label_num, label_ref, area, pos, clip, AR_AREA_MAX, AR_AREA_MIN, 1.0, &wmarker_num);
			if(marker_info2)
			{
				wmarker_info = arGetMarkerInfo(dataPtr, marker_info2, &wmarker_num, _thresh);
				if(wmarker_info && wmarker_num>0)
					break;
			}
		}

		if(!autoThreshold.enable)
			break;
		else
		{
			_thresh = thresh = (rand() % 230) + 10;
			if(++numTries>autoThreshold.numRandomRetries)
				break;
		}

	}

	if(!limage || !marker_info2 || !wmarker_info)
		return -1;


/*
    limage = arLabeling(dataPtr, _thresh, &label_num, &area, &pos, &clip, &label_ref);
    if( limage == 0 )    return -1;

    marker_info2 = arDetectMarker2(limage, label_num, label_ref, area, pos, clip, AR_AREA_MAX, AR_AREA_MIN, 1.0, &wmarker_num);
    if( marker_info2 == 0 ) return -1;

    wmarker_info = arGetMarkerInfo(dataPtr, marker_info2, &wmarker_num, _thresh);
    if( wmarker_info == 0 ) return -1;
*/

    for( i = 0; i < wmarker_num; i++ )
        if( wmarker_info[i].cf < 0.5 )
			wmarker_info[i].id = -1;


    *marker_num  = wmarker_num;
    *marker_info = wmarker_info;

	if(autoThreshold.enable)
		thresh = autoThreshold.calc();

    return 0;
}


}	// namespace ARToolKitPlus
