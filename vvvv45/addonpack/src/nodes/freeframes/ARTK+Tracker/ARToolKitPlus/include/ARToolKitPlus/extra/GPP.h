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
* $Id: GPP.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


#ifndef __GPP_HEADERFILE__
#define __GPP_HEADERFILE__

#include <gpp.h>


#define GPPMUL2(a,b, res, bits)  \
	gppMul_n_32s((a), (b), &res, bits);

#define GPPMUL3(a,b,c, res, bits)  \
	gppMul_n_32s((a), (b), &_tmp1, bits);  \
	gppMul_n_32s(_tmp1, (c), &res, bits);

#define GPPMUL4(a,b,c,d, res, bits)  \
	gppMul_n_32s((a), (b), &_tmp1, bits);  \
	gppMul_n_32s(_tmp1, (c), &_tmp2, bits);  \
	gppMul_n_32s(_tmp2, (d), &res, bits);

#define GPPDIV2(a,b, res, bits)  \
	gppDiv_n_32s((a), (b), &res, bits);


#endif //__GPP_HEADERFILE__
