#include "DSVSTHost.h" 


DSVSTHost::DSVSTHost(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr)
    : CTransInPlaceFilter (tszName, punk,CLSID_DSVSTHost, phr)
{
  samplesPerSecond = 0;        
  bytesPerSample   = 0;        
  nChannels        = 0; 

  enabled = true;

  host = new Host();

}//Constructor

DSVSTHost::~DSVSTHost()
{
  OutputDebugString(L"DSVSTHost::~DSVSTHost()\n");
 
}

STDMETHODIMP DSVSTHost::loadPlugin( char *filename )
{
  if((host == NULL) || (filename == NULL)) return ERROR;
  
  if(!host->loadPlugin( filename )) 
  {
	host = NULL;

	return ERROR;
  }
  
  return NOERROR;
}

STDMETHODIMP DSVSTHost::getParameterNumber (int *number)
{
  if(host == NULL) return ERROR;

  if(!host->getParameterNumber(number)) return ERROR;

  return S_OK;
}

STDMETHODIMP DSVSTHost::getParameterProperties ( wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[])
{ 
  if(host == NULL) return ERROR;

  if( !host->getParameterProperties( paramDisplay, paramName, paramLabel, paramValue ) ) return ERROR;

  return S_OK;
}

STDMETHODIMP DSVSTHost::getParameterValue (int index, double *value)
{
  if(host == NULL) return ERROR;

  host->getParameterValue( index, value);   

  return S_OK;
}

STDMETHODIMP DSVSTHost::setParameter(int index, double value)
{
  if(host == NULL) return ERROR;

  if( !host->setParameter( index, (float)value) ) return ERROR;

  return S_OK;
}

STDMETHODIMP DSVSTHost::setEnabled(unsigned char enabled)
{
  if(host==NULL) return ERROR;

  this->enabled = enabled; 

  return S_OK;
}

STDMETHODIMP DSVSTHost::canDoMidi(unsigned char *can)
{
  if(host==NULL) return ERROR;

  host->canDoMidi(can);
  
  return S_OK;
}

STDMETHODIMP DSVSTHost::sendMidiNotes(unsigned char note, unsigned char velocity )
{
  if(host==NULL) return ERROR;

  host->sendMidiNotes (note, velocity); 

  return S_OK;
}

STDMETHODIMP DSVSTHost::sendMidiNotesEx(int number, int note[], int velocity[] )
{
  if(host==NULL) return ERROR;

  host->sendMidiNotesEx(number, note, velocity); 

  return S_OK;
}

STDMETHODIMP DSVSTHost::sendMidiNotesOff()
{
  if(host == NULL) return ERROR;

  host->sendMidiNotesOff ();

  return S_OK;
}

STDMETHODIMP DSVSTHost::sendMidiController(unsigned char controllerID, unsigned char controllerValue)
{
  if(host==NULL) return ERROR;

  host->sendMidiController(controllerID, controllerValue);  

  return S_OK;
}

STDMETHODIMP DSVSTHost::sendProgram(unsigned char programID)
{
  if(host == NULL) return ERROR;

  host->sendProgram(programID);

  return S_OK;
}

STDMETHODIMP DSVSTHost::sendPolyphonic(unsigned char polyphonicNote, unsigned char polyphonicValue)
{
  if(host==NULL) return ERROR;

  host->sendPolyphonic( polyphonicNote, polyphonicValue);

  return S_OK;
}

STDMETHODIMP DSVSTHost::sendMonophonic(unsigned char monophonicValue)
{
  if(host == NULL) return ERROR;

  host->sendMonophonic( monophonicValue);

  return S_OK;
}

STDMETHODIMP DSVSTHost::sendPitchbend(unsigned char pitchbendValue)
{
  if(host==NULL) return ERROR;

  host->sendPitchbend( pitchbendValue);

  return S_OK;
}

STDMETHODIMP DSVSTHost::destroy(HMODULE *hModule)
{
  if(host == NULL) return ERROR;

  host->destroy();

  return S_OK;
}

