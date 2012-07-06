/* ========================================================================
 * PROJECT: ARToolKitPlus
 * ========================================================================
 *
 * See src/extra/BCH_original.txt for details/orig. copyright notice
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
 * $Id: BCH.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/extra/BCH.h>
#include "assert.h"

#include <vector>

namespace ARToolKitPlus {


static bool
_isBitSet(_64bits n, int which_bit)
{
	return ((n>>which_bit)&1) != 0;
}


static void
_setBit(_64bits &n, int which_bit)
{
	const _64bits one = 1;
	n |= (one<<which_bit);
}

/*
static void
_clearBit(_64bits &n, int which_bit)
{
	const _64bits one = 1;
	n &= ~(one<<which_bit);
}
*/

/*
static void
_copyBit(_64bits &dest_n, const int dest_bit, const _64bits src_n, const int src_bit)
{
	const _64bits one = 1;
	if(((src_n>>src_bit)&1) != 0) dest_n |= (one<<dest_bit);
	else dest_n &= ~(one<<dest_bit);
}
*/

/*
static int
_countOnes(const _64bits src_n)
{
	int cnt = 0;
	for(int i=0; i<64; i++)
		cnt += (int)((src_n>>i)&1);
	return(cnt);
}
*/

static int*
toBitPattern(int b[], _64bits n, int n_bits)
{
	for(int i=0; i<n_bits; i++)
		b[i] = (_isBitSet(n,i) ? 1:0);
	return b;
}

static _64bits
fromBitPattern(int b[], int n_bits)
{
	_64bits n = 0;
	for(int i=0; i<n_bits; i++)
		if(b[i] == 1) _setBit(n,i);
	return(n);
}

/*
static void
printBitPattern(_64bits n, int n_bits)
{
	for(int i=0; i<n_bits; i++)
	{
		if(_isBitSet(n,i)) printf("1");
		else printf("0");
	}
}
*/

/*
static void
printBitArray(int bb[], int bb_length)
{
	for(int i=0; i<bb_length; i++) printf("%i",bb[i]);
}
*/


BCH::BCH()
{
	initialize(BCH_DEFAULT_M, BCH_DEFAULT_LENGTH, BCH_DEFAULT_T);
}

BCH::BCH(int _m, int _length, int _t)
{
	initialize(_m,_length,_t);
}


void BCH::initialize(int _m, int _length, int _t)
{
	int i, ninf;

	m = _m;
	length = _length;
	t = _t;

	p.resize(BCH_MAX_P);
	alpha_to.resize(BCH_MAX_LUT);
	index_of.resize(BCH_MAX_LUT);
	g.resize(BCH_MAX_LUT);

	_elp.resize(BCH_MAX_LUT);
	for(i=0; i<BCH_MAX_LUT; i++) _elp[i].resize(BCH_MAX_LUT);
	_d.resize(BCH_MAX_LUT);
	_l.resize(BCH_MAX_LUT);
	_u_lu.resize(BCH_MAX_LUT);
	_s.resize(BCH_MAX_LUT);
	_root.resize(BCH_MAX_LUT);
	_loc.resize(BCH_MAX_LUT);
	_reg.resize(BCH_MAX_LUT);


	for (i=1; i<m; i++)
		p[i] = 0;
	p[0] = p[m] = 1;
	if (m == 2)			p[1] = 1;
	else if (m == 3)	p[1] = 1;
	else if (m == 4)	p[1] = 1;
	else if (m == 5)	p[2] = 1;
	else if (m == 6)	p[1] = 1;
	else if (m == 7)	p[1] = 1;
	else if (m == 8)	p[4] = p[5] = p[6] = 1;
	else if (m == 9)	p[4] = 1;
	else if (m == 10)	p[3] = 1;
	else if (m == 11)	p[2] = 1;
	else if (m == 12)	p[3] = p[4] = p[7] = 1;
	else if (m == 13)	p[1] = p[3] = p[4] = 1;
	else if (m == 14)	p[1] = p[11] = p[12] = 1;
	else if (m == 15)	p[1] = 1;
	else if (m == 16)	p[2] = p[3] = p[5] = 1;
	else if (m == 17)	p[3] = 1;
	else if (m == 18)	p[7] = 1;
	else if (m == 19)	p[1] = p[5] = p[6] = 1;
	else if (m == 20)	p[3] = 1;

    n = 1;
	for (i = 0; i <= m; i++) 
	{
        n *= 2;
    }
	n = n / 2 - 1;
	ninf = (n + 1) / 2 - 1;

	generate_gf();          /* Construct the Galois Field GF(2**m) */
	gen_poly(t);            /* Compute the generator polynomial of BCH code */
}


