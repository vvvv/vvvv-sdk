//////project name
//FiducialTracker

//////description
//freeframe plugin.
//implemenation of Ross Bencina's fidtrack library 
//for tracking of fiducial markers

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//dev-c++ 5

//////dependencies
//libFidTrack as provided by:
//http://www.iua.upf.es/mtg/reactable/?software

//////initiative stressing to do it + editing
//benedikt -> benedikt@looksgood.de

//////initial author
//joreg -> joreg@gmx.at

//includes
#include "FiducialTracker.h"

#include <stdio.h>
#include <stdlib.h>

//libfidtrack
#include "tiled_bernsen_threshold.h"
#include "segment.h"
#include "fidtrackX.h"

// Plugin Globals
PlugInfoStruct GPlugInfo;
PlugExtendedInfoStruct GPlugExtInfo;
ParamConstsStruct GParamConstants[NUM_PARAMS];
OutputConstsStruct GOutputConstants[NUM_OUTPUTS];

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
    // populate the parameters constants structs
    GParamConstants[0].Type = 0;
    GParamConstants[1].Type = 10;

	GParamConstants[0].Default = 1.0f;
	GParamConstants[1].Default = 40.0f;

	char tempName0[17] = "Show Thresholded";
	char tempName1[17] = "Threshold";
	
	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);

    // populate the output structs
    GOutputConstants[0].Type = 0;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;

	char outName0[17] = "ID";
	char outName1[17] = "X";
	char outName2[17] = "Y";
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
    for (register int i=0; i<NUM_OUTPUTS; i++)
        {
         FOutputs[i].SliceCount = 0;
         FOutputs[i].Spread = (float*) calloc(1, 0);
        }
        
     char *tree_config = "none";
	//char *grid_config = "none";

    FWidth = FVideoInfo.frameWidth;
    FHeight = FVideoInfo.frameHeight;
    // FOutputs[0].Spread = (float*) calloc(FOutputs[0].SliceCount, sizeof(float));
    FThreshedImage = (unsigned char*) calloc(FWidth*FHeight, sizeof(unsigned char));
    FImage = (unsigned char*) calloc(FWidth*FHeight, sizeof(unsigned char));

    //initialize the thresholder
    FThresholder = new TiledBernsenThresholder();
	initialize_tiled_bernsen_thresholder(FThresholder, FWidth, FHeight, 16);
	
	//initialize the tracker
//	FDmap = (FloatPoint*) calloc(FWidth*FHeight, sizeof(FloatPoint));
	
/*	if (strcmp(tree_config,"none")!=0) 
    {	initialize_treeidmap_from_file(&FTreeidmap, tree_config);}
    else */
        initialize_treeidmap(&FTreeidmap);
	
	initialize_segmenter(&FSegmenter, FWidth, FHeight, FTreeidmap.max_adjacencies); 
	initialize_fidtrackerX(&FFidtrackerx, &FTreeidmap, 0);   

    InitializeCriticalSection(&CriticalSection);
}

void plugClass::init()
{

    // -> setting defaults for input values //
    for (int in=0; in<NUM_PARAMS; in++) FParams[in].Value=GParamConstants[in].Default;

  /*  char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);*/

    
}


plugClass::~plugClass()
{
    for (int i=0; i<NUM_OUTPUTS; i++) free(FOutputs[i].Spread);
    
    terminate_tiled_bernsen_thresholder(FThresholder);
    
    terminate_segmenter(&FSegmenter);
    terminate_treeidmap(&FTreeidmap); 

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
}

float* plugClass::getOutput(DWORD index)
{
  /* switch (index) 
   {
     case 0: 
     { 
          for (int i=0; i<FFiducialCount; i++)
            FOutputs[index].Spread[i] = FFiducials[i].id;
     }
     case 1: 
     { 
          for (int i=0; i<FFiducialCount; i++)
            FOutputs[index].Spread[i] = FFiducials[i].x;
     }
     case 2: 
     { 
          for (int i=0; i<FFiducialCount; i++)
            FOutputs[index].Spread[i] = FFiducials[i].y;
     }
     case 3: 
     { 
          for (int i=0; i<FFiducialCount; i++)
            FOutputs[index].Spread[i] = FFiducials[i].angle;
     }
   }  */ 
  
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
    int srcBytes = 3;

    //thresholding
    tiled_bernsen_threshold(FThresholder, FThreshedImage, (unsigned char*) pFrame, srcBytes, FWidth,  FHeight, 16, (int) FParams[1].Value);
    
    //copy thresholded image back to output
    if ((bool) FParams[0].Value == true)
    {
        unsigned char* src = (unsigned char*) FThreshedImage;
        char* dst = (char*) pFrame;
        for (int i=0; i<FWidth*FHeight; i++)
        {
          *dst = *src;
          dst++;
          *dst = *src;
          dst++;
          *dst = *src;
          dst++;
          src++;
        }                        
    }
    
    //segmentation
    step_segmenter(&FSegmenter, FThreshedImage, FWidth, FHeight);

    //fiducial recognition
    FFiducialCount = find_fiducialsX(FFiducials, MAX_FIDUCIAL_COUNT, &FFidtrackerx, &FSegmenter, FWidth, FHeight); 

    //make the outputs fit
    for (int i=0; i<NUM_OUTPUTS; i++)
    {
        if (FOutputs[i].SliceCount != FFiducialCount)
        {
          FOutputs[i].SliceCount = FFiducialCount;
          FOutputs[i].Spread     = (float*) realloc(FOutputs[i].Spread, sizeof(float) * FFiducialCount);
        }
    }
        
    //fill the outputs
    for (DWORD i=0; i<FFiducialCount; i++)
    {
          FOutputs[0].Spread[i] = (int) FFiducials[i].id;    
          FOutputs[1].Spread[i] = FFiducials[i].x / FWidth - 0.5;
          FOutputs[2].Spread[i] = FFiducials[i].y / FHeight - 0.5;
          FOutputs[3].Spread[i] = FFiducials[i].angle;
    }

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
	char ID[5] = "JFFT";		 // this *must* be unique to your plugin
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "FiducialTracker";

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


