#ifndef RESONANCE_H 
#define RESONANCE_H

#include <stdio.h>
#include <math.h>
#include "Global.h"

#define MEANFACTOR  1.4
#define FRAMERATE 172.265625

#define FORRESONANCE for(int i=0;i<NRESONANCE;i++)

class Combfilter
{
  public : Combfilter   ();
		   void process (int count,double onset[],double resonance[]);
		   void reset   ();

		   double mean;

		   double onsetCut         [NSAMPLES];
		   double leakyIntegrate   [NSAMPLES];
		   double response         [NRESONANCE][NSAMPLES];
		   
		   double gain             [NRESONANCE];
		   double gainOverall      [NRESONANCE];
		   double normalize        [NRESONANCE][NSAMPLES];
		   double resonanceOverall [NSAMPLES];
		   
		   double responseEnergy   [NRESONANCE][NSAMPLES];
		   double responseSum      [NRESONANCE];
		   bool   responseToggle   [NRESONANCE];

};

class Resonance
{
  public : Combfilter combfilter[NBANDS];

		   Resonance    ();
		   void process (int count,double **onset,double **resonance);
		   void reset   ();

};


#endif