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
 * $Id: rppGetTransMat.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdlib.h>
#include <math.h>

#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/matrix.h>
#include <ARToolKitPlus/extra/rpp.h>


namespace ARToolKitPlus {

AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::rppGetTransMat(ARMarkerInfo *marker_info, ARFloat center[2], ARFloat width, ARFloat conv[3][4])
{
	const bool initial_estimate_with_arGetInitRot = false; // only for testing
	
	rpp_float err = 1e+20;
	rpp_mat R, R_init;
	rpp_vec t;

	if(initial_estimate_with_arGetInitRot )
	{
		ARFloat  rot[3][3];
		if( arGetInitRot( marker_info, arCamera->mat, rot ) < 0 ) return -1;
		for(int i=0; i<3; i++)
			for(int j=0; j<3; j++)
				R_init[i][j] = (rpp_float)rot[i][j];
	}

	int dir = marker_info->dir;
	rpp_vec ppos2d[4];
	rpp_vec ppos3d[4];
	const unsigned int n_pts = 4;
	const rpp_float model_z =  0;
	const rpp_float iprts_z =  1;

	ppos2d[0][0] = marker_info->vertex[(4-dir)%4][0];
    ppos2d[0][1] = marker_info->vertex[(4-dir)%4][1];
	ppos2d[0][2] = iprts_z;
    ppos2d[1][0] = marker_info->vertex[(5-dir)%4][0];
    ppos2d[1][1] = marker_info->vertex[(5-dir)%4][1];
	ppos2d[1][2] = iprts_z;
    ppos2d[2][0] = marker_info->vertex[(6-dir)%4][0];
    ppos2d[2][1] = marker_info->vertex[(6-dir)%4][1];
	ppos2d[2][2] = iprts_z;
    ppos2d[3][0] = marker_info->vertex[(7-dir)%4][0];
    ppos2d[3][1] = marker_info->vertex[(7-dir)%4][1];
	ppos2d[3][2] = iprts_z;

	ppos3d[0][0] = center[0] - width*(ARFloat)0.5;
    ppos3d[0][1] = center[1] + width*(ARFloat)0.5;
	ppos3d[0][2] = model_z;
    ppos3d[1][0] = center[0] + width*(ARFloat)0.5;
    ppos3d[1][1] = center[1] + width*(ARFloat)0.5;
	ppos3d[1][2] = model_z;
    ppos3d[2][0] = center[0] + width*(ARFloat)0.5;
    ppos3d[2][1] = center[1] - width*(ARFloat)0.5;
	ppos3d[2][2] = model_z;
    ppos3d[3][0] = center[0] - width*(ARFloat)0.5;
    ppos3d[3][1] = center[1] - width*(ARFloat)0.5;
	ppos3d[3][2] = model_z;

	const rpp_float cc[2] = {arCamera->mat[0][2],arCamera->mat[1][2]};
	const rpp_float fc[2] = {arCamera->mat[0][0],arCamera->mat[1][1]};

	robustPlanarPose(err,R,t,cc,fc,ppos3d,ppos2d,n_pts,R_init, !initial_estimate_with_arGetInitRot,0,0,0);

	for(int i=0; i<3; i++)
	{
		conv[i][3] = (ARFloat)t[i];
		for(int j=0; j<3; j++)
			conv[i][j] = (ARFloat)R[i][j];
	}

	if(err > 1e+10) return(-1); // an actual error has occurred in robustPlanarPose()
	return(ARFloat(err)); // NOTE: err is a real number from the interval [0,1e+10]
}

}  // namespace ARToolKitPlus
