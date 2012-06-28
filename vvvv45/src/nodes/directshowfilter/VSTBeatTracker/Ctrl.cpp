#include "Ctrl.h"

Ctrl::Ctrl(Editor *editor,Tracker *tracker)
{
  this->editor  = editor;
  this->tracker = tracker;

  stop = false;
  play = false;

  activeChannel = 0;

}
 