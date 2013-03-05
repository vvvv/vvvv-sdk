
#ifndef _VSTHOST_H
#define _VSTHOST_H

#define _CRT_SECURE_NO_DEPRECATE

#include <windows.h>
#include <iostream>
#include <stdio.h>
#include <string.h>
#include "aeffectx.h"
#include "Global.h"
#include "VSTPlugin.h"
#include "Midi.h"

class VSTPlugin;

//plugin to host communication
static VstIntPtr VSTCALLBACK HostCallback ( AEffect *effect,
										    VstInt32 opcode,
											VstInt32 index,
											VstIntPtr value,
											void *ptr,
											float opt );

//entry-point to the plugin
typedef AEffect* (*PluginMain) (audioMasterCallback);


class VSTHost
{
  private : VSTPlugin   plugin;
            Midi        midi;
			VstEvents  *vstEvents;
            HMODULE     module;
			VstTimeInfo timeInfo;

			int blockSize;
			int sampleRate;

			void sendMidi    ();
			void updateTime1 ();
			void updateTime2 (int nFrames);

   public :	VSTHost ();
		   ~VSTHost ();

			bool process(float **in,float **out,int length);

			AEffect* getEffect ();
			int  getNumInputs  ();
			int  getNumOutputs ();
			
			//IDSVSTWrapper...
			bool load                   (char * filename);	
			
			bool getParameterCount      (int *count);
			bool getParameterProperties (wchar_t paramDisplay[][256], wchar_t paramName[][256], wchar_t paramLabel[][256], double paramValue[] );
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
            
			//AudioMaster...
			long version                ();
			long getTime                ();
			long processEvents          ();
			long getVendorString        (char *str);
			long getVendorVersion       ();
			long getProductString       (char *str);
			long getVendorSpecific      ();
			long getSampleRate          ();
			long getBlockSize           ();
			long canDo                  (char *str);
	        long getDirectory           ();
			long openFileSelector       (VstFileSelect * fileSelect);
	        long closeFileSelector      (VstFileSelect * fileSelect);
			long needIdle               ();   
			long wantMidi               ();
			
};


//Class to manage all of the active hosts 
//One plugin belongs to one host, so in the HostCallback-Function it has 
//to be differentiated to which host the msg is send
class HostList
{
  public  : HostList ();

		    VSTHost* retrieve  (AEffect *effect);
		    void     init      (VSTHost *newHost);
		    void     discharge (VSTHost *oldHost);


  private : VSTHost *vstHost[MAXHOSTCOUNT];
		    int count;

};


#endif