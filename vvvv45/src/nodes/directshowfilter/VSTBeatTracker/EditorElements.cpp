#include "EditorElements.h"

PEN::PEN()
{
	  background = CreatePen(PS_SOLID,1,COLORREF(0x00444444));
	  dark       = CreatePen(PS_SOLID,1,COLORREF(0x00333333));
	  light      = CreatePen(PS_SOLID,1,COLORREF(0x00AAAAAA));
	  white      = CreatePen(PS_SOLID,1,COLORREF(0x00FFFFFF));      
	  red        = CreatePen(PS_SOLID,1,COLORREF(0x000000FF));      
	  green      = CreatePen(PS_SOLID,1,COLORREF(0x0000FF00));      
	  blue       = CreatePen(PS_SOLID,1,COLORREF(0x00FF0000));
	  yellow     = CreatePen(PS_SOLID,1,COLORREF(0x0000AAAA));
	  cyan       = CreatePen(PS_SOLID,1,COLORREF(0x00FFFF00));
	  pink       = CreatePen(PS_SOLID,1,COLORREF(0x00FF00FF));
	  black      = CreatePen(PS_SOLID,1,COLORREF(0x00000000));
}

void PEN::setColor(HDC hdc,int color)
{
	  switch(color)
	  {
	    case BACKGROUND : SelectObject(hdc,background); break;
		case DARK       : SelectObject(hdc,dark);       break;
		case LIGHT      : SelectObject(hdc,light);      break;
		case WHITE      : SelectObject(hdc,white);      break;
		case RED        : SelectObject(hdc,red);        break;
		case GREEN      : SelectObject(hdc,green);      break;
		case BLUE       : SelectObject(hdc,blue);       break;
		case YELLOW     : SelectObject(hdc,yellow);     break;
		case CYAN       : SelectObject(hdc,cyan);       break;
		case PINK       : SelectObject(hdc,pink);       break;
		case BLACK      : SelectObject(hdc,black);      break;
	  }

}

void PEN::resetColor(HDC hdc)
{
  SelectObject(hdc,light);
}

void PEN::destroy()
{
  DeleteObject(background);
  DeleteObject(dark);
  DeleteObject(light);
  DeleteObject(white);      
  DeleteObject(red);      
  DeleteObject(green);      
  DeleteObject(blue);
  DeleteObject(yellow);
  DeleteObject(cyan);
  DeleteObject(pink);
  DeleteObject(black);
}

/***********************************************************************/

TXT::TXT () {}

/***********************************************************************/
	
TXT::TXT (int _x,int _y,char *_str)
{
   x   = _x;
   y   = _y;

   lengthLabel  = 0;
   valueLength  = 0;

   while(*(_str+lengthLabel)!='\0')
   str[lengthLabel] = *(_str+lengthLabel++);

}

/***********************************************************************/

void TXT::init(int _x,int _y,char *_str)
{
   x   = _x;
   y   = _y;

   lengthLabel = 0;
   valueLength = 0;

   while(*(_str+lengthLabel)!='\0')
   str[lengthLabel] = *(_str+lengthLabel++);

}

/***********************************************************************/

void TXT::setValue(int _value)
{
  int i=0;
  char value[80];

  valueLength=0;

  sprintf(&value[0],"%d",_value);

  while(value[i]!='\0')
  str[lengthLabel+valueLength++] = value[i++];

}

/***********************************************************************/

void TXT::setValue(double _value)
{
  int i=0;
  char value[80];

  valueLength=0;

  sprintf(&value[0],"%f",_value);

  while(value[i]!='\0')
  str[lengthLabel+valueLength++] = value[i++];

}

/***********************************************************************/

void TXT::print(HDC hdc)
{
  TextOut(hdc,x,y,str,lengthLabel+valueLength);

}

/***********************************************************************/

void TXT::print(HDC hdc,int _value)
{
  int i=0;
  char value[80];

  valueLength=0;

  sprintf(&value[0],"%d",_value);

  while(value[i]!='\0')
  str[lengthLabel+valueLength++] = value[i++];

  TextOut(hdc,x,y,str,lengthLabel+valueLength);

}
