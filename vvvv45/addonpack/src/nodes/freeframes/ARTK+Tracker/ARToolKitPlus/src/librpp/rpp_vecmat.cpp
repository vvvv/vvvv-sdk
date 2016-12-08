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
 * $Id: rpp_vecmat.cpp 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#ifndef _NO_LIBRPP_


#include "rpp_vecmat.h"
#include "math.h"
#include "assert.h"


namespace rpp {


int svdcmp( double **a, int m,int n, double *w, double **v);
int quintic(double [], double [], double [], int*, double);
int quartic(double[], double[], double[], int* );
int cubic(double[], double[], int*);



#ifdef _WIN32_WCE
	real_t _sin(real_t a) { return(real_t(::sin(a))); }
	real_t _cos(real_t a) { return(real_t(::cos(a))); }
	real_t _atan2(real_t a, real_t b) { return(real_t(::atan2(a,b))); }
	real_t _abs(real_t a) { return((a>0?a:-a)); }
	real_t _acos(real_t a) { return(real_t(::acos(a))); }
	real_t _sqrt(real_t a) { return(real_t(::sqrt(a))); }
	real_t _pow(real_t a, real_t b) { return(real_t(::pow(a,b))); }
#else
	real_t _sin(real_t a) { return(::sin(a)); }
	real_t _cos(real_t a) { return(::cos(a)); }
	real_t _atan2(real_t a, real_t b) { return(::atan2(a,b)); }
	real_t _abs(real_t a) { return((a>0?a:-a)); }
	real_t _acos(real_t a) { return(::acos(a)); }
	real_t _sqrt(real_t a) { return(::sqrt(a)); }
	real_t _pow(real_t a, real_t b) { return(::pow(a,b)); }
	/*
	real_t _sin(real_t a) { return(::sinf(a)); }
	real_t _cos(real_t a) { return(::cosf(a)); }
	real_t _atan2(real_t a, real_t b) { return(::atan2f(a,b)); }
	real_t _abs(real_t a) { return((a>0?a:-a)); }
	real_t _acos(real_t a) { return(::acosf(a)); }
	real_t _sqrt(real_t a) { return(::sqrtf(a)); }
	real_t _pow(real_t a, real_t b) { return(::powf(a,b)); }
	*/
#endif

// ---------------------------------------------------------------------------

void mat33_from_double_pptr(mat33_t &mat, double** m_ptr)
{
	for(int m=0; m<3; m++)
		for(int n=0; n<3; n++)
			mat.m[m][n] = (real_t) m_ptr[m][n];
}


void mat33_from_float_pptr(mat33_t &mat, float** m_ptr)
{
	for(int m=0; m<3; m++)
		for(int n=0; n<3; n++)
			mat.m[m][n] = (real_t) m_ptr[m][n];
}


double ** mat33_to_double_pptr(const mat33_t &mat)
{
	double **M = (double**) malloc(3*sizeof(double*));
	for(int i=0; i<3; i++) M[i] = (double*) malloc(3*sizeof(double));
	for(int m=0; m<3; m++)
		for(int n=0; n<3; n++)
			M[m][n] = (double) mat.m[m][n];
	return(M);
}


float ** mat33_to_float_pptr(const mat33_t &mat)
{
	float **M = (float**) malloc(3*sizeof(float*));
	for(int i=0; i<3; i++) M[i] = (float*) malloc(3*sizeof(float));
	for(int m=0; m<3; m++)
		for(int n=0; n<3; n++)
			M[m][n] = (float) mat.m[m][n];
	return(M);
}


void free_double_pptr(double*** m_ptr)
{
	for(int i=0; i<3; i++) free((*m_ptr)[i]);
	free(*m_ptr);
}

void free_float_pptr(float*** m_ptr)
{
	for(int i=0; i<3; i++) free((*m_ptr)[i]);
	free(*m_ptr);
}

void vec3_from_double_ptr(vec3_t &vec, double* v_ptr)
{
	vec.v[0] = (real_t)v_ptr[0];
	vec.v[1] = (real_t)v_ptr[1];
	vec.v[2] = (real_t)v_ptr[2];
}

void vec3_from_float_ptr(vec3_t &vec, float* v_ptr)
{
	vec.v[0] = (real_t)v_ptr[0];
	vec.v[1] = (real_t)v_ptr[1];
	vec.v[2] = (real_t)v_ptr[2];
}

double* vec3_to_double_ptr(const vec3_t &vec)
{
	double* v = (double*) malloc(3* sizeof(double));
	v[0] = (double) vec.v[0];
	v[1] = (double) vec.v[1];
	v[2] = (double) vec.v[2];
	return(v);
}

float* vec3_to_float_ptr(const vec3_t &vec)
{
	float* v = (float*) malloc(3* sizeof(float));
	v[0] = (float) vec.v[0];
	v[1] = (float) vec.v[1];
	v[2] = (float) vec.v[2];
	return(v);
}


void free_double_ptr(double** v_ptr)
{
	free(*v_ptr);
}

void free_float_ptr(float** v_ptr)
{
	free(*v_ptr);
}



void mat33_assign(mat33_t &m,
				  const real_t m00, const real_t m01, const real_t m02,
				  const real_t m10, const real_t m11, const real_t m12,
				  const real_t m20, const real_t m21, const real_t m22)
{
	m.m[0][0] = m00; 	m.m[0][1] = m01;	m.m[0][2] = m02;
	m.m[1][0] = m10; 	m.m[1][1] = m11;	m.m[1][2] = m12;
	m.m[2][0] = m20; 	m.m[2][1] = m21;	m.m[2][2] = m22;
}


void _dbg_quat_print(const quat_t &q, char* name)
{
	printf("%s:  s = [ %.4f ]  v = [ ",name,q.s);
	for(unsigned int c=0; c<3; c++)
	{
		// double d = (double)(q.v.v[c]);
		printf("%.4f ",q.v.v[c]);
	}
	printf("]\n");
}

void _dbg_mat33_print(const mat33_t &m, char* name)
{
	printf("%s:\n",name);
	for(unsigned int r=0; r<3; r++)
	{
		printf("[ ");
		for(unsigned int c=0; c<3; c++)
		{
			double d = (double)(m.m[r][c]);
			printf("%.4f ",d);
		}
		printf("]\n");
	}
}

void _dbg_mat33_array_print(const mat33_array &m, char* name)
{
	for(unsigned int i=0; i<m.size(); i++)
	{
		printf("%s.at(%i):\n",name,i);
		for(unsigned int r=0; r<3; r++)
		{
			printf("[ ");
			for(unsigned int c=0; c<3; c++)
			{
				double d = (double)(m.at(i).m[r][c]);
				printf("%.4f ",d);
			}
			printf("]\n");
		}
	}
}

void _dbg_vec3_print(const vec3_t &v, char* name)
{
	printf("%s:  [ ",name);
	for(unsigned int c=0; c<3; c++)
	{
		double d = (double)(v.v[c]);
		printf("%.4f ",d);
	}
	printf("]\n");
}

void _dbg_vec3_array_print(const vec3_array &v, char* name)
{
	for(unsigned int i=0; i<v.size(); i++)
	{
		printf("%s.at(%i):  [ ",name,i);
		for(unsigned int c=0; c<3; c++)
		{
			double d = (double)(v.at(i).v[c]);
			printf("%.4f ",d);
		}
		printf("]\n");
	}
}


bool _dbg_load_vec3_array(vec3_array &va, char* filename)
{
	FILE *fp = fopen( filename, "r" );
	if( fp == NULL ) return(false);

	int n;
	int lineno = 0;
	va.clear();

	while(!feof(fp))
	{
		++lineno;
		double vx,vy,vz;
		vec3_t v;
		n = fscanf(fp, "%lf%lf%lf\n", &vx,&vy,&vz);
		if((n!=3) || ferror(fp))
		{
			printf("file i/o error: %s (line %i)",filename,lineno);
			fclose(fp);
			return(lineno > 1);
		}
		v.v[0] = (real_t)vx;
		v.v[1] = (real_t)vy;
		v.v[2] = (real_t)vz;
		va.push_back(v);
	}

	fclose(fp);
	return(true);
}


void vec3_assign(vec3_t &v, const real_t x, const real_t y, const real_t z)
{
	v.v[0] = x;
	v.v[1] = y;
	v.v[2] = z;
}

void vec3_clear(vec3_t &v)
{
	v.v[0] = 0;
	v.v[1] = 0;
	v.v[2] = 0;
}

void vec3_copy(vec3_t &a, const vec3_t &b)
{
	a.v[0] = b.v[0];
	a.v[1] = b.v[1];
	a.v[2] = b.v[2];
}


void vec3_array_sum(vec3_t &v_sum2, const vec3_array &va)
{
	vec3_clear(v_sum2);
	for(vec3_array_const_iter iter = va.begin();
		iter != va.end(); iter++)
	{
		v_sum2.v[0] += (*iter).v[0];
		v_sum2.v[1] += (*iter).v[1];
		v_sum2.v[2] += (*iter).v[2];
	}
}

void vec3_array_sum(scalar_array &v_sum1, const vec3_array &va)
{
	v_sum1.clear();
	v_sum1.resize(va.size());

	for(unsigned int i=0; i<va.size(); i++)
		v_sum1.at(i) = (va[i].v[0] + va[i].v[1] + va[i].v[2]);
}

void vec3_array_pow2(vec3_array &va)
{
	for(vec3_array_iter iter = va.begin();
		iter != va.end(); iter++)
	{
		(*iter).v[0] *= (*iter).v[0];
		(*iter).v[1] *= (*iter).v[1];
		(*iter).v[2] *= (*iter).v[2];
	}
}




void vec3_div(vec3_t &va, const real_t n)
{
	va.v[0] /= n;
	va.v[1] /= n;
	va.v[2] /= n;
}

void vec3_div(vec3_t &va, const vec3_t &vb)
{
	va.v[0] /= vb.v[0];
	va.v[1] /= vb.v[1];
	va.v[2] /= vb.v[2];
}

void vec3_mult(vec3_t &va, const real_t n)
{
	va.v[0] *= n;
	va.v[1] *= n;
	va.v[2] *= n;
}

void vec3_mult(vec3_t &va, const vec3_t &vb)
{
	va.v[0] *= vb.v[0];
	va.v[1] *= vb.v[1];
	va.v[2] *= vb.v[2];
}

void vec3_add(vec3_t &va, const real_t f)
{
	va.v[0] += f;
	va.v[1] += f;
	va.v[2] += f;
}

void vec3_add(vec3_t &va, const vec3_t &vb)
{
	va.v[0] += vb.v[0];
	va.v[1] += vb.v[1];
	va.v[2] += vb.v[2];
}

void vec3_add(vec3_t &va, const vec3_t &vb, const vec3_t &vc)
{
	va.v[0] = vb.v[0] + vc.v[0];
	va.v[1] = vb.v[1] + vc.v[1];
	va.v[2] = vb.v[2] + vc.v[2];
}

void vec3_sub(vec3_t &va, const real_t f)
{
	va.v[0] -= f;
	va.v[1] -= f;
	va.v[2] -= f;
}

void vec3_sub(vec3_t &va, const vec3_t &vb)
{
	va.v[0] -= vb.v[0];
	va.v[1] -= vb.v[1];
	va.v[2] -= vb.v[2];
}

void vec3_sub(vec3_t &va, const vec3_t &vb, const vec3_t &vc)
{
	va.v[0] = vb.v[0] - vc.v[0];
	va.v[1] = vb.v[1] - vc.v[1];
	va.v[2] = vb.v[2] - vc.v[2];
}

real_t vec3_dot(const vec3_t &va, const vec3_t &vb)
{
	return(va.v[0]*vb.v[0]+va.v[1]*vb.v[1]+va.v[2]*vb.v[2]);
}

void vec3_cross(vec3_t &va, const vec3_t &vb, const vec3_t &vc)
{
	va.v[0] = (vb.v[1] * vc.v[2] - vc.v[1] * vb.v[2]);
	va.v[1] = (vb.v[2] * vc.v[0] - vc.v[2] * vb.v[0]);
	va.v[2] = (vb.v[0] * vc.v[1] - vc.v[0] * vb.v[1]);
}

real_t vec3_norm(const vec3_t &v)
{
	return(_sqrt(v.v[0]*v.v[0] + v.v[1]*v.v[1] + v.v[2]*v.v[2]));
}

real_t vec3_sum(const vec3_t &v)
{
	return(v.v[0] + v.v[1] + v.v[2]);
}

void vec3_array_add(vec3_array &va, const vec3_t &a)
{
	for(vec3_array_iter iter = va.begin();
		iter != va.end(); iter++)
	{
		(*iter).v[0] += a.v[0];
		(*iter).v[1] += a.v[1];
		(*iter).v[2] += a.v[2];
	}
}

void vec3_array_sub(vec3_array &va, const vec3_t &a) 
{
	for(vec3_array_iter iter = va.begin();
		iter != va.end(); iter++)
	{
		(*iter).v[0] -= a.v[0];
		(*iter).v[1] -= a.v[1];
		(*iter).v[2] -= a.v[2];
	}
}

void vec3_array_set(vec3_array &va, const vec3_t &a, const bool mask[3])
{
	for(vec3_array_iter iter = va.begin();
		iter != va.end(); iter++)
	{
		if(mask[0]) (*iter).v[0] = a.v[0];
		if(mask[1]) (*iter).v[1] = a.v[1];
		if(mask[2]) (*iter).v[2] = a.v[2];
	}
}

void vec3_array_mult(vec3_array &va, const scalar_array &c)
{
	assert(va.size() == c.size());
	for(unsigned int i=0; i<va.size(); i++)
	{
		va[i].v[0] *= c[i];
		va[i].v[1] *= c[i];
		va[i].v[2] *= c[i];
	}
}

void vec3_array_mean(vec3_t &v_mean, const vec3_array &va)
{
	vec3_array_sum(v_mean, va);
	real_t l = (real_t) va.size();
	vec3_div(v_mean, l);
}


void vec3_mul_vec3trans(mat33_t &m, const vec3_t &va, const vec3_t &vb)
{
	m.m[0][0] = va.v[0] * vb.v[0];
	m.m[0][1] = va.v[0] * vb.v[1];
	m.m[0][2] = va.v[0] * vb.v[2];
	m.m[1][0] = va.v[1] * vb.v[0];
	m.m[1][1] = va.v[1] * vb.v[1];
	m.m[1][2] = va.v[1] * vb.v[2];
	m.m[2][0] = va.v[2] * vb.v[0];
	m.m[2][1] = va.v[2] * vb.v[1];
	m.m[2][2] = va.v[2] * vb.v[2];
}

real_t vec3trans_mul_vec3(const vec3_t &va, const vec3_t &vb)
{
	return(va.v[0] * vb.v[0] + va.v[1] * vb.v[1] + va.v[2] * vb.v[2]);
}


void mat33_clear(mat33_t &m)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			m.m[r][c] = 0;
		}
}

