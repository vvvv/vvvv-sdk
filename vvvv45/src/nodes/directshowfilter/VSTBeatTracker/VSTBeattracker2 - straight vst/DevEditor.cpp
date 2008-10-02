#include "DevEditor.h"


Display::Display(const CRect& size) : CView(size)
{
  context = NULL;

  colorBackground.red   = 231;
  colorBackground.green = 231;
  colorBackground.blue  = 231;

  color[0].red =   0; color[0].green =   0; color[0].blue = 255;  //Blue
  color[1].red =   0; color[1].green = 255; color[1].blue =   0;  //Green
  color[2].red =   0; color[2].green = 255; color[2].blue = 255;  //Cyan
  color[3].red = 255; color[3].green =   0; color[3].blue =   0;  //Red
  color[4].red = 255; color[4].green =   0; color[4].blue = 255;  //Magenta
  color[5].red = 255; color[5].green = 255; color[5].blue =   0;  //Brown

  for(int i=0;i<NSAMPLES;i++)
  tapField[i] = false;
  
}

void Display::drawLine(CDrawContext *context,int px, int py, int qx, int qy, CColor color)
{
  CPoint p;

  context->setFrameColor(color);
  //context->setLineStyle(kLineSolid);

  p.x = px;
  p.y = py;

  context->moveTo(p);

  p.x = qx;
  p.y = qy;

  context->lineTo(p);

}

void Display::drawFrame(int x,int y,int width,int height,CColor color)
{
  CPoint p0;
  CPoint p1;
  CPoint p2;
  CPoint p3;

  p0.x = x;
  p0.y = y;

  p1.x = x + width;
  p1.y = y;

  p2.x = x + width;
  p2.y = y + height;

  p3.x = x;
  p3.y = y + height;

  context->setFrameColor(color);

  context->moveTo(p0);
  context->lineTo(p1);

  context->moveTo(p1);
  context->lineTo(p2);

  context->moveTo(p2);
  context->lineTo(p3);

  context->moveTo(p3);
  context->lineTo(p0);

}

void Display::drawRect(int x, int y, int width,int height,CColor color)
{
  CRect rect(size);

  rect.x = x;
  rect.y = y;

  rect.setWidth (width);
  rect.setHeight(height);

  context->setFillColor(color);
  context->drawRect(rect,kDrawFilled);

  drawFrame(x-1,y-1,width+2,height+2,kBlackCColor);

}


void Display::idle()
{
  if(context==NULL || frame==NULL || signal==NULL)
    return;

  drawOnset     ();

  drawResonance ();

  drawGeneric   ();

}

void Display::drawString(CDrawContext *context,char str[],int x,int y,int width,int height,CColor color)
{
  context->setFont(kNormalFontSmall);
  context->setFontColor(color);

  CRect rect(size);

  rect.left = x;
  rect.top  = y;
 
  rect.setWidth (width);
  rect.setHeight(height);

  context->setFillColor(colorBackground);
  context->drawRect(rect,kDrawFilled);

  context->drawString(str,rect);

}


void Display::drawOnset()
{
  /*
  CBitmap *bitmap = new CBitmap(204);

  COffscreenContext *oc = new COffscreenContext(frame, bitmap->getWidth(), bitmap->getHeight(), colorBackground);

  //Ausschnitt auf dem Bereich (von x,von y,bis x, bis y)
  CRect rectScreen(0,0,bitmap->getWidth(),bitmap->getHeight());

  bitmap->draw(oc,rectScreen);

  //---------------------------------------------------------//

  double fraction = 25;

  for(int i=0;i<NSAMPLES;i++)
    drawLine(oc,i,100,i,100 - signal->onsetStraight[signal->show.bandA][i] * fraction,kGreyCColor);

  for(int i=0;i<NSAMPLES;i++)
  if(signal->band[signal->show.bandA].cover[i] >= 0)
  {
	int index = signal->band[signal->show.bandA].cover[i] % 6;

	drawLine(oc,i,100,i,100 - signal->onsetStraight[signal->show.bandA][i] * fraction,color[index]);
  }

  for(int i=0;i<NSAMPLES;i++)
  {
	int index = ((signal->count-i + DELAY) + NSAMPLES) % NSAMPLES;

    if(tapField[index])
	 drawLine(oc,i,200,i,0,kGreyCColor);
  }

  tapField[(signal->count + DELAY) % NSAMPLES] = 0;


  if(signal->top >= 0)
  {
    for(int i=0;i<NSAMPLES;i++)
      drawLine(oc,i,200,i,200 - signal->onsetStraight[signal->top][i] * fraction,kGreyCColor);

    for(int i=0;i<NSAMPLES;i++)
    if(signal->band[signal->top].cover[i] >= 0)
    {
	  int index = signal->band[signal->top].cover[i] % 6;

	  drawLine(oc,i,200,i,200 - signal->onsetStraight[signal->top][i] * fraction,color[index]);
    }

  }

  ////---------------------------------------------------------//

  //Abstand nach links und oben, wie breit und wie hoch soll tatsächlich auf den Bildschirm gezeichnet werden
  CRect rectOff(OFFSETX,OFFSETX,OFFSETX+bitmap->getWidth(),OFFSETX+bitmap->getHeight());

  CPoint copyPoint(0,0);

  oc->copyFrom(context,rectOff,copyPoint);

  delete oc;
  delete bitmap;
  */
}

