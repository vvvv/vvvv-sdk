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
#include "CamShiftTracker.h"
#include "cvcamshift_mod.h"
#include <string.h>
#include <string>
using namespace std;
#include <stdio.h>


// Plugin Globals
PlugInfoStruct GPlugInfo;
PlugExtendedInfoStruct GPlugExtInfo;
ParamConstsStruct GParamConstants[NUM_PARAMS];
OutputConstsStruct GOutputConstants[NUM_OUTPUTS];

// Marx specials
int first_round=1;
int scaled_before=1;
CvRect selection;
CvRect track_window;
CvBox2D track_box;
CvConnectedComp track_comp;
int hdims = 16;
float hranges_arr[] = {0,180};
float* hranges = hranges_arr;
float angledamp=0 , lastangle=0, angleoffset=0;

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
  // Input pins /////////////////
  
    cvSetErrMode(CV_ErrModeSilent);
    
    // Types & default values for input pins
    
    GParamConstants[0].Type = 0;   	   
    GParamConstants[1].Type = 0;
    GParamConstants[2].Type = 0;
    GParamConstants[3].Type = 10;   	   
    GParamConstants[4].Type = 10;
    GParamConstants[5].Type = 10;   	   
    GParamConstants[6].Type = 10;
    GParamConstants[7].Type = 10;
    GParamConstants[8].Type = 10;   	   
    GParamConstants[9].Type = 10;
    GParamConstants[10].Type = 0;
  	
	  GParamConstants[0].Default = 0.0f;  
    GParamConstants[1].Default = 0.0f;  
    GParamConstants[2].Default = 0.0f;
    GParamConstants[3].Default = 0.0f;   
    GParamConstants[4].Default = 0.0f;
    GParamConstants[5].Default = 0.25f;   
    GParamConstants[6].Default = 0.25f;
    GParamConstants[7].Default = 0.16f;
    GParamConstants[8].Default = 0.04f;   
    GParamConstants[9].Default = 1.0f;
    GParamConstants[10].Default = 1.0f;
	  	
  	// Naming of input pins 
  	
	  char tempName0[17] = "Show ROI"; 
	  char tempName1[17] = "Show Backproject"; 
    char tempName2[17] = "Init Tracker";
    char tempName3[17] = "ROI X"; 
    char tempName4[17] = "ROI Y";
    char tempName5[17] = "ROI Width"; 
    char tempName6[17] = "ROI Height";
    char tempName7[17] = "Saturation Min";
    char tempName8[17] = "Value Min"; 
    char tempName9[17] = "Value Max";
    char tempName10[17] = "Scaled Values";
	
    memcpy(GParamConstants[0].Name, tempName0, 16);	 
    memcpy(GParamConstants[1].Name, tempName1, 16);
    memcpy(GParamConstants[2].Name, tempName2, 16);	 
    memcpy(GParamConstants[3].Name, tempName3, 16);
    memcpy(GParamConstants[4].Name, tempName4, 16);	 
    memcpy(GParamConstants[5].Name, tempName5, 16);
    memcpy(GParamConstants[6].Name, tempName6, 16);
    memcpy(GParamConstants[7].Name, tempName7, 16);	 
    memcpy(GParamConstants[8].Name, tempName8, 16);
    memcpy(GParamConstants[9].Name, tempName9, 16);
    memcpy(GParamConstants[10].Name, tempName10, 16);

  // Output pins ////////////////
  
   // Types for output pins
    
   GOutputConstants[0].Type = 10;
   GOutputConstants[1].Type = 10;
   GOutputConstants[2].Type = 10;
   GOutputConstants[3].Type = 10;
   GOutputConstants[4].Type = 10;
   GOutputConstants[5].Type = 10;
        
   // Naming of output pins 
    
	 char outName0[17] = "X";
	 char outName1[17] = "Y";
	 char outName2[17] = "Width";
	 char outName3[17] = "Height";
	 char outName4[17] = "Angle";
	 char outName5[17] = "IsTracked";
	
   memcpy(GOutputConstants[0].Name, outName0, 16);
	 memcpy(GOutputConstants[1].Name, outName1, 16);
	 memcpy(GOutputConstants[2].Name, outName2, 16);
	 memcpy(GOutputConstants[3].Name, outName3, 16);
	 memcpy(GOutputConstants[4].Name, outName4, 16);
	 memcpy(GOutputConstants[5].Name, outName5, 16);
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
    FOutputs[0].Spread = (float*) calloc(FOutputs[0].SliceCount, sizeof(float));
    FOutputs[1].SliceCount = 1;
    FOutputs[1].Spread = (float*) calloc(FOutputs[1].SliceCount, sizeof(float));
    FOutputs[2].SliceCount = 1;
    FOutputs[2].Spread = (float*) calloc(FOutputs[2].SliceCount, sizeof(float));
    FOutputs[3].SliceCount = 1;
    FOutputs[3].Spread = (float*) calloc(FOutputs[3].SliceCount, sizeof(float));
    FOutputs[4].SliceCount = 1;
    FOutputs[4].Spread = (float*) calloc(FOutputs[4].SliceCount, sizeof(float));
    FOutputs[5].SliceCount = 1;
    FOutputs[5].Spread = (float*) calloc(FOutputs[4].SliceCount, sizeof(float));
    
    InitializeCriticalSection(&CriticalSection);    
}

