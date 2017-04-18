
#include "VSTHost.h"

HostList::HostList()
{
  for(int i=0;i<MAXHOSTCOUNT;i++)
   vstHost[i] = NULL;

  count = 0;
}

//Every new host has to be subscribed to the list
void HostList::init(VSTHost *newHost)
{
  if(!newHost) return;

  for(int i=0;i<MAXHOSTCOUNT;i++)
  if(vstHost[i]==NULL)
  {
   vstHost[i] = newHost;
   count++;
   return;
  }

}

//Retrieve the host the msg has been send to
VSTHost* HostList::retrieve(AEffect *effect)
{
  if(effect == NULL) return NULL;

  for(int i=0;i<MAXHOSTCOUNT;i++)
  if(vstHost[i]!=NULL)
  if(vstHost[i]->getEffect() == effect) //the effect-address as an id
    return vstHost[i];
  
  return NULL;
}

//Unsubscribe a host
void HostList::discharge(VSTHost* oldHost)
{
  for(int i=0;i<MAXHOSTCOUNT;i++)
  if(vstHost[i]!=NULL)
  if(vstHost[i] == oldHost)
  {
   vstHost[i] = NULL;
   count--;
   return;
  }

}

//*********************************************************************************************************************//
//*********************************************************************************************************************//
//*********************************************************************************************************************//

