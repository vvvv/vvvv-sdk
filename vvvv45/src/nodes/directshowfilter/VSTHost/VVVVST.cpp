#include "VVVVST.h" 


VVVVST::VVVVST(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr)
    : CTransInPlaceFilter (tszName, punk,CLSID_VVVVST, phr)
{
  initialized    = false;
  nChannels      = 0;
  samplerate     = 0;
  bytesPerSample = 0;
  enable         = true;

}

VVVVST::~VVVVST()
{}

//Interface--------------------------------------------------------------------------------------------------------------------------------------//

STDMETHODIMP VVVVST::load (char *filename)
{
  if(host.load(filename)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::setEnable(unsigned char value)
{
  enable = value;

  return S_OK;
}

STDMETHODIMP VVVVST::getParameterCount (int *count)
{
  if(host.getParameterCount(count)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::getParameterProperties (wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[])
{
  if(host.getParameterProperties(paramDisplay, paramName, paramLabel, paramValue)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::getParameter(int index,double *value)
{
  if(host.getParameter(index, value)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::setParameter(int index,double  value)
{
  if(host.setParameter(index, value)) return S_OK;

  return ERROR;

}

STDMETHODIMP VVVVST::isInstrument (unsigned char *value)
{
  if(host.isInstrument()) 
   *value = true;
  else
   *value = false;

  return ERROR;
}

STDMETHODIMP VVVVST::sendMidiNotesOff()
{
  if(host.sendMidiNotesOff()) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::sendMidiNotes(int count, int note[],int velocity[])
{
  if(host.sendMidiNotes(count,note,velocity)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::sendPolyphonic (unsigned char polyphonicNote, unsigned char polyphonicValue)
{
  if(host.sendPolyphonic(polyphonicNote,polyphonicValue)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::sendController (unsigned char controllerID, unsigned char controllerValue)
{
  if(host.sendController(controllerID,controllerValue)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::sendProgram (unsigned char programID)
{
  if(host.sendProgram(programID)) return S_OK;

  return ERROR;
}

STDMETHODIMP VVVVST::sendMonophonic (unsigned char monophonicValue)
{
  if(host.sendMonophonic(monophonicValue)) return S_OK;

  return ERROR; 
}

STDMETHODIMP VVVVST::sendPitchbend (unsigned char pitchbendValue)
{
  if(host.sendPitchbend(pitchbendValue)) return S_OK;

  return ERROR;
}

//Derived methods from CTransInPlaceFilter------------------------------------------------------------------------------------------------------//

HRESULT VVVVST::Transform(IMediaSample *pMediaSample)
{
  if(pMediaSample == NULL || !initialized) 
  return ERROR;

  if(!enable) 
  return S_OK;

  //setup two inputs for the vst-plugin-------------------------------------------------------------//
  long nSamples = pMediaSample->GetActualDataLength() / bytesPerSample;
  long nFrames  = nSamples / nChannels;

  float **in  = new float*[STEREO];
  float **out = new float*[STEREO];

  in  [0]  = new float[nFrames];
  in  [1]  = new float[nFrames];
  out [0]  = new float[nFrames];
  out [1]  = new float[nFrames]; 


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

  //Distribute the samples to the two input channels of the filter----------------------------------//
  if(nChannels == STEREO)
  {
    for(int i=0;i<nFrames;i++)
	{
	  int shift = i * STEREO;

	  in [0][i] = ((float)samples[shift    ] / WAVESIZE);
	  in [1][i] = ((float)samples[shift + 1] / WAVESIZE);
	  out[0][i] = 0;
	  out[1][i] = 0;
	}

  }//end if nChannels

  if(nChannels!=STEREO) 
  {
    short int* frames = new short int[nFrames];

    for(int f=0; f<nFrames; f++)
    for(int c=0; c<nChannels; c++)
    frames[f] = samples[f*nChannels+c] / nChannels; //build a mono channel

    for(int i=0; i<nFrames; i++)
    {
     in [0][i] = in [1][i] = ((float)frames[i] / WAVESIZE); //the two inputs are getting the same data
     out[0][i] = out[1][i] = 0;
    }

	delete frames;
  
  }//end if nChannels


  //send the data to the vst-plugin-----------------------------------------------------------------//
  if(host.process( in, out, nFrames))
  {
    //write the data back to the output
    if(nChannels==STEREO)
    {
     for(int k=0; k<STEREO; k++) 
     for(int i=0; i<nFrames; i++)
     samples[ i*STEREO + k ] = (short) (out[k][i] * WAVESIZE);
    }

    if(nChannels!=STEREO)
    {
	 float *frames = new float[nFrames];

	 for(int f=0;f<nFrames;f++)
	 frames[f] = ((out[0][f] + out[1][f]) / STEREO) * WAVESIZE;

     for(int f=0;f<nFrames;f++)
	 for(int c=0;c<nChannels;c++)
     samples[f*nChannels+c] = frames[f];

	 delete frames;
    }

    unsigned char *ucPtr = (unsigned char *) samples;

    pMediaSample->GetPointer(&ptrByte);

    for(int i=0; i<nSamples * bytesPerSample; i++)
    ptrByte [i] = *ucPtr++;

  }//end if host.process


  //free the memory------------------------------------------------------------------------------//
  delete in[0];
  delete in[1];

  delete out[0];
  delete out[1];

  delete in;
  delete out;

  return NOERROR;

}

HRESULT VVVVST::CheckInputType(const CMediaType *pmt)
{
    CheckPointer(pmt,E_POINTER);

	WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->pbFormat;

	if (pmt->majortype != MEDIATYPE_Audio)
	{
	    OutputDebugString(L"ERROR : MEDIATYPE_Audio\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

    if (pmt->formattype != FORMAT_WaveFormatEx)
	{
		OutputDebugString(L"ERROR : FORMAT_WaveFormatEx\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
	}

    if((pwfx->wFormatTag != WAVE_FORMAT_PCM) && (pwfx->wFormatTag != WAVE_FORMAT_EXTENSIBLE))
	{
		OutputDebugString(L"ERROR : WAVE_FORMAT_PCM\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

    if (pwfx->wBitsPerSample!=8 && pwfx->wBitsPerSample!=16 && pwfx->wBitsPerSample != 24) 
	{
		OutputDebugString(L"ERROR : BitsPerSample\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

	return NOERROR;

} // CheckInputType


STDMETHODIMP VVVVST::FindPin(LPCWSTR Id, IPin **ppPin)
{

   CBasePin *pBasePin;

   IPin *pPin;
   
   pPin = NULL;

   int cmp = -1;

    
   if(lstrcmp(Id,L"in0" )==0) cmp = 0; //looking for the input pin (psudPins AMOVIESETUP_PIN)

   if(lstrcmp(Id,L"out0")==0) cmp = 1; //looking for the output pin 

   if(cmp==-1) return VFW_E_NOT_FOUND;
 

   pBasePin = GetPin(cmp);

   if(pBasePin == NULL) return VFW_E_NOT_FOUND;


   pBasePin->QueryInterface(IID_IPin,(void**)&pPin);


   if(pPin == NULL) return VFW_E_NOT_FOUND;

   *ppPin = pPin;  


   return S_OK;

}

HRESULT VVVVST::SetMediaType(PIN_DIRECTION direction,const CMediaType *pmt)
{
  initialized = false; //the filter only does transforming if the correct audioformat is set

  if(pmt == NULL) return E_POINTER;

  if((pmt->majortype == MEDIATYPE_Audio) && (pmt->subtype == MEDIASUBTYPE_PCM))
  {

	if(pmt->formattype == FORMAT_WaveFormatEx)
	if(pmt->cbFormat   >= sizeof(WAVEFORMATEX))
    if(pmt->pbFormat   != NULL)
    {
	  WAVEFORMATEX *ptrFormat = (WAVEFORMATEX *) pmt->Format();

	  nChannels      = ptrFormat->nChannels;
	  samplerate     = ptrFormat->nSamplesPerSec;
	  bytesPerSample = ptrFormat->wBitsPerSample / BITSPERBYTE;
      
	  //set conditions
	  if( (nChannels > 0) &&
	      (samplerate == 44100 || samplerate == 48000) &&
	      (bytesPerSample == 1 || bytesPerSample == 2)) // || bytesPerSample == 3) ) 24-bit?
	  initialized = true;
	  
	  return S_OK;
    }

  }
    
  return VFW_E_INVALIDMEDIATYPE;

}  

//Get the IVVVVST, IBaseFilter... interfaces
STDMETHODIMP VVVVST::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    CheckPointer(ppv,E_POINTER);

	if(riid == IID_IVVVVST)
	return GetInterface( (IVVVVST*)this, ppv);

	return CTransInPlaceFilter::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface


//Called first
CUnknown * WINAPI VVVVST::CreateInstance(LPUNKNOWN punk,HRESULT *phr)
{
  ASSERT(phr);

  VVVVST *pNewObject = new VVVVST(NAME("VVVVST"),punk,phr);

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

  if(dwReason == DLL_PROCESS_ATTACH) {}

  if(dwReason == DLL_PROCESS_DETACH) {}
 
  return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);

}