void Display::drawResonance()
{
  /*
  CBitmap *bitmapA = new CBitmap(205);

  COffscreenContext *ocA = new COffscreenContext(frame,bitmapA->getWidth(),bitmapA->getHeight(),colorBackground);

  CRect rectScreenA(0,0,bitmapA->getWidth(),bitmapA->getHeight());

  bitmapA->draw(ocA,rectScreenA);

  //--------------------------------------------------------------------------------------------------------//

  drawLine(ocA,MININTERVAL,200,MININTERVAL,0,kGreyCColor);

  drawLine(ocA,MAXINTERVAL,200,MAXINTERVAL,0,kGreyCColor);

  for(int i=0;i<NRESONANCE;i++)
  if(signal->interval[signal->show.bandA] == i)
    drawLine(ocA,i,200,i,200 - signal->resonance[signal->show.bandA][i]*200,kGreenCColor);
  else
    drawLine(ocA,i,200,i,200 - signal->resonance[signal->show.bandA][i]*200,kGreyCColor);

  for(int i=0;i<NRESONANCE;i++)
    drawLine(ocA,i,200,i,200 - signal->test[signal->show.bandA][0][i]*200,kGreenCColor);

  for(int i=0;i<NRESONANCE;i++)
    drawLine(ocA,i,200,i,(200 - signal->test[signal->show.bandA][1][i]*200)+2,kRedCColor);

  drawLine(ocA,
	       signal->interval[signal->show.bandA],
		   200,
		   signal->interval[signal->show.bandA],
		   200 - signal->resonance[signal->show.bandA][signal->interval[signal->show.bandA]]*200,
		   kMagentaCColor);

  //--------------------------------------------------------------------------------------------------------//

  CRect rectOffA(12,260,12+bitmapA->getWidth(),260+bitmapA->getHeight());

  CPoint copyPointA(0,0);

  ocA->copyFrom(context,rectOffA,copyPointA);

  delete ocA;
  delete bitmapA;


  CBitmap *bitmapB = new CBitmap(205);

  COffscreenContext *ocB = new COffscreenContext(frame,bitmapB->getWidth(),bitmapB->getHeight(),colorBackground);

  CRect rectScreenB(0,0,bitmapB->getWidth(),bitmapB->getHeight());

  bitmapB->draw(ocB,rectScreenB);

  //--------------------------------------------------------------------------------------------------------//

  drawLine(ocB,MININTERVAL,200,MININTERVAL,0,kGreyCColor);

  drawLine(ocB,MAXINTERVAL,200,MAXINTERVAL,0,kGreyCColor);

  if(signal->top >= 0)
  {
   for(int i=0;i<NRESONANCE;i++)
   if(signal->interval[signal->top] == i)
     drawLine(ocA,i,200,i,200 - signal->resonance[signal->top][i]*200,kGreenCColor);
   else
     drawLine(ocB,i,200,i,200 - signal->resonance[signal->top][i]*200,kGreyCColor);
  }

  //--------------------------------------------------------------------------------------------------------//

  CRect rectOffB(280,260,280+bitmapB->getWidth(),260+bitmapB->getHeight());

  CPoint copyPointB(0,0);

  ocB->copyFrom(context,rectOffB,copyPointB);

  delete ocB;
  delete bitmapB;
  */
}


void Display::drawGeneric()
{
  /*
  char str[STRLENGTH];

  //index of the shown bands and the top agent
  sprintf(str,"Band %d",signal->show.bandA);

  drawString(context,str,850,220,50,20,kBlackCColor);

  if(signal->band[signal->show.bandA].top>=0)
  {
   sprintf(str,"Agent %d",signal->band[signal->show.bandA].top);

   drawString(context,str,990,220,40,20,color[signal->band[signal->show.bandA].top % 6]);
  }


  sprintf(str,"Band %d",signal->top);

  drawString(context,str,850,240,50,20,kBlackCColor);

  if(signal->top>=0)
  {
	sprintf(str,"Agent %d",signal->band[signal->top].top);

    drawString(context,str,990,240,40,20,color[signal->band[signal->top].top % 6]);
  }

  //bpm and interval
  //sprintf(str,"BPM %3d",signal->bpm[showBandA]);

  //drawString(context,str,14,460,80,20,kBlackCColor);

  //sprintf(str,"Interval %3d %3d",signal->interval[showBandA][0],signal->interval[showBandA][1]);

  //drawString(context,str,100,460,120,20,kBlackCColor);

  //sprintf(str,"%d",showBandA);

  //drawString(context,str,250,460,20,20,kBlackCColor);


  //if(signal->top >= 0)
  //{
  // sprintf(str,"BPM %3d",signal->bpm[showBandB]);

  // drawString(context,str,294,460,80,20,kBlackCColor);

  // sprintf(str,"Interval %3d %3d",signal->interval[showBandB][0],signal->interval[showBandB][1]);

  // drawString(context,str,370,460,120,20,kBlackCColor);

  // sprintf(str,"%d",showBandB);

  // drawString(context,str,520,460,20,20,kBlackCColor);
  //}  

  //draw Frames 
  drawFrame(OFFSETX-1,OFFSETX-1,1026,201,kBlackCColor);

  drawFrame(OFFSETX-1,258,258,202,kBlackCColor);

  drawFrame(279,258,258,201,kBlackCColor);
  */
}

