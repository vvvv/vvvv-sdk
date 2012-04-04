//////project name
//DetectObject

//////description
//freeframe plugin.
//implemenation of openCVs cvHaarDetectObjects function
//for patter/object (e.g. face detection)

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//c++/CodeBlocks 10.05

//////dependencies
//opencv v2.31
//http://opencv.willowgarage.com/wiki

//////initiative stressing to do it + editing
//benedikt -> benedikt@looksgood.de

//////initial author
//joreg -> joreg@gmx.at

//includes
#include "DetectObject.h"

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

string filemask = "XML (*.xml)|*.xml";
DWORD initialise()
{
    cvSetErrMode(CV_ErrModeSilent);

    // populate the parameters constants structs
    GParamConstants[0].Type = 0;
	GParamConstants[1].Type = 100;
	GParamConstants[2].Type = 10;
	GParamConstants[3].Type = 10;
	GParamConstants[4].Type = 10;
	GParamConstants[5].Type = 0;
	GParamConstants[6].Type = 0;
	GParamConstants[7].Type = 10;

	GParamConstants[0].Default = 1.0f;

	int i = (int)&filemask[0];
    float* fp = (float*)&i;
 	GParamConstants[1].Default = *fp;

 	GParamConstants[2].Default = 2.0f;
 	GParamConstants[3].Default = 0.2f;
 	GParamConstants[4].Default = 40.0f;
 	GParamConstants[5].Default = 1.0f;
 	GParamConstants[6].Default = 1.0f;
 	GParamConstants[7].Default = 10.0f;


	char tempName0[17] = "Show Rectangle";
	char tempName1[17] = "Training File";
	char tempName2[17] = "Min Neighbors";
	char tempName3[17] = "Scale Cascade";
	char tempName4[17] = "Min Face Size";
	char tempName5[17] = "Canny Pruning";
	char tempName6[17] = "Unique ID";
	char tempName7[17] = "Lifetime";

	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);
	memcpy(GParamConstants[2].Name, tempName2, 16);
	memcpy(GParamConstants[3].Name, tempName3, 16);
	memcpy(GParamConstants[4].Name, tempName4, 16);
	memcpy(GParamConstants[5].Name, tempName5, 16);
	memcpy(GParamConstants[6].Name, tempName6, 16);
	memcpy(GParamConstants[7].Name, tempName7, 16);

    // populate the output structs
    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;
    GOutputConstants[4].Type = 0;

	char outName0[17] = "X";
	char outName1[17] = "Y";
	char outName2[17] = "Width";
	char outName3[17] = "Height";
	char outName4[17] = "Object ID";
	memcpy(GOutputConstants[0].Name, outName0, 16);
	memcpy(GOutputConstants[1].Name, outName1, 16);
	memcpy(GOutputConstants[2].Name, outName2, 16);
	memcpy(GOutputConstants[3].Name, outName3, 16);
	memcpy(GOutputConstants[4].Name, outName4, 16);

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
    FOutputs[0].SliceCount = 0;
    FOutputs[1].SliceCount = 0;
    FOutputs[2].SliceCount = 0;
    FOutputs[3].SliceCount = 0;
    FOutputs[4].SliceCount = 0;
    FOutputs[0].Spread = (float*) calloc(FOutputs[0].SliceCount, sizeof(float));
    FOutputs[1].Spread = (float*) calloc(FOutputs[1].SliceCount, sizeof(float));
    FOutputs[2].Spread = (float*) calloc(FOutputs[2].SliceCount, sizeof(float));
    FOutputs[3].Spread = (float*) calloc(FOutputs[3].SliceCount, sizeof(float));
    FOutputs[4].Spread = (float*) calloc(FOutputs[3].SliceCount, sizeof(float));

    FStorage = 0;
    FCascade = 0;

    FNewCascade = false;

    FFaces_new = (Obj*) malloc(1 * sizeof(Obj));
    FFaces_old = (Obj*) malloc(1 * sizeof(Obj));
    FSortlist = (int*) calloc(1, sizeof(int));

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
    FCopy = cvCreateImage(FImageSize, IPL_DEPTH_8U, 3);

    FIDs_old = (int*) malloc(sizeof(int));
    FIDs_new = (int*) malloc(sizeof(int));

    FFirstRound  = true;
    FMaxID = 0;

  /*  char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);*/
}

plugClass::~plugClass()
{
    cvReleaseImage(&FCopy);

    if (FStorage)
      cvReleaseMemStorage(&FStorage);
    if (FCascade)
      cvReleaseHaarClassifierCascade(&FCascade);

    for (int i=0; i<NUM_OUTPUTS; i++) free(FOutputs[i].Spread);

    free(FFaces_new);
    free(FFaces_old);

    free (FSortlist);
    free (FIDs_old);
    free (FIDs_new);

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

	if (pParam->index == 1)
	{
        float f = pParam->value;
	    int* ip = (int*)&f;
	    char* cp = (char*)*ip;

        FFilename = cp;

        FNewCascade = true;
	}
	return FF_SUCCESS;
}

