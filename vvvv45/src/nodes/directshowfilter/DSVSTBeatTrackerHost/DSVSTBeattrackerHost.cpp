#include "DSVSTBeattrackerHost.h" 


PluginLoader::PluginLoader():module(0)
{}

PluginLoader::~PluginLoader()
{}

bool PluginLoader::loadDLL(const char *fileName)
{
  module = LoadLibraryA(fileName);

  return module!=0;
}

PluginEntry PluginLoader::getMainEntry()
{
  return (PluginEntry) GetProcAddress((HMODULE) module,"VSTPluginMain");

}

void PluginLoader::destroy()
{

}


DSVSTBeattrackerHost::DSVSTBeattrackerHost(TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr)
    : CTransInPlaceFilter (tszName, punk,CLSID_DSVSTBeattrackerHost, phr)
{
   samplesPerSecond = 0;        
   bytesPerSample   = 0;        
   channel          = 0; 
   resolution       = 1;
   nFrames          = 0;
   frameSize        = 0;
   frameTime        = 100000;
   delay            = 40;
   phase            = 0;
   period           = 0;
   nSamplesStereo   = 0;
   nSamplesMono     = 0;
   minBPM           = 60;
   maxBPM           = 160;
   inputSource      = UNDEFINED;
   process          = false;
   acousticFeedback = true;

   plugin = NULL;

   if(pluginEntry) 
	plugin = pluginEntry(HostCallback);

   time.start();


}//Constructor

STDMETHODIMP DSVSTBeattrackerHost::getFeedback(int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,int *bpm)
{
  if(inputSource == FILESTREAM) fb.getStream (opcode,beat,phase,fx0,fx1,fx2,fx3,bpm,streamTime());

  if(inputSource == AUDIOIN)    fb.getLive   (opcode,beat,phase,fx0,fx1,fx2,fx3,bpm,streamTime());

  return NOERROR;
}

STDMETHODIMP DSVSTBeattrackerHost::setDelay(int delay)
{
  if((delay >= 0) && (delay < 1000)) this->delay = delay;

  return NOERROR;
}

STDMETHODIMP DSVSTBeattrackerHost::setMinBPM(int minBPM)
{
  if(plugin != NULL)
  ((AudioEffect*)plugin->object)->setNumInputs(minBPM);

  return NOERROR;
}

STDMETHODIMP DSVSTBeattrackerHost::setMaxBPM(int maxBPM)
{
  if(plugin != NULL)
  ((AudioEffect*)plugin->object)->setNumOutputs(maxBPM);

  return NOERROR;
}

STDMETHODIMP DSVSTBeattrackerHost::destroy()
{
  /*
  if(plugin != NULL)
  {
    FreeLibrary(dllHandle);

    OutputDebugString(L"FREELIBRARY");

    plugin = NULL;
  }
  */

  return NOERROR;
}

STDMETHODIMP DSVSTBeattrackerHost::setResolution (int resolution)
{
  if(resolution == 0 || resolution == 1)
  this->resolution = 2;
  
  if(resolution == 2 || resolution == 3)
  this->resolution = 1;

  fb.reset();

  time.start();

  if(plugin != NULL)
  if((AudioEffect*)plugin->object != NULL) 
  {
	((AudioEffect*)plugin->object)->suspend();

	((AudioEffect*)plugin->object)->setBlockSize(this->resolution);
  }

  return NOERROR;
}

//checks if the source is a filestream or a live-source
bool DSVSTBeattrackerHost::checkSource()
{
  inputSource = UNDEFINED;

  IFilterGraph *pGraph = NULL;

  pGraph = this->GetFilterGraph();

  if(pGraph == NULL) 
  {
	OutputDebugString(L"Error GetFilterGraph()");
	return false;
  }

  //------------------------------------------//

  IEnumFilters *pEnum = NULL;
  IBaseFilter  *pFilter; 
  ULONG cFetched;

  HRESULT hr = pGraph->EnumFilters(&pEnum);

  if(FAILED(hr))
  {
	OutputDebugString(L"Error EnumFilters()");
	return false;
  }

  //------------------------------------------//

  inputSource = FILESTREAM;

  //should not only check for the existence of the pin
  while(pEnum->Next(1,&pFilter,&cFetched) == S_OK)
  {
    IPin *pin = NULL;

	pFilter->FindPin(L"Capture",&pin);
    
	if(pin != NULL) inputSource = AUDIOIN;

	pFilter->Release();

  }
 
  if(inputSource == UNDEFINED)
  {
   OutputDebugString(L"Error: InputSource undefined");

   return false;
  }

  return true;

}

