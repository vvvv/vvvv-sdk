#include "VSTPlugin.h"


Param::Param ()
{
  display[0] = '\0';
  label  [0] = '\0';
  name   [0] = '\0';

  value = 0;

  properties = new VstParameterProperties();

}

Param::~Param ()
{
  if(properties != NULL) delete properties;
}

/*************************************************************************/

VSTPlugin::VSTPlugin()
{
  effect = NULL;

  for(int i=0;i<NPLUGINCANDO;i++)
  canDo[i] = false;

  canDoStr = NULL;
  nCanDo   = 0;
  needIdle = false;

  sampleRate      = SAMPLERATE;
  blockSize       = BLOCKSIZE;
  actualProgram   = 0;
  wndThreadHandle = 0;

}

VSTPlugin::~VSTPlugin()
{

}

void VSTPlugin::destroy()
{
  DWORD exitCode = 0;

  if(wndThreadHandle)
   TerminateThread(wndThreadHandle, exitCode);

  effect = NULL;
}

//after setting the effect-pointer the values and parameters of the plugin 
//are read in
void VSTPlugin::initialize(AEffect *effect)
{
  if(!effect) return;

  this->effect = effect;

  uniqueID = effect->uniqueID;
  
  //Flags-------------------------------------------------------------------------//

  hasEditor      = effect->flags && effFlagsHasEditor;
  canReplacing   = effect->flags && effFlagsCanReplacing;
  programChunks  = effect->flags && effFlagsProgramChunks;
  isSynth        = effect->flags && effFlagsIsSynth;
  noSoundInStop  = effect->flags && effFlagsNoSoundInStop;

  //------------------------------------------------------------------------------//
  
  effect->dispatcher( effect, effGetEffectName, 0, 0, name, 0);

  effect->dispatcher( effect, effGetVendorString, 0, 0, vendor, 0);

  effect->dispatcher( effect, effGetProductString, 0, 0, product, 0);

  effect->dispatcher( effect, effGetProgramName, 0, 0, programName, 0);


  vendorVersion = effect->dispatcher( effect, effGetVendorVersion, 0, 0, 0, 0);

  vstVersion    = effect->dispatcher( effect, effGetVstVersion, 0, 0, 0, 0);

  plugCategory  = effect->dispatcher( effect, effGetPlugCategory, 0, 0, 0, 0);

  tailSize      = effect->dispatcher( effect, effGetTailSize, 0, 0, 0, 0);


  //initialize parameters---------------------------------------------------------//
  
  numParams      = effect->numParams;
  numInputs      = effect->numInputs;
  numOutputs     = effect->numOutputs;
  numPrograms    = effect->numPrograms;

  if(numParams)   param   = new Param   [numParams];
  if(numPrograms) program = new Program [numPrograms];

  if(numInputs)  inputProperties  = new VstPinProperties[numInputs];
  if(numOutputs) outputProperties = new VstPinProperties[numOutputs];


  for(int i=0;i<numPrograms;i++)
  effect->dispatcher(effect, effGetProgramNameIndexed, i, 0, program[i].name, 0);

  for(int i=0; i<numInputs; i++)
  effect->dispatcher(effect, effGetInputProperties, i, 0, &inputProperties[i], 0);

  for(int i=0; i<numOutputs; i++)
  effect->dispatcher(effect, effGetOutputProperties, i, 0, &outputProperties[i], 0);

  for(int i=0;i<numParams;i++)
  {
    effect->dispatcher( effect, effGetParamLabel,   i, 0, param[i].label,   0);
    effect->dispatcher( effect, effGetParamDisplay, i, 0, param[i].display, 0);
    effect->dispatcher( effect, effGetParamName,    i, 0, param[i].name,    0);

	if( !effect->dispatcher( effect, effGetParameterProperties, i, 0, &param[i].properties, 0) )
    param[i].properties = NULL;

	param[i].value = effect->getParameter(effect,i);	
  }


  //Can Do--------------------------------------------------------------------------//

  char *canDoNamespace [NPLUGINCANDO]  = { "sendVstEvents",
                                           "sendVstMidiEvent",
                                           "receiveVstEvents",
                                           "receiveVstMidiEvent",
                                           "receiveVstTimeInfo",
                                           "offline",
                                           "midiProgramNames",
                                           "bypass" };

  for(int i=0; i<NPLUGINCANDO; i++)
  if( effect->dispatcher( effect, effCanDo, 0, 0, canDoNamespace[i], 0) )
  {
   nCanDo++;
   canDo[i] = true;
  }
  else
   canDo[i] = false;
  
  canDoStr = new char*[nCanDo];

  for(int i=0; i<nCanDo; i++)
  canDoStr[i] = new char[STRLENGTH];

  int counter=0;

  for(int i=0;i<NPLUGINCANDO;i++)
  if( effect->dispatcher( effect, effCanDo, 0, 0, canDoNamespace[i], 0) )
  strcpy(canDoStr[counter++],canDoNamespace[i]);

  //--------------------------------------------------------------------------------//

  cbOpen          ();
  suspend         ();
  cbSetSampleRate (SAMPLERATE);
  cbSetBlockSize  (BLOCKSIZE);
  setProgram      (0);

  //--------------------------------------------------------------------------------//

  //open a winapi window
  
  if(hasEditor)
  {
	 wndID = 1000;

     wndThreadHandle = CreateThread( NULL, 0, windowThread, (LPVOID)effect, 0, &wndID );
  }

  displayProperties ();
  resume            ();

}//end initialize

