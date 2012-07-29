//////project name
//ARTK+Tracker

//////description
//freeframe plugin.
//implemenation of the ARToolkitPlus library for tracking of AR markers.
//returns transformation of multiple markers in space.

//////licence
//GNU General Public License (GPL)
//english: http://www.gnu.org/licenses/gpl.html
//german: http://www.gnu.de/documents/gpl.de.html

//////language/ide
//c++/codeblocks

//////dependencies
//ARToolkitPlus library:
//http://studierstube.icg.tu-graz.ac.at/handheld_ar/artoolkitplus.php

//////initial author
//joreg -> joreg@gmx.at

//////additional coding
//norbert.riedelsheimer@hfg-gmuend.de

#ifndef __ARTKPTRACKER_HEADERFILE__
#define __ARTKPTRACKER_HEADERFILE__

//freeframe includes
#include "FreeFrame.h"


//pin constants
#define NUM_PARAMS 5
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

typedef struct InputStructTag {
	DWORD Index;
	DWORD SliceCount;
	double* Spread;
} InputStruct;

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
    DWORD getOutputSliceCount(DWORD index);
    float* getOutput(DWORD index);
    DWORD setInput(InputStruct* pParam);
    DWORD setThreadLock(DWORD Enter);
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
    void loadCameraFile();
    CRITICAL_SECTION CriticalSection;
    int FWidth, FHeight;

    unsigned char* FGrayImage;

    bool FInitialized;
    bool FNewCameraFile;
    char FDebugBuffer[999];

    char* File;

    //char FCameraFile[255];
    //string[] FMarkerFiles;

   // ARToolKitPlus::TrackerSingleMarker* tracker;
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

#endif //__ARTK+TRACKER_HEADERFILE__
