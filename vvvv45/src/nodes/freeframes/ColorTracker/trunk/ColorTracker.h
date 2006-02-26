//////project name
//ColorTracker

//////description
//freeframe plugin.
//outputs location(x/y) width/height and rotation angle of 
//tracked object from image thresholded with parameters extracted from input color.

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
//Marc Sandner -> ms@saphmar.net

//////edited by
//your name here

//freeframe includes
#include "FreeFrame.h"

//opencv includes
#include <cv.h>
#include <highgui.h>

//pin constants
#define NUM_PARAMS 11
#define NUM_INPUTS 1
#define NUM_OUTPUTS 6


// implementation specific definitions

typedef struct ParamConstsStructTag {
    unsigned int Type;
	float Default;
	char Name[16];
} ParamConstsStruct;

typedef struct ParamStructTag {
	float Value;
	char DisplayValue[16];
} ParamStruct;

typedef struct OutputConstsStructTag {
    unsigned int Type;
    char Name[16];
} OutputConstsStruct;

typedef struct OutputStructTag {
	DWORD SliceCount;
    float* Spread;
} OutputStruct;

typedef struct VideoPixel24bitTag {
	BYTE red;
	BYTE green;
	BYTE blue;
} VideoPixel24bit;

typedef struct VideoPixel16bitTag {
	BYTE fb;
	BYTE sb;
} VideoPixel16bit;

typedef struct VideoPixel32bitTag {
	BYTE blue;
	BYTE green;
	BYTE red;
	BYTE alpha;
} VideoPixel32bit;


// PluginInstance Object - these calls relate to instances of plugObj
// created by FF_INSTANTIATE

class plugClass 
{

public:
    plugClass();
    ~plugClass();
	
	void init();

	char* getParameterDisplay(DWORD index);			
	DWORD setParameter(SetParameterStruct* pParam);		
	float getParameter(DWORD index);
	
	//joregs
	DWORD setThreadLock(DWORD Enter);
    DWORD getOutputSliceCount(DWORD index);					
    float* getOutput(DWORD index);
    //
    
	DWORD processFrame(LPVOID pFrame);
	DWORD processFrame24Bit(LPVOID pFrame);
	DWORD processFrame32Bit(LPVOID pFrame);
	DWORD processFrameCopy(ProcessFrameCopyStruct* pFrameData);
	DWORD processFrameCopy24Bit(ProcessFrameCopyStruct* pFrameData);
	DWORD processFrameCopy32Bit(ProcessFrameCopyStruct* pFrameData);
	
	ParamStruct FParams[NUM_PARAMS];
	OutputStruct FOutputs[NUM_OUTPUTS];

	VideoInfoStruct FVideoInfo;
	int FVideoMode;

 // marc's color conversion functions //
	CvScalar hsv2rgb( float hue );
	CvScalar rgb2hsv(CvScalar rgb);
    
private:
    CRITICAL_SECTION CriticalSection;  
     
    IplImage* CCurrentImage;
    IplImage* Chsv, *Ghue, *Gbackproject;
    IplImage* Gmask, *Gmasktemp;
    
    CvSize FImageSize;
    CvHistogram *hist;
    
    CvRect selectall;
    CvRect track_window;
    CvBox2D track_box;
    CvConnectedComp track_comp ;
    
    int lowerbound, upperbound ;
    int smin , smax ;
    int vmin , vmax ;
    int hmin , hmax ;
    
    CvScalar hsv ;
     
    int first_round, scaled_before;    
    float angledamp , lastangle , angleoffset, is_tracked, area ;
    
    int click;
    
    float Pos[7];
};

// Function prototypes - Global Plugin Functions that lie outside the instance object
// see http://freeframe.sourceforge.net/spec.html for details

PlugInfoStruct*	getInfo();							

DWORD	initialise();								

DWORD	deInitialise();								

DWORD	getNumParameters();	

char*	getParameterName(DWORD index);
				
float	getParameterDefault(DWORD index);

unsigned int getParameterType(DWORD index);			

DWORD	getPluginCaps(DWORD index);	

LPVOID instantiate(VideoInfoStruct* pVideoInfo);

DWORD deInstantiate(LPVOID instanceID);	

LPVOID getExtendedInfo();		

//joregs
DWORD	getNumOutputs();
unsigned int getOutputType(DWORD index);
char*	getOutputName(DWORD index);



 

  
