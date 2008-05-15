#include "GUI.h"


LRESULT CALLBACK WndProc( HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{

  switch(msg)
  {
    case WM_INITDIALOG  :   { AEffect *effect = ((AEffect*) lParam);

	                          if(effect) effect->dispatcher( effect, effEditOpen, 0, 0, hwnd, 0); }

                          break; 
						 
	case WM_PAINT       : break;

	case WM_LBUTTONDOWN : break;

    case WM_CREATE      : break;

	case WM_COMMAND     : break;

	case WM_DESTROY     : PostQuitMessage(0);
	                      
	                      OutputDebugString(L"WM_DESTROY\n");
	  
	                      break;

  }

  return 0;

  //return DefWindowProc(hwnd,msg,wParam,lParam);

}


DWORD WINAPI windowThread (LPVOID data)
{
  EDITORWINDOW wnd;

  wnd.style = WS_POPUPWINDOW | WS_DLGFRAME | DS_MODALFRAME | DS_CENTER;
  wnd.cx    = WNDWIDTH  / 2;
  wnd.cy    = WNDHEIGHT / 2;

  ERect   rect;
  ERect  *pRect  = &rect;
  ERect **ppRect = &pRect;

  AEffect* effect = (AEffect*) data;
    
  effect->dispatcher( effect, effEditGetRect, 0, 0, ppRect, 0);

  if(pRect != NULL)
  {
    wnd.cx = (pRect->right  - pRect->left) / 2;
    wnd.cy = (pRect->bottom - pRect->top ) / 2;

	if(wnd.cx > 10 && wnd.cy > 10)
    DialogBoxIndirectParam ( GetModuleHandle(0), &wnd, 0,
 						    (DLGPROC) WndProc,
						    (LPARAM) data);
  }

  return 0;

}