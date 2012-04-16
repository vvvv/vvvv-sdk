//////project name
//Contour

//////description
//freeframe plugin.
//returns points(x/y) along contours found in the thresholded input.

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
//joreg -> joreg@gmx.at

//////edited by
//Marc Sandner -> ms@saphmar.net

//includes
#include "Contour.h"
#include "moments_mod.h"
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
     // -> Input pins /////////////////

    cvSetErrMode(CV_ErrModeSilent);

    // -> Types & default values for input pins //

    GParamConstants[0].Type = 0;
    GParamConstants[1].Type = 10;
   	GParamConstants[2].Type = 0;
   	GParamConstants[3].Type = 0;
   	GParamConstants[4].Type = 0;
   	GParamConstants[5].Type = 10;
   	GParamConstants[6].Type = 0;
   	GParamConstants[7].Type = 0;
   	GParamConstants[8].Type = 10;

    GParamConstants[0].Default = 1.0f;
   	GParamConstants[1].Default = 0.5f;
   	GParamConstants[2].Default = 0.0f;
   	GParamConstants[3].Default = 0.0f;
   	GParamConstants[4].Default = 0.0f;
    GParamConstants[5].Default = 0.01f;
    GParamConstants[6].Default = 1.0f;
    GParamConstants[7].Default = 0.0f;
    GParamConstants[8].Default = 10.0f;

  	 // -> Naming of input pins //

    char tempName0[17] = "Show Filtered";
   	char tempName1[17] = "Threshold";
   	char tempName2[17] = "Invert";
   	char tempName3[17] = "Cleanse";
   	char tempName4[17] = "Show Contours";
   	char tempName5[17] = "Thickness";
   	char tempName6[17] = "Scaled Values";
   	char tempName7[17] = "Unique ID";
   	char tempName8[17] = "Max Countours";

    memcpy(GParamConstants[0].Name, tempName0, 16);
   	memcpy(GParamConstants[1].Name, tempName1, 16);
   	memcpy(GParamConstants[2].Name, tempName2, 16);
   	memcpy(GParamConstants[3].Name, tempName3, 16);
   	memcpy(GParamConstants[4].Name, tempName4, 16);
   	memcpy(GParamConstants[5].Name, tempName5, 16);
   	memcpy(GParamConstants[6].Name, tempName6, 16);
   	memcpy(GParamConstants[7].Name, tempName7, 16);
    memcpy(GParamConstants[8].Name, tempName8, 16);

    // -> Output pins //

    // -> Types for output pins //

    GOutputConstants[0].Type = 10;
    GOutputConstants[1].Type = 10;
    GOutputConstants[2].Type = 0;
    GOutputConstants[3].Type = 10;
    GOutputConstants[4].Type = 10;
    GOutputConstants[5].Type = 10;
    GOutputConstants[6].Type = 10;
    GOutputConstants[7].Type = 10;
    GOutputConstants[8].Type = 10;
    GOutputConstants[9].Type = 0;

    // -> Naming of output pins //

   	char outName0[17] = "Contours X";
   	char outName1[17] = "Contours Y";
   	char outName2[17] = "Contours BinSize";
   	char outName3[17] = "X";
   	char outName4[17] = "Y";
   	char outName5[17] = "Width";
   	char outName6[17] = "Height";
   	char outName7[17] = "Orientation";
   	char outName8[17] = "Area";
   	char outName9[17] = "Contours ID";

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
    // -> allocating output buffers //

    for (register int i=0; i<NUM_OUTPUTS; i++)
        {
         FOutputs[i].SliceCount = 0;
         FOutputs[i].Spread = (float*) calloc(1, 0);
        }

    FStorage = cvCreateMemStorage(0);
    FContours = 0;

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
    // -> deallocating image and output value buffers  //
    cvReleaseImage(&GGrayImage);
    cvReleaseImage(&tmp);
    cvReleaseMemStorage(&FStorage);

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

    // -> setting defaults for input values //

    for (int in=0; in<NUM_PARAMS; in++) FParams[in].Value=GParamConstants[in].Default;

    // -> allocating image buffers  //
    CCurrentImage = cvCreateImageHeader(FImageSize, IPL_DEPTH_8U, 3);
    GGrayImage = cvCreateImage(FImageSize, IPL_DEPTH_8U, 1);
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
	for (int n=0; n<16; n++) 	FParams[index].DisplayValue[n] = ' ';

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

////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////

