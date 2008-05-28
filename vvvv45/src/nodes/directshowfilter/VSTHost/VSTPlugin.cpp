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

  sampleRate = SAMPLERATE;
  blockSize  = BLOCKSIZE;

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
  cbSetSampleRate (SAMPLERATE);
  cbSetBlockSize  (BLOCKSIZE);
  resume          ();

  //--------------------------------------------------------------------------------//

  //open a winapi window
 

  if(hasEditor)
  {
	 wndID = 1000;

	 wndThreadHandle = CreateThread( NULL, 0, windowThread, (LPVOID)effect, 0, &wndID );
  }


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

void VSTPlugin::setParameter(int index,double value)
{
  effect->setParameter(effect,index,(float)value);

  effect->dispatcher(effect, effEditIdle, 0,0,0,0);//???
}

void VSTPlugin::midiMsg(unsigned char d0, unsigned char d1, unsigned char d2)
{
  VstMidiEvent vstMidiEvent;

  vstMidiEvent.type = kVstMidiType;

  vstMidiEvent.midiData[0] = d0;
  vstMidiEvent.midiData[1] = d1;
  vstMidiEvent.midiData[2] = d2;

  vstMidiEvent.deltaFrames = 0;


  VstEvents *vstEvents = new VstEvents();

  vstEvents->numEvents = 1;
  vstEvents->events[0] = (VstEvent*)&vstMidiEvent;

  effect->dispatcher( effect, effProcessEvents, 0, 0, vstEvents, 0);

}

void VSTPlugin::sendMidiNotes(int count,int note[],int velocity[])
{
  VstMidiEvent *midiEvent = new VstMidiEvent[count];

  for(int i;i<count;i++)
  {
    midiEvent[i].type        = kVstMidiType;   //SysEx???
    midiEvent[i].deltaFrames = 0;
    midiEvent[i].byteSize    = sizeof(VstMidiEvent);
    midiEvent[i].flags       = kVstMidiEventIsRealtime;

    midiEvent[i].midiData[0] = NOTEON;
    midiEvent[i].midiData[1] = note     [i];
    midiEvent[i].midiData[2] = velocity [i];

	midiEvent[i].deltaFrames = 0;
  }
  
  VstEvents* vstEvents = (VstEvents*)malloc(sizeof(VstEvents) + count * sizeof(VstEvent*));

  vstEvents->numEvents = count;
  
  for(int i=0;i<count;i++)
  vstEvents->events[i] = (VstEvent*) &midiEvent[i];

  
  effect->dispatcher( effect, effProcessEvents, 0, 0, vstEvents, 0);


  /*
  for(int i=0; i<count; i++)
  {  
	VstEvents  vstEvents;

    vstEvents.numEvents = 1;
    vstEvents.events[0] = (VstEvent*)&midiEvent[i];
    
	
	effect->dispatcher( effect, effProcessEvents, 0, 0, &vstEvents, 0);
  }
  */

  free(vstEvents);
  delete [] midiEvent;

}
