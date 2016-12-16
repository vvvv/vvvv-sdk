#include "Editor.h"


INT_PTR CALLBACK editorCallback (HWND hwnd,
								 UINT msg,
								 WPARAM wParam,
								 LPARAM lParam)
{
    static Editor *editor;
	static Ctrl *ctrl;

	static int mxOld = 0;
	int mx;
	int my;
    

	switch(msg)
	{
	   case WM_INITDIALOG :  editor=(Editor*)lParam;
		                     editor->setHwnd(hwnd);
							 ctrl = editor->getCtrl();

							 if(ctrl!=NULL) printf("CTRL initialized\n");

		                     break;

       case WM_RBUTTONDOWN :  mx = LOWORD(lParam);
		                      my = HIWORD(lParam);

							  printf("MX %4d MY %4d DISTANCE %4d\n",mx-CANVASLEFT,my,(int)(sqrt((double)(mx-mxOld)*(double)(mx-mxOld)))); 

  							  mxOld = mx;

							  break;


	   case WM_KEYUP : switch(wParam)
					   {
	                     case VK_RETURN : ctrl->stop = !ctrl->stop; break;

						 case VK_SPACE  : ctrl->play = true;        break;
					   }

					   break;

	   case WM_CLOSE : PostQuitMessage (0); break;

	}

	return 0;

}

//-------------------------------------------------------------------------//

Editor::Editor()
{

}

Editor::~Editor()
{
  pen.destroy();
}

void Editor::setCtrl(Ctrl *ctrl)
{
  this->ctrl=ctrl;
}

Ctrl* Editor::getCtrl()
{
  return ctrl;
}

void Editor::setHwnd(HWND hwnd)
{
  this->hwnd = hwnd;
  SetWindowText(hwnd,"vvvvBeattracker");
  //paint();
   
}

void Editor::draw(Tracker *t)
{
  tracker = t;

  HDC firstHdc = GetDC(hwnd);
  HDC hdc = CreateCompatibleDC(firstHdc);
  HBITMAP bitmap = CreateCompatibleBitmap(firstHdc,WNDWIDTH,WNDWIDTH);
  HBRUSH brush = CreateSolidBrush(COLORREF(0x00777777));

  SelectObject(hdc,bitmap);
 
  //refresh background----------------------------------------------//      
  SetRect(&rect,0,0,WNDWIDTH,WNDHEIGHT);
  FillRect(hdc,&rect,brush);

  //draw on the backbuffer------------------------------------------//
  drawGeneric   (hdc);
  //drawWaveform  (hdc);
  //drawSpectrum  (hdc);
  drawOnset     (hdc);
  drawResonance (hdc);
  //drawBeat      (hdc);

  //blit and delete-------------------------------------------------//
  BitBlt(firstHdc,0,0,WNDWIDTH,WNDHEIGHT,hdc,0,0,SRCCOPY);

  DeleteObject (bitmap);
  DeleteObject (brush);

  DeleteDC (hdc);
  DeleteDC (firstHdc);

  bitmap   = NULL;
  brush    = NULL;
  hdc      = NULL;
  firstHdc = NULL;
  
}

void Editor::drawGeneric(HDC hdc)
{
  //Canvas----------------------------------------------------------------------------//
  drawRect(hdc,CANVASLEFT-1, CANVASTOP-1,CANVASWIDTH+1,CANVASHEIGHT+2,LIGHT);
  drawRect(hdc,CANVASLEFT-1,CANVAS2TOP-1,CANVASWIDTH+1,CANVASHEIGHT+2,LIGHT);

  SetTextColor(hdc,COLORREF(0x00AAAAAA));
  SetBkColor(hdc,COLORREF(0x00777777));

}

void Editor::drawBeat(HDC hdc)
{
  int channel = 0; //tracker->beatDetection.channel;

  double factor = 10;

  double *signal = tracker->signal.onset[channel];

  //for(int i=0;i<NFIELD;i++)
  //{
  //  int height = (int) (signal[i] * factor);

  //  if(height>CANVASHEIGHT) height = CANVASHEIGHT;

  //  drawBar2 (hdc,i, height, LIGHT);
  //}
  
  signal = tracker->beatDetection.beat[channel].peak;

  for(int i=0;i<NFIELD;i++)
  {
    int height = (int) (signal[i] * factor);

    if(height>CANVASHEIGHT) height = CANVASHEIGHT;

    drawBar2 (hdc,i, height, WHITE);
  }


  for(int a=0;a<NAGENT;a++)
  if(tracker->beatDetection.beat[channel].agent[a].active)
  {
    BASE *base = tracker->beatDetection.beat[channel].agent[a].base;

	int n  = tracker->beatDetection.beat[channel].agent[a].n; 

	for(int i=0;i<n;i++)
	{
	  int index = tracker->beatDetection.beat[channel].agent[a].at(i);

	  //printf("BASE %d  POS %d VALUE %f\n",index,base[i].pos,base[i].value);

	  int color = RED + a%5;

	  if(a==tracker->beatDetection.beat[channel].id) color = BLACK;

	  if(base[index].valid)
  	    drawBar2 (hdc,base[index].pos, base[index].value * factor, color);

	}

  }//end for a-------------------------------------------------------------//


  for(int i=0;i<NFIELD;i++)
  {
	int color = RED + tracker->beatDetection.beat[channel].cover[i].id % 5;

	if(tracker->beatDetection.beat[channel].cover[i].id == tracker->beatDetection.beat[channel].id) color = BLACK;

    drawBar2(hdc, i, tracker->beatDetection.beat[channel].cover[i].value*-5, color);
  }

  if(tracker->beatDetection.beat[channel].beep) 
  drawBar2 (hdc, 0, -16, WHITE);

  drawBar2 (hdc, START, -16, WHITE);
  drawBar2 (hdc, ENTER, -16, WHITE);
    
}