void plugClass::init()
{
    FImageSize.width = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;
    
    /* Image buffer alloc */
    CCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    Chsv = cvCreateImage( FImageSize, 8, 3 );
    Ghue = cvCreateImage( FImageSize, 8, 1 );
    Gmask = cvCreateImage( FImageSize, 8, 1 );
    Gbackproject = cvCreateImage( FImageSize, 8, 1 );
      
    char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);
    
}

plugClass::~plugClass()
{
    /* Image and output value buffers dealloc */
    cvReleaseImage(&Chsv);
    cvReleaseImage(&Ghue);
    cvReleaseImage(&Gmask);
    cvReleaseImage(&Gbackproject);
    
    free(FOutputs[0].Spread);
    free(FOutputs[1].Spread);
    free(FOutputs[2].Spread);
    free(FOutputs[3].Spread);
    free(FOutputs[4].Spread);
    free(FOutputs[5].Spread);
    
    DeleteCriticalSection(&CriticalSection);
}

char* plugClass::getParameterDisplay(DWORD index)
{
	// fill the array with spaces first
	for (int n=0; n<16; n++) 	FParams[index].DisplayValue[n] = ' ';

	sprintf(FParams[index].DisplayValue, "%f",FParams[index].Value);
	return FParams[index].DisplayValue;
}

