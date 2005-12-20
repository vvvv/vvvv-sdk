//////project name
//Contour

//////description
//freeframe plugin.
//outputs points(x/y) along contours found in the thresholded input.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//dev-c++ 5

//////dependencies
//opencv beta5 libraries:
//http://sourceforge.net/projects/opencvlibrary

//////initial author
//joreg -> joreg@gmx.at

//////edited by
//Marc Sandner -> ms@saphmar.net

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
	GParamConstants[3].Type = 0;
	GParamConstants[4].Type = 0;
	GParamConstants[5].Type = 10;
    
	GParamConstants[0].Default = 0.5f;
	GParamConstants[1].Default = 0.0f;
	GParamConstants[2].Default = 0.0f;
	GParamConstants[4].Default = 0.01f;
  GParamConstants[4].Default = 1.00f;

	char tempName0[17] = "Threshold";
	char tempName1[17] = "Levels";
	char tempName2[17] = "Show Contours";
	char tempName3[17] = "Cleanse";
	char tempName4[17] = "Scaled Values";
	char tempName5[17] = "Thickness";
	
	memcpy(GParamConstants[0].Name, tempName0, 16);
	memcpy(GParamConstants[1].Name, tempName1, 16);
	memcpy(GParamConstants[2].Name, tempName2, 16);
	memcpy(GParamConstants[3].Name, tempName3, 16);
	memcpy(GParamConstants[4].Name, tempName4, 16);
	memcpy(GParamConstants[5].Name, tempName5, 16);

    // populate the output structs
    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;
    GOutputConstants[4].Type = 10;
    GOutputConstants[5].Type = 10;
    GOutputConstants[6].Type = 10;
    GOutputConstants[7].Type = 10;
    GOutputConstants[8].Type = 10;
    
    
	char outName0[17] = "PixelsX";
	char outName1[17] = "PixelsY";
	char outName2[17] = "Contours BinSize";
	char outName3[17] = "XPos";
	char outName4[17] = "YPos";
	char outName5[17] = "Width";
	char outName6[17] = "Height";
	char outName7[17] = "Orientation";
	char outName8[17] = "Area";
	

	memcpy(GOutputConstants[0].Name, outName0, 16);
	memcpy(GOutputConstants[1].Name, outName1, 16);
	memcpy(GOutputConstants[2].Name, outName2, 16);
	memcpy(GOutputConstants[3].Name, outName3, 16);
	memcpy(GOutputConstants[4].Name, outName4, 16);
	memcpy(GOutputConstants[5].Name, outName5, 16);
	memcpy(GOutputConstants[6].Name, outName6, 16);
	memcpy(GOutputConstants[7].Name, outName7, 16);
	memcpy(GOutputConstants[8].Name, outName8, 16);
	
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
    FOutputs[4].SliceCount = 0;
    FOutputs[5].SliceCount = 0;
    FOutputs[6].SliceCount = 0;
    FOutputs[7].SliceCount = 0;
    FOutputs[8].SliceCount = 0;
    
    //initialize spreads
    FOutputs[0].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[1].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[2].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[3].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[4].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[5].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[6].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[7].Spread = (float*) calloc(1, sizeof(float));
    FOutputs[8].Spread = (float*) calloc(1, sizeof(float));
    
    
    angledamp   = (float*) calloc(1, sizeof(float));
    lastangle   = (float*) calloc(1, sizeof(float));
    angleoffset = (float*) calloc(1, sizeof(float));
    
    InitializeCriticalSection(&CriticalSection); 
}

void plugClass::init()
{
    FImageSize.width = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    FCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    FGrayImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    tmp = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
}

