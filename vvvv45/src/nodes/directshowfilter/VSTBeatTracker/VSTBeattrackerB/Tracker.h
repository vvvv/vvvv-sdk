#ifndef TRACKER_H
#define TRACKER_H

#include <stdio.h>
#include <math.h>
#include "audioeffectx.h"
#include "Global.h"
#include "FFT.h"
#include "OnsetDetection.h"
#include "Resonance.h"
#include "IntervalDetection.h"
#include "BeatDetection.h"
#include "GUI.h"



class Tracker
{
  public : 

		   FFT               fft;
		   OnsetDetection    onset;
		   Resonance         resonance;
		   IntervalDetection interval;
		   BeatDetection     beat;

		   Signal signal;
		   GUI    *gui;
		   Ctrl   *ctrl;

		   Tracker      (AudioEffect *effect);
           void process (double in[],int length);
		   void reset   ();

};

#endif 