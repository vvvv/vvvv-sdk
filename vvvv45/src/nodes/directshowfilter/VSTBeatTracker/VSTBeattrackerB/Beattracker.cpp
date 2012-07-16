#include "Beattracker.h"

AudioEffect* createEffectInstance (audioMasterCallback audioMaster)
{
  return new Beattracker (audioMaster);
}

Beattracker::Beattracker (audioMasterCallback audioMaster)
:AudioEffectX (audioMaster,NUMPROGRAMS,NUMPARAMS) 
{  
  setNumInputs  (2);
  setNumOutputs (2);
 
  setUniqueID   ('V4BT');

  cEffect.numPrograms = 1;
  cEffect.numParams   = NUMPARAMS;

  cEffect.flags  = 0;
  cEffect.flags |= effFlagsCanReplacing;
  cEffect.flags |= effFlagsNoSoundInStop;
  cEffect.flags |= effFlagsHasEditor;

  tracker = NULL;
  ctrl    = NULL;
  editor  = NULL;

  tracker = new Tracker((AudioEffect*)this);
  ctrl    = tracker->ctrl;
  editor  = tracker->gui;
  
} 
   
Beattracker::~Beattracker () 
{
  delete editor;
}

void Beattracker::processReplacing  (float **in,float **out,VstInt32 length) 
{
  static long count = 0;

  for(int i=0;i<length;i++)
  {
    out[0][i] = in[0][i]; 
    out[1][i] = in[1][i];
  }

  if(!ctrl || !tracker)
  	return;

  int frameSize = 0;
  int nFrames   = 0;

  for(int i=1;i<length;i++)
  if(length / i <= NSAMPLES && fmod((double) length / (double)i,1.0) == 0.0)
  {
	frameSize = length / i;
	nFrames   = length / frameSize;
	break;
  }

  ctrl->setSecondsPerFrame(frameSize);

  double data[NSAMPLES];

  double rad = (2*PI) / 32.; 

  for(int i=0;i<nFrames;i++)
  {
	for(int k=0;k<frameSize;k++)
      data[k] = (in[0][i * frameSize + k] + in[1][i * frameSize + k]) / 2.;
     
    tracker->process(data,frameSize);

	if(ctrl->signal)
    for(int k=0;k<frameSize;k++)
	if(ctrl->currentBeat)
	  out[1][i * frameSize + k] = (float)sin(rad*k);
	else
	  out[1][i * frameSize + k] = 0;

	if(ctrl->adjust)
	{
	  if(ctrl->period)
	  for(int k=0;k<frameSize;k++)
 	  {
  	    out[0][i * frameSize + k] = (float)sin(rad*k);
	    out[1][i * frameSize + k] = (float)sin(rad*k);
	  }
	  else
	  for(int k=0;k<frameSize;k++)
	  {
        out[0][i * frameSize + k] = 0;
  	    out[1][i * frameSize + k] = 0;
	  }
	
	}//end if adjust


  }//end for i

  ++count;


}//end process()

int Beattracker::canDo(char *text)
{
  if(!strcmp(text,"sendVstEvents"))
	return true;

  if(!strcmp(text,"receiveVstEvents"))
	return true;

  return false;
}

void Beattracker::setProgram (long program)
{

}

void Beattracker::setProgramName (char *name)
{
  sprintf(name,"%s","Tracking Beats");
}

void Beattracker::getProgramName (char *name)
{
  sprintf(name,"%s","Beattracker");
}

bool Beattracker::getProgramNameIndexed (VstInt32 category, int index,char *text)
{
  if(index == 0)
  {
	strcpy(text,"Tracking Beats");
    return true;
  }

  return false;
}

void Beattracker::setParameter(VstInt32 index,float value)
{
 
 if(!ctrl  || !tracker || !editor)
	return;

  switch(index)
  {
    case PARAM_SIGNAL    : ctrl->setSignal((int)value);     
	  
	                       ((GUI*)editor)->setParameter(index,ctrl->signal);    break;
	
	case PARAM_DELAY     : ctrl->setDelay(value);     

	                       ((GUI*)editor)->setParameter(index,ctrl->delay);     break;
	
	case PARAM_ADJUST    : ctrl->setAdjust((int)value);   

	                       ((GUI*)editor)->setParameter(index,ctrl->adjust);    break;

	case PARAM_TARGETBPM : ctrl->setTargetBpm(value); 

	                       ((GUI*)editor)->setParameter(index,ctrl->targetBpm); break;

	case PARAM_RESET     : if(value) ctrl->reset();                             break;
  }
  
  
}  

float Beattracker::getParameter(VstInt32 index)
{ 
  float value = 0;
   
  if(!ctrl)
	return 0;

  switch(index)
  {
    case PARAM_BEAT        : value = (float)ctrl->getBeat();         break;

	case PARAM_BEATSWITCH  : value = (float)ctrl->getBeatswitch();   break;

   	case PARAM_PHASE       : value = (float)ctrl->getPhase();        break;

	case PARAM_BPM         : value = (float)ctrl->getBpm();          break;

	case PARAM_PROBABILITY : value = (float)ctrl->getProbability();  break;

	case PARAM_SILENCE     : value = (float)ctrl->getSilence();      break;
	
	case PARAM_SIGNAL      : value = (float)ctrl->signal;            break;
    	
	case PARAM_DELAY       : value = (float)ctrl->delay;             break;
	
	case PARAM_ADJUST      : value = (float)ctrl->adjust;            break;
	
	case PARAM_TARGETBPM   : value = (float)ctrl->targetBpm;         break;
	
	case PARAM_RESET       : value = 0;    	                         break;

	case PARAM_BAND0       : value = (float)ctrl->getBand0();        break;

	case PARAM_BAND1       : value = (float)ctrl->getBand1();        break;

	case PARAM_BAND2       : value = (float)ctrl->getBand2();        break;

	case PARAM_BAND3       : value = (float)ctrl->getBand3();        break;
  
  }
  
 
  return value;

}

