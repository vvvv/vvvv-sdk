#ifndef _GLOBAL_H
#define _GLOBAL_H

#define _CRT_SECURE_NO_DEPRECATE

#include <stdio.h>
#include <windows.h>
#include <math.h>

#define STRLENGTH    256
#define DEBUG          0
#define BLOCKSIZE  11025 
#define SAMPLERATE 44100
#define OPENWINDOW     1
#define NUMMIDINOTES 128


//Plugin + Host CanDos
#define SENDVSTEVENTS                     0
#define SENDVSTMIDIEVENT                  1
#define RECEIVEVSTEVENTS                  2
#define RECEIVEVSTMIDIEVENT               3
#define RECEIVEVSTTIMEINFO                4

//Plugin exclusive
#define OFFLINE                           5
#define MIDIPROGRAMNAMES                  6
#define BYPASS                            7

//Host exclusive
#define REPORTCONNECTIONCHANGES           5
#define ACCEPTIOCHANGES                   6
#define SIZEWINDOW                        7
#define OFFLINE                           8 
#define OPENFILESELECTOR                  9
#define CLOSEFILESELECTOR                10
#define STARTSTOPPROCESS                 11
#define SHELLCATEGORY                    12
#define SENDVSTMIDIEVENTFLAGISREALTIME   13

//Midi-Messages
#define NOTEOFF        0x80
#define NOTEON         0x90
#define POLYTOUCH      0xA0
#define CONTROLCHANGE  0xB0
#define PROGRAMCHANGE  0xC0
#define MONOTOUCH      0xD0
#define PITCHBEND      0xE0


#define NUMPLUGINCANDOS  8
#define NUMHOSTCANDOS   14



void out           (wchar_t *str);
void outputString  (char str[],bool endline);
void outputString  (char label[],char str[],bool endline);

#endif