bool DSVSTBeattrackerHost::setInput(IMediaSample *pSample)
{
   if(pSample==NULL || plugin==NULL)
   return false;
   
   if(pSample->GetActualDataLength() > MAXDATALENGTH) 
   return false;

   if(bytesPerSample == 0) 
   return false;

   //-------------------------------------------------------------------//

   nSamplesStereo = (pSample->GetActualDataLength() / bytesPerSample);
   nSamplesMono   = (pSample->GetActualDataLength() / bytesPerSample) / channel;

   pSample->GetPointer(&pb);

   samples = new short int [nSamplesStereo];

   in  = new float [nSamplesMono];
   out = new float [nSamplesMono];

   //-------------------------------------------------------------------//

   for(int i=0;i<nSamplesStereo;i++)
   {
     short int *pt = (short int *) pb;

     samples[i] = *pt;

	 pb += bytesPerSample;
   }

   for(int i=0;i<nSamplesMono;i++)
   {	  
	 if(channel == 1)
	 in [i] = out [i] = (float) (samples[i] / 32768.); 

	 if(channel == 2)
	 in [i] = out [i] = (float)( ((samples[i*2] / 32768.) + (samples[i*2+1] / 32768.)) / 2.0 ); 
   }

   //-------------------------------------------------------------------//
   //block = the samples delivered--------------------------------------//
   //frame = the block devided into smaller units-----------------------//
   frameSize = nSamplesMono;
   
   nFrames   = 1;

   for(int i=1;i<nSamplesMono;i++)
   if(nSamplesMono / i <= FRAMESIZE * resolution && fmod((double)nSamplesMono / (double)i,1.0)==0.0)
   {
     frameSize = nSamplesMono / i;
 	 break;
   }

   nFrames = nSamplesMono / frameSize;

   //time per frame in milliseconds-------------------------------------//
   if(inputSource == AUDIOIN) 
   {
    
	 frameTime = ((double)frameSize / (double)samplesPerSecond) * MILLI;
   
	 //wchar_t buffer[512];

	 //swprintf(buffer,L"FrameTime %f NSamplesMono %d",frameTime,nSamplesMono);

	 //OutputDebugString(buffer);
   }


   if(inputSource == FILESTREAM)
    frameTime = 10000000.0 / ((double) samplesPerSecond / (double) frameSize);

  return true;

}

void DSVSTBeattrackerHost::sendInput(IMediaSample *pSample)
{

  double processTime = streamTime(); //???

   //send a single frame to the beattracker------------------------------------------//
   for(int i=0; i<nFrames; i++)
   {
	 int shift = i*frameSize;

	 float *px = in  + shift; 
	 float *py = out + shift;

	 //process the mono-signal and generate the feedback-signal
	 if(plugin != NULL)
	   plugin->processReplacing(plugin, &px, &py, frameSize);

	 if(inputSource == FILESTREAM) 
	  fb.setStreamFrame((FEEDBACKFRAME*)plugin->user,i);

	 if(inputSource == AUDIOIN)   
      fb.setLiveFrame  ((FEEDBACKFRAME*)plugin->user, processTime, frameTime, delay);

	 processTime += frameTime;
	 
   }// end for i 
   
   //Set the block-data-------------------------------------------------------------//
   REFERENCE_TIME startTime,endTime;

   pSample->GetTime(&startTime,&endTime);

   if(inputSource == FILESTREAM)
   fb.setBlock(startTime, endTime, frameTime, nFrames);

}

void DSVSTBeattrackerHost::setMute(IMediaSample *pSample)
{
   unsigned char *ucPtr = (unsigned char *) samples;
   
   pSample->GetPointer(&pb);

   for(int i=0; i < nSamplesStereo * bytesPerSample; i++)
   pb[i] = 0;   

}

void DSVSTBeattrackerHost::setOutput(IMediaSample *pSample)
{
   for(int i=0;i<nSamplesMono;i++)
	{
	  if(channel == 1)
  	  samples[i] = (short)(out[i] * 32768.);

  	  if(channel == 2)
	  {
  	    samples[i*2]   = (short)(out[i] * 32768.);
		samples[i*2+1] = (short)(out[i] * 32768.);
	  }
	}

	//Convert back--------------------------------------//
	unsigned char *ucPtr = (unsigned char *) samples;
   
	pSample->GetPointer(&pb);

	for(int i=0; i < nSamplesStereo * bytesPerSample; i++)
	pb[i] = *ucPtr++;   

}

