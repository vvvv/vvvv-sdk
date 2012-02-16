#include "GUI.h"


GUI::GUI(AudioEffect *effect,Ctrl *ctrl) : AEffGUIEditor(effect)
{
  rect.left   = 0;
  rect.top    = 0;
  rect.right  = GUIWIDTH;
  rect.bottom = GUIHEIGHT;

  bmpBackground          = new CBitmap(300);
  bmpDelayTargetBpm      = new CBitmap(301);
  bmpProbabilitySilence  = new CBitmap(302);
  bmpOnsetBeatPhase      = new CBitmap(303);
  bmpButtonsBpm          = new CBitmap(304);
  
  bmpSignal = new CBitmap(305);
  bmpAdjust = new CBitmap(306);
  bmpReset  = new CBitmap(307);
  bmpSlider = new CBitmap(308);

  bmpSliderBackground      = new CBitmap(309);
  bmpSliderDelayBackground = new CBitmap(309);

  bmpResonance = new CBitmap(310);

  context = NULL;
  frame   = NULL;

  colorBackground.red   = 153;
  colorBackground.green = 153;
  colorBackground.blue  = 153;

  this->ctrl = ctrl;
}

GUI::~GUI()
{

}

bool GUI::open(void *ptr)
{
  AEffGUIEditor::open(ptr); 


  CRect size(0,0,bmpBackground->getWidth(),bmpBackground->getHeight());


  CFrame *frame = new CFrame(size, ptr, this);

  frame->setBackground(bmpBackground);

  context = frame->createDrawContext();

  //---------------------------------//
  
  size(OFFSETXBUTTON,OFFSETY,OFFSETXBUTTON+bmpSignal->getWidth(),OFFSETY+bmpSignal->getHeight()/2);

  buttonSignal = new COnOffButton(size,this,PARAM_SIGNAL,bmpSignal);

  buttonSignal->setTransparency(0);

  frame->addView(buttonSignal);


  size(OFFSETXBUTTON,OFFSETY+23,OFFSETXBUTTON+bmpAdjust->getWidth(),OFFSETY+23+bmpAdjust->getHeight()/2);

  buttonAdjust = new COnOffButton(size,this,PARAM_ADJUST,bmpAdjust);

  buttonAdjust->setTransparency(0);

  frame->addView(buttonAdjust);


  size(OFFSETXBUTTON,OFFSETY+46,OFFSETXBUTTON+bmpReset->getWidth(),OFFSETY+46+bmpReset->getHeight()/2);

  buttonReset = new COnOffButton(size,this,PARAM_RESET,bmpReset);

  buttonReset->setTransparency(0);

  frame->addView(buttonReset);
  
  //---------------------------------//

  CPoint point(0,0);

  size(0,0,bmpSliderDelayBackground->getWidth(),bmpSliderDelayBackground->getHeight());

  size.offset(14,OFFSETY);

  sliderDelay = new CVerticalSlider(size,this,PARAM_DELAY,size.top + 1,size.top + bmpSliderDelayBackground->getHeight() - bmpSlider->getHeight(),bmpSlider,bmpSliderDelayBackground,point,kBottom);

  point(1,0);

  sliderDelay->setOffsetHandle(point);

  frame->addView(sliderDelay);

  //---------------------------------//

  point(0,0);

  size(0,0,bmpSliderBackground->getWidth(),bmpSliderBackground->getHeight());

  size.offset(50,OFFSETY);

  sliderTargetBpm = new CVerticalSlider(size,this,PARAM_TARGETBPM,size.top + 1,size.top + bmpSliderBackground->getHeight() - bmpSlider->getHeight(),bmpSlider,bmpSliderBackground,point,kBottom);

  point(1,0);

  sliderTargetBpm->setOffsetHandle(point);

  frame->addView(sliderTargetBpm);

  //---------------------------------//

  this->frame = frame;  

  update();

  return true;

}

void GUI::close()
{
  delete bmpBackground;
  delete bmpDelayTargetBpm;
  delete bmpProbabilitySilence;
  delete bmpOnsetBeatPhase;
  delete bmpButtonsBpm;

  delete bmpSignal;
  delete bmpAdjust;
  delete bmpReset;
  delete bmpSlider;
  delete bmpSliderBackground;

  delete context;
  delete frame;

  frame = 0;
}

