#include "Plugin.h"


//*****************************************************************************************************//

Param::Param ()
{
  display[0] = '\0';
  label  [0] = '\0';
  name   [0] = '\0';

  value = 0;
  min   = 0;
  max   = 0;

  properties = new VstParameterProperties();

}

Param::~Param ()
{
  if(properties != NULL) delete properties;
}

//*****************************************************************************************************//

Plugin::Plugin() 
{
  effect  = NULL;
  param   = NULL;
  program = NULL;

  inputProperties  = NULL;
  outputProperties = NULL;

  numPrograms = 0;
  numParams   = 0;
  numInputs   = 0;
  numOutputs  = 0;

  name        [0] = '\0';
  vendor      [0] = '\0';
  product     [0] = '\0';
  programName [0] = '\0';

  hasEditor     = 0;
  canReplacing  = 0;
  programChunks = 0;
  isSynth       = 0;
  noSoundInStop = 0;

  vendorVersion = 0;
  vstVersion    = 0;

  needIdle      = false;
  uniqueID      = 0;
  plugCategory  = 0;
  tailSize      = 0;

  numCanDos     = 0;
  
  for(int i=0;i<NUMPLUGINCANDOS;i++)
  canDo[i] = false;

  canDoStr = NULL;

}

bool Plugin::destroy()
{
  DWORD exitCode = 0;

  TerminateThread( wndThreadHandle, exitCode);

  effect = NULL;

  return true;
}

int Plugin::init(AEffect* effect)
{
  this->effect = effect;

  if(effect == NULL)
  {
	out(L"plugin->Init> failed\n");

	return 0;
  }

  out(L"plugin->Init> loaded\n");

  //get effects properties-----------------------------------------------------------------------------//
  cbGetEffectName    ();
  cbGetVendorString  ();
  cbGetProductString ();
  cbGetProgramName   ();
  cbGetVendorVersion ();
  cbGetVstVersion    ();

  hasEditor      = effect->flags && effFlagsHasEditor;
  canReplacing   = effect->flags && effFlagsCanReplacing;
  programChunks  = effect->flags && effFlagsProgramChunks;
  isSynth        = effect->flags && effFlagsIsSynth;
  noSoundInStop  = effect->flags && effFlagsNoSoundInStop;

  numParams   = effect->numParams;
  numInputs   = effect->numInputs;
  numOutputs  = effect->numOutputs;
  numPrograms = effect->numPrograms;


  if(numParams)   param   = new Param   [numParams];
  if(numPrograms) program = new Program [numPrograms];

  if(numInputs)  inputProperties  = new VstPinProperties[numInputs];
  if(numOutputs) outputProperties = new VstPinProperties[numOutputs];


  for(int paramIndex = 0; paramIndex < numParams; paramIndex++)
  {
	 cbGetParamDisplay ( paramIndex );
	 cbGetParamName    ( paramIndex );
	 cbGetParamLabel   ( paramIndex );
	 
	 param[paramIndex].value = effect->getParameter(effect, paramIndex);

  }//end for paramIndex------------//

  cbGetParameterProperties ();
  cbGetProgramNameIndexed  ();
  cbGetInputProperties     ();
  cbGetOutputProperties    ();
  cbCanDo                  ();
  cbGetPlugCategory        ();
  cbGetTailSize            ();

  uniqueID = effect->uniqueID;


  if(DEBUG) displayProperties ();
  

  //set effect properties-----------------------------------------------------------------------------//
  cbSetSampleRate  ( SAMPLERATE );
  cbSetBlockSize   ( BLOCKSIZE  );
  cbOpen           ();


  //Open the window----------------------------------------------------------------------------------//

  if(OPENWINDOW)
  {
    wndID = 1000;

    if(OPENWINDOW) wndThreadHandle = CreateThread( NULL, 0, windowThread, (LPVOID)effect, 0, &wndID );
  }

  //------------------------------------------------------------------------------------------------//

  return 1;

}

