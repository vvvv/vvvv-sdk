#include "VSTHost.h"


//with the hostcallback the plugin is able to send data or requests to the host
VstIntPtr VSTCALLBACK HostCallback (AEffect *effect,
						  		    VstInt32 opcode,
								    VstInt32 index,
								    VstIntPtr value,
								    void *ptr,
								    float opt)
{
  static VSTHost *host = (VSTHost*)ptr; //at the first call of this method, this variable is initialized

  if(host!=NULL)
  switch(opcode)
  {
    case audioMasterAutomate                     : out(L"audioMasterAutomate\n");                    return host->cbAutomate                  (index,opt);
	case audioMasterVersion                      : out(L"audioMasterVersion\n");                     return host->cbVersion                   ();
	case audioMasterCurrentId                    : out(L"AudioMasteCurrentId\n");                    return host->cbCurrentId                 ();
	case audioMasterIdle                         : out(L"AudioMasterIdle\n");                        return host->cbIdle                      ();
	case audioMasterPinConnected                 : out(L"AudioMasterPinConnected\n");                return host->cbPinConnected              ();
	case audioMasterGetTime                      : /*out(L"audioMasterGetTime\n"); */                    return host->cbGetTime                   (value);
	case audioMasterProcessEvents                : out(L"audioMasterProcessEvents\n");               return host->cbProcessEvents             ();
    case audioMasterIOChanged                    : out(L"AudioMasterIOChanged\n");                   return host->cbIOChanged                 ();
	case audioMasterSizeWindow                   : out(L"AudioMasterSizeWindow\n");                  return host->cbSizeWindow                ();
	case audioMasterGetSampleRate                : out(L"AudioMasterGetSampleRate\n");               return host->cbGetSampleRate             ();
	case audioMasterGetBlockSize                 : out(L"AudioMasterGetBlockSize\n");                return host->cbGetBlockSize              ();
	case audioMasterGetInputLatency              : out(L"AudioMasterGetInputLatency\n");             return host->cbGetInputLatency           ();
	case audioMasterGetOutputLatency             : out(L"AudioMasterGetOutputLatency\n");            return host->cbGetOutputLatency          ();
	case audioMasterGetCurrentProcessLevel       : out(L"AudioMasterGetCurrentProcessLevel\n");      return host->cbGetCurrentProcessLevel    ();
	case audioMasterGetAutomationState           : out(L"AudioMasterGetAutomationState\n");          return host->cbGetAutomationState        ();
	case audioMasterOfflineStart                 : out(L"AudioMasterOfflineStart\n");                return host->cbOfflineStart              ();
	case audioMasterOfflineRead                  : out(L"AudioMasterOfflineRead\n");                 return host->cbOfflineRead               ();
	case audioMasterOfflineWrite                 : out(L"AudioMasterOfflineWrite\n");                return host->cbOfflineWrite              ();
	case audioMasterOfflineGetCurrentPass        : out(L"AudioMasterGetCurrentPass\n");              return host->cbOfflineGetCurrentPass     ();
	case audioMasterOfflineGetCurrentMetaPass    : out(L"AudioMasterOfflineGetCurrentMetaPass\n");   return host->cbOfflineGetCurrentMetaPass ();
	case audioMasterGetVendorString              : out(L"AudioMasterGetVendorString\n");             return host->cbGetVendorString           ( (char*) ptr );
	case audioMasterGetProductString             : out(L"AudioMasterGetProductString\n");            return host->cbGetProductString          ( (char*) ptr );
	case audioMasterGetVendorVersion             : out(L"AudioMasterGetVendorVersion\n");            return host->cbGetVendorVersion          ();
	case audioMasterVendorSpecific               : out(L"AudioMasterVendorSpecific\n");              return host->cbVendorSpecific            ();
	case audioMasterCanDo                        : out(L"AudioMasterCanDo\n");                       return host->cbCanDo                     ( (char*) ptr );
	case audioMasterGetLanguage                  : out(L"AudioMasterGetLanguage\n");                 return host->cbGetLanguage               ();
	case audioMasterGetDirectory                 : out(L"AudioMasterGetDirectory\n");                return host->cbGetDirectory              ( (char*) ptr);
	case audioMasterUpdateDisplay                : out(L"AudioMasterUpdateDisplay\n");               return host->cbUpdateDisplay             ();
	case audioMasterBeginEdit                    : out(L"AudioMasterBeginEdit\n");                   return host->cbBeginEdit                 ();
	case audioMasterEndEdit                      : out(L"AudioMasterEndEdit\n");                     return host->cbEndEdit                   ();
	case audioMasterOpenFileSelector             : out(L"AudioMasterOpenFileSelector\n");            return host->cbOpenFileSelector          ();
	case audioMasterCloseFileSelector            : out(L"AudioMasterCloseFileSelector\n");           return host->cbCloseFileSelector         ();
   	
	//deprecated---------------------------------------------------------------------------------------------------------------------------------//
	case audioMasterWantMidi                     : out(L"AudioMasterWantsMidi\n");                   return host->cbWantMidi                     ();
    case audioMasterSetTime                      : out(L"AudioMasterSetTime\n");                     return host->cbSetTime                      ();
    case audioMasterTempoAt                      : out(L"AudioMasterTempoAt\n");                     return host->cbTempoAt                      ();
    case audioMasterGetNumAutomatableParameters  : out(L"AudioMasterGetNumAutomatableParameters\n"); return host->cbGetNumAutomatableParameters  ();
	case audioMasterGetParameterQuantization     : out(L"AudioMasterGetParameterQuantization\n");    return host->cbGetParameterQuantization     ();
    case audioMasterNeedIdle                     : out(L"AudioMasterNeedIdle\n");                    return host->cbNeedIdle                     ();
    case audioMasterGetPreviousPlug              : out(L"AudioMasterGetPreviousPlug\n");             return host->cbGetPreviousPlug              ();
    case audioMasterGetNextPlug                  : out(L"AudioMasterGetNextPlug\n");                 return host->cbGetNextPlug                  ();
    case audioMasterWillReplaceOrAccumulate      : out(L"AudioMasterWillReplaceOrAccumulate\n");     return host->cbWillReplaceOrAccumulate      ();
    case audioMasterSetOutputSampleRate          : out(L"AudioMasterSetOutputSampleRate\n");         return host->cbSetOutputSampleRate          ();
    case audioMasterGetOutputSpeakerArrangement  : out(L"AudioMasterGetOutputSpeakerArrangement\n"); return host->cbGetOutputSpeakerArrangement  ();
    case audioMasterSetIcon                      : out(L"AudioMasterSetIcon\n");                     return host->cbSetIcon                      ();
    case audioMasterOpenWindow                   : out(L"AudioMasterOpenWindow\n");                  return host->cbOpenWindow                   ();
    case audioMasterCloseWindow                  : out(L"AudioMasterCloseWindow\n");                 return host->cbCloseWindow                  ();
    case audioMasterEditFile                     : out(L"AudioMasterEditFile\n");                    return host->cbEditFile                     ();
    case audioMasterGetChunkFile                 : out(L"AudioMasterGetChunkFile\n");                return host->cbGetChunkFile                 ();
    case audioMasterGetInputSpeakerArrangement   : out(L"AudioMasterGetInputSpeakerArrangement\n");  return host->cbGetInputSpeakerArrangement   ();

  }

  if(opcode!=-1) out(L"HOSTCALLBACK : UNDEFINED OPCODE\n");

  return 0;//1??

}