void mat33_copy(mat33_t &md, const mat33_t &ms)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			md.m[r][c] = ms.m[r][c];
		}
}


void mat33_to_col_vec3(vec3_t &c0, vec3_t &c1, vec3_t &c2, const mat33_t &m)
{
	for(unsigned int r=0; r<3; r++)
	{
		c0.v[r] = m.m[r][0];
		c1.v[r] = m.m[r][1];
		c2.v[r] = m.m[r][2];
	}
}


void mat33_div(mat33_t &m, const real_t f)
{
	m.m[0][0] /= f;
	m.m[0][1] /= f;
	m.m[0][2] /= f;
	m.m[1][0] /= f;
	m.m[1][1] /= f;
	m.m[1][2] /= f;
	m.m[2][0] /= f;
	m.m[2][1] /= f;
	m.m[2][2] /= f;
}

void mat33_eye(mat33_t &m)
{
	m.m[0][0] = 1;
	m.m[0][1] = 0;
	m.m[0][2] = 0;
	m.m[1][0] = 0;
	m.m[1][1] = 1;
	m.m[1][2] = 0;
	m.m[2][0] = 0;
	m.m[2][1] = 0;
	m.m[2][2] = 1;
}

real_t mat33_sum(const mat33_t &m)
{
	real_t sum(0.0f);
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			sum += m.m[r][c];
		}
		return(sum);
}


