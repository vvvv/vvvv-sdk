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
 ** @author   Daniel Wagner
 *
 * $Id: arGetTransMat2.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


//#include <stdlib.h>
//#include <math.h>

//#include <ARToolKitPlus/Tracker.h>
//#include <ARToolKitPlus/matrix.h>
//#include <ARToolKitPlus/extra/Profiler.h>


namespace ARToolKitPlus {


#define MD_PI         3.14159265358979323846


#ifndef _FIXEDPOINT_MATH_ACTIVATED_


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arModifyMatrix(ARFloat rot[3][3], ARFloat trans[3], ARFloat cpara[3][4],
				   ARFloat vertex[][3], ARFloat pos2d[][2], int num)
{
    ARFloat    factor;
    ARFloat    a, b, c;
    ARFloat    a1, b1, c1;
    ARFloat    a2, b2, c2;
    ARFloat    ma = 0, mb = 0, mc = 0;
    ARFloat    combo[3][4];
    ARFloat    hx, hy, h, x, y;
    ARFloat    err, minerr;
    int        t1, t2, t3;
    int       s1 = 0, s2 = 0, s3 = 0;
    int       i, j;

	PROFILE_BEGINSEC(profiler, MODIFYMATRIX)

    arGetAngle( rot, &a, &b, &c );

    a2 = a;
    b2 = b;
    c2 = c;
    factor = (ARFloat)(10.0*MD_PI/180.0);
    for( j = 0; j < 15; j++ ) {
        minerr = 1000000000.0;
        for(t1=-1;t1<=1;t1++) {
        for(t2=-1;t2<=1;t2++) {
        for(t3=-1;t3<=1;t3++) {

            a1 = a2 + factor*t1;
            b1 = b2 + factor*t2;
            c1 = c2 + factor*t3;

            arGetNewMatrix( a1, b1, c1, trans, NULL, cpara, combo );

            err = 0.0;
            for( i = 0; i < num; i++ ) {
                hx = combo[0][0] * vertex[i][0]
                   + combo[0][1] * vertex[i][1]
                   + combo[0][2] * vertex[i][2]
                   + combo[0][3];
                hy = combo[1][0] * vertex[i][0]
                   + combo[1][1] * vertex[i][1]
                   + combo[1][2] * vertex[i][2]
                   + combo[1][3];
                h  = combo[2][0] * vertex[i][0]
                   + combo[2][1] * vertex[i][1]
                   + combo[2][2] * vertex[i][2]
                   + combo[2][3];
                x = hx / h;
                y = hy / h;

                err += (pos2d[i][0] - x) * (pos2d[i][0] - x)
                     + (pos2d[i][1] - y) * (pos2d[i][1] - y);
            }

            if( err < minerr ) {
                minerr = err;
                ma = a1;
                mb = b1;
                mc = c1;
                s1 = t1; s2 = t2; s3 = t3;
            }
        }
        }
        }

        if( s1 == 0 && s2 == 0 && s3 == 0 /*&& ss1 == 0 && ss2 == 0 && ss3 == 0*/ ) factor *= 0.5;
        a2 = ma;
        b2 = mb;
        c2 = mc;
    }

    arGetRot( ma, mb, mc, rot );

	PROFILE_ENDSEC(profiler, MODIFYMATRIX)

    return minerr/num;
}


AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arModifyMatrix2(ARFloat rot[3][3], ARFloat trans[3], ARFloat cpara[3][4],
					ARFloat vertex[][3], ARFloat pos2d[][2], int num)
{
    ARFloat    factor;
    ARFloat    a, b, c;
    ARFloat    a1, b1, c1;
    ARFloat    a2, b2, c2;
    ARFloat    ma, mb, mc;
    ARFloat    combo[3][4];
    ARFloat    hx, hy, h, x, y;
    ARFloat    err, minerr;
    int        t1, t2, t3, tt1,tt2,tt3;
	ARFloat	   tfact[5] = { 0.96f, 0.98f, 1.0f, 1.02f, 1.04f };
	ARFloat	   modtrans[3], mmodtrans[3];
    int       s1, s2, s3, ss1,ss2,ss3;
    int       i, j;

    arGetAngle( rot, &a, &b, &c );

    a2 = a;
    b2 = b;
    c2 = c;
    factor = (ARFloat)(40.0*MD_PI/180.0);
    for( j = 0; j < 15; j++ ) {
        minerr = 1000000000.0;
        for(t1=-1;t1<=1;t1++) {
        for(t2=-1;t2<=1;t2++) {
        for(t3=-1;t3<=1;t3++) {
        for(tt1=-2;tt1<=2;tt1++) {
        for(tt2=-2;tt2<=2;tt2++) {
        for(tt3=-2;tt3<=2;tt3++) {

            a1 = a2 + factor*t1;
            b1 = b2 + factor*t2;
            c1 = c2 + factor*t3;
			modtrans[0] = trans[0]*tfact[tt1+2];
			modtrans[1] = trans[1]*tfact[tt2+2];
			modtrans[2] = trans[2]*tfact[tt3+2];

            arGetNewMatrix( a1, b1, c1, modtrans, NULL, cpara, combo );

            err = 0.0;
            for( i = 0; i < num; i++ ) {
                hx = combo[0][0] * vertex[i][0]
                   + combo[0][1] * vertex[i][1]
                   + combo[0][2] * vertex[i][2]
                   + combo[0][3];
                hy = combo[1][0] * vertex[i][0]
                   + combo[1][1] * vertex[i][1]
                   + combo[1][2] * vertex[i][2]
                   + combo[1][3];
                h  = combo[2][0] * vertex[i][0]
                   + combo[2][1] * vertex[i][1]
                   + combo[2][2] * vertex[i][2]
                   + combo[2][3];
                x = hx / h;
                y = hy / h;

                err += (pos2d[i][0] - x) * (pos2d[i][0] - x)
                     + (pos2d[i][1] - y) * (pos2d[i][1] - y);
            }

            if( err < minerr ) {
                minerr = err;
                ma = a1;
                mb = b1;
                mc = c1;
				mmodtrans[0] = modtrans[0];
				mmodtrans[1] = modtrans[1];
				mmodtrans[2] = modtrans[2];

                s1 = t1; s2 = t2; s3 = t3;
				ss1 = tt1; ss2 = tt2; ss3 = tt3;
            }
        }
        }
        }
		}
		}
		}

        if( s1 == 0 && s2 == 0 && s3 == 0 /*&& ss1 == 0 && ss2 == 0 && ss3 == 0*/ ) factor *= 0.5;
        a2 = ma;
        b2 = mb;
        c2 = mc;
		trans[0] = mmodtrans[0];
		trans[1] = mmodtrans[1];
		trans[2] = mmodtrans[2];
    }

    arGetRot( ma, mb, mc, rot );

    return minerr/num;
}


#else //_FIXEDPOINT_MATH_ACTIVATED_


//////////////////////////////////////////////////////////////
//
//             FIXEDPOINT specific code starts here
//
//

#define BITS 8


int arGetNewMatrix12(I32 _a, I32 _b, I32 _c, FIXED_VEC3D _trans, ARFloat trans2[3][4],
					 FIXED_VEC3D _cpara[3], I32 _cpara3[3], FIXED_VEC3D ret[3], I32 _ret3[3], Profiler& nProfiler);



AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arModifyMatrix(ARFloat rot[3][3], ARFloat trans[3], ARFloat cpara[3][4],
                   ARFloat vertex[][3], ARFloat pos2d[][2], int num)
{
    //ARFloat		factor;
    ARFloat		a, b, c;
    //ARFloat		a1, b1, c1;
    ARFloat		a2, b2, c2;
    ARFloat		ma, mb, mc;
    int			t1, t2, t3;
    int			s1, s2, s3;
    int			i, j, k;
	ARFloat		minerr;

    I32    _hx, _hy, _h;
	I32    _err, _minerr;
	U32    _ures;
	I32	   _a1,_b1,_c1;
	I32	   _a2,_b2,_c2;
	I32		_ma, _mb, _mc;

	PROFILE_BEGINSEC(profiler, MODIFYMATRIX)

	FIXED_VEC3D	*_vertex = (FIXED_VEC3D*)malloc(num*sizeof(FIXED_VEC3D)),
				*_pos2d = (FIXED_VEC3D*)malloc(num*sizeof(FIXED_VEC3D)),
				_combo[3], _vec1, _vec2, _trans;
	I32			_combo3[3];

	FIXED_VEC3D	_cpara[3];
	I32			_cpara3[3];

	_vec1.z = 0;

	_trans.x = FIXED_Float_To_Fixed_n(trans[0], 12);
	_trans.y = FIXED_Float_To_Fixed_n(trans[1], 12);
	_trans.z = FIXED_Float_To_Fixed_n(trans[2], 12);

	for(j=0; j<3; j++)
	{
		_cpara[j].x = FIXED_Float_To_Fixed_n(cpara[j][0], 12);
		_cpara[j].y = FIXED_Float_To_Fixed_n(cpara[j][1], 12);
		_cpara[j].z = FIXED_Float_To_Fixed_n(cpara[j][2], 12);
		_cpara3[j] =  FIXED_Float_To_Fixed_n(cpara[j][3], 12);
	}

	for(j=0; j<num; j++)
	{
		_vertex[j].x = FIXED_Float_To_Fixed_n(vertex[j][0], BITS);
		_vertex[j].y = FIXED_Float_To_Fixed_n(vertex[j][1], BITS);
		_vertex[j].z = FIXED_Float_To_Fixed_n(vertex[j][2], BITS);

		_pos2d[j].x = FIXED_Float_To_Fixed_n(pos2d[j][0], BITS);
		_pos2d[j].y = FIXED_Float_To_Fixed_n(pos2d[j][1], BITS);
		_pos2d[j].z = 0;
	}


    arGetAngle( rot, &a, &b, &c );

	PROFILE_BEGINSEC(profiler, MODIFYMATRIX_LOOP)

    a2 = a;
    b2 = b;
    c2 = c;
    //factor = (ARFloat)(10.0*MD_PI/180.0);

	//I32 fix_a2, fix_b2, fix_c2;
	I32 fix_factor = FIXED_Float_To_Fixed_n((10.0*MD_PI/180.0), 12);
	I32 fix_a[3], fix_b[3], fix_c[3];

	_a2 = FIXED_Float_To_Fixed_n(a2, 12);
	_b2 = FIXED_Float_To_Fixed_n(b2, 12);
	_c2 = FIXED_Float_To_Fixed_n(c2, 12);

    for( j = 0; j < 10; j++ ) {
		_minerr = 0x40000000;		// value

		fix_a[0] = _a2 - fix_factor;   fix_a[1] = _a2;   fix_a[2] = _a2 + fix_factor;
		fix_b[0] = _b2 - fix_factor;   fix_b[1] = _b2;   fix_b[2] = _b2 + fix_factor;
		fix_c[0] = _c2 - fix_factor;   fix_c[1] = _c2;   fix_c[2] = _c2 + fix_factor;

        for(t1=-1;t1<=1;t1++) {
        for(t2=-1;t2<=1;t2++) {
        for(t3=-1;t3<=1;t3++) {
            //a1 = a2 + factor*t1;
            //b1 = b2 + factor*t2;
            //c1 = c2 + factor*t3;

			_a1 = fix_a[t1+1];
			_b1 = fix_b[t2+1];
			_c1 = fix_c[t3+1];

			PROFILE_BEGINSEC(profiler, GETNEWMATRIX)
            arGetNewMatrix12(_a1, _b1, _c1, _trans, NULL, _cpara, _cpara3, _combo, _combo3, profiler);
			PROFILE_ENDSEC(profiler, GETNEWMATRIX)

			for(k=0; k<3; k++)
			{
				_combo[k].x >>= 4;
				_combo[k].y >>= 4;
				_combo[k].z >>= 4;
				_combo3[k] >>= 4;
			}

            _err = 0;
            for( i = 0; i < num; i++ ) {
				FIXED_VEC3_DOT(_combo+0, _vertex+i, &_hx, BITS);
				_hx += _combo3[0];

				FIXED_VEC3_DOT(_combo+1, _vertex+i, &_hy, BITS);
				_hy += _combo3[1];

				FIXED_VEC3_DOT(_combo+2, _vertex+i, &_h, BITS);
				_h += _combo3[2];

				FIXED_DIV2(_hx, _h, _vec1.x, BITS);
				FIXED_DIV2(_hy, _h, _vec1.y, BITS);

				FIXED_VEC3_SUB(_pos2d+i, &_vec1, &_vec2);
				FIXED_VEC3_LENGTH_SQ(&_vec2, &_ures, BITS);
				_err += _ures;
            }

            if( _err < _minerr ) {
                _minerr = _err;
                //ma = a1;
                //mb = b1;
                //mc = c1;
				_ma = _a1;
				_mb = _b1;
				_mc = _c1;
                s1 = t1; s2 = t2; s3 = t3;
            }
        }
        }
        }

        if(s1 == 0 && s2 == 0 && s3 == 0)
		{
			//factor *= (ARFloat)0.5;
			fix_factor >>= 1;
		}
        //a2 = ma;
        //b2 = mb;
        //c2 = mc;
		_a2 = _ma;
		_b2 = _mb;
		_c2 = _mc;
    }

	PROFILE_ENDSEC(profiler, MODIFYMATRIX_LOOP)

	free(_vertex);
	free(_pos2d);

	ma = FIXED_Fixed_n_To_Float(_ma, 12);
	mb = FIXED_Fixed_n_To_Float(_mb, 12);
	mc = FIXED_Fixed_n_To_Float(_mc, 12);

    arGetRot( ma, mb, mc, rot );

	minerr = FIXED_Fixed_n_To_Float(_minerr, BITS);

	PROFILE_ENDSEC(profiler, MODIFYMATRIX)

    return minerr/num;
}

/*
AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::arModifyMatrix(ARFloat rot[3][3], ARFloat trans[3], ARFloat cpara[3][4],
								 ARFloat vertex[][3], ARFloat pos2d[][2], int num)
{
	ARFloat		factor;
	ARFloat		a, b, c;
	ARFloat		a1, b1, c1;
	ARFloat		a2, b2, c2;
	ARFloat		ma, mb, mc;
	int			t1, t2, t3;
	int			s1, s2, s3;
	int			i, j, k;
	ARFloat		minerr;

	I32    _hx, _hy, _h;
	I32    _err, _minerr;
	U32    _ures;
	I32	   _a1,_b1,_c1;

	FIXED_VEC3D	*_vertex = (FIXED_VEC3D*)malloc(num*sizeof(FIXED_VEC3D)),
		*_pos2d = (FIXED_VEC3D*)malloc(num*sizeof(FIXED_VEC3D)),
		_combo[3], _vec1, _vec2, _trans;
	I32			_combo3[3];

	FIXED_VEC3D	_cpara[3];
	I32			_cpara3[3];

	_vec1.z = 0;

	_trans.x = FIXED_Float_To_Fixed_n(trans[0], 12);
	_trans.y = FIXED_Float_To_Fixed_n(trans[1], 12);
	_trans.z = FIXED_Float_To_Fixed_n(trans[2], 12);

	for(j=0; j<3; j++)
	{
		_cpara[j].x = FIXED_Float_To_Fixed_n(cpara[j][0], 12);
		_cpara[j].y = FIXED_Float_To_Fixed_n(cpara[j][1], 12);
		_cpara[j].z = FIXED_Float_To_Fixed_n(cpara[j][2], 12);
		_cpara3[j] = FIXED_Float_To_Fixed_n(cpara[j][3], 12);
	}

	for(j=0; j<num; j++)
	{
		_vertex[j].x = FIXED_Float_To_Fixed_n(vertex[j][0], BITS);
		_vertex[j].y = FIXED_Float_To_Fixed_n(vertex[j][1], BITS);
		_vertex[j].z = FIXED_Float_To_Fixed_n(vertex[j][2], BITS);

		_pos2d[j].x = FIXED_Float_To_Fixed_n(pos2d[j][0], BITS);
		_pos2d[j].y = FIXED_Float_To_Fixed_n(pos2d[j][1], BITS);
		_pos2d[j].z = 0;
	}


	arGetAngle( rot, &a, &b, &c );


	a2 = a;
	b2 = b;
	c2 = c;
	factor = (ARFloat)(10.0*MD_PI/180.0);
	for( j = 0; j < 10; j++ ) {
		_minerr = 0x40000000;		// value

		for(t1=-1;t1<=1;t1++) {
			for(t2=-1;t2<=1;t2++) {
				for(t3=-1;t3<=1;t3++) {
					a1 = a2 + factor*t1;
					b1 = b2 + factor*t2;
					c1 = c2 + factor*t3;

					_a1 = FIXED_Float_To_Fixed_n(a1, 12);
					_b1 = FIXED_Float_To_Fixed_n(b1, 12);
					_c1 = FIXED_Float_To_Fixed_n(c1, 12);

					arGetNewMatrix12(_a1, _b1, _c1, _trans, NULL, _cpara, _cpara3, _combo, _combo3, profiler);

					for(k=0; k<3; k++)
					{
						_combo[k].x >>= 4;
						_combo[k].y >>= 4;
						_combo[k].z >>= 4;
						_combo3[k] >>= 4;
					}

					_err = 0;
					for( i = 0; i < num; i++ ) {
						FIXED_VEC3_DOT(_combo+0, _vertex+i, &_hx, BITS);
						_hx += _combo3[0];

						FIXED_VEC3_DOT(_combo+1, _vertex+i, &_hy, BITS);
						_hy += _combo3[1];

						FIXED_VEC3_DOT(_combo+2, _vertex+i, &_h, BITS);
						_h += _combo3[2];

						FIXED_DIV2(_hx, _h, _vec1.x, BITS)
							FIXED_DIV2(_hy, _h, _vec1.y, BITS)

							FIXED_VEC3_SUB(_pos2d+i, &_vec1, &_vec2);
						FIXED_VEC3_LENGTH_SQ(&_vec2, &_ures, BITS);
						_err += _ures;
					}

					if( _err < _minerr ) {
						_minerr = _err;
						ma = a1;
						mb = b1;
						mc = c1;
						s1 = t1; s2 = t2; s3 = t3;
					}
				}
			}
		}

		if( s1 == 0 && s2 == 0 && s3 == 0 ) factor *= (ARFloat)0.5;
		a2 = ma;
		b2 = mb;
		c2 = mc;
	}

	free(_vertex);
	free(_pos2d);

	arGetRot( ma, mb, mc, rot );

	minerr = FIXED_Fixed_n_To_Float(_minerr, BITS);

	return minerr/num;
}

*/


//
//
//             FIXEDPOINT specific code ends here
//
//////////////////////////////////////////////////////////////


#ifdef BITS
#undef BITS
#endif


#endif //_FIXEDPOINT_MATH_ACTIVATED_


}  // namespace ARToolKitPlus