void BCH::generate_gf()
/*
 * Generate field GF(2**m) from the irreducible polynomial p(X) with
 * coefficients in p[0]..p[m].
 *
 * Lookup tables:
 *   index->polynomial form: alpha_to[] contains j=alpha^i;
 *   polynomial form -> index form:	index_of[j=alpha^i] = i
 *
 * alpha=2 is the primitive element of GF(2**m) 
 */
{
	int    i, mask;

	mask = 1;
	alpha_to[m] = 0;
	for (i = 0; i < m; i++) {
		alpha_to[i] = mask;
		index_of[alpha_to[i]] = i;
		if (p[i] != 0)
			alpha_to[m] ^= mask;
		mask <<= 1;
	}
	index_of[alpha_to[m]] = m;
	mask >>= 1;
	for (i = m + 1; i < n; i++) {
		if (alpha_to[i - 1] >= mask)
		  alpha_to[i] = alpha_to[m] ^ ((alpha_to[i - 1] ^ mask) << 1);
		else
		  alpha_to[i] = alpha_to[i - 1] << 1;
		index_of[alpha_to[i]] = i;
	}
	index_of[0] = -1;
}


bool BCH::gen_poly(int _t)
/*
 * Compute the generator polynomial of a binary BCH code. Fist generate the
 * cycle sets modulo 2**m - 1, cycle[][] =  (i, 2*i, 4*i, ..., 2^l*i). Then
 * determine those cycle sets that contain integers in the set of (d-1)
 * consecutive integers {1..(d-1)}. The generator polynomial is calculated
 * as the product of linear factors of the form (x+alpha^i), for every i in
 * the above cycle sets.
 */
{
	int	ii, jj, ll, kaux;
	int	test, aux, nocycles, root, noterms, rdncy;
	int             cycle[1024][21], size[1024], min[1024], zeros[1024];

	/* Generate cycle sets modulo n, n = 2**m - 1 */
	cycle[0][0] = 0;
	size[0] = 1;
	cycle[1][0] = 1;
	size[1] = 1;
	jj = 1;			/* cycle set index */

	#ifdef _BCH_VERBOSE_
		if (m > 9)  {
			printf("Computing cycle sets modulo %d\n", n);
			printf("(This may take some time)...\n");
		}
	#endif
	
	do {
		/* Generate the jj-th cycle set */
		ii = 0;
		do {
			ii++;
			cycle[jj][ii] = (cycle[jj][ii - 1] * 2) % n;
			size[jj]++;
			aux = (cycle[jj][ii] * 2) % n;
		} while (aux != cycle[jj][0]);
		/* Next cycle set representative */
		ll = 0;
		do {
			ll++;
			test = 0;
			for (ii = 1; ((ii <= jj) && (!test)); ii++)	
			/* Examine previous cycle sets */
			  for (kaux = 0; ((kaux < size[ii]) && (!test)); kaux++)
			     if (ll == cycle[ii][kaux])
			        test = 1;
		} while ((test) && (ll < (n - 1)));
		if (!(test)) {
			jj++;	/* next cycle set index */
			cycle[jj][0] = ll;
			size[jj] = 1;
		}
	} while (ll < (n - 1));
	nocycles = jj;		/* number of cycle sets modulo n */

	//printf("Enter the error correcting capability, t: ");
	//scanf("%d", &t);
	t = _t;

	d = 2 * t + 1;

	/* Search for roots 1, 2, ..., d-1 in cycle sets */
	kaux = 0;
	rdncy = 0;
	for (ii = 1; ii <= nocycles; ii++) {
		min[kaux] = 0;
		test = 0;
		for (jj = 0; ((jj < size[ii]) && (!test)); jj++)
			for (root = 1; ((root < d) && (!test)); root++)
				if (root == cycle[ii][jj])  {
					test = 1;
					min[kaux] = ii;
				}
		if (min[kaux]) {
			rdncy += size[min[kaux]];
			kaux++;
		}
	}
	noterms = kaux;
	kaux = 1;
	for (ii = 0; ii < noterms; ii++)
		for (jj = 0; jj < size[min[ii]]; jj++) {
			zeros[kaux] = cycle[min[ii]][jj];
			kaux++;
		}

	k = length - rdncy;

    if (k<0)
      {
		#ifdef _BCH_VERBOSE_
			printf("Parameters invalid!\n");
		#endif
         return(false);
      }

	#ifdef _BCH_VERBOSE_
		printf("This is a (%d, %d, %d) binary BCH code\n", length, k, d);
	#endif

	/* Compute the generator polynomial */
	g[0] = alpha_to[zeros[1]];
	g[1] = 1;		/* g(x) = (X + zeros[1]) initially */
	for (ii = 2; ii <= rdncy; ii++) {
	  g[ii] = 1;
	  for (jj = ii - 1; jj > 0; jj--)
	    if (g[jj] != 0)
	      g[jj] = g[jj - 1] ^ alpha_to[(index_of[g[jj]] + zeros[ii]) % n];
	    else
	      g[jj] = g[jj - 1];
	  g[0] = alpha_to[(index_of[g[0]] + zeros[ii]) % n];
	}

	#ifdef _BCH_VERBOSE_
		printf("Generator polynomial:\ng(x) = ");
		for (ii = 0; ii <= rdncy; ii++) {
		  printf("%d", g[ii]);
		  if (ii && ((ii % 50) == 0))
			printf("\n");
		}
		printf("\n");
	#endif
	return(true);
}



