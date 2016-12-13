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
class FUIMain;

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

class FANTASTIQUI_API FUIMain
{
private:
	void* pCorePointer;
public:
	int CreateUI(
		void* pResizeTexCallback,void* pGetSurfCallback,
		void* pReleaseSurfCallback,char* sFlashControl,void* pDirtyRect,bool bRequireInteraction=true);

	void SetFlashPath(char* fpath);

	FUIFlashPlayer* CreateFlashPlayer();
	void DeleteFlashPlayer(FUIFlashPlayer* pPlayer);
	int LoadFlashHeader(char* file,int* x,int* y,float* fps,int* numframes);
	void SetLicenseKey(int licensetype,char* licensename,char* licensekey);

	FUIMain(void);
	~FUIMain(void);
};
