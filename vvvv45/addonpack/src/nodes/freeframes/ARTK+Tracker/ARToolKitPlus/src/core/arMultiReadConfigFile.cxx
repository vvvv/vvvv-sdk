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
 * $Id: arMultiReadConfigFile.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/matrix.h>


namespace ARToolKitPlus {


static char*
get_buff( char *buf, int n, FILE *fp )
{
    char *ret;

    for(;;) {
        ret = fgets( buf, n, fp );
        if(ret==NULL)
			return(NULL);
        if(buf[0] != '\n' && buf[0] != '#')
			return(ret);
    }
}


static bool
isNumber(const char* nString)
{
	int i,len = (int)strlen(nString);

	for(i=0; i<len; i++)
		if(nString[i]<'0' || nString[i]>'9')
			return false;

	return true;
}


AR_TEMPL_FUNC ARMultiMarkerInfoT*
AR_TEMPL_TRACKER::arMultiReadConfigFile(const char *filename)
{
    FILE                   *fp;
    ARMultiEachMarkerInfoT *marker;
    ARMultiMarkerInfoT     *marker_info;
    ARFloat                 wpos3d[4][2];
    char                   buf[256], buf1[256];
    int                    num;
    int                    i, j;

    if( (fp=fopen(filename,"r")) == NULL ) return NULL;

    get_buff(buf, 256, fp);
    if( sscanf(buf, "%d", &num) != 1 ) {fclose(fp); return NULL;}

    arMalloc(marker,ARMultiEachMarkerInfoT,num);

    for( i = 0; i < num; i++ ) {
        get_buff(buf, 256, fp);
        if( sscanf(buf, "%s", buf1) != 1 ) {
            fclose(fp); free(marker); return NULL;
        }
		// Added by Daniel: if the markername is an integer number
		// we directly interprete this as the marker id (used for
		// id-based markers)
		if(isNumber(buf1))
			marker[i].patt_id = atoi(buf1);
		else
        if( (marker[i].patt_id = arLoadPatt(buf1)) < 0 ) {
            fclose(fp); free(marker); return NULL;
        }

        get_buff(buf, 256, fp);
#ifdef _USE_DOUBLE_
        if( sscanf(buf, "%lf", &marker[i].width) != 1 ) {
#else
        if( sscanf(buf, "%f", &marker[i].width) != 1 ) {
#endif
            fclose(fp); free(marker); return NULL;
        }

        get_buff(buf, 256, fp);
#ifdef _USE_DOUBLE_
		if( sscanf(buf, "%lf %lf", &marker[i].center[0], &marker[i].center[1]) != 2 ) {
#else
		if( sscanf(buf, "%f %f", &marker[i].center[0], &marker[i].center[1]) != 2 ) {
#endif
            fclose(fp); free(marker); return NULL;
        }

        for( j = 0; j < 3; j++ ) {
            get_buff(buf, 256, fp);
            if( sscanf(buf,
#ifdef _USE_DOUBLE_
                       "%lf %lf %lf %lf",
#else
                       "%f %f %f %f",
#endif
                       &marker[i].trans[j][0], &marker[i].trans[j][1], &marker[i].trans[j][2], &marker[i].trans[j][3]) != 4 )
			{
                fclose(fp); free(marker); return NULL;
            }
        }
        arUtilMatInv( marker[i].trans, marker[i].itrans );

        wpos3d[0][0] = marker[i].center[0] - marker[i].width*0.5f;
        wpos3d[0][1] = marker[i].center[1] + marker[i].width*0.5f;
        wpos3d[1][0] = marker[i].center[0] + marker[i].width*0.5f;
        wpos3d[1][1] = marker[i].center[1] + marker[i].width*0.5f;
        wpos3d[2][0] = marker[i].center[0] + marker[i].width*0.5f;
        wpos3d[2][1] = marker[i].center[1] - marker[i].width*0.5f;
        wpos3d[3][0] = marker[i].center[0] - marker[i].width*0.5f;
        wpos3d[3][1] = marker[i].center[1] - marker[i].width*0.5f;
        for( j = 0; j < 4; j++ ) {
            marker[i].pos3d[j][0] = marker[i].trans[0][0] * wpos3d[j][0]
                                  + marker[i].trans[0][1] * wpos3d[j][1]
                                  + marker[i].trans[0][3];
            marker[i].pos3d[j][1] = marker[i].trans[1][0] * wpos3d[j][0]
                                  + marker[i].trans[1][1] * wpos3d[j][1]
                                  + marker[i].trans[1][3];
            marker[i].pos3d[j][2] = marker[i].trans[2][0] * wpos3d[j][0]
                                  + marker[i].trans[2][1] * wpos3d[j][1]
                                  + marker[i].trans[2][3];
        }
    }

    fclose(fp);

    marker_info = (ARMultiMarkerInfoT *)malloc( sizeof(ARMultiMarkerInfoT) );
    if( marker_info == NULL ) {free(marker); return NULL;}
    marker_info->marker     = marker;
    marker_info->marker_num = num;
    marker_info->prevF      = 0;

    return marker_info;
}


}  // namespace ARToolKitPlus