void GUI::drawLine(CDrawContext *context,int px,int py,int qx,int qy,CColor color)
{
  CPoint p;

  context->setFrameColor(color);

  p.x = px;
  p.y = py;

  context->moveTo(p);

  p.x = qx;
  p.y = qy;

  context->setLineStyle(kLineSolid);
  context->lineTo(p);
}

void GUI::drawRect(CDrawContext *context,int x, int y, int width,int height,CColor color)
{
  CRect rect;

  rect.x = x;
  rect.y = y;

  rect.setWidth (width);
  rect.setHeight(height);

  context->setFillColor(color);
  context->drawRect(rect,kDrawFilled);
}


void GUI::drawString (CDrawContext *context,char str[],int x,int y,int width,int height)
{
  context->setFont(kNormalFontSmall);
  context->setFontColor(kWhiteCColor);

  CRect rect;

  rect.left   = x;
  rect.top    = y;
  rect.setWidth (width);
  rect.setHeight(height);

  context->setFillColor(colorBackground);
  context->drawRect(rect,kDrawFilled);

  context->drawString(str,rect);
}

void GUI::drawStringGrey(CDrawContext *context,char str[],int x,int y,int width,int height)
{
  context->setFont(kNormalFontSmall);
  context->setFontColor(colorBackground);

  CRect rect;

  rect.left   = x;
  rect.top    = y;
  rect.setWidth (width);
  rect.setHeight(height);

  context->setFillColor(kWhiteCColor);
  context->drawRect(rect,kDrawFilled);

  context->drawString(str,rect);
}

void GUI::drawDelayTargetBpm()
{
  char str[STRLENGTH];

  sprintf(str,"%2.2fs",ctrl->delayInSeconds);

  drawString(context,str,8,166,40,20);

  //sprintf(str,"%d frm",ctrl->delay);

  //drawString(context,str,8,186,40,20);


  if(ctrl->targetBpm > 0)
    sprintf(str,"%d",ctrl->targetBpm);
  else
    sprintf(str,"auto");

  drawString(context,str,48,166,25,20);

}

void GUI::drawProbabilitySilence ()
{
  COffscreenContext *oc = beginOC(bmpProbabilitySilence);

  double height = ctrl->getProbability() * SLIDERLENGTH;

  if(ctrl->adjust)
	height = 0;

  if(height>SLIDERLENGTH) 
	height = SLIDERLENGTH;

  for(int i=0;i<SLIDERWIDTH;i++)
    drawLine(oc,1+i,SLIDERLENGTH,1+i,SLIDERLENGTH-(int)height,kGreenCColor);


  height = ctrl->getSilence() * SLIDERLENGTH; //Achtung!!!

  if(ctrl->adjust)
	height = 0;

  if(height > SLIDERLENGTH) 
	height = SLIDERLENGTH;

  for(int i=0;i<SLIDERWIDTH;i++)
    drawLine(oc,39+i,SLIDERLENGTH,39+i,SLIDERLENGTH-(int)height,kGreenCColor);
  
  if(!ctrl->adjust)
  {
   //text out
   char str[STRLENGTH];

   sprintf(str,"%1.2f",ctrl->getProbability());

   drawString(oc,str,0,150,25,20);

   sprintf(str,"%1.2f",1-ctrl->getSilence());

   drawString(oc,str,38,150,25,20);
  }

  endOC(oc,bmpProbabilitySilence,111,OFFSETY);

}