/*****************************************************************************************************************************/
/*****************************************************************************************************************************/
/*****************************************************************************************************************************/

VSTHost::VSTHost()
{
  //set the host pointer
  HostCallback( 0, -1, 0, 0, this, 0);

  for(int i=0;i<HOSTCANDOCOUNT;i++)
  canDo[i] = false;

  GetCurrentDirectoryA( MAX_PATH, directoryPath);

  blockSize  = BLOCKSIZE;
  sampleRate = SAMPLERATE; 
  nInputs    = STEREO;
  nOutputs   = STEREO;
  module     = NULL;

  timeInfo.barStartPos        = 0;
  timeInfo.cycleEndPos        = 0;
  timeInfo.cycleStartPos      = 0;
  timeInfo.flags              = 0;
  timeInfo.nanoSeconds        = 0;
  timeInfo.ppqPos             = 0;
  timeInfo.samplePos          = 0;
  timeInfo.sampleRate         = SAMPLERATE;
  timeInfo.samplesToNextClock = 0;
  timeInfo.smpteFrameRate     = 1;
  timeInfo.smpteOffset        = 0;
  timeInfo.tempo              = 120;
  timeInfo.timeSigDenominator = 4;
  timeInfo.timeSigNumerator   = 4;

}

VSTHost::~VSTHost()
{

}

