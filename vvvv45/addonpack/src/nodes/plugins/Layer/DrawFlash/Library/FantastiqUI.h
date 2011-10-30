///////////////////////////////////////////////////////////////////////////////////
//					FantastiqUI Flash player 
//					2008 version
//	
//					Use the power of Adobe Flash in hardware accelerated
//					enviroments!
//
//					(c) mathijs baaijens
///////////////////////////////////////////////////////////////////////////////////

#include "FUIFlashPlayer.h"
#include "FUIMain.h"

#ifdef FANTASTIQUI_EXPORTS
#define FANTASTIQUI_API __declspec(dllexport)
#else
#ifndef FANTASTIQUI_STATIC
#define FANTASTIQUI_API __declspec(dllimport)
#else
#define FANTASTIQUI_API
#endif
#endif

FANTASTIQUI_API FUIMain* CreateFantastiqUI(void);
FANTASTIQUI_API void DeleteFantastiqUI(FUIMain* p);