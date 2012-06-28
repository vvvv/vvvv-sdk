//////project name
//Trautner

//////description
//freeframe plugin.
//simple movement detection in regions defined via a grayscale bitmap.
//based on an idea by mr. trautner -> http://www.brainsalt.net

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//c++/CodeBlocks 10.05

//////dependencies
//opencv v2.31
//http://opencv.willowgarage.com/wiki

//////initial author
//joreg -> joreg@gmx.at

//////edited by
//your name here

//includes
#include "Trautner.h"

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
	GParamConstants[3].Type = 0;    //show filtered
	GParamConstants[4].Type = 10;   //3->4
	GParamConstants[5].Type = 100;  //4->5
	GParamConstants[6].Type = 1;    //reload mask

	GParamConstants[0].Default = 0.0f;
	GParamConstants[1].Default = 0.0f;
	GParamConstants[2].Default = 0.0f;
	GParamConstants[3].Default = 1.0f;
	GParamConstants[4].Default = 0.1f;
	GParamConstants[6].Default = 0.0f;

	int i = (int)&filemask[0];
    float* fp = (float*)&i;
 	GParamConstants[5].Default = *fp;

	char tempName0[17] = "Hold Background";
	char tempName1[17] = "Dark Background";
	char tempName2[17] = "Show Mask";
	char tempName3[17] = "Show Filtered";
	char tempName4[17] = "Threshold";
	char tempName5[17] = "Mask Image";
	char tempName6[17] = "Reload Mask";

	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);
	memcpy(GParamConstants[2].Name, tempName2, 16);
	memcpy(GParamConstants[3].Name, tempName3, 16);
	memcpy(GParamConstants[4].Name, tempName4, 16);
	memcpy(GParamConstants[5].Name, tempName5, 16);
	memcpy(GParamConstants[6].Name, tempName6, 16);

    // populate the output structs
    GOutputConstants[0].Type = 0;
    GOutputConstants[1].Type = 0;

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

    FNewMask = true;

    InitializeCriticalSection(&CriticalSection);
}

void plugClass::init()
{
    FImageSize.width = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    // -> setting defaults for input values //
    for (int in=0; in<NUM_PARAMS; in++) FParams[in].Value=GParamConstants[in].Default;

    // -> allocating image buffers  //
    FCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    FGrayImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    FLastImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    FMask = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);

  /*  char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);*/
}

plugClass::~plugClass()
{
    cvReleaseImage(&FGrayImage);
    cvReleaseImage(&FLastImage);
    cvReleaseImage(&FMask);

    for (int i=0; i<NUM_OUTPUTS; i++) free(FOutputs[i].Spread);

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

	if (pParam->index == 5)
	{
	    float f = pParam->value;
	    int* ip = (int*)&f;
	    char* cp = (char*)*ip;

        FFilename = cp;
        /*	char buffer[100];
        sprintf(buffer, "new file: %s", &FFilename[0]);
        OutputDebugString(buffer);*/

        FNewMask = true;
	}
	if ((pParam->index == 6) && (pParam->value >= 0.5))
        FNewMask = true;

	return FF_SUCCESS;
}

void plugClass::loadMask()
{
    IplImage* tmp;

    tmp = cvLoadImage(&FFilename[0], 0);

    if (tmp != 0)
    {
        cvFlip(tmp, tmp, 1);
        cvResize(tmp, FMask, CV_INTER_NN);
        cvReleaseImage(&tmp);
    }

    FNewMask = false;
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
        case 0: memcpy(FOutputs[0].Spread, FPixelCount, 256 * sizeof(float));
        case 1: memcpy(FOutputs[1].Spread, FChangedPixels, 256 * sizeof(float));
    }
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

    if (FNewMask)
        loadMask();

    FCurrentImage->origin = 1;
    FCurrentImage->imageData = (char*)pFrame;

    cvCvtColor(FCurrentImage, FGrayImage, CV_BGR2GRAY);

    if (FParams[0].Value < 0.5) //substract two consecutive images
    {
        IplImage* tmp = cvCloneImage(FGrayImage);

        cvSub(FGrayImage, FLastImage, FGrayImage);
        cvReleaseImage(&FLastImage);
        FLastImage = cvCloneImage(tmp);

        cvReleaseImage(&tmp);
    }
    else //hold background
    {
        if (FParams[1].Value < 0.5)  //bright background
        {
            cvSub(FLastImage, FGrayImage, FGrayImage);
        }
        else    //dark background
        {
            cvSub(FGrayImage, FLastImage, FGrayImage);
        }
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
            if (gray[j] > FParams[4].Value * 255)
                FChangedPixels[mask[j]]++;
        }

        mask += step;  // next line
        gray += step;
    }

    if (FParams[2].Value >= 0.5)   //show mask
        cvAdd(FGrayImage, FMask, FGrayImage);
    if (FParams[3].Value >= 0.5)   //show filtered
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


