//////project name
//ColorTracker

//////description
//freeframe plugin.
//returns location(x/y) width/height and rotation angle
//of tracked object from image thresholded with
//user-defined color parameters

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
//Marc Sandner -> ms@saphmar.net

//////edited by
//your name here



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

    GParamConstants[0].Type   = 10;
    GParamConstants[1].Type   = 10;
    GParamConstants[2].Type   = 20;
    GParamConstants[3].Type   = 21;
    GParamConstants[4].Type   = 21;
    GParamConstants[5].Type   = 20;
    GParamConstants[6].Type   = 20;
    GParamConstants[7].Type   = 10;
    GParamConstants[8].Type   = 10;
    GParamConstants[9].Type   = 10;
    GParamConstants[10].Type  = 10;
    GParamConstants[11].Type  = 10;

    GParamConstants[0].Default  = 1.0f;
    GParamConstants[1].Default  = 1.0f;
    GParamConstants[2].Default  = 0.0f;
    GParamConstants[3].Default  = 0.4f;
    GParamConstants[4].Default  = 0.4f;
    GParamConstants[5].Default  = 0.0f;
    GParamConstants[6].Default  = 0.0f;
    GParamConstants[7].Default  = 0.0f;
    GParamConstants[8].Default  = 0.0f;
    GParamConstants[9].Default  = 1.0f;
    GParamConstants[10].Default = 1.0f;
    GParamConstants[11].Default = 1.0f;

   	// -> Naming of input pins //

    char tempName0[17]  = "Show SearchPos";
    char tempName1[17]  = "Show ThreshImage";
    char tempName2[17]  = "Init Tracker";
    char tempName3[17]  = "Track Color";
    char tempName4[17]  = "TC Tolerances";
    char tempName5[17]  = "Area Threshold";
    char tempName6[17]  = "Noise Reduction";
    char tempName7[17]  = "ROI X";
    char tempName8[17]  = "ROI Y";
    char tempName9[17]  = "ROI Width";
    char tempName10[17] = "ROI Height";
    char tempName11[17] = "Scaled Values";

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
    memcpy(GParamConstants[11].Name, tempName11, 16);

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
    // -> intial allocation of output buffers //
    for (register int op=0; op<NUM_OUTPUTS; op++)
        {FOutputs[op].SliceCount = 1;
         FOutputs[op].Spread = (float*) calloc(1, sizeof(float));
        }

    // -> initial allocation of spreadsize-dependent buffers //
    reinit       = (float*) calloc(1, sizeof(float));
    filtersize   = (float*) calloc(1, sizeof(float));
    areathresh   = (float*) calloc(1, sizeof(float));

    track_window = (CvRect*) calloc(1, sizeof(CvRect));
    track_box    = (CvBox2D*) calloc(1, sizeof(CvBox2D));
    track_comp   = (CvConnectedComp*) calloc(1, sizeof(CvConnectedComp));

    angledamp   = (float*) calloc(1, sizeof(float));
    lastangle   = (float*) calloc(1, sizeof(float));
    angleoffset = (float*) calloc(1, sizeof(float));
    area        = (float*) calloc(1, sizeof(float));
    is_tracked  = (float*) calloc(1, sizeof(float));

    for (int cval=0; cval<3; cval++) colvals[cval] = (float*) calloc(1, sizeof(float));
    for (int tval=0; tval<3; tval++) tolvals[tval] = (float*) calloc(1, sizeof(float));

    // -> setting some values to defaults //
    first_round   =1;
    scaled_before =1;
    angledamp[0]  =0;
    lastangle[0]  =0;
    angleoffset[0]=0;
    is_tracked[0] =0;
    NumObs_old    =0;
    NumObs        =0;

    sc_reinit = sc_colvals = sc_tolvals = sc_areathresh = sc_filtersize = 0;

    InitializeCriticalSection(&CriticalSection);
}

plugClass::~plugClass()
{
    // -> deallocating image and output value buffers  //
    cvReleaseImage(&Chsv);
    cvReleaseImage(&Gmask);
    cvReleaseImage(&Cmask);
    cvReleaseImage(&Ctmp);
    cvReleaseImage(&Ctmp2);
    cvReleaseImage(&Gmasktemp);

    // -> deallocating output value buffers  //
    for (register int no=0; no<NUM_OUTPUTS; no++) free(FOutputs[no].Spread);

     // -> deallocating spreadsize-dependent buffers //
    free(reinit); free(filtersize); free(areathresh);

    free(track_window); free(track_box);   free(track_comp);
    free(angledamp);    free(angleoffset); free(lastangle);
    free(area);         free(is_tracked);

    for (int cval=0; cval<3; cval++) free(colvals[cval]);
    for (int tval=0; tval<3; tval++) free(tolvals[tval]);

    DeleteCriticalSection(&CriticalSection);
}

