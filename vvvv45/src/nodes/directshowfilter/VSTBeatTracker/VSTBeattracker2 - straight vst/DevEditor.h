#ifndef DEVEDITOR_H
#define DEVEDITOR_H

#include "vstgui.h"
#include "vstcontrols.h"
#include "Global.h"
#include "Signal.h"


#define WNDHEIGHT     512
#define WNDWIDTH     1048
#define AREAWIDTH    1024
#define AREAHEIGHT    200
#define BUTTONPLAY      0
#define BUTTONSHIFT     1
#define BUTTONTAP       2
#define BUTTONBANDA     3
#define BUTTONBANDB     4
#define BUTTONINTERVALA 5
#define BUTTONINTERVALB 6
#define OFFSETX        12


class Display : public CView
{
  public : 	CColor color[6];

			CDrawContext *context;
			CFrame       *frame;

			CColor colorBackground;

			Signal *signal;

			bool tapField[NSAMPLES];

            Display (const CRect& size);

			void idle          ();
			void drawResonance ();
			void drawOnset     ();
			void drawGeneric   ();

			void drawString    (CDrawContext *context,char str[],int x,int y,int width,int height,CColor color);
			void drawFrame     (int x,int y,int width,int height,CColor color);
			void drawRect      (int x,int y,int width,int height,CColor color);
			void drawLine      (CDrawContext *context,int px, int py, int qx, int qy, CColor color);

};

class DevEditor : public AEffGUIEditor, public CControlListener
{
  private : HWND hwnd;

			CBitmap *background;
		
			CVerticalSlider *fader;
			COnOffButton    *buttonPlay;
			COnOffButton    *buttonShift;
			COnOffButton    *buttonTap;
			COnOffButton    *buttoNBANDSA;
			COnOffButton    *buttoNBANDSB;
			COnOffButton    *buttonIntervalA;
			COnOffButton    *buttonIntervalB;

  public  : DevEditor(AudioEffect *effect);
            
			virtual ~DevEditor();
		   
            virtual bool open         (void *ptr);
			virtual void close        ();
			virtual void idle         (Signal *signal);
			virtual void setParameter (VstInt32 index,float value);
			virtual void valueChanged (CDrawContext *context,CControl *control);

			Display *display;

			bool play;
			bool shift;

};

#endif