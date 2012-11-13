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
 * $Id: arGetCode.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <math.h>

#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/ar.h>
#include <ARToolKitPlus/matrix.h>


namespace ARToolKitPlus {
	

static void get_cpara(ARFloat world[4][2], ARFloat vertex[4][2], ARFloat para[3][3]);
static void put_zero(ARUint8 *p, int size);


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arLoadPatt(char *filename)
{
    FILE    *fp;
    int     patno;
    int     h, i, j, l, m;
    int     i1, i2, i3;

    if(pattern_num == -1 ) {
        for( i = 0; i < MAX_LOAD_PATTERNS; i++ ) patf[i] = 0;
        pattern_num = 0;
    }

    for( i = 0; i < MAX_LOAD_PATTERNS; i++ ) {
        if(patf[i] == 0) break;
    }
    if( i == MAX_LOAD_PATTERNS ) return -1;
    patno = i;

    if( (fp=fopen(filename, "r")) == NULL ) {
        printf("\"%s\" not found!!\n", filename);
        return(-1);
    }

    for( h=0; h<4; h++ ) {
        l = 0;
        for( i3 = 0; i3 < 3; i3++ ) {
            for( i2 = 0; i2 < PATTERN_HEIGHT; i2++ ) {
                for( i1 = 0; i1 < PATTERN_WIDTH; i1++ ) {
                    if( fscanf(fp, "%d", &j) != 1 ) {
                        printf("Pattern Data read error!!\n");
                        return -1;
                    }
					if(binaryMarkerThreshold!=-1)
						j = (j<binaryMarkerThreshold) ? 0 : 255;
                    j = 255-j;
                    pat[patno][h][(i2*PATTERN_WIDTH+i1)*3+i3] = j;
                    if( i3 == 0 ) patBW[patno][h][i2*PATTERN_WIDTH+i1]  = j;
                    else          patBW[patno][h][i2*PATTERN_WIDTH+i1] += j;
                    if( i3 == 2 ) patBW[patno][h][i2*PATTERN_WIDTH+i1] /= 3;
                    l += j;
                }
            }
        }
        l /= (PATTERN_HEIGHT*PATTERN_WIDTH*3);

        m = 0;
        for( i = 0; i < PATTERN_HEIGHT*PATTERN_WIDTH*3; i++ ) {
            pat[patno][h][i] -= l;
            m += (pat[patno][h][i]*pat[patno][h][i]);
        }
        patpow[patno][h] = (ARFloat)sqrt((ARFloat)m);
        if( patpow[patno][h] == 0.0 ) patpow[patno][h] = (ARFloat)0.0000001;

        m = 0;
        for( i = 0; i < PATTERN_HEIGHT*PATTERN_WIDTH; i++ ) {
            patBW[patno][h][i] -= l;
            m += (patBW[patno][h][i]*patBW[patno][h][i]);
        }
        patpowBW[patno][h] = (ARFloat)sqrt((ARFloat)m);
        if( patpowBW[patno][h] == 0.0 ) patpowBW[patno][h] = (ARFloat)0.0000001;
    }
    fclose(fp);

    patf[patno] = 1;
    pattern_num++;

/*
    gen_evec();
*/

    return( patno );
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arFreePatt( int patno )
{
    if( patf[patno] == 0 ) return -1;

    patf[patno] = 0;
    pattern_num--;

    gen_evec();

    return 1;
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arActivatePatt( int patno )
{
    if( patf[patno] == 0 ) return -1;

    patf[patno] = 1;

    return 1;
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arDeactivatePatt( int patno )
{
    if( patf[patno] == 0 ) return -1;

    patf[patno] = 2;

    return 1;
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetCode(ARUint8 *image, int *x_coord, int *y_coord, int *vertex,
				   int *code, int *dir, ARFloat *cf, int thresh)
{
    ARUint8 ext_pat[PATTERN_HEIGHT][PATTERN_WIDTH][3];

    arGetPatt(image, x_coord, y_coord, vertex, ext_pat);

	if(autoThreshold.enable)
	{
		int x,y;

		for(y=0; y<PATTERN_HEIGHT; y++)
			for(x=0; x<PATTERN_WIDTH; x++)
				//autoThreshold.addValue(ext_pat[y][x][0], ext_pat[y][x][1], ext_pat[y][x][2]);
				autoThreshold.addValue(ext_pat[y][x][0], ext_pat[y][x][1], ext_pat[y][x][2], pixelFormat);
	}


//#pragma message (">>> WARNING: compiling with marker content dumping. performance will be very low !!!")
//	FILE* fp = fopen("dump.raw", "wb");
//	fwrite(ext_pat, PATTERN_HEIGHT*PATTERN_WIDTH*3, 1, fp);
//	fclose(fp);

	switch(markerMode)
	{
	case MARKER_TEMPLATE:
		pattern_match((ARUint8 *)ext_pat, code, dir, cf);
		break;

	case MARKER_ID_SIMPLE:
		bitfield_check_simple((ARUint8 *)ext_pat, code, dir, cf, thresh);
		break;

	case MARKER_ID_BCH:
		bitfield_check_BCH((ARUint8 *)ext_pat, code, dir, cf, thresh);
		break;
	}

	/*if(useBitFieldMarkers)
	{
		if(idMarkerMode==IDMARKER_SIMPLE)
			bitfield_check_simple((ARUint8 *)ext_pat, code, dir, cf, thresh);
		else
			bitfield_check_BCH((ARUint8 *)ext_pat, code, dir, cf, thresh);
	}
	else
		pattern_match((ARUint8 *)ext_pat, code, dir, cf);*/

    return(0);
}

//#if 1
AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arGetPatt(ARUint8 *image, int *x_coord, int *y_coord, int *vertex,
						    ARUint8 ext_pat[PATTERN_HEIGHT][PATTERN_WIDTH][3])
{
    ARUint32  ext_pat2[PATTERN_HEIGHT][PATTERN_WIDTH][3];
    ARFloat    world[4][2];
    ARFloat    local[4][2];
    ARFloat    para[3][3];
    ARFloat    d, xw, yw;
    int       xc, yc;
    int       xdiv, ydiv;
    int       xdiv2, ydiv2;
    int       lx1, lx2, ly1, ly2;
    int       i, j;
	ARUint8		col8;
    // int       k1, k2, k3; // unreferenced

	unsigned short* image16 = (unsigned short*)image;

    world[0][0] = 100.0;
    world[0][1] = 100.0;
    world[1][0] = 100.0 + 10.0;
    world[1][1] = 100.0;
    world[2][0] = 100.0 + 10.0;
    world[2][1] = 100.0 + 10.0;
    world[3][0] = 100.0;
    world[3][1] = 100.0 + 10.0;
    for( i = 0; i < 4; i++ ) {
        local[i][0] = (ARFloat)x_coord[vertex[i]];
        local[i][1] = (ARFloat)y_coord[vertex[i]];
    }
    get_cpara( world, local, para );

    lx1 = (int)((local[0][0] - local[1][0])*(local[0][0] - local[1][0])
              + (local[0][1] - local[1][1])*(local[0][1] - local[1][1]));
    lx2 = (int)((local[2][0] - local[3][0])*(local[2][0] - local[3][0])
              + (local[2][1] - local[3][1])*(local[2][1] - local[3][1]));
    ly1 = (int)((local[1][0] - local[2][0])*(local[1][0] - local[2][0])
              + (local[1][1] - local[2][1])*(local[1][1] - local[2][1]));
    ly2 = (int)((local[3][0] - local[0][0])*(local[3][0] - local[0][0])
              + (local[3][1] - local[0][1])*(local[3][1] - local[0][1]));
    if( lx2 > lx1 ) lx1 = lx2;
    if( ly2 > ly1 ) ly1 = ly2;
    xdiv2 = PATTERN_WIDTH;
    ydiv2 = PATTERN_HEIGHT;
    if( arImageProcMode == AR_IMAGE_PROC_IN_FULL ) {
        while( xdiv2*xdiv2 < lx1/4 ) xdiv2*=2;
        while( ydiv2*ydiv2 < ly1/4 ) ydiv2*=2;
    }
    else {
        while( xdiv2*xdiv2*4 < lx1/4 ) xdiv2*=2;
        while( ydiv2*ydiv2*4 < ly1/4 ) ydiv2*=2;
    }
    if( xdiv2 > PATTERN_SAMPLE_NUM ) xdiv2 = PATTERN_SAMPLE_NUM;
    if( ydiv2 > PATTERN_SAMPLE_NUM ) ydiv2 = PATTERN_SAMPLE_NUM;

    xdiv = xdiv2/PATTERN_WIDTH;
    ydiv = ydiv2/PATTERN_HEIGHT;
/*
printf("%3d(%f), %3d(%f)\n", xdiv2, sqrt(lx1), ydiv2, sqrt(ly1));
*/


	// special case xdiv==1 and ydiv==1, so we can remove all divides and multiplies for indexing
	//
	if(xdiv==1 && ydiv==1)
	{
		ARFloat border = relBorderWidth * 10.0f;
		ARFloat xyFrom = 100.0f + border,
				xyTo = 110.0f - border,
				xyStep = xyTo-xyFrom;
		ARFloat steps[PATTERN_WIDTH];

		for( i = 0; i < xdiv2; i++ )
			steps[i] = xyFrom + xyStep * (ARFloat)(i+0.5f) / (ARFloat)xdiv2;

		for( j = 0; j < ydiv2; j++ ) {
			yw = steps[j];
			for( i = 0; i < xdiv2; i++ ) {
				xw = steps[i];
				d = para[2][0]*xw + para[2][1]*yw + para[2][2];
				if( d == 0 ) return(-1);
				xc = (int)((para[0][0]*xw + para[0][1]*yw + para[0][2])/d);
				yc = (int)((para[1][0]*xw + para[1][1]*yw + para[1][2])/d);
				//das hier sollte noch getestet werden...
				/*
				if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
					xc = ((xc+1)/2)*2;
					yc = ((yc+1)/2)*2;
				}*/

				if( xc >= 0 && xc < arImXsize && yc >= 0 && yc < arImYsize )
				{
					switch(pixelFormat)
					{
					case PIXEL_FORMAT_ABGR:
						ext_pat[j][i][0] = image[(yc*arImXsize+xc)*4+1];
						ext_pat[j][i][1] = image[(yc*arImXsize+xc)*4+2];
						ext_pat[j][i][2] = image[(yc*arImXsize+xc)*4+3];
						break;
					
					case PIXEL_FORMAT_BGRA:
						ext_pat[j][i][0] = image[(yc*arImXsize+xc)*4+0];
						ext_pat[j][i][1] = image[(yc*arImXsize+xc)*4+1];
						ext_pat[j][i][2] = image[(yc*arImXsize+xc)*4+2];
						break;

					case PIXEL_FORMAT_BGR:
						ext_pat[j][i][0] = image[(yc*arImXsize+xc)*3+0];
						ext_pat[j][i][1] = image[(yc*arImXsize+xc)*3+1];
						ext_pat[j][i][2] = image[(yc*arImXsize+xc)*3+2];
						break;

					case PIXEL_FORMAT_RGBA:
						ext_pat[j][i][0] = image[(yc*arImXsize+xc)*4+2];
						ext_pat[j][i][1] = image[(yc*arImXsize+xc)*4+1];
						ext_pat[j][i][2] = image[(yc*arImXsize+xc)*4+0];

					case PIXEL_FORMAT_RGB:
						ext_pat[j][i][0] = image[(yc*arImXsize+xc)*3+2];
						ext_pat[j][i][1] = image[(yc*arImXsize+xc)*3+1];
						ext_pat[j][i][2] = image[(yc*arImXsize+xc)*3+0];
						break;

					case PIXEL_FORMAT_RGB565:
						col8 = getLUM8_from_RGB565(image16+yc*arImXsize+xc);
						ext_pat[j][i][0] = col8;
						ext_pat[j][i][1] = col8;
						ext_pat[j][i][2] = col8;
						break;

					case PIXEL_FORMAT_LUM:
						col8 = image[(yc*arImXsize+xc)];
						ext_pat[j][i][0] = col8;
						ext_pat[j][i][1] = col8;
						ext_pat[j][i][2] = col8;
						break;
					}
				}
			}
		}
	}
	else
	// general case: xdiv!=1 or ydiv!=1
	//
	{
		ARFloat border = relBorderWidth * 10.0f;
		ARFloat xyFrom = 100.0f + border,
				xyTo = 110.0f - border,
				xyStep = xyTo-xyFrom;
		int jy,ix;
		ARUint8 col8;

		put_zero( (ARUint8 *)ext_pat2, PATTERN_HEIGHT*PATTERN_WIDTH*3*sizeof(ARUint32) );

		for( j = 0; j < ydiv2; j++ ) {
			//yw = (ARFloat)(102.5) + (ARFloat)(5.0) * (ARFloat)(j+0.5) / (ARFloat)ydiv2;
			yw = xyFrom + xyStep * (ARFloat)(j+0.5f) / (ARFloat)ydiv2;
			for( i = 0; i < xdiv2; i++ ) {
				//xw = (ARFloat)(102.5) + (ARFloat)(5.0) * (ARFloat)(i+0.5) / (ARFloat)xdiv2;
				xw = xyFrom + xyStep * (ARFloat)(i+0.5f) / (ARFloat)xdiv2;
				d = para[2][0]*xw + para[2][1]*yw + para[2][2];
				if( d == 0 ) return(-1);
				xc = (int)((para[0][0]*xw + para[0][1]*yw + para[0][2])/d);
				yc = (int)((para[1][0]*xw + para[1][1]*yw + para[1][2])/d);
				/*
				if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
					xc = ((xc+1)/2)*2;
					yc = ((yc+1)/2)*2;
				}*/
				if( xc >= 0 && xc < arImXsize && yc >= 0 && yc < arImYsize )
				{
					/*if(PIX_FORMAT==PIXEL_FORMAT_ABGR) {
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*PIX_SIZE+1];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*PIX_SIZE+2];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*PIX_SIZE+3];
					}
					if(PIX_FORMAT==PIXEL_FORMAT_BGRA) {
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*PIX_SIZE+0];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*PIX_SIZE+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*PIX_SIZE+2];
					}
					if(PIX_FORMAT==PIXEL_FORMAT_BGR) {
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*PIX_SIZE+0];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*PIX_SIZE+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*PIX_SIZE+2];
					}
					if(PIX_FORMAT==PIXEL_FORMAT_RGBA) {
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*PIX_SIZE+2];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*PIX_SIZE+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*PIX_SIZE+0];
					}
					if(PIX_FORMAT==PIXEL_FORMAT_RGB) {
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*PIX_SIZE+2];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*PIX_SIZE+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*PIX_SIZE+0];
					}
					if(PIX_FORMAT==PIXEL_FORMAT_RGB565) {
						int jy=j/ydiv, ix=i/xdiv;
						//ARUint8 col = RGB565_to_LUM8_LUT[image16[yc*arImXsize+xc]];
						ARUint8 col = getLUM8_from_RGB565(image16+yc*arImXsize+xc);
						ext_pat2[jy][ix][0] += col;
						ext_pat2[jy][ix][1] += col;
						ext_pat2[jy][ix][2] += col;
					}
					if(PIX_FORMAT==PIXEL_FORMAT_LUM) {
						int jy=j/ydiv, ix=i/xdiv;
						ARUint8 col = image[(yc*arImXsize+xc)*PIX_SIZE];
						ext_pat2[jy][ix][0] += col;
						ext_pat2[jy][ix][1] += col;
						ext_pat2[jy][ix][2] += col;
					}*/

					switch(pixelFormat)
					{
					case PIXEL_FORMAT_ABGR:
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*4+1];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*4+2];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*4+3];
						break;

					case PIXEL_FORMAT_BGRA:
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*4+0];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*4+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*4+2];
						break;

					case PIXEL_FORMAT_BGR:
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*3+0];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*3+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*3+2];
						break;

					case PIXEL_FORMAT_RGBA:
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*4+2];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*4+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*4+0];
						break;

					case PIXEL_FORMAT_RGB:
						ext_pat2[j/ydiv][i/xdiv][0] += image[(yc*arImXsize+xc)*3+2];
						ext_pat2[j/ydiv][i/xdiv][1] += image[(yc*arImXsize+xc)*3+1];
						ext_pat2[j/ydiv][i/xdiv][2] += image[(yc*arImXsize+xc)*3+0];
						break;

					case PIXEL_FORMAT_RGB565:
						jy=j/ydiv; ix=i/xdiv;
						col8 = getLUM8_from_RGB565(image16+yc*arImXsize+xc);
						ext_pat2[jy][ix][0] += col8;
						ext_pat2[jy][ix][1] += col8;
						ext_pat2[jy][ix][2] += col8;
						break;

					case PIXEL_FORMAT_LUM:
						jy=j/ydiv; ix=i/xdiv;
						col8 = image[yc*arImXsize+xc];
						ext_pat2[jy][ix][0] += col8;
						ext_pat2[jy][ix][1] += col8;
						ext_pat2[jy][ix][2] += col8;
						break;
					}
				}
			}
		}

		for( j = 0; j < PATTERN_HEIGHT; j++ ) {
			for( i = 0; i < PATTERN_HEIGHT; i++ ) {
				ext_pat[j][i][0] = ext_pat2[j][i][0] / (xdiv*ydiv);
				ext_pat[j][i][1] = ext_pat2[j][i][1] / (xdiv*ydiv);
				ext_pat[j][i][2] = ext_pat2[j][i][2] / (xdiv*ydiv);
			}
		}
	}

    return(0);
}
//#else
/*
int arGetPatt( ARUint8 *image, int *x_coord, int *y_coord, int *vertex,
               ARUint8 ext_pat[PATTERN_HEIGHT][PATTERN_WIDTH][3] )
{
    ARFloat  world[4][2];
    ARFloat  local[4][2];
    ARFloat  para[3][3];
    ARFloat  d, xw, yw;
    int     xc, yc;
    int     i, j;
    int     k1, k2, k3;

    world[0][0] = 100.0;
    world[0][1] = 100.0;
    world[1][0] = 100.0 + 10.0;
    world[1][1] = 100.0;
    world[2][0] = 100.0 + 10.0;
    world[2][1] = 100.0 + 10.0;
    world[3][0] = 100.0;
    world[3][1] = 100.0 + 10.0;
    for( i = 0; i < 4; i++ ) {
        local[i][0] = x_coord[vertex[i]];
        local[i][1] = y_coord[vertex[i]];
    }
    get_cpara( world, local, para );

    put_zero( (ARUint8 *)ext_pat, PATTERN_HEIGHT*PATTERN_WIDTH*3 );
    for( j = 0; j < AR_PATT_SAMPLE_NUM; j++ ) {
        yw = 102.5 + 5.0 * (j+0.5) / (ARFloat)AR_PATT_SAMPLE_NUM;
        for( i = 0; i < AR_PATT_SAMPLE_NUM; i++ ) {
            xw = 102.5 + 5.0 * (i+0.5) / (ARFloat)AR_PATT_SAMPLE_NUM;
            d = para[2][0]*xw + para[2][1]*yw + para[2][2];
            if( d == 0 ) return(-1);
            xc = (int)((para[0][0]*xw + para[0][1]*yw + para[0][2])/d);
            yc = (int)((para[1][0]*xw + para[1][1]*yw + para[1][2])/d);
            if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
                xc = ((xc+1)/2)*2;
                yc = ((yc+1)/2)*2;
            }
            if( xc >= 0 && xc < arImXsize && yc >= 0 && yc < arImYsize ) {
#ifdef AR_PIX_FORMAT_ABGR
                k1 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+1];
                k1 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0]
                   + k1*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0] = (k1 > 255)? 255: k1;
                k2 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+2];
                k2 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1]
                   + k2*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1] = (k2 > 255)? 255: k2;
                k3 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+3];
                k3 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2]
                   + k3*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2] = (k3 > 255)? 255: k3;
#endif
#ifdef AR_PIX_FORMAT_BGRA
                k1 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+0];
                k1 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0]
                   + k1*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0] = (k1 > 255)? 255: k1;
                k2 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+1];
                k2 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1]
                   + k2*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1] = (k2 > 255)? 255: k2;
                k3 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+2];
                k3 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2]
                   + k3*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2] = (k3 > 255)? 255: k3;
#endif
#ifdef AR_PIX_FORMAT_BGR
                k1 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+0];
                k1 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0]
                   + k1*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0] = (k1 > 255)? 255: k1;
                k2 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+1];
                k2 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1]
                   + k2*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1] = (k2 > 255)? 255: k2;
                k3 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+2];
                k3 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2]
                   + k3*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2] = (k3 > 255)? 255: k3;
#endif
#ifdef AR_PIX_FORMAT_RGBA
                k1 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+2];
                k1 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0]
                   + k1*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0] = (k1 > 255)? 255: k1;
                k2 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+1];
                k2 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1]
                   + k2*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1] = (k2 > 255)? 255: k2;
                k3 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+0];
                k3 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2]
                   + k3*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2] = (k3 > 255)? 255: k3;
#endif
#ifdef AR_PIX_FORMAT_RGB
                k1 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+2];
                k1 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0]
                   + k1*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][0] = (k1 > 255)? 255: k1;
                k2 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+1];
                k2 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1]
                   + k2*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][1] = (k2 > 255)? 255: k2;
                k3 = image[(yc*arImXsize+xc)*AR_PIX_SIZE+0];
                k3 = ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2]
                   + k3*(PATTERN_HEIGHT*PATTERN_WIDTH)/(AR_PATT_SAMPLE_NUM*AR_PATT_SAMPLE_NUM);
                ext_pat[j*PATTERN_HEIGHT/AR_PATT_SAMPLE_NUM][i*PATTERN_WIDTH/AR_PATT_SAMPLE_NUM][2] = (k3 > 255)? 255: k3;
#endif
            }
        }
    }

    return(0);
}
//#endif
*/


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::pattern_match( ARUint8 *data, int *code, int *dir, ARFloat *cf)
{
    ARFloat invec[EVEC_MAX];
    int    input[PATTERN_HEIGHT*PATTERN_WIDTH*3];
    int    i, j, l;
    int    k = 0; // fix VC7 compiler warning: uninitialized variable
    int    ave, sum, res, res2;
    ARFloat datapow, sum2, min;
    ARFloat max = 0.0; // fix VC7 compiler warning: uninitialized variable

	// uncomment to dump the unprojected content of the marker that artoolkit found in the image
/*		if(FILE* fp = fopen("dump.raw", "wb"))
		{
			fwrite(data, 1, PATTERN_HEIGHT*PATTERN_WIDTH*3, fp);
			fclose(fp);
		}*/

    sum = ave = 0;
    for(i=0;i<PATTERN_HEIGHT*PATTERN_WIDTH*3;i++) {
        ave += (255-data[i]);
    }
    ave /= (PATTERN_HEIGHT*PATTERN_WIDTH*3);

    if( arTemplateMatchingMode == AR_TEMPLATE_MATCHING_COLOR ) {
        for(i=0;i<PATTERN_HEIGHT*PATTERN_WIDTH*3;i++) {
            input[i] = (255-data[i]) - ave;
            sum += input[i]*input[i];
        }
    }
    else {
        for(i=0;i<PATTERN_HEIGHT*PATTERN_WIDTH;i++) {
            input[i] = ((255-data[i*3+0]) + (255-data[i*3+1]) + (255-data[i*3+02]))/3 - ave;
            sum += input[i]*input[i];
        }
    }

    datapow = (ARFloat)sqrt( (ARFloat)sum );
    if( datapow == 0.0 ) {
        *code = 0;
        *dir  = 0;
        *cf   = -1.0;
        return -1;
    }

    res = res2 = -1;
    if( arTemplateMatchingMode == AR_TEMPLATE_MATCHING_COLOR ) {
        if( arMatchingPCAMode == AR_MATCHING_WITH_PCA && evecf ) {

            for( i = 0; i < evec_dim; i++ ) {
                invec[i] = 0.0;
                for( j = 0; j < PATTERN_HEIGHT*PATTERN_WIDTH*3; j++ ) {
                    invec[i] += evec[i][j] * input[j];
                }
                invec[i] /= datapow;
            }

            min = 10000.0;
            k = -1;
            for( l = 0; l < pattern_num; l++ ) {
                k++;
                while( patf[k] == 0 ) k++;
                if( patf[k] == 2 ) continue;
#ifdef ARTK_DEBUG
                printf("%3d: ", k);
#endif
                for( j = 0; j < 4; j++ ) {
                    sum2 = 0;
                    for(i = 0; i < evec_dim; i++ ) {
                        sum2 += (invec[i] - epat[k][j][i]) * (invec[i] - epat[k][j][i]);
                    }
#ifdef ARTK_DEBUG
                    printf("%10.7f ", sum2);
#endif
                    if( sum2 < min ) { min = sum2; res = j; res2 = k; }
                }
#ifdef ARTK_DEBUG
                printf("\n");
#endif
            }
            sum = 0;
            for(i=0;i<PATTERN_HEIGHT*PATTERN_WIDTH*3;i++) sum += input[i]*pat[res2][res][i];
            max = sum / patpow[res2][res] / datapow;
        }
        else {
            k = -1;
            max = 0.0;
            for( l = 0; l < pattern_num; l++ ) {
                k++;
                while( patf[k] == 0 ) k++;
                if( patf[k] == 2 ) continue;
                for( j = 0; j < 4; j++ ) {
                    sum = 0;
                    for(i=0;i<PATTERN_HEIGHT*PATTERN_WIDTH*3;i++) sum += input[i]*pat[k][j][i];
                    sum2 = sum / patpow[k][j] / datapow;
                    if( sum2 > max ) { max = sum2; res = j; res2 = k; }
                }
            }
        }
    }
    else {
        for( l = 0; l < pattern_num; l++ ) {
            k++;
            while( patf[k] == 0 ) k++;
            if( patf[k] == 2 ) continue;
            for( j = 0; j < 4; j++ ) {
                sum = 0;
                for(i=0;i<PATTERN_HEIGHT*PATTERN_WIDTH;i++) sum += input[i]*patBW[k][j][i];
                sum2 = sum / patpowBW[k][j] / datapow;
                if( sum2 > max ) { max = sum2; res = j; res2 = k; }
            }
        }
    }

    *code = res2;
    *dir  = res;
    *cf   = max;

#ifdef ARTK_DEBUG
    printf("%d %d %f\n", res2, res, max);
#endif

    return 0;
}


