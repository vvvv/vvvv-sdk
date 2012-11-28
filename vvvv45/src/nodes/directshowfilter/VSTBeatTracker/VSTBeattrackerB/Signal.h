#ifndef SIGNAL_H
#define SIGNAL_H

#include "Global.h"
#include "IntervalDetection.h"

//-------------------------------------------------//

struct SIGNALBAND
{
  double resonance      [NRESONANCE];
  double resonancePeaks [NRESONANCE];
  double onset          [NSAMPLES];
  double onsetStraight  [NSAMPLES];
  double onsetCut       [NSAMPLES];
  int    cover          [NSAMPLES];

  int    interval;
  double onsetMean;
  double energy;
  int    count;
  int    countInc;
    
  SIGNALBAND        ();
  void update       (int interval);
  bool localMaximum ();
  void reset        ();

};

//-------------------------------------------------//

//Ctrl provides the delayed data to display to the user 
//and the in/out parameters 
struct Ctrl
{
  Ctrl();

  void   reset              ();
  void   update             (int count,int beat,int topInterval,double probability,double silence);
  
  void   setSignal          (int value);
  void   setAdjust          (int value);
  void   setDelay           (int value);
  void   setDelaySlider     (float value);
  void   setTargetBpm       (int value);
  void   setTargetBpmSlider (float value);
  void   setSecondsPerFrame (int frameSize);
  
  float  getDelayInSec  ();
  int    getBeat        ();
  int    getBeatswitch  ();
  double getPhase       ();
  int    getBpm         ();
  double getProbability ();
  double getSilence     ();

  double getBand0       ();
  double getBand1       ();
  double getBand2       ();
  double getBand3       ();

  int    signal;
  int    adjust;
  int    delay;
  int    targetBpm;
  int    targetInterval;
  int    interval;
  int    count;
  int    currentBeat;
  int    period;
  int    resetSignal;
  int    bpm;
  int    beatswitch;
  double phase;
  double secondsPerFrame;
  double delayInSeconds;

  double silence     [NSAMPLES];
  double probability [NSAMPLES];
  double beat        [NSAMPLES];

  double **band;
  double **resonance;
  double **resonancePeaks;

  CHAIN  **chain;



};

//-------------------------------------------------//

class Signal
{
  public : Signal           ();
		  ~Signal           ();
		   void init        (Ctrl *ctrl,CHAIN chain[][NINTERVAL]);
		   void process     ();
		   void calcSilence ();
		   void inc         ();
		   void reset       ();

  public : double *in;

		   double freq    [NSAMPLES];
		   int    interval[NBANDS];
		   double weight  [NBANDS];

		   double **onset;
		   double **onsetStraight;
		   double **onsetCut;
		   double **resonance;
		   double **resonancePeaks;
		   int    **cover;

		   Ctrl *ctrl;
		   SIGNALBAND band [NBANDS];

		   int    top;
		   int    count;
		   int    countInc;
		   double probability;
		   long   probabilityCount;
		   int    bpm;
		   int    beat;
		   int    targetInterval;
		   double silence;
		   int    topInterval;
		   
		   double energySample[NOISECYCLE];

};

#endif

