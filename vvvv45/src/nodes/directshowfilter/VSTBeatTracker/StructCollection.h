#ifndef STRUCTCOLLECTION_H
#define STRUCTCOLLECTION_H

#include <stdio.h>
#include <math.h>
#include <windows.h>
#include "Define.h"

//-----------------------------------------------------------------------------//


struct FEEDBACKFRAME
{
  bool   beat;
  int    interval;
  double phase;
  bool   active;
  int    bpm;

  LONGLONG startTime;
  LONGLONG endTime;
  LONGLONG midTime;

  double fx[NCHANNEL];
  
  FEEDBACKFRAME ();
  void set      (bool beat,int interval,double fx[],int bpm);
  void set      (FEEDBACKFRAME *ptr);
  void setTime  (LONGLONG startTime, LONGLONG midTime, LONGLONG endTime);
  void reset    ();
  bool check    (LONGLONG streamTime);

};

//-----------------------------------------------------------------------------//

typedef struct SIGNAL
{
  double *in; 
  double *out;

  double freq      [NSAMPLES];
  double onset     [NCHANNEL][NFIELD]; 
  double resonance [NCHANNEL][NRESONANCE]; 
  int    interval  [NCHANNEL];

  int maxInterval;
  int minInterval;

  int maxBPM;
  int minBPM;

  int  c;
  bool beep;
  
  double fx[4];

  SIGNAL ();
  void reset();
  void zero ();
  
}_SIGNAL;

struct SINUS
{
  double fx[NCHANNEL][NSAMPLES];

  SINUS();
};


#endif