AR_TEMPL_FUNC void
AR_TEMPL_TRACKER::gen_evec(void)
{
    int    i, j, k, ii, jj;
    ARMat  *input, *wevec;
    ARVec  *wev;
    ARFloat sum, sum2;
    int    dim;

    if( pattern_num < 4 ) {
        evecf   = 0;
        evecBWf = 0;
        return;
    }

#ifdef ARTK_DEBUG
    printf("------------------------------------------\n");
#endif

    dim = (pattern_num*4 < PATTERN_HEIGHT*PATTERN_WIDTH*3)? pattern_num*4: PATTERN_HEIGHT*PATTERN_WIDTH*3;
    input  = Matrix::alloc( pattern_num*4, PATTERN_HEIGHT*PATTERN_WIDTH*3 );
    wevec   = Matrix::alloc( dim, PATTERN_HEIGHT*PATTERN_WIDTH*3 );
    wev     = Vector::alloc( dim );

    for( j = jj = 0; jj < MAX_LOAD_PATTERNS; jj++ ) {
        if( patf[jj] == 0 ) continue;
        for( k = 0; k < 4; k++ ) {
            for( i = 0; i < PATTERN_HEIGHT*PATTERN_WIDTH*3; i++ ) {
                input->m[(j*4+k)*PATTERN_HEIGHT*PATTERN_WIDTH*3+i] = pat[j][k][i] / patpow[j][k];
            }
        }
        j++;
    }

    if( arMatrixPCA2(input, wevec, wev) < 0 ) {
        Matrix::free( input );
        Matrix::free( wevec );
        Vector::free( wev );
        evecf   = 0;
        evecBWf = 0;
        return;
    }

    sum = 0.0;
    for( i = 0; i < dim; i++ ) {
        sum += wev->v[i];
#ifdef ARTK_DEBUG
        printf("%2d(%10.7f): \n", i+1, sum);
#endif
        if( sum > 0.90 ) break;
        if( i == EVEC_MAX-1 ) break;
    }
    evec_dim = i+1;

    for( j = 0; j < evec_dim; j++ ) {
        for( i = 0; i < PATTERN_HEIGHT*PATTERN_WIDTH*3; i++ ) {
            evec[j][i] = wevec->m[j*PATTERN_HEIGHT*PATTERN_WIDTH*3+i];
        }
    }
    
    for( i = 0; i < MAX_LOAD_PATTERNS; i++ ) {
        if(patf[i] == 0) continue;
        for( j = 0; j < 4; j++ ) {
#ifdef ARTK_DEBUG
            printf("%2d[%d]: ", i+1, j+1);
#endif
            sum2 = 0.0;
            for( k = 0; k < evec_dim; k++ ) {
                sum = 0.0;
                for(ii=0;ii<PATTERN_HEIGHT*PATTERN_WIDTH*3;ii++) {
                    sum += evec[k][ii] * pat[i][j][ii] / patpow[i][j];
                }
#ifdef ARTK_DEBUG
                printf("%10.7f ", sum);
#endif
                epat[i][j][k] = sum;
                sum2 += sum*sum;
            }
#ifdef ARTK_DEBUG
            printf(":: %10.7f\n", sqrt(sum2));
#endif
        }
    }

    Matrix::free( input );
    Matrix::free( wevec );
    Vector::free( wev );

    evecf   = 1;
    evecBWf = 0;

    return;
}


