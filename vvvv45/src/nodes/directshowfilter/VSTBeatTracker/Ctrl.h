#ifndef CTRL_H
#define CTRL_H


#include <stdio.h>
#include <math.h>
#include "Define.h"

#include "Editor.h"
#include "Tracker.h"

class Editor;
class Tracker;

class Ctrl
{
  public : Editor  *editor;
		   Tracker *tracker;

		   bool play;
		   bool stop;

		   int activeChannel;

		   Ctrl            (Editor *editor,Tracker *tracker);
 
};


#endif