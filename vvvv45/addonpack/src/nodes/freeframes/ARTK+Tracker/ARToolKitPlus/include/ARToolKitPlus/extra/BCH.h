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
* $Id: BCH.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __BCH_CODE__H__
#define __BCH_CODE__H__

#include <stdio.h>
#include <stdlib.h>
#include <vector>


namespace ARToolKitPlus {


// --------------------------------------------------------
// WARNING: It is *NOT SAFE* to alter the values below!

	#define BCH_DEFAULT_M       6
	#define BCH_DEFAULT_LENGTH 36
	#define BCH_DEFAULT_T       4
	#define BCH_DEFAULT_K      12

	#define BCH_MAX_M    6
	#define BCH_MAX_P    7  // MAX_M+1
	#define BCH_MAX_LUT 64  // 2^MAX_M
	#define BCH_MAX_SQ   8  // SQRT(MAX_LUT) -- (?)
// -------------------------------------------------------

// we only use unsigned __int64 under windows.
// have to use unsigned long long othersie...
#if defined(_MSC_VER) || defined(_WIN32_WCE)
typedef unsigned __int64 _64bits;
#else
typedef unsigned long long _64bits;
#endif


static bool _isBitSet(_64bits bn, int which_bit);
static void _setBit(_64bits &bn, int which_bit);
/*
static void _clearBit(_64bits &bn, int which_bit);
static void _copyBit(_64bits &dest_n, const int dest_bit, const _64bits src_n, const int src_bit);
static int _countOnes(const _64bits src_n);
*/

static int* toBitPattern(int b[], _64bits n, int n_bits);
static _64bits fromBitPattern(int b[], int n_bits);

// static void printBitPattern(_64bits n, int n_bits);


class BCH
// this class implements a (36, 12, 9) binary BCH encoder/decoder
{
	public:
		BCH();

		void encode(int encoded_bits[BCH_DEFAULT_LENGTH], const _64bits orig_n);
		bool decode(int &err_n, _64bits &orig_n, const int encoded_bits[BCH_DEFAULT_LENGTH]);

		void encode(_64bits &encoded_n, const _64bits orig_n);
		bool decode(int &err_n, _64bits &orig_n,    const _64bits encoded_n);


	protected:
		BCH(int _m, int _length, int _t);
		void initialize(int _m, int _length, int _t);
		void generate_gf();
		bool gen_poly(int _t);
		void encode_bch(int *bb, const int *data); // int bb[length - k], data[k]
		int decode_bch(int *recd);

		int t;
		int             m, n, length, k, d;
		
		std::vector<int> p;
		std::vector<int> alpha_to;
		std::vector<int> index_of;
		std::vector<int> g;

		std::vector<std::vector<int> > _elp;
		std::vector<int> _d;
		std::vector<int> _l;
		std::vector<int> _u_lu;
		std::vector<int> _s;
		std::vector<int> _root;
		std::vector<int> _loc;
		std::vector<int> _reg;
};


}  // namespace ARToolKitPlus


#endif // __BCH_CODE__H__