void Plugin::setParameter(int index, float value)
{
  if(effect == NULL) return;

  effect->setParameter(effect, index, value);

  //swprintf(buffer,L"SetParameter : %f\n",effect->getParameter(effect,index));
  //out(buffer);

  effect->dispatcher(effect,effEditIdle,0,0,0,0); //!!! Update
 
}

int  Plugin::getNumParams ()
{
  return numParams;
}

bool Plugin::getNeedIdle  ()
{
  return needIdle;
}

void Plugin::setNeedIdle  (bool needIdle)
{
  this->needIdle = needIdle;
}

void Plugin::getParamDisplay(int index, wchar_t display[])
{
  for(int k=0;k<STRLENGTH;k++)
  {
    display[k] = param[index].display[k];

	if(param[index].display[k] == '\0') break;
  }
}

void Plugin::getParamName(int index, wchar_t name[])
{
  for(int k=0;k<STRLENGTH;k++)
  {
   name[k] = param[index].name[k];
 
   if(param[index].name[k] == '\0') break;
  }
}

void Plugin::getParamLabel(int index, wchar_t label[])
{
  for(int k=0;k<STRLENGTH;k++)
  {
   label[k] = param[index].label[k];

   if(param[index].label[k] == '\0') break;
  }
}

double Plugin::getParamValue(int index)
{
  if(effect == NULL) return 0;

  param[index].value = effect->getParameter( effect, index);

  return (double)param[index].value;
}

