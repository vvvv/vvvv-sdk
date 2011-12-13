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
* $Id: config.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */


/**
 * This file is a stripped down version of AR Toolkit original
 * config.h file. Only defines necessary for the core toolkit
 * routines have been left. I tried to comment all variables in a
 * meaningful way. Please extend the comments if you have any idea!
 */

#ifndef AR_CONFIG_H
#define AR_CONFIG_H

// autoconf generated file with configuration information
#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

// may be changed to double, float is particularly useful for PDA's
#ifdef _USE_DOUBLE_
	typedef double ARFloat;
#else
	typedef float ARFloat;
#endif


/**
 * Endianness:
 * ususally evaluated by autoconf script, otherwise set by hand
 */
#ifdef HAVE_CONFIG_H
	#ifdef WORDS_BIGENDIAN
		#define AR_BIG_ENDIAN
		#undef AR_LITTLE_ENDIAN
	#else
		#define AR_LITTLE_ENDIAN
		#undef AR_BIG_ENDIAN
	#endif
#else
	#ifdef __linux
		#undef   AR_BIG_ENDIAN
		#define  AR_LITTLE_ENDIAN
	#endif
	#ifdef _WIN32
		#undef   AR_BIG_ENDIAN
		#define  AR_LITTLE_ENDIAN
	#endif
	#ifdef __sgi
		#undef   AR_LITTLE_ENDIAN
		#define  AR_BIG_ENDIAN
	#endif
	#ifdef __APPLE_CC__
		#undef   AR_LITTLE_ENDIAN
		#define  AR_BIG_ENDIAN
	#endif
#endif

/**
 * Pixel format:
 * depends on platform
 *
 * One day we have to provide a version that
 * just takes an 8-Bit b&w image.
 */

//#ifdef _USE_8BITIMAGE_
//  #define AR_PIX_SIZE 1
//  #define AR_PIX_FORMAT_LUM
//#else 
//  #ifdef _USE_16BITIMAGE_
//    #define AR_PIX_SIZE 2
//    #define AR_PIX_FORMAT_RGB565
//  #else
//    #define AR_PIX_SIZE 4
//    #ifdef __linux
//      #define AR_PIX_FORMAT_RGBA
//    #endif
//    #ifdef _WIN32
//      #define AR_PIX_FORMAT_BGRA
//    #endif
//    #ifdef __sgi
//      #define AR_PIX_FORMAT_ABGR
//    #endif
//    #ifdef __APPLE_CC__
//      #define AR_PIX_FORMAT_RGBA
//    #endif
//  #endif //_USE_16BITIMAGE_
//#endif //_USE_8BITIMAGE_


/*------------------------------------------------------------
 * see 
 * http://www.hitl.washington.edu/people/grof/SharedSpace/Download/Doc/art240.html 
 * for an explanation of the next two define blocks
 */

// constants for variable arImageProcMode
// half mode is faster and useful for interlaced images
#define  AR_IMAGE_PROC_IN_FULL        0
#define  AR_IMAGE_PROC_IN_HALF        1
#define  DEFAULT_IMAGE_PROC_MODE     AR_IMAGE_PROC_IN_HALF

// constants for variable arFittingMode
#define  AR_FITTING_TO_IDEAL          0
#define  AR_FITTING_TO_INPUT          1
#define  DEFAULT_FITTING_MODE        AR_FITTING_TO_IDEAL

// constants for variable arTemplateMatchingMode
#define  AR_TEMPLATE_MATCHING_COLOR   0
#define  AR_TEMPLATE_MATCHING_BW      1
#define  DEFAULT_TEMPLATE_MATCHING_MODE     AR_TEMPLATE_MATCHING_COLOR

// constant for variable arMatchingPCAMode
#define  AR_MATCHING_WITHOUT_PCA      0
#define  AR_MATCHING_WITH_PCA         1
#define  DEFAULT_MATCHING_PCA_MODE          AR_MATCHING_WITHOUT_PCA


// constants influencing accuracy of arGetTransMat(...)
#define   AR_GET_TRANS_MAT_MAX_LOOP_COUNT         5
#define   AR_GET_TRANS_MAT_MAX_FIT_ERROR          1.0
// criterium for arGetTransMatCont(...) to call 
// arGetTransMat(...) instead
#define   AR_GET_TRANS_CONT_MAT_MAX_FIT_ERROR     1.0

// min/max area of fiducial interiors to be matched
// against templates, used in arDetectMarker.c
#define   AR_AREA_MAX      100000
#define   AR_AREA_MIN          70

// used in arDetectMarker2(...), this param controls the
// maximum number of potential markers evaluated further.
// Only the first AR_SQUARE_MAX patterns are examined.
//#define   AR_SQUARE_MAX        50
// plays some role in arDetectMarker2 I don't understand yet
#define   AR_CHAIN_MAX      10000
// maximum number of markers that can be simultaneously loaded
//#define   AR_PATT_NUM_MAX      50 

// These parameters control the way the toolkit warps a found
// marker to a perfect square. The square has size 
// AR_PATT_SIZE_X * AR_SIZE_PATT_Y, the projected
// square in the image is subsampled at a min of
// AR_PATT_SIZE_X/Y and a max of AR_PATT_SAMPLE_NUM
// steps in both x and y direction
//#define   AR_PATT_SIZE_X       16
//#define   AR_PATT_SIZE_Y       16
//#define   AR_PATT_SAMPLE_NUM   64

// Constants controlling the behavior of
// atParamGet (for calibrating HMDs and cameras)
// max/min number of feature points to match 2D<->3D
#define   AR_PARAM_NMIN         6
#define   AR_PARAM_NMAX      1000
#define   AR_PARAM_C34        100.0

#define   EVEC_MAX     10
#define	  P_MAX       500

// this defines the maximum screen width that
// can be processed by artoolkit...
// memory consumption (if static) is: 2*width*height bytes
#ifdef _WIN32_WCE
  #define MAX_BUFFER_WIDTH  320
  #define MAX_BUFFER_HEIGHT 240
#else
  #define MAX_BUFFER_WIDTH  720
  #define MAX_BUFFER_HEIGHT 576
#endif //_WIN32_WCE

//#define WORK_SIZE   1024*32


//#define SMALL_LUM8_TABLE

#ifdef SMALL_LUM8_TABLE
  #define getLUM8_from_RGB565(ptr)   RGB565_to_LUM8_LUT[ (*(unsigned short*)(ptr))>>6 ]
#else
  #define getLUM8_from_RGB565(ptr)   RGB565_to_LUM8_LUT[ (*(unsigned short*)(ptr))    ]
#endif //SMALL_LUM8_TABLE


#if defined(_MSC_VER) || defined(_WIN32_WCE)
#  include <windows.h>
#else
// for linux no MAX_PATH variable is set
#  define MAX_PATH 512
#endif


#endif //  AR_CONFIG_H

