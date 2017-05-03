/*-----------------------------------------------------------------------------------//

  the class Interval receives the resonance in each channel and 
  returns a tempo-hypothesis for the beat. it uses one object of
  ChainBuilder for each channel. ChainBuilder examines the resonance
  and find peaks which belong together and builds chains of them. if 
  more than one chain is discovered. they have to be put in order.
  the first element of the winning chain is the tempo-hypothesis: 
  the interval between to beats

/------------------------------------------------------------------------------------*/

#ifndef INTERVAL_H
#define INTERVAL_H

#include <stdio.h>
#include <math.h>
#include <windows.h>

#include "Define.h"
#include "PeakPicker.h"


/****************************************************/

//one link of a chain is a peak in the resonance
typedef struct LINK
{
  int    pos;
  double value;
 
  LINK ();
  void   reset    ();

}_LINK;

/****************************************************/

//a chain is build of peaks in the resonance
typedef struct CHAIN
{
   int    nLink;
   double value;
   int    interval;

   LINK link[NRESONANCE];

   CHAIN ();
   void  reset    ();

}_CHAIN;

/****************************************************/

//ChainBuilder examines the resonance in one channel
class ChainBuilder
{
  private : CHAIN  chain[MAXINTERVAL-MININTERVAL];
     	    double peak[NRESONANCE];
		    int    nChain;
		    PEAKPICKER peakpicker;

		    LINK top;
			int  interval;

			int minInterval;
			int maxInterval;
 
		    void setTop       ();
		    void setupChains  ();
		    void sortChains   ();
		    void calInterval  ();
		    void copy         (CHAIN &a,CHAIN &b);
		    bool compare      (CHAIN  a,CHAIN  b);

  public :  ChainBuilder      ();
	        void    process   (double resonance[],int &interval,int minInterval,int maxInterval);	
		    int     getNChain ();
            void    reset     ();
			double* getPeak   ();
			double  getPeak   (int index);
			int     getInterval       ();
			int     getChainNLink     (int c);
			int     getChainLinkPos   (int c,int l);
            double  getChainLinkValue (int c,int l);	

};


/****************************************************/

//Interval organizes the processing of the resonances
class Interval
{
  public  : ChainBuilder chainBuilder[NCHANNEL];

		    void process (double [][NRESONANCE],int interval[],int minInterval,int maxInterval,bool optimize);

			void reset ();
 
			ChainBuilder* getChainBuilder(int index);

};

#endif