void Plugin::displayProperties()
{
   /*
   out(L"\nplugin->DisplayProperties>***********************************\n\n");

   outputString("Name        : ", name,        1);
   outputString("Vendor      : ", vendor,      1);
   outputString("Product     : ", product,     1);
   outputString("ProgramName : ", programName, 1);

   out(L"\n");

   swprintf(buffer,L"VendorVersion : %d\nVstVersion    : %0.1f\n\n",vendorVersion,vstVersion / 1000.0);
   out(buffer);

   swprintf(buffer,L"NumParams  : %d\nNumInputs  : %d\nNumOutputs : %d\n",numParams,numInputs,numOutputs);
   out(buffer);

   out(L"\n");

   for(int paramIndex = 0; paramIndex < numParams; paramIndex++)
   {
	 outputString("Name: ",0);
	 outputString(param[paramIndex].name,0);
    
	 outputString(" Label: ",0);
	 outputString(param[paramIndex].label,0);

	 outputString(" Display: ",0);
	 outputString(param[paramIndex].display,1);

     if(param[paramIndex].properties != NULL)
	 if(param[paramIndex].properties->category != 0) 
	 {
	   outputString(" Label          : ",param[paramIndex].properties->label,1);
	   outputString(" Category-Label : ",param[paramIndex].properties->categoryLabel,1);
	   outputString(" ShortLabel     : ",param[paramIndex].properties->shortLabel,1);
       
	   swprintf(buffer,L"Properties: category %d\n displayIndex %d\n stepFloat %f\n smallStepFloat %f\n largeStepFloat %f\n minInteger %d\n maxInteger %d\n stepInteger %d\n largeStepInteger %d\n displayIndex %d\n numParamsInCategory %d\n",
		        param[paramIndex].properties->category,
	   	        param[paramIndex].properties->displayIndex,
				param[paramIndex].properties->stepFloat,
			    param[paramIndex].properties->smallStepFloat,
				param[paramIndex].properties->largeStepFloat,
				param[paramIndex].properties->minInteger,
				param[paramIndex].properties->maxInteger,
				param[paramIndex].properties->stepInteger,
				param[paramIndex].properties->largeStepInteger,
				param[paramIndex].properties->displayIndex,
				param[paramIndex].properties->numParametersInCategory );

       out(buffer);
	 
	 }
	 //else out(L"No properties\n");

   }

   out(L"\n");

   for(int programIndex = 0; programIndex < numPrograms; programIndex++)
   outputString("Program : ",program[programIndex].name,1);

   out(L"\n");

   for(int i=0; i<numInputs; i++)
   {
	 if(strlen(inputProperties[i].label)) outputString("InputProperties.Label  : ",inputProperties[i].label,1);

	 swprintf(buffer,L"InputProperties.Flags  : %d\n",inputProperties[i].flags);

	 OutputDebugString(buffer);
 
	 //if(strlen(inputProperties[i].shortLabel)) outputString("InputProperties.ShortLabel : ",inputProperties[i].shortLabel,1);
	 
	 //swprintf(buffer,L"InputProperties.ArangementType : %d\n",inputProperties[i].arrangementType);
	
	 //OutputDebugString(buffer);
   }

   out(L"\n");

   for(int i=0; i<numOutputs; i++)
   {
	 if(strlen(outputProperties[i].label)) outputString("OutputProperties.Label : ",outputProperties[i].label,1);

	 swprintf(buffer,L"OutputProperties.Flags : %d\n",outputProperties[i].flags);

	 OutputDebugString(buffer);
 
	 //if(strlen(outputProperties[i].shortLabel)) outputString("OutputProperties.ShortLabel : ",outputProperties[i].shortLabel,1);
	 
	 //swprintf(buffer,L"OutputProperties.ArangementType : %d\n",outputProperties[i].arrangementType);
	
	 //OutputDebugString(buffer);
   }

  
   out(L"\n");

   if(hasEditor) out(L"has an Editor\n");
    else out(L"has no Editor\n");

   if(canReplacing) out(L"can replacing\n");
    else out(L"cannot do replacing\n");

   if(noSoundInStop) out(L"no sound in stop\n");
    else out(L"plays sound in stop\n");

   if(programChunks) out(L"program chunks\n");
    else out(L"no program chunks\n");

   if(isSynth) out(L"is synth\n");
    else out(L"is no synth\n");

   out(L"\n");

   out(L"Category : ");

   switch( plugCategory )
   {
	  case kPlugCategUnknown        : out(L"Unknown\n");                               break;
	  case kPlugCategEffect         : out(L"Simple Effect\n");                         break;
	  case kPlugCategSynth          : out(L"VST Instrument (Synths,Samplers,...)\n");  break;
	  case kPlugCategAnalysis       : out(L"Scope,Tuner...\n");                        break;
	  case kPlugCategMastering      : out(L"Dynamics,...\n");                          break;
	  case kPlugCategSpacializer    : out(L"Panners,...\n");                           break;
	  case kPlugCategRoomFx         : out(L"Delays and Reverbs\n");                    break;
	  case kPlugSurroundFx          : out(L"Dedicated surround processor\n");          break;
	  case kPlugCategRestoration    : out(L"Denoiser,...\n");                          break;
	  case kPlugCategOfflineProcess : out(L"Offline Process\n");                       break;
	  case kPlugCategShell          : out(L"Plug-in is container of other plugins\n"); break;
	  case kPlugCategGenerator      : out(L"ToneGenerator,...\n");                     break;
   }

   out(L"\n");

   for(int i=0; i<numCanDos; i++)
   outputString("CanDo : ",canDoStr[i],1);

   out(L"\n");

   if( tailSize == 0) out(L"TailSize : Unknown Tailsize\n");

   if( tailSize == 1) out(L"TailSize : No Tail\n");

   if( tailSize >  1) 
   {
     wchar_t buffer[STRLENGTH];

	 swprintf(buffer,L"TailSize : %d\n",tailSize);

	 out(buffer);
   }

   out(L"\n");

   swprintf(buffer,L"UniqueID : %d\n",uniqueID);

   OutputDebugString(buffer);


   out(L"\n************************************************************\n");
   */

}

int Plugin::process( float **in, float **out, int length, int nChannels)
{
  if(effect == NULL) return 0;

  //if( nChannels != effect->numInputs ) return 0;

  if(effect != NULL) effect->processReplacing( effect, in, out, length );

  return 1;

}

void Plugin::cbProcessEvents ()
{  

}

