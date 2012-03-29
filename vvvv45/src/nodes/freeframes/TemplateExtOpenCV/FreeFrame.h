#ifdef LINUX
extern "C" {
#include <string.h>
#include <stdlib.h>
#endif
///////////////////////////////////////////////////////////////////////////////////
// FreeFrame.h
//
// FreeFrame Open Video Plugin Prototype
// ANSI C Version

// www.freeframe.org
// marcus@freeframe.org

/*
Copyright (c) 2002, Marcus Clements www.freeframe.org
All rights reserved.

FreeFrame 1.0 upgrade by Pete Warden
www.petewarden.com

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

   * Redistributions of source code must retain the above copyright
     notice, this list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright
     notice, this list of conditions and the following disclaimer in
     the documentation and/or other materials provided with the
     distribution.
   * Neither the name of FreeFrame nor the names of its
     contributors may be used to endorse or promote products derived
     from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

/*
freeframe 1.0 extended. see: http://vvvv.org/tiki-index.php?page=FreeFrameExtendedSpecification

the following functions have been introduced:
setThreadLock:        function code = 19
getNumOutputs:        function code = 20
getOutputName:        function code = 21
getOutputType:        function code = 22
getOutputSliceCount:  function code = 23
getOutput:            function code = 24

setInput:             function code = 30
*/


#ifndef __FREEFRAME_H__
#define __FREEFRAME_H__

//////////////////////////////////////////////////////////////////////////////////
//
// includes
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifdef WIN32
#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers
#include <windows.h>

#elif defined LINUX

//typedefs for linux - in windows these are defined in files included by windows.h
typedef unsigned int DWORD;
typedef void * LPVOID;
typedef char BYTE;

#endif




/////////////////////////////////////////////////////////////////////////////////////////
// #defines

#define FF_EFFECT				0
#define FF_FAIL					0xFFFFFFFF
#define FF_SUCCESS				0
#define FF_TRUE					1
#define FF_FALSE				0

#define FF_GETINFO				0
#define FF_INITIALISE			1
#define FF_DEINITIALISE			2
#define FF_PROCESSFRAME			3
#define FF_GETNUMPARAMETERS		4
#define FF_GETPARAMETERNAME		5
#define FF_GETPARAMETERDEFAULT	6
#define FF_GETPARAMETERDISPLAY	7
#define FF_SETPARAMETER			8
#define FF_GETPARAMETER			9
#define FF_GETPLUGINCAPS		10
#define FF_INSTANTIATE			11
#define FF_DEINSTANTIATE		12
#define FF_GETEXTENDEDINFO		13
#define FF_PROCESSFRAMECOPY		14
#define FF_GETPARAMETERTYPE		15

#define FF_SETTHREADLOCK		19
#define FF_GETNUMOUTPUTS		20
#define FF_GETOUTPUTNAME   		21
#define FF_GETOUTPUTTYPE   		22
#define FF_GETOUTPUTSLICECOUNT 	23
#define FF_GETOUTPUT        	24
#define FF_SETINPUT           	30

#define FF_CAP_16BITVIDEO		0
#define FF_CAP_24BITVIDEO		1
#define FF_CAP_32BITVIDEO		2
#define FF_CAP_PROCESSFRAMECOPY	3
#define FF_CAP_MINIMUMINPUTFRAMES	10
#define FF_CAP_MAXIMUMINPUTFRAMES	11
#define FF_CAP_COPYORINPLACE		15

#define FF_CAP_PREFER_NONE			0
#define FF_CAP_PREFER_INPLACE		1
#define FF_CAP_PREFER_COPY			2
#define	FF_CAP_PREFER_BOTH			3

#define FF_TYPE_BOOLEAN				0
#define FF_TYPE_EVENT				1
#define FF_TYPE_RED					2
#define FF_TYPE_GREEN				3
#define FF_TYPE_BLUE				4
#define FF_TYPE_XPOS				5
#define FF_TYPE_YPOS				6
#define FF_TYPE_STANDARD			10
#define FF_TYPE_TEXT				100

//freeframe 1.0 extended. see: http://vvvv.org/documentation/freeframeextendedspecification
#define FF_TYPE_VALUESPREAD			20
#define FF_TYPE_COLORSPREAD			21
#define FF_TYPE_STRINGESPREAD		22


/////////////////////////////////////////////////////////////////////////////////////////
//
// FreeFrame types

#if TARGET_OS_MAC
typedef unsigned int DWORD;
typedef unsigned char BYTE;
#endif // TARGET_OS_MAC

typedef struct PlugInfoStructTag {
	DWORD	APIMajorVersion;
	DWORD	APIMinorVersion;
	BYTE	uniqueID[4];			// 4 chars uniqueID - not null terminated
	BYTE	pluginName[16];			// 16 chars plugin friendly name - not null terminated
	DWORD	pluginType;				// Effect or source
} PlugInfoStruct;

typedef struct PlugExtendedInfoStructTag {
	DWORD PluginMajorVersion;
	DWORD PluginMinorVersion;
	char* Description;
	char* About;
	DWORD FreeFrameExtendedDataSize;
	void* FreeFrameExtendedDataBlock;
} PlugExtendedInfoStruct;

typedef struct VideoInfoStructTag {
	DWORD frameWidth;				// width of frame in pixels
	DWORD frameHeight;				// height of frame in pixels
	DWORD bitDepth;					// enumerated indicator of bit depth of video
									// 0 = 16 bit 5-6-5   1 = 24bit packed   2 = 32bit
} VideoInfoStruct;

typedef struct ProcessFrameCopyStructTag {
	DWORD numInputFrames;
	void** InputFrames;
	void* OutputFrame;
} ProcessFrameCopyStruct;

typedef struct SetParameterStructTag {
	DWORD index;
	float value;
} SetParameterStruct;

typedef union plugMainUnionTag {
	DWORD ivalue;
	float fvalue;
	VideoInfoStruct *VISvalue;
	PlugInfoStruct *PISvalue;
	char *svalue;
} plugMainUnion;

///////////////////////////////////////////////////////////////////////////////////////
// Function prototypes
//

///////////////////////////////////////////////////////////////////////////////////////
// plugMain - The one and only exposed function
// parameters:
//	functionCode - tells the plugin which function is being called
//  pParam - 32-bit parameter or 32-bit pointer to parameter structure
//
// PLUGIN DEVELOPERS:  you shouldn't need to change this function
//
// All parameters are cast as 32-bit untyped pointers and cast to appropriate
// types here
//
// All return values are cast to 32-bit untyped pointers here before return to
// the host
//

#ifdef WIN32

BOOL APIENTRY DllMain( HANDLE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved );

__declspec(dllexport) void* __stdcall plugMain(DWORD functionCode, LPVOID pParam, DWORD reserved);

typedef __declspec(dllimport) void* (__stdcall *FF_Main_FuncPtr)(DWORD,LPVOID,DWORD);

#elif LINUX
plugMainUnion plugMain( DWORD functionCode, LPVOID pParam, DWORD reserved);

#elif TARGET_OS_MAC

typedef void* (*FF_Main_FuncPtr)(DWORD,void*,DWORD);

#endif


#endif
#ifdef LINUX
}
#endif




