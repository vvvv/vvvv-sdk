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
 * $Id: rpp.cpp 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#ifndef _NO_LIBRPP_


#include <vector>
#include "assert.h"

#include "rpp.h"
#include "rpp_const.h"
#include "rpp_vecmat.h"

namespace rpp {

// ===========================================================================================
void Quaternion_byAngleAndVector(quat_t &Q, const real_t &q_angle, const vec3_t &q_vector)
{
	vec3_t rotation_axis;
	normRv(rotation_axis, q_vector);
	real_t f = _sin(q_angle/2.0f);
	vec3_copy(Q.v,rotation_axis);
	vec3_mult(Q.v,f);
	Q.s = _cos(q_angle/2.0f);
	quat_mult(Q, 1.0f / quat_norm(Q));
}

// ===========================================================================================

void GetRotationbyVector(mat33_t &R, const vec3_t &v1, const vec3_t &v2)
{
	real_t winkel = _acos(vec3_dot(v1,v2));
	quat_t QU;
	vec3_t vc;
	vec3_cross(vc,v2,v1);
	Quaternion_byAngleAndVector(QU,winkel,vc);
	mat33_from_quat(R,QU);
}

// ===========================================================================================
void xform(vec3_array &Q, const vec3_array &P, const mat33_t &R, const vec3_t &t)
{
	const unsigned int n = (unsigned int) P.size();
	for(unsigned int i=0; i<n; i++)
	{
		vec3_mult(Q.at(i),R,P.at(i));
		vec3_add(Q.at(i),t);
	}
}
// ===========================================================================================

void xformproj(vec3_array &Qp, const vec3_array &P, const mat33_t &R, const vec3_t &t)
{
	const unsigned int n = (unsigned int) P.size();
	vec3_array Q;
	Q.resize(n);

	for(unsigned int i=0; i<n; i++)
	{
		vec3_mult(Q.at(i),R,P.at(i));
		vec3_add(Q.at(i),t);
		Qp.at(i).v[0] = Q.at(i).v[0] / Q.at(i).v[2]; 
		Qp.at(i).v[1] = Q.at(i).v[1] / Q.at(i).v[2]; 
		Qp.at(i).v[2] = 1.0;
	}
}

// ===========================================================================================
void rpyMat(mat33_t &R, const vec3_t &rpy) // rpy: roll,pitch,yaw
{
	const real_t cosA = _cos(rpy.v[2]);
	const real_t sinA = _sin(rpy.v[2]);
	const real_t cosB = _cos(rpy.v[1]);
	const real_t sinB = _sin(rpy.v[1]);
	const real_t cosC = _cos(rpy.v[0]);
	const real_t sinC = _sin(rpy.v[0]);
	const real_t cosAsinB = cosA * sinB;
	const real_t sinAsinB = sinA * sinB;

	R.m[0][0] = cosA*cosB;
	R.m[0][1] = cosAsinB*sinC-sinA*cosC;
	R.m[0][2] = cosAsinB*cosC+sinA*sinC;

	R.m[1][0] = sinA*cosB;
	R.m[1][1] = sinAsinB*sinC+cosA*cosC;
	R.m[1][2] = sinAsinB*cosC-cosA*sinC;

	R.m[2][0] = -sinB;
	R.m[2][1] = cosB*sinC;
	R.m[2][2] = cosB*cosC;
}
// ===========================================================================================
void rpyAng(vec3_t &angs, const mat33_t &R)
{
	const real_t sinB = -(R.m[2][0]);
	const real_t cosB = _sqrt(R.m[0][0]*R.m[0][0] + R.m[1][0]*R.m[1][0]);

	if(_abs(cosB) > real_t(1E-15))
	{
		const real_t sinA = R.m[1][0] / cosB;
		const real_t cosA = R.m[0][0] / cosB;
		const real_t sinC = R.m[2][1] / cosB;
		const real_t cosC = R.m[2][2] / cosB;
		vec3_assign(angs,_atan2(sinC,cosC),_atan2(sinB,cosB),_atan2(sinA,cosA));
	}
	else
	{
		const real_t sinC = (R.m[0][1] - R.m[1][2]) / 2.0f;
		const real_t cosC = (R.m[1][1] - R.m[0][2]) / 2.0f;
		vec3_assign(angs,_atan2(sinC,cosC),CONST_PI_OVER_2, 0.0f);
	}
}

// ===========================================================================================

void rpyAng_X(vec3_t &ang_zyx, const mat33_t &R)
{
	rpyAng(ang_zyx,R);

	if(_abs(ang_zyx.v[0]) > CONST_PI_OVER_2)
	{
		while(_abs(ang_zyx.v[0]) > CONST_PI_OVER_2)
		{
			if(ang_zyx.v[0] > 0)
			{
				vec3_assign(ang_zyx, ang_zyx.v[0]+CONST_PI,
					                 3*CONST_PI-ang_zyx.v[1],
									 ang_zyx.v[2]+CONST_PI);
				vec3_sub(ang_zyx,CONST_2_PI);
			}
			else
			{
				vec3_assign(ang_zyx, ang_zyx.v[0]+CONST_PI,
									 3*CONST_PI-ang_zyx.v[1],
									 ang_zyx.v[2]+CONST_PI);
			}
		}
	}
}

// ===========================================================================================
void decomposeR(mat33_t &Rz, const mat33_t &R)
{
	real_t cl = _atan2(R.m[2][1],R.m[2][0]);
	vec3_t rpy;
	vec3_assign(rpy,0,0,cl);
	rpyMat(Rz,rpy);
}

// ===========================================================================================
void abskernel(mat33_t &R, vec3_t &t, vec3_array &Qout, real_t &err2, 
			   const vec3_array _P, const vec3_array _Q, 
			   const mat33_array F, const mat33_t G)

{
	unsigned i,j;

	vec3_array P(_P.begin(),_P.end());
	vec3_array Q(_Q.begin(),_Q.end());
	const unsigned int n = (unsigned int) P.size();

	for(i=0; i<n; i++)
	{
		vec3_t _q;
		vec3_mult(_q,F.at(i),_Q.at(i));
		vec3_copy(Q.at(i),_q);
	}

	vec3_t pbar;
	vec3_array_sum(pbar,P);
	vec3_div(pbar,real_t(n));
	vec3_array_sub(P,pbar);

	vec3_t qbar;
	vec3_array_sum(qbar,Q);
	vec3_div(qbar,real_t(n));
	vec3_array_sub(Q,qbar);

	mat33_t M;
	mat33_clear(M);
	for(j=0; j<n; j++)
	{
		mat33_t _m;
		vec3_mul_vec3trans(_m,P.at(j),Q.at(j));
		mat33_add(M,_m);
	}

	mat33_t _U;
	mat33_t _S;
	mat33_t _V;
	mat33_clear(_U);
	mat33_clear(_S);
	mat33_clear(_V);
	mat33_svd2(_U,_S,_V,M);

	mat33_t _Ut;
	mat33_transpose(_Ut,_U);
	mat33_mult(R,_V,_Ut);

	vec3_t _sum;
	vec3_clear(_sum);
	for(i=0; i<n; i++)
	{
		vec3_t _v1,_v2;
		vec3_mult(_v1,R,P.at(i));
		vec3_mult(_v2,F.at(i),_v1);
		vec3_add(_sum,_v2);
	}

	vec3_mult(t,G,_sum);
	xform(Qout,P,R,t);
	err2 = 0;
	for(i=0; i<n; i++)
	{
		mat33_t _m1;
		vec3_t _v1;
		mat33_eye(_m1);
		mat33_sub(_m1,F.at(i));
		vec3_mult(_v1,_m1,Qout.at(i));
		err2 += vec3_dot(_v1,_v1);
	}
}
// ===========================================================================================

void objpose(mat33_t &R, vec3_t &t, unsigned int &it, real_t &obj_err, real_t &img_err,
			 bool calc_img_err, const vec3_array _P, const vec3_array Qp, const options_t options)
{
	unsigned int i,j;
	vec3_array P(_P.begin(),_P.end());

	const unsigned int n = (unsigned int) P.size();
	vec3_t pbar;
	vec3_array_sum(pbar,P);
	vec3_div(pbar,real_t(n));
	vec3_array_sub(P,pbar);
	vec3_array Q(Qp.begin(),Qp.end());
	vec3_t ones;
	ones.v[0] = 1;
	ones.v[1] = 1;
	ones.v[2] = 1;
	const bool mask_z[3] = {0,0,1};
	vec3_array_set(Q,ones,mask_z);
	mat33_array F;
	F.resize(n);
	vec3_t V;
	for(i=0; i<n; i++)
	{
		V.v[0] = Q.at(i).v[0] / Q.at(i).v[2];
		V.v[1] = Q.at(i).v[1] / Q.at(i).v[2];
		V.v[2] = 1.0;
		mat33_t _m;
		vec3_mul_vec3trans(_m,V,V);
		mat33_div(_m,vec3trans_mul_vec3(V,V));
		F.at(i) = _m;
	}

	mat33_t tFactor;
	{
		mat33_t _m1,_m2,_m3;
		mat33_eye(_m1);
		mat33_array_sum(_m2,F);
		mat33_div(_m2,real_t(n));
		mat33_sub(_m3,_m1,_m2);
		mat33_inv(tFactor,_m3);
		mat33_div(tFactor,real_t(n));
	}

	it = 0;
	bool initR_approximate = mat33_all_zeros(options.initR);
	mat33_t Ri;
	vec3_t ti;
	vec3_array Qi; Qi.resize(n);
	real_t old_err, new_err;

	// ----------------------------------------------------------------------------------------
	if(!initR_approximate)
	{
		mat33_copy(Ri,options.initR);
		vec3_t _sum;
		vec3_clear(_sum);
		for(j=0; j<n; j++)
		{
			vec3_t _v1, _v2;
			mat33_t _m1,_m2;
			mat33_eye(_m1);              
			mat33_sub(_m2,F.at(j),_m1);
			vec3_mult(_v1,Ri,P.at(j));
			vec3_mult(_v2,_m2,_v1);
			vec3_add(_sum,_v2);
		}
		vec3_mult(ti,tFactor,_sum);
		xform(Qi,P,Ri,ti);
		old_err = 0;
		vec3_t _v;
		for(j=0; j<n; j++)
		{
			mat33_t _m1,_m2;
			mat33_eye(_m1);
			mat33_sub(_m2,F.at(j),_m1);
			vec3_mult(_v,_m2,Qi.at(j));
			old_err += vec3_dot(_v,_v);
		}
	// ----------------------------------------------------------------------------------------
	}
	else
	{
		abskernel(Ri,ti,Qi,old_err,P,Q,F,tFactor);
		it = 1;
	}
	// ----------------------------------------------------------------------------------------

	abskernel(Ri,ti,Qi,new_err,P,Qi,F,tFactor);
	it = it + 1;

	while((_abs((old_err-new_err)/old_err) > options.tol) && (new_err > options.epsilon) &&
		  (options.max_iter == 0 || it<options.max_iter))
	{
		old_err = new_err;
		abskernel(Ri,ti,Qi,new_err,P,Qi,F,tFactor);
		it = it + 1;
	}


	mat33_copy(R,Ri);
	vec3_copy(t,ti);
	obj_err = _sqrt(new_err/real_t(n));

	if(calc_img_err)
	{
		vec3_array Qproj; Qproj.resize(n);
		xformproj(Qproj, P, Ri, ti);
		img_err = 0;

		vec3_t _v;
		for(unsigned int j=0; j<n; j++)
		{
			vec3_sub(_v,Qproj.at(j),Qp.at(j));
			img_err += vec3_dot(_v,_v);
		}
		img_err = _sqrt(img_err/real_t(n));
	}

	if(t.v[2] < 0)
	{
		mat33_mult(R,-1.0);
		vec3_mult(t,-1.0);
	}

	vec3_t _ts;
	vec3_mult(_ts,Ri,pbar);
	vec3_sub(t,_ts);
}

// =====================================================================================

void getRotationY_wrtT(scalar_array &al_ret, vec3_array &tnew, const vec3_array &v,
					   const vec3_array &p, const vec3_t &t, const real_t &DB,
					   const mat33_t &Rz)
{
	unsigned int i,j;
	const unsigned int n = (unsigned int) v.size();
	mat33_array V;
	V.resize(n);
	for(i=0; i<n; i++)
	{
		vec3_mul_vec3trans(V.at(i),v.at(i),v.at(i));
		mat33_div(V.at(i), vec3trans_mul_vec3(v.at(i),v.at(i)));
	}

	mat33_t G, _g1, _g2, _g3;
	mat33_array_sum(_g1,V);
	mat33_eye(_g2);
	mat33_div(_g1,real_t(n));
	mat33_sub(_g3,_g2,_g1);
	mat33_inv(G, _g3);
	mat33_div(G,real_t(n));
	mat33_t _opt_t;
	mat33_clear(_opt_t);

	for(i=0; i<n; i++)
	{
		const real_t v11 = V.at(i).m[0][0]; 
		const real_t v21 = V.at(i).m[1][0];
		const real_t v31 = V.at(i).m[2][0];
		const real_t v12 = V.at(i).m[0][1]; 
		const real_t v22 = V.at(i).m[1][1];
		const real_t v32 = V.at(i).m[2][1];
		const real_t v13 = V.at(i).m[0][2]; 
		const real_t v23 = V.at(i).m[1][2];
		const real_t v33 = V.at(i).m[2][2];
		const real_t px = p.at(i).v[0];
		const real_t py = p.at(i).v[1];
		const real_t pz = p.at(i).v[2];
		const real_t r1 = Rz.m[0][0];
		const real_t r2 = Rz.m[0][1];
		const real_t r3 = Rz.m[0][2];
		const real_t r4 = Rz.m[1][0];
		const real_t r5 = Rz.m[1][1];
		const real_t r6 = Rz.m[1][2];
		const real_t r7 = Rz.m[2][0];
		const real_t r8 = Rz.m[2][1];
		const real_t r9 = Rz.m[2][2];

		mat33_t _o;
		_o.m[0][0] = (((v11-real_t(1))*r2+v12*r5+v13*r8)*py+(-(v11-real_t(1))*r1-v12*r4-v13*r7)*px+(-(v11-real_t(1))*r3-v12*r6-v13*r9)*pz);
		_o.m[0][1] = ((real_t(2)*(v11-real_t(1))*r1+real_t(2)*v12*r4+real_t(2)*v13*r7)*pz+(-real_t(2)*(v11-real_t(1))*r3-real_t(2)*v12*r6-real_t(2)*v13*r9)*px);
		_o.m[0][2] = ((v11-real_t(1))*r1+v12*r4+v13*r7)*px+((v11-real_t(1))*r3+v12*r6+v13*r9)*pz+((v11-real_t(1))*r2+v12*r5+v13*r8)*py;

		_o.m[1][0] = ((v21*r2+(v22-real_t(1))*r5+v23*r8)*py+(-v21*r1-(v22-real_t(1))*r4-v23*r7)*px+(-v21*r3-(v22-real_t(1))*r6-v23*r9)*pz);
		_o.m[1][1] = ((real_t(2)*v21*r1+real_t(2)*(v22-real_t(1))*r4+real_t(2)*v23*r7)*pz+(-real_t(2)*v21*r3-real_t(2)*(v22-real_t(1))*r6-real_t(2)*v23*r9)*px);
		_o.m[1][2] = (v21*r1+(v22-real_t(1))*r4+v23*r7)*px+(v21*r3+(v22-real_t(1))*r6+v23*r9)*pz+(v21*r2+(v22-real_t(1))*r5+v23*r8)*py;

		_o.m[2][0] = ((v31*r2+v32*r5+(v33-real_t(1))*r8)*py+(-v31*r1-v32*r4-(v33-real_t(1))*r7)*px+(-v31*r3-v32*r6-(v33-real_t(1))*r9)*pz);
		_o.m[2][1] = ((real_t(2)*v31*r1+real_t(2)*v32*r4+real_t(2)*(v33-real_t(1))*r7)*pz+(-real_t(2)*v31*r3-real_t(2)*v32*r6-real_t(2)*(v33-real_t(1))*r9)*px);
		_o.m[2][2] = (v31*r1+v32*r4+(v33-real_t(1))*r7)*px+(v31*r3+v32*r6+(v33-real_t(1))*r9)*pz+(v31*r2+v32*r5+(v33-real_t(1))*r8)*py;

		mat33_add(_opt_t,_o);
	}

	mat33_t opt_t;
	mat33_mult(opt_t,G,_opt_t);
	real_t E_2[5] = {0,0,0,0,0};
	for(i=0; i<n; i++)
	{
#if 0	  
		const real_t v11 = V.at(i).m[0][0]; 
		const real_t v21 = V.at(i).m[1][0];
		const real_t v31 = V.at(i).m[2][0];
		const real_t v12 = V.at(i).m[0][1]; 
		const real_t v22 = V.at(i).m[1][1];
		const real_t v32 = V.at(i).m[2][1];
		const real_t v13 = V.at(i).m[0][2]; 
		const real_t v23 = V.at(i).m[1][2];
		const real_t v33 = V.at(i).m[2][2];
#endif
		const real_t px = p.at(i).v[0];
		const real_t py = p.at(i).v[1];
		const real_t pz = p.at(i).v[2];

		mat33_t Rpi;
		mat33_assign(Rpi,-px,real_t(2)*pz,px,py,real_t(0),py,-pz,-real_t(2)*px,pz);

		mat33_t E,_e1,_e2;
		mat33_eye(_e1);
		mat33_sub(_e1,V.at(i));
		mat33_mult(_e2,Rz,Rpi);
		mat33_add(_e2,opt_t);
		mat33_mult(E,_e1,_e2);
		vec3_t e2,e1,e0;
		mat33_to_col_vec3(e2,e1,e0,E);
		vec3_t _E2_0,_E2_1,_E2_2,_E2_3,_E2_4;
		vec3_copy(_E2_0,e2);
		vec3_mult(_E2_0,e2);
		vec3_copy(_E2_1,e1);
		vec3_mult(_E2_1,e2);
		vec3_mult(_E2_1,2.0f);
		vec3_copy(_E2_2,e0);
		vec3_mult(_E2_2,e2);
		vec3_mult(_E2_2,2.0f);
		vec3_t _e1_sq;
		vec3_copy(_e1_sq,e1);
		vec3_mult(_e1_sq,e1);
		vec3_add(_E2_2,_e1_sq);
		vec3_copy(_E2_3,e0);
		vec3_mult(_E2_3,e1);
		vec3_mult(_E2_3,2.0f);
		vec3_copy(_E2_4,e0);
		vec3_mult(_E2_4,e0);
		E_2[0] += vec3_sum(_E2_0);
		E_2[1] += vec3_sum(_E2_1);
		E_2[2] += vec3_sum(_E2_2);
		E_2[3] += vec3_sum(_E2_3);
		E_2[4] += vec3_sum(_E2_4);
	}

	scalar_array _a;
	_a.resize(5);
	_a[4] = -E_2[1];
	_a[3] = real_t(4)*E_2[0] - real_t(2)*E_2[2];
	_a[2] = -real_t(3)*E_2[3] + real_t(3)*E_2[1];
	_a[1] = -real_t(4)*E_2[4] + real_t(2)*E_2[2];
	_a[0] = E_2[3];

	scalar_array at_sol;
	int num_sol = solve_polynomial(at_sol, _a);
	scalar_array e;
	e.resize(num_sol);
	scalar_array_clear(e);
	scalar_array at;
	scalar_array_add(e,_a[0]);
	at.clear();
	at.assign(at_sol.begin(),at_sol.end());
	scalar_array_mult(at,_a[1]);
	scalar_array_add(e,at);

	for(j=2; j<=4; j++)
	{
		at.clear();
		at.assign(at_sol.begin(),at_sol.end());
		scalar_array_pow(at,real_t(j));
		scalar_array_mult(at,_a[j]);
		scalar_array_add(e,at);
	}

	scalar_array at_;
	at_.clear();
	for(i=0; i<at.size(); i++)
	{
		if(_abs(e[i]) < real_t(1e-3)) at_.push_back(at_sol[i]);
	}

	scalar_array p1(at_.begin(),at_.end());
	scalar_array_pow(p1,2);
	scalar_array_add(p1,1);
	scalar_array_pow(p1,3);

	at.clear();
	for(i=0; i<at_.size(); i++)
	{
		if(_abs(p1[i]) > real_t(0.1f)) at.push_back(at_[i]);
	}

	scalar_array sa(at.begin(),at.end());
	scalar_array_mult(sa,2);
	scalar_array _ca1(at.begin(),at.end());
	scalar_array_pow(_ca1,2);
	scalar_array_add(_ca1,1);
	scalar_array ca(at.begin(),at.end());
	scalar_array_pow(ca,2);
	scalar_array_negate(ca);
	scalar_array_add(ca,1);
	scalar_array_div(ca,_ca1);
	scalar_array_div(sa,_ca1);
	scalar_array al;
	scalar_array_atan2(al,sa,ca);
	scalar_array_mult(al,real_t(180./CONST_PI));
	scalar_array _c_tMaxMin;
	_c_tMaxMin.resize(at.size());
	scalar_array_clear(_c_tMaxMin);
	scalar_array_add(_c_tMaxMin,_a[1]);
	scalar_array _at;
	_at.clear();
	_at.assign(at.begin(),at.end());
	scalar_array_mult(_at,_a[2]);
	scalar_array_mult(_at,2);
	scalar_array_add(_c_tMaxMin,_at);

	for(j=3; j<=4; j++)
	{
		_at.clear();
		_at.assign(at.begin(),at.end());
		scalar_array_pow(_at,(real_t)real_t(j)-real_t(1.0f));
		scalar_array_mult(_at,_a[j]);
		scalar_array_mult(_at,real_t(j));
		scalar_array_add(_c_tMaxMin,_at);
	}

	scalar_array tMaxMin(_c_tMaxMin.begin(),_c_tMaxMin.end());
	scalar_array al_;
	al_.clear();
	for(i=0; i<tMaxMin.size(); i++)
	{
		if(tMaxMin.at(i) > 0) al_.push_back(al.at(i));
	}

	tnew.resize(al_.size());
	for(unsigned int a=0; a<al_.size(); a++)
	{
		vec3_t rpy;
		vec3_assign(rpy,real_t(0),real_t(al_[a] * CONST_PI / real_t(180)), real_t(0));
		mat33_t R,Ry_;
		rpyMat(Ry_,rpy);
		mat33_mult(R,Rz,Ry_);
		vec3_t t_opt;
		vec3_clear(t_opt);

		for(i=0; i<n; i++)
		{
			mat33_t _m1,_eye3;
			mat33_eye(_eye3);
			mat33_copy(_m1,V.at(i));
			mat33_sub(_m1,_eye3);
			vec3_t _v1,_v2;
			vec3_mult(_v1,R,p.at(i));
			vec3_mult(_v2,_m1,_v1);
			vec3_add(t_opt,_v2);
		}

		vec3_t t_opt_;
		vec3_mult(t_opt_,G,t_opt);
		tnew.at(a) = t_opt_;
	}
	al_ret.assign(al_.begin(),al_.end());
}

// =====================================================================================

void getRfor2ndPose_V_Exact(pose_vec &sol, const vec3_array &v, const vec3_array &P,
					        const mat33_t R, const vec3_t t, const real_t DB)
{

	mat33_t RzN;
	decomposeR(RzN, R);
	mat33_t R_;
	mat33_mult(R_,R,RzN);
	mat33_t RzN_tr;
	mat33_transpose(RzN_tr,RzN);
	vec3_array P_;
	vec3_array_mult(P_,RzN_tr,P);
	vec3_t ang_zyx;
	rpyAng_X(ang_zyx,R_);
	vec3_t rpy;
	mat33_t Ry,Rz;
	vec3_assign(rpy,0,ang_zyx.v[1],0);
	rpyMat(Ry,rpy);
	vec3_assign(rpy,0,0,ang_zyx.v[2]);
	rpyMat(Rz,rpy);
	scalar_array bl;
	vec3_array Tnew;
	getRotationY_wrtT(bl,Tnew, v ,P_, t, DB, Rz);
	scalar_array_div(bl,180.0f/CONST_PI);
	const unsigned int n = (unsigned int) v.size();
	mat33_array V;
	V.resize(n);
	for(unsigned int i=0; i<n; i++)
	{
		vec3_mul_vec3trans(V.at(i),v.at(i),v.at(i));
		mat33_div(V.at(i),vec3trans_mul_vec3(v.at(i),v.at(i)));
	}

	sol.clear();
	sol.resize(bl.size());

	for(unsigned int j=0; j<(unsigned int)bl.size(); j++)
	{
		mat33_clear(Ry);
		vec3_assign(rpy,0,bl[j],0);
		rpyMat(Ry,rpy);
		mat33_t _m1;
		mat33_mult(_m1,Rz,Ry);
		mat33_mult(sol[j].R,_m1,RzN_tr);
		vec3_copy(sol[j].t,Tnew[j]);
		real_t E = 0;
		for(unsigned int i=0; i<n; i++)
		{
			mat33_t _m2;
			mat33_eye(_m2);
			mat33_sub(_m2,V.at(i));
			vec3_t _v1;
			vec3_mult(_v1,sol[j].R,P.at(i));
			vec3_add(_v1,sol[j].t);
			vec3_t _v2;
			vec3_mult(_v2,_m2,_v1);
			vec3_mult(_v2,_v2);
			E += vec3_sum(_v2);
		}
		sol[j].E = E;
	}
}

// =====================================================================================

void get2ndPose_Exact(pose_vec &sol, const vec3_array &v, const vec3_array &P,
					  const mat33_t R, const vec3_t t, const real_t DB)
{
	vec3_t cent, _v1;
	vec3_array _va1;
	normRv(_va1,v);
	vec3_array_mean(_v1,_va1);
	normRv(cent,_v1);
	mat33_t Rim;
	vec3_clear(_v1);
	_v1.v[2] = 1.0f;
	GetRotationbyVector(Rim,_v1,cent);
	vec3_array v_;
	vec3_array_mult(v_,Rim,v);
	normRv(_va1,v_);
	vec3_array_mean(_v1,_va1);
	normRv(cent,_v1);
	mat33_t R_;
	vec3_t  t_;
	mat33_mult(R_,Rim,R);
	vec3_mult(t_,Rim,t);
	getRfor2ndPose_V_Exact(sol,v_,P,R_,t_,DB);
	mat33_t Rim_tr;
	mat33_transpose(Rim_tr,Rim);
	for(unsigned int i=0; i<sol.size(); i++)
	{
		vec3_t _t;
		mat33_t _R;
		vec3_mult(_t,Rim_tr,sol[i].t);
		mat33_mult(_R,Rim_tr,sol[i].R);

		vec3_copy(sol[i].t,_t);
		mat33_copy(sol[i].R,_R);
	}
}

// =====================================================================================
void robust_pose(real_t &err, mat33_t &R, vec3_t &t,
		         const vec3_array &_model, const vec3_array &_iprts,
		         const options_t _options)
{
	mat33_t Rlu_;
	vec3_t tlu_;
	unsigned int it1_;
	real_t obj_err1_;
	real_t img_err1_;

	vec3_array model(_model.begin(),_model.end());
	vec3_array iprts(_iprts.begin(),_iprts.end());
	options_t options;
	memcpy(&options,&_options,sizeof(options_t));

	mat33_clear(Rlu_);
	vec3_clear(tlu_);
	it1_ = 0;
	obj_err1_ = 0;
	img_err1_ = 0;

	objpose(Rlu_, tlu_, it1_, obj_err1_, img_err1_, true, model, iprts, options);

	pose_vec sol;
	sol.clear();
	get2ndPose_Exact(sol,iprts,model,Rlu_,tlu_,0);
	int min_err_idx = (-1);
	real_t min_err = MAX_FLOAT;
	for(unsigned int i=0; i<sol.size(); i++)
	{
		mat33_copy(options.initR,sol[i].R);
		objpose(Rlu_, tlu_, it1_, obj_err1_, img_err1_, true, model, iprts, options);
		mat33_copy(sol[i].PoseLu_R,Rlu_);
		vec3_copy(sol[i].PoseLu_t,tlu_);
		sol[i].obj_err = obj_err1_;
		if(sol[i].obj_err < min_err)
		{
			min_err = sol[i].obj_err;
			min_err_idx = i;
		}
	}

	if(min_err_idx >= 0)
	{
		mat33_copy(R,sol[min_err_idx].PoseLu_R);
		vec3_copy(t,sol[min_err_idx].PoseLu_t);
		err = sol[min_err_idx].obj_err;
	}
	else
	{
		mat33_clear(R);
		vec3_clear(t);
		err = MAX_FLOAT;
	}
}

// ----------------------------------------
} // namespace rpp


#endif //_NO_LIBRPP_
