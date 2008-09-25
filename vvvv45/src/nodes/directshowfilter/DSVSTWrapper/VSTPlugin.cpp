
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
  if(properties != NULL) 
	delete properties;
}

//*********************************************************************************************************************//

VSTPlugin::VSTPlugin()
{
  effect = NULL;

  hasEditor    = false;
  canReplacing = false;
  isSynth      = false;

  numInputs   = 0;
  numOutputs  = 0;
  numParams   = 0;
  numPrograms = 0;

  receiveVstEvents    = false;
  receiveVstMidiEvent = false;

  width  = 0;
  height = 0;

  hwnd = 0;

}

void VSTPlugin::init(AEffect *effect)
{
  this->effect = effect;

  //get the plugins properties
  numParams    = effect->numParams;
  numInputs    = effect->numInputs;
  numOutputs   = effect->numOutputs;
  numPrograms  = effect->numPrograms;

  if(effect->flags & effFlagsHasEditor > 0)
	hasEditor = true;

  if(effect->flags & effFlagsCanReplacing > 0)
	canReplacing = true;
  
  if(effect->flags & effFlagsIsSynth > 0)
	isSynth = true;

  if(effect->dispatcher(effect,effCanDo,0,0,"receiveVstEvents",0)>0)
    receiveVstEvents = true;  

  if(effect->dispatcher(effect,effCanDo,0,0,"receiveVstMidiEvent",0)>0)
	receiveVstMidiEvent = true;

  if(numParams)   
	param = new Param [numParams];
  
  if(numPrograms) 
	program = new Program [numPrograms];

  for(int i=0;i<numPrograms;i++)
    effect->dispatcher(effect, effGetProgramNameIndexed, i, 0, program[i].name, 0);

  for(int i=0;i<numParams;i++)
  {
    effect->dispatcher( effect, effGetParamLabel,   i, 0, param[i].label,   0);
    effect->dispatcher( effect, effGetParamDisplay, i, 0, param[i].display, 0);
    effect->dispatcher( effect, effGetParamName,    i, 0, param[i].name,    0);

	if( !effect->dispatcher( effect, effGetParameterProperties, i, 0, &param[i].properties, 0) )
    param[i].properties = NULL;

	param[i].value = effect->getParameter(effect,i);	
  }
  
  //initialize the plugin
  open          ();
  suspend       ();
  setSamplerate (SAMPLERATE);
  setBlocksize  (BLOCKSIZE);
  setProgram    (0);
  resume        ();

}

VSTPlugin::~VSTPlugin()
{
  if(!effect)
	return;

  if(numParams == 1)
  if(param)
  {
 	delete param;
	param = NULL;
  }

  if(numParams >  1)
  if(param)
  {
	delete [] param;
	param = NULL;
  }

  if(numPrograms == 1)
  if(program)
  {
	delete program;
	program = NULL;
  }

  if(numPrograms  > 1)
  if(program)
  {
	delete [] program;
	program = NULL;
  }

  suspend ();
  close   (); //causes an error, why?

  effect = NULL;

}

double VSTPlugin::getParameter(int index)
{
  param[index].value = effect->getParameter(effect,index);

  return (double)param[index].value;
}

void VSTPlugin::setParameter(int index,double value)
{
  effect->setParameter(effect,index,(float)value);
}

//send a single midi-msg
void VSTPlugin::midiMsg(unsigned char d0, unsigned char d1, unsigned char d2)
{
  static VstMidiEvent vstMidiEvent;

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

  vstMidiEvent.midiData[0]     = d0;
  vstMidiEvent.midiData[1]     = d1;
  vstMidiEvent.midiData[2]     = d2;
  vstMidiEvent.midiData[3]     = 0;

  static VstEvents vstEvents;

  vstEvents.numEvents = 1;
  vstEvents.reserved  = 0L;
  vstEvents.events[0] = (VstEvent*)&vstMidiEvent;

  effect->dispatcher( effect, effProcessEvents, 0, 0, &vstEvents, 0);
  
}

void VSTPlugin::setWindowHandle(HWND hwnd)
{
  ERect* eRect;

  this->hwnd = hwnd;

  try
  {

   effect->dispatcher( effect, effEditOpen, 0, 0, hwnd, 0);

  }
  catch(...)
  {

  }

  effect->dispatcher(effect,effEditGetRect,0,0,&eRect,0);

  if(eRect)
  {
 	width  = eRect->right  - eRect->left;
	height = eRect->bottom - eRect->top;

	if(width < 0) 
	width = 0;

	if(height < 0)
	height = 0;

  }//if ERect

  effect->dispatcher(effect,effEditIdle,0,0,0,0);

}

//eff-Callbacks...
void VSTPlugin::open()
{
  effect->dispatcher( effect, effOpen, 0, 0, 0, 0);
}

void VSTPlugin::close()
{
  if(hwnd) 
	effect->dispatcher(effect,effEditClose, 0, 0, 0, 0);

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

void VSTPlugin::setSamplerate(int samplerate)
{
  effect->dispatcher( effect, effSetSampleRate, 0, 0, 0, (float)samplerate );
}

void VSTPlugin::setBlocksize(int blocksize)
{
  effect->dispatcher( effect, effSetBlockSize, 0, (float)blocksize, 0, 0 );
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
