#ifndef RESONATOR_H
#define RESONATOR_H

#include <stdio.h>
#include <math.h>
#include "Global.h"
//#include <omp.h>


class Combfilter
{
  public : Combfilter();
	
	       void process(double onset[NSAMPLES],double resonance[NRESONANCE],int count);

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

};

class Resonator
{
  public : Resonator();

		   void process(double onset[][NSAMPLES],double [][NRESONANCE]);
	
	       Combfilter combfilter[NCHANNEL];

};

#endif