#include "Feedback.h"

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
  this->active    = true;
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

FEEDBACKBLOCK::FEEDBACKBLOCK()
{
  nFrames   = NFRAMES;
  startTime = 0;
  endTime   = 0;
  frameTime = 0;

}

void FEEDBACKBLOCK::set(LONGLONG startTime, LONGLONG endTime, double frameTime, int nFrames)
{
  for(int i=0;i<nFrames;i++)
  {
	double t = startTime + (i*frameTime);

    frame[i].setTime(t, t+(frameTime/2.), t+frameTime);
  }

  this->startTime = startTime;
  this->endTime   = endTime;
  this->frameTime = frameTime;
  this->nFrames   = nFrames;

}

bool FEEDBACKBLOCK::check(long streamTime)
{
  if(streamTime >= startTime && streamTime < endTime) 
   return true;

  return false;
}

//-----------------------------------------------------------------------------//

CFeedback::CFeedback()
{
  c                = 0;
  beatTime         = 0; 
  beatInterval     = 0;
  beatCounter      = 0;
  beatStoreCounter = 0;
  bpm              = 0;

  for(int i=0;i<NBEAT;i++)
  beatStore[i] = 0;

  for(int i=0;i<NCHANNEL;i++)
  liveFx[i] = 0;

}

//is called after all the frames of one block are read-------------------------------------//
void CFeedback::setBlock(LONGLONG startTime, LONGLONG endTime, double frameTime, int nFrames)
{
  block[c].set(startTime, endTime, frameTime, nFrames);

  this->frameTime = frameTime;
  
  ++c %= NBLOCKS;
}

void CFeedback::setStreamFrame(FEEDBACKFRAME *ptr,int i)
{
  if(ptr!=NULL)
  {
	block[c].frame[i].set(ptr);

    bpm = ptr->bpm;
  }

}

void CFeedback::getStream(int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,int *bpm,LONGLONG streamTime)
{
  getStreamZero   (opcode,beat,phase,fx0,fx1,fx2,fx3,streamTime);

  getStreamActual (opcode,beat,phase,fx0,fx1,fx2,fx3,streamTime);

  *opcode = 1;
  *beat   = beatCounter;
  *phase  = getStreamPhase(streamTime);
  *bpm    = this->bpm;

}

void CFeedback::getStreamZero(int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,LONGLONG streamTime)
{
  for(int i=0;i<NBLOCKS;i++)
  for(int k=0;k<NFRAMES;k++)
  {
	if(block[i].frame[k].endTime != 0 && streamTime > block[i].frame[k].endTime)
	{
	    if(block[i].frame[k].beat)
	    {
	      ++beatCounter %= BEATPERIOD;

          beatTime     = block[i].frame[k].midTime;
		  beatInterval = block[i].frame[k].interval; 
	    }
	    
	    *fx0 = block[i].frame[k].fx[0] / 5.0;
        *fx1 = block[i].frame[k].fx[1] / 5.0;
        *fx2 = block[i].frame[k].fx[2] / 5.0;
        *fx3 = block[i].frame[k].fx[3] / 5.0;

		if(*fx0 > 1.0) *fx0 = 1.0;
		if(*fx1 > 1.0) *fx1 = 1.0;
		if(*fx2 > 1.0) *fx2 = 1.0;
		if(*fx3 > 1.0) *fx3 = 1.0;

		if(*fx0 < 0.0) *fx0 = 0.0;
		if(*fx1 < 0.0) *fx1 = 0.0;
		if(*fx2 < 0.0) *fx2 = 0.0;
		if(*fx3 < 0.0) *fx3 = 0.0;

 	    block[i].frame[k].reset();
	}

  }//end for 


}


