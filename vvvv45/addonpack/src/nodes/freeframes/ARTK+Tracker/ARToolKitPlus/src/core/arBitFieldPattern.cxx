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
 * $Id: arBitFieldPattern.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/arBitFieldPattern.h>

#include <assert.h>
#include <memory.h>


namespace ARToolKitPlus {


static void
applyMaskSimple(IDPATTERN& nPattern)
{
	nPattern ^= fullMask;
}


static void
applyMaskBCH(IDPATTERN& nPattern)
{
	nPattern ^= bchMask;
}


static bool
isBitSet(IDPATTERN pat, int which)
{
	return ((pat>>which)&1) != 0;
}

/*
static void
setBit(IDPATTERN& pat, int which)
{
	static IDPATTERN one = 1;
	pat |= (one<<which);
}
*/

/*
static void
clearBit(IDPATTERN& pat, int which)
{
	static IDPATTERN one = 1;
	pat &= ~(one<<which);
}
*/

static void
rotate90CW(IDPATTERN& nPattern)
{
	IDPATTERN one=1, tmpPat = nPattern;
	nPattern = 0;

	for(int i=0; i<pattBits; i++)
	{
		if(isBitSet(tmpPat, rotate90[i]))
			nPattern |= (one<<i);
	}
}


static void
generatePatternSimple(int nID, IDPATTERN& nPattern)
{
	IDPATTERN tmpPat = nID & idMask;

	nPattern = (tmpPat<<posMask0) | (tmpPat<<posMask1) | (tmpPat<<posMask2) | (tmpPat<<posMask3);
	applyMaskSimple(nPattern);
}


static void
generatePatternBCH(int nID, IDPATTERN& nPattern)
{
	BCH bchProcessor;
	_64bits encoded;

	bchProcessor.encode(encoded, nID);

	nPattern = encoded;
	applyMaskBCH(nPattern);
}


static float
checkPatternBitSimple(IDPATTERN nPattern, int nBit, int& nValue)
{
	int b0 = (int)(nPattern>>(posMask0+nBit))&1,
		b1 = (int)(nPattern>>(posMask1+nBit))&1,
		b2 = (int)(nPattern>>(posMask2+nBit))&1,
		b3 = (int)(nPattern>>(posMask3+nBit))&1,
		sum = b0+b1+b2+b3;

	switch(sum)
	{
	case 0:
		nValue = 0;
		return 1.00f;
	case 1:
		nValue = 0;
		return 0.50f;
	case 2:
		nValue = 0;			// opt for zero since one means white area which can happen due to reflectance
		return 0.00f;
	case 3:
		nValue = 1;
		return 0.50f;
	case 4:
		nValue = 1;
		return 1.00f;
	}

	// should never come here...
	//assert(false);
	return 0.0f;
}


static void
checkPatternSimple(IDPATTERN nPattern, int& nID, float& nProp)
{
	nProp = 0.0f;
	nID = 0;

	applyMaskSimple(nPattern);

	for(int i=0; i<idBits; i++)
	{
		int bitvalue = 0;
		nProp += checkPatternBitSimple(nPattern, i, bitvalue);

		nID |= (bitvalue<<i);
	}

	nProp /= (float)idBits;

	if(nProp<0.9f)
		nProp = 0.0f;
}


static void
checkPatternBCH(IDPATTERN nPattern, int& nID, float& nProp, BCH* nProcessor)
{
	int err = -1;
	_64bits decodedPattern = 0;
	unsigned int andMask = (1<<bchBits)-1;

	nProp = 0.0f;
	nID = 0;

	applyMaskBCH(nPattern);

	nProcessor->decode(err, decodedPattern, nPattern);

	nID = (int)(decodedPattern & andMask);

	switch(err)
	{
	case 0:
		nProp = 1.00f;
		break;

	case 1:
		nProp = 0.75f;
		break;

	case 2:
		nProp = 0.50f;
		break;

	case 3:
		nProp = 0.25f;
		break;

	default:
		nProp = 0.0f;
		break;
	}
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::downsamplePattern(ARUint8* data, unsigned char* imgPtr)
{
	int x,y;

	if(PATTERN_WIDTH==18 && PATTERN_HEIGHT==18)
	{
		// this code simply takes the center pixel of each 3x3 cell
		// this is the fastest method for 18x18 to convert down to 6x6 but gives worst results
		// even more: this method gives no advantage over directly using a pattern size of 6x6.
		//
/*
		for(y=1; y<PATTERN_HEIGHT; y+=3)
			for(x=1; x<PATTERN_WIDTH; x+=3)
			{
				int idx = (y*PATTERN_WIDTH+x)*3;
				*imgPtr++ = (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;
			}
*/

		// this code downsamples the 18x18 RGB24 image to 6x6 LUM8 by
		// averaging each 3x3 cell into a single pixel
		//
		for(y=0; y<PATTERN_HEIGHT; y+=3)
			for(x=0; x<PATTERN_WIDTH; x+=3)
			{
				int idx = (y*PATTERN_WIDTH+x)*3, val=0;
				val = (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += PATTERN_WIDTH*3 - 6;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += PATTERN_WIDTH*3 - 6;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				*imgPtr++ = val/9;
			}

/*
		FILE* fp = fopen("dump18x18.raw", "wb");
		fwrite(data, PATTERN_HEIGHT*PATTERN_WIDTH*3, 1, fp);
		fclose(fp);

		fp = fopen("dump6x6.raw", "wb");
		fwrite(patimg, 6*6, 1, fp);
		fclose(fp);
*/
	}
	else
	if(PATTERN_WIDTH==12 && PATTERN_HEIGHT==12)
	{
		// this code downsamples the 12x12 RGB24 image to 6x6 LUM8 by
		// averaging each 2x2 cell into a single pixel
		//
		for(y=0; y<PATTERN_HEIGHT; y+=2)
			for(x=0; x<PATTERN_WIDTH; x+=2)
			{
				int idx = (y*PATTERN_WIDTH+x)*3, val=0;
				val = (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += PATTERN_WIDTH*3 - 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				idx += 3;
				val += (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

				*imgPtr++ = val/4;
			}

/*
		FILE* fp = fopen("dump12x12.raw", "wb");
		fwrite(data, PATTERN_HEIGHT*PATTERN_WIDTH*3, 1, fp);
		fclose(fp);

		fp = fopen("dump6x6.raw", "wb");
		fwrite(patimg, 6*6, 1, fp);
		fclose(fp);
*/
	}
	else
	if(PATTERN_WIDTH==6 && PATTERN_HEIGHT==6)
	{
		// this code simply converts the 6x6 RGB pattern into a
		// 6x6 greyscale image
		//
		for(int idx=0; idx<PATTERN_WIDTH*PATTERN_HEIGHT*3; idx+=3)
			*imgPtr++ = (data[idx+0]+(data[idx+1]<<1)+data[idx+2])>>2;

/*
	FILE* fp = fopen("dump6x6.raw", "wb");
	fwrite(data, PATTERN_HEIGHT*PATTERN_WIDTH, 1, fp);
	fclose(fp);
*/
	}
	else
	{
		// the pattern size has to be 18x18, 12x12 or 6x6
		// for performance reasons generic downsampling is not supported
		assert((PATTERN_WIDTH==18 && PATTERN_HEIGHT==18) || (PATTERN_WIDTH==12 && PATTERN_HEIGHT==12) || (PATTERN_WIDTH==6 && PATTERN_HEIGHT==6));
		return -1;
	}

	return 0;
}



AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::bitfield_check_simple( ARUint8 *data, int *code, int *dir, ARFloat *cf, int thresh)
{
	assert(sizeof(IDPATTERN)>=8 && "IDPATTERN must be at least 64-bit integer");

	unsigned char patimg[idPattWidth*idPattHeight], *imgPtr=patimg;
	int i;

	// first step is to reduce the pattern to 6x6. only the following
	// three resolutions are supported:
	//   - 18x18 (downsampling of each 3x3 cell)
	//   - 12x12 (downsampling of each 2x2 cell)
	//   -  6x6  (no downsampling)
	//
	if(downsamplePattern(data, imgPtr)==-1)
	{
		*code = 0;
		*dir = 0;
		*cf = -1.0f;
		return -1;
	}


	// now we do a thresholding and create the IDPATTERN bitfield
	//
	IDPATTERN pat=0, one=1;

	for(i=0; i<pattBits; i++)
		if(patimg[pattBits-1-i]>thresh)
			pat |= one<<i;


	// finally we check all four rotations and take the best one
	// if it is good enough
	//
	IDPATTERN	pat0, pat90, pat180, pat270;
	int			id0=-1,id90=-1,id180=-1,id270=-1;
	float		prop0=0.0f,prop90=0.0f,prop180=0.0f,prop270=0.0f;

	pat0 = pat;
	checkPatternSimple(pat0, id0, prop0);

	pat90 = pat0;
	rotate90CW(pat90);
	checkPatternSimple(pat90, id90, prop90);

	pat180 = pat90;
	rotate90CW(pat180);
	checkPatternSimple(pat180, id180, prop180);

	pat270 = pat180;
	rotate90CW(pat270);
	checkPatternSimple(pat270, id270, prop270);

	if(prop0>=prop90 && prop0>=prop180 && prop0>=prop270)		// is prop0 maximum?
	{
		*dir = 0;
		*cf = prop0;
		*code = id0;
	}
	else
	if(prop90>=prop0 && prop90>=prop180 && prop90>=prop270)		// is prop90 maximum?
	{
		*dir = 1;
		*cf = prop90;
		*code = id90;
	}
	else
	if(prop180>=prop0 && prop180>=prop90 && prop180>=prop270)	// is prop180 maximum?
	{
		*dir = 2;
		*cf = prop180;
		*code = id180;
	}
	else
	if(prop270>=prop0 && prop270>=prop90 && prop270>=prop180)	// is prop270 maximum?
	{
		*dir = 3;
		*cf = prop270;
		*code = id270;
	}
	else
	{
		assert(false);
	}
	
	return 0;
}


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::bitfield_check_BCH( ARUint8 *data, int *code, int *dir, ARFloat *cf, int thresh)
{
	assert(sizeof(IDPATTERN)>=8 && "IDPATTERN must be at least 64-bit integer");

	unsigned char patimg[idPattWidth*idPattHeight], *imgPtr=patimg;
	int i;


	// first step is to reduce the pattern to 6x6. only the following
	// three resolutions are supported:
	//   - 18x18 (downsampling of each 3x3 cell)
	//   - 12x12 (downsampling of each 2x2 cell)
	//   -  6x6  (no downsampling)
	//
	if(downsamplePattern(data, imgPtr)==-1)
	{
		*code = 0;
		*dir = 0;
		*cf = -1.0f;
		return -1;
	}


	// now we do a thresholding and create the IDPATTERN bitfield
	//
	IDPATTERN pat=0, one=1;

	for(i=0; i<pattBits; i++)
		if(patimg[pattBits-1-i]>thresh)
			pat |= one<<i;


	// finally we check all four rotations and take the best one
	// if it is good enough
	//
	IDPATTERN	pat0, pat90, pat180, pat270;
	int			id0=-1,id90=-1,id180=-1,id270=-1;
	float		prop0=0.0f,prop90=0.0f,prop180=0.0f,prop270=0.0f;

	if(bchProcessor==NULL)
		bchProcessor = new BCH;

	pat0 = pat;
	checkPatternBCH(pat0, id0, prop0, bchProcessor);

	pat90 = pat0;
	rotate90CW(pat90);
	checkPatternBCH(pat90, id90, prop90, bchProcessor);

	pat180 = pat90;
	rotate90CW(pat180);
	checkPatternBCH(pat180, id180, prop180, bchProcessor);

	pat270 = pat180;
	rotate90CW(pat270);
	checkPatternBCH(pat270, id270, prop270, bchProcessor);

	if(prop0>=prop90 && prop0>=prop180 && prop0>=prop270)		// is prop0 maximum?
	{
		*dir = 0;
		*cf = prop0;
		*code = id0;
	}
	else
	if(prop90>=prop0 && prop90>=prop180 && prop90>=prop270)		// is prop90 maximum?
	{
		*dir = 1;
		*cf = prop90;
		*code = id90;
	}
	else
	if(prop180>=prop0 && prop180>=prop90 && prop180>=prop270)	// is prop180 maximum?
	{
		*dir = 2;
		*cf = prop180;
		*code = id180;
	}
	else
	if(prop270>=prop0 && prop270>=prop90 && prop270>=prop180)	// is prop270 maximum?
	{
		*dir = 3;
		*cf = prop270;
		*code = id270;
	}
	else
	{
		assert(false);
	}
	
	return 0;
}



}  // namespace ARToolKitPlus