void Plugin::midiMsg(VstMidiEvent vstMidiEvent)
{
  if(effect == NULL) return;

  VstEvents    *vstEvents    = new VstEvents    ();

  vstEvents->numEvents = 1;
  vstEvents->events[0] = (VstEvent*)&vstMidiEvent;

  effect->dispatcher( effect, effProcessEvents, 0, 0, vstEvents, 0);

  //latency!!!beep
  //Beep(1000,50);

}


void Plugin::cbOpen  ()  
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effOpen, 0, 0, 0, 0);
}

void Plugin::cbClose ()  
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effClose, 0, 0, 0, 0);
}

void Plugin::cbSetProgramName ()
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effSetProgramName, 0, 0, programName, 0);
}

void Plugin::cbSetProgram (int indexProgram)
{
  if(effect == NULL) return;

  if(indexProgram < numPrograms)
  effect->dispatcher( effect, effSetProgram, 0, indexProgram, 0, 0);

}

void Plugin::cbGetProgramName ()  
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effGetProgramName, 0, 0, programName, 0);
}

void Plugin::cbGetParamLabel (int paramIndex)  
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effGetParamLabel, paramIndex, 0, param[paramIndex].label, 0);
}

void Plugin::cbGetParamDisplay (int paramIndex)   
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effGetParamDisplay, paramIndex, 0, param[paramIndex].display, 0);
}

void Plugin::cbGetParamName (int paramIndex)    
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effGetParamName, paramIndex, 0, param[paramIndex].name, 0);
}

void Plugin::cbSetSampleRate (int sampleRate)
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effSetSampleRate, 0, 0, 0, (float)sampleRate );
}

void Plugin::cbSetBlockSize (int blockSize)
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effSetBlockSize, 0, 0, 0, (float)blockSize );
}

void Plugin::cbMainsChanged (bool active)
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effMainsChanged, active, 0, 0, 0);
}

void Plugin::cbEditGetRect () 
{
 /* ERect  *pRect  = &rect;
  ERect **ppRect = &pRect;
  effect->dispatcher( effect, effEditGetRect, 0, 0, ppRect, 0);

  *(*ppRect)->bottom = 1000;*/

}

void Plugin::cbEditOpen                  ()  {}
void Plugin::cbEditClose                 ()  {}
void Plugin::cbEditIdle                  ()  {}
void Plugin::cbGetChunk                  ()  {}

void Plugin::cbNumOpcodes ()  {}


void Plugin::cbCanBeAutomated            ()  {}
void Plugin::cbString2Parameter          ()  {}


void Plugin::cbGetProgramNameIndexed ()
{
  if(effect == NULL) return;

  for(int i=0;i<numPrograms;i++)
  effect->dispatcher(effect, effGetProgramNameIndexed, i, 0, program[i].name, 0);

}

void Plugin::cbGetInputProperties ()
{
  if(effect == NULL) return;

  for(int i=0; i<numInputs; i++)
  effect->dispatcher(effect, effGetInputProperties, i, 0, &inputProperties[i], 0);
}

void Plugin::cbGetOutputProperties ()  
{
  if(effect == NULL) return;

  for(int i=0; i<numOutputs; i++)
  effect->dispatcher(effect, effGetOutputProperties, i, 0, &outputProperties[i], 0);
}

void Plugin::cbGetPlugCategory() 
{
  if(effect == NULL) return;

  plugCategory = effect->dispatcher( effect, effGetPlugCategory, 0, 0, 0, 0);

}

void Plugin::cbOfflineNotify             ()  {}
void Plugin::cbOfflinePrepare            ()  {}
void Plugin::cbOfflineRun                ()  {}
void Plugin::cbProcessVarIO              ()  {}
void Plugin::cbSetSpeakerArangement      ()  {}
void Plugin::cbSetBypass                 ()  {}

void Plugin::cbGetEffectName ()  
{
  if(effect == NULL) return;

   effect->dispatcher( effect, effGetEffectName, 0, 0, name, 0);
}

