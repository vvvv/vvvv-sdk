

#ifndef PEAKPICKER_H
#define PEAKPICKER_H

#include <stdio.h>
#include <math.h>
#include "Global.h"

typedef struct PEAKPICKER
{
  double signal[NSAMPLES];
  double peak  [NSAMPLES];

  PEAKPICKER      ();
  void process    (double _signal[],double _peak[]);
  void process    (double _signal[],double _peak[],int length);
  void clustering (int start,int end);
  void getPeak    (int start,int end);

}_PEAKPICKER;

#endif
