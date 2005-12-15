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

//includes 
#include "Trautner.h"
#include <string.h>
#include <string>
using namespace std;
#include <stdio.h>

// Plugin Globals
PlugInfoStruct GPlugInfo;
PlugExtendedInfoStruct GPlugExtInfo;
ParamConstsStruct GParamConstants[NUM_PARAMS];
OutputConstsStruct GOutputConstants[NUM_OUTPUTS];

#define CV_ErrModeLeaf    0
#define CV_ErrModeParent  1
#define CV_ErrModeSilent  2

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

	pPlugObj->init();

	// Russell - return pointer to the plugin instance object we have created

	// return pointer to object cast as LPVOID 
	return (LPVOID) pPlugObj;
}

DWORD deInstantiate(LPVOID instanceID)
{
    //cvReleaseImage(&last);
	// declare pPlugObj - pointer to this instance
	plugClass *pPlugObj;

	// typecast LPVOID into pointer to a plugClass
	pPlugObj = (plugClass*) instanceID;

	delete pPlugObj; // todo: ? success / fail?
	
	return FF_SUCCESS;
}

string filemask = "Bitmap (*.bmp)|*.bmp";
DWORD initialise()
{
    cvSetErrMode(CV_ErrModeSilent);
    
    // populate the parameters constants structs
    GParamConstants[0].Type = 0;
	GParamConstants[1].Type = 0;
	GParamConstants[2].Type = 0;
	GParamConstants[3].Type = 10;
	GParamConstants[4].Type = 10;
	GParamConstants[5].Type = 10;
	GParamConstants[6].Type = 100;
    
	GParamConstants[0].Default = 0.0f;
	GParamConstants[1].Default = 1.0f;
	GParamConstants[2].Default = 0.0f;
	GParamConstants[3].Default = 0.2f;
	GParamConstants[4].Default = 0.6f;
	GParamConstants[5].Default = 0.1f;

	int i = (int)&filemask[0];
    float* fp = (float*)&i;
 	GParamConstants[6].Default = *fp;
  	
	char tempName0[17] = "Hold Background";
	char tempName1[17] = "Diff Mode";
	char tempName2[17] = "Show Mask";
	char tempName3[17] = "Edge Threshold1";
	char tempName4[17] = "Edge Threshold2";
	char tempName5[17] = "Diff Threshold";
	char tempName6[17] = "Mask Image";
	
	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);
	memcpy(GParamConstants[2].Name, tempName2, 16);
	memcpy(GParamConstants[3].Name, tempName3, 16);
	memcpy(GParamConstants[4].Name, tempName4, 16);
	memcpy(GParamConstants[5].Name, tempName5, 16);
	memcpy(GParamConstants[6].Name, tempName6, 16);

    // populate the output structs
    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    
	char outName0[17] = "PixelCount";
	char outName1[17] = "Changed Pixels";
	memcpy(GOutputConstants[0].Name, outName0, 16);
	memcpy(GOutputConstants[1].Name, outName1, 16);
	
	return FF_SUCCESS;
}

DWORD deInitialise()
{
	return FF_SUCCESS;
}

DWORD getNumParameters()
{
	return NUM_PARAMS;  
}

