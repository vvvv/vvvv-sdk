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
 ** @author   Thomas Pintaric
 *
 * $Id: CameraImpl.cxx 172 2006-07-25 14:05:47Z daniel $
 * @file
 * ======================================================================== */


#include <stdlib.h>
#include <stdio.h>
#include <math.h>

#include <ARToolKitPlus/CameraImpl.h>
#include <ARToolKitPlus/byteSwap.h>


namespace ARToolKitPlus {

#define PD_LOOP 3

CameraImpl::CameraImpl()
{}

CameraImpl::~CameraImpl()
{}

bool CameraImpl::
loadFromFile(const char* filename)
{
	FILE           *fp;
	ARParamDouble  param;

	fp = fopen( filename, "rb" );
	if( fp == NULL ) return(false);

	setFileName(filename);

	if(fread(&param, sizeof(ARParamDouble), 1, fp ) != 1 ) 
	{
		fclose(fp);
		return(false);
	}

#ifdef AR_LITTLE_ENDIAN
	byteswap( &param );
#endif

	fclose(fp);

	unsigned int i,j;
	for(i=0; i<4; i++) this->dist_factor[i] = (ARFloat) param.dist_factor[i];
	this->xsize = (int) param.xsize;
	this->ysize = (int) param.ysize;
	for(i=0; i<3; i++)
		for(j=0; j<4; j++)
			this->mat[i][j] = (ARFloat) param.mat[i][j];

	if( (mat[0][1] != 0.0) ||
		(mat[0][3] != 0.0) ||
		(mat[1][0] != 0.0) ||
		(mat[1][3] != 0.0) ||
		(mat[2][0] != 0.0) ||
		(mat[2][1] != 0.0) ||
		(mat[2][2] != 1.0) ||
		(mat[2][3] != 0.0)) return(false);

	return(true);
}



void CameraImpl::
observ2Ideal(ARFloat ox, ARFloat oy, ARFloat *ix, ARFloat *iy)
{
	ARFloat  z02, z0, p, q, z, px, py;
	int     i;

	px = ox - this->dist_factor[0];
	py = oy - this->dist_factor[1];
	p = this->dist_factor[2]/(ARFloat)100000000.0;
	z02 = px*px+ py*py;
	q = z0 = (ARFloat)sqrt(px*px+ py*py);

	for( i = 1; ; i++ ) {
		if( z0 != 0.0 ) {
			z = z0 - (((ARFloat)1.0 - p*z02)*z0 - q) / ((ARFloat)1.0 - (ARFloat)3.0*p*z02);
			px = px * z / z0;
			py = py * z / z0;
		}
		else {
			px = 0.0;
			py = 0.0;
			break;
		}
		if( i == PD_LOOP ) break;

		z02 = px*px+ py*py;
		z0 = (ARFloat)sqrt(px*px+ py*py);
	}

	*ix = px / this->dist_factor[3] + this->dist_factor[0];
	*iy = py / this->dist_factor[3] + this->dist_factor[1];
}

void CameraImpl::
ideal2Observ(ARFloat ix, ARFloat iy, ARFloat *ox, ARFloat *oy)
{
	ARFloat    x, y, d;

	x = (ix - this->dist_factor[0]) * this->dist_factor[3];
	y = (iy - this->dist_factor[1]) * this->dist_factor[3];
	if( x == 0.0 && y == 0.0 ) {
		*ox = this->dist_factor[0];
		*oy = this->dist_factor[1];
	}
	else {
		d = (ARFloat)1.0 - this->dist_factor[2]/(ARFloat)100000000.0 * (x*x+y*y);
		*ox = x * d + this->dist_factor[0];
		*oy = y * d + this->dist_factor[1];
	}
}

Camera* CameraImpl::clone()
{
	CameraImpl* pCam = new CameraImpl();
	pCam->xsize = xsize;
	pCam->ysize = ysize;
	unsigned int i,j;
	for(i=0; i<3; i++) for(j=0; j<4; j++) pCam->mat[i][j] = mat[i][j];
	for(i=0; i<4; i++) pCam->dist_factor[i] = dist_factor[i];
	return((Camera*)pCam);
}

bool CameraImpl::
changeFrameSize(const int frameWidth, const int frameHeight)
{
	if(frameWidth <=0 || frameHeight <=0) return(false);
	const ARFloat scale = (ARFloat)frameWidth / (ARFloat)xsize;
	xsize = frameWidth;
	ysize = frameHeight;

	for(int i = 0; i < 4; i++ )
	{
		mat[0][i] *= scale;
		mat[1][i] *= scale;
	}

	dist_factor[0] *= scale;
	dist_factor[1] *= scale;
	dist_factor[2] /= (scale*scale);

	return(true);
}

void CameraImpl::
logSettings(Logger* logger)
{
	if(logger != NULL)
	{
		logger->artLogEx("ARToolKitPlus: CamSize %d , %d\n", xsize, ysize);
		logger->artLogEx("ARToolKitPlus: Dist.Factor %.2f %.2f %.2f %.2f\n", dist_factor[0],
			dist_factor[1], dist_factor[2], dist_factor[3] );
	}
}


}  // namespace ARToolKitPlus
