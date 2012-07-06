
#ifndef _MIDI_H
#define _MIDI_H

#include <stdio.h>
#include <windows.h>
#include <conio.h>
#include <mmsystem.h>
#include "Global.h"


bool midiErrorCheck(MMRESULT result);

//*********************************************************************************************************************//

class MidiMsg
{
  public : MIDIHDR header;
		   char data[MSGLEN];
		   unsigned char sysEx[SYSEXLEN];

		   MidiMsg();

};

//*********************************************************************************************************************//

class MidiInDevice
{
  public : int        id;
		   MIDIINCAPS caps;
		   HMIDIIN    handle;
};

//*********************************************************************************************************************//

//Actually only used to store the midi-msgs (MIDION = 0), but could be used to receive 
//msgs directly from midi-devices
class Midi
{
  public : MidiInDevice midiInDevice;

		   MidiMsg midiMsg;

		   MidiMsg msgBuffer[NMSG];

		   int  count;
		   bool open;
		   
		   Midi ();
		  ~Midi ();

		   bool openDevice  ();
		   bool closeDevice ();

		   void setMsg(unsigned int status, unsigned int note, unsigned int velocity);
	
		   static void __stdcall MidiInCallback(HMIDIIN midiDevice,UINT msg,DWORD dwInstance,DWORD dwParam1,DWORD dwParam2);

};

#endif