static void
get_cpara( ARFloat world[4][2], ARFloat vertex[4][2], ARFloat para[3][3] )
{
    ARMat   *a, *b, *c;
    int     i;

    a = Matrix::alloc( 8, 8 );
    b = Matrix::alloc( 8, 1 );
    c = Matrix::alloc( 8, 1 );
    for( i = 0; i < 4; i++ ) {
        a->m[i*16+0]  = world[i][0];
        a->m[i*16+1]  = world[i][1];
        a->m[i*16+2]  = 1.0;
        a->m[i*16+3]  = 0.0;
        a->m[i*16+4]  = 0.0;
        a->m[i*16+5]  = 0.0;
        a->m[i*16+6]  = -world[i][0] * vertex[i][0];
        a->m[i*16+7]  = -world[i][1] * vertex[i][0];
        a->m[i*16+8]  = 0.0;
        a->m[i*16+9]  = 0.0;
        a->m[i*16+10] = 0.0;
        a->m[i*16+11] = world[i][0];
        a->m[i*16+12] = world[i][1];
        a->m[i*16+13] = 1.0;
        a->m[i*16+14] = -world[i][0] * vertex[i][1];
        a->m[i*16+15] = -world[i][1] * vertex[i][1];
        b->m[i*2+0] = vertex[i][0];
        b->m[i*2+1] = vertex[i][1];
    }
    Matrix::selfInv( a );
    Matrix::mul( c, a, b );
    for( i = 0; i < 2; i++ ) {
        para[i][0] = c->m[i*3+0];
        para[i][1] = c->m[i*3+1];
        para[i][2] = c->m[i*3+2];
    }
    para[2][0] = c->m[2*3+0];
    para[2][1] = c->m[2*3+1];
    para[2][2] = 1.0;
    Matrix::free( a );
    Matrix::free( b );
    Matrix::free( c );
}


/*
static void   put_zero( ARUint8 *p, int size )
{
    while( (size--) > 0 ) *(p++) = 0;
}
*/

}  // namespace ARToolKitPlus