void VSTPlugin::cbOpen  ()  
{
  effect->dispatcher( effect, effOpen, 0, 0, 0, 0);
}

void VSTPlugin::cbClose ()  
{
  effect->dispatcher( effect, effClose, 0, 0, 0, 0);
}

void VSTPlugin::resume()
{
  effect->dispatcher(effect, effMainsChanged, 0, true, 0, 0); 
}

void VSTPlugin::suspend()
{
  effect->dispatcher(effect, effMainsChanged, 0, false, 0, 0);
}

void VSTPlugin::cbSetSampleRate (int sampleRate)
{
  effect->dispatcher( effect, effSetSampleRate, 0, 0, 0, (float)sampleRate );
}

void VSTPlugin::cbSetBlockSize (int blockSize)
{
  effect->dispatcher( effect, effSetBlockSize, 0, (float)blockSize, 0, 0 );
}

double VSTPlugin::getParameter(int index)
{
  param[index].value = effect->getParameter(effect,index);

  return (double)param[index].value;
}

void VSTPlugin::setProgram(int index)
{
  effect->dispatcher(effect, effSetProgram, 0, index, NULL, 0);

  actualProgram = effect->dispatcher(effect, effGetProgram, 0, actualProgram, NULL, 0);
}

void VSTPlugin::getProgramName(char *name)
{
  effect->dispatcher(effect, effGetProgram, 0, 0, name, 0);
}

void VSTPlugin::setParameter(int index,double value)
{
  effect->setParameter(effect,index,(float)value);

  effect->dispatcher(effect, effEditIdle, 0,0,0,0);//???
}

void VSTPlugin::midiMsg(unsigned char d0, unsigned char d1, unsigned char d2)
{
  VstMidiEvent vstMidiEvent;

  vstMidiEvent.type            = kVstMidiType;
  vstMidiEvent.deltaFrames     = 0;
  vstMidiEvent.flags           = kVstMidiEventIsRealtime;
  vstMidiEvent.byteSize        = sizeof(VstMidiEvent);
  vstMidiEvent.noteLength      = 0;
  vstMidiEvent.noteOffset      = 0;
  vstMidiEvent.noteOffVelocity = 0;
  vstMidiEvent.detune          = 0;
  vstMidiEvent.reserved1       = 0;
  vstMidiEvent.reserved2       = 0;

  vstMidiEvent.midiData[0] = d0;
  vstMidiEvent.midiData[1] = d1;
  vstMidiEvent.midiData[2] = d2;
  vstMidiEvent.midiData[3] = 0;

  VstEvents vstEvents;

  vstEvents.numEvents = 1;
  vstEvents.reserved  = 0L;
  vstEvents.events[0] = (VstEvent*)&vstMidiEvent;

  effect->dispatcher( effect, effProcessEvents, 0, 0, &vstEvents, 0);

}