bool mat33_all_zeros(const mat33_t &m)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			if(m.m[r][c] != 0) return(false);
		}
		return(true);
}

void mat33_set_all_zeros(mat33_t &m)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			m.m[r][c] = 0;
		}
}


void mat33_array_sum(mat33_t &s, const mat33_array &ma)
{
	mat33_clear(s);
	for(mat33_array_const_iter iter = ma.begin();
		iter != ma.end(); iter++)
	{
		for(unsigned int c=0; c<3; c++)
		{
			s.m[0][c] += (*iter).m[0][c];
			s.m[1][c] += (*iter).m[1][c];
			s.m[2][c] += (*iter).m[2][c];
		}
	}
}


void mat33_sub(mat33_t &mr, const mat33_t &ma, const mat33_t &mb)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			mr.m[r][c] = ma.m[r][c]-mb.m[r][c];
		}
}

void mat33_sub(mat33_t &ma, const mat33_t &mb)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			ma.m[r][c] -= mb.m[r][c];
		}
}

void mat33_add(mat33_t &mr, const mat33_t &ma, const mat33_t &mb)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			mr.m[r][c] = ma.m[r][c]+mb.m[r][c];
		}
}

void mat33_add(mat33_t &ma, const mat33_t &mb)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			ma.m[r][c] += mb.m[r][c];
		}
}


