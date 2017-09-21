// Author: Thomas Pintaric <pintaric@ims.tuwien.ac.at>

#include <vector>
#include "../rpp.h"
#include "../rpp_types.h"
#include "../rpp_vecmat.h"
#include "mex.h"

using namespace rpp;

void mexFunction(int nlhs, mxArray *plhs[], int nrhs, const mxArray *prhs[])
{
    const mxArray *model_p, *iprts_p;
    double *model_v, *iprts_v, *R_v, *t_v, *e_v;
    
    model_p = prhs[0]; 
    iprts_p = prhs[1]; 
    model_v = mxGetPr(model_p);
    iprts_v = mxGetPr(iprts_p);

	if(mxGetN(model_p) != mxGetN(iprts_p))
	{
        mexErrMsgTxt("size(model) ~= size(iprts)");        
        return;
	}
	if((mxGetM(model_p) != 3) || (mxGetM(iprts_p) != 3))
	{
        mexErrMsgTxt("[model] and [iprts] must be 3-by-N matrices");        
        return;
	}
    

	unsigned int n = mxGetN(model_p);
	vec3_array model;
	vec3_array iprts;

	model.resize(n);
	iprts.resize(n);

	unsigned int j=0;
	for(unsigned int i=0; i<n; i++)
	{
		vec3_t _vm,_vi;
		vec3_assign(_vm,(real_t)model_v[j],(real_t)model_v[j+1],(real_t)model_v[j+2]);
		vec3_assign(_vi,(real_t)iprts_v[j],(real_t)iprts_v[j+1],(real_t)iprts_v[j+2]);
		j += 3;
		model[i] = _vm;
		iprts[i] = _vi;
	}

	options_t options;
    options.epsilon = (float)DEFAULT_EPSILON;
    options.tol =     (float)DEFAULT_TOL;
	mat33_set_all_zeros(options.initR);

	real_t  err;
	vec3_t  t;
	mat33_t R;    
    
	robust_pose(err,R,t,model,iprts,options);
   
    plhs[0] = mxCreateDoubleMatrix(3, 3, mxREAL);
    R_v = mxGetPr(plhs[0]);

	R_v[0] = R.m[0][0];	R_v[1] = R.m[1][0];	R_v[2] = R.m[2][0];
	R_v[3] = R.m[0][1];	R_v[4] = R.m[1][1];	R_v[5] = R.m[2][1];
	R_v[6] = R.m[0][2];	R_v[7] = R.m[1][2];	R_v[8] = R.m[2][2];

    plhs[1] = mxCreateDoubleMatrix(3, 1, mxREAL);
    t_v = mxGetPr(plhs[1]);

	t_v[0] = t.v[0];
	t_v[1] = t.v[1];
	t_v[2] = t.v[2];

    plhs[2] = mxCreateDoubleMatrix(1, 1, mxREAL);
    e_v = mxGetPr(plhs[2]);
	e_v[0] = (double)err;

    return;
}