void Beattracker::getParameterLabel(VstInt32 index,char *label)
{
  switch(index)
  {
    case PARAM_BEAT        : strcpy(label,"Beat");             break;
	case PARAM_BEATSWITCH  : strcpy(label,"Beatswitch");       break;
    case PARAM_PHASE       : strcpy(label,"Phase");            break;
	case PARAM_BPM         : strcpy(label,"Bpm");              break;
	case PARAM_PROBABILITY : strcpy(label,"Probability");      break;
	case PARAM_SILENCE     : strcpy(label,"Silence");          break;
	case PARAM_SIGNAL      : strcpy(label,"Signal");           break;
	case PARAM_DELAY       : strcpy(label,"Delay");            break;
	case PARAM_ADJUST      : strcpy(label,"Adjust");           break;
	case PARAM_TARGETBPM   : strcpy(label,"Target Bpm");       break;
	case PARAM_RESET       : strcpy(label,"Reset");            break;
   	case PARAM_BAND0       : strcpy(label,"Onset Band-0");     break;
	case PARAM_BAND1       : strcpy(label,"Onset Band-1");     break;
	case PARAM_BAND2       : strcpy(label,"Onset Band-2");     break;
	case PARAM_BAND3       : strcpy(label,"Onset Band-3");     break;
  }

}

void Beattracker::getParameterDisplay(VstInt32 index,char *text)
{
  
  if(!ctrl)
	return;

  switch(index)
  {
    case PARAM_BEATSWITCH  : int2string(ctrl->getBeatswitch(),text,kVstMaxParamStrLen);           break;
    case PARAM_BEAT        : int2string(ctrl->getBeat(),text,kVstMaxParamStrLen);                 break;
	case PARAM_PHASE       : float2string((float)ctrl->getPhase(),text,kVstMaxParamStrLen);       break;
	case PARAM_BPM         : int2string(ctrl->getBpm(),text,kVstMaxParamStrLen);                  break;
	case PARAM_PROBABILITY : float2string((float)ctrl->getProbability(),text,kVstMaxParamStrLen); break;
	case PARAM_SILENCE     : float2string((float)ctrl->getSilence(),text,kVstMaxParamStrLen);     break;
	case PARAM_SIGNAL      : int2string(ctrl->signal,text,kVstMaxParamStrLen);                    break;
    case PARAM_DELAY       : int2string(ctrl->delay,text,kVstMaxParamStrLen);                     break;
    case PARAM_ADJUST      : int2string(ctrl->adjust,text,kVstMaxParamStrLen);                    break;
	case PARAM_TARGETBPM   : int2string(ctrl->targetBpm,text,kVstMaxParamStrLen);                 break;
    case PARAM_RESET       : strcpy(text,"0");                                                    break;
	case PARAM_BAND0       : float2string((float)ctrl->getBand0(),text,kVstMaxParamStrLen);       break;
	case PARAM_BAND1       : float2string((float)ctrl->getBand1(),text,kVstMaxParamStrLen);       break;
	case PARAM_BAND2       : float2string((float)ctrl->getBand2(),text,kVstMaxParamStrLen);       break;
	case PARAM_BAND3       : float2string((float)ctrl->getBand3(),text,kVstMaxParamStrLen);       break;
  }
  
}

void Beattracker::getParameterName(VstInt32 index,char *text)
{
  switch(index)
  {
    case PARAM_BEATSWITCH  : strcpy(text,"Beatswitch");       break;
    case PARAM_BEAT        : strcpy(text,"Beat");             break;
	case PARAM_PHASE       : strcpy(text,"Phase");            break;
	case PARAM_BPM         : strcpy(text,"Bpm");              break;
	case PARAM_PROBABILITY : strcpy(text,"Beat-Probability"); break;
	case PARAM_SILENCE     : strcpy(text,"Silence");          break;
	case PARAM_SIGNAL      : strcpy(text,"Signal");           break;
    case PARAM_DELAY       : strcpy(text,"Delay");            break;
	case PARAM_ADJUST      : strcpy(text,"Adjust");           break;
	case PARAM_TARGETBPM   : strcpy(text,"Target-Bpm");       break;
	case PARAM_RESET       : strcpy(text,"Reset");            break;
	case PARAM_BAND0       : strcpy(text,"Onset Band-0");     break;
	case PARAM_BAND1       : strcpy(text,"Onset Band-1");     break;
	case PARAM_BAND2       : strcpy(text,"Onset Band-2");     break;
	case PARAM_BAND3       : strcpy(text,"Onset Band-3");     break;
  }

}

void Beattracker::resume()
{
  AudioEffectX::resume();
}

bool Beattracker::getEffectName(char *name)
{
  strcpy(name,"Beattracker");

  return true;
}

bool Beattracker::getVendorString(char *text)
{
  strcpy(text,"meanimal.vvvv");

  return true;
}

bool Beattracker::getProductString(char *text)
{
  strcpy(text,"Beattracker");

  return true;
}

int  Beattracker::getVendorVersion()
{
  return VENDORVERSION;
}

VstPlugCategory Beattracker::getPlugCategory () 
{ 
  return kPlugCategAnalysis;
}