DevEditor::DevEditor(AudioEffect *effect) : AEffGUIEditor(effect)
{
  rect.left   = 0;
  rect.top    = 0;
  rect.right  = WNDWIDTH;
  rect.bottom = WNDHEIGHT;
  
  background = new CBitmap(200);

  play  = true;
  shift = false;

}

DevEditor::~DevEditor()
{


}

bool DevEditor::open(void *ptr)
{
  AEffGUIEditor::open(ptr); 

  CRect size(0,0,WNDWIDTH,WNDHEIGHT);


  CFrame *frame = new CFrame(size, ptr, this);

  frame->setBackground(background);

  display = new Display(size);

  frame->addView((CView*)display);

  display->context = frame->createDrawContext();

  display->frame   = frame;

  //-------------------------------------//
  
  CBitmap *buttonBitmap = new CBitmap(203);

  int yPosition = 220;


  CRect buttonPlaySize(16,yPosition,16+buttonBitmap->getWidth(),yPosition+buttonBitmap->getHeight()/2);

  buttonPlay = new COnOffButton(buttonPlaySize,this,BUTTONPLAY,buttonBitmap);

  frame->addView(buttonPlay);

  
  CRect buttonShiftSize(40,yPosition,40+buttonBitmap->getWidth(),yPosition+buttonBitmap->getHeight()/2);
  
  buttonShift = new COnOffButton(buttonShiftSize,this,BUTTONSHIFT,buttonBitmap);

  frame->addView(buttonShift);
  

  CRect buttonTapSize(64,yPosition,64+buttonBitmap->getWidth(),yPosition+buttonBitmap->getHeight()/2);
  
  buttonTap = new COnOffButton(buttonTapSize,this,BUTTONTAP,buttonBitmap);

  frame->addView(buttonTap);



  CRect r(900,yPosition,984+buttonBitmap->getWidth(),yPosition+buttonBitmap->getHeight()/2);

  buttoNBANDSA = new COnOffButton(r,this,BUTTONBANDA,buttonBitmap);

  frame->addView(buttoNBANDSA);


  r.moveTo(r.x + 60,r.y);

  buttonIntervalA = new COnOffButton(r,this,BUTTONINTERVALA,buttonBitmap);

  frame->addView(buttonIntervalA);


  r.moveTo(r.x - 60,r.y+20);
  
  buttoNBANDSB = new COnOffButton(r,this,BUTTONBANDB,buttonBitmap);

  frame->addView(buttoNBANDSB); 


  r.moveTo(r.x + 60,r.y);

  buttonIntervalB = new COnOffButton(r,this,BUTTONINTERVALB,buttonBitmap);

  frame->addView(buttonIntervalB);


  this->frame = frame;  

  return true;
}

void DevEditor::close()
{
  delete frame;
  frame = 0;
}

void DevEditor::idle(Signal *signal)
{
  if(!display)
	return;

  display->signal = signal;

  AEffGUIEditor::idle ();

  if(display->context != NULL)
  {
    buttonPlay->draw(display->context);

    buttonShift->draw(display->context);

	buttonTap->draw(display->context);

	buttoNBANDSA->draw(display->context);

	buttoNBANDSB->draw(display->context);

	buttonIntervalA->draw(display->context);

	buttonIntervalB->draw(display->context);
  }

  display->idle();
  

}

void DevEditor::setParameter(VstInt32 index, float value)
{
  

}

void DevEditor::valueChanged(CDrawContext *context, CControl *control)
{
  float value = control->getValue();

  switch(control->getTag())
  {
    case BUTTONPLAY      : play = !play;                                     break;

	case BUTTONSHIFT     : shift = true;                                     break;

	case BUTTONTAP       : display->tapField[display->signal->count] = true; break;

	case BUTTONBANDA     : ++display->signal->show.bandA %= NBANDS;          break;

	case BUTTONBANDB     : ++display->signal->show.bandB %= NBANDS;          break;

	case BUTTONINTERVALA : ++display->signal->show.intervalA %= NINTERVAL;   break;

	case BUTTONINTERVALB : ++display->signal->show.intervalB %= NINTERVAL;   break;

  }

}
