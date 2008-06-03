#ifndef _VSTHOST_H
#define _VSTHOST_H

#define _CRT_SECURE_NO_DEPRECATE

#include <stdio.h>
#include <windows.h>
#include <math.h>
#include "pluginterfaces/vst2.x/aeffectx.h"
#include "VSTPlugin.h"


/*************************************************************************/

//a pointer to the main-function of the vst-plugin
//is used for initialization
typedef AEffect* (*PluginMain) (audioMasterCallback audioMaster);

//with the hostcallback the plugin is able to send data 
//or requests to the host
static VstIntPtr VSTCALLBACK HostCallback ( AEffect *effect,
										    VstInt32 opcode,
											VstInt32 index,
											VstIntPtr value,
											void *ptr,
											float opt );

/*************************************************************************/

class VSTHost
{
  public  : VSTHost      ();
		   ~VSTHost      ();
		   	bool process (float **in, float **out,int length);
		 
			//IVVVVST-Interface-Definitions
			bool load                   (char * filename);	
			bool getParameterCount      (int *count);
			bool getParameterProperties ( wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[] );
			bool getParameter           (int index, double *value);
			bool setParameter           (int index, double  value);
			bool isInstrument           ();
			bool sendMidiNotes          (int count,int note[],int velocity[]);
			bool sendMidiNotesOff       ();
			bool sendPolyphonic         (unsigned char polyphonicNote, unsigned char polyphonicValue);
            bool sendController         (unsigned char controllerID, unsigned char controllerValue);
            bool sendProgram            (unsigned char programID);
            bool sendMonophonic         (unsigned char monophonicValue);
            bool sendPitchbend          (unsigned char pitchbendValue);
			bool getInputsCount         (int *count);
			bool getOutputsCount        (int *count);
			bool destroy                ();
            bool getProgramNames        (int *count, wchar_t names[][256]);
			bool getActualProgram       (int *count);
            bool setActualProgram       (int count);

  public  : int blockSize;
			int sampleRate;
			int nInputs;
			int nOutputs;

			VstTimeInfo timeInfo;

  private : VSTPlugin plugin; //one vstplugin per host
			HMODULE   module;

			bool canDo[HOSTCANDOCOUNT];
			char directoryPath[MAX_PATH];
			bool pluginIsInstrument;

  //implementations of the callback-functions
  public : virtual long cbAutomate                     (int index, float value);
	       virtual long cbVersion                      ();
	       virtual long cbCurrentId                    ();
	       virtual long cbIdle                         ();
           virtual long cbPinConnected                 ();
	       virtual long cbGetTime                      (VstIntPtr value);
	       virtual long cbProcessEvents                ();
           virtual long cbIOChanged                    ();
	       virtual long cbSizeWindow                   ();
	       virtual long cbGetSampleRate                ();
	       virtual long cbGetBlockSize                 ();
	       virtual long cbGetInputLatency              ();
	       virtual long cbGetOutputLatency             ();
	       virtual long cbGetCurrentProcessLevel       ();
	       virtual long cbGetAutomationState           ();
	       virtual long cbOfflineStart                 ();  
	       virtual long cbOfflineRead                  ();  
	       virtual long cbOfflineWrite                 ();  
	       virtual long cbOfflineGetCurrentPass        (); 
	       virtual long cbOfflineGetCurrentMetaPass    ();  
	       virtual long cbGetVendorString              (char *str); 
		   virtual long cbGetProductString             (char *str);
		   virtual long cbGetVendorVersion             ();
		   virtual long cbVendorSpecific               ();
	       virtual long cbCanDo                        (char *str);
	       virtual long cbGetLanguage                  ();
	       virtual long cbGetDirectory                 (char *str);
	       virtual long cbUpdateDisplay                ();
	       virtual long cbBeginEdit                    ();
	       virtual long cbEndEdit                      ();
	       virtual long cbOpenFileSelector             ();
	       virtual long cbCloseFileSelector            ();

		   //deprecated
		   virtual long cbWantMidi                     ();
		   virtual long cbSetTime                      ();
		   virtual long cbTempoAt                      ();
		   virtual long cbGetNumAutomatableParameters  ();
		   virtual long cbGetParameterQuantization     ();
		   virtual long cbNeedIdle                     ();
		   virtual long cbGetPreviousPlug              ();
		   virtual long cbGetNextPlug                  ();
		   virtual long cbWillReplaceOrAccumulate      ();
		   virtual long cbSetOutputSampleRate          ();
		   virtual long cbGetOutputSpeakerArrangement  ();
		   virtual long cbSetIcon                      ();
		   virtual long cbOpenWindow                   ();
		   virtual long cbCloseWindow                  ();
		   virtual long cbEditFile                     ();
		   virtual long cbGetChunkFile                 ();
		   virtual long cbGetInputSpeakerArrangement   ();
		   

};

/*************************************************************************/

#endif