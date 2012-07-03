#ifndef EDITOR_H
#define EDITOR_H

#include <stdio.h>
#include <math.h>
#include <windows.h>
#include "Ctrl.h"
#include "Time.h"
#include "EditorElements.h"
#include "Tracker.h"
#include <math.h>

const int WNDWIDTH     = 1048;
const int WNDHEIGHT    =  458;

const int CANVASLEFT   =   10;
const int CANVASWIDTH  = 1024; 
const int CANVASHEIGHT =  200;
const int CANVASTOP    =   10;
const int CANVAS2TOP   =  218;
const int CANVASFLOOR  =  210;
const int CANVAS2FLOOR =  418;



class Ctrl;


class Editor
{
  private : HWND  hwnd;
		    HDC   hdc;
			RECT  rect;
			PEN   pen;
			TXT   txt; 

			Ctrl *ctrl;

			void drawGeneric (HDC hdc);
			void drawRect    (HDC hdc,int x,int y,int width,int height,int color);
            void drawLine    (HDC hdc,int px,int py,int qx,int qy,int color);
            void drawBar     (HDC hdc,int x,int y,int color);
			void drawBar2    (HDC hdc,int x,int y,int color);

			void drawWaveform  (HDC hdc);
			void drawSpectrum  (HDC hdc);
			void drawOnset     (HDC hdc);
			void drawResonance (HDC hdc);
			void drawBeat      (HDC hdc);

  public :  Editor          ();
	       ~Editor          ();
		    void openWnd    ();
		    void refreshWnd ();
		    //void paint      ();
		    void setHwnd    (HWND hwnd);
		    void setCtrl    (Ctrl *ctrl);
			void setText    (long frameLength,double sampleRate,long blockSize,int bpmHalf,int bpm,int bpmDouble);
			
			Ctrl* getCtrl    ();

            Tracker* tracker;

			void draw   (Tracker *t);

};

#endif