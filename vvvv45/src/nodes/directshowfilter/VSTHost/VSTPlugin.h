#ifndef _VSTPLUGIN_H
#define _VSTPLUGIN_H

#define _CRT_SECURE_NO_DEPRECATE


#include <stdio.h>
#include <iostream>
#include <windows.h>
#include <math.h>
#include "pluginterfaces/vst2.x/aeffectx.h"
#include "Global.h"
#include "GUI.h"


/*************************************************************************/

int __cdecl scanf(__in_z __format_string const char * _Format, ...);

struct Param
{
  char name    [STRLENGTH];
  char label   [STRLENGTH];
  char display [STRLENGTH];

  float value;

  VstParameterProperties *properties;

  Param ();
 ~Param ();

};

/*************************************************************************/

struct Program
{
  char name [STRLENGTH];
};

/*************************************************************************/

class VSTPlugin
{
  public : AEffect *effect;
           Param   *param;
		   Program *program;

		   //Flags
		   bool hasEditor;
		   bool canReplacing;
		   bool programChunks;
		   bool isSynth;
		   bool noSoundInStop;
		   bool canDoubleReplacing;

 		   int  numPrograms;
		   int  numParams;
		   int  numInputs;
		   int  numOutputs;

		   char name        [STRLENGTH];
		   char vendor      [STRLENGTH];
		   char product     [STRLENGTH];
		   char programName [STRLENGTH];

		   int uniqueID;
		   int plugCategory;
		   int tailSize;
		   int blockSize;
		   int sampleRate;
		   int nCanDo;
		   int actualProgram;

		   long vendorVersion;
		   long vstVersion;

		   bool needIdle;
		   bool canDo[8];

		   char **canDoStr;	

   		   HANDLE wndThreadHandle;
		   DWORD  wndID;

		   VstPinProperties *inputProperties;
		   VstPinProperties *outputProperties;

public :           VSTPlugin         ();
		          ~VSTPlugin         ();
		    void   initialize        (AEffect *effect);
		    double getParameter      (int index);
			void   setParameter      (int index,double value);
			void   sendMidiNotes     (int count,int note[],int velocity[]);
			void   midiMsg           (unsigned char d0, unsigned char d1, unsigned char d2);
			void   resume            ();
			void   suspend           ();
			void   destroy           ();
			void   displayProperties ();
			void   setProgram        (int index);
			void   getProgramName    (char *name);
					    
		    //encapsulated Callback-Functions-----------------------//
		   
			  virtual void cbOpen                      ();
			  virtual void cbClose                     ();
			//virtual void cbSetProgramName            ();
			//virtual void cbGetProgramName            ();
			//virtual void cbGetParamLabel             (int paramIndex);
			//virtual void cbGetParamDisplay           (int paramIndex);
			//virtual void cbGetParamName              (int paramIndex);
			  virtual void cbSetSampleRate             (int sampleRate);
			  virtual void cbSetBlockSize              (int blockSize);
			//virtual void cbMainsChanged              (bool active);
			//virtual void cbEditGetRect               ();
			//virtual void cbEditOpen                  ();
			//virtual void cbEditClose                 ();
			//virtual void cbEditIdle                  ();
			//virtual void cbGetChunk                  ();
            //virtual void cbNumOpcodes                ();
			//virtual void cbSetProgram                (int index);
			//virtual void cbProcessEvents             ();
			//virtual void cbCanBeAutomated            ();
			//virtual void cbString2Parameter          ();
			//virtual void cbGetProgramNameIndexed     ();
			//virtual void cbGetInputProperties        ();
			//virtual void cbGetOutputProperties       ();
			//virtual void cbGetPlugCategory           ();
			//virtual void cbOfflineNotify             ();
			//virtual void cbOfflinePrepare            ();
			//virtual void cbOfflineRun                ();
			//virtual void cbProcessVarIO              ();
			//virtual void cbSetSpeakerArangement      ();
			//virtual void cbSetBypass                 ();
			//virtual void cbGetEffectName             ();
			//virtual void cbGetVendorString           ();
			//virtual void cbGetProductString          ();
			//virtual void cbGetVendorVersion          ();
			//virtual void cbVendorSpecific            ();
			//virtual void cbCanDo                     ();
			//virtual void cbGetTailSize               ();
			//virtual void cbGetParameterProperties    ();
			//virtual void cbGetVstVersion             ();			
			//virtual void cbEditKeyDown               ();
			//virtual void cbEditKeyUp                 ();
			//virtual void cbSetEditKnobMode           ();
			//virtual void cbGetMidiProgramName        ();
			//virtual void cbGetCurrentMidiProgramName ();
			//virtual void cbMidiProgramCategory       ();
			//virtual void cbMidiProgramsChanged       ();
			//virtual void cbMidiKeyName               ();
			//virtual void cbBeginSetProgram           ();
			//virtual void cbEndSetProgram             ();
			//virtual void cbGetSpeakerArrangement     ();
			//virtual void cbShellGetNextPlugin        ();
			//virtual void cbStartProcess              ();
			//virtual void cbStopProcess               ();
			//virtual void cbSetTotalSampleToProcess   ();
			//virtual void cbSetPanLaw                 ();
			//virtual void cbBeginLoadBank             ();
			//virtual void cbBeginLoadProgram          ();
			//virtual void cbSetProcessPrecision       ();
			//virtual void cbGetNumInputMidiChannels   ();
			//virtual void cbGetNumMidiOutputChannels  ();
			

			//Effect-Opcode-Functions deprecated---------------------//
			//virtual void cbGetVu                     ();
			//virtual void cbEditDraw                  ();
			//virtual void cbEditMouse                 ();
			//virtual void cbEditKey                   ();
			//virtual void cbEditTop                   ();
			//virtual void cbEditSleep                 ();
			//virtual void cbIdentify                  ();

			//virtual void cbGetNumProgramCategories   ();
			//virtual void cbCopyProgram               ();
			//virtual void cbConnectInput              ();
			//virtual void cbConnectOutput             ();
			//virtual void cbGetCurrentPosition        ();
			//virtual void cbGetDestinationBuffer      ();
			//virtual void cbSetBlockSizeAndSampleRate ();
			//virtual void cbGetErrorText              ();
			//virtual void cbIdle                      ();
			//virtual void cbGetIcon                   ();
			//virtual void cbSetViewPosition           ();
			//virtual void cbKeysRequired              ();         

};

/*************************************************************************/

#endif