void GUI::drawOnsetBeatPhase     ()
{
  COffscreenContext *oc = beginOC(bmpOnsetBeatPhase);

  int factor = 50;

  int heightBand0 = (int)(ctrl->getBand0() * factor);
  int heightBand1 = (int)(ctrl->getBand1() * factor);
  int heightBand2 = (int)(ctrl->getBand2() * factor);
  int heightBand3 = (int)(ctrl->getBand3() * factor);

  if(heightBand0 > SLIDERLENGTH)
	heightBand0 = SLIDERLENGTH;

  if(heightBand1 > SLIDERLENGTH)
	heightBand1 = SLIDERLENGTH;

  if(heightBand2 > SLIDERLENGTH)
	heightBand2 = SLIDERLENGTH;

  if(heightBand3 > SLIDERLENGTH)
	heightBand3 = SLIDERLENGTH;
  
  if(ctrl->adjust)
  {
	heightBand0 = 0;
    heightBand1 = 0;
	heightBand2 = 0;
	heightBand3 = 0;
  }

  for(int i=0;i<SLIDERWIDTH;i++)
  {
    drawLine(oc,14+i,SLIDERLENGTH,14+i,SLIDERLENGTH-heightBand0,kGreenCColor);
    drawLine(oc,36+i,SLIDERLENGTH,36+i,SLIDERLENGTH-heightBand1,kGreenCColor);
    drawLine(oc,58+i,SLIDERLENGTH,58+i,SLIDERLENGTH-heightBand2,kGreenCColor);
    drawLine(oc,80+i,SLIDERLENGTH,80+i,SLIDERLENGTH-heightBand3,kGreenCColor);
  }

  //------------------------------------------------------------------------//

  if(ctrl->phase >= 0 && ctrl->phase < 0.075)
  {
	CRect rect(131,21,131+82,21+82);

    oc->setFillColor(kGreenCColor);
    oc->fillEllipse(rect);
  }

  //------------------------------------------------------------------------//

  if(ctrl->phase >= 0 && ctrl->phase <= 1)
  {
    int width = 231 - 113;

    int pos = (int)(ctrl->phase * width);

    CColor colorDarkGrey;

    colorDarkGrey.red   = 51;
    colorDarkGrey.green = 51;
    colorDarkGrey.blue  = 15;

    drawLine(oc,113+pos-1,SLIDERLENGTH-1,113+pos-1,SLIDERLENGTH-24,colorDarkGrey);
    drawLine(oc,113+pos,  SLIDERLENGTH-1,113+pos,  SLIDERLENGTH-24,kGreenCColor);
    drawLine(oc,113+pos+1,SLIDERLENGTH-1,113+pos+1,SLIDERLENGTH-24,colorDarkGrey);
  }


  endOC(oc,bmpOnsetBeatPhase,206,OFFSETY);

}

void GUI::drawButtonsBpm()
{
  context->setFont(kNormalFontVeryBig);
  context->setFontColor(kWhiteCColor);

  char str[STRLENGTH];

  sprintf(str,"%3d",ctrl->bpm);

  CRect rect;

  rect.left   = 525;
  rect.top    = 146;
  rect.setWidth (40);
  rect.setHeight(20);

  context->setFillColor(colorBackground);
  context->drawRect(rect,kDrawFilled);

  if(!ctrl->adjust)
    context->drawString(str,rect);

}