DWORD plugClass::processFrame24Bit(LPVOID pFrame)
{
    EnterCriticalSection(&CriticalSection);

    CCurrentImage->origin = 1;
    CCurrentImage->imageData = (char*)pFrame;
    float h = CCurrentImage->height;
    float w = CCurrentImage->width;

    //////////////////////////////////////////////////////////////////////
    // STEP I : image preprocessing and call of 'FindContours' function //

    cvCvtColor(CCurrentImage, GGrayImage, CV_BGR2GRAY);
    // -> threshold the image //
    cvThreshold(GGrayImage, GGrayImage, FParams[1].Value * 255, 255, CV_THRESH_BINARY);
    // -> invert image if requested //
    if (FParams[2].Value >= 0.5)
    {
        for (int i=0; i<(GGrayImage->height*GGrayImage->width); i++)
            GGrayImage->imageData[i] = (GGrayImage->imageData[i]) ? 0 : 255;
    }
    // -> perform Median filtering (cleansing) of input image if requested //
    if (FParams[3].Value >= 0.5)
    {
        cvSmooth( GGrayImage, tmp, CV_MEDIAN, 9,  9, 0 );
        cvCopy (tmp, GGrayImage, NULL);
    }

    if (FParams[0].Value >= 0.5)
        cvCvtColor(GGrayImage, CCurrentImage, CV_GRAY2BGR);

    // -> clear memory from last round //
    cvClearMemStorage(FStorage);

    // -> find the contours //
    FContoursCount_old  = FContoursCount;
    FContoursCount_temp = cvFindContours(GGrayImage, FStorage, &FContours, sizeof(CvContour), CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE);

    //limit contourscount
    FContoursCount_temp = min(FContoursCount_temp, (int)FParams[8].Value);

    FPointCount = 0;
    if (FBinSizes)
        free(FBinSizes);
    if (FMoments)
        free(FMoments);

    // -> allocate FBinSizes and FMoments arrays //
    FBinSizes = (float*) calloc(FContoursCount_temp, sizeof(float));
    FMoments = (CvMoments*) calloc(FContoursCount_temp, sizeof(CvMoments));
    // -> save pointer to firstcontour //
    FContours_temp = FContours;

    ///////////////////////////////////////////////////////////////////
    // STEP II : calculation of contour moments & contour validation //

    FPointCount = 0;
    FContoursCount = 0;

    for(int i=0; i<FContoursCount_temp; i++)
    {
        // -> calculate orientation of contour //
        if (FParams[6].Value)
            cvMoments_mod(FContours, &FMoments[i], (float)w, (float)h, 0);
        else
            cvMoments(FContours, &FMoments[i]);

        if (FMoments[i].m00 != 0.0)
        {
            FContoursCount++ ; // -> Contour is valid & counted
            FPointCount += FContours->total;
        }
        FContours = FContours->h_next;
    }
    FContours = FContours_temp;
    if (B_firstRound)
        FContoursCount_old =  FContoursCount;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    // STEP III : set slicecounts and reallocate fresh memory for output and angle-calculation variables  //

    for (int i=0; i<2; i++)
    {
        FOutputs[i].SliceCount = FPointCount;
        FOutputs[i].Spread = (float*) realloc(FOutputs[i].Spread, sizeof(float) * FPointCount);
    }
    for (int i=2; i<10; i++)
    {
        FOutputs[i].SliceCount = FContoursCount;
        FOutputs[i].Spread = (float*) realloc(FOutputs[i].Spread, sizeof(float) * FContoursCount);
    }

    Sortlist = (int*)   realloc(Sortlist, sizeof(int) * FContoursCount);
    angledamp = (float*) realloc(angledamp, sizeof(float) * FContoursCount);
    lastangle_temp = (float*) realloc(lastangle_temp, sizeof(float) * FContoursCount);
    angleoffset_temp = (float*) realloc(angleoffset_temp, sizeof(float) * FContoursCount);

    ////////////////////////////////////////////////////////////////////
    // STEP IV : reallocate fresh memory for IDs and prepare ID lists //

    IDs_old = (int*) realloc(IDs_old, sizeof(int) * FContoursCount_old);
    Objlist_old = (Obj*) realloc(Objlist_old, sizeof(Obj) * FContoursCount_old);
    // -> put old Object positions and IDs into object list //
    if (B_firstRound)
    {
        IDs_new = (int*) realloc(IDs_new, sizeof(int) * FContoursCount);
        for (int i=0; i<FContoursCount; i++) IDs_new[i] = i;
            Objlist_new = (Obj*) realloc(Objlist_new, sizeof(Obj) * FContoursCount);
    }
    for (int i=0; i<FContoursCount_old; i++)
    {
         Objlist_old[i].x = Objlist_new[i].x;  Objlist_old[i].y = Objlist_new[i].y;
         IDs_old[i] = IDs_new[i];
    }

    Objlist_new = (Obj*) realloc(Objlist_new, sizeof(Obj) * FContoursCount);
    IDs_new = (int*) realloc(IDs_new, sizeof(int) * FContoursCount);

    // -> define and init variables needed to calculate output values //
    int binsize;
    int j=0, p=0;
    float theta, inv_m00, cs, sn, rotate_a, rotate_c, width, length;
    float a, b, c, square;
    CvPoint* PointArray;
    float ratio     = w/h;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // STEP IV : go through all contours, check them (area!=0), calculate and set output values if valid //

    // -> loop over all found contours (variable i) and valid contours (j) //
    for(int i=0; i<FContoursCount_temp; i++)
    {
        if (FMoments[i].m00!=0.0)
        {
            // -> draw contour if requested //
            if (FParams[4].Value > 0)
               cvDrawContours(CCurrentImage, FContours, CV_RGB(255,0,0), CV_RGB(0,255,0), /*(int)(FParams[2].Value * 255)*/ 0, (int)(FParams[5].Value * 255), CV_AA);

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
            FOutputs[3].Spread[j] = (FParams[6].Value)? (FMoments[i].m10*inv_m00) : (((FMoments[i].m10*inv_m00) / w) - 0.5) * ratio;
            FOutputs[4].Spread[j] = (FParams[6].Value)? (FMoments[i].m01*inv_m00) : ((FMoments[i].m01*inv_m00) / h) - 0.5;
            Objlist_new[j].x = FOutputs[3].Spread[j];
            Objlist_new[j].y = FOutputs[4].Spread[j];

            // -> calculate and set height, width and rotation angle of object //
            a = FMoments[i].mu20 * inv_m00;
            b = FMoments[i].mu11 * inv_m00;
            c = FMoments[i].mu02 * inv_m00;
            square = sqrt( 4 * b * b + (a - c) * (a - c) );
            theta = atan2( (2 * b), (a - c + square));
            cs = cos( theta ); sn = sin( theta );
            rotate_a = cs * cs * FMoments[i].mu20 + 2.0 * cs * sn * FMoments[i].mu11 + sn * sn * FMoments[i].mu02;
            rotate_c = sn * sn * FMoments[i].mu20 - 2.0 * cs * sn * FMoments[i].mu11 + cs * cs * FMoments[i].mu02;

            // TODO : why is there a linear abberation in object lenght?
            //       (as a temp fix it is compensated by multiplicator 0.882 calculated by linear regression)
            width = sqrt( rotate_c * inv_m00 ) * 4.0;
            length = sqrt( rotate_a * inv_m00 ) * 4.0;
            if( length  < width )
            {
                double t;
                CV_SWAP( length , width, t );
                CV_SWAP( cs, sn, t );
                theta = CV_PI*0.5 - theta;
            }
            FOutputs[5].Spread[j] = (FParams[6].Value)? width : (width /w)*ratio;
            FOutputs[6].Spread[j] = (FParams[6].Value)? length : (length/h);
            FOutputs[8].Spread[j] = (FParams[6].Value)? FMoments[i].m00 : FMoments[i].m00/(w*h);

            // -> save angle to temp array for further processing (PART V) //
            lastangle_temp[j] = (theta / (2*CV_PI));

            free(PointArray);
            j++;
        }
        FContours = FContours->h_next;
    }

    /////////////////////////////////////////////////////////////////////////////////
    // STEP V : sorting the object list, calculating damped angle for each object  //

    if (!B_firstRound)
    {
        adaptindex(Objlist_old, Objlist_new, FContoursCount_old, FContoursCount, IDs_old, IDs_new, Sortlist, &inc, (int)FParams[7].Value);
    }
    // -> angle damping //
    if (!B_firstRound)
    {
        for (j=0; j<FContoursCount; j++ )
        {
            if (Sortlist[j]!=-1)
            {
                angleoffset_temp[j]= angleoffset[Sortlist[j]];

                if (lastangle_temp[j]-lastangle[Sortlist[j]] < -0.4 )
                    angleoffset_temp[j] += 0.5;
                else
                {
                    if (lastangle_temp[j]-lastangle[Sortlist[j]] >  0.4 )
                        angleoffset_temp[j] -= 0.5;
                }
            }
            else
                angleoffset_temp[j]=0;

            angledamp[j] = lastangle_temp[j] + angleoffset_temp[j];
            FOutputs[7].Spread[j] = angledamp[j]-0.25; // -> angle to y-axis //
        }
    }
    else
    {
        for (j=0; j<FContoursCount; j++)
        {
            angleoffset_temp[j] = 0;
            angledamp[j] = lastangle_temp[j];
            FOutputs[7].Spread[j] = angledamp[j] - 0.25; // -> angle to y-axis //
        }
    }

    lastangle = (float*) realloc(lastangle,   sizeof(float) * FContoursCount);
    angleoffset  = (float*) realloc(angleoffset, sizeof(float) * FContoursCount);

    for (j=0; j<FContoursCount; j++)
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
	GPlugInfo.APIMinorVersion = 200;		// this is the number after the decimal point
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