VstIntPtr VSTCALLBACK HostCallback ( AEffect *effect,
  							         VstInt32 opcode,
									 VstInt32 index,
									 VstIntPtr value,
									 void *ptr,
									 float opt )
{
  static HostList hostList;

  VSTHost *vstHost = NULL;

  if(opcode == NEWHOST && value == NEWHOST) //the first call of this function is done by the new host 
  {                                         //to put itself into the list
   hostList.init((VSTHost*)ptr);
   return 0;
  }

  if(opcode == DISCHARGEHOST && value == DISCHARGEHOST) //this happens when the plugin is deleted in the vvvv-patch
  {
   hostList.discharge((VSTHost*)ptr);
   return 0;
  }  

  if(opcode == audioMasterVersion) //is called first by a new effect
   return kVstVersion;

  vstHost = hostList.retrieve((AEffect*)effect); //which host belongs to the plugin?

  //-------------------------------------------------------------------------------------------------------------//

  if(vstHost)
  switch(opcode)
  {
    case audioMasterAutomate                     : OutputDebugString(L"audioMasterAutomate\n");                    break;
	
	case audioMasterVersion                      : return vstHost->version(); 
	
	case audioMasterCurrentId                    : OutputDebugString(L"AudioMasteCurrentId\n");                    break;
	case audioMasterIdle                         : OutputDebugString(L"AudioMasterIdle\n");                        break;
	case audioMasterPinConnected                 : OutputDebugString(L"AudioMasterPinConnected\n");                break;
	
	case audioMasterGetTime                      : return vstHost->getTime();
	
	case audioMasterProcessEvents                : return vstHost->processEvents();

    case audioMasterIOChanged                    : OutputDebugString(L"AudioMasterIOChanged\n");                   break;
	case audioMasterSizeWindow                   : OutputDebugString(L"AudioMasterSizeWindow\n");                  break;
	
	case audioMasterGetSampleRate                : return vstHost->getSampleRate ();

	case audioMasterGetBlockSize                 : return vstHost->getBlockSize  ();

	case audioMasterGetInputLatency              : OutputDebugString(L"AudioMasterGetInputLatency\n");             break;
	case audioMasterGetOutputLatency             : OutputDebugString(L"AudioMasterGetOutputLatency\n");            break;
	case audioMasterGetCurrentProcessLevel       : OutputDebugString(L"AudioMasterGetCurrentProcessLevel\n");      break;
	case audioMasterGetAutomationState           : OutputDebugString(L"AudioMasterGetAutomationState\n");          break;
	case audioMasterOfflineStart                 : OutputDebugString(L"AudioMasterOfflineStart\n");                break;
	case audioMasterOfflineRead                  : OutputDebugString(L"AudioMasterOfflineRead\n");                 break;
	case audioMasterOfflineWrite                 : OutputDebugString(L"AudioMasterOfflineWrite\n");                break;
	case audioMasterOfflineGetCurrentPass        : OutputDebugString(L"AudioMasterGetCurrentPass\n");              break;
	case audioMasterOfflineGetCurrentMetaPass    : OutputDebugString(L"AudioMasterOfflineGetCurrentMetaPass\n");   break;
	
	case audioMasterGetVendorString              : return vstHost->getVendorString((char*)ptr);
	case audioMasterGetProductString             : return vstHost->getProductString((char*)ptr);
	case audioMasterGetVendorVersion             : return vstHost->getVendorVersion();
	case audioMasterVendorSpecific               : return vstHost->getVendorSpecific();
	
	case audioMasterCanDo                        : return vstHost->canDo((char*)ptr);
	
	case audioMasterGetLanguage                  : OutputDebugString(L"AudioMasterGetLanguage\n");                 break;
	
	case audioMasterGetDirectory                 : return vstHost->getDirectory();

	case audioMasterUpdateDisplay                : OutputDebugString(L"AudioMasterUpdateDisplay\n");               break;
	case audioMasterBeginEdit                    : OutputDebugString(L"AudioMasterBeginEdit\n");                   break;
	case audioMasterEndEdit                      : OutputDebugString(L"AudioMasterEndEdit\n");                     break;

	case audioMasterOpenFileSelector             : return vstHost->openFileSelector  ((VstFileSelect*)ptr);

	case audioMasterCloseFileSelector            : return vstHost->closeFileSelector ((VstFileSelect*)ptr);

	//deprecated opcodes  
   	case audioMasterWantMidi                     : return vstHost->wantMidi();

    case audioMasterSetTime                      : OutputDebugString(L"AudioMasterSetTime\n");                     break;
    case audioMasterTempoAt                      : OutputDebugString(L"AudioMasterTempoAt\n");                     break;
    case audioMasterGetNumAutomatableParameters  : OutputDebugString(L"AudioMasterGetNumAutomatableParameters\n"); break;
	case audioMasterGetParameterQuantization     : OutputDebugString(L"AudioMasterGetParameterQuantization\n");    break;
    
	case audioMasterNeedIdle                     : return vstHost->needIdle();
    
	case audioMasterGetPreviousPlug              : OutputDebugString(L"AudioMasterGetPreviousPlug\n");             break;
    case audioMasterGetNextPlug                  : OutputDebugString(L"AudioMasterGetNextPlug\n");                 break;
    case audioMasterWillReplaceOrAccumulate      : OutputDebugString(L"AudioMasterWillReplaceOrAccumulate\n");     break;
    case audioMasterSetOutputSampleRate          : OutputDebugString(L"AudioMasterSetOutputSampleRate\n");         break;
    case audioMasterGetOutputSpeakerArrangement  : OutputDebugString(L"AudioMasterGetOutputSpeakerArrangement\n"); break;
    case audioMasterSetIcon                      : OutputDebugString(L"AudioMasterSetIcon\n");                     break;
    case audioMasterOpenWindow                   : OutputDebugString(L"AudioMasterOpenWindow\n");                  break;
    case audioMasterCloseWindow                  : OutputDebugString(L"AudioMasterCloseWindow\n");                 break;
    case audioMasterEditFile                     : OutputDebugString(L"AudioMasterEditFile\n");                    break;
    case audioMasterGetChunkFile                 : OutputDebugString(L"AudioMasterGetChunkFile\n");                break;
    case audioMasterGetInputSpeakerArrangement   : OutputDebugString(L"AudioMasterGetInputSpeakerArrangement\n");  break;  

  }

  return 0;
}

//*********************************************************************************************************************//
//*********************************************************************************************************************//
//*********************************************************************************************************************//

VSTHost::VSTHost()
{
  //Subscribe the host in the host-list
  HostCallback( 0, NEWHOST, 0, NEWHOST, this, 0);

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

  blockSize  = BLOCKSIZE;
  sampleRate = SAMPLERATE;

  module = NULL;

}


VSTHost::~VSTHost()
{
  //Unsubscribe the host in the list
  HostCallback( 0, DISCHARGEHOST, 0, DISCHARGEHOST, this, 0);

  plugin.~VSTPlugin();

  if(vstEvents)
  {
   free(vstEvents);

   vstEvents = NULL;
  }

  //Unload the dll
  if(module)
  {
    FreeLibrary(module);
    module = NULL;
  }

}

