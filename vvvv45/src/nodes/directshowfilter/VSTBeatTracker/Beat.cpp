#include "Beat.h"

BASE::BASE()
{
  reset();
}

void BASE::reset()
{
  pos      = 0;
  value    = 0;
  distance = 0;
  valid    = false;
}

//----------------------------------//

Agent::Agent()
{ 
  reset();
}

void Agent::reset()
{
  interval = INTERVAL;

  n = 0;
  c = 0;

  beep   = false;
  active = false;
  value  = 0;
  valid  = 0;
}

void Agent::start(double p[])
{
  active = true;
  value  = p[ENTER];
  valid  = 1;
  
  c = 0;
  n = 1;
  
  base[0].pos   = ENTER;
  base[0].value = p[ENTER];
  base[0].valid = true;
    
}

void Agent::process(double p[])
{
  peak = p;

  shift   ();
  check   ();
  setBeep ();

}

void Agent::shift()
{
  for(int i=0;i<n;i++)
  base[at(i)].pos++;

  if(base[at(0)].pos>=MAXEMPTY)
  {
    value -= base[at(0)].value;

	if(base[at(0)].valid) valid--;

	if(valid==0) active=false;

    --n;
	++c %= NBASE;
  }

}

void Agent::check()
{
  int posCheck = base[at(n-1)].pos - interval;
  int posHit   = -1;

  beep = false;

  if(posCheck<START) 
  return; 


  for(int i=0;(i<=TOLERANCE) && (posHit<0);i++)
  {
    if(peak[posCheck+i]>0) posHit=posCheck+i;
	 else
	if(peak[posCheck-i]>0) posHit=posCheck-i;
  }

  if(posHit>0) 
  {
    base[at(n)].pos      = posHit;
	base[at(n)].value    = peak[posHit];
	base[at(n)].valid    = true; 
	base[at(n)].distance = (int) sqrt((double)((posHit-posCheck) * (posHit-posCheck)));
    
	value += base[at(n)].value;

	valid++;

  }
  else
  {
    base[at(n)].pos      = posCheck;
	base[at(n)].value    = 0;
	base[at(n)].valid    = false; 
	base[at(n)].distance = 0;
  }

  n++;

}

void Agent::setBeep()
{
  int threshold = 8;
  int distance  = 0;

  beep = false;

  if(base[at(n-1)].pos - interval != 0)  return;


  for(int i=n-1;i>=0;i--)
  {
    if(base[at(i)].valid)
	break;

	distance++;
  }  

  if(distance <= threshold)  beep = true; 

}


int Agent::at(int i)
{ 
  return (c+i) % NBASE;

}

//----------------------------------//

Beat::Beat()
{ 
  reset();
}

void Beat::reset()
{
  beep        = false;
  id          = 0;
  interval    = INTERVAL;
  valid       = 0;
  value       = 0;
  idOld       = 0;
  
  time.start     ();
  time.startIdle ();
}

void Beat::process(double signal[], int interval)
{
  peakpicker.process(signal,peak);

  for(int i=0;i<NAGENT;i++)
  if(agent[i].active)
  agent[i].process(peak);

  setInterval (interval);
  setCover    ();
  start       ();
  setBeep     ();
  setFX       ();
  setValue    ();

}

void Beat::setInterval(int interval)
{
   for(int i=0;i<NAGENT;i++)
   agent[i].interval = interval;

   this->interval = interval;

}

void Beat::setCover()
{
  for(int i=0;i<NFIELD;i++)
  {
    cover[i].id    = 0;
    cover[i].value = 0;
  }

  for(int a=0; a<NAGENT; a++)
  if(agent[a].active)
  for(int i=0; i<agent[a].n; i++)
  {
    int pos = agent[a].base[agent[a].at(i)].pos;

	if(cover[pos].value==0)
	{
      cover[pos].value = 1;
	  cover[pos].id    = a;
	}
	else
	{
	  if(agent[a].value > agent[cover[pos].id].value)
        agent[cover[pos].id].active = false;
	  else
	    agent[a].active = false;
	}

  }//end for

}

void Beat::start()
{
  if(peak[ENTER]>0 && cover[ENTER].value==0)
  for(int i=0;i<NAGENT;i++)
  if(!agent[i].active)
  {
   agent[i].start(peak);
   break;
  }

}

void Beat::setBeep()
{
  double topValue=0;

  id    = -1; 
  beep  = false;

  for(int i=0;i<NAGENT;i++)
  if((agent[i].active) && (topValue < agent[i].value))
  {
    topValue = agent[i].value;
	id       = i; 
  }

  valid = agent[id].valid;

  if(id>=0)
  {
    if(id==idOld)
    {
      beep = agent[id].beep;
	
	  time.start    ();
    }
	else time.startIdle();

    if(!time.stopInInterval(IDLETIME)) idOld = id;

  }//end

}

void Beat::setFX()
{
  double mean=0;
  double c=0;
  double threshold = 1.25; 


  for(int i=0;i<NFIELD;i++)
  if(peak[i]>0)
  {
    mean+=peak[i];
	c++;
  }

  mean/=c;

  if(peak[0]>mean*threshold)
   fx = 1.+peak[0];
  else
   fx = 0;

}

void Beat::setValue()
{
  value = time.idle()/1000000. * (double)agent[id].valid * (double)interval;

}

BeatDetection::BeatDetection()
{
  reset();
}

void BeatDetection::reset()
{
  for(int i=0;i<NCHANNEL;i++)
  beep[i] = false;

  channel = 0;
  idOld   = 0;

  time.start();
}

void BeatDetection::process(double signal[][NFIELD],int interval[],double resonance[][NRESONANCE],bool &beep,double fx[],bool optimize)
{
  int id = 0;


  int i=0;

  for(int i=0;i<NCHANNEL;i++)
  {
   if(optimize)
   if((i==1) || (i==3)) continue;

   beat[i].process(signal[i],interval[i]);

   if((interval[i] > 0) && (interval[i] < NRESONANCE))
   beat[i].value *= resonance[i][interval[i]];

   if(beat[i].value > beat[id].value) id = i;
 
  }//-------------------------------------------//
  
  beep = false;

  if(beat[id].beep)
  {
	if(id == idOld)
	{
      beep    = true;
      channel = id;

	  time.start();
	}
	else 
	if(!time.stopInInterval(IDLETIME)) idOld = id;

  }//end if beep--------------------------------//

}
