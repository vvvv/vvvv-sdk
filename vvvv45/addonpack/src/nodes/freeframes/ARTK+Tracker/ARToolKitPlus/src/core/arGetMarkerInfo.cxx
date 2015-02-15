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
 * $Id: arGetMarkerInfo.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/Tracker.h>


namespace ARToolKitPlus {


AR_TEMPL_FUNC ARMarkerInfo*
AR_TEMPL_TRACKER::arGetMarkerInfo(ARUint8 *image, ARMarkerInfo2 *marker_info2, int *marker_num, int thresh)
{
    int            id, dir;
    ARFloat         cf;
    int            i, j;

	PROFILE_BEGINSEC(profiler, GETMARKERINFO)

    for( i = j = 0; i < *marker_num; i++ ) {
        marker_infoL[j].area   = marker_info2[i].area;
        marker_infoL[j].pos[0] = marker_info2[i].pos[0];
        marker_infoL[j].pos[1] = marker_info2[i].pos[1];

        if( arGetLine(marker_info2[i].x_coord, marker_info2[i].y_coord,
                      marker_info2[i].coord_num, marker_info2[i].vertex,
                      marker_infoL[j].line, marker_infoL[j].vertex) < 0 ) continue;

        arGetCode( image,
                   marker_info2[i].x_coord, marker_info2[i].y_coord,
                   marker_info2[i].vertex, &id, &dir, &cf, thresh);

        marker_infoL[j].id  = id;
        marker_infoL[j].dir = dir;
        marker_infoL[j].cf  = cf;

        j++;
    }
    *marker_num = j;

	PROFILE_ENDSEC(profiler, GETMARKERINFO)

    return( marker_infoL );
}


}  // namespace ARToolKitPlus
