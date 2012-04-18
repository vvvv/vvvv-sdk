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
* $Id: matrix.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __ARTOOLKITMATRIX_HEADERFILE__
#define __ARTOOLKITMATRIX_HEADERFILE__

#include <math.h>
#include <ARToolKitPlus/config.h>
#include <ARToolKitPlus/vector.h>


/* === matrix definition ===

  <---- clm --->
  [ 10  20  30 ] ^
  [ 20  10  15 ] |
  [ 12  23  13 ] row
  [ 20  10  15 ] |
  [ 13  14  15 ] v

=========================== */


namespace ARToolKitPlus {


struct ARMat {
	ARFloat *m;
	int row;
	int clm;
};


namespace Matrix {


/* 0 origin */
#define ARELEM0(mat,r,c) ((mat)->m[(r)*((mat)->clm)+(c)])
/* 1 origin */
#define ARELEM1(mat,row,clm) ARELEM0(mat,row-1,clm-1)



static ARMat  *alloc(int row, int clm);
static int    free(ARMat *m);

static int    dup(ARMat *dest, ARMat *source);
static ARMat  *allocDup(ARMat *source);

// static int    unit(ARMat *unit);
// static ARMat  *allocUnit(int dim);

static int    mul(ARMat *dest, ARMat *a, ARMat *b);
// static ARMat  *allocMul(ARMat *a, ARMat *b);

// static int    trans(ARMat *dest, ARMat *source);
// static ARMat  *allocTrans(ARMat *source);

// static int    inv(ARMat *dest, ARMat *source);
static int    selfInv(ARMat *m);
// static ARMat  *allocInv(ARMat *source);

// static ARFloat det(ARMat *m);

// static int    PCA( ARMat *input, ARMat *evec, ARVec *ev, ARVec *mean );
// static int    PCA2( ARMat *input, ARMat *evec, ARVec *ev );

// static int    disp(ARMat *m);


}  // namespace Matrix


}  // namespace ARToolKitPlus


#endif // __ARTOOLKITMATRIX_HEADERFILE__
