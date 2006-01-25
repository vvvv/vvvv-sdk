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
#include "ColorTracker.h"
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

// -> tracking constants //
int    hdims=180;
float  hranges_arr[] = {0,180};
float* hranges = hranges_arr;
int params_changed=1; // has tracking color been changed?
int  obj; // loop counter for object loops


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

string filemask = "Bitmap (*.bmp)|*.bmp";
DWORD initialise()
{
    // -> Input pins ///////////////// 
  
    cvSetErrMode(CV_ErrModeSilent);
    
    // -> Types & default values for input pins //
    
    GParamConstants[0].Type  = 0;   	   
    GParamConstants[1].Type  = 0;
    GParamConstants[2].Type  = 0;
    GParamConstants[3].Type  = 2;   	   
    GParamConstants[4].Type  = 3;
    GParamConstants[5].Type  = 4;   	    
    GParamConstants[6].Type  = 10;
    GParamConstants[7].Type  = 10;
    GParamConstants[8].Type  = 10;   	   
    GParamConstants[9].Type  = 10;
    GParamConstants[10].Type = 0;
  	
    GParamConstants[0].Default  = 1.0f;  
    GParamConstants[1].Default  = 1.0f;  
    GParamConstants[2].Default  = 0.0f;
    GParamConstants[3].Default  = 0.4f;   
    GParamConstants[4].Default  = 0.4f;
    GParamConstants[5].Default  = 0.0f; 
    GParamConstants[6].Default  = 0.4f;
    GParamConstants[7].Default  = 0.4f;
    GParamConstants[8].Default  = 0.5f;   
    GParamConstants[9].Default  = 0.0f;
    GParamConstants[10].Default = 1.0f;
	  	
   	// -> Naming of input pins // 
  	
    char tempName0[17]  = "ShowROI"; 
    char tempName1[17]  = "Show ThreshImage"; 
    char tempName2[17]  = "Init Tracker";
    char tempName3[17]  = "Track Color"; 
    char tempName4[17]  = "G";
    char tempName5[17]  = "B"; 
    char tempName6[17]  = "H Tolerance";
    char tempName7[17]  = "S Tolerance";
    char tempName8[17]  = "V Tolerance"; 
    char tempName9[17]  = "Area Threshold";
    char tempName10[17] = "Scaled Values";
	
    memcpy(GParamConstants[0].Name,  tempName0,  16);	 
    memcpy(GParamConstants[1].Name,  tempName1,  16);
    memcpy(GParamConstants[2].Name,  tempName2,  16);	 
    memcpy(GParamConstants[3].Name,  tempName3,  16);
    memcpy(GParamConstants[4].Name,  tempName4,  16);	 
    memcpy(GParamConstants[5].Name,  tempName5,  16);
    memcpy(GParamConstants[6].Name,  tempName6,  16);
    memcpy(GParamConstants[7].Name,  tempName7,  16);	 
    memcpy(GParamConstants[8].Name,  tempName8,  16);
    memcpy(GParamConstants[9].Name,  tempName9,  16);
    memcpy(GParamConstants[10].Name, tempName10, 16);

    // -> Output pins // 
   
    // -> Types for output pins //
     
    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 10;
    GOutputConstants[3].Type = 10;
    GOutputConstants[4].Type = 10;
    GOutputConstants[5].Type = 10;
        
    // -> Naming of output pins //
    
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
    // -> allocating output buffers //
    
    for (register int op=0; op<NUM_OUTPUTS; op++)
        { 
          FOutputs[op].SliceCount = 1;  
          FOutputs[op].Spread = (float*) calloc(1, sizeof(float));
        }
        
    InitializeCriticalSection(&CriticalSection);    
}

plugClass::~plugClass()
{
    // -> deallocating image and output value buffers  //
    cvReleaseImage(&Chsv);
    cvReleaseImage(&Ghue);
    cvReleaseImage(&Gbackproject);
    cvReleaseImage(&Gmask);
    cvReleaseHist(&hist);
    
    for (register int no=0; no<NUM_OUTPUTS; no++) free(FOutputs[no].Spread);
        
    DeleteCriticalSection(&CriticalSection);
}

