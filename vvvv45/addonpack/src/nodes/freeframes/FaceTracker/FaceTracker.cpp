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
#include "FaceTracker.h"
#include <stdio.h>

using namespace std;

// Plugin Globals
PlugInfoStruct GPlugInfo;
PlugExtendedInfoStruct GPlugExtInfo;
ParamConstsStruct GParamConstants[NUM_PARAMS];
OutputConstsStruct GOutputConstants[NUM_OUTPUTS];


PlugInfoStruct* getInfo()
{
	GPlugInfo.APIMajorVersion = 1;		// number before decimal point in version nums
	GPlugInfo.APIMinorVersion = 0;		// this is the number after the decimal point
	// so version 0.511 has major num 0, minor num 501
	char ID[5] = "JFFT";		 // this *must* be unique to your plugin
	// see www.freeframe.org for a list of ID's already taken
	char name[17] = "FaceTracker";

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

string filemask = "tracker (*.tracker)|*.tracker";
DWORD initialise()
{
    // populate the parameters constants structs
    GParamConstants[0].Type = 100;
    GParamConstants[1].Type = 10;
	GParamConstants[2].Type = 10;
	GParamConstants[3].Type = 10;
	GParamConstants[4].Type = 10;
	GParamConstants[5].Type = 0;
	GParamConstants[6].Type = 1;

    int i = (int)&filemask[0];
    float* fp = (float*)&i;
 	GParamConstants[0].Default = *fp;
    GParamConstants[1].Default = 1;
	GParamConstants[2].Default = 0.1;
	GParamConstants[3].Default = 0.75;
	GParamConstants[4].Default = 0.01;
	GParamConstants[5].Default = 0;
	GParamConstants[6].Default = 0;

    char tempName0[17] = "Tracker Model";
    char tempName1[17] = "Rescale";
    char tempName2[17] = "Iterations";
    char tempName3[17] = "Clamp";
    char tempName4[17] = "Tolerance";
    char tempName5[17] = "Auto Initialize";
	char tempName6[17] = "Initialize";
	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);
	memcpy(GParamConstants[2].Name, tempName2, 16);
	memcpy(GParamConstants[3].Name, tempName3, 16);
	memcpy(GParamConstants[4].Name, tempName4, 16);
	memcpy(GParamConstants[5].Name, tempName5, 16);
	memcpy(GParamConstants[6].Name, tempName6, 16);

	// populate the output structs
    GOutputConstants[0].Type = 20;
    GOutputConstants[1].Type = 0;

	char outName0[17] = "Vertices XYZ";
	char outName1[17] = "Is Tracked";
	memcpy(GOutputConstants[0].Name, outName0, 16);
	memcpy(GOutputConstants[1].Name, outName1, 16);

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

	pPlugObj->init();

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


/////////////////////////////////////////////////////////////////////////////////

plugClass::plugClass()
{
    InitializeCriticalSection(&CriticalSection);
}

plugClass::~plugClass()
{
    //cvReleaseImage(&FGrayImage);

    for (int i=0; i<NUM_OUTPUTS; i++) free(FOutputs[i].Spread);

    DeleteCriticalSection(&CriticalSection);
}

void plugClass::init()
{
    FImageSize.width = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    // -> setting defaults for input values //
    for (int in=0; in<NUM_PARAMS; in++)
        FParams[in].Value = GParamConstants[in].Default;

	FIsTracked = false;
	FFrameSkip = -1;

	FWindowSize1.resize(1);
	FWindowSize1[0] = 7;

	FWindowSize2.resize(3);
	FWindowSize2[0] = 11;
	FWindowSize2[1] = 9;
	FWindowSize2[2] = 7;

	//initialize outputs
    FOutputs[0].SliceCount = 0;
    FOutputs[0].Spread = (float*) calloc(FOutputs[0].SliceCount, sizeof(float));

    FOutputs[1].SliceCount = 1;
    FOutputs[1].Spread = (float*) calloc(FOutputs[1].SliceCount, sizeof(float));

    FNewTracker = false;
    FTrackerLoaded = false;
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

bool hasEnding (std::string const &fullString, std::string const &ending)
{
    if (fullString.length() >= ending.length())
    {
        return (0 == fullString.compare (fullString.length() - ending.length(), ending.length(), ending));
    }
    else
    {
        return false;
    }
}

bool fexists(const char *filename)
{
  ifstream ifile(filename);
  return ifile;
}

DWORD plugClass::setParameter(SetParameterStruct* pParam)
{
	FParams[pParam->index].Value = pParam->value;

	if (pParam->index == 0)
	{
        float f = pParam->value;
	    int* ip = (int*)&f;
	    char* cp = (char*)*ip;

        FFilename = cp;

        if (hasEnding (FFilename, ".tracker") && fexists(&FFilename[0]))
            FNewTracker = true;
	}
	if (pParam->index == 1)
        FParams[1].Value = max(0.1, (double)FParams[1].Value);

	return FF_SUCCESS;
}

void plugClass::loadTracker()
{
	FTracker.Load(FFilename.c_str());
	FOutputs[0].SliceCount = FTracker._shape.rows / 2 * 3;
    FOutputs[0].Spread = (float*) realloc(FOutputs[0].Spread, sizeof(float) * FOutputs[0].SliceCount);

  /* char buffer[999];
    sprintf(buffer, "ERROR: Could not load classifier cascade\n%s", &FFilename[0]);
    OutputDebugString(buffer);
*/
    FNewTracker = false;
    FTrackerLoaded = true;
}

// -> Function is called when spread input values (types 20, 21 or 22) are modified //
DWORD plugClass::setInput(InputStruct* pParam)
{
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

DWORD plugClass::setThreadLock(DWORD Enter)
{
	if (*(bool*) Enter)
	  EnterCriticalSection(&CriticalSection);
    else
      LeaveCriticalSection(&CriticalSection);

    return FF_SUCCESS;
}

float* plugClass::getOutput(DWORD index)
{
    switch(index)
    {
       //compute the return values for the given index
       case 0:
       {
           if (FDataIsNew)
           {
               int n = FFaceData.rows / 2;
               float* slice = FOutputs[0].Spread;
               double d;
               float f;
               for (int i=0; i<n; i++)
               {
                  d = FFaceData.at<double>(i,0) * 1/FParams[1].Value;
                  f = d / FImageSize.width - 0.5;
                  *slice = f;
                  slice++;
                  d = FFaceData.at<double>(i+n,0) * 1/FParams[1].Value;
                  f = -(d / FImageSize.height - 0.5);
                  *slice = f;
                  slice++;
                  *slice = 0;
                  slice++;
               }

               FDataIsNew = false;
           }
           break;
       }
       case 1:
       {
           *FOutputs[1].Spread = (float)FIsTracked;
           break;
       }
    }
    return FOutputs[index].Spread;
}

DWORD plugClass::processFrame(LPVOID pFrame)
{
    if (FNewTracker)
        loadTracker();

	//turn pFrame into opencv image
	cv::Mat frame = cv::Mat(FImageSize.height, FImageSize.width, CV_8UC3, (char*)pFrame, 0);

    //for some reason needs a vertically flipped image
    cv::flip(frame, frame, 0);

	//track da face
	if (FTrackerLoaded)
        update(frame);

    //and a vertical backflip
    cv::flip(frame, frame, 0);

	return FF_SUCCESS;
}

//the function where tracking happens
bool plugClass::update(cv::Mat image)
{
	if(FParams[1].Value == 1)
	{
		FCurrentImage = image;
	}
	else
	{
		cv::resize(image, FCurrentImage, cv::Size(FParams[1].Value * image.cols, FParams[1].Value*image.rows));
	}

	cv::cvtColor(FCurrentImage, FCurrentGray, CV_RGB2GRAY);

    vector<int>wSize;
    if(FIsTracked)
    {
        wSize = FWindowSize1;
    }
    else
    {
        wSize = FWindowSize2;
    }

    if (FParams[6].Value >= 0.5)
    {
        FIsTracked = false;
        FTracker.FrameReset();
    }
    else
    {
        bool fcheck = FParams[5].Value >= 0.5;

        try
        {
            int iterations = FParams[2].Value * 50;
            double clamp = FParams[3].Value * 4;
            double tolerance = FParams[4].Value;

            if (FTracker.Track(FCurrentGray, wSize, FFrameSkip, iterations, clamp, tolerance, fcheck) == 0)
            {
                EnterCriticalSection(&CriticalSection);
                    FFaceData = FTracker._shape.clone();
                    FIsTracked = true;
                    FDataIsNew = true;
                LeaveCriticalSection(&CriticalSection);
            }
            else
            {
                EnterCriticalSection(&CriticalSection);
                    FIsTracked = false;
                LeaveCriticalSection(&CriticalSection);

                if (fcheck)
                    FTracker.FrameReset();
            }
        }
        catch( cv::Exception& e )
        {
            //this will sometimes throw error -215:
            //OpenCV Error: Assertion failed
            //(0 <= roi.x && 0 <= roi.width && roi.x + roi.width <= m.cols && 0 <= roi.y && 0 <= roi.height && roi.y + roi.height <= m.rows)
            //in Mat, file matrix.cpp, line 303
            //just need to catch that in order not to crash
        }
    }

	return FIsTracked;
}

DWORD plugClass::processFrameCopy(ProcessFrameCopyStruct* pFrameData)
{
    return FF_FAIL;
}
