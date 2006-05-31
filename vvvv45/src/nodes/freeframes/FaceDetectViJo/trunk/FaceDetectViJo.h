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

//////initiative stressing to do it + editing
//benedikt -> benedikt@looksgood.de

//////initial author
//joreg -> joreg@gmx.at

//freeframe includes
#include "FreeFrame.h"

//opencv includes
#include <cv.h>
#include <highgui.h>
#include <string.h>
#include <string>
using namespace std;

//pin constants
#define NUM_PARAMS 6
#define NUM_INPUTS 1
#define NUM_OUTPUTS 4


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

//bene
typedef struct Obj {
     float x;
     float y;
     float width;
     float height;
     bool found;
} Obj;



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

private:
    void loadCascade();
    
    CRITICAL_SECTION CriticalSection;  
    CvSize FImageSize;
    string Filename;
    bool newCascade;
    
    Obj* Objlist;
    
    IplImage* FCurrentImage;
    IplImage* FCopy;
    CvMemStorage* FStorage;
    CvHaarClassifierCascade* FCascade;

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
