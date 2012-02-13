/*-----------------------------------------------------------------------------------//

  Peakpicker takes an array and detects peaks in it
  It uses processes the array in a recursiv way

/------------------------------------------------------------------------------------*/


#ifndef PEAKPICKER_H
#define PEAKPICKER_H

#include <stdio.h>
#include <math.h>
#include "Define.h"

typedef struct PEAKPICKER
{
  double signal[NFIELD];
  double peak  [NFIELD];

  PEAKPICKER      ();
  void process    (double _signal[],double _peak[]);
  void process    (double _signal[],double _peak[],int length);
  void clustering (int start,int end);
  void getPeak    (int start,int end);

}_PEAKPICKER;

#endif
