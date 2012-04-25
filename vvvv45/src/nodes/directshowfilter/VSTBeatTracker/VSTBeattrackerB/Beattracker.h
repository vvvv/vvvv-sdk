#ifndef BEATTRACKER_H
#define BEATTRACKER_H

#include <stdio.h>
#include <math.h>
#include <windows.h>
#include "audioeffectx.h"
#include "Global.h"
#include "Tracker.h"
#include "GUI.h"

//Implements the interface to the vst-host---------------------------------------------------//
class Beattracker : public AudioEffectX
{
   public : Beattracker (audioMasterCallback audioMaster);
		   ~Beattracker ();
		   
		    virtual void  processReplacing (float  **in,float  **out,int length);

			virtual void  setProgram (long program);
			virtual void  setProgramName (char *name);
			virtual void  getProgramName (char *name);
			virtual bool  getProgramNameIndexed (VstInt32 category, VstInt32 index,char *text);

			virtual void  setParameter(VstInt32 index,float value);
			virtual float getParameter(VstInt32 index);
			virtual void  getParameterLabel(VstInt32 index,char *label);
			virtual void  getParameterDisplay(VstInt32 index,char *text);
			virtual void  getParameterName(VstInt32 index,char *text);

			virtual int   canDo(char *text);
			virtual void  resume();

			virtual bool  getEffectName(char *name);
			virtual bool  getVendorString(char *text);
            virtual bool  getProductString(char *text);
			virtual int   getVendorVersion();

			virtual VstPlugCategory getPlugCategory ();

			void process(float **in,float **out,int length);

   private : Tracker *tracker;
			 Ctrl    *ctrl;

};

#endif