void plugClass::init()
{
    FImageSize.width  = FVideoInfo.frameWidth;
    FImageSize.height = FVideoInfo.frameHeight;

    selectall.x=0;  selectall.y=0;
    selectall.width=FVideoInfo.frameWidth-1;  selectall.height=FVideoInfo.frameHeight-1;

    // -> (re)allocating image buffers  //
    CCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    Chsv          = cvCreateImage(FImageSize, 8, 3);
    Gmask         = cvCreateImage(FImageSize, 8, 1);
    Cmask         = cvCreateImage(FImageSize, 8, 3);
    Ctmp          = cvCreateImage(FImageSize, 8, 3);
    Ctmp2         = cvCreateImage(FImageSize, 8, 3);
    Gmasktemp     = cvCreateImage(FImageSize, 8, 1);

    dorealloc=1;
}

char* plugClass::getParameterDisplay(DWORD index)
{
	// fill the array with spaces first
	for (int n=0; n<16; n++) 	FParams[index].DisplayValue[n] = ' ';

	sprintf(FParams[index].DisplayValue, "%f",FParams[index].Value);
	return FParams[index].DisplayValue;
}

// -> Function is called when scalar input values (types 0-6, 10 or 100) are modified //
DWORD plugClass::setParameter(SetParameterStruct* pParam)
{
	FParams[pParam->index].Value = pParam->value;

	return FF_SUCCESS;
}