real_t mat33_det(const mat33_t &a)
{
	real_t determinant = a.m[0][0]*a.m[1][1]*a.m[2][2] + a.m[0][1]*a.m[1][2]*a.m[2][0] +
		a.m[0][2]*a.m[1][0]*a.m[2][1] - a.m[2][0]*a.m[1][1]*a.m[0][2] - 
		a.m[2][1]*a.m[1][2]*a.m[0][0] - a.m[2][2]*a.m[1][0]*a.m[0][1];
	return(determinant);
}

void mat33_inv(mat33_t &mi, const mat33_t &ma)
{
	real_t determinant = mat33_det(ma);
	mi.m[0][0] = (ma.m[1][1]*ma.m[2][2] - ma.m[1][2]*ma.m[2][1])/determinant;
	mi.m[0][1] = (ma.m[0][2]*ma.m[2][1] - ma.m[0][1]*ma.m[2][2])/determinant;
	mi.m[0][2] = (ma.m[0][1]*ma.m[1][2] - ma.m[0][2]*ma.m[1][1])/determinant;

	mi.m[1][0] = (ma.m[1][2]*ma.m[2][0] - ma.m[1][0]*ma.m[2][2])/determinant;
	mi.m[1][1] = (ma.m[0][0]*ma.m[2][2] - ma.m[0][2]*ma.m[2][0])/determinant;
	mi.m[1][2] = (ma.m[0][2]*ma.m[1][0] - ma.m[0][0]*ma.m[1][2])/determinant;

	mi.m[2][0] = (ma.m[1][0]*ma.m[2][1] - ma.m[1][1]*ma.m[2][0])/determinant;
	mi.m[2][1] = (ma.m[0][1]*ma.m[2][0] - ma.m[0][0]*ma.m[2][1])/determinant;
	mi.m[2][2] = (ma.m[0][0]*ma.m[1][1] - ma.m[0][1]*ma.m[1][0])/determinant;
}

