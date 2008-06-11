#include "VSTHost.h"


HostList::HostList()
{
  for(int i=0;i<MAXHOSTCOUNT;i++)
  host[i] = NULL;

  count = 0;
}

void HostList::init(VSTHost *newHost)
{
  if(!newHost) return;

  for(int i=0;i<MAXHOSTCOUNT;i++)
  if(host[i]==NULL)
  {
   host[i] = newHost;
   count++;
   return;
  }

}

VSTHost* HostList::retrieve(AEffect *effect)
{
  if(effect == NULL) return NULL;

  for(int i=0;i<MAXHOSTCOUNT;i++)
  if(host[i]!=NULL)
  if(host[i]->plugin.effect == effect)
  return host[i];
  
  return NULL;
}

void HostList::discharge(VSTHost* oldHost)
{
  for(int i=0;i<MAXHOSTCOUNT;i++)
  if(host[i]!=NULL)
  if(host[i] == oldHost)
  {
   host[i] = NULL;
   count--;
   return;
  }

}

/*****************************************************************************************************************************/
/*****************************************************************************************************************************/
/*****************************************************************************************************************************/

//!!!Achtung mehrere Hosts müssen vorhanden sein!!!

//with the hostcallback the plugin is able to send data or requests to the host
VstIntPtr VSTCALLBACK HostCallback (AEffect *effect,
						  		    VstInt32 opcode,
								    VstInt32 index,
								    VstIntPtr value,
								    void *ptr,
								    float opt)
{
  static HostList hostList;

  VSTHost *host = NULL;

  if(opcode == NEWHOST && value == NEWHOST)
  {
   hostList.init((VSTHost*)ptr);
   return 0;
  }

  if(opcode == DISCHARGEHOST && value == DISCHARGEHOST)
  {
   hostList.discharge((VSTHost*)ptr);
   return 0;
  }  

  if(opcode == audioMasterVersion) //is called first by a new effect
   return kVstVersion;

  host = hostList.retrieve((AEffect*)effect);

  //----------------------------------------------------------------------------------------------------//

  VstTimeInfo info;

  info.barStartPos        = 0;
  info.cycleEndPos        = 0;
  info.cycleStartPos      = 0;
  info.flags              = 0;
  info.nanoSeconds        = 0;
  info.ppqPos             = 0;
  info.samplePos          = 0;
  info.sampleRate         = SAMPLERATE;
  info.samplesToNextClock = 0;
  info.smpteFrameRate     = 1;
  info.smpteOffset        = 0;
  info.tempo              = 120;
  info.timeSigDenominator = 4;
  info.timeSigNumerator   = 4;

  //if(opcode == audioMasterGetTime) 
  //	return (long)&info;


  //----------------------------------------------------------------------------------------------------//

  if(host!=NULL)
  switch(opcode)
  {
    case audioMasterAutomate                     : out(L"audioMasterAutomate\n");                    return host->cbAutomate                  (index,opt);
	case audioMasterVersion                      : out(L"audioMasterVersion\n");                     return host->cbVersion                   ();
	case audioMasterCurrentId                    : out(L"AudioMasteCurrentId\n");                    return host->cbCurrentId                 ();
	case audioMasterIdle                         : out(L"AudioMasterIdle\n");                        return host->cbIdle                      ();
	case audioMasterPinConnected                 : out(L"AudioMasterPinConnected\n");                return host->cbPinConnected              ();
	case audioMasterGetTime                      : out(L"audioMasterGetTime\n");                     return host->cbGetTime                   (value);
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
	case audioMasterOpenFileSelector             : out(L"AudioMasterOpenFileSelector\n");            return host->cbOpenFileSelector          ((VstFileSelect*)ptr);
	case audioMasterCloseFileSelector            : out(L"AudioMasterCloseFileSelector\n");           return host->cbCloseFileSelector         ((VstFileSelect*)ptr);
   	
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

  out(L"HOST OR OPCODE UNDEFINED\n");
  
  return 0;

}

/*****************************************************************************************************************************/
/*****************************************************************************************************************************/
/*****************************************************************************************************************************/

VSTHost::VSTHost()
{
  //Subscribe the host in the host-list
  HostCallback( 0, NEWHOST, 0, NEWHOST, this, 0);

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
  HostCallback( 0, DISCHARGEHOST, 0, DISCHARGEHOST, this, 0);

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

  timeInfo.ppqPos = pos * timeInfo.tempo / 60.0;

  double offsetInSeconds = pos - int(pos);

  timeInfo.smpteOffset = (long)(offsetInSeconds * 25.0 * 80.0);

  //---------------------------------------------------------//
  __try
  {
    if(plugin.canReplacing) 
     plugin.effect->processReplacing(plugin.effect, in, out, length);
  }
  __except(GetExceptionCode() == EXCEPTION_INT_DIVIDE_BY_ZERO | EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
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

  try
  {
    effect = pluginMain(HostCallback);
  }
  catch(...)
  {

  }

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

bool VSTHost::getMidiIsInstrument()
{
  if(!plugin.effect) 
	return false;

  if(!plugin.canDo[RECEIVEVSTEVENTS]) 
	return false;  //???

  if(!plugin.canDo[RECEIVEVSTMIDIEVENT]) 
	return false;

  return true;
}

bool VSTHost::sendMidiNote(int count,int note[],int velocity[])
{
  if(!plugin.effect) return false;

  plugin.sendMidiNotes( count, note, velocity);

  return true;
}

bool VSTHost::sendMidiNoteAllOff()
{
  if(!plugin.effect) return false;

  for(int i=0;i<MIDINOTESCOUNT;i++)
  plugin.midiMsg((char)NOTEON,i,0);

  return true;

}

bool VSTHost::sendMidiPolyphonic (unsigned char polyphonicNote, unsigned char polyphonicValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(POLYTOUCH, polyphonicNote, polyphonicValue);

  return true;
}


bool VSTHost::sendMidiController (unsigned char controllerID, unsigned char controllerValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(CONTROLCHANGE, controllerID, controllerValue);

  return true;
}

bool VSTHost::sendMidiProgram (unsigned char programID)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(PROGRAMCHANGE, programID, programID);

  return true;
}

bool VSTHost::sendMidiMonophonic (unsigned char monophonicValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(MONOTOUCH, monophonicValue, 0);  

  return true;
}

bool VSTHost::sendMidiPitchbend (unsigned char pitchbendValue)
{
  if(!plugin.effect) return false;

  plugin.midiMsg(PITCHBEND, pitchbendValue, pitchbendValue);

  return true;
}

bool VSTHost::getInputCount(int *count)
{
  if(!plugin.effect) return false;

  *count = nInputs;

  return true;
}

bool VSTHost::getOutputCount(int *count)
{
  if(!plugin.effect) return false;

  *count = nOutputs;

  return true;
}

bool VSTHost::setBpm(int value)
{
  if(!plugin.effect) return true;

  if(value > 0)
  timeInfo.tempo = value;

  return true;
}

bool VSTHost::getHasWindow()
{
  if(!plugin.effect) return false;

  if(plugin.hasEditor) return true;

  return false;  
}

bool VSTHost::setWindowHandle(HWND hwnd)
{
  if(!plugin.effect) return false;

  plugin.setWindowHandle(hwnd);

  return true;
}

bool VSTHost::getWindowSize(int *width,int *height)
{
  *width  = plugin.width;
  *height = plugin.height;

  return true;
}

bool VSTHost::setWindowIdle()
{
  if(!plugin.effect) return false;

  plugin.effect->dispatcher(plugin.effect,effEditIdle,0,0,0,0);

  return true;
}

bool VSTHost::destroy()
{
  if(!plugin.effect) return false;

  plugin.destroy();

  if(module != NULL) 
   FreeLibrary(module);
 
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
  if(!strcmp(str, "sendVstEvents"))
  {
    canDo[SENDVSTEVENTS] = true;
    return true;
  }

  if(!strcmp(str,"sendVstMidiEvent"))  
  {
    canDo[SENDVSTMIDIEVENT] = true;
    return true;
  }

  if(!strcmp(str,"receiveVstEvents"))  
  {
    canDo[RECEIVEVSTEVENTS] = true;
    return true;
  }

  if(!strcmp(str,"receiveVstMidiEvent"))  
  {
    canDo[RECEIVEVSTMIDIEVENT] = true;
    return true;
  }

  if(!strcmp(str,"openFileSelector"))
  {
    canDo[OPENFILESELECTOR] = true;
	return true;
  }

  if(!strcmp(str,"closeFileSelector"))
  {
    canDo[CLOSEFILESELECTOR] = true;
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

long VSTHost::cbOpenFileSelector  (VstFileSelect *fileSelect)
{
  int result = 0;

  if(fileSelect == NULL)
   return 0;

  if(fileSelect->command == kVstMultipleFilesLoad) 
   return 0;


  if(fileSelect->command == kVstFileLoad || fileSelect->command == kVstFileSave)
  {
	//title--------------------------------------------//

 	wchar_t title [STRLENGTH];

	for(int i=0;i<64;i++)
	{
      title[i] = (wchar_t)fileSelect->title[i];
	  
	  if(fileSelect->title[i] == '\0')
		break;
	}

	title[STRLENGTH-1] = '\0';

	//filterstring--------------------------------------//

	char str[STRLENGTH];

	int strlength = 0;

	strcpy(str,fileSelect->fileTypes->name);

	if(fileSelect->nbFileTypes>0)
	{
	  int count = strlen(fileSelect->fileTypes->name);

	  str[count++] = ' ';
	  str[count++] = '(';

      for(int i=0;i<fileSelect->nbFileTypes;i++)
	  {
  	    str[count++] = '*';
	    str[count++] = '.';

	    for(int k=0;k<strlen(fileSelect->fileTypes[i].dosType);k++)
	    str[count++] = fileSelect->fileTypes[i].dosType[k];

		if(i<fileSelect->nbFileTypes-1)
		{
		 str[count++] = ' ';
		 str[count++] = '|';
		 str[count++] = ' ';
		}
	  }

	  str[count++] = ')';

	  str[count++] = '\0';

      for(int i=0;i<fileSelect->nbFileTypes;i++)
	  {
	   str[count++] = '*';

  	   for(int k=0;k<strlen(fileSelect->fileTypes[i].dosType);k++)
	   str[count++] = fileSelect->fileTypes[i].dosType[k];

	   if(i==fileSelect->nbFileTypes-1)
	   {
		 str[count++] = '\0';
		 str[count++] = '\0';
	   }

	   str[count++] = ';';	
	  }
	 

  	  strlength += count;
	
	}//end if nbfileTypes


	wchar_t wideFilter[STRLENGTH];

	for(int i=0;i<strlength;i++)
	  wideFilter[i] = str[i];
	
	wideFilter[STRLENGTH-1] = '\0';

	//--------------------------------------------------//
	
	static TCHAR filename [MAX_PATH] = TEXT("\0");
    static TCHAR path     [MAX_PATH] = TEXT("."); 
	
	OPENFILENAME openFilename = { sizeof(OPENFILENAME),
	                              plugin.hwnd,
	  							  NULL,
								  wideFilter,
								  NULL,
								  0,
								  1,
								  filename,
								  MAX_PATH,
								  NULL,
								  0,
								  path,
								  title,
								  OFN_FILEMUSTEXIST | OFN_HIDEREADONLY,
								  0,
								  0,
								  TEXT(""),
								  0,
								  NULL,
								  NULL };


	if(fileSelect->command == kVstFileLoad)
	 result = GetOpenFileName(&openFilename);

	if(fileSelect->command == kVstFileSave)
     result = GetSaveFileName(&openFilename);

	//result--------------------------------------------//

	if(!fileSelect->returnPath)
	{
	  fileSelect->reserved = 1;

  	  fileSelect->returnPath = new char[lstrlen(filename)];
	}

	for(int i=0;i<lstrlen(filename);i++)
	fileSelect->returnPath[i] = (char)filename[i];

	fileSelect->returnPath[lstrlen(filename)] = '\0';

	//fileSelect->returnPath[MAX_PATH-1] = '\0';
	
	fileSelect->sizeReturnPath = lstrlen(filename);

	fileSelect->nbReturnPath = 1;

	fileSelect->returnMultiplePaths = NULL;
    
  }//end if kVstFileSave


  return result;

}

long VSTHost::cbCloseFileSelector            (VstFileSelect * fileSelect)
{
  if(fileSelect == NULL)
  return 0;

 if(fileSelect->reserved == 1)
 {
   delete [] fileSelect->returnPath;

   fileSelect->returnPath = 0;
   fileSelect->reserved   = 0;
 }

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