plugClass::~plugClass()
{
    cvReleaseImage(&FGrayImage);
    cvReleaseImage(&tmp);
    cvReleaseMemStorage(&FStorage);
    
    free(FOutputs[0].Spread);
    free(FOutputs[1].Spread);
    free(FOutputs[2].Spread);
    free(FOutputs[3].Spread);
    free(FOutputs[4].Spread);
    free(FOutputs[5].Spread);
    free(FOutputs[6].Spread); 
    free(FOutputs[7].Spread);
    free(FOutputs[8].Spread);
    
    free(angledamp);
    free(lastangle);   
    free(angleoffset); 
        
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
        
        FOutputs[4].Spread = (float*) realloc(FOutputs[4].Spread, sizeof(float) * FContoursCount);
        FOutputs[5].Spread = (float*) realloc(FOutputs[5].Spread, sizeof(float) * FContoursCount);
        FOutputs[6].Spread = (float*) realloc(FOutputs[6].Spread, sizeof(float) * FContoursCount);
        FOutputs[7].Spread = (float*) realloc(FOutputs[7].Spread, sizeof(float) * FContoursCount);
        FOutputs[8].Spread = (float*) realloc(FOutputs[8].Spread, sizeof(float) * FContoursCount);
        
        angledamp   = (float*) realloc(angledamp, sizeof(float) * FContoursCount);
        lastangle   = (float*) realloc(lastangle, sizeof(float) * FContoursCount);
        angleoffset = (float*) realloc(angleoffset, sizeof(float) * FContoursCount);
        
        memcpy(FOutputs[2].Spread, FBinSizes, FContoursCount * sizeof(float)); 
               
        int c;
        int p = 0;
        float theta, cs, sn, rotate_a, rotate_c, width, height, ratio;
        CvPoint* PointArray;
        float ImageSize=(FImageSize.width*FImageSize.height);
        
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
              FOutputs[0].Spread[p] = ((float)PointArray[j].x / FImageSize.width) * 1 - 0.5;
              FOutputs[1].Spread[p] = ((float)PointArray[j].y / FImageSize.height) * 1 - 0.5;
              p++;
           } 
                      
           //calculate orientation of contour
           //cvMoments(FFirstContour, &FMoments);
           cvMoments(FFirstContour, &FMoments);
             
           //output area of object
           float nPixels         = cvGetCentralMoment( &FMoments, 0, 0 );  
           FOutputs[3].Spread[i] = (FMoments.m00!=0)? ( (((float)FMoments.m10/(float)FMoments.m00) /FImageSize.width )*1 - 0.5 ) : -200 ;//cvGetNormalizedCentralMoment( &FMoments,0,0);//FMoments.mu20 /nPixels;//(float)(cvGetCentralMoment( &FMoments, 0, 0 ));
           FOutputs[4].Spread[i] = (FMoments.m00!=0)? ( (((float)FMoments.m01/(float)FMoments.m00) /FImageSize.height)*1 - 0.5 ) : -200;//cvGetNormalizedCentralMoment( &FMoments,1,1);//FMoments.mu02 /nPixels;//(float)(cvGetCentralMoment( &FMoments, 0, 1 ));
              
           if (FParams[4].Value) 
              {
               ratio = (float) FImageSize.width  / (float) FImageSize.height ;
               theta = 0.5 * atan2(2 * FMoments.mu11 * ratio, FMoments.mu20 - FMoments.mu02);  
              }
           else
              {
               theta = 0.5 * atan2(2 * FMoments.mu11, FMoments.mu20 - FMoments.mu02);
              }
           
            cs = cos( theta  ); 
            sn = sin( theta );              
            rotate_a = cs * cs * FMoments.mu20 + 2 * cs * sn * FMoments.mu11 + sn * sn * FMoments.mu02;
            rotate_c = sn * sn * FMoments.mu20 - 2 * cs * sn * FMoments.mu11 + cs * cs * FMoments.mu02;
            width = sqrt( rotate_a * (1.0/(float)nPixels) ) * 4 ;
            height = sqrt( rotate_c * (1.0/(float)nPixels) ) * 4;
           
           if (FParams[4].Value) 
              {
               FOutputs[5].Spread[i] = (width - (width*cos(theta)*0.25 ) )    / (float) FImageSize.width ;
               FOutputs[6].Spread[i] = (height - (height*cos(theta)*0.25 ) )  / (float) FImageSize.height ;                  
              }
           else
              {
               FOutputs[5].Spread[i] =  width                                 / (float) FImageSize.width ;  
               FOutputs[6].Spread[i] =  height                                / (float) FImageSize.height ;                   
              }
            
           FOutputs[7].Spread[i] = theta/2/CV_PI;
           FOutputs[8].Spread[i] = (float) nPixels / ImageSize ;
           
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
    int size = FCurrentImage->height * FCurrentImage->width;
    int h = FCurrentImage->height;
    int w = FCurrentImage->width;
    
    cvCvtColor(FCurrentImage, FGrayImage, CV_BGR2GRAY);
    
    //threshold the image
    cvThreshold(FGrayImage, FGrayImage, FParams[0].Value * 255, 255, CV_THRESH_BINARY);
    //cvCopy(FGrayImage, tmp, NULL);
    
    if (FParams[3].Value > 0) 
    {
    //cvSmooth( tmp, FGrayImage, CV_MEDIAN, 3, 3, 0 ); 
    
    // -> opening image //
    
    // -> erode //
    
    register int offset0 = 0, offset =w, offset1=w-1, offset2=2*w;
    
    for ( register int py=0; py<h-2; py++)
        {
         for ( register int px=0; px<w-1; px++)
             {
              tmp->imageData[px+offset]=  (FGrayImage->imageData[px+offset0] && FGrayImage->imageData[px+offset1] && FGrayImage->imageData[px+offset] && FGrayImage->imageData[px+offset1+2] && FGrayImage->imageData[px+offset2])? FGrayImage->imageData[px+offset] : 0;
              }
         offset0+=w;
         offset+=w;
         offset1+=w;
         offset2+=w;
        }
   
    // -> dilate //
      
    offset0 = 0; offset =w; offset1=w-1; offset2=2*w;
    
    for ( register int py=0; py<h-2; py++)
        {
         for ( register int px=0; px<w-1; px++)
             {
              FGrayImage->imageData[px+offset]=  (tmp->imageData[px+offset0] || tmp->imageData[px+offset1] || tmp->imageData[px+offset] || tmp->imageData[px+offset1+2] || tmp->imageData[px+offset2])? tmp->imageData[px+offset] : 0;
              }
         offset0+=w;
         offset+=w;
         offset1+=w;
         offset2+=w;
        }
    }        
               
    cvCvtColor(FGrayImage, FCurrentImage, CV_GRAY2BGR);
    //clear memory from last round
    cvClearMemStorage(FStorage);

    //find the contours
    FContoursCount = cvFindContours(FGrayImage, FStorage, &FContours, sizeof(CvContour), CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE);
    
    if (FContoursCount>200) FContoursCount=200;
    FPointCount = 0;
    free(FBinSizes);
    FBinSizes = (float*) calloc(FContoursCount, sizeof(float));

    //save pointer to firstcontour for use in getOutput
    FFirstContour = FContours;
       
    //go through all contours sum up the number of all points and output binsizes
    for(int i=0; i<FContoursCount; i++)
    {
       if (FParams[2].Value > 0)
         cvDrawContours(FCurrentImage, FContours, CV_RGB(255,0,0), CV_RGB(0,255,0), (int)(FParams[1].Value * 255), (int)(FParams[5].Value * 255), CV_AA);
         
       FPointCount += FContours->total;
       FBinSizes[i] = (float)FContours->total;
       FContours = FContours->h_next;
    }    
     
    //set output slicecounts
    FOutputs[0].SliceCount = FPointCount;
    FOutputs[1].SliceCount = FPointCount;
    FOutputs[2].SliceCount = FContoursCount;
    FOutputs[3].SliceCount = FContoursCount;
    FOutputs[4].SliceCount = FContoursCount;
    FOutputs[5].SliceCount = FContoursCount;
    FOutputs[6].SliceCount = FContoursCount;
    FOutputs[7].SliceCount = FContoursCount;
    FOutputs[8].SliceCount = FContoursCount;
    
  
    //retrieving of points is done in getOutput    

    FContoursChanged = true;
    //cvCvtColor(FGrayImage, FCurrentImage, CV_GRAY2BGR);
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
