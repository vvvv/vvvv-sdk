#ifndef _GLOBAL_H
#define _GLOBAL_H

#define _CRT_SECURE_NO_DEPRECATE

#include <stdio.h>
#include <windows.h>
#include <math.h>

#define BITSPERBYTE      8
#define BYTESPERSAMPLE   2
#define NCHANNELS        2
#define STRLENGTH      256
#define BLOCKSIZE     2048 //11025 
#define SAMPLERATE   44100
#define WAVESIZE     32768
#define DEBUG            0
#define STEREO           2
#define MIDINOTESCOUNT 128
#define NEWHOST       4747
#define DISCHARGEHOST 4848
#define MAXHOSTCOUNT   256

//deprecated opcodes 
#define audioMasterWantMidi                       6
#define audioMasterSetTime                        9
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

//Plugin + Host CanDos
#define NPLUGINCANDO         8
#define HOSTCANDOCOUNT      14

#define SENDVSTEVENTS        0
#define SENDVSTMIDIEVENT     1
#define RECEIVEVSTEVENTS     2
#define RECEIVEVSTMIDIEVENT  3
#define RECEIVEVSTTIMEINFO   4

//Midi-Messages
#define NOTEOFF           0x80
#define NOTEON            0x90
#define POLYTOUCH         0xA0
#define CONTROLCHANGE     0xB0
#define PROGRAMCHANGE     0xC0
#define MONOTOUCH         0xD0
#define PITCHBEND         0xE0



void out           (wchar_t *str);
void out           (char *str);
void outputString  (char str[],bool endline);
void outputString  (char label[],char str[],bool endline);

#endif






