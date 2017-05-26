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
 * $Id: rpp_vecmat.h 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#ifndef __RPP_VECMAT_H__
#define __RPP_VECMAT_H__

#include "rpp_const.h"
#include "rpp_types.h"

namespace rpp {

real_t _sin(real_t a);
real_t _cos(real_t a);
real_t _atan2(real_t a, real_t b);
real_t _abs(real_t a);
real_t _acos(real_t a);
real_t _sqrt(real_t a);
real_t _pow(real_t a);

void mat33_from_double_pptr(mat33_t &mat, double** m_ptr);
double ** mat33_to_double_pptr(const mat33_t &mat);
void free_double_pptr(double*** m_ptr);
void vec3_from_double_ptr(vec3_t &vec, double* v_ptr);
double* vec3_to_double_ptr(const vec3_t &vec);
void free_double_ptr(double** v_ptr);
void mat33_assign(mat33_t &m,
				  const real_t m00, const real_t m01, const real_t m02,
				  const real_t m10, const real_t m11, const real_t m12,
				  const real_t m20, const real_t m21, const real_t m22);
void _dbg_quat_print(const quat_t &q, char* name);
void _dbg_mat33_print(const mat33_t &m, char* name);
void _dbg_mat33_array_print(const mat33_array &m, char* name);
void _dbg_vec3_print(const vec3_t &v, char* name);
void _dbg_vec3_array_print(const vec3_array &v, char* name);
bool _dbg_load_vec3_array(vec3_array &va, char* filename);
void vec3_assign(vec3_t &v, const real_t x, const real_t y, const real_t z);
void vec3_clear(vec3_t &v);
void vec3_copy(vec3_t &a, const vec3_t &b);
void vec3_array_sum(vec3_t &v_sum2, const vec3_array &va);
void vec3_array_sum(scalar_array &v_sum1, const vec3_array &va);
void vec3_array_pow2(vec3_array &va);
void vec3_div(vec3_t &va, const real_t n);
void vec3_div(vec3_t &va, const vec3_t &vb);
void vec3_mult(vec3_t &va, const real_t n);
void vec3_mult(vec3_t &va, const vec3_t &vb);
void vec3_add(vec3_t &va, const real_t f);
void vec3_add(vec3_t &va, const vec3_t &vb);
void vec3_add(vec3_t &va, const vec3_t &vb, const vec3_t &vc);
void vec3_sub(vec3_t &va, const real_t f);
void vec3_sub(vec3_t &va, const vec3_t &vb);
void vec3_sub(vec3_t &va, const vec3_t &vb, const vec3_t &vc);
real_t vec3_dot(const vec3_t &va, const vec3_t &vb);
void vec3_cross(vec3_t &va, const vec3_t &vb, const vec3_t &vc);
real_t vec3_norm(const vec3_t &v);
real_t vec3_sum(const vec3_t &v);
void vec3_array_add(vec3_array &va, const vec3_t &a);
void vec3_array_sub(vec3_array &va, const vec3_t &a);
void vec3_array_set(vec3_array &va, const vec3_t &a, const bool mask[3]);
void vec3_array_mult(vec3_array &va, const scalar_array &c);
void vec3_array_mean(vec3_t &v_mean, const vec3_array &va);
void vec3_mul_vec3trans(mat33_t &m, const vec3_t &va, const vec3_t &vb);
real_t vec3trans_mul_vec3(const vec3_t &va, const vec3_t &vb);
void mat33_clear(mat33_t &m);
void mat33_copy(mat33_t &md, const mat33_t &ms);
void mat33_to_col_vec3(vec3_t &c0, vec3_t &c1, vec3_t &c2, const mat33_t &m);
void mat33_div(mat33_t &m, const real_t f);
void mat33_eye(mat33_t &m);
real_t mat33_sum(const mat33_t &m);
bool mat33_all_zeros(const mat33_t &m);
void mat33_set_all_zeros(mat33_t &m);
void mat33_array_sum(mat33_t &s, const mat33_array &ma);
void mat33_sub(mat33_t &mr, const mat33_t &ma, const mat33_t &mb);
void mat33_sub(mat33_t &ma, const mat33_t &mb);
void mat33_add(mat33_t &mr, const mat33_t &ma, const mat33_t &mb);
void mat33_add(mat33_t &ma, const mat33_t &mb);
real_t mat33_det(const mat33_t &a);
void mat33_inv(mat33_t &mi, const mat33_t &ma);
void mat33_mult(mat33_t &m0, const mat33_t &m1, const mat33_t &m2);
void mat33_mult(mat33_t &mr, const real_t n);
void mat33_transpose(mat33_t &t, const mat33_t m);
void vec3_mult(vec3_t &v0, const mat33_t &m1, const vec3_t &v2);
void vec3_array_mult(vec3_array &va, const mat33_t &m, const vec3_array &vb);
void mat33_svd2(mat33_t &u, mat33_t &s, mat33_t &v, const mat33_t &m);
void quat_mult(quat_t &q, const real_t s);
real_t quat_norm(const quat_t &q);
void mat33_from_quat(mat33_t &m, const quat_t &q);
void normRv(vec3_t &n, const vec3_t &v);
void normRv(vec3_array &normR_v, const vec3_array &v);

int solve_polynomial(scalar_array &sol, const scalar_array &coefficients);
void scalar_array_pow(scalar_array &sa, const real_t f);
void scalar_array_negate(scalar_array &sa);
void scalar_array_assign(scalar_array &sa,
						 const	real_t f,
						 const unsigned int sz);
void scalar_array_add(scalar_array &sa, const scalar_array &sb);
void scalar_array_clear(scalar_array &sa);
void scalar_array_atan2(scalar_array &sa, 
						const scalar_array &sb,
						const scalar_array &sc);

void _dbg_scalar_array_print(const scalar_array &sa, char* name);
void scalar_array_div(scalar_array &sa, real_t f);
void scalar_array_div(scalar_array &sa, const scalar_array &sb);
void scalar_array_mult(scalar_array &sa, real_t f);
void scalar_array_add(scalar_array &sa, real_t f);
void scalar_array_sub(scalar_array &sa, real_t f);
void mat33_pow2(mat33_t &m);

void _dbg_vec3_fprint(void* fp, const vec3_t &v, char* name);
void _dbg_mat33_fprint(void* fp, const mat33_t &m, char* name);

} // namespace rpp

#endif
