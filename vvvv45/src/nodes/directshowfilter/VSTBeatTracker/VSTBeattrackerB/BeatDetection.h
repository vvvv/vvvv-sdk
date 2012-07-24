#ifndef BEATDETECTION_H
#define BEATDETECTION_H

#include <stdio.h>
#include <math.h>
#include <windows.h>
#include "Global.h"
#include "Signal.h"

#define NAGENTS 16
#define START    8
#define NTICKS 128 

//-------------------------------------------------//

struct TICK
{
  int position;
  double value;

  TICK();
};

class Ticklist
{
  public : TICK list[NTICKS];
		   int count;
		   int first;

		   double weight;

		   Ticklist();

           void   update ();
		   void   init   (int position, double value);
		   void   set    (int position, double value);
		   int    getPositionStraight (int i);
		   double getValueStraight    (int i); 
		   void   calcWeight          ();
		   void   reset               ();

};

//-------------------------------------------------//

class Agent
{
   public : int position;
            int interval;
			
			bool active;
			bool beat;
			int  top;

			Ticklist tick;

			Agent();

			void start   (int position,double value);
			void process (int interval,double field[]); 
			void reset   ();
			
};

class Agentlist
{
   public : Agent agent[NAGENTS];

			SIGNALBAND *band;

			int    top;
			double weight;
			bool   beat;
			int    bpm;
			long   count;
		
			Agentlist();

		    void process (SIGNALBAND *band);
			void start   ();
			void getInfo ();
			void clean   ();
			void getTop  ();
			void reset   ();

};

//-------------------------------------------------//

class BeatDetection
{
   public : BeatDetection ();
	        void process  (Signal &signal);
			void reset    ();

		    Agentlist agentlist[NBANDS];
};


#endif   