void BCH::encode_bch(int *bb, const int *data)
/*
 * Compute redundacy bb[], the coefficients of b(x). The redundancy
 * polynomial b(x) is the remainder after dividing x^(length-k)*data(x)
 * by the generator polynomial g(x).
 */
{
	int    i, j;
	int    feedback;

	for (i = 0; i < length - k; i++)
		bb[i] = 0;
	for (i = k - 1; i >= 0; i--) {
		feedback = data[i] ^ bb[length - k - 1];
		if (feedback != 0) {
			for (j = length - k - 1; j > 0; j--)
				if (g[j] != 0)
					bb[j] = bb[j - 1] ^ feedback;
				else
					bb[j] = bb[j - 1];
			bb[0] = g[0] && feedback;
		} else {
			for (j = length - k - 1; j > 0; j--)
				bb[j] = bb[j - 1];
			bb[0] = 0;
		}
	}
}


int BCH::decode_bch(int *recd)
/*
 * Simon Rockliff's implementation of Berlekamp's algorithm.
 *
 * Assume we have received bits in recd[i], i=0..(n-1).
 *
 * Compute the 2*t syndromes by substituting alpha^i into rec(X) and
 * evaluating, storing the syndromes in s[i], i=1..2t (leave s[0] zero) .
 * Then we use the Berlekamp algorithm to find the error location polynomial
 * elp[i].
 *
 * If the degree of the elp is >t, then we cannot correct all the errors, and
 * we have detected an uncorrectable error pattern. We output the information
 * bits uncorrected.
 *
 * If the degree of elp is <=t, we substitute alpha^i , i=1..n into the elp
 * to get the roots, hence the inverse roots, the error location numbers.
 * This step is usually called "Chien's search".
 *
 * If the number of errors located is not equal the degree of the elp, then
 * the decoder assumes that there are more than t errors and cannot correct
 * them, only detect them. We output the information bits uncorrected.
 */
{
	int i, j, u, q, t2, count = 0, syn_error = 0;
	bool too_many_errors = false;
	//int elp[BCH_MAX_LUT][BCH_MAX_LUT], d[BCH_MAX_LUT], l[BCH_MAX_LUT], u_lu[BCH_MAX_LUT], s[BCH_MAX_LUT];
	//int root[BCH_MAX_SQ], loc[BCH_MAX_SQ], reg[BCH_MAX_SQ];

	t2 = 2 * t;

	/* first form the syndromes */
	#ifdef _BCH_VERBOSE_
		printf("S(x) = ");
	#endif
	for (i = 1; i <= t2; i++) {
		_s[i] = 0;
		for (j = 0; j < length; j++)
			if (recd[j] != 0)
				_s[i] ^= alpha_to[(i * j) % n];
		if (_s[i] != 0)
			syn_error = 1; /* set error flag if non-zero syndrome */
/*
 * Note:    If the code is used only for ERROR DETECTION, then
 *          exit program here indicating the presence of errors.
 */
		/* convert syndrome from polynomial form to index form  */
		_s[i] = index_of[_s[i]];
		#ifdef _BCH_VERBOSE_
			printf("%3d ", _s[i]);
		#endif
	}

	#ifdef _BCH_VERBOSE_
		printf("\n");
	#endif

	if (syn_error) {	/* if there are errors, try to correct them */
		/*
		 * Compute the error location polynomial via the Berlekamp
		 * iterative algorithm. Following the terminology of Lin and
		 * Costello's book :   d[u] is the 'mu'th discrepancy, where
		 * u='mu'+1 and 'mu' (the Greek letter!) is the step number
		 * ranging from -1 to 2*t (see L&C),  l[u] is the degree of
		 * the elp at that step, and u_l[u] is the difference between
		 * the step number and the degree of the elp. 
		 */
		/* initialise table entries */
		_d[0] = 0;			/* index form */
		_d[1] = _s[1];		/* index form */
		_elp[0][0] = 0;		/* index form */
		_elp[1][0] = 1;		/* polynomial form */
		for (i = 1; i < t2; i++) {
			_elp[0][i] = -1;	/* index form */
			_elp[1][i] = 0;	/* polynomial form */
		}
		_l[0] = 0;
		_l[1] = 0;
		_u_lu[0] = -1;
		_u_lu[1] = 0;
		u = 0;
 
		do {
			u++;
			if (_d[u] == -1) {
				_l[u + 1] = _l[u];
				for (i = 0; i <= _l[u]; i++) {
					_elp[u + 1][i] = _elp[u][i];
					_elp[u][i] = index_of[_elp[u][i]];
				}
			} else
				/*
				 * search for words with greatest _u_lu[q] for
				 * which _d[q]!=0 
				 */
			{
				q = u - 1;
				while ((_d[q] == -1) && (q > 0))
					q--;
				/* have found first non-zero _d[q]  */
				if (q > 0) {
				  j = q;
				  do {
				    j--;
				    if ((_d[j] != -1) && (_u_lu[q] < _u_lu[j]))
				      q = j;
				  } while (j > 0);
				}
 
				/*
				 * have now found q such that _d[u]!=0 and
				 * _u_lu[q] is maximum 
				 */
				/* store degree of new elp polynomial */
				if (_l[u] > _l[q] + u - q)
					_l[u + 1] = _l[u];
				else
					_l[u + 1] = _l[q] + u - q;
 
				/* form new elp(x) */
				for (i = 0; i < t2; i++)
					_elp[u + 1][i] = 0;
				for (i = 0; i <= _l[q]; i++)
					if (_elp[q][i] != -1)
						_elp[u + 1][i + u - q] = 
                                   alpha_to[(_d[u] + n - _d[q] + _elp[q][i]) % n];
				for (i = 0; i <= _l[u]; i++) {
					_elp[u + 1][i] ^= _elp[u][i];
					_elp[u][i] = index_of[_elp[u][i]];
				}
			}
			_u_lu[u + 1] = u - _l[u + 1];
 
			/* form (u+1)th discrepancy */
			if (u < t2) {	
			/* no discrepancy computed on last iteration */
			  if (_s[u + 1] != -1)
			    _d[u + 1] = alpha_to[_s[u + 1]];
			  else
			    _d[u + 1] = 0;
			    for (i = 1; i <= _l[u + 1]; i++)
			      if ((_s[u + 1 - i] != -1) && (_elp[u + 1][i] != 0))
			        _d[u + 1] ^= alpha_to[(_s[u + 1 - i] 
			                      + index_of[_elp[u + 1][i]]) % n];
			  /* put _d[u+1] into index form */
			  _d[u + 1] = index_of[_d[u + 1]];	
			}
		} while ((u < t2) && (_l[u + 1] <= t));
 
		u++;
		if (_l[u] <= t) {/* Can correct errors */
			/* put elp into index form */
			for (i = 0; i <= _l[u]; i++)
				_elp[u][i] = index_of[_elp[u][i]];


#ifdef _BCH_VERBOSE_
			printf("sigma(x) = ");
			for (i = 0; i <= _l[u]; i++)
				printf("%3d ", _elp[u][i]);
			printf("\n");
			printf("Roots: ");
#endif

			/* Chien search: find roots of the error location polynomial */
			for (i = 1; i <= _l[u]; i++)
				_reg[i] = _elp[u][i];
			count = 0;
			for (i = 1; i <= n; i++) {
				q = 1;
				for (j = 1; j <= _l[u]; j++)
					if (_reg[j] != -1) {
						_reg[j] = (_reg[j] + j) % n;
						q ^= alpha_to[_reg[j]];
					}
				if (!q) {	/* store root and error
						 * location number indices */
					_root[count] = i;
					_loc[count] = n - i;
					count++;
					#ifdef _BCH_VERBOSE_
						printf("%3d ", n - i);
					#endif
				}
			}
			#ifdef _BCH_VERBOSE_
				printf("\n");
			#endif
			if (count == _l[u])	
			{
				/* no. roots = degree of elp hence <= t errors */
					for (i = 0; i < _l[u]; i++)
					{
						if(_loc[i]<BCH_DEFAULT_LENGTH) recd[_loc[i]] ^= 1;
						else too_many_errors = true;
					}
			}
			else	/* elp has degree >t hence cannot solve */
			{
				return(_l[u]); // number of errors
				#ifdef _BCH_VERBOSE_
					printf("Incomplete decoding: errors detected\n");
				#endif
			}
		}
	}
	if(too_many_errors) return(BCH_DEFAULT_T+1);
	else return(syn_error == 0 ? 0 : _l[u]); // number of errors
}


