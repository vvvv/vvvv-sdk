#include "Resonance.h"

Combfilter::Combfilter()
{
  reset();
}

void Combfilter::reset()
{
  mean  = 0;
  
  for(int i=0;i<NSAMPLES;i++)
  {
	onsetCut        [i] = 0;
    leakyIntegrate  [i] = 0;
  }

  for(int i=0;i<NRESONANCE;i++)
  {
	resonanceOverall[i] = 0;
	gainOverall     [i] = 0;
	
	responseToggle  [i] = false;
    responseSum     [i] = 0;

	for(int k=0;k<NSAMPLES;k++)
    {
     response       [i][k] = 0;
	 responseEnergy [i][k] = 0;
	 normalize      [i][k] = 0;
    }
 
	double exponent = (double)i / (1.5 * FRAMERATE);

	gain[i] = pow(0.5,exponent);
  }

  for(int i=1;i<NRESONANCE;i++)
	gainOverall[i] = ((1-gain[i]) * (1-gain[i])) / (1 - (gain[i] * gain[i]));

}

void Combfilter::process(int count,double onset[],double resonance[])
{
  int countAdd =  (count + 1) % NSAMPLES;
  int countSub = ((count - 1) + NSAMPLES) % NSAMPLES;

  mean += onset[count]    / NSAMPLES;
  mean -= onset[countAdd] / NSAMPLES;

  if(onset[count] > mean * MEANFACTOR)
   onsetCut[count] = onset[count];
  else
   onsetCut[count] = 0;


  leakyIntegrate[count] = gain[1] * leakyIntegrate[countSub] + ((1-gain[1]) * (onset[count] * onset[count]));


  for(int i=0;i<NRESONANCE;i++)
  //for(int i=MININTERVAL;i<MAXINTERVAL;i++)
  {
    int delay = count - i;

	if(delay < 0)
	  delay += NSAMPLES;

	response[i][count] = (gain[i] * response[i][delay]) + ((1.-gain[i]) * onsetCut[count]);
  
	//-----------------------------------------------------------------------------------//

	if(i>0)
	{
	 responseSum[i] += response[i][count] * response[i][count];

     int delaySum = ((count-i) + NSAMPLES) % NSAMPLES;

	 if(delaySum == 0)
	  responseToggle[i] = true;

	 if(responseToggle[i])
	  responseSum[i] -= response[i][delaySum] * response[i][delaySum];

	 responseEnergy[i][count] = responseSum[i] / (double)i;

	}

    //-----------------------------------------------------------------------------------//

    double tmp = (1 / (1-gainOverall[i])) * ((responseEnergy[i][count] / leakyIntegrate[count]) - gainOverall[i]);

	if(tmp > 0)
	  normalize[i][count] = tmp;
	else
	  normalize[i][count] = 0;


	resonanceOverall[i] -= normalize[i][countAdd] / NSAMPLES;

	if(resonanceOverall[i] < 0) 
	  resonanceOverall[i] = 0;

	if(normalize[i][count] > 0)
	  resonanceOverall[i] += normalize[i][count] / NSAMPLES;


	resonance[i] = resonanceOverall[i];

  }//end for i
 
}

Resonance::Resonance()
{
 
}

void Resonance::reset()
{
  for(int i=0;i<NBANDS;i++)
   combfilter[i].reset();
}

void Resonance::process(int count,double **onset,double **resonance)
{
  for(int i=0;i<NBANDS;i++)
  if(bandSwitch[i])
   combfilter[i].process(count,onset[i],resonance[i]);

}