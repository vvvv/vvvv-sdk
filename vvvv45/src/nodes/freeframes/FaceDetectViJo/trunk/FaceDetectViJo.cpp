//////project name
//face detection in Paul Viola and Michael Jones Style
//http://scholar.google.com/scholar?q=face%20detection%20viola%20jones&hl=de&lr=&oi=scholart

//////description
//freeframe plugin for face detection in vvvv
//vvvv.meso.net

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//dev-c++ 5

//////dependencies
//opencv beta5 libraries:
//http://sourceforge.net/projects/opencvlibrary

//////initiative stressing to do it
//benedikt -> benedikt@looksgood.de

//////initial author
//joreg -> joreg@gmx.at

//includes 
#include "FaceDetectViJo.h"

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
    
	GParamConstants[0].Default = 1.0f;	

	int i = (int)&filemask[0];
    float* fp = (float*)&i;
 	GParamConstants[1].Default = *fp;
 	
 	GParamConstants[2].Default = 2.0f;
 	GParamConstants[3].Default = 0.2f;
 	GParamConstants[4].Default = 40.0f;
  	
	char tempName0[17] = "Show Rectangle";
	char tempName1[17] = "Training File";
	char tempName2[17] = "Min Neighbors";
	char tempName3[17] = "Scale Cascade";
	char tempName4[17] = "Min Face Size";
	
	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);
	memcpy(GParamConstants[2].Name, tempName2, 16);
	memcpy(GParamConstants[3].Name, tempName3, 16);
	memcpy(GParamConstants[4].Name, tempName4, 16);

    // populate the output structs
    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;
	char outName0[17] = "X";
	char outName1[17] = "Y";
	char outName2[17] = "Width";
	char outName3[17] = "Height";
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
    FOutputs[0].SliceCount = 1;
    FOutputs[1].SliceCount = 1;
    FOutputs[2].SliceCount = 1;
    FOutputs[3].SliceCount = 1;
    FOutputs[0].Spread = (float*) calloc(FOutputs[0].SliceCount, sizeof(float));
    FOutputs[1].Spread = (float*) calloc(FOutputs[1].SliceCount, sizeof(float));
    FOutputs[2].Spread = (float*) calloc(FOutputs[2].SliceCount, sizeof(float));
    FOutputs[3].Spread = (float*) calloc(FOutputs[3].SliceCount, sizeof(float));
 
    newCascade = true;
    
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

  /*  char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);*/
}

plugClass::~plugClass()
{
    cvReleaseImage(&FCurrentImage);
    cvReleaseImage(&FCopy);
    cvReleaseMemStorage(&FStorage);

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
	
	if (pParam->index == 1) 	
	{ 	    
        float f = pParam->value; 
	    int* ip = (int*)&f;
	    char* cp = (char*)*ip;

        memcpy(&Filename[0], cp, strlen(cp)+1);

        newCascade = true;
	}    
	return FF_SUCCESS;
}




void plugClass::loadCascade()
{
    cvReleaseMemStorage(&FStorage);
    FStorage = cvCreateMemStorage(0);
     
    FCascade = (CvHaarClassifierCascade*)cvLoad(&Filename[0], 0, 0, 0 );
    
    if(!FCascade)
    {
     char buffer[100];
     sprintf(buffer, "ERROR: Could not load classifier cascade\n%s", &Filename[0]);
     OutputDebugString(buffer);
    }

    newCascade = false;
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
    
    if (newCascade)
        loadCascade();
    
    FCurrentImage->origin = 1;
    FCurrentImage->imageData = (char*)pFrame;

    if(FCascade)
    {
        int scale = 1;
        CvPoint pt1, pt2;
        int i;       
        
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
                                    //1.1, 2, CV_HAAR_DO_CANNY_PRUNING,
                                    //cvSize(40, 40) );
                                    scale_factor, int(FParams[2].Value), CV_HAAR_DO_CANNY_PRUNING, 
                                    cvSize(int(FParams[4].Value), int(FParams[4].Value)) );
                 
        int facecount = faces->total;
        int spreadsize = facecount * sizeof(float);
        
        FOutputs[0].SliceCount = facecount;
        FOutputs[1].SliceCount = facecount;
        FOutputs[2].SliceCount = facecount;
        FOutputs[3].SliceCount = facecount;
        FOutputs[0].Spread = (float*) realloc(FOutputs[0].Spread, spreadsize);
        FOutputs[1].Spread = (float*) realloc(FOutputs[1].Spread, spreadsize);
        FOutputs[2].Spread = (float*) realloc(FOutputs[2].Spread, spreadsize);
        FOutputs[3].Spread = (float*) realloc(FOutputs[3].Spread, spreadsize);
        
        
       
        
        // draw rect
        for( i = 0; i < (faces ? faces->total : 0); i++ )
        {
            CvRect* r = (CvRect*)cvGetSeqElem( faces, i );            
                        
            FOutputs[0].Spread[i] = ((float) (r->x + (float) r->width / 2) / FImageSize.width) - 0.5;
            FOutputs[1].Spread[i] = 1 - ((float) (r->y + (float) r->height / 2) / FImageSize.height) - 0.5;
            FOutputs[2].Spread[i] = (float) r->width / FImageSize.width;
            FOutputs[3].Spread[i] = (float) r->height / FImageSize.height;
             
            
            
            if (FParams[0].Value > 0)                       
            {
                pt1.x = r->x;
                pt2.x = (r->x+r->width);
                pt1.y = FImageSize.height-r->y;
                pt2.y = FImageSize.height-(r->y+r->height);
                cvRectangle(FCurrentImage, pt1, pt2, CV_RGB(255,0,0), 3, 8, 0);
            }
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
	char name[17] = "FaceDetectViJo";
	
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