void mat33_mult(mat33_t &m0, const mat33_t &m1, const mat33_t &m2)
{
	m0.m[0][0] = m1.m[0][0]*m2.m[0][0] + m1.m[0][1]*m2.m[1][0] + m1.m[0][2]*m2.m[2][0];
	m0.m[0][1] = m1.m[0][0]*m2.m[0][1] + m1.m[0][1]*m2.m[1][1] + m1.m[0][2]*m2.m[2][1];
	m0.m[0][2] = m1.m[0][0]*m2.m[0][2] + m1.m[0][1]*m2.m[1][2] + m1.m[0][2]*m2.m[2][2];
	m0.m[1][0] = m1.m[1][0]*m2.m[0][0] + m1.m[1][1]*m2.m[1][0] + m1.m[1][2]*m2.m[2][0];
	m0.m[1][1] = m1.m[1][0]*m2.m[0][1] + m1.m[1][1]*m2.m[1][1] + m1.m[1][2]*m2.m[2][1];
	m0.m[1][2] = m1.m[1][0]*m2.m[0][2] + m1.m[1][1]*m2.m[1][2] + m1.m[1][2]*m2.m[2][2];
	m0.m[2][0] = m1.m[2][0]*m2.m[0][0] + m1.m[2][1]*m2.m[1][0] + m1.m[2][2]*m2.m[2][0];
	m0.m[2][1] = m1.m[2][0]*m2.m[0][1] + m1.m[2][1]*m2.m[1][1] + m1.m[2][2]*m2.m[2][1];
	m0.m[2][2] = m1.m[2][0]*m2.m[0][2] + m1.m[2][1]*m2.m[1][2] + m1.m[2][2]*m2.m[2][2];
}

void mat33_mult(mat33_t &mr, const real_t n)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			mr.m[r][c] *= n;
		}
}

