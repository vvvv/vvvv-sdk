/*-----------------------------------------------------------------------------------//

  the Mixer class generates a feedbacksignal 
  if the actual frame contains a beat

/------------------------------------------------------------------------------------*/

#ifndef MIXER_H
#define MIXER_H

#include <math.h>
#include <stdio.h>
#include <windows.h>
#include "Define.h"
#include "Time.h"

const double TIMEOUT  = 100000;
const double VOLMUSIC =    1.0;
const double VOLFX    =    1.0;
const bool   MUTE     =      1;

class Mixer//---------------------------------------------------------//
{
  private : void setMute     (double out[]);
		    void generateFX  (double out[]);
		    void signalOut   (double in [],double out[]);
		    void displayTime ();

		    bool  playMusic;
			bool  playFX;
		    bool  mute;

            int   freqFX;
		   
		    double volMusic;
			double volFX;

		    double fx [NCHANNEL][NSAMPLES];

		    Time  time;

  public :  Mixer ();
		    void process      (double in[], double out[], bool beep);
			void setMute      ();
			void setPlayFrame ();
			void setStop      ();

};

#endif