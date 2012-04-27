///////////////////////////////////////////////////////////////////////////////////
// FreeFrameSample.cpp
//
// FreeFrame Open Video Plugin
// C Version
//
// Implementation of the Free Frame sample plugin
//
// www.freeframe.org
// marcus@freeframe.org

/*
Copyright (c) 2002, Marcus Clements www.freeframe.org
All rights reserved.

FreeFrame 1.0 upgrade by Russell Blakeborough
email: boblists@brightonart.org

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

   * Redistributions of source code must retain the above copyright
     notice, this list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright
     notice, this list of conditions and the following disclaimer in
     the documentation and/or other materials provided with the
     distribution.
   * Neither the name of FreeFrame nor the names of its
     contributors may be used to endorse or promote products derived
     from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

// includes
#include "Template.h"
#include <string.h>
#include <stdio.h>


// Plugin Globals
PlugInfoStruct GPlugInfo;
PlugExtendedInfoStruct GPlugExtInfo;
ParamConstsStruct GParamConstants[NUM_PARAMS];


PlugInfoStruct* getInfo()
{
	GPlugInfo.APIMajorVersion = 1;		// number before decimal point in version nums
	GPlugInfo.APIMinorVersion = 000;		// this is the number after the decimal point
										// so version 0.511 has major num 0, minor num 501
	char ID[5] = "FFT1";		 // this *must* be unique to your plugin
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "Template";

	memcpy(GPlugInfo.uniqueID, ID, 4);
	memcpy(GPlugInfo.pluginName, name, 16);
	GPlugInfo.pluginType = FF_EFFECT;

	return &GPlugInfo;
}

LPVOID getExtendedInfo()
{
	GPlugExtInfo.PluginMajorVersion = 1;
	GPlugExtInfo.PluginMinorVersion = 10;

	// I'm just passing null for description etc for now
	// todo: send through description and about
	GPlugExtInfo.Description = NULL;
	GPlugExtInfo.About = NULL;

	// FF extended data block is not in use by the API yet
	// we will define this later if we want to
	GPlugExtInfo.FreeFrameExtendedDataSize = 0;
	GPlugExtInfo.FreeFrameExtendedDataBlock = NULL;

	return (LPVOID) &GPlugExtInfo;
}

DWORD getPluginCaps(DWORD index)
{
	switch (index) {

	case FF_CAP_16BITVIDEO:
		return FF_FALSE;

	case FF_CAP_24BITVIDEO:
		return FF_TRUE;

	case FF_CAP_32BITVIDEO:
		return FF_TRUE;

	case FF_CAP_PROCESSFRAMECOPY:
		return FF_FALSE;

	case FF_CAP_MINIMUMINPUTFRAMES:
		return NUM_INPUTS;

	case FF_CAP_MAXIMUMINPUTFRAMES:
		return NUM_INPUTS;

	case FF_CAP_COPYORINPLACE:
		return FF_FALSE;

	default:
		return FF_FALSE;
	}
}


DWORD initialise()
{
    // populate the parameters constants structs
    GParamConstants[0].Type = 10;
	GParamConstants[1].Type = 10;
	GParamConstants[2].Type = 10;

    GParamConstants[0].Default = 0.5f;
	GParamConstants[1].Default = 0.5f;
	GParamConstants[2].Default = 0.5f;

	char tempName1[17] = "Red";
	char tempName2[17] = "Green";
	char tempName3[17] = "Blue";
	memcpy(GParamConstants[0].Name, tempName1, 16);
	memcpy(GParamConstants[1].Name, tempName2, 16);
	memcpy(GParamConstants[2].Name, tempName3, 16);

	return FF_SUCCESS;
}

DWORD deInitialise()
{
	return FF_SUCCESS;
}

LPVOID instantiate(VideoInfoStruct* pVideoInfo)
{
	// Create local pointer to plugObject
	plugClass *pPlugObj;
	// create new instance of plugClass
	pPlugObj = new plugClass;

	// make a copy of the VideoInfoStruct
	pPlugObj->FVideoInfo.frameWidth = pVideoInfo->frameWidth;
	pPlugObj->FVideoInfo.frameHeight = pVideoInfo->frameHeight;
	pPlugObj->FVideoInfo.bitDepth = pVideoInfo->bitDepth;

	// this shouldn't happen if the host is checking the capabilities properly
	pPlugObj->FVideoMode = pPlugObj->FVideoInfo.bitDepth;
	if (pPlugObj->FVideoMode >2 || pPlugObj->FVideoMode < 0) {
	  return (LPVOID) FF_FAIL;
	}

	// return pointer to object cast as LPVOID
	return (LPVOID) pPlugObj;
}

DWORD deInstantiate(LPVOID instanceID)
{
	// declare pPlugObj - pointer to this instance
	plugClass *pPlugObj;

	// typecast LPVOID into pointer to a plugClass
	pPlugObj = (plugClass*) instanceID;

	delete pPlugObj; // todo: ? success / fail?

	return FF_SUCCESS;
}

DWORD getNumParameters()
{
	return NUM_PARAMS;
}

char* getParameterName(DWORD index)
{
	return GParamConstants[index].Name;
}

float getParameterDefault(DWORD index)
{
	return GParamConstants[index].Default;
}

unsigned int getParameterType(DWORD index)
{
	return GParamConstants[index].Type;
}


/////////////////////////////////////////////////////////////////////////////////


char* plugClass::getParameterDisplay(DWORD index)
{
	// fill the array with spaces first
	for (int n=0; n<16; n++) {
		FParams[index].DisplayValue[n] = ' ';
	}
	sprintf(FParams[index].DisplayValue, "%f",FParams[index].Value);
	return FParams[index].DisplayValue;
}

DWORD plugClass::setParameter(SetParameterStruct* pParam)
{
	FParams[pParam->index].Value = pParam->value;
	return FF_SUCCESS;
}

float plugClass::getParameter(DWORD index)
{
	return FParams[index].Value;
}

DWORD plugClass::processFrame(LPVOID pFrame)
{
	switch (FVideoInfo.bitDepth) {
		case 1:
			return processFrame24Bit(pFrame);
		case 2:
			return processFrame32Bit(pFrame);
		default:
			return FF_FAIL;
	}
}

DWORD plugClass::processFrame24Bit(LPVOID pFrame)
{
	VideoPixel24bit* pPixel = (VideoPixel24bit*) pFrame;
	for (DWORD x = 0; x < FVideoInfo.frameWidth; x++) {
	  for (DWORD y = 0; y < FVideoInfo.frameHeight; y++) {
	    // this is very slow! Should be a lookup table
	    pPixel->red = (BYTE) (pPixel->red * FParams[0].Value);
	    pPixel->green = (BYTE) (pPixel->green * FParams[1].Value);
	    pPixel->blue = (BYTE) (pPixel->blue * FParams[2].Value);
	    pPixel++;
	  }
	}

	return FF_SUCCESS;
}

DWORD plugClass::processFrame32Bit(LPVOID pFrame)
{
	VideoPixel32bit* pPixel = (VideoPixel32bit*) pFrame;
	for (DWORD x = 0; x < FVideoInfo.frameWidth; x++) {
	  for (DWORD y = 0; y < FVideoInfo.frameHeight; y++) {
	    // this is very slow! Should be a lookup table
	    pPixel->red = (BYTE) (pPixel->red * FParams[0].Value);
	    pPixel->green = (BYTE) (pPixel->green * FParams[1].Value);
	    pPixel->blue = (BYTE) (pPixel->blue * FParams[2].Value);
	    pPixel++;
	  }
	}

	return FF_SUCCESS;
}

DWORD plugClass::processFrameCopy(ProcessFrameCopyStruct* pFrameData)
{
	if (pFrameData->numInputFrames<NUM_INPUTS) {
		return FF_FAIL;
	}

	switch (FVideoInfo.bitDepth) {
		case 1:
			return processFrameCopy24Bit(pFrameData);
		case 2:
			return processFrameCopy32Bit(pFrameData);
		default:
			return FF_FAIL;
	}
}

DWORD plugClass::processFrameCopy24Bit(ProcessFrameCopyStruct* pFrameData)
{
	VideoPixel24bit* pInputPixel = (VideoPixel24bit*) pFrameData->InputFrames[0];
	VideoPixel24bit* pOutputPixel = (VideoPixel24bit*) pFrameData->OutputFrame;
	for (DWORD x = 0; x < FVideoInfo.frameWidth; x++) {
	  for (DWORD y = 0; y < FVideoInfo.frameHeight; y++) {
	    // this is very slow! Should be a lookup table
	    pOutputPixel->red = (BYTE) (pInputPixel->red * FParams[0].Value);
	    pOutputPixel->green = (BYTE) (pInputPixel->green * FParams[1].Value);
	    pOutputPixel->blue = (BYTE) (pInputPixel->blue * FParams[2].Value);
	    pInputPixel++;
		pOutputPixel++;
	  }
	}

	return FF_SUCCESS;
}


DWORD plugClass::processFrameCopy32Bit(ProcessFrameCopyStruct* pFrameData)
{
	VideoPixel32bit* pInputPixel = (VideoPixel32bit*) pFrameData->InputFrames[0];
	VideoPixel32bit* pOutputPixel = (VideoPixel32bit*) pFrameData->OutputFrame;
	for (DWORD x = 0; x < FVideoInfo.frameWidth; x++) {
	  for (DWORD y = 0; y < FVideoInfo.frameHeight; y++) {
	    // this is very slow! Should be a lookup table
	    pOutputPixel->red = (BYTE) (pInputPixel->red * FParams[0].Value);
	    pOutputPixel->green = (BYTE) (pInputPixel->green * FParams[1].Value);
	    pOutputPixel->blue = (BYTE) (pInputPixel->blue * FParams[2].Value);
	    pInputPixel++;
		pOutputPixel++;
	  }
	}

	return FF_SUCCESS;
}
