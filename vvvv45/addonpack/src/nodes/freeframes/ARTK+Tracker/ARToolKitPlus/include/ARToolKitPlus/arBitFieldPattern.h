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
* $Id: arBitFieldPattern.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __ARBITFIELDPATTERN_HEADERFILE__
#define __ARBITFIELDPATTERN_HEADERFILE__


namespace ARToolKitPlus {

	enum {
		// size of the marker images
		idPattWidth = 6,
		idPattHeight = 6,

		// number of bits we can use for marker id
		idBits = 9,
		idMask = (1<<idBits) - 1,
		idMax = (1<<idBits) - 1,

		pattBits = 4*idBits
	};

	// we only use __int64 under windows.
	// have to use unsigned long long othersie...
#if defined(_MSC_VER) || defined(_WIN32_WCE)
	typedef __int64 IDPATTERN;
	const IDPATTERN bchMask = 0x8f80b8750;
#else
	typedef unsigned long long IDPATTERN;
	const IDPATTERN bchMask = 0x8f80b8750ll;
#endif

	const IDPATTERN xorMask0 = 0x0027;
	const IDPATTERN xorMask1 = 0x014e;
	const IDPATTERN xorMask2 = 0x0109;
	const IDPATTERN xorMask3 = 0x00db;

	const int posMask0 = 0;
	const int posMask1 = idBits;
	const int posMask2 = 2*idBits;
	const int posMask3 = 3*idBits;

	// full mask that is used to xor raw pattern image
	const IDPATTERN fullMask = (xorMask0<<posMask0) | (xorMask1<<posMask1) | (xorMask2<<posMask2) | (xorMask3<<posMask3);


	const unsigned int bchBits = 12;
	const unsigned int idMaxBCH = (1<<bchBits) - 1;

	// array with indices for 90° CW rotated grid
	const int rotate90[] = {
		30, 24, 18, 12,  6,  0,
		31, 25, 19, 13,  7,  1,
		32, 26, 20, 14,  8,  2,
		33, 27, 21, 15,  9,  3,
		34, 28, 22, 16, 10,  4,
		35, 29, 23, 17, 11,  5 
	};


	// some internal methods. primarily needed for
	// marker printing, etc.
	//
	static void generatePatternSimple(int nID, IDPATTERN& nPattern);

	static void generatePatternBCH(int nID, IDPATTERN& nPattern);

	// static void setBit(IDPATTERN& pat, int which);

	static bool isBitSet(IDPATTERN pat, int which);


}  // namespace ARToolKitPlus


#endif //__ARBITFIELDPATTERN_HEADERFILE__
