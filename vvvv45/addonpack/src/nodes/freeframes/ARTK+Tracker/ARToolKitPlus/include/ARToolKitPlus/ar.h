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
* $Id: ar.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __ARTOOLKITAR_HEADERFILE__
#define __ARTOOLKITAR_HEADERFILE__


#include <stdio.h>
#ifndef __APPLE__
#include <malloc.h>
#else
#include <stdlib.h>
#endif

#include <ARToolKitPlus/config.h>
#include <ARToolKitPlus/param.h>

#define arMalloc(V,T,S)  \
{ if( ((V) = (T *)malloc( sizeof(T) * (S) )) == 0 ) \
{printf("malloc error!!\n"); exit(1);} }


namespace ARToolKitPlus {


typedef char              ARInt8;
typedef short             ARInt16;
typedef int               ARInt32;
typedef unsigned char     ARUint8;
typedef unsigned short    ARUint16;
typedef unsigned int      ARUint32;


typedef struct {
    int     area;
    int     id;
    int     dir;
    ARFloat  cf;
    ARFloat  pos[2];
    ARFloat  line[4][3];
    ARFloat  vertex[4][2];
} ARMarkerInfo;


typedef struct {
    int     area;
    ARFloat  pos[2];
    int     coord_num;
    int     x_coord[AR_CHAIN_MAX];
    int     y_coord[AR_CHAIN_MAX];
    int     vertex[5];
} ARMarkerInfo2;


typedef struct {
    ARMarkerInfo  marker;
    int     count;
} arPrevInfo;


}  // namespace ARToolKitPlus


#endif  //__ARTOOLKITAR_HEADERFILE__