DWORD plugClass::setParameter(SetParameterStruct* pParam)
{
	FParams[pParam->index].Value = pParam->value;
	
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

float* plugClass::getOutput(DWORD index)
{ 
    EnterCriticalSection(&CriticalSection); 
    switch(index) 
    {
        case 0: memcpy(FOutputs[0].Spread, &Pos[0], sizeof(float));
        case 1: memcpy(FOutputs[1].Spread, &Pos[1], sizeof(float));
        case 2: memcpy(FOutputs[2].Spread, &Pos[2], sizeof(float));
        case 3: memcpy(FOutputs[3].Spread, &Pos[3], sizeof(float));
        case 4: memcpy(FOutputs[4].Spread, &Pos[4], sizeof(float));
        case 5: memcpy(FOutputs[5].Spread, &Pos[5], sizeof(float));
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

// Seperate Function for Color Tracking ///////////////////////////////////////////////////////////

CvScalar hsv2rgb( float hue )
{
    int rgb[3], p, sector;
    static const int sector_data[][3]=
        {{0,2,1}, {1,2,0}, {1,0,2}, {2,0,1}, {2,1,0}, {0,1,2}};
    hue *= 0.033333333333333333333333333333333f;
    sector = cvFloor(hue);
    p = cvRound(255*(hue - sector));
    p ^= sector & 1 ? 255 : 0;

    rgb[sector_data[sector][0]] = 255;
    rgb[sector_data[sector][1]] = 0;
    rgb[sector_data[sector][2]] = p;

    return cvScalar(rgb[2], rgb[1], rgb[0],0);
}


// Frame processing takes place here: /////////////////////////////////////////////////////////////
   
DWORD plugClass::processFrame24Bit(LPVOID pFrame)
{
    EnterCriticalSection(&CriticalSection);    
  
    /* Putting frame into IplImage format*/
    
    CCurrentImage->origin = 1;
    CCurrentImage->imageData = (char*)pFrame;
    IplImage *Ctmp = cvCloneImage (CCurrentImage); 
         
    int h    = Ctmp->height;
    int w    = Ctmp->width ;     
    
    int smin  = (int) (FParams[7].Value*180.0);//30;    
    int _vmin = (int) (FParams[8].Value*255.0);//10, 
    int _vmax = (int) (FParams[9].Value*255.0);// 256, 
                    
    int S_x = (int)((FParams[3].Value *(float)w)+(float)w/2), S_y = (int)((FParams[4].Value *(float)h)+(float)h/2);
    int S_w = (int)(FParams[5].Value *(float)w), S_h = (int)(FParams[6].Value *(float)h); 
    
 //-> ///////
   
    hist = cvCreateHist( 1, &hdims, CV_HIST_ARRAY, &hranges, 1 );
    
    /* Checking if input rect lies within image & setting the search rect */ 

    if ( S_x-S_w/2<0 || S_x+S_w/2>w || S_y-S_h/2<0 || S_y+S_h/2>h) 
       /* bad rect */
       { selection.x=(w*3)/8;  selection.y=(h*3)/8;  selection.width=w/4;  selection.height=h/4;}
    else    
       /* good rect */
       { selection.x=S_x-S_w/2; selection.y=S_y-S_h/2; selection.width=S_w; selection.height=S_h; }
            

    cvCvtColor( Ctmp, Chsv, CV_BGR2HSV );
    cvSplit( Chsv, Ghue, 0, 0, 0 );
    
    /* Mask pixels that fit in given saturation and grayscale level bounds*/       
    cvInRangeS( Chsv, cvScalar(0,smin,MIN(_vmin,_vmax),0),
                      cvScalar(180,256,MAX(_vmin,_vmax),0), Gmask );
    
    /* Reset Search if requested*/
    if(FParams[2].Value) first_round=1;
    
    if(first_round)
      {
       float max_val = 0.f;
       cvSetImageROI( Ghue, selection );
       cvSetImageROI( Gmask, selection );
       cvCalcHist( &Ghue, hist, 0, Gmask );
       cvGetMinMaxHistValue( hist, 0, &max_val, 0, 0 );
       cvConvertScale( hist->bins, hist->bins, max_val ? 255. / max_val : 0., 0 );
       cvResetImageROI( Ghue );
       cvResetImageROI( Gmask );
       track_window = selection;
      }
       
    cvCalcBackProject( &Ghue, Gbackproject, hist );
    cvAnd( Gbackproject, Gmask, Gbackproject, 0 );
    
    /* Calling CamShift_mod or CamShift Func, depending on option 'Scaled values */
    if (FParams[10].Value)
       {cvCamShift_mod( Gbackproject, track_window, cvTermCriteria( CV_TERMCRIT_EPS | CV_TERMCRIT_ITER, 10, 1 ),
                    &track_comp, &track_box, 
                    w, h , &first_round, &angledamp, &lastangle, &angleoffset, &Pos[5]);
                    scaled_before=1;
                    }
    else 
        cvCamShift( Gbackproject, track_window, cvTermCriteria( CV_TERMCRIT_EPS | CV_TERMCRIT_ITER, 10, 1 ),
                    &track_comp, &track_box);
        
      
    /* Next time we'll start just where we left */              
    track_window = track_comp.rect;

  //-> ///////


    /* Show Histogram backprojection if requested */  
    if( FParams[1].Value )
      {
      cvCvtColor( Gbackproject, Ctmp, CV_GRAY2BGR );
      }
      
    /* Show initial Searchbox if requested */          
    if (FParams[0].Value)
      {
       cvRectangle( Ctmp, cvPoint (selection.x,selection.y), 
                          cvPoint (selection.x+(selection.width),selection.y+(selection.height)),
                          cvScalar(0, 106, 255,0),1, 8, 0 );
      }
      
    if (FParams[0].Value || FParams[1].Value) cvCopy(Ctmp, CCurrentImage, 0);  
    
    /* Copy tracking values to output parameters */
    
    Pos[0]=        track_box.center.x;  
    Pos[1]=        track_box.center.y;  
    Pos[2]=(float) track_box.size.height;
    Pos[3]=(float) track_box.size.width ; 
    
    /* If unscaled, map and damp angle */ 
    if (FParams[10].Value==0)
       {   
        track_box.angle /= 2*CV_PI;   
        if (first_round || scaled_before ) 
           {       
            angleoffset = 0;
            lastangle = track_box.angle;
            first_round=0;
            scaled_before=0;
           }
        else
           {
            if (track_box.angle-lastangle < -0.4 ) angleoffset+= 0.5; 
            else { if (track_box.angle-lastangle > 0.4) angleoffset-= 0.5; } 
           }
        angledamp = track_box.angle + angleoffset;
        lastangle = track_box.angle; // Update History  
       }  
    
    Pos[4]=angledamp; 
    
    cvReleaseImage(&Ctmp);
    cvReleaseHist(&hist);
    
    LeaveCriticalSection(&CriticalSection);

	return FF_SUCCESS;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

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
	char ID[5] = "CMTr";		 // this *must* be unique to your plugin 
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "CamShiftTracker";
	
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

