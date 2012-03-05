#ifndef FEEDBACK_H
#define FEEDBACK_H

#include <stdio.h>
#include <windows.h>
#include <math.h>
#include "GlobalDefine.h"

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

struct FEEDBACKBLOCK
{
  FEEDBACKBLOCK ();
  void get   (int opcode[],INT64 time[],INT64 beat[],double phase[],double fx0[],double fx1[],double fx2[],double fx3[],int bpm,long streamTime);
  void set   (LONGLONG startTime, LONGLONG endTime, double frameTime, int nFrames);
  bool check (long streamTime);
  
  FEEDBACKFRAME frame[NFRAMES];

  int    nFrames;
  double frameTime;

  LONGLONG startTime;
  LONGLONG endTime;

};

//-----------------------------------------------------------------------------//

class CFeedback
{
  public: FEEDBACKBLOCK block[NBLOCKS];

		  int c;
		  int beatCounter;
		  int beatStoreCounter;
		  int bpm;

		  double beatTime;
		  double beatInterval;
		  double beatDelay;
		  double beatStore[NBEAT];
		  double frameTime;
		  double liveFx[4];

		  double testtime;
		         
		  CFeedback ();
		  void   setBlock        (LONGLONG startTime, LONGLONG endTime, double frameTime, int nFrames);
		  void   getStream       (int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,int *bpm,LONGLONG streamTime);
		  void   getStreamZero   (int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,LONGLONG streamTime);
          void   getStreamActual (int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,LONGLONG streamTime);
		  void   setStreamFrame  (FEEDBACKFRAME* f,int i);
		  double getStreamPhase  (LONGLONG streamTime);
		  void   getLive         (int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,int *bpm,long streamTime);
		  void   setLiveFrame    (FEEDBACKFRAME *ptr, long streamTime, double frameTime, int delay);
          double getLivePhase    (long streamTime);	
		  int    getLiveBeat     (long streamTime);
		  void   getLiveOnset    (double *fx0,double *fx1,double *fx2,double *fx3);
		  void   reset           ();



};

#endif