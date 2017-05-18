#include "Resonator.h"

COMBFILTER::COMBFILTER()
{
  reset();
}

void COMBFILTER::reset()
{
  for(int i=0;i<NFIELD;i++)
  v[i] = v0[i] = v_[i] = 0;

  for(int i=0;i<NRESONANCE;i++)
  {
	  t[i] = g[i] = g_[i] = 0;

	for(int k=0;k<NFIELD;k++)
	r [i][k] =  r_ [i][k] = s [i][k] = 0;
  }

  //double framerate = 93.75;//------------------------------------------------------------------------------------
	double framerate=FRAMERATE;

  //gain of the signal--------------------------------//
  for(int i=0;i<NRESONANCE;i++)
  {
	double delay    = (double) i;
	double exponent = delay/(1.5*framerate);

	g[i]  = pow(0.5,exponent);
  }

  //overall power of the combfilter-------------------//
	for(int i=1;i<NRESONANCE;i++)
	{
	  g_[i]=((1-g[i])*(1-g[i])) / (1-(g[i]*g[i]));
	double factor=1/(1.-g_[i]);
	}

  meanFactor = 1.4;
  mean       = 0;
}

void COMBFILTER::process(int c,int cm1,int cp1,double onset,double resonance[])
{

	v[c]=onset;

	//mean of the onset-array------------------------------------------//
	mean += v[c]/NFIELD;
	mean -= v[cp1]/NFIELD;

	//cut off noise----------------------------------------------------//
    if(v[c]>mean*meanFactor)
     v0[c]=v[c];
	else
     v0[c]=0;

	//combfilter-------------------------------------------------------//
	for(int d=0;d<NRESONANCE;d++)
	{
	  int delayIndex=c-d;
	  if(delayIndex<0) delayIndex+=NFIELD;

      r[d][c]=(g[d]*r[d][delayIndex])+((1.-g[d])*v0[c]);

	}

	//current energy of the combfilter---------------------------------//
    for(int d=1;d<NRESONANCE;d++)
    {
	  double sum=0;

      for(int n=0;n<d-1;n++)
	  {
	   int delayIndex = c-n;

	   if(delayIndex<0) 
	   delayIndex+=NFIELD;

	   sum+=r[d][delayIndex]*r[d][delayIndex];

	  }

	  r_[d][c]=sum/(double)d;

    }

	//leaky integrator-------------------------------------------------//
    v_[c]=g[1]*v_[cm1]+(1.-g[1])*(v[c]*v[c]);

	//normalization----------------------------------------------------//
    for(int d=0;d<NRESONANCE;d++)
	{
     double tmp=(1/(1-g_[d]))*((r_[d][c]/v_[c])-g_[d]);

     if(tmp>0)
	  s[d][c]=tmp;
	 else
	  s[d][c]=0;
	
	}

   //resonance over 1024 samples---------------------------------------//
   //this is the data which is used in next steps of the processing----//
   for(int d=0;d<NRESONANCE;d++)
   {
	 t[d]-=s[d][cp1]/NFIELD;

	 if(t[d]<0) t[d]=0;

	 if(s[d][c]>0)
     t[d]+=s[d][c]/NFIELD;

	 resonance[d]=t[d];
   }
   
}

Resonator::Resonator()
{
  reset();   
}

void Resonator::reset()
{
  for(int h=0;h<NCHANNEL;h++)
  combfilter[h].reset();
}

void Resonator::process(double onset[][NFIELD],double resonance[][NRESONANCE],bool optimize)
{
	//the count variable
    static int c=0;

	int cm1 = c-1;             //one element before the actual element
	int cp1 = (c+1)%NFIELD;  //the next element 

    if(cm1<0) cm1+=NFIELD;   // if c==0 cm1=1023

	//combfilter-banks for the 4 channels
    for(int h=0;h<NCHANNEL;h++)
	{
      if(optimize)
      if((h==1) || (h==3)) continue;

	  combfilter[h].process(c,cm1,cp1,onset[h][0],resonance[h]);
	}

	//Achtung!!
	++c%=NFIELD;
	//c = (c+2)%NFIELD;

}
