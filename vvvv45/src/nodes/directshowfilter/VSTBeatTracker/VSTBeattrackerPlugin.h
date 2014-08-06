#ifndef DIRECTBEAT_H
#define DIRECTBEAT_H

#include <stdio.h>
#include <math.h>
#include <windows.h>

#include "audioeffectx.h"
#include "Editor.h"
#include "Tracker.h"
#include "Ctrl.h"
#include "StructCollection.h"

//the separate editor-thread-----------------------------------------------------//
DWORD WINAPI editorThread (LPVOID data);


//main class of the plugin controls communication with vst-host------------------//
class VSTBeattrackerPlugin : public AudioEffectX
{
   public : VSTBeattrackerPlugin (audioMasterCallback audioMaster);
		   ~VSTBeattrackerPlugin ();
		   
		   virtual void processReplacing (float  **in,float  **out,VstInt32 length);
		   virtual void suspend          ();
		   virtual void setBlockSize  (int);
		   virtual void setNumInputs  (int);
		   virtual void setNumOutputs (int);
		   
   private : Editor  *editor;
			 Tracker *tracker;
			 Ctrl    *ctrl;

			 FEEDBACKFRAME fb;
			 long noisecounter;
			 long counter;
			 
};

#endif