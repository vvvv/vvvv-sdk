#ifndef _DSVSTHOST_H
#define _DSVSTHOST_H 

#include "stdafx.h"
#include <streams.h>
#include <math.h>
#include <olectl.h>
#include "Global.h"
#include "VSTHost.h"
#include "IDSVSTHost.h"

/* 
   Schnittstelle : Keyboard





*/


const CLSID CLSID_DSVSTHost = {	0xf4df121a, 0x91d3, 0x4cc1, { 0x83, 0x8f, 0x7b, 0xaa, 0xbb, 0x6a, 0xab, 0xe7 } };

const CLSID IID_IDSVSTHost  = { 0xd23e4eb0, 0xe697, 0x4df9, { 0x90, 0x79, 0xcd, 0xbc, 0xb,  0x24, 0xed, 0xbe } };

extern VstIntPtr VSTCALLBACK HostCallback (AEffect *effect,
										   VstInt32 opcode,
										   VstInt32 index,
										   VstIntPtr value,
										   void *ptr,
										   float opt);

class Host;

class DSVSTHost : public CTransInPlaceFilter,IDSVSTHost 
{

public:

    DECLARE_IUNKNOWN;

	HRESULT CheckInputType(const CMediaType *mtIn);

    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

    STDMETHODIMP FindPin(LPCWSTR Id, IPin **ppPin);

    
	//IDSVSTHost---------------------------------------------//

	STDMETHODIMP loadPlugin( char *filename );

	STDMETHODIMP getParameterNumber (int *number);

    STDMETHODIMP getParameterValue  (int index, double *value );

	STDMETHODIMP getParameterProperties ( wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[]);
    
	STDMETHODIMP setParameter ( int index, double value );

	STDMETHODIMP setEnabled(unsigned char enabled);

	STDMETHODIMP canDoMidi(unsigned char *can);

	STDMETHODIMP sendMidiNotes(unsigned char note, unsigned char velocity);

	STDMETHODIMP sendMidiNotesOff();

	STDMETHODIMP sendPolyphonic(unsigned char polyphonicNote, unsigned char polyphonicValue);

	STDMETHODIMP sendMidiController(unsigned char controllerID, unsigned char controllerValue);

	STDMETHODIMP sendProgram(unsigned char programID);

	STDMETHODIMP sendMonophonic(unsigned char monophonicValue);

	STDMETHODIMP sendPitchbend(unsigned char pitchbendValue);

	STDMETHODIMP destroy (HMODULE *hModule);     


    DSVSTHost(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr);

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

   ~DSVSTHost ();

private:

    HRESULT Transform (IMediaSample *pSample);

	virtual HRESULT SetMediaType(PIN_DIRECTION direction, const CMediaType *pmt);

	int samplesPerSecond;        
    int bytesPerSample;        
    int nChannels; 

	unsigned char enabled;

	Host *host;

}; // class DSVSTHost


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
sudDSVSTHost = { &CLSID_DSVSTHost // class id
            , L"DSVSTHost"           // strName
            , MERIT_DO_NOT_USE          // dwMerit
            , 2                         // nPins
            , psudPins                  // lpPin
            };

CFactoryTemplate g_Templates[1]= { { L"DSVSTHost"
                                   , &CLSID_DSVSTHost
                                   , DSVSTHost::CreateInstance
                                   , NULL
								   , &sudDSVSTHost}

                                 };

int g_cTemplates = sizeof(g_Templates)/sizeof(g_Templates[0]);


#endif
