
#include "DSVSTWrapper.h" 


DSVSTWrapper::DSVSTWrapper(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr)
    : CTransInPlaceFilter (tszName, punk,CLSID_DSVSTWrapper, phr)
{
  initialized    = false;
  nChannels      = 0;
  samplerate     = 0;
  bytesPerSample = 0;
  enable         = true;
  
}

DSVSTWrapper::~DSVSTWrapper()
{
  vstHost.~VSTHost();
}

//IDSVSTWrapper Interface-Definitions----------------------------------------------------------------------------------------------------------//

STDMETHODIMP DSVSTWrapper::load (char *filename)
{
  if(vstHost.load(filename)) 
	return S_OK;
 
  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::setEnable(unsigned char value)
{
  enable = (bool)value;

  return S_OK;
}

STDMETHODIMP DSVSTWrapper::getParameterCount (int *count)
{
  if(vstHost.getParameterCount(count)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getParameterProperties (wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[])
{
  if(vstHost.getParameterProperties(paramDisplay, paramName, paramLabel, paramValue))
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getParameter(int index,double *value)
{
  if(vstHost.getParameter(index, value))
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::setParameter(int index,double  value)
{
  if(vstHost.setParameter(index, value)) 
	return S_OK;

  return S_FALSE;
}
//is the vst-plugin a synth?
STDMETHODIMP DSVSTWrapper::getMidiIsInstrument ()
{
  if(vstHost.getMidiIsInstrument()) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::sendMidiNoteAllOff()
{
  if(vstHost.sendMidiNoteAllOff()) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::sendMidiNote(int count, int note[],int velocity[])
{
  if(vstHost.sendMidiNote(count,note,velocity)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::sendMidiPolyphonic (unsigned char polyphonicNote, unsigned char polyphonicValue)
{
  if(vstHost.sendMidiPolyphonic(polyphonicNote,polyphonicValue)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::sendMidiController (unsigned char controllerID, unsigned char controllerValue)
{
  if(vstHost.sendMidiController(controllerID,controllerValue)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::sendMidiProgram (unsigned char programID)
{
  if(vstHost.sendMidiProgram(programID)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::sendMidiMonophonic (unsigned char monophonicValue)
{
  if(vstHost.sendMidiMonophonic(monophonicValue)) 
	return S_OK;

  return S_FALSE; 
}

STDMETHODIMP DSVSTWrapper::sendMidiPitchbend (unsigned char pitchbendValue)
{
  if(vstHost.sendMidiPitchbend(pitchbendValue)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getInputCount(int *count)
{
  if(vstHost.getInputCount(count)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getOutputCount(int *count)
{
  if(vstHost.getOutputCount(count)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getProgramNames(int *count, wchar_t names[][256])
{
  if(vstHost.getProgramNames(count,names)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::setActualProgram(int count)
{
  if(vstHost.setActualProgram(count)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getActualProgram(int *count)
{
  if(vstHost.getActualProgram(count)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::setBpm(int val)
{
  if(vstHost.setBpm(val)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getHasWindow()
{
  if(vstHost.getHasWindow()) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::setWindowHandle(HWND hwnd)
{
  if(vstHost.setWindowHandle(hwnd)) 
	return S_OK;

  return S_FALSE;
}

STDMETHODIMP DSVSTWrapper::getWindowSize(int *width,int *height)
{
  if(vstHost.getWindowSize(width,height)) 
	return S_OK;

  return S_OK;
}

STDMETHODIMP DSVSTWrapper::setWindowIdle()
{
  if(vstHost.setWindowIdle())
	return S_OK;

  return S_OK;
}

//---------------------------------------------------------------------------------------------------------------------------------------------//

HRESULT DSVSTWrapper::Transform(IMediaSample *pMediaSample)
{  
  if(pMediaSample == NULL || !vstHost.getEffect()) 
   return S_FALSE;

  if(!enable) 
   return S_OK;

  int nInputs  = vstHost.getNumInputs  ();
  int nOutputs = vstHost.getNumOutputs (); 

  //setup of the in- and outputs--------------------------------------------------------------------//
  long nSamples = pMediaSample->GetActualDataLength() / bytesPerSample;
  long nFrames  = nSamples / nChannels;

  if(nInputs >  0) 
   nFrames = nSamples / nInputs;
  else
  if(nOutputs > 0)
   nFrames = nSamples / nOutputs;

  float **in  = new float*[nInputs ];
  float **out = new float*[nOutputs];

  for(int i=0;i<nInputs;i++)
  in [i] = new float[nFrames];
  
  for(int i=0;i<nOutputs;i++)
  out[i] = new float[nFrames];
  

  //read in the audiodata in imediasample-----------------------------------------------------------//
  byte *ptrByte;

  pMediaSample->GetPointer(&ptrByte);


  short int* samples = new short int [nSamples];

  for(int i=0;i<nSamples;i++)
  {
	short int* ptrInt = (short int*) ptrByte;

	samples[i] = *ptrInt;

	ptrByte += bytesPerSample;
  } 

  //Distribute the samples to the input channels of the plugin--------------------------------------//

  int frameCount = 0;

  for(int i=0;i<nFrames;i++)
  for(int c=0;c<nInputs;c++)
  in[c][i] = (float)samples[frameCount++] / WAVESIZE;

  for(int i=0;i<nFrames;i++)
  for(int c=0; c<nOutputs; c++)
  out[c][i] = 0; 

  //send the data to the vst-plugin-----------------------------------------------------------------//
  if(vstHost.process(in,out,nFrames))
  {
    for(int i=0;i<nSamples;i++)
	samples[i] = 0;

	frameCount = 0;

    for(int f=0;f<nFrames;f++)
	for(int c=0;c < nOutputs && frameCount < nSamples;c++)
    samples[frameCount++] = (short)(out[c][f] * WAVESIZE);
	
    unsigned char *ucPtr = (unsigned char *) samples;

    pMediaSample->GetPointer(&ptrByte);

    for(int i=0; i<nSamples * bytesPerSample; i++)
    ptrByte [i] = *ucPtr++;

  }//end if host.process


  //free the memory------------------------------------------------------------------------------//

  delete samples;

  for(int c=0; c<nInputs; c++)
  delete in[c];

  for(int c=0; c<nOutputs; c++)
  delete out[c];

  delete in;
  delete out;

  return NOERROR;

}

HRESULT DSVSTWrapper::CheckInputType(const CMediaType *pmt)
{
    CheckPointer(pmt,E_POINTER);

	WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->pbFormat;

	if (pmt->majortype != MEDIATYPE_Audio)
	{
	    //OutputDebugString(L"ERROR : MEDIATYPE_Audio\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

    if (pmt->formattype != FORMAT_WaveFormatEx)
	{
		//OutputDebugString(L"ERROR : FORMAT_WaveFormatEx\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
	}

    if((pwfx->wFormatTag != WAVE_FORMAT_PCM) && (pwfx->wFormatTag != WAVE_FORMAT_EXTENSIBLE))
	{
		//OutputDebugString(L"ERROR : WAVE_FORMAT_PCM\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

	//not tested for 24-bit samples
    if (pwfx->wBitsPerSample!=8 && pwfx->wBitsPerSample!=16 && pwfx->wBitsPerSample != 24) 
	{
		//OutputDebugString(L"ERROR : BitsPerSample\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

	return NOERROR;

} // CheckInputType


STDMETHODIMP DSVSTWrapper::FindPin(LPCWSTR Id, IPin **ppPin)
{

   CBasePin *pBasePin;

   IPin *pPin;
   
   pPin = NULL;

   int cmp = -1;

    
   if(lstrcmp(Id,L"in0" )==0) cmp = 0; 

   if(lstrcmp(Id,L"out0")==0) cmp = 1; 

   if(cmp==-1) return VFW_E_NOT_FOUND;
 

   pBasePin = GetPin(cmp);

   if(pBasePin == NULL) return VFW_E_NOT_FOUND;


   pBasePin->QueryInterface(IID_IPin,(void**)&pPin);


   if(pPin == NULL) return VFW_E_NOT_FOUND;

   *ppPin = pPin;  


   return S_OK;

}

HRESULT DSVSTWrapper::SetMediaType(PIN_DIRECTION direction,const CMediaType *pmt)
{
  initialized = false;

  if(pmt == NULL) return E_POINTER;

  if((pmt->majortype == MEDIATYPE_Audio) && (pmt->subtype == MEDIASUBTYPE_PCM))
  {
	if(pmt->formattype == FORMAT_WaveFormatEx)
	if(pmt->cbFormat   >= sizeof(WAVEFORMATEX))
    if(pmt->pbFormat   != NULL)
    {
      HRESULT hr = CTransInPlaceFilter::SetMediaType(direction, pmt);

  	  WAVEFORMATEX *ptrFormat = (WAVEFORMATEX *) pmt->Format();

	  nChannels      = ptrFormat->nChannels;
	  samplerate     = ptrFormat->nSamplesPerSec;
	  bytesPerSample = ptrFormat->wBitsPerSample / BITSPERBYTE;
 
	  initialized = true;

	  return NOERROR;
    }

  }
    
  return VFW_E_INVALIDMEDIATYPE;
}  

//Get the IDSVSTWrapper, IBaseFilter... interfaces
STDMETHODIMP DSVSTWrapper::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    CheckPointer(ppv,E_POINTER);

    if(riid == IID_IDSVSTWrapper)
	  return GetInterface( (IDSVSTWrapper*)this, ppv);


    return CTransInPlaceFilter::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface

//Called first
CUnknown * WINAPI DSVSTWrapper::CreateInstance(LPUNKNOWN punk,HRESULT *phr)
{
  ASSERT(phr);

  DSVSTWrapper *pNewObject = new DSVSTWrapper(NAME("DSVSTWrapper"),punk,phr);

  if(pNewObject == NULL)
  {
	if(phr)
	  *phr = E_OUTOFMEMORY;
  }

  return pNewObject;

}


/*************************************************************************/
/*************************************************************************/
/*DLL-Moduldefinitions****************************************************/

STDAPI DllRegisterServer()
{
  return AMovieDllRegisterServer2( TRUE ); //register the filter in the registry
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

  if(dwReason == DLL_PROCESS_ATTACH) {}

  if(dwReason == DLL_PROCESS_DETACH) {}
 
  return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);

}

