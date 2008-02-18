#ifndef _VSTHOST_H
#define _VSTHOST_H

#define _CRT_SECURE_NO_DEPRECATE

#include <stdio.h>
#include <windows.h>
#include <math.h>
#include "pluginterfaces/vst2.x/aeffectx.h"
#include "Global.h"
#include "GUI.h"
#include "Plugin.h"

//deprecated opcodes
#define audioMasterWantMidi                      6
#define audioMasterSetTime                       9
#define audioMasterTempoAt                       10
#define audioMasterGetNumAutomatableParameters   11
#define audioMasterGetParameterQuantization      12
#define audioMasterNeedIdle                      14 
#define audioMasterGetPreviousPlug               20 
#define audioMasterGetNextPlug                   21  
#define audioMasterWillReplaceOrAccumulate       22  
#define audioMasterSetOutputSampleRate           30  
#define audioMasterGetOutputSpeakerArrangement   31  
#define audioMasterSetIcon                       36  
#define audioMasterOpenWindow                    39  
#define audioMasterCloseWindow                   40  
#define audioMasterEditFile                      47  
#define audioMasterGetChunkFile                  48  
#define audioMasterGetInputSpeakerArrangement    49  


typedef AEffect* (*PluginMain) (audioMasterCallback audioMaster);

static VstIntPtr VSTCALLBACK HostCallback (AEffect *effect,
	  									   VstInt32 opcode,
										   VstInt32 index,
										   VstIntPtr value,
										   void *ptr,
   								           float opt);


class Loader
{
  public  : Loader    ();
           ~Loader    ();
            AEffect* init (char *filename);
			bool     destroy ();
		    
  private : HMODULE    module;
            PluginMain pluginMain;
			AEffect   *effect;
	
};


class Host
{
  public : int blockSize;
		   int sampleRate;

		   char directoryPath[MAX_PATH];

		   VstTimeInfo timeInfo;

		   Plugin *plugin;

		   Loader loader;

		   bool canDo[NUMHOSTCANDOS];
		   
  public : Host ();

		   bool loadPlugin             ( char *filename);
		   bool getParameterNumber     ( int *number);
		   bool getParameterProperties ( wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[] );
		   bool setParameter           ( int index, float value);
		   void process                ( float **in, float **out, int length, int nChannels, int samplesPerSecond );
           bool destroy                ();
		   bool getParameterValue      (int index, double *value);
		   void sendMidiMsg            (unsigned char status, unsigned char note, unsigned char velocity);
		   void sendMidiNotesOff       (unsigned char notesoff);
		   void canDoMidi              (int *can);
		   void sendMidiController     (unsigned char controllerID, unsigned int controllerValue );
		   void getMidiDeviceNames     (int *number, wchar_t name[][256]);

		   virtual long cbAutomate                     (int index, float value);
	       virtual long cbVersion                      ();
	       virtual long cbCurrentId                    ();
	       virtual long cbIdle                         ();
           virtual long cbPinConnected                 ();
	       virtual long cbGetTime                      ();
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

//*****************************************************************************************************//

#endif