void BCH::encode(int encoded_bits[BCH_DEFAULT_LENGTH], const _64bits orig_n)
{
	assert(k == BCH_DEFAULT_K && length == BCH_DEFAULT_LENGTH);
	int orig_bits[BCH_DEFAULT_K];
	toBitPattern(orig_bits,orig_n,k);
	encode_bch(encoded_bits,orig_bits);

	for (int i = 0; i < k; i++)
		encoded_bits[i + length - k] = orig_bits[i];

	//printf("[x] encoded_bits: ");
	//printBitArray(encoded_bits, BCH_DEFAULT_LENGTH);
	//printf("\n");

}

bool BCH::decode(int &err_n, _64bits &orig_n, const int encoded_bits[BCH_DEFAULT_LENGTH])
{
	assert(k == BCH_DEFAULT_K && length == BCH_DEFAULT_LENGTH);
	int temp_bits[BCH_DEFAULT_LENGTH];
	for(int i=0; i<BCH_DEFAULT_LENGTH; i++) temp_bits[i] = encoded_bits[i];
	err_n = decode_bch(temp_bits);
	if(err_n > t) return(false);
	orig_n = fromBitPattern(&temp_bits[length - k],BCH_DEFAULT_K);
	return(true);
}

void BCH::encode(_64bits &encoded_n, const _64bits orig_n)
{
	assert(k == BCH_DEFAULT_K && length == BCH_DEFAULT_LENGTH);
	int encoded_bits[BCH_DEFAULT_LENGTH];
	encode(encoded_bits,orig_n);
	encoded_n = fromBitPattern(encoded_bits,BCH_DEFAULT_LENGTH);
}

bool BCH::decode(int &err_n, _64bits &orig_n,    const _64bits encoded_n)
{
	assert(k == BCH_DEFAULT_K && length == BCH_DEFAULT_LENGTH);
	int encoded_bits[BCH_DEFAULT_LENGTH];
	toBitPattern(encoded_bits,encoded_n,BCH_DEFAULT_LENGTH);
	return(decode(err_n, orig_n,encoded_bits));
}


}  // namespace ARToolKitPlus
