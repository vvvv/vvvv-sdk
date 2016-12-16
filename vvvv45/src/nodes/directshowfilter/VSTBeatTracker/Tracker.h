/*-----------------------------------------------------------------------------------//

  Tracker is the main class of the beatracking-process
  the different steps in the processing are encapsulated into classes  
  
/------------------------------------------------------------------------------------*/


#ifndef TRACKER_H
#define TRACKER_H

#include <stdio.h>
#include <math.h>

#include "Define.h"
#include "FFT.h"
#include "OnsetDetection.h"
#include "Resonator.h"
#include "Interval.h"
#include "Beat.h"
#include "StructCollection.h"

class Ctrl;

class Tracker
{
  public :  OnsetDetection onsetDetection;		   
		    Resonator      resonator;
		    Interval       interval;
		    FFT            fft;
            BeatDetection  beatDetection;

  public :	Tracker ();
           ~Tracker ();
		    void setCtrl        (Ctrl *ctrl);
		    void process        (double in[],int length,float samplerate);
			void checkLength    (int length);
			void reset          ();
			void setPerformance (int p);
			void setBPM         ();
			
			SIGNAL signal;

			int  counter;
			int  nSamples;
			bool optimize;
			int  bpm;
};


#endif