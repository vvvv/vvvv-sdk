#include "GUI.h"


void openWnd(AEffect* effect,HWND hwnd)
{
  if(effect == NULL) return;

  ERect tmpRect;

  ERect* eRect = &tmpRect;

  effect->dispatcher( effect, effEditOpen, 0, 0, hwnd, 0); 

  effect->dispatcher(effect,effEditGetRect,0,0,&eRect,0);


  if(eRect)
  {
	int width  = eRect->right  - eRect->left;
	int height = eRect->bottom - eRect->top;

	if(width < 100) 
	width = 100;

	if(height < 100)
	height = 100;

	RECT wRect;

	SetRect(&wRect, 0, 0, width, height);

	AdjustWindowRectEx(&wRect, GetWindowLong(hwnd,GWL_STYLE),FALSE,GetWindowLong(hwnd,GWL_EXSTYLE));

	width  = wRect.right  - wRect.left;
	height = wRect.bottom - wRect.top;

	SetWindowPos (hwnd, HWND_TOP,0,0,width,height,SWP_NOMOVE);
  }

  SetWindowText(hwnd,L"DSVSTWrapper");
  SetTimer(hwnd,1,20,0);
  
}


LRESULT CALLBACK WndProc( HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{ 
  static AEffect* effect = ((VSTPlugin*)lParam)->effect;

  switch(msg)
  {
    case WM_INITDIALOG  : /*((VSTPlugin*)lParam)->openWindow(hwnd);*/
						  
                          break; 
						 
	case WM_PAINT       : break;

	case WM_TIMER       : effect->dispatcher(effect, effEditIdle, 0, 0, 0, 0);
	   
	                      break;

	case WM_LBUTTONDOWN : break;

    case WM_CREATE      : break;

	case WM_COMMAND     : break;

	case WM_CLOSE       : EndDialog(hwnd,IDOK); 
							
						  break;

	case WM_DESTROY     : PostQuitMessage(0);
	                      
	                      break;

  }
  
  return 0;

  //return DefWindowProc(hwnd,msg,wParam,lParam);
  
}


EDITORWINDOW wnd;

DWORD WINAPI windowThread (LPVOID data)
{
  EDITORWINDOW wnd;

  HWND hwnd = (HWND) GetModuleHandle(0);

  wnd.style = WS_POPUPWINDOW | WS_DLGFRAME | DS_MODALFRAME | DS_CENTER;


  __try
  {
    DialogBoxIndirectParam ( (HINSTANCE)hwnd, &wnd, 0,
 	 					   (DLGPROC) WndProc,
						   (LPARAM) data);
  }
  __except(GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION)
  {
    OutputDebugString(L"EXCEPTION_ACCESS_VIOLATION");
  }


  return (DWORD)hwnd;

}