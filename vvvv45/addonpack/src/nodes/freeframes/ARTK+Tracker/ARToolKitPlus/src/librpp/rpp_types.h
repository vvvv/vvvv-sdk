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
 * $Id: rpp_types.h 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */



#ifndef __RPP_TYPES_H__
#define __RPP_TYPES_H__

#include <vector>
#include "rpp_const.h"

//
// _USE_CUSTOMFLOAT_ is defined in rpp_const.h
//

#ifdef _USE_CUSTOMFLOAT_
#  include "../math/artkpFloat_float.h"
#endif

namespace rpp {

#ifdef _USE_CUSTOMFLOAT_
  typedef artkpFloat_float<float> real_t;
#else
  typedef double real_t;
#endif

// standard types
//
typedef real_t vec3[3];
struct vec3_t { vec3 v; };
typedef std::vector<vec3_t> vec3_array;
typedef std::vector<vec3_t>::iterator vec3_array_iter;
typedef std::vector<vec3_t>::const_iterator vec3_array_const_iter;

typedef real_t mat33[3][3];
struct mat33_t { mat33 m; };
typedef std::vector<mat33_t> mat33_array;
typedef std::vector<mat33_t>::iterator mat33_array_iter;
typedef std::vector<mat33_t>::const_iterator mat33_array_const_iter;

typedef std::vector<real_t> scalar_array;



struct pose_t
{
	mat33_t R;
	vec3_t  t;
	real_t  E;
	mat33_t PoseLu_R;
	vec3_t  PoseLu_t;
	real_t  obj_err;
};

typedef std::vector<pose_t> pose_vec;

struct quat_t
{
	vec3_t v;
	real_t  s;
};

struct options_t
{
	mat33_t initR;
	real_t tol;
	real_t epsilon;
	unsigned int max_iter;
};

} // namespace rpp
#endif
