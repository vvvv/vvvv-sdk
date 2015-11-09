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
//c++/CodeBlocks 10.05

//////dependencies
//libFidTrack as provided with the reacTVision source:
//http://www.iua.upf.es/mtg/reactable/?software

//////initial author
//joreg -> joreg@gmx.at

//includes
#include "FiducialTracker.h"
#include "FiducialObject.h"

#include <stdio.h>
#include <stdlib.h>
#include <list>

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
	GParamConstants[1].Default = 0.5f;

	char tempName0[17] = "Show Thresholded";
	char tempName1[17] = "Threshold";

	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);

    // populate the output structs
    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;

	char outName0[17] = "Fiducial ID";
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

    FWidth = FVideoInfo.frameWidth;
    FHeight = FVideoInfo.frameHeight;

    FThreshedImage = (unsigned char*) calloc(FWidth*FHeight, sizeof(unsigned char));
    FImage = (unsigned char*) calloc(FWidth*FHeight, sizeof(unsigned char));

    //initialize the thresholder
    FThresholder = new TiledBernsenThresholder();
	initialize_tiled_bernsen_thresholder(FThresholder, FWidth, FHeight, 16);

	//initialize the tracker
    initialize_treeidmap(&FTreeidmap);

	initialize_segmenter(&FSegmenter, FWidth, FHeight, FTreeidmap.max_adjacencies);
	initialize_fidtrackerX(&FFidtrackerx, &FTreeidmap, 0);

    InitializeCriticalSection(&CriticalSection);
}

void plugClass::init()
{
    // -> setting defaults for input values //
    for (int in=0; in<NUM_PARAMS; in++) FParams[in].Value=GParamConstants[in].Default;
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

    return FF_SUCCESS;
}

float* plugClass::getOutput(DWORD index)
{
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
    tiled_bernsen_threshold(FThresholder, FThreshedImage, (unsigned char*) pFrame, srcBytes, FWidth,  FHeight, 16, (int) (FParams[1].Value * 127));

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
    step_segmenter(&FSegmenter, FThreshedImage);//, FWidth, FHeight);

    //fiducial recognition
    FFiducialCount = find_fiducialsX(FFiducials, MAX_FIDUCIAL_COUNT, &FFidtrackerx, &FSegmenter, FWidth, FHeight);

    std::list<FiducialObject> new_fiducials;

	//process newly found symbols
	for(DWORD i=0; i<FFiducialCount; i++)
    {
		FiducialObject *existing_fiducial = NULL;

		// update objects we had in the last frame
		// also check if we have an ID/position conflict
		// or correct an INVALID_FIDUCIAL_ID if we had an ID in the last frame
		for (fiducial = fiducialList.begin(); fiducial!=fiducialList.end(); fiducial++)
        {
			float distance = fiducial->distance(FFiducials[i].x, FFiducials[i].y);

			if (FFiducials[i].id == fiducial->fiducial_id)
            {
				// find and match a fiducial we had last frame already ...
				if(existing_fiducial)
                {
					if (distance<existing_fiducial->distance(FFiducials[i].x, FFiducials[i].y))
						existing_fiducial = &(*fiducial);
				}
                else
                {
					existing_fiducial = &(*fiducial);
					for (DWORD j=0; j<FFiducialCount; j++)
                    {
						if ((i!=j) && (fiducial->distance(FFiducials[j].x, FFiducials[j].y)<distance))
                        {
							existing_fiducial = NULL;
							break;
						}
					}
				}
			}
            else if (distance<5.0f)
            {
				// do we have a different ID at the same place?

				// this should correct wrong or invalid fiducial IDs
				// assuming that between two frames
				// there can't be a rapid exchange of two symbols
				// at the same place

                /*
				if (fiducials[i].id==INVALID_FIDUCIAL_ID) printf("corrected invalid ID to %d at %f %f\n",fiducials[i].id,fiducials[i].x/width,fiducials[i].y/height);
				else if (fiducials[i].id!=pos->classId) printf("corrected wrong ID from %d to %d at %f %f\n",fiducials[i].id,fiducial->fiducial_id,fiducials[i].x/fiducials[i].y,ypos/height);
				*/

				FFiducials[i].id = fiducial->fiducial_id;
				existing_fiducial = &(*fiducial);
				break;
			}
		}

		if  (existing_fiducial != NULL)
        {
			// just update the fiducial from last frame ...
			existing_fiducial->update(FFiducials[i].x, FFiducials[i].y, FFiducials[i].angle);
        }
        else if (FFiducials[i].id != INVALID_FIDUCIAL_ID)
        {
			//add the newly found object
			//FiducialObject addFiducial(session_id, fiducials[i].id, width, height);
			FiducialObject *addFiducial = new FiducialObject(0, FFiducials[i].id, 0, 0);

			addFiducial->update(FFiducials[i].x, FFiducials[i].y, FFiducials[i].angle);
            //if(msg_listener) {
			//	char add_message[16];
			//	sprintf(add_message,"add obj %d %ld", fiducials[i].id,session_id);
			//	msg_listener->setMessage(std::string(add_message));
			//}
			new_fiducials.push_back(*addFiducial);
			//session_id++;
		}

		//if(FFiducials[i].id!=INVALID_FIDUCIAL_ID && msg_listener)
		//	 msg_listener->setObject(FFiducials[i].id, (int)(FFiducials[i].x), (int)(FFiducials[i].y), FFiducials[i].angle );
	}

	//finally add all newly found objects
	for (std::list<FiducialObject>::iterator iter = new_fiducials.begin(); iter!=new_fiducials.end(); iter++)
    {
		fiducialList.push_back(*iter);
	}

	//see if we can remove any old objects from our list
	for(fiducial = fiducialList.begin(); fiducial!=fiducialList.end();)
    {
		if (fiducial->checkRemoved())
        {
			fiducial = fiducialList.erase(fiducial);
		}
        else
        {
			fiducial++;
		}
	}

    DWORD fidcount = fiducialList.size();

    //make the outputs fit
    for (int i=0; i<NUM_OUTPUTS; i++)
    {
        if (FOutputs[i].SliceCount != fidcount)
        {
          FOutputs[i].SliceCount = fidcount;
          FOutputs[i].Spread     = (float*) realloc(FOutputs[i].Spread, sizeof(float) * fidcount);
        }
    }

    //fill the outputs
    int i=0;
    for (fiducial = fiducialList.begin(); fiducial!=fiducialList.end(); fiducial++)
    {
        FOutputs[0].Spread[i] = (int) fiducial->fiducial_id;
        FOutputs[1].Spread[i] = fiducial->current.xpos / FWidth - 0.5;
        FOutputs[2].Spread[i] = fiducial->current.ypos / FHeight - 0.5;
        FOutputs[3].Spread[i] = fiducial->current.angle / DOUBLEPI;
        i++;
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