void CFeedback::getStreamActual(int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,LONGLONG streamTime)
{  
  for(int i=0;i<NBLOCKS;i++)
  for(int k=0;k<NFRAMES;k++)
  {
	  if((streamTime >= block[i].frame[k].startTime) && (streamTime < block[i].frame[k].endTime))
	  {
		if(block[i].frame[k].beat) 
		{
  	      ++beatCounter %= BEATPERIOD;

          beatTime     = block[i].frame[k].midTime;
		  beatInterval = block[i].frame[k].interval; 
		}

	    *fx0 = block[i].frame[k].fx[0] / 5.0;
        *fx1 = block[i].frame[k].fx[1] / 5.0;
        *fx2 = block[i].frame[k].fx[2] / 5.0;
        *fx3 = block[i].frame[k].fx[3] / 5.0;

		if(*fx0 > 1.0) *fx0 = 1.0;
		if(*fx1 > 1.0) *fx1 = 1.0;
		if(*fx2 > 1.0) *fx2 = 1.0;
		if(*fx3 > 1.0) *fx3 = 1.0;

		if(*fx0 < 0.0) *fx0 = 0.0;
		if(*fx1 < 0.0) *fx1 = 0.0;
		if(*fx2 < 0.0) *fx2 = 0.0;
		if(*fx3 < 0.0) *fx3 = 0.0;



 	    block[i].frame[k].reset();
	  }

  }//end for  

}

double CFeedback::getStreamPhase(LONGLONG streamTime)
{
  if((beatTime == 0) || (beatInterval == 0)) return -1;

  double now        = (double) streamTime;

  double difference = now - beatTime; 

  double period     = beatInterval * frameTime;

  double fraction   = difference / period;

  double phase      = fmod(fraction,1.0);

  //if(difference > 20000000) phase = -1; //no fresh beat since two seconds
  if(difference > period * 1.5) phase = -1; // no fresh beat since two seconds

  return phase;

}


//if the source is line-in or microphone--------------------------------------------------//
void CFeedback::setLiveFrame(FEEDBACKFRAME *ptr,long streamTime,double frameTime, int delay)
{
  if(ptr->beat)
  {
    beatTime     = (double) streamTime;
    beatInterval = (double) ptr->interval;
	bpm          = ptr->bpm;

    beatDelay    = (double) delay;

    this->frameTime = frameTime;

    beatStore[beatStoreCounter] = (beatTime - beatDelay) + (beatInterval * frameTime);

    ++beatStoreCounter %= NBEAT;
  }

  for(int i=0;i<NCHANNEL;i++)
  liveFx[i] = ptr->fx[i];

}

double CFeedback::getLivePhase(long streamTime)
{
  if((beatTime == 0) || (beatInterval == 0)) return -1;


  double delayedBT  = beatTime - beatDelay; 

  double now        = (double) streamTime;

  double difference = now - delayedBT; 

  double period     = beatInterval * frameTime;

  double fraction   = difference / period;

  double phase      = fmod(fraction,1.0);

  //if(difference > 2000) phase = -1; // no fresh beat since two seconds
  if(difference > period * 1.5) phase = -1; // no fresh beat since two seconds

  return phase;

}

int CFeedback::getLiveBeat(long streamTime)
{
  for(int i=0; i<NBEAT; i++)
  if((beatStore[i] < (double)streamTime) && (beatStore[i] != 0))
  {
    beatStore[i] = 0;
	
	++beatCounter %= BEATPERIOD;
  }

  return beatCounter;
}

void CFeedback::getLive(int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,int *bpm,long streamTime)
{
  *opcode = 1;
  *phase  = getLivePhase(streamTime);
  *beat   = getLiveBeat (streamTime);
  *bpm    = this->bpm;

  getLiveOnset(fx0,fx1,fx2,fx3);

}

void CFeedback::getLiveOnset(double *fx0,double *fx1,double *fx2,double *fx3)
{
    *fx0 = liveFx[0] / 5.0;
    *fx1 = liveFx[1] / 5.0;
    *fx2 = liveFx[2] / 5.0;
    *fx3 = liveFx[3] / 5.0;

	if(*fx0 > 1.0) *fx0 = 1.0;
	if(*fx1 > 1.0) *fx1 = 1.0;
	if(*fx2 > 1.0) *fx2 = 1.0;
	if(*fx3 > 1.0) *fx3 = 1.0;

	if(*fx0 < 0.0) *fx0 = 0.0;
	if(*fx1 < 0.0) *fx1 = 0.0;
	if(*fx2 < 0.0) *fx2 = 0.0;
	if(*fx3 < 0.0) *fx3 = 0.0;

}

void CFeedback::reset()
{
  for(int i=0;i<NBLOCKS;i++)
  for(int k=0;k<NFRAMES;k++)
  block[i].frame[k].reset();

  beatTime     = 0;
  beatCounter  = 0;
  beatInterval = 0;

  for(int i=0;i<NBEAT;i++)
  beatStore[i] = 0;

}//reset