void plugClass::init()
{
    FImageSize.width  = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;
    
    // -> setting defaults for input values and some tracking parameters //  
    
    for (int in=0; in<NUM_PARAMS; in++) FParams[in].Value=GParamConstants[in].Default;
        
    selectall.x=0;  selectall.y=0;  
    selectall.width=FVideoInfo.frameWidth-1;  selectall.height=FVideoInfo.frameHeight-1;  
    
    first_round=1;
    scaled_before=1; 
    angledamp=0; 
    lastangle=0; 
    angleoffset=0; 
    is_tracked=0;
    click=0;
    
    // -> allocating image buffers  //
    CCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    Chsv          = cvCreateImage( FImageSize, 8, 3 );
    Ghue          = cvCreateImage( FImageSize, 8, 1 );
    Gmasktemp     = cvCreateImage( FImageSize, 8, 1 );
    Gbackproject  = cvCreateImage( FImageSize, 8, 1 );
    Gmask         = cvCreateImage( FImageSize, 8, 1 );
    hist          = cvCreateHist( 1, &hdims, CV_HIST_ARRAY, &hranges, 1 );       
          
    char buffer[100];
    sprintf(buffer, "%i x %i", FImageSize.width, FImageSize.height);
    OutputDebugString(buffer);
    
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

	// -> ColorTracker specific //
    if (pParam->index!=0 && pParam->index!=1 && pParam->index!=13) params_changed=1;
	
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
     
    // -> set output values //  
    FOutputs[0].Spread[0] = track_box.center.x;    
    FOutputs[1].Spread[0] = track_box.center.y;    
    FOutputs[2].Spread[0] = track_box.size.width; 
    FOutputs[3].Spread[0] = track_box.size.height;  
    FOutputs[4].Spread[0] = angledamp-0.25; // -> angle to y-axis // 
    FOutputs[5].Spread[0] = is_tracked; 

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

// Color Conversion Functions /////////////////////////////////////////////////////////////////////

CvScalar plugClass::hsv2rgb( float hue )
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


CvScalar plugClass::rgb2hsv(CvScalar rgb)
{
    float r = rgb.val[0]*255.0, g = rgb.val[1]*255.0, b = rgb.val[2]*255.0 ;
    
    if (r<0) r=0; if (r>255) r=255; 
    if (g<0) g=0; if (g>255) g=255; 
    if (b<0) b=0; if (b>255) b=255;
     
    float h, s, v;

    float vmin, diff;

    v = vmin = r;
    if( v < g ) v = g;
    if( v < b ) v = b;
    if( vmin > g ) vmin = g;
    if( vmin > b ) vmin = b;

    diff = v - vmin;
    s = diff/(float)(fabs(v) + FLT_EPSILON) * 255.0;
    diff = (float)(60./(diff + FLT_EPSILON));
    if( v == r )
        h = (g - b)*diff;
    else if( v == g )
        h = (b - r)*diff + 120.f;
    else
        h = (r - g)*diff + 240.f;

    if( h < 0 ) h += 360.f;
    
    h/=2.0;
 
    return cvScalar(h, s, v, 0);
}


// Frame processing takes place here: /////////////////////////////////////////////////////////////
   
DWORD plugClass::processFrame24Bit(LPVOID pFrame)
{
    EnterCriticalSection(&CriticalSection);    
  
    // -> putting frame into IplImage format //
    
    CCurrentImage->origin = 1;
    CCurrentImage->imageData = (char*)pFrame;
    IplImage *Ctmp = cvCloneImage (CCurrentImage); 
         
    int h    = Ctmp->height;
    int w    = Ctmp->width ;     
    int tol;
    float ratio = (float)w/(float)h;  
    
    // -> convert input image and track color into hsv space //
    cvCvtColor( Ctmp, Chsv, CV_BGR2HSV );
    cvSplit( Chsv, Ghue, 0, 0, 0 );
    
    hsv =  rgb2hsv( cvScalar(FParams[3].Value, FParams[4].Value, FParams[5].Value, 0) );
        
    // -> for ROI display color
    CvScalar rgb = hsv2rgb(hsv.val[0]); 
       
    // -> reset search if requested //
    if(FParams[2].Value) first_round=1;    
    
    ///////////////////////////////////////////////////////////////////////////////////// 
    // STEP I : If params changed or reset pin is banged, refresh tracking parameters  //
    
    if(first_round || params_changed)
       {        
        // -> at initialisation, search everywhere //  
        track_window = selectall;     
        
        // -> calculate hue, saturation and value bounds according to tolerances //  
        // -> hue bounds  
        tol = (FParams[6].Value>0)? (int) ( FParams[6].Value*90.0 ) : (int) ( -FParams[6].Value*90.0 ); 
        
        hmin = (int) ( (hsv.val[0]) - tol ); 
        hmax = (int) ( (hsv.val[0]) + tol );
       
        if (tol>89)
           {
            hmin = 0 ;
            hmax = 181 ;
          }
        else
           {
            if (hmin < 0)  hmin += 180 ;   
            if (hmax >180) hmax -= 180 ; 
           }
         
        // -> saturation bounds          
        tol = (FParams[7].Value>0)? (int) ( FParams[7].Value*255.0 ) : (int) ( -FParams[7].Value*255.0 ); 

        smin =(int) hsv.val[1]-tol;
        if (smin<0) smin = 0;  if (smin>255) smin = 256;
        
        smax =(int) hsv.val[1]+tol;
        if (smax<0) smax = 0;  if (smax>255) smax = 256;
        
        // -> value bounds
        tol = (FParams[8].Value>0)? (int) ( FParams[8].Value*255.0 ) : (int) ( -FParams[8].Value*255.0 ); 
          
        vmin =(int) hsv.val[2]-tol;
        if (vmin<0) vmin = 0;  if (vmin>255) vmin = 256;
        
        vmax =(int) hsv.val[2]+tol;
        if (vmax<0) vmax = 0;  if (vmax>255) vmax = 256;
        
        params_changed=0;
       }

    cvSetZero(Gbackproject); 
  
    ///////////////////////////////////////////////////////////////////////////////////   
    // STEP II : Mask pixels that fit in given saturation and grayscale level bounds //     
    
    if (hmin < hmax || hmin == hmax)
       { int dings=0;
         cvInRangeS( Chsv, cvScalar(hmin, smin, vmin, 0),
                           cvScalar(hmax, smax, vmax, 0), Gmask );             
       }
    // -> wrapping color if necessary (because of color circle) //
    else
       {
         cvInRangeS( Chsv, cvScalar(0,    smin, vmin, 0),
                           cvScalar(hmax, smax, vmax, 0), Gmask );
        
         cvInRangeS( Chsv, cvScalar(hmin, smin, vmin, 0),
                           cvScalar(181,  smax, vmax, 0), Gmasktemp );
        
         cvAdd( Gmasktemp, Gmask, Gmask, NULL);                    
       }
       
    ///////////////////////////////////////////////////////////////////////////////////////////////////// 
    // STEP III : Call cvCamShift_mod (with image thresholded with tolerances) to track color objects  //   
    // -> calling CamShift_mod with option 'scaled values' (Farams[10].Value)        //
    
    cvCamShift_mod( Gmask, track_window, cvTermCriteria( CV_TERMCRIT_EPS | CV_TERMCRIT_ITER, 10, 1 ),
                    &track_comp, &track_box, w, h , &first_round, &angledamp, &lastangle, &angleoffset, 
                    &area, FParams[10].Value);
                    
    scaled_before=1;
    
    // -> TT is trackingthresh (Area Threshold) to the power of 4                              //
    //    This is to fit the range of 0 (no thresholding) to 1 (object has size of full image) //
    //    with a wide numerical range for small objects                                        //
    float TT=FParams[9].Value*FParams[9].Value;
    TT *= TT;        
    if ( area/255.0 > TT ) is_tracked=1.0;
    else                   is_tracked=0.0;  

    // -> next time we'll start just where we left //              
    track_window = track_comp.rect;
    
    // -> reinit search ROI if requested  //
    if (is_tracked==0) track_window = selectall;    
    
    // -> show thresholded image if requested  //  
    if( (int)FParams[1].Value ) 
      {
       cvCopy(Gmask, Gbackproject,0);
       cvCvtColor( Gbackproject, Ctmp, CV_GRAY2BGR );
      }
     
    // -> show searchboxes if requested //          
    if (FParams[0].Value)
      {
       cvRectangle( Ctmp, cvPoint (track_window.x,track_window.y), 
                          cvPoint (track_window.x+(track_window.width),track_window.y+(track_window.height)),
                          cvScalar (rgb.val[0], rgb.val[1], rgb.val[2]), 1, 8, 0 );
       }
    if (FParams[0].Value || FParams[1].Value) cvCopy(Ctmp, CCurrentImage, 0);  
       
    cvReleaseImage(&Ctmp);
    
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
	GPlugInfo.APIMinorVersion = 200;		// this is the number after the decimal point
										// so version 0.511 has major num 0, minor num 501
	char ID[5] = "CLTr";		 // this *must* be unique to your plugin 
								 // see www.freeframe.org for a list of ID's already taken
	char name[17] = "ColorTracker";
	
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
