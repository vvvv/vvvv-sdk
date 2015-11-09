#include "BeatDetection.h"

TICK::TICK()
{
  position = 0;
  value    = 0;
}

Ticklist::Ticklist()
{
  reset();
}

void Ticklist::reset()
{
  count  = 0;
  first  = 0;
  weight = 0;
}

void Ticklist::update()
{
  for(int i=0;i<count;i++)
  {
   int index = (first + i) % NTICKS;

   list[index].position++;

   if(list[index].position >= NSAMPLES)
   {
	 count--;
     first = (index+1) % NTICKS;
   }
  
  }//end for i

}

int Ticklist::getPositionStraight(int i)
{
  int index = (first + i) % NTICKS;

  return list[index].position;
}

double Ticklist::getValueStraight(int i)
{
  int index = (first + i) % NTICKS;

  return list[index].value;
}

void Ticklist::init(int position,double value)
{
  count  = 0;
  first  = 0;
  weight = 0;
    
  set(position,value);
}

void Ticklist::set(int position,double value)
{
  if(count>=NTICKS)
	return;

  int actualIndex = (first+count) % NTICKS;

  list[actualIndex].position = position;
  list[actualIndex].value    = value;

  ++count;
}

void Ticklist::calcWeight()
{
  weight = 0;

  for(int i=0;i<count;i++)
  {
   int index = (first + i) % NTICKS;

   weight += list[index].value;  
  }

}

//--------------------------------------------------------------------//

Agent::Agent()
{
  reset();
}

void Agent::reset()
{
  position = 0;
  interval = 0;
  active   = false;

  tick.reset();
}

void Agent::start(int position,double value)
{
  this->position = position;
  this->interval = position;
  this->active   = true;

  tick.init(position,value);
}

void Agent::process(int interval,double field[])
{
  this->interval = interval;//???

  if(++position >= NSAMPLES)
	active = false;

  int index = position - interval;

  if(index == 0)
	beat = true;
  else
	beat = false;

  tick.update();

  if(index == START) 
  {
    double max = field[index];
    int newIndex = index;

	if(field[index-2]>max)
	{
	  max = field[index-2];
	  newIndex = index-2;
	}

	if(field[index-1]>max)
	{
	  max = field[index-1];
	  newIndex = index-1;
	}

  	if(field[index+2]>max)
	{
	  max = field[index+2];
	  newIndex = index+2;
	}

  	if(field[index+1]>max)
	{
	  max = field[index+1];
	  newIndex = index+1;
	}

	position = newIndex;

	tick.set(position,max);
	
  }//end if index 

  tick.calcWeight();

}

//--------------------------------------------------------------------//

Agentlist::Agentlist()
{
  reset();
}

void Agentlist::reset()
{
  top            = -1;
  beat           = false;
  bpm            = 0;
  weight         = 0;
  count          = 0;

  for(int i=0;i<NAGENTS;i++)
	agent[i].reset();
}

void Agentlist::process(SIGNALBAND *band)
{
  this->band = band;

  for(int i=0;i<NAGENTS;i++)
  if(agent[i].active)
	agent[i].process(band->interval,band->onsetStraight);

}

void Agentlist::start()
{
  for(int i=0;i<NAGENTS;i++)
  if(!agent[i].active)
  {
	agent[i].start(band->interval,band->onset[band->interval]);
	break;
  }

}

void Agentlist::clean()
{
  for(int i=0;i<NAGENTS;i++)
  for(int k=0;k<NAGENTS;k++)
  if(i!=k)
  if(agent[i].active && agent[k].active)
  {
	int distance = sqrt((double)((agent[i].position - agent[k].position) * (agent[i].position - agent[k].position)));

	if(distance < AREA)
	{
	  if(agent[i].tick.weight > agent[k].tick.weight)
		agent[k].active = false;
	  else
		agent[i].active = false;
	}

  }

}

