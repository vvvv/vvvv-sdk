#include "Mixer.h"


Mixer::Mixer()
{
  playFX    = true; 
  playMusic = true;
  freqFX    = 0;
  mute      = MUTE;
  volMusic  = VOLMUSIC;
  volFX     = VOLFX;

  time.start();

  //setup fx----------------------------//
  double cycle=2*3.1415926;
  double div[]={295,71,27,7};
  double rad;

  //a sinuswave with some random noise
  for(int i=0;i<NCHANNEL;i++)
  {
    rad=(2*PI)/div[i];

    for(int k=0;k<NSAMPLES;k++)
	{
	  fx[i][k] =sin(rad*k);
	 
	  if(i>0)
	  fx[i][k]+=(double)(rand()%100)/100.;
	}
  }

}

//of beep is true a feedback-signal is generated
void Mixer::process(double in[],double out[],bool beep)
{
  for(int i=0;i<NSAMPLES;i++)
  out[i] = fx[freqFX][i] * 0.2;

  /*
  if(playMusic) signalOut(in,out);
  
  if(mute) 
   setMute(out);
  else
   if(beep & !time.stopInInterval(TIMEOUT) & playFX)  //the time interval between two 
   {                                                  //beats must be bigger than TIMEOUT (0.1 sec)
     generateFX (out);
  	 time.start (); 
   }
  */

}

void Mixer::setMute(double out[])
{
  for(int i=0;i<NSAMPLES;i++)
  out[i] = 0;

}

void Mixer::generateFX(double out[])
{
  for(int i=0;i<NSAMPLES;i++)
  out[i] = fx[freqFX][i] * 0.2;

}

void Mixer::signalOut(double in[],double out[])
{
  for(int i=0;i<NSAMPLES;i++)
  out[i] = in[i] * volMusic;
 
}

//to control when the detected beat is really played. to detect delays
void Mixer::displayTime()
{
 static double lastTime    = 0;
 double currentTime = time.micros();

 if(lastTime!=0) printf("DIFFERENCE %12.0f BPM %8.0f\n",(currentTime-lastTime)/1000.,60000000./(currentTime-lastTime));

 lastTime = currentTime;

}

void Mixer::setMute ()
{
  mute = !mute;
}

