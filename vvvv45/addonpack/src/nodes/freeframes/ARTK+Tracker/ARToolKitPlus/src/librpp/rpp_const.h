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
 * $Id: rpp_const.h 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#ifndef __RPP_CONST_H__
#define __RPP_CONST_H__

#define CONST_PI_OVER_4       0.7853981633974483f
#define CONST_PI_OVER_2       1.5707963267948966f
#define CONST_PI              3.1415926535897932f
#define CONST_2_PI            6.2331853071795865f


// experimental and not working yet, do not define !
//#define _USE_CUSTOMFLOAT_


#ifdef _USE_CUSTOMFLOAT_
#  define MAX_FLOAT        1E10
#  define DEFAULT_TOL      1E-3
#  define DEFAULT_EPSILON  1E-4
#else
#  define MAX_FLOAT        1E10
#  define DEFAULT_TOL      1E-5
#  define DEFAULT_EPSILON  1E-8
#endif


#endif
