#ifndef ONSETDETECTION_H
#define ONSETDETECTION_H

#include <stdio.h>
#include <math.h>
#include "Global.h"

struct ONSETBAND
{
  int start,end,nBands;

  double rms    [NSAMPLES];
  double com    [NSAMPLES];
  double low    [NSAMPLES];
  double diff   [NSAMPLES];
  double weight [NSAMPLES];

  ONSETBAND  ();
  void reset ();

};

class OnsetDetection
{
  public : OnsetDetection();

		   void process (int count,double *freq,double **onset);
           void reset   ();

		   ONSETBAND band[NBANDS];
		   
};

#endif