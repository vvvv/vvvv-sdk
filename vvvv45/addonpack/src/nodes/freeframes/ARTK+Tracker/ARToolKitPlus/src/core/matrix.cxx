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
 * $Id: matrix.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <math.h>
#ifndef __APPLE__
#include <malloc.h>
#else
#include <stdlib.h>
#endif
#include <ARToolKitPlus/matrix.h>


namespace ARToolKitPlus {


namespace Matrix {


#define MATRIX(name,x,y,width)  ( *(name + (width) * (x) + (y)) )


// declaration of internal helper functions (see end of file)
// static ARFloat mdet(ARFloat *ap, int dimen, int rowa);
static ARFloat *minv( ARFloat *ap, int dimen, int rowa );


// from mAlloc.c
static ARMat*
alloc(int row, int clm)
{
	ARMat *m;

	m = (ARMat *)malloc(sizeof(ARMat));
	if( m == NULL ) return NULL;

	m->m = (ARFloat *)malloc(sizeof(ARFloat) * row * clm);
	if(m->m == NULL) {
		free(m);
		return NULL;
	}
	else {
		m->row = row;
		m->clm = clm;
	}

	return m;
}


// from mFree.c
static int
free(ARMat *m)
{
	::free(m->m);
	::free(m);

	return 0;
}


// from mAllocDup.c
static ARMat*
allocDup(ARMat *source)
{
	ARMat *dest;

	dest = alloc(source->row, source->clm);
        if( dest == NULL ) return NULL;

	if( dup(dest, source) < 0 ) {
		free(dest);
		return NULL;
	}

	return dest;
}


// from mAllocInv.c
/*
static ARMat*
allocInv(ARMat *source)
{
	ARMat *dest;

	dest = alloc(source->row, source->row);
	if( dest == NULL ) return NULL;

	if( inv(dest, source) < 0 ) {
		free( dest );
		return NULL;
	}

	return dest;
}
*/

// from mAllocMul.c
/*
static ARMat*
allocMul(ARMat *a, ARMat *b)
{
	ARMat *dest;

	dest = alloc(a->row, b->clm);
	if( dest == NULL ) return NULL;

	if( mul(dest, a, b) < 0 ) {
		free(dest);
		return NULL;
	}

	return dest;
}
*/

// from mAllocTrans.c
/*
static ARMat*
allocTrans(ARMat *source)
{
	ARMat *dest;

	dest = alloc(source->clm, source->row);
	if( dest == NULL ) return NULL;

	if( trans(dest, source) < 0 ) {
		free(dest);
		return NULL;
	}

	return dest;
}
*/

// from mAllocUnit.c
/*
static ARMat*
allocUnit(int dim)
{
	ARMat *m;

	m = alloc(dim, dim);
	if( m == NULL ) return NULL;

	if( unit(m) < 0 ) {
		free(m);
		return NULL;
	}

	return m;
}
*/

// from mDet.c
/*
static ARFloat
det(ARMat *m)
{

	if(m->row != m->clm) return 0.0;

	return mdet(m->m, m->row, m->row);
}
*/

// from mDisp.c
/*
static int
disp(ARMat *m)
{
	int r, c;

	printf(" === matrix (%d,%d) ===\n", m->row, m->clm);
	for(r = 0; r < m->row; r++) {
		printf(" |");
		for(c = 0; c < m->clm; c++) {
			printf(" %10g", ARELEM0(m, r, c));
		}
		printf(" |\n");
	}
	printf(" ======================\n");

	return 0;
}
*/

// from mDup.c
static int
dup(ARMat *dest, ARMat *source)
{
	int r,c;

	if(dest->row != source->row || dest->clm != source->clm) {
		return -1;
	}
	for(r = 0; r < source->row; r++) {
		for(c = 0; c < source->clm; c++) {
			ARELEM0(dest, r, c) = ARELEM0(source, r, c);
		}
	}
	return 0;
}


// from mInv.c
/*
static int
inv(ARMat *dest, ARMat *source)
{
	if(dup(dest, source) < 0) return -1;

	return selfInv(dest);
}
*/

// from mMul.c
static int
mul(ARMat *dest, ARMat *a, ARMat *b)
{
	int r, c, i;

	if(a->clm != b->row || dest->row != a->row || dest->clm != b->clm) return -1;

	for(r = 0; r < dest->row; r++) {
		for(c = 0; c < dest->clm; c++) {
			ARELEM0(dest, r, c) = 0.0;
			for(i = 0; i < a->clm; i++) {
				ARELEM0(dest, r, c) += ARELEM0(a, r, i) * ARELEM0(b, i, c);
			}
		}
	}

	return 0;
}


// from mSelfInv.c
static int
selfInv(ARMat *m)
{
	if(minv(m->m, m->row, m->row) == NULL) return -1;

	return 0;
}


// from mTrans.c
/*
static int
trans(ARMat *dest, ARMat *source)
{
	int r, c;

	if(dest->row != source->clm || dest->clm != source->row) return -1;

	for(r = 0; r < dest->row; r++) {
		for(c = 0; c < dest->clm; c++) {
			ARELEM0(dest, r, c) = ARELEM0(source, c, r);
		}
	}

	return 0;
}
*/

// from mUnit.c
/*
static int
unit(ARMat *unit)
{
	int r, c;

	if(unit->row != unit->clm) return -1;

	for(r = 0; r < unit->row; r++) {
		for(c = 0; c < unit->clm; c++) {
			if(r == c) {
				ARELEM0(unit, r, c) = 1.0;
			}
			else {
				ARELEM0(unit, r, c) = 0.0;
			}
		}
	}

	return 0;
}
*/

/****************************************************************
 *
 *                  INTERNAL HELPER FUNCTIONS
 *
 ***************************************************************/

// from mDet.c  -- helper function for arMatrixDet()
#if 0
static ARFloat
mdet(ARFloat *ap, int dimen, int rowa)
/*  ARFloat  *ap;          input matrix */
/*  int     dimen;        Dimension of linre and row, those must be equal,
                          that is square matrix.       */
/*  int     rowa;         ROW Dimension of matrix A    */
{
    ARFloat det = 1.0;
    ARFloat work;
    int    is = 0;
    int    mmax;
    int    i, j, k;

    for(k = 0; k < dimen - 1; k++) {
        mmax = k;
        for(i = k + 1; i < dimen; i++)
            if (fabs(MATRIX(ap, i, k, rowa)) > fabs(MATRIX(ap, mmax, k, rowa)))
                mmax = i;
        if(mmax != k) {
            for (j = k; j < dimen; j++) {
                work = MATRIX(ap, k, j, rowa);
                MATRIX(ap, k, j, rowa) = MATRIX(ap, mmax, j, rowa);
                MATRIX(ap, mmax, j, rowa) = work;
            }
            is++;
        }
        for(i = k + 1; i < dimen; i++) {
            work = MATRIX(ap, i, k, rowa) / MATRIX(ap, k, k, rowa);
            for (j = k + 1; j < dimen; j++)
                MATRIX(ap, i, j, rowa) -= work * MATRIX(ap, k, j, rowa);
        }
    }
    for(i = 0; i < dimen; i++)
        det *= MATRIX(ap, i, i, rowa);
    for(i = 0; i < is; i++) 
        det *= -1.0;
    return(det);
}
#endif

// from mSelfInv.c -- MATRIX inverse function
static ARFloat*
minv( ARFloat *ap, int dimen, int rowa )
{
        ARFloat *wap, *wcp, *wbp;/* work pointer                 */
        int i,j,n,ip=0,nwork;
        int nos[50];
        ARFloat epsl;
        ARFloat p,pbuf,work;
        //ARFloat  fabs();

        epsl = (ARFloat)1.0e-10;         /* Threshold value      */

        switch (dimen) {
                case (0): return(NULL);                 /* check size */
                case (1): *ap = (ARFloat)1.0 / (*ap);
                          return(ap);                   /* 1 dimension */
        }

        for(n = 0; n < dimen ; n++)
                nos[n] = n;

        for(n = 0; n < dimen ; n++) {
                wcp = ap + n * rowa;

                for(i = n, wap = wcp, p = 0.0; i < dimen ; i++, wap += rowa)
                        if( p < ( pbuf = (ARFloat)fabs(*wap)) ) {
                                p = pbuf;
                                ip = i;
                        }
                if (p <= epsl)
                        return(NULL);

                nwork = nos[ip];
                nos[ip] = nos[n];
                nos[n] = nwork;

                for(j = 0, wap = ap + ip * rowa, wbp = wcp; j < dimen ; j++) {
                        work = *wap;
                        *wap++ = *wbp;
                        *wbp++ = work;
                }

                for(j = 1, wap = wcp, work = *wcp; j < dimen ; j++, wap++)
                        *wap = *(wap + 1) / work;
                *wap = (ARFloat)1.0 / work;

                for(i = 0; i < dimen ; i++) {
                        if(i != n) {
                                wap = ap + i * rowa;
                                for(j = 1, wbp = wcp, work = *wap;
                                                j < dimen ; j++, wap++, wbp++)
                                        *wap = *(wap + 1) - work * (*wbp);
                                *wap = -work * (*wbp);
                        }
                }
        }

        for(n = 0; n < dimen ; n++) {
                for(j = n; j < dimen ; j++)
                        if( nos[j] == n) break;
                nos[j] = nos[n];
                for(i = 0, wap = ap + j, wbp = ap + n; i < dimen ;
                                        i++, wap += rowa, wbp += rowa) {
                        work = *wap;
                        *wap = *wbp;
                        *wbp = work;
                }
        }
        return(ap);
}


}  // namespace Matrix


}  // namespace ARToolKitPlus
