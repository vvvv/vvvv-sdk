
#ifndef _VSTPLUGIN_H
#define _VSTPLUGIN_H

#define _CRT_SECURE_NO_DEPRECATE

#include <stdio.h>
#include <math.h>
#include <windows.h>
#include "aeffectx.h"
#include "Global.h"

//*********************************************************************************************************************//

struct Param
{
  char name    [STRLENGTH];
  char label   [STRLENGTH];
  char display [STRLENGTH];

  float value;

  VstParameterProperties *properties; //most plugins do not support this info

  Param ();
 ~Param ();

};

//*********************************************************************************************************************//

struct Program
{
  char name[STRLENGTH];
};

//*********************************************************************************************************************//

//the task of the VSTPlugin and VSTHost is not clear differentiated
//perhaps it would be cleaner if all the effect-functions are encapsulated 
//in the VSTPlugin
class VSTPlugin
{
  public : bool hasEditor;
		   bool canReplacing;
		   bool isSynth;

           int numInputs;
           int numOutputs;
           int numParams;
		   int numPrograms;

		   int height;
		   int width;

		   bool receiveVstEvents;
		   bool receiveVstMidiEvent;

		   AEffect *effect; //the pointer to the plugin
		   Param   *param;
		   Program *program;

		   HWND hwnd; //editor-window

		   int actualProgram;

		   VSTPlugin();
		  ~VSTPlugin();

		   void midiMsg(unsigned char d0, unsigned char d1, unsigned char d2);

		   void   init            (AEffect *effect);
		   void   open            ();
		   void   close           ();
		   void   resume          ();
		   void   suspend         ();
           void   setSamplerate   (int samplerate);
		   void   setBlocksize    (int blocksize);
           double getParameter    (int index);
           void   setProgram      (int index);
           void   getProgramName  (char *name);
           void   setParameter    (int index,double value);
		   void   setWindowHandle (HWND hwnd);

};

#endif