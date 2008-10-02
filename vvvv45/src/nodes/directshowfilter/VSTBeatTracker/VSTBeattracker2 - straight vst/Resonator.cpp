#include "Resonator.h"

#define FRAMERATE 172.265625

Combfilter::Combfilter()
{
  for(int i=0;i<NSAMPLES;i++)
  v0[i] = v_[i] = 0;

  for(int i=0;i<NRESONANCE;i++)
  {
	  t[i] = g[i] = g_[i] = 0;

	for(int k=0;k<NSAMPLES;k++)
	r [i][k] =  r_ [i][k] = s [i][k] = 0;
  }

  double framerate=FRAMERATE;

  for(int i=0;i<NRESONANCE;i++)
  {
	double delay    = (double) i;
	double exponent = delay/(1.5*framerate);

	g[i]  = pow(0.5,exponent);
  }

  for(int i=1;i<NRESONANCE;i++)
    g_[i]=((1-g[i])*(1-g[i])) / (1-(g[i]*g[i]));

  meanFactor = 1.4;
  mean       = 0;

}

void Combfilter::process(double onset[NSAMPLES],double resonance[],int count)
{
    int cm1 = count - 1;
	int cp1 = (count + 1) % NSAMPLES;

	if(cm1 == -1) 
	  cm1 = NSAMPLES - 1;

	double test[NSAMPLES];

	FORLOOP
	  test[i] = onset[i];

	//mean of the onset-array------------------------------------------//
	mean += (onset[count] - onset[cp1]) / NSAMPLES;

	//cut off noise----------------------------------------------------//
    if(onset[count] > mean*meanFactor)
     v0[count] = onset[count];
	else
     v0[count] = 0;

	//combfilter-------------------------------------------------------//
	for(int d=0;d<NRESONANCE;d++)
	{
	  int delayIndex=count-d;
	  if(delayIndex<0) delayIndex+=NSAMPLES;

	  //Feedback-Kammfilter: der aktuelle Wert plus dem Ausgabewert um die Verzögerung versetzt
      r[d][count]=((1.-g[d])*v0[count]) + (g[d]*r[d][delayIndex]);

	}

	//current energy of the combfilter---------------------------------//
    for(int d=1;d<NRESONANCE;d++)
    {
	  double sum=0;

   //   for(int n=0;n<d-1;n++)
	  //{
	  // int delayIndex = count - n;

	  // if(delayIndex<0) 
	  // delayIndex+=NSAMPLES;

	  // sum+=r[d][delayIndex]*r[d][delayIndex];

	  //}

	  r_[d][count]=sum/(double)d;

    }

	//leaky integrator-------------------------------------------------//
    v_[count]=g[1]*v_[cm1]+(1.-g[1])*(onset[count]*onset[count]);


	//normalization----------------------------------------------------//
    for(int d=0;d<NRESONANCE;d++)
	{
     double tmp=(1/(1-g_[d]))*((r_[d][count]/v_[count])-g_[d]);

     if(tmp>0)
	  s[d][count]=tmp;
	 else
	  s[d][count]=0;
	
	}

   //resonance over 1024 samples---------------------------------------//
   //this is the data which is used in next steps of the processing----//
   for(int d=0;d<NRESONANCE;d++)
   {
	 t[d]-=s[d][cp1]/NSAMPLES;

	 if(t[d]<0) 
	   t[d]=0;

	 if(s[d][count]>0)
       t[d]+=s[d][count]/NSAMPLES;

	 resonance[d]=t[d];
   }

}

Resonator::Resonator()
{


}

void Resonator::process(double onset[][NSAMPLES],double resonance[][NRESONANCE])
{
  static int count = 0;

  double channel[NCHANNEL][NSAMPLES];

  for(int i=0;i<NSAMPLES;i++)
  {
    channel[0][i] = (onset[0][i] + onset[1][i]) / 2.;

	channel[1][i] = (onset[2][i] + onset[3][i]) / 2.;

	channel[2][i] = (onset[4][i] + onset[5][i] + onset[6][i]) / 3.;
  }

  for(int i=0;i<NCHANNEL;i++)
	combfilter[i].process(channel[i],resonance[i],count);

  ++count %= NSAMPLES;

} 
