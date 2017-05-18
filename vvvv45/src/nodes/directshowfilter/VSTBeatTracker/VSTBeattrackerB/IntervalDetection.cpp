#include "IntervalDetection.h"

CHAIN::CHAIN()
{
  count    = 0;
  interval = 0;
  top      = false;
}

void CHAIN::init(int index,double value)
{
  count    = 0;
  interval = index;
  top      = false;

  set(index,value);
}

void CHAIN::set(int index,double value)
{
  link[count].index = index;
  link[count].value = value;

  count++;
}

int IntervalDetection::getMaximum(double field[])
{
  int index  = 0;
  double max = 0;

  for(int i=MININTERVAL;i<MAXINTERVAL;i++)
  if(field[i] > max)
  {
	max   = field[i];
	index = i;
  }

  return index;

}

int IntervalDetection::getSecondMaximum(double field[],int firstIndex)
{
  int index  = 0;
  double max = 0;

  for(int i=MININTERVAL;i<MAXINTERVAL;i++)
  if(field[i] > max)
  {
	int distance = sqrt((double)((i-firstIndex)*(i-firstIndex)));

	if(distance > AREA)
	{
	 max   = field[i];
	 index = i;
	}
  }

  return index;

}

void IntervalDetection::setChain(int interval,double field[],CHAIN &chain)
{
  int index = interval;

  chain.init(interval,field[index]);

  for(int i=0;i<32;i++)
  {
    index += interval;

	if(index+1 >= NRESONANCE) 
	  break;

	double newIndex = index;
	double newValue = field[index];

	if(field[index-1] > newValue)
	{
	  newIndex = index-1;
      newValue = field[index-1];
	}

	if(field[index+1] > newValue)
	{
	  newIndex = index+1;
	  newValue = field[index+1];
	}

	chain.set(newIndex,newValue);

	index = newIndex;
  }

}

bool IntervalDetection::tripleInterval(int *interval,CHAIN &chain0,CHAIN &chain1)
{
  	if(chain0.link[2].index == chain1.link[3].index)
	{
      //*interval= chain0.interval;
	  *interval  = chain0.link[2].index;
	  chain0.top = true;
	  return true;
	}

	if(chain1.link[2].index == chain0.link[3].index)
	{
      //*interval= chain1.interval;
	  *interval  = chain1.link[2].index;
	  chain1.top = true;
	  return true;
	}

	return false;
}

int IntervalDetection::getMaxChain(CHAIN &chain0,CHAIN &chain1)
{
  int count = chain0.count;

  if(chain1.count < count)
	count = chain1.count;

  double value0 = 0;
  double value1 = 0;

  for(int i=0;i<count;i++)
  {
	value0 += chain0.link[i].value;
	value1 += chain1.link[i].value;
  }

  int interval;

  if(value0 >= value1)
  {
	chain0.top = true;
	interval   = chain0.interval;
  }
  else
  {
	chain1.top = true;
	interval   = chain1.interval;
  }

  if(interval >= MININTERVAL && interval < MAXINTERVAL) //???why???
    return interval;

  return 0;

}

int IntervalDetection::getNearest(double peak[],int targetInterval)
{
  int mindistance = 1000000;
  int interval    = 0;

  for(int i=MININTERVAL;i<MAXINTERVAL;i++)
  if(peak[i] > 0)
  {
    int distance = sqrt((double)((i - targetInterval) * (i - targetInterval)));

	if(distance < mindistance)
	{
	  mindistance = distance;
	  interval    = i;
  	}

  }

  return interval;

}

void IntervalDetection::process(double **resonance, int *interval, int targetInterval)
{
  PEAKPICKER peakpicker;

  for(int i=0;i<NBANDS;i++)
  if(bandSwitch[i])
  {
	if((targetInterval >= MININTERVAL) && (targetInterval < MAXINTERVAL))
	{
      double peaks[NRESONANCE];

	  peakpicker.process(resonance[i],peaks,NRESONANCE);

	  interval[i] = getNearest(peaks,targetInterval);
	}
	else
	{
	  int index = getMaximum(resonance[i]);

	  setChain(index,resonance[i],chain[i][0]);

	  index = getSecondMaximum(resonance[i],index);

	  setChain(index,resonance[i],chain[i][1]);

	  interval[i] = getMaxChain(chain[i][0],chain[i][1]);
	}

  }//end for i

}


