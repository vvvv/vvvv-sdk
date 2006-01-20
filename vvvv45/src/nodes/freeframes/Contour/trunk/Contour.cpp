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
   	GParamConstants[1].Type = 0;
   	GParamConstants[2].Type = 0;
   	GParamConstants[3].Type = 0;
   	GParamConstants[4].Type = 10;
   	GParamConstants[5].Type = 0;
   	GParamConstants[6].Type = 0;
       
   	GParamConstants[0].Default = 0.5f;
   	GParamConstants[1].Default = 1.0f;
   	GParamConstants[2].Default = 1.0f;
   	GParamConstants[3].Default = 1.0f;
    GParamConstants[4].Default = 0.01f;
    GParamConstants[5].Default = 1.0f;
    GParamConstants[6].Default = 0.0f;
   
   	char tempName0[17] = "Threshold";
   	char tempName1[17] = "Invert";
   	char tempName2[17] = "Cleanse";
   	char tempName3[17] = "Show Contours";
   	char tempName4[17] = "Thickness";
   	char tempName5[17] = "Scaled Values";
   	char tempName6[17] = "Unique ID";
   	
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
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;
    GOutputConstants[4].Type = 10;
    GOutputConstants[5].Type = 10;
    GOutputConstants[6].Type = 10;
    GOutputConstants[7].Type = 10;
    GOutputConstants[8].Type = 10;
    GOutputConstants[9].Type = 10;
       
   	char outName0[17] = "Contours X";
   	char outName1[17] = "Contours Y";
   	char outName2[17] = "Contours BinSize";
   	char outName3[17] = "X";
   	char outName4[17] = "Y";
   	char outName5[17] = "Width";
   	char outName6[17] = "Height";
   	char outName7[17] = "Orientation";
   	char outName8[17] = "Area";
   	char outName9[17] = "ID";
   	
   	memcpy(GOutputConstants[0].Name, outName0, 16);
   	memcpy(GOutputConstants[1].Name, outName1, 16);
   	memcpy(GOutputConstants[2].Name, outName2, 16);
   	memcpy(GOutputConstants[3].Name, outName3, 16);
   	memcpy(GOutputConstants[4].Name, outName4, 16);
   	memcpy(GOutputConstants[5].Name, outName5, 16);
   	memcpy(GOutputConstants[6].Name, outName6, 16);
   	memcpy(GOutputConstants[7].Name, outName7, 16);
   	memcpy(GOutputConstants[8].Name, outName8, 16);
   	memcpy(GOutputConstants[9].Name, outName9, 16);
   	
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
    
    for (int i=0; i<NUM_OUTPUTS; i++)
        {
         FOutputs[i].SliceCount = 0;
         //initialize spreads
         FOutputs[i].Spread = (float*) calloc(1, 0);
        }
         
    Objlist_old = (Obj*) malloc(1 * sizeof(Obj));
    Objlist_new = (Obj*) malloc(1 * sizeof(Obj));
    
    angledamp   = (float*) calloc(1, sizeof(float));
    lastangle   = (float*) calloc(1, sizeof(float));
    angleoffset = (float*) calloc(1, sizeof(float));
    
    Sortlist         = (int*)   calloc(1, sizeof(int));
    lastangle_temp   = (float*) calloc(1, sizeof(float));
    angleoffset_temp = (float*) calloc(1, sizeof(float));
    
    InitializeCriticalSection(&CriticalSection); 
}

plugClass::~plugClass()
{
    cvReleaseImage(&FGrayImage);
    cvReleaseImage(&tmp);
    cvReleaseMemStorage(&FStorage);
    
    // -> deallocate output spreads //
    for (int i=0; i<NUM_OUTPUTS; i++) free(FOutputs[i].Spread);
    
    free(Objlist_old);    
    free(Objlist_new);
    
    free(angledamp); 
    free(lastangle);    
    free(angleoffset); 
    
    free(lastangle_temp);
    free(angleoffset_temp); 
    
    free (Sortlist);  
    free (IDs_old);    
    free (IDs_new);  
    free (FMoments);      
        
    DeleteCriticalSection(&CriticalSection);
}

