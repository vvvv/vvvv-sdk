#ifndef INTERVALDETECTION_H
#define INTERVALDETECTION_H

#include <math.h>
#include "Global.h"
#include "Peakpicker.h"

struct LINK
{
  int    index;
  double value;

};

struct CHAIN
{
  LINK link[32];

  int  count;
  int  interval;
  bool top;

  CHAIN();
  void init(int index,double value);
  void set (int index,double value);
};
  
class IntervalDetection
{
  public  : CHAIN chain[NBANDS][NINTERVAL];

  public  : void process          (double **resonance, int *interval, int targetInterval); 
		    int  getMaximum       (double field[]);
			int  getSecondMaximum (double field[],int firstIndex);
		    void setChain         (int index,double field[],CHAIN &chain);
			bool tripleInterval   (int *interval,CHAIN &chain0,CHAIN &chain1);
			int  getMaxChain      (CHAIN &chain0,CHAIN &chain1);
			int  getNearest       (double peak[],int targetInterval);

};

#endif