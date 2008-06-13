#ifndef _VSTHOST_H
#define _VSTHOST_H

#define _CRT_SECURE_NO_DEPRECATE

#include <stdio.h>
#include <windows.h>
#include <math.h>
#include "pluginterfaces/vst2.x/aeffectx.h"
#include "VSTPlugin.h"
#include "shlobj.h"


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

struct MidiNote
{
 int    note;
 int    velocity;
 long   time;
};

struct MidiNoteBuffer
{
  MidiNote midiNotes[MIDINOTESCOUNT];

  int count;

  MidiNoteBuffer();

  void fill(int note,int velocity);

  void reset();  
};


class VSTHost
{
  public  : VSTHost      ();
		   ~VSTHost      ();

		   	bool process (float **in, float **out,int length);
		 
			//IDSVSTWrapper-Interface-Definitions
			bool load                   (char * filename);	
			
			bool getParameterCount      (int *count);
			bool getParameterProperties ( wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[] );
			bool getParameter           (int index, double *value);
			bool setParameter           (int index, double  value);
			
			bool getMidiIsInstrument    ();
			bool sendMidiNote           (int count,int note[],int velocity[]);
			bool sendMidiNoteAllOff     ();
			bool sendMidiPolyphonic     (unsigned char polyphonicNote, unsigned char polyphonicValue);
            bool sendMidiController     (unsigned char controllerID, unsigned char controllerValue);
            bool sendMidiProgram        (unsigned char programID);
            bool sendMidiMonophonic     (unsigned char monophonicValue);
            bool sendMidiPitchbend      (unsigned char pitchbendValue);
			
			bool getInputCount          (int *count);
			bool getOutputCount         (int *count);
            bool getProgramNames        (int *count, wchar_t names[][256]);
			bool getActualProgram       (int *count);
            bool setActualProgram       (int count);
			bool setBpm                 (int value);
			bool getHasWindow           ();
			bool setWindowHandle        (HWND hwnd);
			bool getWindowSize          (int *width,int *height);
			bool setWindowIdle          ();
            bool destroy                ();

			void updateTime             ();
			void sendMidiBuffer         (int length);
			void updateTimeSamplePos    (int length);

			
  public  : int blockSize;
			int sampleRate;
			int nInputs;
			int nOutputs;

			bool midi;

			MidiNoteBuffer midiNoteBuffer;

			VstTimeInfo timeInfo;

            VSTPlugin plugin; //one vstplugin per host
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
	       virtual long cbOpenFileSelector             (VstFileSelect * fileSelect);
	       virtual long cbCloseFileSelector            (VstFileSelect * fileSelect);

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

class HostList
{
  public : VSTHost *host[MAXHOSTCOUNT];
		   int count;

		   HostList ();
		   VSTHost*  retrieve  (AEffect *effect);
		   void      init      (VSTHost *newHost);
		   void      discharge (VSTHost *oldHost);

};

/*************************************************************************/

#endif