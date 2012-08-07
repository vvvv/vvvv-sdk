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


//freeframe includes
#include "FreeFrame.h"

//opencv includes
#include <opencv/cv.h>

//pin constants
#define NUM_PARAMS 9
#define NUM_INPUTS 1
#define NUM_OUTPUTS 10

//contour node includes
#include "Indexsort.h"


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
    CRITICAL_SECTION CriticalSection;

    IplImage* CCurrentImage;
    IplImage* GGrayImage;
    IplImage* tmp;

    CvSize FImageSize;

    Obj* Objlist_old;
    Obj* Objlist_new;

    CvMemStorage* FStorage;
    CvSeq* FContours;
    CvSeq* FContours_temp;

    bool B_firstRound;
    int FPointCount;
    int FContoursCount, FContoursCount_old, FContoursCount_temp;

    float* FBinSizes;
    float* angledamp, * lastangle, * angleoffset, * lastangle_temp, * angleoffset_temp;
    int *IDs_old, *IDs_new, *Sortlist;
    int inc;

    CvMoments*  FMoments;
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