// -> Function is called when spread input values (types 20, 21 or 22) are modified //
DWORD plugClass::setInput(InputStruct* pParam)
{
    int index = pParam->Index - 3;
    DWORD Slicecount = pParam->SliceCount;

    // -> if reinit choice buffer is set
    if(index==-1)
       {// -> realloc reinit choice buffer if necessary//
        if (Slicecount!=sc_reinit)
            reinit = (float*) realloc(reinit, sizeof(float)*Slicecount);
        // -> set reinit slicecounts //
        sc_reinit = Slicecount;
        // -> set reinit to input values //
        for (DWORD u=0; u<sc_reinit; u++)
            reinit[u]=pParam->Spread[u];
       }

    // -> if colorvalues are being set
    else if (index==0)
       {// -> realloc colvals buffer if necessary//
        if (Slicecount!=sc_colvals) for (int cval=0; cval<3; cval++) colvals[cval]=(float*) realloc(colvals[cval], sizeof(float)*Slicecount);
        // -> set colvals slicecounts //
        sc_colvals = Slicecount;
        // -> set colvals to input values //
        int c=0;
        for (DWORD n=0; n<sc_colvals; n++)
            {colvals[0][n] = pParam->Spread[c];
             colvals[1][n] = pParam->Spread[c+1];
             colvals[2][n] = pParam->Spread[c+2];
             c+=4;
            }
       }

    // -> if tolerance values are being set
    else if (index==1)
       {// -> realloc tolvals buffer if necessary//
        if (Slicecount!=sc_tolvals) for (int tval=0; tval<3; tval++) tolvals[tval]=(float*) realloc(tolvals[tval], sizeof(float)*Slicecount);
        // -> set tolvals slicecounts //
        sc_tolvals = Slicecount;
        // -> set tolvals to input values //
        int c=0;
        for (DWORD n=0; n<sc_tolvals; n++)
            {tolvals[0][n] = pParam->Spread[c];
             tolvals[1][n] = pParam->Spread[c+1];
             tolvals[2][n] = pParam->Spread[c+2];
             c+=4;
            }
       }

    // -> if area thresholds are set
    else if(index==2)
       {// -> realloc areathresh buffer if necessary//
       if (Slicecount!=sc_areathresh) areathresh=(float*) realloc(areathresh, sizeof(float)*Slicecount);
        // -> set areathresh slicecounts //
        sc_areathresh = Slicecount;
        // -> set areathresh to input values //
        for (DWORD u=0; u<sc_areathresh; u++) areathresh[u]=pParam->Spread[u];
       }

    // -> if medianfilter sizes are set
    else if(index==3)
       {// -> check if these input values have a different slicecount //
        if (Slicecount!=NumObs) dorealloc=1;
        // -> realloc filtersize buffer if necessary//
        if (Slicecount!=sc_filtersize) filtersize=(float*) realloc(filtersize, sizeof(float)*Slicecount);
        // -> set filtersize slicecounts //
        sc_filtersize = Slicecount;
        // -> set filtersize to input values //
        for (DWORD u=0; u<sc_filtersize; u++)
            filtersize[u]=pParam->Spread[u];
        }

    NumObs=maxNumObs();
    // -> Reallocate only if input values have different slicecounts //
    if (Slicecount!=NumObs)
        dorealloc=1;

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

///////////////////////////////////////////////////////////////////////////////////////////////////////////
// -> Frame processing takes place here: //

DWORD plugClass::processFrame24Bit(LPVOID pFrame)
{
    // -> Leave if setting function is still active //

    EnterCriticalSection(&CriticalSection);

    // -> If no tracking parameters are set, quit //
    NumObs=maxNumObs();
    if (NumObs <= 0)
    {
        for (int os=0; os<NUM_OUTPUTS; os++)
        {
            FOutputs[os].SliceCount=0;
            FOutputs[os].Spread = (float*) realloc (FOutputs[os].Spread, 0);
        }
        LeaveCriticalSection(&CriticalSection);
        return FF_SUCCESS;
    }

    // -> Reallocate input value buffers if necessary (if slicecounts are different)

/*    dorealloc &= (NumObs > 0);

    FOutputs[0].SliceCount=1;
    FOutputs[0].Spread = (float*) realloc (FOutputs[0].Spread, sizeof(float));
    FOutputs[0].Spread[0] = (DWORD) sc_reinit;
    //
*/
  if (dorealloc)
        ReallocBuffers();

     // -> putting frame into IplImage format //
    CCurrentImage->origin = 1;
    CCurrentImage->imageData = (char*)pFrame;

    cvCopy(CCurrentImage, Ctmp, 0);
    cvSetZero(Ctmp2);
    //Gmasktemp  = cvCreateImage(cvGetSize(Ctmp),IPL_DEPTH_8U,1);

    int h    = Ctmp->height;
    int w    = Ctmp->width ;
    int tol;
    float TT;
    CvScalar rgb;

    // -> set pixels outside ROI to zero //
    SetImageROI(Ctmp, (int) ((FParams[7].Value+0.5)*w), (int) ((FParams[8].Value+0.5)*h), (int) (FParams[9].Value*w), (int) (FParams[10].Value*h));

    // -> convert input image and track color into hsv space //
    cvCvtColor( Ctmp, Chsv, CV_BGR2HSV );

    ////MAIN OBJECT LOOP////////////////////////////////////////////////////////////////////////////////

    for (DWORD obj=0; obj<NumObs; obj++)
        {
        hsv     = rgb2hsv( cvScalar(colvals[0][obj], colvals[1][obj], colvals[2][obj], 0) );
         hsv_tol = rgb2hsv( cvScalar(tolvals[0][obj], tolvals[1][obj], tolvals[2][obj], 0) );

         // -> reset search if requested //
         if(reinit[obj]) first_round=1;

         if(first_round)
            // -> at initialisation, search everywhere //
            track_window[obj] = selectall;

         /////////////////////////////////////////////////////////////////////////////////////
         // STEP I : Calculate tracking parameters                                          //

         // -> calculate hue, saturation and value bounds according to tolerances //
         // -> hue bounds
         tol = (hsv_tol.val[0]>0)? (int) ( hsv_tol.val[0]) : (int) ( -hsv_tol.val[0]);

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
         tol = (hsv_tol.val[1]>0)? (int) ( hsv_tol.val[1]) : (int) ( -hsv_tol.val[1] );

         smin =(int) hsv.val[1]-tol;
         if (smin<0) smin = 0;  if (smin>255) smin = 256;

         smax =(int) hsv.val[1]+tol;
         if (smax<0) smax = 0;  if (smax>255) smax = 256;

         // -> value bounds
         tol = (hsv_tol.val[2]>0)? (int) ( hsv_tol.val[2] ) : (int) ( -hsv_tol.val[2] );

         vmin =(int) hsv.val[2]-tol;
         if (vmin<0) vmin = 0;  if (vmin>255) vmin = 256;

         vmax =(int) hsv.val[2]+tol;
         if (vmax<0) vmax = 0;  if (vmax>255) vmax = 256;

         cvSetZero(Cmask);

         ///////////////////////////////////////////////////////////////////////////////////
         // STEP II : Mask pixels that fit in given saturation and grayscale level bounds //

         if (hmin < hmax || hmin == hmax)
            {
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
         // -> calling CamShift_mod with option 'scaled values' (Farams[11].Value)        //


         if (((int)filtersize[obj]>0) && ((int)filtersize[obj]<15))
            {
             cvCopy(Gmask, Gmasktemp, NULL);
             cvSmooth( Gmasktemp, Gmask, CV_MEDIAN, ((int)filtersize[obj]*2)-1, ((int)filtersize[obj]*2)-1, 0 );
            }

         cvCamShift_mod( Gmask, track_window[obj], cvTermCriteria( CV_TERMCRIT_EPS | CV_TERMCRIT_ITER, 10, 1 ),
                         &track_comp[obj], &track_box[obj], w, h , &first_round, &angledamp[obj], &lastangle[obj], &angleoffset[obj],
                         &area[obj], FParams[11].Value);

         // -> TT is trackingthresh (Area Threshold) to the power of 4                              //
         //    This is to fit the range of 0 (no thresholding) to 1 (object has size of full image) //
         //    with a wide numerical range for small objects                                        //
         TT = areathresh[obj] * areathresh[obj] * areathresh[obj] * areathresh[obj];

         if ( area[obj]/255.0 > TT )
            {is_tracked[obj]=1.0;
             }
         else  is_tracked[obj]=0.0;

         // -> next time we'll start just where we left //
             track_window[obj] = track_comp[obj].rect;

         // -> reinit search ROI if requested  //
         if (is_tracked[obj]==0)
            {track_window[obj].height = selectall.height;
             track_window[obj].width = selectall.width;
             track_window[obj].x = selectall.x;
             track_window[obj].y = selectall.y;
            }

         // -> show thresholded image if requested  //
         if( (DWORD)FParams[1].Value == (obj) )
           {
            cvCvtColor( Gmask, Cmask, CV_GRAY2BGR );
            cvNot(Cmask, Cmask);
            cvOr(Cmask, Ctmp, Ctmp, 0);
           }
       }
    ////END OF MAIN OBJECT LOOP/////////////////////////////////////////////////////////////////////////

    // -> show searchboxes if requested //
    if ((FParams[0].Value>-2) && (FParams[0].Value<NumObs))
       {for (DWORD obj=0; obj<NumObs; obj++)
            {hsv = rgb2hsv( cvScalar(colvals[0][obj], colvals[1][obj], colvals[2][obj], 0) );

             // -> for ROI display color
             rgb = hsv2rgb(hsv.val[0]);

             if (FParams[0].Value == (obj) || FParams[0].Value==-1)
               {
                cvRectangle( Ctmp, cvPoint (track_window[obj].x,track_window[obj].y),
                                   cvPoint (track_window[obj].x+(track_window[obj].width),track_window[obj].y+(track_window[obj].height)),
                                   cvScalar (rgb.val[0], rgb.val[1], rgb.val[2]), 1, 8, 0 );
                }
            }
       }
    cvCopy(Ctmp, CCurrentImage, 0);
    first_round=0;

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // STEP IV : Set  Outputs                                                                         //

   // -> set output value slicecount and realloc value arrays //
    if (FOutputs[0].SliceCount!=NumObs)
        for (register int op=0; op<NUM_OUTPUTS; op++)
            {
             FOutputs[op].SliceCount= NumObs;
             FOutputs[op].Spread = (float*) realloc (FOutputs[op].Spread, sizeof(float)* NumObs);
            }

    // -> set output values //
    for (register DWORD obj=0; obj<NumObs; obj++)
        {
         if (is_tracked[obj])
            {
             FOutputs[0].Spread[obj] = track_box[obj].center.x;
             FOutputs[1].Spread[obj] = track_box[obj].center.y;
             FOutputs[2].Spread[obj] = track_box[obj].size.width;
             FOutputs[3].Spread[obj] = track_box[obj].size.height;
             FOutputs[4].Spread[obj] = angledamp[obj]-0.25; // -> angle to y-axis //
            }
             FOutputs[5].Spread[obj] = is_tracked[obj];
        }

    LeaveCriticalSection(&CriticalSection);

	return FF_SUCCESS;
}

///////////////////////////////////////////////////////////////////////////////////////////////////

DWORD plugClass::processFrame32Bit(LPVOID pFrame){	return FF_FAIL; }
DWORD plugClass::processFrameCopy(ProcessFrameCopyStruct* pFrameData){ return FF_FAIL;}
DWORD plugClass::processFrameCopy24Bit(ProcessFrameCopyStruct* pFrameData){	return FF_FAIL;}
DWORD plugClass::processFrameCopy32Bit(ProcessFrameCopyStruct* pFrameData){	return FF_FAIL;}

DWORD getPluginCaps(DWORD index)
{
 switch (index)
 {

	case FF_CAP_16BITVIDEO:         return FF_FALSE;
	case FF_CAP_24BITVIDEO:	        return FF_TRUE;
	case FF_CAP_32BITVIDEO:	        return FF_FALSE;
	case FF_CAP_PROCESSFRAMECOPY:	  return FF_FALSE;
	case FF_CAP_MINIMUMINPUTFRAMES:	return NUM_INPUTS;
	case FF_CAP_MAXIMUMINPUTFRAMES:	return NUM_INPUTS;
	case FF_CAP_COPYORINPLACE:	     return FF_FALSE;
	default:	                       return FF_FALSE;
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