bool VSTHost::process (float **in, float **out,int length) //length = number of frames
{
  if(!plugin.effect) 
	return false;

  if((plugin.effect->numInputs != nInputs) || (plugin.effect->numOutputs != nOutputs)) 
  {
	nInputs  = plugin.effect->numInputs;
	nOutputs = plugin.effect->numOutputs;
	return false;
  }

  if(length > blockSize) 
  {
    blockSize = length;
    plugin.cbSetBlockSize(length);
  }

  //---------------------------------------------------------//

  timeInfo.nanoSeconds = (double)timeGetTime() * 1000000.0;

  double pos = timeInfo.samplePos / timeInfo.sampleRate;

  double offsetInSeconds = pos - int(pos);

  timeInfo.smpteOffset = (long)(offsetInSeconds * 25.0 * 80.0);

  //---------------------------------------------------------//

  __try
  {
    if(plugin.canReplacing) 
     plugin.effect->processReplacing(plugin.effect, in, out, length);
  }
  __except(GetExceptionCode() == EXCEPTION_INT_DIVIDE_BY_ZERO ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
  {

  }
 
  timeInfo.samplePos  += (float)length;

  return true;
}

//Interface-Definitions----------------------------------------------------------------------------------------------------//

bool VSTHost::load(char *filename)
{
  PluginMain  pluginMain;
  AEffect    *effect;
   
  module = LoadLibraryA(filename);

  if(module == NULL) return false;

  pluginMain = (PluginMain) GetProcAddress((HMODULE) module,"VSTPluginMain");

  if(pluginMain == NULL)
  pluginMain = (PluginMain) GetProcAddress((HMODULE) module,"main");

  if(pluginMain == NULL) return false;

  
  effect = pluginMain(HostCallback);

  if(effect == NULL) return false;

  if(effect->magic != kEffectMagic)	return false;

  plugin.initialize(effect);

  nInputs  = plugin.effect->numInputs;
  nOutputs = plugin.effect->numOutputs;


  return true;

}

bool VSTHost::getProgramNames (int *count, wchar_t names[][256])
{
  if(!plugin.effect) return false;

  *count = plugin.numPrograms;
  
  for(int i=0;i<plugin.numPrograms;i++)
  {
	for(int k=0;k<STRLENGTH;k++)
	{
	  names[i][k] =  plugin.program[i].name[k];  //plugin.param[h].display[k];
	  if(plugin.program[i].name[k] == '\0') break;
	}
  }

  return true;
}

bool VSTHost::getActualProgram(int *count)
{
  if(!plugin.effect) return false;

  *count = plugin.actualProgram;

  return true;
}

bool VSTHost::setActualProgram(int count)
{
  if(!plugin.effect) return false;

  if(count < 0 || count >= plugin.numPrograms) return false;

  plugin.setProgram(count);

  return true;
}

bool VSTHost::getParameterCount(int *count)
{
  if(!plugin.effect) 
  {
	*count = 0;

	return false;
  }

  *count = plugin.numParams;

  return true;  
}

bool VSTHost::getParameterProperties( wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[])
{
  if(!plugin.effect) return false;


  for(int h=0; h<plugin.numParams; h++)
  {
	for(int k=0;k<STRLENGTH;k++)
	{
	  paramDisplay[h][k] = plugin.param[h].display[k];
	  if(plugin.param[h].display[k] == '\0') break;
	}

	for(int k=0;k<STRLENGTH;k++)
	{
	  paramName[h][k] = plugin.param[h].name[k];
	  if(plugin.param[h].name[k] == '\0') break;
	}

	for(int k=0;k<STRLENGTH;k++)
	{
	  paramLabel[h][k] = plugin.param[h].label[k];
	  if(plugin.param[h].label[k] == '\0') break;
	}

	paramValue[h] = plugin.param[h].value;
  }

  return true;
}

bool VSTHost::getParameter(int index, double *value)
{
  if(!plugin.effect)  return false;

  if(index < 0 || index >= plugin.numParams) return false;
  
  *value = plugin.getParameter(index);

  return true;
}

bool VSTHost::setParameter(int index, double  value)
{
  if(!plugin.effect)  return false;

  if(index < 0 || index >= plugin.numParams) return false;

  plugin.setParameter(index,value);

  return true;
}

bool VSTHost::isInstrument()
{
  if(!plugin.effect) return false;

  if(!plugin.canDo[RECEIVEVSTMIDIEVENT]) return false;

  return true;
}

bool VSTHost::sendMidiNotes(int count,int note[],int velocity[])
{
  if(!plugin.effect) return false;

  plugin.sendMidiNotes( count, note, velocity);

  return true;
}

bool VSTHost::sendMidiNotesOff()
{
  if(!plugin.effect) return false;

  for(int i=0;i<MIDINOTESCOUNT;i++)
  plugin.midiMsg((char)NOTEON,i,0);

  return true;

}

bool VSTHost::sendPolyphonic (unsigned char polyphonicNote, unsigned char polyphonicValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(POLYTOUCH, polyphonicNote, polyphonicValue);

  return true;
}


bool VSTHost::sendController (unsigned char controllerID, unsigned char controllerValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(CONTROLCHANGE, controllerID, controllerValue);

  return true;
}

bool VSTHost::sendProgram (unsigned char programID)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(PROGRAMCHANGE, programID, programID);

  return true;
}

