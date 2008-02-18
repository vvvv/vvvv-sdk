#ifndef _PLUGIN_H
#define _PLUGIN_H

#define _CRT_SECURE_NO_DEPRECATE

#include <stdio.h>
#include <windows.h>
#include <math.h>
#include "pluginterfaces/vst2.x/aeffectx.h"
#include "Global.h"
#include "GUI.h"


struct Param
{
  char name    [STRLENGTH];
  char label   [STRLENGTH];
  char display [STRLENGTH];

  VstParameterProperties *properties;

  float value;
  float min;
  float max;

  Param ();
 ~Param ();
  
  void free();

};


struct Program
{
  char name [STRLENGTH];

};

		  



class Plugin 
{
  public  : AEffect *effect;
		 	Param   *param;
			Program *program;

			HANDLE wndThreadHandle;
			DWORD  wndID;
	
	        char name        [STRLENGTH];
			char vendor      [STRLENGTH];
			char product     [STRLENGTH];
			char programName [STRLENGTH];

			long vendorVersion;
			long vstVersion;

			int numPrograms;
			int numParams;
			int numInputs;
			int numOutputs;

			//Flags------------------------------------//
			int hasEditor;
			int canReplacing;
			int programChunks;
			int isSynth;
			int noSoundInStop;
			int canDoubleReplacing;
 
			//Flags deprecated-------------------------//
			//int hasClip;
			//int hasVu;
			//int canMono;
			//int extIsAsync;
			//int extHasBuffer;

			int  uniqueID;
			int  plugCategory;
			int  tailSize;

			VstPinProperties *inputProperties;
			VstPinProperties *outputProperties;

			//char **canDo;
			int  numCanDos;
			bool needIdle;

			bool canDo[8];

			char **canDoStr;	
			

			wchar_t buffer[512];

			//------------------------------------------------------------------------//

			Plugin                   ();
            int    init              (AEffect* effect);
			void   openWindow        ();
			void   setBuffers        ();
			int    process           (float **in, float **out, int length, int nChannels);
			void   displayProperties ();
			void   initParameters    ();
			void   suspend           ();
   		    void   resume            ();
			void   setParameter      (int index,float value);
			bool   destroy           ();
			void   midiMsg           (VstMidiEvent vstMidiEvent);
			int    getNumParams      ();
			bool   getNeedIdle       ();
			void   setNeedIdle       (bool needIdle);
			void   getParamDisplay   (int index, wchar_t display[]);
			void   getParamName      (int index, wchar_t name[]);
			void   getParamLabel     (int index, wchar_t label[]);
			double getParamValue     (int index);
						
			//Effect-Opcode-Functions--------------------------------//
			virtual void cbOpen                      ();
			virtual void cbClose                     ();
			virtual void cbSetProgramName            ();
			virtual void cbGetProgramName            ();
			virtual void cbGetParamLabel             (int paramIndex);
			virtual void cbGetParamDisplay           (int paramIndex);
			virtual void cbGetParamName              (int paramIndex);
			virtual void cbSetSampleRate             (int sampleRate);
			virtual void cbSetBlockSize              (int blockSize);
			virtual void cbMainsChanged              (bool active);
			virtual void cbEditGetRect               ();
			virtual void cbEditOpen                  ();
			virtual void cbEditClose                 ();
			virtual void cbEditIdle                  ();
			virtual void cbGetChunk                  ();
            virtual void cbNumOpcodes                ();
			virtual void cbSetProgram                (int indexProgram);
			virtual void cbProcessEvents             ();
			virtual void cbCanBeAutomated            ();
			virtual void cbString2Parameter          ();
			virtual void cbGetProgramNameIndexed     ();
			virtual void cbGetInputProperties        ();
			virtual void cbGetOutputProperties       ();
			virtual void cbGetPlugCategory           ();
			virtual void cbOfflineNotify             ();
			virtual void cbOfflinePrepare            ();
			virtual void cbOfflineRun                ();
			virtual void cbProcessVarIO              ();
			virtual void cbSetSpeakerArangement      ();
			virtual void cbSetBypass                 ();
			virtual void cbGetEffectName             ();
			virtual void cbGetVendorString           ();
			virtual void cbGetProductString          ();
			virtual void cbGetVendorVersion          ();
			virtual void cbVendorSpecific            ();
			virtual void cbCanDo                     ();
			virtual void cbGetTailSize               ();
			virtual void cbGetParameterProperties    ();
			virtual void cbGetVstVersion             ();			
			virtual void cbEditKeyDown               ();
			virtual void cbEditKeyUp                 ();
			virtual void cbSetEditKnobMode           ();
			virtual void cbGetMidiProgramName        ();
			virtual void cbGetCurrentMidiProgramName ();
			virtual void cbMidiProgramCategory       ();
			virtual void cbMidiProgramsChanged       ();
			virtual void cbMidiKeyName               ();
			virtual void cbBeginSetProgram           ();
			virtual void cbEndSetProgram             ();
			virtual void cbGetSpeakerArrangement     ();
			virtual void cbShellGetNextPlugin        ();
			virtual void cbStartProcess              ();
			virtual void cbStopProcess               ();
			virtual void cbSetTotalSampleToProcess   ();
			virtual void cbSetPanLaw                 ();
			virtual void cbBeginLoadBank             ();
			virtual void cbBeginLoadProgram          ();
			virtual void cbSetProcessPrecision       ();
			virtual void cbGetNumInputMidiChannels   ();
			virtual void cbGetNumMidiOutputChannels  ();
			
			//Effect-Opcode-Functions deprecated---------------------//
			/*
			virtual void cbGetVu                     ();
			virtual void cbEditDraw                  ();
			virtual void cbEditMouse                 ();
			virtual void cbEditKey                   ();
			virtual void cbEditTop                   ();
			virtual void cbEditSleep                 ();
			virtual void cbIdentify                  ();

			virtual void cbGetNumProgramCategories   ();
			virtual void cbCopyProgram               ();
			virtual void cbConnectInput              ();
			virtual void cbConnectOutput             ();
			virtual void cbGetCurrentPosition        ();
			virtual void cbGetDestinationBuffer      ();
			virtual void cbSetBlockSizeAndSampleRate ();
			virtual void cbGetErrorText              ();
			virtual void cbIdle                      ();
			virtual void cbGetIcon                   ();
			virtual void cbSetViewPosition           ();
			virtual void cbKeysRequired              ();
            */

};


#endif