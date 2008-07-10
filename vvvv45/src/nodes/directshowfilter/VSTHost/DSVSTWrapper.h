
#ifndef _DSVSTWrapper_H
#define _DSVSTWrapper_H

#include "stdafx.h"
#include <streams.h>
#include <math.h>
#include <olectl.h>
#include "IDSVSTWrapper.h"
#include "Global.h"
#include "VSTHost.h"

class DSVSTWrapper : public CTransInPlaceFilter,IDSVSTWrapper
{

public : 

   DECLARE_IUNKNOWN;

   DSVSTWrapper(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr);

  ~DSVSTWrapper ();


   HRESULT CheckInputType(const CMediaType *mtIn);

   STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

   STDMETHODIMP FindPin(LPCWSTR Id, IPin **ppPin);

   HRESULT Transform (IMediaSample *pSample);

   virtual HRESULT SetMediaType(PIN_DIRECTION direction, const CMediaType *pmt);

   static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);


   //IDSVSTWrapper Interface-Definitions

   STDMETHODIMP load                   (char *filename);
   STDMETHODIMP setEnable              (unsigned char value);
  
   STDMETHODIMP getParameterCount      (int *count);
   STDMETHODIMP getParameterProperties (wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[]);
   STDMETHODIMP getParameter           (int index,double *value);
   STDMETHODIMP setParameter           (int index,double  value);

   STDMETHODIMP getMidiIsInstrument    ();
   STDMETHODIMP sendMidiNote           (int count, int note[],int velocity[]);
   STDMETHODIMP sendMidiNoteAllOff     ();
   STDMETHODIMP sendMidiPolyphonic     (unsigned char polyphonicNote, unsigned char polyphonicValue);
   STDMETHODIMP sendMidiController     (unsigned char controllerID, unsigned char controllerValue);
   STDMETHODIMP sendMidiProgram        (unsigned char programID);
   STDMETHODIMP sendMidiMonophonic     (unsigned char monophonicValue);
   STDMETHODIMP sendMidiPitchbend      (unsigned char pitchbendValue);

   STDMETHODIMP getHasWindow           ();
   STDMETHODIMP setWindowHandle        (HWND hwnd);
   STDMETHODIMP getWindowSize          (int *width,int *height);
   STDMETHODIMP setWindowIdle          ();

   STDMETHODIMP getInputCount          (int *count);
   STDMETHODIMP getOutputCount         (int *count);
   STDMETHODIMP getProgramNames        (int *count, wchar_t names[][256]);
   STDMETHODIMP getActualProgram       (int *count);
   STDMETHODIMP setActualProgram       (int count);
   STDMETHODIMP setBpm                 (int val);
   
private : 

   VSTHost vstHost;

   int nChannels;
   int bytesPerSample;
   int samplerate;
   int initialized;

   bool enable;


}; 



/*************************************************************************/
/*************************************************************************/
/*DirectShow-Filterdeclaration********************************************/


const CLSID CLSID_DSVSTWrapper = { 0xf4df121a, 0x91d3, 0x4cc1, { 0x83, 0x8f, 0x7b, 0xaa, 0xbb, 0x6a, 0xab, 0xe7 } };

const CLSID IID_IDSVSTWrapper  = { 0xd23e4eb0, 0xe697, 0x4df9, { 0x90, 0x79, 0xcd, 0xbc, 0xb,  0x24, 0xed, 0xbe } };

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
sudDSVSTWrapper = { &CLSID_DSVSTWrapper      // class id
             , L"DSVSTWrapper"         // strName
             , MERIT_DO_NOT_USE  // dwMerit
             , 2                 // nPins
             , psudPins          // lpPin
            };

CFactoryTemplate g_Templates[1]= { { L"DSVSTWrapper"
                                   , &CLSID_DSVSTWrapper
                                   , DSVSTWrapper::CreateInstance
                                   , NULL
								   , &sudDSVSTWrapper}
                                 };

int g_cTemplates = sizeof(g_Templates)/sizeof(g_Templates[0]);

/*************************************************************************/
/*************************************************************************/
/*************************************************************************/


#endif