bool VSTHost::sendMonophonic (unsigned char monophonicValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(MONOTOUCH, monophonicValue, 0);  

  return true;
}

bool VSTHost::sendPitchbend (unsigned char pitchbendValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(PITCHBEND, pitchbendValue, pitchbendValue);

  return true;
}

bool VSTHost::getInputsCount(int *count)
{
  if(!plugin.effect) return false;

  *count = nInputs;

  return true;
}

bool VSTHost::getOutputsCount(int *count)
{
  if(!plugin.effect) return false;

  *count = nOutputs;

  return true;
}

bool VSTHost::destroy()
{
  if(!plugin.effect) return false;

  plugin.destroy();

  if(module != NULL) FreeLibrary(module);
 
  return true;
}

//Callback-Functions---------------------------------------------------------------------------------------------------------//

long VSTHost::cbAutomate                     (int index, float value)
{
 return 0;
}

long VSTHost::cbVersion                      ()
{
 return kVstVersion;
}

long VSTHost::cbCurrentId                    ()
{
 return 0;
}

long VSTHost::cbIdle                         ()
{
 //???
 plugin.needIdle = true;

 return 0;
}

long VSTHost::cbPinConnected                 ()
{
 return 0;
}

long VSTHost::cbGetTime                      (VstIntPtr value)
{
 return (long)&timeInfo;
}

long VSTHost::cbProcessEvents                ()
{
 return 0;
}

long VSTHost::cbIOChanged                    ()
{
 return 0;
}

long VSTHost::cbSizeWindow                   ()
{
 return 0;
}

