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
* $Id$
* @file
* ======================================================================== */


#ifndef __LINK_WITH_RPP__
#define __LINK_WITH_RPP__

typedef double rpp_float;
typedef double rpp_vec[3];
typedef double rpp_mat[3][3];


void 
robustPlanarPose(rpp_float &err,
				 rpp_mat &R,
				 rpp_vec &t,
				 const rpp_float cc[2],
				 const rpp_float fc[2],
				 const rpp_vec *model,
				 const rpp_vec *iprts,
				 const unsigned int model_iprts_size,
				 const rpp_mat R_init,
				 const bool estimate_R_init,
				 const rpp_float epsilon,
				 const rpp_float tolerance,
				 const unsigned int max_iterations);


bool rppSupportAvailabe();


#endif // __LINK_WITH_RPP__