DWORD getNumOutputs()
{
	return NUM_OUTPUTS;  
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

unsigned int getOutputType(DWORD index)
{
	return GOutputConstants[index].Type;
}

char* getOutputName(DWORD index)
{
	return GOutputConstants[index].Name;
}

plugClass::plugClass()
{
    FOutputs[0].SliceCount = 256;
    FOutputs[0].Spread = (float*) calloc(FOutputs[0].SliceCount, sizeof(float));
    FOutputs[1].SliceCount = 256;
    FOutputs[1].Spread = (float*) calloc(FOutputs[1].SliceCount, sizeof(float));
    
    InitializeCriticalSection(&CriticalSection);    
}

void plugClass::init()
{
    FImageSize.width = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    FCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    FGrayImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    FLastImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    FMask = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    
    /*char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);*/
}

plugClass::~plugClass()
{
    cvReleaseImage(&FGrayImage);
    cvReleaseImage(&FLastImage);
    cvReleaseImage(&FMask);

    free(FOutputs[0].Spread);
    free(FOutputs[1].Spread);
    
    DeleteCriticalSection(&CriticalSection);
}

char* plugClass::getParameterDisplay(DWORD index)
{
	// fill the array with spaces first
	for (int n=0; n<16; n++) 
    {
		FParams[index].DisplayValue[n] = ' ';
	}
	sprintf(FParams[index].DisplayValue, "%f",FParams[index].Value);
	return FParams[index].DisplayValue;
}

DWORD plugClass::setParameter(SetParameterStruct* pParam)
{
	FParams[pParam->index].Value = pParam->value;
	
	if (pParam->index == 6) 	
	{ 	
	    EnterCriticalSection(&CriticalSection); 
        
        float f = pParam->value; 
	    int* ip = (int*)&f;
	    char* cp = (char*)*ip;
          
        IplImage* tmp;  
        tmp = cvLoadImage(cp, 0);
        
        if (tmp != 0)
        {
            cvFlip(tmp, tmp, 1);
            cvResize(tmp, FMask, CV_INTER_LINEAR);
            cvReleaseImage(&tmp);
        }    

	    LeaveCriticalSection(&CriticalSection); 
	}    
	return FF_SUCCESS;
}

float plugClass::getParameter(DWORD index)
{
	return FParams[index].Value;
}

DWORD plugClass::getOutputSliceCount(DWORD index)
{
	return FOutputs[index].SliceCount;
}

float* plugClass::getOutput(DWORD index)
{ 
    EnterCriticalSection(&CriticalSection); 
    switch(index) 
    {
        case 0: memcpy(FOutputs[0].Spread, FPixelCount, 256 * sizeof(float));
        case 1: memcpy(FOutputs[1].Spread, FChangedPixels, 256 * sizeof(float));
    }    
    LeaveCriticalSection(&CriticalSection);	
    return FOutputs[index].Spread;
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
    EnterCriticalSection(&CriticalSection);

    
  
        FCurrentImage->origin = 1;
        FCurrentImage->imageData = (char*)pFrame;
        
        cvCvtColor(FCurrentImage, FGrayImage, CV_BGR2GRAY);
    
        if (FParams[1].Value == 0)
          cvCanny(FGrayImage, FGrayImage, FParams[3].Value * 255, FParams[4].Value * 255, 3);
        
        if (FParams[0].Value == 0)
        {
            IplImage* tmp = cvCloneImage(FGrayImage);
            
            cvSub(FGrayImage, FLastImage, FGrayImage);
            cvReleaseImage(&FLastImage);
            FLastImage = cvCloneImage(tmp);
        
            cvReleaseImage(&tmp);
            
        }
        else
        {
            cvSub(FGrayImage, FLastImage, FGrayImage); 
        }    

        for (int i=0;i<256;i++)
        {
          FPixelCount[i] = 0;
          FChangedPixels[i] = 0;
        }
        
        int h = FGrayImage->height;
        int w = FGrayImage->width * 1; //FGrayImage->nChannels;
        int step= FGrayImage->widthStep; // because of alignment
        
        // because imageData is a signed char*
        unsigned char *mask = reinterpret_cast<unsigned char *>(FMask->imageData);
        unsigned char *gray = reinterpret_cast<unsigned char *>(FGrayImage->imageData);

        for (int i=0; i<h; i++) 
        {
          for (int j=0; j<w; j += 1) 
          {
            FPixelCount[mask[j]]++;
            if (gray[j] > FParams[5].Value * 255) 
              FChangedPixels[mask[j]]++;
          }
          mask += step;  // next line
          gray += step;
        }

        if (FParams[2].Value > 0)
          cvAdd(FGrayImage, FMask, FGrayImage);
          
        cvCvtColor(FGrayImage, FCurrentImage, CV_GRAY2BGR);

    
    LeaveCriticalSection(&CriticalSection);

	return FF_SUCCESS;
}

DWORD plugClass::processFrame32Bit(LPVOID pFrame)
{
	return FF_FAIL;
}

DWORD plugClass::processFrameCopy(ProcessFrameCopyStruct* pFrameData)
{
    return FF_FAIL;
}

DWORD plugClass::processFrameCopy24Bit(ProcessFrameCopyStruct* pFrameData)
{
	return FF_FAIL;
}

DWORD plugClass::processFrameCopy32Bit(ProcessFrameCopyStruct* pFrameData)
{
	return FF_FAIL;
}

DWORD getPluginCaps(DWORD index)
{
	switch (index) {

	case FF_CAP_16BITVIDEO:
		return FF_FALSE;

	case FF_CAP_24BITVIDEO:
		return FF_TRUE;

	case FF_CAP_32BITVIDEO:
		return FF_FALSE;

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

PlugInfoStruct* getInfo() 
{
	GPlugInfo.APIMajorVersion = 2;		// number before decimal point in version nums
	GPlugInfo.APIMinorVersion = 000;		// this is the number after the decimal point
										// so version 0.511 has major num 0, minor num 501
	char ID[5] = "JFTN";		 // this *must* be unique to your plugin 
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "Trautner";
	
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