//use milliseconds
LONGLONG DSVSTBeattrackerHost::streamTime()
{
  CRefTime t;

  StreamTime(t);

  if(inputSource == FILESTREAM) 
  	return t.GetUnits();

  return t.Millisecs();

}

void DSVSTBeattrackerHost::cleanUp()
{
  delete samples;
  delete in;
  delete out;

}

HRESULT DSVSTBeattrackerHost::Transform(IMediaSample *pSample)
{
    if(plugin == NULL) 
    return ERROR;

    if(inputSource == UNDEFINED)
	if(!checkSource()) 
	 return ERROR;
	
    if(!setInput(pSample)) 
	 return ERROR;

    if(time.stopInInterval(TIMEOUT))
	 return NOERROR;

	////processing--------------------------//
	sendInput(pSample);
	  
    //setOutput(pSample);

	if(inputSource == AUDIOIN) setMute(pSample);

   	cleanUp();
	
    return NOERROR;

} // Transform

CUnknown * WINAPI DSVSTBeattrackerHost::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    ASSERT(phr);
    
    DSVSTBeattrackerHost *pNewObject = new DSVSTBeattrackerHost(NAME("DSVSTBeattrackerHost"), punk, phr);
    if (pNewObject == NULL) 
	{
      if (phr)
       *phr = E_OUTOFMEMORY;
    }

	return pNewObject;

  return NULL;

}// CreateInstance


STDMETHODIMP DSVSTBeattrackerHost::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    CheckPointer(ppv,E_POINTER);

	if(riid == IID_IBeattracker) 
    return GetInterface((IBeattracker *) this,ppv);

    return CTransInPlaceFilter::NonDelegatingQueryInterface(riid, ppv);

} // NonDelegatingQueryInterface


HRESULT DSVSTBeattrackerHost::CheckInputType(const CMediaType *pmt)
{
    CheckPointer(pmt,E_POINTER);

	WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->pbFormat;

	if (pmt->majortype != MEDIATYPE_Audio)
	{
		printf("error MEDIATYPE_Audio\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

    if (pmt->formattype != FORMAT_WaveFormatEx)
	{
		//printf("error FORMAT_WaveFormatEx\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
	}

    if (pwfx->wFormatTag != WAVE_FORMAT_PCM) 
	{
		printf("error WAVE_FORMAT_PCM\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

    if (pwfx->wBitsPerSample!=8 && pwfx->wBitsPerSample!=16) 
	{
		printf("error BitsPerSample\n");
        return VFW_E_TYPE_NOT_ACCEPTED;
    }

	samplesPerSecond = pwfx->nSamplesPerSec;

	return NOERROR;

} // CheckInputType


/*************************************************************************/

STDMETHODIMP DSVSTBeattrackerHost::FindPin(LPCWSTR Id, IPin **ppPin)
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
 
   return S_OK;

}

HRESULT DSVSTBeattrackerHost::SetMediaType(PIN_DIRECTION direction,const CMediaType *pmt)
{
 CheckPointer(pmt,E_POINTER);

  WAVEFORMATEX *pwfx = (WAVEFORMATEX *) pmt->Format();

  channel            = pwfx->nChannels;
  samplesPerSecond   = pwfx->nSamplesPerSec;
  bytesPerSample     = pwfx->wBitsPerSample/8;

  CTransInPlaceFilter::SetMediaType(direction, pmt);

  checkSource();

  fb.reset();

  if(plugin->object != NULL) 
  {
    ((AudioEffect*)plugin->object)->suspend();

	((AudioEffect*)plugin->object)->setSampleRate( (float)samplesPerSecond );
  }

  return NOERROR;

} // SetMediaType



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

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{ 

  if(dwReason == DLL_PROCESS_ATTACH)
  {
    PluginLoader pluginLoader;

	const char *fileName = "VSTBeattrackerPlugin.dll";

	pluginLoader.loadDLL(fileName);

	pluginEntry = pluginLoader.getMainEntry();

  }//end if DLL_PROCESS_ATTACH

 
  return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);

}

