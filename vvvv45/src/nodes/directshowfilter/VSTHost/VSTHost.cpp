#include "VSTHost.h"


/******************************************************************************************************************************************************/
/******************************************************************************************************************************************************/
/******************************************************************************************************************************************************/

VstIntPtr VSTCALLBACK HostCallback (AEffect *effect,
						  		    VstInt32 opcode,
								    VstInt32 index,
								    VstIntPtr value,
								    void *ptr,
								    float opt)
{
  static Host *host = (Host*)ptr;


  switch(opcode)
  {
    case audioMasterAutomate                     : out(L"audioMasterAutomate\n");                    return host->cbAutomate                  (index,opt);
	case audioMasterVersion                      : out(L"audioMasterVersion\n");                     return host->cbVersion                   ();
	case audioMasterCurrentId                    : out(L"AudioMasteCurrentId\n");                    return host->cbCurrentId                 ();
	case audioMasterIdle                         : out(L"AudioMasterIdle\n");                        return host->cbIdle                      ();
	case audioMasterPinConnected                 : out(L"AudioMasterPinConnected\n");                return host->cbPinConnected              ();
	case audioMasterGetTime                      : out(L"audioMasterGetTime\n");                     return host->cbGetTime                   ();
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

  out(L"host->callback> Received undefined/deprecated opcode\n");

  

  return 1;

}

/******************************************************************************************************************************************************/
/******************************************************************************************************************************************************/
/******************************************************************************************************************************************************/

Loader::Loader()
{
  
}

AEffect* Loader::init(char *filename)
{
  
  module     = NULL;
  pluginMain = NULL;
  effect     = NULL;

  if(filename == NULL)
  {
	out(L"Loader.Init> No valid filename\n");
	return NULL;
  }

  module = LoadLibraryA(filename);

  if(module == NULL)
  {
    out(L"Loader.Init> LoadLibrary failed\n");
	return NULL;
  }

  pluginMain = (PluginMain) GetProcAddress((HMODULE) module,"VSTPluginMain");

  if(pluginMain == NULL)
	pluginMain = (PluginMain) GetProcAddress((HMODULE) module,"main");

  if(pluginMain == NULL)
  {
    out(L"Loader.Init> GetProcAddress of the main-function failed\n");
	return NULL;
  }

  effect = pluginMain(HostCallback);

  if(effect == NULL)
  {
    out(L"Loader.Init> Getting a pointer to the plugin failed\n");
	return NULL;
  }

  if(effect->magic != kEffectMagic) 
  {
	effect = NULL;

    out(L"Loader.Init> Loaded file is no VST-Plugin\n");
	return NULL;
  }  

  out(L"Loader.Init> loaded the plugin\n");
  

  return effect;

}

bool Loader::destroy()
{
  if(module != NULL)
  {
	if( !FreeLibrary((HMODULE) module) )
	 out(L"Loader.~Loader> Unable to free the plugin\n");
	else
     out(L"Loader.~Loader> FreeLibrary\n");
  }

  return true;
}

Loader::~Loader()
{
  destroy();
}

/******************************************************************************************************************************************************/
/******************************************************************************************************************************************************/
/******************************************************************************************************************************************************/

Host::Host()
{
  sampleRate = SAMPLERATE;
  blockSize  = BLOCKSIZE;

  GetCurrentDirectoryA( MAX_PATH, directoryPath);

  HostCallback( 0, -1, 0, 0, this, 0);

  plugin = new Plugin();

  for(int i=0; i<NUMHOSTCANDOS; i++)
  canDo[i] = false;

}

void Host::canDoMidi(unsigned char *can)
{
  plugin->cbCanDo();

  if(plugin->canDo[RECEIVEVSTMIDIEVENT])
	*can = true;
  else
	*can = false;
}

bool Host::destroy()
{
  plugin->destroy();

  loader.destroy();

  return true;
}

bool Host::loadPlugin( char *filename )
{
  if( !plugin->init(loader.init(filename)) ) return false;

  return true;

}

bool Host::getParameterNumber ( int *number )
{
  *number = plugin->getNumParams();

  return true;

}

bool Host::getParameterProperties ( wchar_t paramDisplay[][STRLENGTH], wchar_t paramName[][STRLENGTH], wchar_t paramLabel[][STRLENGTH], double paramValue[] )
{
  for(int i=0;i<plugin->getNumParams();i++)
  {
    plugin->getParamDisplay ( i, paramDisplay[i]);

	plugin->getParamName    ( i, paramName[i]);

	plugin->getParamLabel   ( i, paramLabel[i]);

	paramValue [i] = plugin->getParamValue (i);

  }//end for i


  return true;

}

bool Host::getParameterValue(int index,  double *value)
{
  if( index < 0 || index >= plugin->getNumParams() ) return false;

  *value = plugin->getParamValue(index);

  return true; 

}

bool Host::setParameter ( int index, float value)
{
  if( index < 0 || index >= plugin->getNumParams() ) return false;

  plugin->setParameter( index, value);

  return true;

}

void Host::process( float **in, float **out, int length, int nChannels, int samplesPerSecond )
{

  if( plugin->getNeedIdle() )
  {
    for(int channelIndex = 0; channelIndex < nChannels; channelIndex++)
	for(int i=0; i<length; i++)
     out[channelIndex][i] = in[channelIndex][i];

	return;
  }

  if( samplesPerSecond != sampleRate )
  {
    sampleRate = samplesPerSecond;

	plugin->cbSetSampleRate(sampleRate);
  }

  if( length != blockSize )
  {
    blockSize = length;

	plugin->cbSetBlockSize(blockSize);
  }

  plugin->process( in, out, length, nChannels );

}

//Midi-Messages---------------------------------------------------------------------------//

void Host::midiMsg (unsigned char data0, unsigned char data1, unsigned char data2 )
{
  VstMidiEvent vstMidiEvent;

  vstMidiEvent.type = kVstMidiType;

  vstMidiEvent.midiData[0] = data0;
  vstMidiEvent.midiData[1] = data1; //for all channels
  vstMidiEvent.midiData[2] = data2;


  vstMidiEvent.deltaFrames = 0;

  plugin->midiMsg(vstMidiEvent);
}

void Host::sendMidiNotes(unsigned char note, unsigned char velocity)
{
  if(velocity > 0) 
   midiMsg( NOTEON,  note, velocity);
  else
   midiMsg( NOTEOFF, note, velocity);
  
}

void Host::sendMidiNotesOff()
{
  for(unsigned char i=0; i<NUMMIDINOTES; i++) 
  midiMsg(NOTEON, i, 0);

  midiMsg( CONTROLCHANGE , 0x78, 0);

  midiMsg( CONTROLCHANGE , 0x7B, 0);

}

void Host::sendMidiController( unsigned char controllerID, unsigned int controllerValue )
{
  midiMsg(CONTROLCHANGE,controllerID,controllerValue);
}

void Host::sendProgram(unsigned char programID)
{
  midiMsg(PROGRAMCHANGE, programID, programID);
}

void Host::sendPolyphonic (unsigned char polyphonicNote, unsigned char polyphonicValue)
{
  midiMsg(POLYTOUCH, polyphonicNote, polyphonicValue);
}

void Host::sendMonophonic (unsigned char monophonicValue)
{
  midiMsg(MONOTOUCH, monophonicValue, 0);
}

void Host::sendPitchbend (unsigned char pitchbendValue)
{
  midiMsg(PITCHBEND, pitchbendValue, pitchbendValue);
}

//---------------------------------------------------------------------------------------//

long Host::cbAutomate (int index, float value) 
{ 
  if(index > plugin->getNumParams() -1) return 0;

  char buffer[STRLENGTH];

  //sprintf(buffer,"%f",value);

  outputString(plugin->param[index].name,buffer,1);

  return 1; 

}

long Host::cbVersion                      () { return 1000; }
long Host::cbCurrentId                    () { return plugin->uniqueID; }
long Host::cbIdle () 
{

  return 0; 
}

long Host::cbPinConnected                 () { return 0; }
long Host::cbGetTime                      () { return 0; }
long Host::cbProcessEvents                () { return 0; }
long Host::cbIOChanged                    () { return 0; }
long Host::cbSizeWindow                   () { return 0; }

long Host::cbGetSampleRate () 
{
  plugin->cbSetSampleRate( sampleRate ); 
  return 1;
}

long Host::cbGetBlockSize () 
{ 
  plugin->cbSetBlockSize( blockSize ); 
  return 1;
}

long Host::cbGetInputLatency              () { return 0; }
long Host::cbGetOutputLatency             () { return 0; }
long Host::cbGetCurrentProcessLevel       () { return 0; }
long Host::cbGetAutomationState           () { return 0; }
long Host::cbOfflineStart                 () { return 0; }  
long Host::cbOfflineRead                  () { return 0; }  
long Host::cbOfflineWrite                 () { return 0; }  
long Host::cbOfflineGetCurrentPass        () { return 0; } 
long Host::cbOfflineGetCurrentMetaPass    () { return 0; } 

long Host::cbGetVendorString  (char *str) 
{ 
  strcpy(str,"vvvv-Group");  
  return 1; 
} 

long Host::cbGetProductString (char *str) 
{ 
  strcpy(str,"vvvv-VstHost"); 
  return 1;  
}

long Host::cbGetVendorVersion () 
{ 
  return 1000; 
}

long Host::cbVendorSpecific () { return 0; }

long Host::cbWantMidi () { return 0; }

long Host::cbSetTime () 
{
  //timeInfo.samplePos     = 0;
  //timeInfo.sampleRate    = 0;
  //timeInfo.nanoSeconds   = 0;
  //timeInfo.ppqPos        = 0;
  //timeInfo.tempo         = 0;
  //timeInfo.barStartPos   = 0;
  //timeInfo.cycleStartPos = 0;
  //timeInfo.cycleEndPos   = 0;

  //timeInfo.timeSigNumerator   = 0;
  //timeInfo.timeSigDenominator = 0;
  //timeInfo.smpteOffset        = 0;
  //timeInfo.smpteFrameRate     = 0;
  //timeInfo.samplesToNextClock = 0;
  //timeInfo.flags              = 0;
  
  return 0; 

}

long Host::cbTempoAt () 
{ 
  return 0; 
}


long Host::cbGetNumAutomatableParameters  () { return 0; }
long Host::cbGetParameterQuantization     () { return 0; }

long Host::cbNeedIdle () 
{
  plugin->setNeedIdle(true);

  return 1; 
}

long Host::cbGetPreviousPlug              () { return 0; }
long Host::cbGetNextPlug                  () { return 0; }
long Host::cbWillReplaceOrAccumulate      () { return 0; }
long Host::cbSetOutputSampleRate          () { return 0; }
long Host::cbGetOutputSpeakerArrangement  () { return 0; }
long Host::cbSetIcon                      () { return 0; }
long Host::cbOpenWindow                   () { return 0; }
long Host::cbCloseWindow                  () { return 0; }
long Host::cbEditFile                     () { return 0; }
long Host::cbGetChunkFile                 () { return 0; }
long Host::cbGetInputSpeakerArrangement   () { return 0; }


long Host::cbCanDo (char *str) 
{ 
  outputString("host->CbCanDo> ", str, true);

  char *canDoStr[] = 
  {
   "sendVstEvents",
   "sendVstMidiEvent",
   "sendVstTimeInfo",
   "receiveVstEvents",
   "receiveVstMidiEvent",
   "reportConnectionChanges",
   "acceptIOChanges",
   "sizeWindow",
   "offline",
   "openFileSelector",
   "closeFileSelector",
   "startStopProcess",
   "shellCategory",
   "sendVstMidiEventFlagIsRealtime"

  };

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

  return false;
}

long Host::cbGetLanguage () 
{ 
  return 1;  // 1 = English 
}

long Host::cbGetDirectory (char *str)
{ 
  return (long)directoryPath;
}


long Host::cbUpdateDisplay              () { return 0; }
long Host::cbBeginEdit                  () { return 0; }
long Host::cbEndEdit                    () { return 0; }
long Host::cbOpenFileSelector           () { return 0; }
long Host::cbCloseFileSelector          () { return 0; }


