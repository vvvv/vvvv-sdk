#ifndef _VVVVST_H
#define _VVVVST_H

#include "stdafx.h"
#include <streams.h>
#include <math.h>
#include <olectl.h>
#include "Global.h"
#include "IVVVVST.h"
#include "VSTHost.h"


/*************************************************************************/
/*************************************************************************/
/*************************************************************************/

class VVVVST : public CTransInPlaceFilter,IVVVVST 
{

public : //Derived-Methods------------------------------------------------//

   DECLARE_IUNKNOWN;

   VVVVST(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr);

  ~VVVVST ();


   HRESULT CheckInputType(const CMediaType *mtIn);

   STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

   STDMETHODIMP FindPin(LPCWSTR Id, IPin **ppPin);

   static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

   HRESULT Transform (IMediaSample *pSample);

   virtual HRESULT SetMediaType(PIN_DIRECTION direction, const CMediaType *pmt);


public : //Interface-Definitions-------------------------------------------//

   STDMETHODIMP load                   (char *filename,unsigned char *val);
   STDMETHODIMP setEnable              (unsigned char value);
   STDMETHODIMP getParameterCount      (int *count);
   STDMETHODIMP getParameterProperties (wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[]);
   STDMETHODIMP getParameter           (int index,double *value);
   STDMETHODIMP setParameter           (int index,double  value);
   STDMETHODIMP isInstrument           (unsigned char *value);
   STDMETHODIMP sendMidiNotes          (int count, int note[],int velocity[]);
   STDMETHODIMP sendMidiNotesOff       ();
   STDMETHODIMP sendPolyphonic         (unsigned char polyphonicNote, unsigned char polyphonicValue);
   STDMETHODIMP sendController         (unsigned char controllerID, unsigned char controllerValue);
   STDMETHODIMP sendProgram            (unsigned char programID);
   STDMETHODIMP sendMonophonic         (unsigned char monophonicValue);
   STDMETHODIMP sendPitchbend          (unsigned char pitchbendValue);
   STDMETHODIMP getInputsCount         (int *count);
   STDMETHODIMP getOutputsCount        (int *count);
   STDMETHODIMP destroy                ();
   STDMETHODIMP getProgramNames        (int *count, wchar_t names[][256]);
   STDMETHODIMP getActualProgram       (int *count);
   STDMETHODIMP setActualProgram       (int count);
   STDMETHODIMP setBpm                 (int val);
   STDMETHODIMP hasEditor              ();
   STDMETHODIMP setWindowHandle        (HWND hwnd);
   STDMETHODIMP getWindowSize          (int *width,int *height);
   STDMETHODIMP idle                   ();



private : //Attributes----------------------------------------------------//

   VSTHost host;

   int nChannels;
   int bytesPerSample;
   int samplerate;
   int initialized;

   bool enable;

}; // class VVVVST


/*************************************************************************/
/*************************************************************************/
/*DirectShow-Filterdeclaration********************************************/


const CLSID CLSID_VVVVST = { 0xf4df121a, 0x91d3, 0x4cc1, { 0x83, 0x8f, 0x7b, 0xaa, 0xbb, 0x6a, 0xab, 0xe7 } };

const CLSID IID_IVVVVST  = { 0xd23e4eb0, 0xe697, 0x4df9, { 0x90, 0x79, 0xcd, 0xbc, 0xb,  0x24, 0xed, 0xbe } };


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
sudVVVVST = { &CLSID_VVVVST      // class id
             , L"VVVVST"         // strName
             , MERIT_DO_NOT_USE  // dwMerit
             , 2                 // nPins
             , psudPins          // lpPin
            };

CFactoryTemplate g_Templates[1]= { { L"VVVVST"
                                   , &CLSID_VVVVST
                                   , VVVVST::CreateInstance
                                   , NULL
								   , &sudVVVVST}
                                 };

int g_cTemplates = sizeof(g_Templates)/sizeof(g_Templates[0]);

/*************************************************************************/
/*************************************************************************/
/*************************************************************************/

#endif