HRESULT DSVSTHost::Transform(IMediaSample *pSample)
{
  if(pSample == NULL || host == NULL) return ERROR;

  if(!enabled) return NOERROR;

  //Die gesamte Anzahl der Samples------------------------------------------//
  long nSamples = pSample->GetActualDataLength() / bytesPerSample;
  int length    = nSamples / nChannels;

  //Für jeden der Eingabekanäle einen Puffer allokieren---------------------//
  float **in  = new float*[nChannels];
  float **out = new float*[nChannels];

  for(int i=0; i<nChannels; i++)
  {
    in [i] = new float[nSamples / nChannels];
	out[i] = new float[nSamples / nChannels];
  }

  //Die gesamte Anzahl der Samples einlesen---------------------------------//
  byte *ptrByte;

  pSample->GetPointer(&ptrByte);

  short int *samples = new short int[nSamples];

  for(int i=0; i<nSamples; i++)
  {
	short int *ptrInt = (short int*) ptrByte;

	samples[i] = *ptrInt;

	ptrByte += bytesPerSample;
  }

  //Die Samples in Float-Werte überführen und auf die Kanäle verteilen------//
  for(int k=0; k<nChannels; k++) 
  for(int i=0; i<nSamples / nChannels; i++)
  {
	long shift = (i * nChannels) + k;

	in [k][i] = ( (float)samples[shift] / 32768.);
	out[k][i] = 0;
  }

  
  //Die Zeiger samt der Anzahl der Samples in einem Kanal senden------------//
  //host->process( in, out, length, nChannels, samplesPerSecond );
  host->process( in, out, length, nChannels, samplesPerSecond);


  //Die Ausgabesamples zurückwandeln und in das MediaSample schreiben------//
  for(int k=0; k<nChannels; k++) 
  for(int i=0; i<nSamples / nChannels; i++)
   samples[ i*nChannels + k ] = (short) (out[k][i] * 32768.);

  unsigned char *ucPtr = (unsigned char *) samples;

  pSample->GetPointer(&ptrByte);

  for(int i=0; i<nSamples * bytesPerSample; i++)
  ptrByte [i] = *ucPtr++;

  //Freigeben des allokierten Speicherbereiches----------------------------//
  for(int i=0;i<nChannels;i++)
  {
   delete in  [i];
   delete out [i];
  }

  delete in;
  delete out;
  delete samples;  
  

  return NOERROR;

} // Transform


STDMETHODIMP DSVSTHost::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    CheckPointer(ppv,E_POINTER);

	if(riid == IID_IDSVSTHost)
	return GetInterface( (IDSVSTHost*)this, ppv);

	return CTransInPlaceFilter::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface


HRESULT DSVSTHost::CheckInputType(const CMediaType *pmt)
{
    CheckPointer(pmt,E_POINTER);

	WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->pbFormat;

	if (pmt->majortype != MEDIATYPE_Audio)
	{
		OutputDebugString(L"error MEDIATYPE_Audio\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

    if (pmt->formattype != FORMAT_WaveFormatEx)
	{
		OutputDebugString(L"error FORMAT_WaveFormatEx\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
	}

	/*
    if (pwfx->wFormatTag != WAVE_FORMAT_PCM) 
	{
		OutputDebugString(L"error WAVE_FORMAT_PCM\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }
	*/

    if((pwfx->wFormatTag != WAVE_FORMAT_PCM) && (pwfx->wFormatTag != WAVE_FORMAT_EXTENSIBLE))
	{
		OutputDebugString(L"error WAVE_FORMAT_PCM\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

    if (pwfx->wBitsPerSample!=8 && pwfx->wBitsPerSample!=16) 
	{
		OutputDebugString(L"error BitsPerSample\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

	samplesPerSecond = pwfx->nSamplesPerSec;

	return NOERROR;

} // CheckInputType


/*************************************************************************/

STDMETHODIMP DSVSTHost::FindPin(LPCWSTR Id, IPin **ppPin)
{
   
   CBasePin *pBasePin;

   IPin *pPin;
   
   pPin = NULL;

   int cmp = -1;

   //--------------------------------------------//
   
   if(lstrcmp(Id,L"in0" )==0) cmp = 0; 

   if(lstrcmp(Id,L"out0")==0) cmp = 1; 

   if(cmp==-1) return VFW_E_NOT_FOUND;

   //--------------------------------------------//

   pBasePin = GetPin(cmp);

   pBasePin->QueryInterface(IID_IPin,(void**)&pPin);

   *ppPin = pPin;  

   //----------------------------------------------------------------------//

   return S_OK;

}

HRESULT DSVSTHost::SetMediaType(PIN_DIRECTION direction,const CMediaType *pmt)
{
  CheckPointer(pmt,E_POINTER);

  WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->Format();

  nChannels          = pwfx->nChannels;
  samplesPerSecond   = pwfx->nSamplesPerSec;
  bytesPerSample     = pwfx->wBitsPerSample/8;

  CTransInPlaceFilter::SetMediaType(direction, pmt);

  return NOERROR;

} // SetMediaType


CUnknown * WINAPI DSVSTHost::CreateInstance(LPUNKNOWN punk,HRESULT *phr)
{
  ASSERT(phr);

  DSVSTHost *pNewObject = new DSVSTHost(NAME("DSVSTHost"),punk,phr);

  if(pNewObject == NULL)
  {
	if(phr)
	  *phr = E_OUTOFMEMORY;
  }

  return pNewObject;

}

//DLL-Functions----------------------------------------------//

STDAPI DllRegisterServer()
{
  return AMovieDllRegisterServer2( TRUE );
}

STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2( FALSE );
}

extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HMODULE hModule,
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{ 

  if(dwReason == DLL_PROCESS_ATTACH)
  {

  }//end if DLL_PROCESS_ATTACH

  if(dwReason == DLL_PROCESS_DETACH)
  {  
  
  }
 
  return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);

}






