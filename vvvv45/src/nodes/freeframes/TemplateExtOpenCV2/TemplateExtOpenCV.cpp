//////project name
//TemplateExtOpenCV

//////description
//freeframe plugin.
//simple template using freeframe 1.0 extended functionality
//for spreaded in- and outputs
//see: http://vvvv.org/documentation/freeframeextendedspecification
//and OpenCV

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//c++/Code::Blocks

//////initial author
//joreg -> joreg@gmx.at

//////edited by
//your name here

//includes
#include "TemplateExtOpenCV.h"

// Plugin Globals
PlugInfoStruct GPlugInfo;
PlugExtendedInfoStruct GPlugExtInfo;
ParamConstsStruct GParamConstants[NUM_PARAMS];
OutputConstsStruct GOutputConstants[NUM_OUTPUTS];

//OpenCV error handling defines:
#define CV_ErrModeLeaf    0
#define CV_ErrModeParent  1
#define CV_ErrModeSilent  2


PlugInfoStruct* getInfo()
{
	GPlugInfo.APIMajorVersion = 1;		// number before decimal point in version nums
	GPlugInfo.APIMinorVersion = 1;		// this is the number after the decimal point
										// so version 0.511 has major num 0, minor num 501
	char ID[5] = "FFTO";		 // this *must* be unique to your plugin
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "TmpltExtOpenCV";

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

DWORD initialise()
{
    //cvSetErrMode(CV_ErrModeSilent);

    // populate the parameters constants structs
    GParamConstants[0].Type = 20;

	GParamConstants[0].Default = 0.0f;

	char tempName0[17] = "Dummy Spread";
	memcpy(GParamConstants[0].Name, tempName0, 16);

    // populate the output structs
    GOutputConstants[0].Type = 10;

	char outName0[17] = "Dummy Spread Out";
	memcpy(GOutputConstants[0].Name, outName0, 16);

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

	// return pointer to the plugin instance object we have created
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

/////////////////////////////////////////////////////////////////////////////////


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

    InitializeCriticalSection(&CriticalSection);
}

void plugClass::init()
{
    FImageSize.width = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    // -> setting defaults for input values //
    for (int in=0; in<NUM_PARAMS; in++)
        FParams[in].Value=GParamConstants[in].Default;

    // -> allocating image buffers  //
  //  FCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
  //  FGrayImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);

   /*
    char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);*/
}

plugClass::~plugClass()
{
    //cvReleaseImage(&FGrayImage);

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

	return FF_SUCCESS;
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
       // case 0: memcpy(FOutputs[0].Spread, FPixelCount, 256 * sizeof(float));
    }
    return FOutputs[index].Spread;
}

static Scalar randomColor(RNG& rng)
{
    int icolor = (unsigned)rng;
    return Scalar(icolor&255, (icolor>>8)&255, (icolor>>16)&255);
}

DWORD plugClass::processFrame(LPVOID pFrame)
{
    EnterCriticalSection(&CriticalSection);

    //*** enrico code ***//
	cv::Mat curFrame;
	int width = FVideoInfo.frameWidth;
	int height = FVideoInfo.frameHeight;
    int cvType = CV_MAKETYPE(CV_8U, 3);
	curFrame = cv::Mat(width, height, cvType, (char*)pFrame, 0);

    int lineType = CV_AA; // change it to 8 to see non-antialiased graphics
    int x1 = -width/2, x2 = width*3/2, y1 = -height/2, y2 = height*3/2;
	RNG rng(0xFFFFFFFF);
	Point center;
    center.x = 50;
    center.y = 100;
    cv::circle(curFrame, center, 50, randomColor(rng), rng.uniform(-1, 9), lineType);


	//another way to do it could be something like
/*
	int cvType = CV_MAKETYPE(CV_8U,3);
	curFrame = cv::Mat(width,height,cvType,(char*)pFrame,0);

	update(curFrame);


	//cv::Mat curFrame = cv::Mat(320,240,cv::CV_MAKETYPE(cv),char,0);


    //convert input image to opencv-image
    FCurrentImage->origin = 1;
    FCurrentImage->imageData = (char*)pFrame;
*/
    //use opencv functions
   // cvCvtColor(FCurrentImage, FGrayImage, CV_BGR2GRAY);
   // cvCvtColor(FGrayImage, FCurrentImage, CV_GRAY2BGR);

    //extract some data

    LeaveCriticalSection(&CriticalSection);

	return FF_SUCCESS;
}

DWORD plugClass::processFrameCopy(ProcessFrameCopyStruct* pFrameData)
{
    return FF_FAIL;
}




