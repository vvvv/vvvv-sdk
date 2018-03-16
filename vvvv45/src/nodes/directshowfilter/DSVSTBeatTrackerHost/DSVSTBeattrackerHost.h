#ifndef _TESTTRACKER_H
#define _TESTTRACKER_H 

#include "stdafx.h"
#include <streams.h>
#include <math.h>
#include <olectl.h>
#include "IBeattracker.h"
#include "VSTHost.h"
#include "Time.h"
#include "GlobalDefine.h"
#include "Feedback.h"


//----------------------------------------------------------------------------------------------------------------//

class PluginLoader
{
  //private : void *module;

  public  : void *module;

  public  :  PluginLoader (); 
            ~PluginLoader ();

			bool loadDLL (const char *fileName);
			PluginEntry  getMainEntry ();
            void destroy ();

};

PluginEntry pluginEntry;

class DSVSTBeattrackerHost : public CTransInPlaceFilter,IBeattracker 
{

public:

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

    DECLARE_IUNKNOWN;

    HRESULT CheckInputType(const CMediaType *mtIn);

    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

    STDMETHODIMP FindPin(LPCWSTR Id, IPin **ppPin);

	STDMETHODIMP getFeedback(int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,int *bpm);

	STDMETHODIMP setDelay (int delay);

	STDMETHODIMP setResolution (int resolution);

	STDMETHODIMP setMinBPM (int minBPM);

	STDMETHODIMP setMaxBPM (int maxBPM);

	STDMETHODIMP destroy   ();

private:

    DSVSTBeattrackerHost(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr);

    HRESULT Transform (IMediaSample *pSample);

	virtual HRESULT SetMediaType(PIN_DIRECTION direction, const CMediaType *pmt);

	bool setInput       (IMediaSample *pSample);

	void sendInput      (IMediaSample *pSample);
	
	void setOutput      (IMediaSample *pSample);
	
	void setMute        (IMediaSample *pSample);
	
	void cleanUp        ();
	
	bool checkSource    ();
	
	void reset          ();

	void init           ();
	
	LONGLONG streamTime ();

	AEffect *plugin;

	int samplesPerSecond;        
    int bytesPerSample;        
    int channel; 
	int resolution;
	int nSamplesMono;
	int nSamplesStereo;
	int nFrames;
	int frameSize;
	int inputSource;
	int delay;
	int minBPM;
	int maxBPM;

	double phase;
	double period;
	double frameTime;
	double performanceFreq;

	wchar_t buffer[512];

	short int *samples;
	BYTE  *pb;
	float *in;
	float *out;

	bool process;
	bool acousticFeedback;

	Time time;
	
    INT64 beatTime[25];
	
	CFeedback fb;

	

}; // class DSVSTBeattrackerHost


//----------------------------------------------------------------------------------------------------------------//

const AMOVIESETUP_MEDIATYPE
sudPinTypes =   { &MEDIATYPE_Audio        // clsMajorType
                , &MEDIASUBTYPE_NULL };   // clsMinorType

const AMOVIESETUP_PIN
psudPins[] = { { L"In0"          // strName
               , FALSE           // bRendered
               , FALSE           // bOutput
               , FALSE           // bZero
               , FALSE           // bMany
               , &CLSID_NULL     // clsConnectsToFilter
               , L"Output"       // strConnectsToPin
               , 1               // nTypes
               , &sudPinTypes    // lpTypes
               }
             , { L"Out0"         // strName
               , FALSE           // bRendered
               , TRUE            // bOutput
               , FALSE           // bZero
               , FALSE           // bMany
               , &CLSID_NULL     // clsConnectsToFilter
               , L"Input"        // strConnectsToPin
               , 1               // nTypes
               , &sudPinTypes    // lpTypes
               }
             };

const AMOVIESETUP_FILTER
sudDSVSTBeattrackerHost = { &CLSID_DSVSTBeattrackerHost // class id
            , L"DSVSTBeattrackerHost"           // strName
            , MERIT_DO_NOT_USE          // dwMerit
            , 2                         // nPins
            , psudPins                  // lpPin
            };

CFactoryTemplate g_Templates[1]= { { L"DSVSTBeattrackerHost"
                                   , &CLSID_DSVSTBeattrackerHost
                                   , DSVSTBeattrackerHost::CreateInstance
                                   , NULL
								   , &sudDSVSTBeattrackerHost}

                                 };

int g_cTemplates = sizeof(g_Templates)/sizeof(g_Templates[0]);


#endif
