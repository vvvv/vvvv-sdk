#ifndef GUI_H
#define GUI_H

#include "vstgui.h"
#include "vstcontrols.h"
#include "Global.h"
#include "Signal.h"

#define GUIHEIGHT 190
#define GUIWIDTH  600

#define OFFSETY        16
#define OFFSETXBUTTON 482
#define SLIDERLENGTH  148
#define SLIDERWIDTH    18



class GUI : public AEffGUIEditor, public CControlListener
{
  private : HWND hwnd;

			CBitmap *bmpBackground;
            CBitmap *bmpDelayTargetBpm;
            CBitmap *bmpProbabilitySilence;
			CBitmap *bmpOnsetBeatPhase;
			CBitmap *bmpButtonsBpm;
			CBitmap *bmpResonance;

			CBitmap *bmpSignal;
			CBitmap *bmpAdjust;
			CBitmap *bmpReset;

			CBitmap *bmpSlider;
            CBitmap *bmpSliderBackground;
			CBitmap *bmpSliderDelayBackground;

			COnOffButton *buttonSignal;
			COnOffButton *buttonAdjust;
			COnOffButton *buttonReset;

			CVerticalSlider *sliderDelay;
			CVerticalSlider *sliderTargetBpm;

            CDrawContext *context;

			CColor colorBackground;

  public  : GUI(AudioEffect *effect,Ctrl *ctrl);
            
			virtual ~GUI();
		   
            virtual bool open                   (void *ptr);
			virtual void close                  ();
			virtual void idle                   ();
			        void update                 ();   
			virtual void setParameter           (VstInt32 index,float value);
			virtual void valueChanged           (CDrawContext *context,CControl *control);
		      	    void drawDelayTargetBpm     ();
   			        void drawProbabilitySilence ();
		 	        void drawOnsetBeatPhase     ();
			        void drawButtonsBpm         ();
					void drawResonance          ();
					void drawRect               (CDrawContext *context,int x, int y, int width,int height,CColor color);
			        void drawLine               (CDrawContext *context,int px,int py,int qx,int qy,CColor color);
			        void drawString             (CDrawContext *context,char str[],int x,int y,int width,int height);
					void drawStringGrey         (CDrawContext *context,char str[],int x,int y,int width,int height);

			COffscreenContext* beginOC(CBitmap *bmp);
            
			void endOC(COffscreenContext *oc,CBitmap *bmp,int offsetX,int offsetY);
			
			Ctrl *ctrl;

};

#endif