AEffect* VSTHost::getEffect()
{
  return plugin.effect;
}

int VSTHost::getNumInputs()
{
  return plugin.numInputs;
}

int VSTHost::getNumOutputs()
{
  return plugin.numOutputs;
}

//send the audiodata to the plugin
bool VSTHost::process(float **in, float **out,int nFrames) 
{
  if(!plugin.effect)
	return false;

  if(nFrames > blockSize)
  {
    blockSize = nFrames;
	plugin.setBlocksize(blockSize);
  }
  
  updateTime1();

  //if the plugin can receive midi-data the msgs are collected and send right before processReplacing()
  if(midi.count) 
	sendMidi();

  __try
  {
    plugin.effect->processReplacing(plugin.effect,in,out,nFrames);
  }
  __except(GetExceptionCode() == EXCEPTION_INT_DIVIDE_BY_ZERO | EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
  {

  }

  updateTime2(nFrames);

  return true;
}

void VSTHost::sendMidi()
{
    static VstMidiEvent midiEvents[NMSG];

	int count = midi.count;
 
	for(int i= 0; i<count; i++)
	{
	  midiEvents[i].type        = kVstMidiType;
	  midiEvents[i].byteSize    = sizeof(VstMidiEvent);
	  midiEvents[i].deltaFrames = 0; //maybe that should be changed because of jitter, but only short frames are used
	  midiEvents[i].flags       = 0L;
	  midiEvents[i].noteLength  = 0L;
	  midiEvents[i].noteOffset  = 0L;

	  midiEvents[i].midiData[0] = midi.msgBuffer[i].data[0];
	  midiEvents[i].midiData[1] = midi.msgBuffer[i].data[1];
	  midiEvents[i].midiData[2] = midi.msgBuffer[i].data[2];
	  midiEvents[i].midiData[3] = 0;   

	  midiEvents[i].detune          = 0;
	  midiEvents[i].noteOffVelocity = 0;
	  midiEvents[i].reserved1       = 0;
	  midiEvents[i].reserved2       = 0;
	
	}
	
	//allocate enough space for the msgs
	vstEvents = (VstEvents*)malloc(sizeof(VstEvents) +  count * sizeof(VstEvent*));

	for(int i=0;i<count;i++)
	vstEvents->events[i] = (VstEvent*) &midiEvents[i];

	vstEvents->numEvents = count;
	vstEvents->reserved  = 0;

	__try
    {
     plugin.effect->dispatcher(plugin.effect,effProcessEvents,0,0,vstEvents,0);
    }
    __except(GetExceptionCode() == EXCEPTION_INT_DIVIDE_BY_ZERO | EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
    {

    }

	midi.count = 0;
    
}

void VSTHost::updateTime1()
{
  timeInfo.nanoSeconds   = (double)timeGetTime() * TIMEUNIT;

  double position        = timeInfo.samplePos / timeInfo.sampleRate;

  timeInfo.ppqPos        = position * timeInfo.tempo / 60.0;

  double offsetInSeconds = position - int(position);

  timeInfo.smpteOffset   = (long)(offsetInSeconds * 25.0 * 80.0);

}

void VSTHost::updateTime2(int nFrames)
{
  timeInfo.samplePos    += (float)nFrames;
}

//IDSVSTWrapper-Definitions********************************************************************************************//

bool VSTHost::load(char *filename)
{
  AEffect *effect = NULL;

  vstEvents = NULL;

  PluginMain pluginMain;


  module = LoadLibraryA(filename);

  if(!module)
	return false;

  //Two possible names of the entrypoint
  pluginMain = (PluginMain) GetProcAddress((HMODULE) module,"VSTPluginMain");

  if(!pluginMain)
	pluginMain = (PluginMain) GetProcAddress((HMODULE) module,"main");

  try
  {
    effect = pluginMain(HostCallback);
  }
  catch(...)
  {

  }

  if(!effect) 
	return false;

  //is it really a vst-plugin?
  if(effect->magic != kEffectMagic)	
	return false;

  //read-in the properties an initialize
  plugin.init(effect);

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

//some plugins might have a lot (>750) parameters
bool VSTHost::getParameterProperties( wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[])
{
  if(!plugin.effect) 
	return false;

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


bool VSTHost::getProgramNames (int *count, wchar_t names[][256])
{
  if(!plugin.effect) 
	return false;

  if(plugin.numPrograms >= MAXPROGRAM)
	*count = 0;
  else
  {
    *count = plugin.numPrograms;
  
    for(int i=0;i<plugin.numPrograms;i++)
    {
  	  for(int k=0;k<STRLENGTH;k++)
	  {
	   names[i][k] =  plugin.program[i].name[k];  //plugin.param[h].display[k];
	   if(plugin.program[i].name[k] == '\0') break;
	  }
    }

   }//end if numPrograms

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

bool VSTHost::getMidiIsInstrument()
{
 if(!plugin.effect) 
   return false;

 if(plugin.isSynth)
   return true;

 return false; 

}

//the msgs are collected and send right before processReplacing()
bool VSTHost::sendMidiNote(int count,int note[],int velocity[])
{
  if(!plugin.effect) 
	return false;

  for(int i=0;i<count;i++)
	midi.setMsg(NOTEON,note[i],velocity[i]); 
 
  return true;

}

bool VSTHost::sendMidiNoteAllOff()
{
  if(!plugin.effect) 
	return false;

  for(int i=0;i<NMIDINOTES;i++)
    midi.setMsg((char)NOTEON,i,0);

  return true;

}

bool VSTHost::sendMidiPolyphonic (unsigned char polyphonicNote, unsigned char polyphonicValue)
{
  if(!plugin.effect) 
	return false;

  midi.setMsg(POLYTOUCH, polyphonicNote, polyphonicValue);

  return true;

}

bool VSTHost::sendMidiController (unsigned char controllerID, unsigned char controllerValue)
{
  if(!plugin.effect) 
	return false;

  midi.setMsg(CONTROLCHANGE, controllerID, controllerValue);

  return true;

}

bool VSTHost::sendMidiProgram (unsigned char programID)
{
  if(!plugin.effect) 
	return false;

  midi.setMsg(PROGRAMCHANGE, programID, programID);

  return true;

}

bool VSTHost::sendMidiMonophonic (unsigned char monophonicValue)
{
  if(!plugin.effect) 
	return false;

  midi.setMsg(MONOTOUCH, monophonicValue, 0);  

  return true;

}

bool VSTHost::sendMidiPitchbend (unsigned char pitchbendValue)
{
  if(!plugin.effect) 
	return false;

  midi.setMsg(PITCHBEND, pitchbendValue, pitchbendValue);

  return true;

}

bool VSTHost::getInputCount(int *count)
{
  if(!plugin.effect) return false;

  *count = plugin.numInputs;

  return true;
}

bool VSTHost::getOutputCount(int *count)
{
  if(!plugin.effect) return false;

  *count = plugin.numOutputs;

  return true;
}

bool VSTHost::setBpm(int value)
{
  if(!plugin.effect) 
	return true;

  if(value > 0)
    timeInfo.tempo = value;

  return true;

}

bool VSTHost::getHasWindow()
{
  if(!plugin.effect) 
	return false;

  if(plugin.hasEditor) 
	return true;

  return false;  
}

//open the editor-window of the plugin
bool VSTHost::setWindowHandle(HWND hwnd)
{
  if(!plugin.effect) 
	return false;

  if(!plugin.hasEditor) 
	return false;

  plugin.setWindowHandle(hwnd);

  return true;

}

bool VSTHost::getWindowSize(int *width,int *height)
{
  if(!plugin.effect) 
	return false;

  *width  = plugin.width;
  *height = plugin.height;

  if(*width  < 0) *width  = 0;
  if(*height < 0) *height = 0; 

  if(!plugin.hasEditor) 
	return false;

  return true;

}

//called regularly in the evaluate-procedure of the vvvv-node
//to give the editor-window of the plugin some time
//to update itself
bool VSTHost::setWindowIdle()
{
  if(!plugin.effect) 
	return false;

  if(!plugin.hasEditor) 
	return false;

  plugin.effect->dispatcher(plugin.effect,effEditIdle,0,0,0,0);

  return true;

}

//AudioMaster-Callbacks************************************************************************************************//

long VSTHost::version()
{
  OutputDebugString(L"audioMasterVersion\n");

  return kVstVersion;
}

//???
long VSTHost::processEvents() 
{
  OutputDebugString(L"audioMasterProcessEvents\n");

  return false;
}

long VSTHost::getSampleRate()
{
  OutputDebugString(L"audioMasterGetSampleRate\n");

  return sampleRate;
}

long VSTHost::getBlockSize()
{
  OutputDebugString(L"audioMasterGetBlockSize\n");

  return blockSize;
}

long VSTHost::getTime()
{
  //OutputDebugString(L"audioMasterGetTime\n");

  return (long)&timeInfo;
}

long VSTHost::canDo(char *str)
{
  OutputDebugString(L"audioMasterCanDo\n");

  if(!strcmp(str, "sendVstEvents"))
    return true;

  if(!strcmp(str,"sendVstMidiEvent"))  
    return true;

  if(!strcmp(str,"receiveVstEvents"))  
    return true;

  if(!strcmp(str,"receiveVstMidiEvent"))  
    return true;

  if(!strcmp(str,"openFileSelector"))
	return true;

  if(!strcmp(str,"closeFileSelector"))
	return true;

  return false;

}

long VSTHost::getDirectory()
{
  OutputDebugString(L"audioMasterGetDirectory\n");

  char path[MAX_PATH];

  GetCurrentDirectoryA( MAX_PATH, path);

  return (long)path;
}

//used to open/store banks,patches and files
long VSTHost::openFileSelector  (VstFileSelect * fileSelect)
{
  int result = 0;

  if(fileSelect == NULL)
   return 0;

  if(!plugin.hwnd)
   return 0;

  if(fileSelect->command == kVstMultipleFilesLoad) //only one file per call
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

	strcpy(str,fileSelect->fileTypes->name); //description

    //the possible filetypes are shown to the user and 
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

	  //this section is not visible and is used to show only the files
	  //with the right filetype to the user
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

    //copy the chars into the wide-charstring
	wchar_t wideFilter[STRLENGTH];

	for(int i=0;i<strlength;i++)
	  wideFilter[i] = str[i];
	
	wideFilter[STRLENGTH-1] = '\0';

	//--------------------------------------------------//
	
	static TCHAR filename [MAX_PATH] = TEXT("\0");
    static TCHAR path     [MAX_PATH] = TEXT("."); //begin in the actual folder
	
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

	//sometimes the returnPath has to be initialized here
	if(!fileSelect->returnPath)
	{
	  fileSelect->reserved = 1;

  	  fileSelect->returnPath = new char[lstrlen(filename)];
	}

	for(int i=0;i<lstrlen(filename);i++)
	fileSelect->returnPath[i] = (char)filename[i];

	fileSelect->returnPath[lstrlen(filename)] = '\0';

	fileSelect->sizeReturnPath = lstrlen(filename);

	fileSelect->nbReturnPath = 1;

	fileSelect->returnMultiplePaths = NULL;
    
  }//end if kVstFileSave


  return result;

}

long VSTHost::closeFileSelector (VstFileSelect * fileSelect)
{
 if(fileSelect == NULL)
  return 0;

 //only if we allocated the space we should also unallocate it
 if(fileSelect->reserved == 1)
 {
   delete [] fileSelect->returnPath;

   fileSelect->returnPath = 0;
   fileSelect->reserved   = 0;
 }

 return 0;
}

long VSTHost::needIdle()
{
  if(plugin.effect)
    plugin.effect->dispatcher(plugin.effect,effIdle,0,0,0,0);
  
  return true;
}

long VSTHost::getVendorString(char *str)
{
  static char vendorString[] = "vvvv meanimal";

  str = vendorString;

  return true;
}

long VSTHost::getVendorVersion()
{
  return 1;
}

long VSTHost::getProductString (char *str)
{
  static char productString [] = "DSVSTWrapper 0.1";

  str = productString;

  return true;
}

long VSTHost::getVendorSpecific()
{
  return 1;
}

long VSTHost::wantMidi()
{
  OutputDebugString(L"audioMasterWantMidi\n");

  plugin.isSynth = true;

  return true;
}