long VSTHost::cbGetSampleRate                ()
{
 return sampleRate;
}

long VSTHost::cbGetBlockSize                 ()
{
 return blockSize;
}

long VSTHost::cbGetInputLatency              ()
{
 return 0;
}

long VSTHost::cbGetOutputLatency             ()
{
 return 0;
}

long VSTHost::cbGetCurrentProcessLevel       ()
{
 return 0;
}

long VSTHost::cbGetAutomationState           ()
{
 return 0;
}

long VSTHost::cbOfflineStart                 ()
{
 return 0;
}
  
long VSTHost::cbOfflineRead                  ()
{
 return 0;
}
  
long VSTHost::cbOfflineWrite                 ()
{
 return 0;
}
  
long VSTHost::cbOfflineGetCurrentPass        ()
{
 return 0;
}
 
long VSTHost::cbOfflineGetCurrentMetaPass    ()
{
 return 0;
}

long VSTHost::cbGetVendorString              (char *str)
{
 return 0;
}
 
long VSTHost::cbGetProductString             (char *str)
{
 return 0;
}

long VSTHost::cbGetVendorVersion             ()
{
 return 0;
}

long VSTHost::cbVendorSpecific               ()
{
 return 0;
}

//it seems like nobody wants to know what the host can do
long VSTHost::cbCanDo(char *str)
{
  if(strcmp(str, "sendVstEvents"))
  {
    canDo[SENDVSTEVENTS] = true;
    return true;
  }

  if(strcmp(str,"sendVstMidiEvent"))  
  {
    canDo[SENDVSTMIDIEVENT] = true;
    return true;
  }

  if(strcmp(str,"receiveVstEvents"))  
  {
    canDo[RECEIVEVSTEVENTS] = true;
    return true;
  }

  if(strcmp(str,"receiveVstMidiEvent"))  
  {
    canDo[RECEIVEVSTMIDIEVENT] = true;
    return true;
  }

  return 0;
}

long VSTHost::cbGetLanguage                  ()
{
 return 0;
}

long VSTHost::cbGetDirectory                 (char *str)
{
 return (long)directoryPath;
}

long VSTHost::cbUpdateDisplay                ()
{
 return 0;
}

long VSTHost::cbBeginEdit                    ()
{
 plugin.needIdle = true;

 return 0;
}

long VSTHost::cbEndEdit                      ()
{
 plugin.needIdle = false;

 return 0;
}

long VSTHost::cbOpenFileSelector             ()
{
 return 0;
}

long VSTHost::cbCloseFileSelector            ()
{
 return 0;
}

//deprecated
long VSTHost::cbWantMidi                     ()
{
 plugin.isSynth = true;

 return 0;
}

long VSTHost::cbSetTime                      ()
{
 return 0;
}

long VSTHost::cbTempoAt                      ()
{
 return 0;
}

long VSTHost::cbGetNumAutomatableParameters  ()
{
 return 0;
}

long VSTHost::cbGetParameterQuantization     (){
 return 0;
}

long VSTHost::cbNeedIdle                     ()
{
 ///???
// plugin.needIdle = true;

 return 0;
}

long VSTHost::cbGetPreviousPlug              ()
{
 return 0;
}

long VSTHost::cbGetNextPlug                  ()
{
 return 0;
}

long VSTHost::cbWillReplaceOrAccumulate      ()
{
 return 0;
}

long VSTHost::cbSetOutputSampleRate          ()
{
 return 0;
}

long VSTHost::cbGetOutputSpeakerArrangement  ()
{
 return 0;
}

long VSTHost::cbSetIcon                      ()
{
 return 0;
}

long VSTHost::cbOpenWindow                   ()
{
 return 0;
}

long VSTHost::cbCloseWindow                  ()
{
 return 0;
}

long VSTHost::cbEditFile                     ()
{
 return 0;
}

long VSTHost::cbGetChunkFile                 ()
{
 return 0;
}

long VSTHost::cbGetInputSpeakerArrangement   ()
{
 return 0;
}