void plugClass::loadCascade()
{
    if (FStorage)
      cvReleaseMemStorage(&FStorage);
    FStorage = cvCreateMemStorage(0);

    if (FCascade)
      cvReleaseHaarClassifierCascade(&FCascade);

    FCascade = (CvHaarClassifierCascade*)cvLoad(&FFilename[0], 0, 0, 0 );

    if(!FCascade)
    {
     char buffer[999];
     sprintf(buffer, "ERROR: Could not load classifier cascade\n%s", &FFilename[0]);
     OutputDebugString(buffer);
    }

    FNewCascade = false;
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

    if (FNewCascade)
        loadCascade();

    FCurrentImage->origin = 1;
    FCurrentImage->imageData = (char*)pFrame;
    if(FCascade)
    {
        int scale = 1;
        CvPoint pt1, pt2;

        int i,j,refacecountfound = 0;
        bool cannyflag = false;
        float distance = 0.0;

        //CV_HAAR_DO_CANNY_PRUNING flag
        cannyflag = (FParams[6].Value < 0.00f);

        //if scale_factor = 1 -> crash ... so we do not allow this
        float scale_factor = 1.0f + FParams[3].Value;
        if (FParams[3].Value < 0.01f)
        {
           scale_factor = 1.01f;
        }

        //for some reason the image has to be flipped for haardetect
        cvFlip( FCurrentImage, FCopy, 0 );

        cvClearMemStorage(FStorage);
        //http://www.cs.bham.ac.uk/resources/courses/robotics/doc/opencvdocs/ref/OpenCVRef_Experimental.htm#decl_cvHaarDetectObjects
        CvSeq* faces = cvHaarDetectObjects(FCopy, FCascade, FStorage,
                                           scale_factor, int(FParams[2].Value), cannyflag,
                                           cvSize(int(FParams[4].Value), int(FParams[4].Value)) );

        //handle faces ids, duplicates and lifetime
        if (faces)
        {
            FFaceCount_old = FFaceCount_new;
            FFaceCount_new = faces->total;

            FIDs_old = (int*) realloc(FIDs_old, sizeof(int) * FFaceCount_old);
            FFaces_old = (Obj*) realloc(FFaces_old, sizeof(Obj) * FFaceCount_old);

            if (FFirstRound)
            {
                FIDs_new = (int*) realloc(FIDs_new, sizeof(int) * FFaceCount_new);
                for (int i=0; i<FFaceCount_new; i++)
                    FIDs_new[i] = i;
            }

           //move last frames faces to _old list
            for (int i=0; i<FFaceCount_old; i++)
            {
                FFaces_old[i] = FFaces_new[i];
                FIDs_old[i] = FIDs_new[i];
            }

            //fill Objlist_new with data from cvHaarDetectObjects
            FNewFaceList.clear();
            Obj tmp;
            CvRect *r;
            for(int i = 0; i < FFaceCount_new; i++ )
            {
                r = (CvRect*)cvGetSeqElem(faces, i);

                tmp.x = ((float) (r->x + (float) r->width / 2) / FImageSize.width) - 0.5;
                tmp.x = ((float) (r->x + (float) r->width / 2) / FImageSize.width) - 0.5;
                tmp.y = 1 - ((float) (r->y + (float) r->height / 2) / FImageSize.height) - 0.5;
                tmp.width = (float) r->width / FImageSize.width;
                tmp.height = (float) r->height / FImageSize.height;
                tmp.lostframes = 0;
                tmp.found = false;

                FNewFaceList.push_back(tmp);
            }

            //go through all newly found and remove doubles (faces that are too close together)
            int next = 0;
            for(FFaceIter=FNewFaceList.begin(); FFaceIter!=FNewFaceList.end(); ++FFaceIter)
            //for(int i=0; i<FNewFaceList.size(); i++)
            {
                next++;
                //if point is already marked -> skip this loop
                if (!FFaceIter->found)
                {
                    //for(FFaceIter2=FNewFaceList.begin(); FFaceIter2!=FNewFaceList.end(); ++FFaceIter2)
                    for(int i=next; i<FNewFaceList.size(); i++)
                    {

                        //calculate distance between every point
                        distance = sqrtf(pow(FFaceIter->x - FNewFaceList.at(i).x, 2) + pow(FFaceIter->y - FNewFaceList.at(i).y, 2));

                        //mark point
                        if ((distance < FFaceIter->width/2)
                        && (distance < FFaceIter->height/2))
                        {
                            FNewFaceList.at(i).found = true;
                        }
                    }
                }
            }

            //remove all doubles from the list
            for(FFaceIter = FNewFaceList.begin(); FFaceIter!=FNewFaceList.end();)
            {
                if (FFaceIter->found)
                    FFaceIter = FNewFaceList.erase(FFaceIter);
                else
                    FFaceIter++;
            }

           //for each old face go through all the newly found and see if we find a close one, if not increase lostframecount
            for (int i=0; i<FFaceCount_old; i++)
            {
                FFaces_old[i].lostframes++;
                for(FFaceIter=FNewFaceList.begin(); FFaceIter!=FNewFaceList.end(); ++FFaceIter)
                {
                    distance = sqrtf(pow(FFaces_old[i].x - FFaceIter->x, 2) + pow(FFaces_old[i].y - FFaceIter->y, 2));
                    if (distance < FFaces_old[i].width / 2)
                    {
                        FFaces_old[i].lostframes = 0;
                        break;
                    }
                }

                //old face that does not exceed old framelost_threshold is added to newlist
                if ((FFaces_old[i].lostframes > 0)
                && (FFaces_old[i].lostframes < (int) FParams[7].Value))
                {
                    //add it to the newly found
                    FNewFaceList.push_back(FFaces_old[i]);
                }
            }

            FFaceCount_new = FNewFaceList.size();
            FFaces_new = (Obj*) realloc(FFaces_new, sizeof(Obj) * FFaceCount_new);
            FIDs_new = (int*) realloc(FIDs_new, sizeof(int) * FFaceCount_new);
            FSortlist = (int*) realloc(FSortlist, sizeof(int) * FFaceCount_new);

            //put list elements into array for further processing
            for(int i=0; i<FNewFaceList.size(); i++)
            {
                FFaces_new[i] = FNewFaceList.at(i);
            }

            //do adapt ids
            if (!FFirstRound)
            {   //sort face ids
                if (FParams[6].Value < 0.5)
                    FMaxID = 0;

                adaptindex(FFaces_old, FFaces_new, FFaceCount_old, FFaceCount_new, FIDs_old, FIDs_new, FSortlist, &FMaxID, FParams[6].Value >= 0.5);
            }

            FFirstRound=false;

            if (FFaceCount_new != FFaceCount_old)
            {
                FOutputs[0].SliceCount = FFaceCount_new;
                FOutputs[1].SliceCount = FFaceCount_new;
                FOutputs[2].SliceCount = FFaceCount_new;
                FOutputs[3].SliceCount = FFaceCount_new;
                FOutputs[4].SliceCount = FFaceCount_new;
                FOutputs[0].Spread = (float*) realloc(FOutputs[0].Spread, FFaceCount_new * sizeof(float));
                FOutputs[1].Spread = (float*) realloc(FOutputs[1].Spread, FFaceCount_new * sizeof(float));
                FOutputs[2].Spread = (float*) realloc(FOutputs[2].Spread, FFaceCount_new * sizeof(float));
                FOutputs[3].Spread = (float*) realloc(FOutputs[3].Spread, FFaceCount_new * sizeof(float));
                FOutputs[4].Spread = (float*) realloc(FOutputs[4].Spread, FFaceCount_new * sizeof(float));
            }

            for(int i = 0; i < FFaceCount_new; i++ )
            {
                //output to vvvv
                FOutputs[0].Spread[i] = FFaces_new[i].x;
                FOutputs[1].Spread[i] = FFaces_new[i].y;
                FOutputs[2].Spread[i] = FFaces_new[i].width;
                FOutputs[3].Spread[i] = FFaces_new[i].height;
                FOutputs[4].Spread[i] = FIDs_new[i];

                //show rectangle if enabled -> draw rect
                if (FParams[0].Value > 0)
                {
                    //convert from coordinaten system (-0.5 to +0.5) to cv like coordinaten system (0 to ImageSize)
                    pt1.x = int(float(FImageSize.width)*(FFaces_new[i].x - (FFaces_new[i].width/2) + 0.5));
                    pt2.x = int(float(FImageSize.width)*(FFaces_new[i].x + (FFaces_new[i].width/2) + 0.5));
                    pt1.y = FImageSize.height - int(float(FImageSize.height)*(1-(FFaces_new[i].y + FFaces_new[i].height/2 + 0.5)));
                    pt2.y = FImageSize.height - int(float(FImageSize.height)*( 1-(FFaces_new[i].y - FFaces_new[i].height/2 + 0.5)) );

                    cvRectangle(FCurrentImage, pt1, pt2, CV_RGB(255,0,0), 3, 8, 0);
                }
            }/**/
        }
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
	char ID[5] = "JFFD";		 // this *must* be unique to your plugin
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "DetectObject";

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


