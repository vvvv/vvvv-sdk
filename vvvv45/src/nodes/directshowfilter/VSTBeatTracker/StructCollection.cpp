#include "StructCollection.h"

//-----------------------------------------------------------------------------//

SIGNAL::SIGNAL()
{
  reset();
}

void SIGNAL::zero()
{
  beep = false;

  for(int h=0;h<NCHANNEL;h++)
  fx[h]       = 0;
}

void SIGNAL::reset()
{
  for(int i=0;i<NSAMPLES;i++)
  freq[i] = 0;

  for(int h=0;h<NCHANNEL;h++)
  for(int i=0;i<NFIELD;i++)
  onset[h][i]    = 0;  

  beep = false;
  c    = 0;

  for(int h=0;h<NCHANNEL;h++)
  {
   fx[h]       = 0;
   interval[h] = NFIELD;
  }

  maxBPM = MAXBPM;
  minBPM = MINBPM;

}

//-----------------------------------------------------------------------------//

FEEDBACKFRAME::FEEDBACKFRAME()
{
  reset();
}

void FEEDBACKFRAME::reset()
{
  beat      =  0;
  interval  =  0;
  startTime =  0;
  endTime   =  0;
  midTime   =  0;
  phase     = -1;
  active    = false;
  bpm       =  0;
  
  for(int i=0;i<NCHANNEL;i++)
   fx[i] = 0;

}

void FEEDBACKFRAME::set(bool beat,int interval,double fx[],int bpm)
{
  this->beat      = beat;
  this->interval  = interval;
  this->startTime = 0;
  this->endTime   = 0;
  this->midTime   = 0;
  this->bpm       = bpm;
  
  for(int i=0;i<NCHANNEL;i++)
   this->fx[i] = fx[i];

}

//copy whole frame---------------------------------------------------------------//
void FEEDBACKFRAME::set(FEEDBACKFRAME* other)
{
  beat      = other->beat;
  interval  = other->interval;
  startTime = other->startTime;
  endTime   = other->endTime;
  midTime   = other->midTime;
  bpm       = other->bpm;

  for(int i=0;i<NCHANNEL;i++)
   fx[i] = other->fx[i];

}

void FEEDBACKFRAME::setTime(LONGLONG startTime, LONGLONG midTime, LONGLONG endTime)
{
  this->startTime = startTime;
  this->midTime   = midTime;
  this->endTime   = endTime;

  active = true;

}

bool FEEDBACKFRAME::check(LONGLONG streamTime)
{
  if(streamTime >= startTime && streamTime < endTime) return true;

  return false;
}

//-----------------------------------------------------------------------------//

SINUS::SINUS()
{
  double cycle=2*3.1415926;
  double div[]={128,71,27,7};
  double rad;

  for(int i=0;i<NCHANNEL;i++)
  {
    rad=(2*PI)/div[i];  

    for(int k=0;k<NSAMPLES;k++)
    fx[i][k]  = sin(rad*k);
  }
  
}//end SINUS