void Plugin::cbGetVendorString ()  
{
  if(effect == NULL) return;

   effect->dispatcher( effect, effGetVendorString, 0, 0, vendor, 0);
}

void Plugin::cbGetProductString ()
{
  if(effect == NULL) return;

  effect->dispatcher( effect, effGetProductString, 0, 0, product, 0);
}

void Plugin::cbGetVendorVersion () 
{
  if(effect == NULL) return;

  vendorVersion = effect->dispatcher( effect, effGetVendorVersion, 0, 0, 0, 0);
}

void Plugin::cbVendorSpecific            ()  {}

void Plugin::cbCanDo () 
{
  if(effect == NULL) return;

  const int num = 8;

  char *canDoNamespace [num]  = { "sendVstEvents",
                                  "sendVstMidiEvent",
                                  "receiveVstEvents",
                                  "receiveVstMidiEvent",
                                  "receiveVstTimeInfo",
                                  "offline",
                                  "midiProgramNames",
                                  "bypass" };

  for(int index = 0; index < num; index++)
  {
    if( effect->dispatcher( effect, effCanDo, 0, 0, canDoNamespace[index], 0) )
	{
	  numCanDos++;
      canDo[index] = true;
	}
	else
	  canDo[index] = false;

  }//end for index

  
  canDoStr = new char*[numCanDos];

  for(int i=0; i<numCanDos; i++)
  canDoStr[i] = new char[STRLENGTH];


  int counter=0;

  for(int index = 0; index < num; index++)
  if( effect->dispatcher( effect, effCanDo, 0, 0, canDoNamespace[index], 0) )
  strcpy(canDoStr[counter++],canDoNamespace[index]);
  
}

void Plugin::cbGetTailSize  ()
{
  if(effect == NULL) return;

  tailSize = effect->dispatcher(effect, effGetTailSize, 0, 0, 0, 0);
}


void Plugin::cbGetParameterProperties ()
{
  if(effect == NULL) return;

  for(int paramIndex = 0; paramIndex < numParams; paramIndex++)
  if( !effect->dispatcher( effect, effGetParameterProperties, paramIndex, 0, &param[paramIndex].properties, 0) )
   param[paramIndex].properties = NULL;

}

void Plugin::cbGetVstVersion ()
{
  if(effect == NULL) return;

  vstVersion = effect->dispatcher( effect, effGetVstVersion, 0, 0, 0, 0);
}
			
void Plugin::cbEditKeyDown               ()  {}
void Plugin::cbEditKeyUp                 ()  {}
void Plugin::cbSetEditKnobMode           ()  {}
void Plugin::cbGetMidiProgramName        ()  {}
void Plugin::cbGetCurrentMidiProgramName ()  {}
void Plugin::cbMidiProgramCategory       ()  {}
void Plugin::cbMidiProgramsChanged       ()  {}
void Plugin::cbMidiKeyName               ()  {}

void Plugin::cbBeginSetProgram ()  
{
  if(effect == NULL) return;

    //if(index < numPrograms && index >= 0)
	//effect->dispatcher(effect, effBeginSetProgram, 0, 0, 0, 0);

}

void Plugin::cbEndSetProgram             ()  {}
void Plugin::cbGetSpeakerArrangement     ()  {}
void Plugin::cbShellGetNextPlugin        ()  {}
void Plugin::cbStartProcess              ()  {}
void Plugin::cbStopProcess               ()  {}
void Plugin::cbSetTotalSampleToProcess   ()  {}
void Plugin::cbSetPanLaw                 ()  {}
void Plugin::cbBeginLoadBank             ()  {}
void Plugin::cbBeginLoadProgram          ()  {}
void Plugin::cbSetProcessPrecision       ()  {}
void Plugin::cbGetNumInputMidiChannels   ()  {}
void Plugin::cbGetNumMidiOutputChannels  ()  {}

//-------------------------------------------//

void Plugin::suspend()
{
  cbMainsChanged( false );
}

void Plugin::resume()
{
  needIdle = false;

  cbMainsChanged( true );
}

//*****************************************************************************************************//
