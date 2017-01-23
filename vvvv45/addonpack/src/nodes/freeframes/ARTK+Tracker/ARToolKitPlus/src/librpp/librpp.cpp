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
 * $Id: librpp.cpp 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include "librpp.h"
#include "rpp.h"
#include "rpp_vecmat.h"
using namespace rpp;

#ifdef LIBRPP_DLL
BOOL APIENTRY DllMain( HANDLE hModule, 
					DWORD  ul_reason_for_call, 
					LPVOID lpReserved
					)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}
#endif //LIBRPP_DLL


#ifndef _NO_LIBRPP_


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
								 const unsigned int max_iterations)
{
	vec3_array _model;
	vec3_array _iprts;
	_model.resize(model_iprts_size);
	_iprts.resize(model_iprts_size);

	mat33_t K, K_inv;
	mat33_eye(K);
	K.m[0][0] = (real_t)fc[0];
	K.m[1][1] = (real_t)fc[1];
	K.m[0][2] = (real_t)cc[0];
	K.m[1][2] = (real_t)cc[1];

	mat33_inv(K_inv, K);

	for(unsigned int i=0; i<model_iprts_size; i++)
	{
		vec3_t _v,_v2;
		vec3_assign(_v,(real_t)model[i][0],(real_t)model[i][1],(real_t)model[i][2]);
		_model[i] = _v;
		vec3_assign(_v,(real_t)iprts[i][0],(real_t)iprts[i][1],(real_t)iprts[i][2]);
		vec3_mult(_v2,K_inv,_v);
		_iprts[i] = _v2;
	}

	options_t options;
	options.max_iter = max_iterations;
	options.epsilon = (real_t)(epsilon == 0 ? DEFAULT_EPSILON : epsilon);
	options.tol =     (real_t)(tolerance == 0 ? DEFAULT_TOL : tolerance);
	if(estimate_R_init)
		mat33_set_all_zeros(options.initR);
	else
	{
		mat33_assign(options.initR,
					(real_t)R_init[0][0], (real_t)R_init[0][1], (real_t)R_init[0][2],
					(real_t)R_init[1][0], (real_t)R_init[1][1], (real_t)R_init[1][2],
					(real_t)R_init[2][0], (real_t)R_init[2][1], (real_t)R_init[2][2]);
	}

	real_t _err;
	mat33_t _R;
	vec3_t _t;

	robust_pose(_err,_R,_t,_model,_iprts,options);

	for(int j=0; j<3; j++)
	{
		R[j][0] = (rpp_float)_R.m[j][0];
		R[j][1] = (rpp_float)_R.m[j][1];
		R[j][2] = (rpp_float)_R.m[j][2];
		t[j] = (rpp_float)_t.v[j];
	}
	err = (rpp_float)_err;
}


bool rppSupportAvailabe()
{
	return true;
}


#else //_NO_LIBRPP_


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
								 const unsigned int max_iterations)
{
}


bool rppSupportAvailabe()
{
	return false;
}

#endif //_NO_LIBRPP_