void mat33_transpose(mat33_t &t, const mat33_t m)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			t.m[r][c] = m.m[c][r];
		}
}

void vec3_mult(vec3_t &v0, const mat33_t &m1, const vec3_t &v2)
{
	v0.v[0] = m1.m[0][0]*v2.v[0] + m1.m[0][1]*v2.v[1] + m1.m[0][2]*v2.v[2];
	v0.v[1] = m1.m[1][0]*v2.v[0] + m1.m[1][1]*v2.v[1] + m1.m[1][2]*v2.v[2];
	v0.v[2] = m1.m[2][0]*v2.v[0] + m1.m[2][1]*v2.v[1] + m1.m[2][2]*v2.v[2];
}

void vec3_array_mult(vec3_array &va, const mat33_t &m, const vec3_array &vb)
{
	va.clear();
	va.resize(vb.size());
	for(unsigned int i=0; i<vb.size(); i++)
	{
		vec3_mult(va.at(i),m,vb.at(i));
	}
}

void mat33_svd2(mat33_t &u, mat33_t &s, mat33_t &v, const mat33_t &m)
{
	mat33_clear(u);
	mat33_clear(v);

	double** m_ptr = mat33_to_double_pptr(m);
	double** v_ptr = mat33_to_double_pptr(v);

	vec3_t q;
	vec3_clear(q);
	double*  q_ptr = vec3_to_double_ptr(q);

	/*int ret =*/ svdcmp(m_ptr, 3, 3, q_ptr, v_ptr);

	mat33_from_double_pptr(u,m_ptr);
	mat33_from_double_pptr(v,v_ptr);
	vec3_from_double_ptr(q,q_ptr);

	mat33_clear(s);
	s.m[0][0] = (real_t)q_ptr[0];
	s.m[1][1] = (real_t)q_ptr[1];
	s.m[2][2] = (real_t)q_ptr[2];

	free_double_pptr(&m_ptr);
	free_double_pptr(&v_ptr);
	free_double_ptr(&q_ptr);
}

void quat_mult(quat_t &q, const real_t s)
{
	vec3_mult(q.v,s);
	q.s *= s;
}

real_t quat_norm(const quat_t &q)
{
	const real_t f_vn = vec3_norm(q.v);
	return(_sqrt((f_vn*f_vn) + (q.s*q.s)));

}

void mat33_from_quat(mat33_t &m, const quat_t &q)
{
	const real_t a = q.s;
	const real_t b = q.v.v[0];
	const real_t c = q.v.v[1];
	const real_t d = q.v.v[2];

	m.m[0][0] = (a*a)+(b*b)-(c*c)-(d*d);
	m.m[0][1] = real_t(2.0f)*(b*c-a*d);
	m.m[0][2] = real_t(2.0f)*(b*d+a*c);

	m.m[1][0] = real_t(2.0f)*(b*c+a*d);
	m.m[1][1] = (a*a)+(c*c)-(b*b)-(d*d);
	m.m[1][2] = real_t(2.0f)*(c*d-a*b);

	m.m[2][0] = real_t(2.0f)*(b*d-a*c);
	m.m[2][1] = real_t(2.0f)*(c*d+a*b);
	m.m[2][2] = (a*a)+(d*d)-(b*b)-(c*c);
}

// ===========================================================================================
void normRv(vec3_t &n, const vec3_t &v)
{
	vec3_t _v1;
	vec3_copy(_v1,v);
	vec3_mult(_v1,_v1);
	real_t l = 1.0f / _sqrt(_v1.v[0] + _v1.v[1] + _v1.v[2]);
	vec3_copy(n,v);
	vec3_mult(n,l);
}

// ===========================================================================================
void normRv(vec3_array &normR_v, const vec3_array &v)
{
	normR_v.assign(v.begin(),v.end());
	vec3_array_pow2(normR_v);
	scalar_array _l;
	vec3_array_sum(_l,normR_v);

	for(scalar_array::iterator iter = _l.begin();
		iter != _l.end(); iter++)
	{
		(*iter) = 1.0f / _sqrt(*iter);
	}

	normR_v.assign(v.begin(),v.end());
	vec3_array_mult(normR_v,_l);
}