void GUI::drawResonance()
{
  COffscreenContext *ocA = beginOC(bmpResonance);
  COffscreenContext *ocB = beginOC(bmpResonance);

  drawLine(ocA,1+MININTERVAL,200,1+MININTERVAL,0,kGreyCColor);
  drawLine(ocA,1+MAXINTERVAL,200,1+MAXINTERVAL,0,kGreyCColor);

  int bandID = 3;  
  int factor = 100;

  if(ctrl->targetBpm)
  {
	for(int i=0;i<NRESONANCE;i++)
	{
	  int height = ctrl->resonance[bandID][i] * factor;

	  drawLine(ocA,1+i,200,1+i,200-height,kGreyCColor);

	  height = ctrl->resonancePeaks[bandID][i] * factor;

	  if(i==ctrl->interval)
	   drawLine(ocA,1+i,200,1+i,200-height,kRedCColor);
	  else
	   drawLine(ocA,1+i,200,1+i,200-height,kGreenCColor);
	}

  }//end if ctrl->targetBpm


  if(ctrl->targetBpm == 0)
  {
	  for(int i=0;i<NRESONANCE;i++)
	  {
	    int height = ctrl->resonance[bandID][i] * factor;

	    drawLine(ocA,i,200,i,200-height,kGreyCColor);
      }

	  for(int i=0;i < ctrl->chain[bandID][0].count;i++)
	  {
		int index  = ctrl->chain[bandID][0].link[i].index;
		int height = ctrl->chain[bandID][0].link[i].value * factor;

		drawLine(ocA,index,200,index,200-height-2,kBlueCColor);	
	  }

	  for(int i=0;i < ctrl->chain[bandID][1].count;i++)
	  {
		int index  = ctrl->chain[bandID][1].link[i].index;
		int height = ctrl->chain[bandID][1].link[i].value * factor;

		drawLine(ocA,index,200,index,200-height,kGreenCColor);	
	  }

  }//end if targetBpm

  char str[STRLENGTH];

  if(ctrl->chain[bandID][0].top)
  {
	drawRect(ocA,4,4,8,8,kBlueCColor);

	int interval = ctrl->interval;
    int bpm = 0;

	if(interval > 0)
	  bpm = (int)(60. / (interval * SECPERFRAME));

	sprintf(str,"Interval %3d  Bpm %3d",interval,bpm);

	drawStringGrey(ocA,str,100,4,100,16);
  }

  if(ctrl->chain[bandID][1].top)
  {
	drawRect(ocA,4,4,8,8,kGreenCColor);

	int interval = ctrl->interval;
    int bpm = 0;

	if(interval > 0)
	  bpm = (int)(60. / (interval * SECPERFRAME));

	sprintf(str,"Interval %3d  Bpm %3d",interval,bpm);

	drawStringGrey(ocA,str,100,4,100,16);
  }
  
  endOC(ocA,bmpResonance,8,8);
  endOC(ocB,bmpResonance,270,8);

}

COffscreenContext* GUI::beginOC(CBitmap *bmp)
{
  COffscreenContext *oc = new COffscreenContext(frame,bmp->getWidth(),bmp->getHeight(),kGreyCColor);

  CRect rectScreen(0,0,bmp->getWidth(),bmp->getHeight());

  bmp->draw(oc,rectScreen);

  return oc;
}

void GUI::endOC(COffscreenContext *oc,CBitmap *bmp,int offsetX,int offsetY)
{
  CRect rectOff(offsetX,offsetY,offsetX+bmp->getWidth(),offsetY+bmp->getHeight());

  CPoint copyPoint(0,0);

  oc->copyFrom(context,rectOff,copyPoint);

  delete oc;
}

void GUI::update()
{
  AEffGUIEditor::idle ();

  if(!ctrl)
	return;

  drawDelayTargetBpm     ();

  drawProbabilitySilence ();

  drawOnsetBeatPhase     ();

  drawButtonsBpm         ();

  //drawResonance        ();

}

void GUI::idle()
{

}

void GUI::setParameter(VstInt32 index, float value)
{
  switch(index)
  {
    case PARAM_SIGNAL    : buttonSignal->setValue(value);       break;

	case PARAM_ADJUST    : buttonAdjust->setValue(value);       break;

	case PARAM_RESET     : buttonReset->setValue (0);           break;

	case PARAM_DELAY     : sliderDelay->setValue (value / 100); break;

	case PARAM_TARGETBPM : float position = (float)((value - MINBPM) / 100.);

	                       if(position < 0) 
							 position = 0;

						   if(position > 1)
							 position = 1;
	  	  
	                       sliderTargetBpm->setValue(position); break;   
  }

}

void GUI::valueChanged(CDrawContext *context, CControl *control)
{
  float value = control->getValue();

  switch(control->getTag())
  {
    case PARAM_SIGNAL    : ctrl->setSignal((int)value);                break;

	case PARAM_ADJUST    : ctrl->setAdjust((int)value);                break;

	case PARAM_RESET     : if((int)value)
						   {
						    ctrl->reset();
	                        buttonReset->setValue (0);
						   }                                           break;
    
	case PARAM_DELAY     : ctrl->setDelaySlider(value);                break;

	case PARAM_TARGETBPM : if(value > 0)
						   {	  
	                         int bpm = MINBPM + (int)(value * 100);

	                         ctrl->setTargetBpm(bpm);
						   }
						   else
							 ctrl->setTargetBpm(0);						   
						   
						                                               break;
  }

}

