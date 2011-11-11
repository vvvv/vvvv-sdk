/*-----------------------------------------------------------------------------------//

   the Resonator-class examines the 1024 elements long onset-signals of the 
   different channels if they contain periodic high values. this is done by
   a combfilterbank, one per channel.

/------------------------------------------------------------------------------------*/


#ifndef RESONATOR_H
#define RESONATOR_H

#include <stdio.h>
#include <math.h>
#include "Define.h"


//const double FRAMERATE = 43.07;

const double FRAMERATE = 172.265625; //eigentlich für 256 Sample-Blöcke

//Combilter-bank for one channel
typedef struct COMBFILTER
{
	COMBFILTER   ();
	void process (int c, int cm1, int cp2, double onset, double resonance[]);
    void reset   ();

	double v [NFIELD]; 
	double v0[NFIELD]; 
	double v_[NFIELD];
	double r [NRESONANCE][NFIELD];
	double r_[NRESONANCE][NFIELD];
	double g [NRESONANCE];
	double g_[NRESONANCE];
	double s [NRESONANCE][NFIELD];
	double t [NRESONANCE];
		
    double mean;
	double meanFactor;

}_COMBFILTER;

class Resonator
{
  private : COMBFILTER combfilter[NCHANNEL];

  public  :	void process(double [][NFIELD],double [][NRESONANCE],bool optimize);
            Resonator  ();
			void reset ();

			int performance;

};



#endif


