#ifndef _BEAT_H
#define _BEAT_H

#include <stdio.h>
#include <math.h>
#include "Define.h"
#include "PeakPicker.h"
#include "Time.h"


const int ENTER     =      24;
const int START     =       8;
const int NBASE     =     128;
const int NAGENT    =      32;
const int INTERVAL  =  NFIELD; 
const int TOLERANCE =       4;
const int IDLETIME  = 1000000;
const int MAXEMPTY  =  NFIELD;

//----------------------------------//

struct BASE
{
  BASE       ();
  void reset ();

  int      pos;
  double value;
  int distance;
  bool   valid;

}; 

//----------------------------------//

struct COVER
{
  int value;
  int id;
};

//----------------------------------//

class Agent
{
  public : Agent ();

           void process (double p[]);
		   void start   (double p[]);
           void shift   ();
           void check   ();
		   void setBeep ();
           int  at      (int i);
		   void reset   ();

		   BASE base    [NBASE];

		   double *peak; 
		   double value;
		   int    valid;
           int interval;
		   int n;
		   int c;

		   bool beep;
		   bool active;

};

//----------------------------------//

class Beat
{
  public : Beat ();
		   void process     (double signal[], int interval);
		   void setChannel  ();
		   void setInterval (int interval);
		   void setCover    ();
		   void start       ();
		   void setBeep     ();
		   void setFX       ();
		   void setValue    ();
		   void reset       ();

		   PEAKPICKER peakpicker;
           Agent   agent[NAGENT];

		   double peak [NFIELD];
		   bool   beep;
		   double fx;
		   double value;

		   int id;
		   int interval;
		   int valid;
		   int idOld;

		   Time time;

		   COVER cover [NFIELD];

};

class BeatDetection
{
   public : BeatDetection ();
            void  process (double signal[][NFIELD],int interval[],double resonance[][NRESONANCE],bool &beep,double fx[],bool optimize);
            void  reset   ();

			Beat beat[NCHANNEL];
            bool beep[NCHANNEL];

			Time time;

			int idOld;
			int channel;

};

#endif