void Editor::drawWaveform(HDC hdc)
{
  double *signal = tracker->signal.in;

  for(int i=1;i<NSAMPLES;i++)
  this->drawLine(hdc,(i-1)*2,(int) (signal[i-1] * 200),i*2,(int) (signal[i] * 200),DARK);
}

void Editor::drawSpectrum(HDC hdc)
{
  double *signal  = tracker->signal.freq;

  for(int i=0;i<NSAMPLES/2;i++)
  {
   int height = (int) (signal[i] * 4000);

   if(height>CANVASHEIGHT) height = CANVASHEIGHT;

   drawBar(hdc,i*4,   height,LIGHT);
   drawBar(hdc,i*4+1, height,LIGHT);
   drawBar(hdc,i*4+2, height,LIGHT);
   drawBar(hdc,i*4+3, height,LIGHT);
  }
}

void Editor::drawOnset(HDC hdc)
{
  double *signal = tracker->signal.onset[0];//tracker->signal.onset[tracker->beatDetection.channel];

  for(int i=0;i<1024;i++)
  {
    int height = (int) (signal[i] * 40);

    if(height>CANVASHEIGHT) height = CANVASHEIGHT;

    drawBar2 (hdc,i, height, LIGHT);
  }

}

void Editor::drawResonance(HDC hdc)
{
  for(int a=0;a<NCHANNEL;a++)
  {
	double *signal = tracker->signal.resonance[a];

	for(int i=0;i<NRESONANCE;i++)
	drawBar(hdc,i+a*200,   (int) (signal[i] * 120),LIGHT);


	for(int i=0;i<NRESONANCE;i++)
	drawBar(hdc,i+a*200,   (int) (signal[i] * 120),LIGHT);

	signal = tracker->interval.chainBuilder[a].getPeak();

	int interval = tracker->signal.interval[a];

	drawBar(hdc,a*200-1,201,LIGHT);

	//MINMAXINTERVAL
	drawBar(hdc,a*200-1 + tracker->signal.minInterval,201,LIGHT);
	drawBar(hdc,a*200-1 + tracker->signal.maxInterval,201,LIGHT);

	for(int i=0;i<NRESONANCE;i++)
	{
	  if(i!=interval) 
	   drawBar(hdc,i+a*200,   (int) (signal[i] * 120),WHITE);
	  else
	   drawBar(hdc,i+a*200,   (int) (signal[i] * 120),PINK);
	}

  }//end for a

}

void Editor::openWnd()
{
  EDITORWINDOW wnd;

  wnd.style = WS_POPUPWINDOW|WS_DLGFRAME|DS_MODALFRAME|DS_CENTER;
  wnd.cx    = WNDWIDTH /2;
  wnd.cy    = WNDHEIGHT/2;
	   
  DialogBoxIndirectParam ( GetModuleHandle(0), &wnd,0,
  						   (DLGPROC)editorCallback,
						   (LPARAM)this);

}

void Editor::refreshWnd()
{
  ::PostMessage(hwnd,WM_COMMAND,0,0);

}

void Editor::drawLine(HDC hdc,int px,int py,int qx,int qy,int color)
{
  pen.setColor(hdc,color);
  //MoveToEx(hdc,0,0,NULL);
  //LineTo(hdc,512,512);
   MoveToEx(hdc,CANVASLEFT+px,CANVASFLOOR/2-py,NULL);
   LineTo  (hdc,CANVASLEFT+qx,CANVASFLOOR/2-qy);
  pen.resetColor(hdc);
}

void Editor::drawBar(HDC hdc,int x,int y,int color)
{
  pen.setColor(hdc,color);
  MoveToEx(hdc,CANVASLEFT+x,CANVASFLOOR,NULL);
  LineTo(hdc,CANVASLEFT+x,CANVASFLOOR-y);
  pen.resetColor(hdc);
}

void Editor::drawBar2(HDC hdc,int x,int y,int color)
{
  pen.setColor(hdc,color);
  MoveToEx(hdc,CANVASLEFT+x,CANVAS2FLOOR,NULL);
  LineTo(hdc,CANVASLEFT+x,CANVAS2FLOOR-y);
  pen.resetColor(hdc);
}


void Editor::drawRect(HDC hdc,int x,int y,int width,int height,int color)
{
  pen.setColor(hdc,color);
    
  MoveToEx(hdc,x,y,NULL);
  LineTo(hdc,x+width,y);

  MoveToEx(hdc,x+width,y,NULL);
  LineTo(hdc,x+width,y+height);

  MoveToEx(hdc,x+width,y+height,NULL);
  LineTo(hdc,x,y+height);

  MoveToEx(hdc,x,y+height,NULL);
  LineTo(hdc,x,y);

  pen.resetColor(hdc);

}