// ===========================================================================================
int solve_polynomial(scalar_array &r_sol, const scalar_array &coefficients)
{
	if(coefficients.size() != 5) return(0);
	r_sol.clear();

	double dd[5] = {(double)coefficients[0],
		(double)coefficients[1],
		(double)coefficients[2],
		(double)coefficients[3],
		(double)coefficients[4] };

	double sol[4] = {0,0,0,0};
	double soli[4] = {0,0,0,0};
	int n_sol = 0;
	quartic(dd, sol, soli, &n_sol);

	if(n_sol <= 0) return(0); // assert(false);

	r_sol.resize(n_sol);
	for(int i=0; i<n_sol; i++) r_sol[i] = (real_t)sol[i];
	return(n_sol);
}
// ===========================================================================================

void scalar_array_pow(scalar_array &sa, const real_t f)
{
	for(unsigned int i=0; i<sa.size(); i++) sa.at(i) = _pow(sa[i],f);
}

void scalar_array_negate(scalar_array &sa)
{
	for(unsigned int i=0; i<sa.size(); i++) sa.at(i) = - sa.at(i);
}

void scalar_array_assign(scalar_array &sa,
						 const	real_t f,
						 const unsigned int sz)
{
	sa.clear();
	sa.resize(sz);
	for(unsigned int i=0; i<sz; i++) sa.at(i) = f;
}


void scalar_array_add(scalar_array &sa, const scalar_array &sb)
{
	assert(sa.size() == sb.size());
	for(unsigned int i=0; i<(unsigned int)sa.size(); i++)
		sa.at(i) = sa.at(i) + sb.at(i);
}

void scalar_array_clear(scalar_array &sa)
{
	for(unsigned int i=0; i<(unsigned int)sa.size(); i++) sa.at(i) = 0.;
}

void scalar_array_atan2(scalar_array &sa, 
						const scalar_array &sb,
						const scalar_array &sc)
{
	assert(sb.size() == sc.size());
	sa.clear();
	sa.resize(sb.size());
	for(unsigned int i=0; i<(unsigned int)sb.size(); i++)
		sa[i] = _atan2(sb[i],sc[i]);
}

void _dbg_scalar_array_print(const scalar_array &sa, char* name)
{
	for(unsigned int i=0; i<sa.size(); i++)
		printf("%s.at(%i):  [ %e ]\n",name,i,sa[i]);
}

void scalar_array_div(scalar_array &sa, real_t f)
{
	for(unsigned int i=0; i<(unsigned int)sa.size(); i++)
	{
		sa.at(i) /= f;
	}
}

void scalar_array_div(scalar_array &sa, const scalar_array &sb)
{
	assert(sa.size() == sb.size());
	for(unsigned int i=0; i<(unsigned int)sa.size(); i++)
		sa[i] /= sb[i];
}

void scalar_array_mult(scalar_array &sa, real_t f)
{
	for(unsigned int i=0; i<(unsigned int)sa.size(); i++)
	{
		sa.at(i) *= f;
	}
}

void scalar_array_add(scalar_array &sa, real_t f)
{
	for(unsigned int i=0; i<(unsigned int)sa.size(); i++)
	{
		sa.at(i) += f;
	}
}

void scalar_array_sub(scalar_array &sa, real_t f)
{
	for(unsigned int i=0; i<(unsigned int)sa.size(); i++)
	{
		sa.at(i) -= f;
	}
}

void mat33_pow2(mat33_t &m)
{
	for(unsigned int r=0; r<3; r++)
		for(unsigned int c=0; c<3; c++)
		{
			m.m[r][c] *= m.m[r][c];
		}
}


void _dbg_vec3_fprint(void* fp, const vec3_t &v, char* name)
{
	fprintf((FILE*)fp,"%s:  [ ",name);
	for(unsigned int c=0; c<3; c++)
	{
		fprintf((FILE*)fp,"%.4f ",v.v[c]);
	}
	fprintf((FILE*)fp,"]\n");
}

void _dbg_mat33_fprint(void* fp, const mat33_t &m, char* name)
{
	fprintf((FILE*)fp,"%s:\n",name);
	for(unsigned int r=0; r<3; r++)
	{
		fprintf((FILE*)fp,"[ ");
		for(unsigned int c=0; c<3; c++)
		{
			fprintf((FILE*)fp,"%.4f ",m.m[r][c]);
		}
		fprintf((FILE*)fp,"]\n");
	}
}


} // namespace rpp


#endif //_NO_LIBRPP_
