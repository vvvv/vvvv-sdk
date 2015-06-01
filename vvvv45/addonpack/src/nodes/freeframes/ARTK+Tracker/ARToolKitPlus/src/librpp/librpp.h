/* ========================================================================
 * PROJECT: ARToolKitPlus
 * ========================================================================
 *
 * The robust pose estimator algorithm has been provided by G. Schweighofer
 * and A. Pinz (Inst.of El.Measurement and Measurement Signal Processing,
 * Graz University of Technology). Details about the algorithm are given in
 * a Technical Report: TR-EMT-2005-01, available at:
 * http://www.emt.tu-graz.ac.at/publications/index.htm
 *
 * Ported from MATLAB to C by T.Pintaric (Vienna University of Technology).
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
 * $Id: librpp.h 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */

#if defined(_MSC_VER) || defined(_WIN32_WCE)

#ifdef LIBRPP_STATIC
#  define LIBRPP_API
#elif LIBRPP_DLL
#  ifdef LIBRPP_EXPORTS
#    define LIBRPP_API __declspec(dllexport)
#  else
#    define LIBRPP_API __declspec(dllimport)
#  endif
#else
#  pragma error ("please define either LIBRPP_STATIC or LIBRPP_DLL")
#endif

#else
// for linux
#define LIBRPP_API

#endif

typedef double rpp_float;
typedef double rpp_vec[3];
typedef double rpp_mat[3][3];

LIBRPP_API void robustPlanarPose(rpp_float &err,
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
/*

	[OUTPUT]

	err: squared reprojection error
	R:   rotation matrix (iprts[n] = R*model[n]+t)
	t:   translation vector (iprts[n] = R*model[n]+t)

	[INPUT]
    
	cc:    camera's principal point [x,y]
	fc:    camera's focal length    [x,y]
	model: 3d points [x,y,z]
	iprts: 2d projections [x,y,1]
    
    model_iprts_size:   number of 2d/3d point correspondences
    R_init:             initial estimate of the rotation matrix R
    estimate_R_init:    when true, the estimate in R_init is ignored
    epsilon*:           see below (default: 1E-8)
    tolerance*:         see below (default: 1E-5)
	max_iterations*:    max. number of iterations (0 = infinite)
    
    *) the following code fragment illustrates the use of epsilon,
       tolerance and max_iterations:

    while((ABS(( old_err - new_err ) / old_err) > tolerance) && 
          ( new_err > epsilon ) &&
		  ( max_iterations == 0 || iterations < max_iterations ))
    {
        NEW ITERATION
    }
          
*/
