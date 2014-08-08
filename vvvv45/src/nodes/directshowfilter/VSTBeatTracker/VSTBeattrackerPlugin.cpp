#include "VSTBeattrackerPlugin.h"



AudioEffect* createEffectInstance (audioMasterCallback audioMaster)
{
  return new VSTBeattrackerPlugin (audioMaster);
}

DWORD WINAPI editorThread (LPVOID data)
{
  ((Editor*)data)->openWnd();

  return 0;

}

// Übertragung der Werte. Generierung von Werten zwischen 0 und 1;
VSTBeattrackerPlugin::VSTBeattrackerPlugin (audioMasterCallback audioMaster)
:AudioEffectX (audioMaster,0,0) 
{  
  HANDLE thread;
  DWORD  editorID=0;

  tracker = new Tracker ();               //the processing takes place in this class
  editor  = new Editor  ();               //used to display analysation-data 
  ctrl    = new Ctrl    (editor,tracker); //lies between tracker and editor and control their communication

  //functions are defined in the base-class AudioEffect
  //the host has access to the values set here 
  setNumInputs  (2);
  setNumOutputs (2);
  setUniqueID   ('V4BT');

  noisecounter = 0;
  counter = 0;

  if(DEBUG_WINDOW)
  thread = CreateThread(NULL,0,editorThread,(LPVOID)editor,0,&editorID);

  //WaitForMultipleObjects(1,&thread,true,INFINITE);  

}

VSTBeattrackerPlugin::~VSTBeattrackerPlugin () 
{
   
}

//Achtung im Moment nur für Stereodaten!
void VSTBeattrackerPlugin::processReplacing  (float **in,float **out,VstInt32 length) 
{
   if(length < MINLENGTH || length > MAXLENGTH) return;

   SINUS sinus;

   double *samplesIn = new double[length];

   for(int k=0; k<length; k++)
   {
     samplesIn[k] = *(*in+k);
	 *(*out+k)    = *(*in+k);
   }
   
   tracker->process( samplesIn, length, getSampleRate() );

   //-------------------------------------------------//

   if(DEBUG_SOUND)
   {
	 for(int k=0; k<length; k++)
	 *(*out+k) = 0;

	 if(tracker->signal.beep)
	 {
	   for(int k=0; k<length; k++)
	   *(*out+k) = (*(*out+k) + sinus.fx[3][k]) * 0.5;
	 }

   }//end if DEBUG_SOUND


   fb.set(tracker->signal.beep,tracker->signal.interval[tracker->beatDetection.channel],tracker->signal.fx,tracker->bpm);

   //
   //char buffer[256];

   //sprintf(buffer,"FX0 %f",tracker->signal.fx[0]);

   //OutputDebugString(buffer);


   //-------------------------------------------------//     

   this->cEffect.user = &fb;

   if(DEBUG_WINDOW)
   {
    if(counter == 0)
     ctrl->editor->draw(tracker);

	++counter %= 100;
   }

   delete samplesIn;

}//processReplacing()

void VSTBeattrackerPlugin::setBlockSize(int res)
{
  tracker->setPerformance(res);
}

void VSTBeattrackerPlugin::setNumInputs(int minBPM)
{
  tracker->signal.minBPM = minBPM;

  tracker->setBPM();
}

void VSTBeattrackerPlugin::setNumOutputs(int maxBPM)
{
  tracker->signal.maxBPM = maxBPM;

  tracker->setBPM();
}

void VSTBeattrackerPlugin::suspend()
{
   fb.reset();

   tracker->reset();
}