void plugClass::init()
{
    FImageSize.width  = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    FCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    FGrayImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    tmp = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
    
    IDs_old       = (int*) malloc(sizeof(int));
    IDs_new       = (int*) malloc(sizeof(int));
    
    B_firstRound  = true;  
    FBinSizes     = NULL;
    FMoments      = NULL;
    
    inc  =  0;
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
    //if (pParam->index=5) B_firstRound=1;
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
  
////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////  
   
DWORD plugClass::processFrame24Bit(LPVOID pFrame)
{
    EnterCriticalSection(&CriticalSection);
    
    FCurrentImage->origin = 1;
    FCurrentImage->imageData = (char*)pFrame;
    int size = FCurrentImage->height * FCurrentImage->width;
    float h = FCurrentImage->height;
    float w = FCurrentImage->width; 
    
    //////////////////////////////////////////////////////////////////////
    // PART I : image preprocessing and call of 'FindContours' function // 
    
    cvCvtColor(FCurrentImage, FGrayImage, CV_BGR2GRAY);
    // -> threshold the image //
    cvThreshold(FGrayImage, FGrayImage, FParams[0].Value * 255, 255, CV_THRESH_BINARY); 
    // -> invert image if requested //
    if (FParams[1].Value>0)
       {for (int i=0; i< (FGrayImage->height*FGrayImage->width); i++) FGrayImage->imageData[i] = (FGrayImage->imageData[i])? 0 : 255;}
    // -> perform Median filtering (cleansing) of input image if requested //
    if (FParams[2].Value > 0) 
       {
        cvSmooth( FGrayImage, tmp, CV_MEDIAN, 9,  9, 0 );
        cvCopy (tmp, FGrayImage, NULL);
       }        
    cvCvtColor(FGrayImage, FCurrentImage, CV_GRAY2BGR);
    // -> clear memory from last round //
    cvClearMemStorage(FStorage);
  
    // -> find the contours //
    FContoursCount_old  = FContoursCount;
    FContoursCount_temp = cvFindContours(FGrayImage, FStorage, &FContours, sizeof(CvContour), CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE);
    //FContoursCount_temp = cvFindContours(FGrayImage, FStorage, &FContours, sizeof(CvContour), CV_RETR_LIST, CV_CHAIN_CODE);
    
    if (FContoursCount_temp>100) FContoursCount_temp=100;
    FPointCount = 0;
    if (FBinSizes) free(FBinSizes);
    if (FMoments)  free(FMoments);
    // -> allocate FBinSizes and FMoments arrays //
    FBinSizes = (float*)     calloc(FContoursCount_temp, sizeof(float)    );
    FMoments  = (CvMoments*) calloc(FContoursCount_temp, sizeof(CvMoments));
    // -> save pointer to firstcontour // 
    FContours_temp = FContours;    
    
    ///////////////////////////////////////////////////////////////////
    // PART II : calculation of contour moments & contour validation // 
       
    FPointCount    = 0;
    FContoursCount = 0;
    
    for(int i=0; i<FContoursCount_temp; i++)
    {       
       // -> calculate orientation of contour //
       if (FParams[5].Value) cvMoments_mod(FContours, &FMoments[i], (float)w, (float)h );
       else                       cvMoments(FContours, &FMoments[i] );
        if (FMoments[i].m00!=0.0)
           {
            FContoursCount++ ; // -> Contour is valid & counted
            //FOutputs[8].Spread[i]  = cvContourArea(FContours, CV_WHOLE_SEQ ) /size;//FMoments[i].m00  ;
            //FOutputs[8].Spread[i] = fabs(area);
            FPointCount += FContours->total;
           }
        FContours = FContours->h_next;
    }
    FContours = FContours_temp;
    if (B_firstRound) FContoursCount_old =  FContoursCount;
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    // PART III : set slicecounts and reallocate fresh memory for output and angle-calculation variables  //
    
    for (int i=0; i<2; i++)
        {
         FOutputs[i].SliceCount = FPointCount;
         FOutputs[i].Spread     = (float*) realloc(FOutputs[i].Spread, sizeof(float) * FPointCount);
        }
    for (int i=2; i<10; i++) 
        {
         FOutputs[i].SliceCount = FContoursCount;
         FOutputs[i].Spread     = (float*) realloc(FOutputs[i].Spread, sizeof(float) * FContoursCount);
        }
    Sortlist         = (int*)   realloc(Sortlist,         sizeof(int)   * FContoursCount);
    angledamp        = (float*) realloc(angledamp,        sizeof(float) * FContoursCount);
    lastangle_temp   = (float*) realloc(lastangle_temp,   sizeof(float) * FContoursCount);
    angleoffset_temp = (float*) realloc(angleoffset_temp, sizeof(float) * FContoursCount);    
    
    ////////////////////////////////////////////////////////////////////
    // PART IV : reallocate fresh memory for IDs and prepare ID lists //
    
    IDs_old     = (int*) realloc(IDs_old, sizeof(int) * FContoursCount_old);  
    Objlist_old = (Obj*) realloc(Objlist_old, sizeof(Obj) * FContoursCount_old); 
    // -> put old Object positions and IDs into object list //
    if (B_firstRound)
       {
        IDs_new     = (int*) realloc(IDs_new, sizeof(int) * FContoursCount); 
        for (int i=0; i<FContoursCount; i++) IDs_new[i] = i;
        Objlist_new = (Obj*) realloc(Objlist_new, sizeof(Obj) * FContoursCount);    
       }     
    for (int i=0; i<FContoursCount_old; i++)
        {
         Objlist_old[i].x = Objlist_new[i].x;  Objlist_old[i].y = Objlist_new[i].y;  
         IDs_old[i] = IDs_new[i];
        }
    Objlist_new = (Obj*) realloc(Objlist_new, sizeof(Obj) * FContoursCount);
    IDs_new     = (int*) realloc(IDs_new, sizeof(int) * FContoursCount);   
     
    // -> define and init variables needed to calculate output values //
    int binsize; 
    int j=0, p=0;
    float theta, theta_mod, inv_m00, cs, sn, rotate_a, rotate_c, width, height;
    float a, b, c, xc, yc, square;
    CvPoint* PointArray;
    float ImageSize = w*h; 
    float ratio     = w/h;
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // PART IV : go through all contours, check them (area!=0), calculate and set output values if valid //

    // -> loop over all found contours (variable i) and valid contours (j) //
    for(int i=0; i<FContoursCount_temp; i++)
    {      
        if (FMoments[i].m00!=0.0)
           {        
            // -> draw contour if requested //
            if (FParams[3].Value > 0)
               cvDrawContours(FCurrentImage, FContours, CV_RGB(255,0,0), CV_RGB(0,255,0), /*(int)(FParams[1].Value * 255)*/ 0, (int)(FParams[4].Value * 255), CV_AA);
            
            binsize = FContours->total; 
            // -> allocate memory for temp point array & copy points to the array //
            PointArray = (CvPoint*)malloc(binsize * sizeof(CvPoint));               
            cvCvtSeqToArray(FContours, PointArray, CV_WHOLE_SEQ);
            inv_m00 = 1.0 / FMoments[i].m00;
           
            // -> set contour pixels and bin sizes //
            for(int l=0; l<binsize; l++)
               {
                FOutputs[0].Spread[p] = ((float)PointArray[l].x / w) -0.5;
                FOutputs[1].Spread[p] = ((float)PointArray[l].y / h) -0.5;
                p++;
               }                
            FOutputs[2].Spread[j] =  binsize;
         
            // -> set position (x, y coordinates) i.e. center of mass //
            FOutputs[3].Spread[j] =  (FParams[5].Value)? (FMoments[i].m10*inv_m00) : (((FMoments[i].m10*inv_m00) / w)-0.5 )*ratio ;
            FOutputs[4].Spread[j] =  (FParams[5].Value)? (FMoments[i].m01*inv_m00) : ( (FMoments[i].m01*inv_m00) / h)-0.5 ;
            Objlist_new[j].x      =  FOutputs[3].Spread[j];
            Objlist_new[j].y      =  FOutputs[4].Spread[j];  
            
            // -> calculate and set height, width and rotation angle of object //
            a = FMoments[i].mu20 * inv_m00;
            b = FMoments[i].mu11 * inv_m00;
            c = FMoments[i].mu02 * inv_m00; 
            square = sqrt( 4 * b * b + (a - c) * (a - c) );
            theta    = atan2( 2 * b , a - c + square );
            cs       = cos( theta ); sn = sin( theta );              
            rotate_a = cs * cs * FMoments[i].mu20 + 2.0 * cs * sn * FMoments[i].mu11 + sn * sn * FMoments[i].mu02;
            rotate_c = sn * sn * FMoments[i].mu20 - 2.0 * cs * sn * FMoments[i].mu11 + cs * cs * FMoments[i].mu02;
            
            // TODO : why is there a linear abberation in object width and height? 
            //       (as a temp fix it is compensated by multiplicators 0.8663 and 0.882 calculated by linear regression)
            width    = sqrt( rotate_a * inv_m00 ) * 4.0 * 0.8663;  
            height   = sqrt( rotate_c * inv_m00 ) * 4.0 * 0.882;
            if( width < height )
            {
                double t;  
                CV_SWAP(height , width, t );
                CV_SWAP( cs, sn, t );
                theta = CV_PI*0.5 - theta;
            }
            FOutputs[5].Spread[j] = (FParams[5].Value)? width           : (width /w)*ratio;  
            FOutputs[6].Spread[j] = (FParams[5].Value)? height          : (height/h);  
            FOutputs[8].Spread[j] = (FParams[5].Value)? FMoments[i].m00 : FMoments[i].m00/(w*h) ;
            
            // -> save angle to temp array for further processing (PART V) // 
            lastangle_temp[j] = theta / (2*CV_PI);   
            
            free(PointArray);    
            j++;
           }
       FContours = FContours->h_next; 
    }      
      
    /////////////////////////////////////////////////////////////////////////////////
    // PART V : sorting the object list, calculating damped angle for each object  //
      
    if (!B_firstRound) 
       {
         adaptindex(Objlist_old, Objlist_new, FContoursCount_old, FContoursCount, IDs_old, IDs_new, Sortlist, &inc, (int)FParams[6].Value);  
       }
    // -> angle damping //
    if (!B_firstRound)
       { 
        for (j=0; j<FContoursCount; j++ )
            {
             if (Sortlist[j]!=-1)
                {
                 angleoffset_temp[j]= angleoffset[Sortlist[j]];     
                         
                 if (lastangle_temp[j]-lastangle[Sortlist[j]] < -0.4 ) angleoffset_temp[j]+= 0.5; 
                 else{ 
                      if (lastangle_temp[j]-lastangle[Sortlist[j]] >  0.4 ) angleoffset_temp[j]-= 0.5; 
                     }
                }
             else 
                 angleoffset_temp[j]=0;
                
             angledamp[j] = lastangle_temp[j] + angleoffset_temp[j];            
             FOutputs[7].Spread[j] = angledamp[j];  
            }
       }
    else
       {
        for (j=0; j<FContoursCount; j++ )
            {
             angleoffset_temp[j]   = 0;
             angledamp[j]          = lastangle_temp[j];
             FOutputs[7].Spread[j] = angledamp[j]; 
            }
       }     
    lastangle    = (float*) realloc(lastangle,   sizeof(float) * FContoursCount);
    angleoffset  = (float*) realloc(angleoffset, sizeof(float) * FContoursCount);    
    for (j=0; j<FContoursCount; j++ )
        { 
         // -> updating ID list and angle history//
         FOutputs[9].Spread[j] = IDs_new[j];  
         lastangle[j]          = lastangle_temp[j];
         angleoffset[j]        = angleoffset_temp[j];
        }
     
    B_firstRound=false;  
    
    LeaveCriticalSection(&CriticalSection);
        
	return FF_SUCCESS;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////


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
	char ID[5] = "MOCT";		 // this *must* be unique to your plugin 
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
