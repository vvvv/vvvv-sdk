#include <stdio.h>
#include <math.h>
#include <windows.h>

enum colorDef   { BACKGROUND, DARK, LIGHT, WHITE, RED, GREEN, BLUE, YELLOW, CYAN, PINK, BLACK };

//----------------------------------------------------------------------//

struct EDITORWINDOW : DLGTEMPLATE
{
  WORD ext[3];
  EDITORWINDOW () { memset(this,0,sizeof(*this)); }

};

//----------------------------------------------------------------------//

typedef struct PEN
{
  HPEN background,dark,light,white,red,green,blue,yellow,cyan,pink,black;

  PEN             ();
  void setColor   (HDC hdc,int color);
  void resetColor (HDC hdc);
  void destroy    ();

}_PEN;

//----------------------------------------------------------------------//

typedef struct TXT
{
	char str[80];
	int  lengthLabel;
	int  valueLength;
	int  x;
	int  y;

	TXT ();
	TXT (int _x,int _y,char *_str);
	void init     (int _x,int _y,char *_str);
	void setValue (int _value);
	void setValue (double _value);
	void print    (HDC hdc);
	void print    (HDC hdc,int _value);
	

}_TXT;


