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
* $Id$
* @file
 * ======================================================================== */


#include <math.h>
#include <assert.h>
#include <stdio.h>

#include "FixedPoint.h"


namespace ARToolKitPlus {


#ifdef _USE_GENERIC_TRIGONOMETRIC_


// Select LUT size wisely: the number of bits chosen
// for SIN_LUT_SIZEBITS will quite closely resemble 
// the number of correct bits look-ups will have...
// Memory footprint: 4*(2<<SIN_LUT_SIZEBITS) bytes
//
#define SIN_LUT_SIZEBITS 12						// maximum error for sin/cos for SIN_LUT_SIZEBITS=12: 0.000383
#define SIN_LUT_SIZE (1<<SIN_LUT_SIZEBITS)
#define SIN_LUT_BITS 28

#define DOUBLE_PI 3.1415926535897932384626433832795
#define DOUBLE_PI_OVER_2 (DOUBLE_PI/2.0)
#define DOUBLE_3_PI_OVER_2 (3.0*DOUBLE_PI/2.0)
#define DOUBLE_2_PI (2.0*DOUBLE_PI)
#define DOUBLE_2_OVER_PI (2.0/DOUBLE_PI)

#define FIXED28_PI FIXED_Float_To_Fixed_n(DOUBLE_PI, 28)
#define FIXED28_PI_OVER_2 FIXED_Float_To_Fixed_n(DOUBLE_PI_OVER_2, 28)
#define FIXED28_3_PI_OVER_2 FIXED_Float_To_Fixed_n(DOUBLE_3_PI_OVER_2, 28)
#define FIXED28_2_PI FIXED_Float_To_Fixed_n(DOUBLE_2_PI, 28)
#define FIXED28_2_OVER_PI FIXED_Float_To_Fixed_n(DOUBLE_2_OVER_PI, 28)


static I32 *sinLUT_28 = 0;


static I32*
createSinLUT(int nSize, int nBits)
{
	I32* lut = new I32[nSize];

	for(int i=0; i<nSize; i++)
	{
		double phi = i*DOUBLE_PI_OVER_2/nSize;
		double sinPhi = sin(phi);

		I32 sinPhiFixed = FIXED_Float_To_Fixed_n(sinPhi, nBits);
		lut[i] = sinPhiFixed;
	}

	return lut;
}


inline void
Fixed28_SinCos(I32 phi, I32 &sin, I32 &cos)
{
	I32 quadrant, i;
	bool negative = false;

	if(phi < 0)
	{
		negative = true;
		phi = -phi;
	}

	phi %= FIXED28_2_PI;				// modulo clamps to range 0 to 2*Pi

	if(phi<FIXED28_PI_OVER_2)
		quadrant = 0;
	else if(phi<FIXED28_PI)
	{
		quadrant = 1;
		phi=FIXED28_PI-phi;
	}
	else if(phi<FIXED28_3_PI_OVER_2)
	{
		quadrant = 2;
		phi=phi-FIXED28_PI;
	}
	else
	{
		quadrant = 3;
		phi=FIXED28_2_PI-phi;
	}

	// scale from [0..1<<28] to [0..SIN_LUT_SIZE-1]
	FIXED_MUL2(phi, FIXED28_2_OVER_PI, i, SIN_LUT_BITS);
	i>>=(SIN_LUT_BITS-SIN_LUT_SIZEBITS);

	assert(i<SIN_LUT_SIZE);

	sin = sinLUT_28[i];
	cos = sinLUT_28[SIN_LUT_SIZE-1-i];

	switch(quadrant)
	{
	case 0:
		break;
	case 1:
		cos=-cos;
		break;
	case 2:
		sin=-sin;
		cos=-cos;
		break;
	case 3:
		sin=-sin;
		break;	
	}

	if(negative)
		sin = -sin;
}


void
checkPrecisionSinCos()
{
	double maxErrorSin=0.0, maxErrorCos=0.0;

	for(double phi=-5.0; phi<5.0; phi+=0.0001)
	{
		I32 fixedPhi = FIXED_Float_To_Fixed_n(phi, SIN_LUT_BITS);
		I32 fixedSin,fixedCos;

		Fixed28_SinCos(fixedPhi, fixedSin,fixedCos);

		double	doubleSin=sin(phi),doubleCos=cos(phi);
		double	doubleSinF=FIXED_Fixed_n_To_Float(fixedSin, SIN_LUT_BITS),
				doubleCosF=FIXED_Fixed_n_To_Float(fixedCos, SIN_LUT_BITS);

		double	diffSin = fabs(doubleSin-doubleSinF),
				diffCos = fabs(doubleCos-doubleCosF);

		if(diffSin>maxErrorSin)
			maxErrorSin=diffSin;

		if(diffCos>maxErrorCos)
			maxErrorCos=diffCos;
	}

	printf("Maximum error for sin() and cos(): %f  %f\n", (float)maxErrorSin, (float)maxErrorCos);
}


void
Fixed28_Init()
{
	if(!sinLUT_28)
		sinLUT_28 = createSinLUT(SIN_LUT_SIZE, SIN_LUT_BITS);
}


void
Fixed28_Deinit()
{
	delete sinLUT_28;
}



#endif //_USE_GENERIC_TRIGONOMETRIC_


}  // namespace ARToolKitPlus
