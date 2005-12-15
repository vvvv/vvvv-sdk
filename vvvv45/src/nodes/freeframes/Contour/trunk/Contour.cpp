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

#include "Contour.h"
#include <string.h>
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
	// declare pPlugObj - pointer to this instance
	plugClass *pPlugObj;

	// typecast LPVOID into pointer to a plugClass
	pPlugObj = (plugClass*) instanceID;

	delete pPlugObj; // todo: ? success / fail?
	
	return FF_SUCCESS;
}

DWORD initialise()
{
    cvSetErrMode(CV_ErrModeSilent);
      
    // populate the parameters constants structs
    GParamConstants[0].Type = 10;
	GParamConstants[1].Type = 10;
	GParamConstants[2].Type = 0;
	GParamConstants[3].Type = 10;
    
	GParamConstants[0].Default = 0.5f;
	GParamConstants[1].Default = 0.0f;
	GParamConstants[2].Default = 0.0f;
	GParamConstants[3].Default = 0.01f;

	char tempName0[17] = "Threshold";
	char tempName1[17] = "Levels";
	char tempName2[17] = "Show Contours";
	char tempName3[17] = "Thickness";
	
	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);
	memcpy(GParamConstants[2].Name, tempName2, 16);
	memcpy(GParamConstants[3].Name, tempName3, 16);

    // populate the output structs
    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;

	char outName0[17] = "X";
	char outName1[17] = "Y";
	char outName2[17] = "Contours BinSize";
	char outName3[17] = "Orientation";

	memcpy(GOutputConstants[0].Name, outName0, 16);
	memcpy(GOutputConstants[1].Name, outName1, 16);
	memcpy(GOutputConstants[2].Name, outName2, 16);
	memcpy(GOutputConstants[3].Name, outName3, 16);
	
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

char* getOutputName(DWORD index)
{
	return GOutputConstants[index].Name;
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


plugClass::plugClass()
{
    FStorage = cvCreateMemStorage(0);
    FContours = 0; 
    
    FOutputs[0].SliceCount = 0;
    FOutputs[1].SliceCount = 0;
    FOutputs[2].SliceCount = 0;
    FOutputs[3].SliceCount = 0;
    
    //initialize spreads
    FOutputs[0].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[1].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[2].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[3].Spread = (float*) calloc(1, sizeof(float));
    
    InitializeCriticalSection(&CriticalSection); 
}

void plugClass::init()
{
    FImageSize.width = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    FCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    FGrayImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
}

plugClass::~plugClass()
{
    cvReleaseImage(&FGrayImage);
    cvReleaseMemStorage(&FStorage);
    
    free(FOutputs[0].Spread);
    free(FOutputs[1].Spread);
    free(FOutputs[2].Spread);
    free(FOutputs[3].Spread);
    
    DeleteCriticalSection(&CriticalSection);
}


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
    //only set value if really changed..should better be checked in host!!
    if (pParam->value == FParams[pParam->index].Value) 
        return FF_SUCCESS;
    else
    {
    	FParams[pParam->index].Value = pParam->value;
    	return FF_SUCCESS;
   	} 
}

float plugClass::getParameter(DWORD index)
{
	return FParams[index].Value;
}

DWORD plugClass::getOutputSliceCount(DWORD index)
{
	return FOutputs[index].SliceCount;
}

#define PI 3.14159265

float* plugClass::getOutput(DWORD index)
{
    EnterCriticalSection(&CriticalSection);
    
    if (FContoursChanged) //then compute new output before returning it
    {
        //reallocate fresh memory for outputs
        FOutputs[0].Spread = (float*) realloc(FOutputs[0].Spread, sizeof(float) * FPointCount);
        FOutputs[1].Spread = (float*) realloc(FOutputs[1].Spread, sizeof(float) * FPointCount);
        FOutputs[2].Spread = (float*) realloc(FOutputs[2].Spread, sizeof(float) * FContoursCount);
        FOutputs[3].Spread = (float*) realloc(FOutputs[3].Spread, sizeof(float) * FContoursCount);

        memcpy(FOutputs[2].Spread, FBinSizes, FContoursCount * sizeof(float)); 
               
        int c;
        int p = 0;
        float theta;
        CvPoint* PointArray;
        
        //go through all contours again and output the points
        for(int i=0; i<FContoursCount; i++)
        {
           // if(CV_IS_SEQ_CURVE(contours))
           c = FFirstContour->total;
           
           //allocate some memory
           PointArray = (CvPoint*)malloc(c * sizeof(CvPoint));
                           
           //copy points to the array
           cvCvtSeqToArray(FFirstContour, PointArray, CV_WHOLE_SEQ);
    
           for(int j=0; j<c; j++)
           {
              FOutputs[0].Spread[p] = ((float)PointArray[j].x / FImageSize.width) * 2 - 1;
              FOutputs[1].Spread[p] = ((float)PointArray[j].y / FImageSize.height) * 2 - 1;
              p++;
           } 
                      
           //calculate orientation of contour
           cvMoments(FFirstContour, &FMoments);
           theta = 0.5 * atan2(2 * FMoments.mu11, FMoments.mu20 - FMoments.mu02);
           FOutputs[3].Spread[i] = theta/2/PI;
           
           FFirstContour = FFirstContour->h_next; 
           free(PointArray);
        }
        
        FContoursChanged = false;
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
    
    //threshold the image
    cvThreshold(FGrayImage, FGrayImage, FParams[0].Value * 255, 255, CV_THRESH_BINARY);
    cvCvtColor(FGrayImage, FCurrentImage, CV_GRAY2BGR);

    //clear memory from last round
    cvClearMemStorage(FStorage);

    //find the contours
    FContoursCount = cvFindContours(FGrayImage, FStorage, &FContours, sizeof(CvContour), CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE);
    
    FPointCount = 0;
    free(FBinSizes);
    FBinSizes = (float*) calloc(FContoursCount, sizeof(float));

    //save pointer to firstcontour for use in getOutput
    FFirstContour = FContours;
       
    //go through all contours sum up the number of all points and output binsizes
    for(int i=0; i<FContoursCount; i++)
    {
       if (FParams[2].Value > 0)
         cvDrawContours(FCurrentImage, FContours, CV_RGB(255,0,0), CV_RGB(0,255,0), (int)(FParams[1].Value * 255), (int)(FParams[3].Value * 255), CV_AA);
         
       FPointCount += FContours->total;
       FBinSizes[i] = (float)FContours->total;
       FContours = FContours->h_next;
    }    
     
    //set output slicecounts
    FOutputs[0].SliceCount = FPointCount;
    FOutputs[1].SliceCount = FPointCount;
    FOutputs[2].SliceCount = FContoursCount;
    FOutputs[3].SliceCount = FContoursCount;
  
    //retrieving of points is done in getOutput    

    FContoursChanged = true;
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


PlugInfoStruct* getInfo() 
{
	GPlugInfo.APIMajorVersion = 2;		// number before decimal point in version nums
	GPlugInfo.APIMinorVersion = 000;		// this is the number after the decimal point
										// so version 0.511 has major num 0, minor num 501
	char ID[5] = "JFCT";		 // this *must* be unique to your plugin 
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "Contour";
	
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
