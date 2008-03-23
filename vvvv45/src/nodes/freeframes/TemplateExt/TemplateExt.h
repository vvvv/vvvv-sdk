//////project name
//Trautner

//////description
//freeframe plugin.
//simple movement detection in regions defined via a grayscale bitmap.
//based on an idea by mr. trautner -> http://www.brainsalt.net

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
//your name here

//freeframe includes
#include "FreeFrame.h"

//for calloc():
#include <string>


//pin constants
#define NUM_INPUTS 1  //number of video inputs

#define NUM_PARAMS 3  //number of input pins
#define NUM_OUTPUTS 2 //number of output pins


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
	DWORD processFrameCopy(ProcessFrameCopyStruct* pFrameData);

	ParamStruct FParams[NUM_PARAMS];
	OutputStruct FOutputs[NUM_OUTPUTS];

	VideoInfoStruct FVideoInfo;
	int FVideoMode;

private:
    CRITICAL_SECTION CriticalSection;

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

//freeframe 1.0 extended. see: http://vvvv.org/tiki-index.php?page=FreeFrameExtendedSpecification
DWORD	getNumOutputs();
unsigned int getOutputType(DWORD index);
char*	getOutputName(DWORD index);
