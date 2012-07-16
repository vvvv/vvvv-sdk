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
class FUIFlashPlayer;

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

class FANTASTIQUI_API FUIFlashPlayer
{
public:
	void* pCorePointer;
public:
	int CreateFlashControl(int iTransparency,int iSizeX,int iSizeY,void* pTexture1=NULL,void* pTexture2=NULL,bool b2NTextures=false,void* pUniqueId=NULL);
	int LoadMovie(__wchar_t* sMovie);
	int Resize(int iNewSizeX,int iNewSizeY);

	//input
	void UpdateMouseButton(int iButton,bool bPressed);
	void UpdateMousePosition(int iX, int iY);
	void DoCopy();
	void DoPaste();
	void DoSelectAll();
	void DoCut();
	void DoMouseWheel(int iScroll);
	void SendKey(bool bDown,int iVirtualKey,int iExtended);
	void SendChar(int iChar,int iExtended);
	void SendUniChar(int iChar,int iExtended);

	//texture
	void* GetTexture();
	void ReleaseTexture();

	//actionscript
	void PrepareFunctionCall(__wchar_t *funcname);
	void PushStringArgument(__wchar_t *arg);
	void PushNumberArgument(float arg);
	void PushBoolArgument(bool arg);
	bool FinishCall(bool bReturnArgument);
	void PrepareReturn();
	void ReturnArguments();
	//void SetVariable(__wchar_t* name,__wchar_t* value);
	//__wchar_t* GetVariable(__wchar_t* name);

	//events
	int GetNumEvents();
	FUIFlashEvent* GetEvent(int iNum);
	void DeleteEvent(FUIFlashEvent* p);
	void ClearEvents();
	void SetEventNotifier(void* notifier);
	void SetFlashSettings(bool localsecurity,int allownetworking,bool allowscriptaccess,int backgroundcolor);
	void SetFrameTime(float frametime);

	void DisableFlashRendering(bool bDisable);
	void EnableFlashCursors(bool bEnable);

	FUIFlashPlayer(void);
	~FUIFlashPlayer(void);
};
