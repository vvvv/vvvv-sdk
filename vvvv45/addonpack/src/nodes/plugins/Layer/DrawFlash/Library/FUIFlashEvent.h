///////////////////////////////////////////////////////////////////////////////////
//					FantastiqUI Flash player 
//					2008 version
//	
//					Use the power of Adobe Flash in hardware accelerated
//					enviroments!
//
//					(c) mathijs baaijens
///////////////////////////////////////////////////////////////////////////////////

#pragma once
class FUIFlashEvent;

#include "FUIFlashPlayer.h"
#include "FUIMain.h"
#include "FUIFlashEvent.h"

#ifdef FANTASTIQUI_EXPORTS
#define FANTASTIQUI_API __declspec(dllexport)
#else
#ifndef FANTASTIQUI_STATIC
#define FANTASTIQUI_API __declspec(dllimport)
#else
#define FANTASTIQUI_API
#endif
#endif

class FANTASTIQUI_API FUIFlashEvent
{
public:
	void* pCorePointer;
public:
	__wchar_t* GetFSCommand();
	__wchar_t* GetFunctionName();

	__wchar_t* GetValueString(int v);
	int GetValueNumber(int v);
	float GetValueFloat(int v);
	bool GetValueBool(int v);
	int GetValueType(int v);

	int GetNumArguments();
	int GetEventType();
	int GetIntValue();
public:
	FUIFlashEvent(void);
	~FUIFlashEvent(void);
};