void Agentlist::getTop()
{
  double max = 0;
  
  int actualTop = -1;
  
  for(int i=0;i<NAGENTS;i++)
  if(agent[i].active)
  if(agent[i].tick.weight > max)
  {
	max = agent[i].tick.weight;

	actualTop = i;
  }

  if(max==0 || actualTop == -1) 
	return;

  int oldTop = top;

  if(top == -1)
	top = actualTop;

  if(agent[actualTop].tick.weight > agent[top].tick.weight * WEIGHTFACTOR) 
    top = actualTop;

  if(agent[top].beat)
	count++;

  if(oldTop != top)
	count = 0;
  
}

void Agentlist::getInfo()
{
  for(int i=0;i<NSAMPLES;i++)
    band->cover[i] = -1;

  for(int i=0;i<NAGENTS;i++)
  if(agent[i].active)
  {
	for(int k=0;k<agent[i].tick.count;k++)
	{
	  int index = agent[i].tick.getPositionStraight(k);

	  band->cover[index] = i;
	}
  }

  if(top >= 0)
  {
    beat   = agent[top].beat;

	weight = agent[top].tick.weight * band->interval;
  }
  else
  {
    beat   = false;

	weight = 0;
  }
  
}

//--------------------------------------------------------------------//

BeatDetection::BeatDetection()
{


}

void BeatDetection::reset()
{
  for(int i=0;i<NBANDS;i++)
	agentlist[i].reset();
}

void BeatDetection::process(Signal &signal)
{
  double max  = 0;
  
  signal.beat = false;
  signal.bpm  = 0;

  int oldTop      = signal.top;
  int oldTopAgent = 0;
  int newTop      =-1;

  if(signal.top >= 0)
	oldTopAgent = agentlist[signal.top].top;

  //------------------------------------------------------------------------------------------

  for(int i=0;i<NBANDS;i++)
  if(bandSwitch[i])
  {
	if(signal.interval[i] < MININTERVAL || signal.interval[i] >= MAXINTERVAL)
	  continue;

	int distance = (int)(sqrt((double)((signal.interval[i] - signal.band[i].interval) * (signal.interval[i] - signal.band[i].interval))));

	if(distance >= AREA)
	  agentlist[i].reset();
	  
	signal.band[i].update(signal.interval[i]);

	agentlist[i].process (&signal.band[i]); 

	if(signal.band[i].localMaximum())
	  agentlist[i].start ();

	agentlist[i].clean   ();

	agentlist[i].getTop  ();

	agentlist[i].getInfo ();


	if(agentlist[i].weight > max)
	{
	  max = agentlist[i].weight;
    
	  newTop = i;
	}
	

  }//end for NBANDS

  //------------------------------------------------------------------------------------------

  //if no processing took place
  if(newTop == -1)
	return;

  if(oldTop == -1)
    oldTop = newTop;

  int finalTop = 0;

  if(agentlist[newTop].weight > agentlist[oldTop].weight * WEIGHTFACTOR)
    finalTop = newTop;
  else
    finalTop = oldTop;

  signal.beat = agentlist[finalTop].beat;

  signal.topInterval = signal.interval[finalTop];

  signal.top = finalTop;

  //------------------------------------------------------------------------------------------

  //calculate the probability
  bool toggle = ((oldTop == signal.top) && (oldTopAgent == agentlist[signal.top].top));

  if(!toggle)
  {
	Agent *oldAgent = &agentlist[oldTop].agent[oldTopAgent];

	Agent *newAgent = &agentlist[signal.top].agent[agentlist[signal.top].top];

	int distance = sqrt((double)((oldAgent->interval - newAgent->interval) * (oldAgent->interval - newAgent->interval)));

	if(distance < AREA)
	{
  	  distance = sqrt((double)((oldAgent->position - newAgent->position) * (oldAgent->position - newAgent->position)));

	  if(distance < AREA)
	    toggle = true;
	  else
	  {
        distance = sqrt((double)((distance - newAgent->interval) * (distance - newAgent->interval)));

		if(distance < AREA)
		  toggle = true;
	  }
	}

  }//end if !toggle

  if(toggle)
	signal.probabilityCount++;
  else
	signal.probabilityCount = 0;

  signal.probability = (((double)signal.probabilityCount) / NSAMPLES);

  if(signal.probability > 1)
	signal.probability = 1;
  
}

