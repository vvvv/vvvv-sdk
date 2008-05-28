#include "GUI.h"


LRESULT CALLBACK WndProc( HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
  static AEffect* effect = (AEffect*) lParam;

  switch(msg)
  {
    case WM_INITDIALOG  : { SetWindowText(hwnd,L"VST");
							SetTimer(hwnd,1,20,0);

							if(effect) 
							{
							  effect->dispatcher( effect, effEditOpen, 0, 0, hwnd, 0); 

							  ERect* eRect = 0;

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

								SetRect(&wRect, 0, height, width, 0);

								AdjustWindowRectEx(&wRect, GetWindowLong(hwnd,GWL_STYLE),FALSE,GetWindowLong(hwnd,GWL_EXSTYLE));

								width  = wRect.right - wRect.left;
								height = wRect.top   - wRect.bottom;

								SetWindowPos (hwnd, HWND_TOP,0,0,width,height,SWP_NOMOVE);
							  }

							}//end if effect

						  }

                          break; 
						 
	case WM_PAINT       : break;

	case WM_TIMER       : if(effect) 
							effect->dispatcher(effect, effEditIdle, 0, 0, 0, 0);

	                      break;

	case WM_LBUTTONDOWN : break;

    case WM_CREATE      : break;

	case WM_COMMAND     : break;

	case WM_CLOSE       : { KillTimer(hwnd,1);

	                        if(effect) 
							 effect->dispatcher(effect, effEditClose, 0, 0, 0, 0);

							EndDialog(hwnd,IDOK); 
						  }

						  break;

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

  HWND hwnd = (HWND) GetModuleHandle(0);

  wnd.style = WS_POPUPWINDOW | WS_DLGFRAME | DS_MODALFRAME | DS_CENTER;

  DialogBoxIndirectParam ( (HINSTANCE)hwnd, &wnd, 0,
 						   (DLGPROC) WndProc,
						   (LPARAM) data);

  return (DWORD)hwnd;

}