
#ifndef _GLOBAL_H
#define _GLOBAL_H

#define BLOCKSIZE     2048
#define SAMPLERATE   44100
#define STRLENGTH      256
#define WAVESIZE     16384 //32768? 
#define BITSPERBYTE      8
#define TIMEUNIT  10000000 //Nanoseconds / 100
#define MIDION           0 //use internal midi-callback
#define MAXHOSTCOUNT   128
#define NEWHOST       4747
#define DISCHARGEHOST 4848
#define MAXDEVICE       16
#define STRLEN         256
#define MSGLEN          64
#define SYSEXLEN       256
#define NMSG           256
#define MAXPROGRAM     256
#define NMIDINOTES     128

//Midi-Messages
#define NOTEOFF       0x80
#define NOTEON        0x90
#define POLYTOUCH     0xA0
#define CONTROLCHANGE 0xB0
#define PROGRAMCHANGE 0xC0
#define MONOTOUCH     0xD0
#define PITCHBEND     0xE0

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

#define effIdle                                  53


#endif