void VSTPlugin::sendMidiNotes(int count,int note[],int velocity[])
{
  VstMidiEvent *vstMidiEvent = new VstMidiEvent[count];

  for(int i=0;i<count;i++)
  {
    vstMidiEvent[i].type            = kVstMidiType;
    vstMidiEvent[i].deltaFrames     = 0;
    vstMidiEvent[i].flags           = kVstMidiEventIsRealtime;
    vstMidiEvent[i].byteSize        = sizeof(VstMidiEvent);
    vstMidiEvent[i].noteLength      = 0;
    vstMidiEvent[i].noteOffset      = 0;
    vstMidiEvent[i].noteOffVelocity = 0;
    vstMidiEvent[i].detune          = 0;
    vstMidiEvent[i].reserved1       = 0;
    vstMidiEvent[i].reserved2       = 0;

    vstMidiEvent[i].midiData[0] = NOTEON;
    vstMidiEvent[i].midiData[1] = note     [i];
    vstMidiEvent[i].midiData[2] = velocity [i];
	vstMidiEvent[i].midiData[3] = 0;
  }
  
  VstEvents* vstEvents = (VstEvents*)malloc(sizeof(VstEvents) + count * sizeof(VstEvent*));

  vstEvents->numEvents = count;
  vstEvents->reserved  = 0L;
  
  for(int i=0;i<count;i++)
  vstEvents->events[i] = (VstEvent*) &vstMidiEvent[i];

  
  effect->dispatcher( effect, effProcessEvents, 0, 0, vstEvents, 0);


  free(vstEvents);
  delete [] vstMidiEvent;

}

void VSTPlugin::displayProperties()
{
   if(!DEBUG) return;
  

   char buffer[512];

   out(L"\nplugin->DisplayProperties>***********************************\n\n");

   outputString("Name        : ", name,        1);
   outputString("Vendor      : ", vendor,      1);
   outputString("Product     : ", product,     1);
   outputString("ProgramName : ", programName, 1);

   out(L"\n");

   scanf(buffer,"VendorVersion : %d\nVstVersion    : %0.1f\n\n",vendorVersion,vstVersion / 1000.0);
   out(buffer);

  
   scanf(buffer,"NumParams  : %d\nNumInputs  : %d\nNumOutputs : %d\n",numParams,numInputs,numOutputs);
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
       
	   scanf(buffer,"Properties: category %d\n displayIndex %d\n stepFloat %f\n smallStepFloat %f\n largeStepFloat %f\n minInteger %d\n maxInteger %d\n stepInteger %d\n largeStepInteger %d\n displayIndex %d\n numParamsInCategory %d\n",
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
	 else out(L"No properties\n");

   }

   out("\n");

   for(int programIndex = 0; programIndex < numPrograms; programIndex++)
   outputString("Program : ",program[programIndex].name,1);

   out(L"\n");

   for(int i=0; i<numInputs; i++)
   {
	 if(strlen(inputProperties[i].label)) outputString("InputProperties.Label  : ",inputProperties[i].label,1);

	 scanf(buffer,"InputProperties.Flags  : %d\n",inputProperties[i].flags);

	 out(buffer);
 
	 //if(strlen(inputProperties[i].shortLabel)) outputString("InputProperties.ShortLabel : ",inputProperties[i].shortLabel,1);
	 
	 //swprintf(buffer,L"InputProperties.ArangementType : %d\n",inputProperties[i].arrangementType);
	
	 //OutputDebugString(buffer);
   }

   out(L"\n");

   for(int i=0; i<numOutputs; i++)
   {
	 if(strlen(outputProperties[i].label)) outputString("OutputProperties.Label : ",outputProperties[i].label,1);

	 scanf(buffer,"OutputProperties.Flags : %d\n",outputProperties[i].flags);

	 out(buffer);

	 if(outputProperties[i].flags & kVstPinIsActive)
	   OutputDebugString(L"Output is active");
 
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

   for(int i=0; i<nCanDo; i++)
   outputString("CanDo : ",canDoStr[i],1);

   out(L"\n");

   if( tailSize == 0) out(L"TailSize : Unknown Tailsize\n");

   if( tailSize == 1) out(L"TailSize : No Tail\n");

   if( tailSize >  1) 
   {
   	 scanf(buffer,"TailSize : %d\n",tailSize);

	 out(buffer);
   }

   out(L"\n");

   scanf(buffer,"UniqueID : %d\n",uniqueID);

   out(buffer);


   out(L"\n************************************************************\n");


}

