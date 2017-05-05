#include "OnsetDetection.h"

ONSETBAND::ONSETBAND()
{
  reset();
}

void ONSETBAND::reset()
{
  for(int i=0;i<NSAMPLES;i++)
    rms[i] = com[i] = low[i] = diff[i] = weight[i] = 0;
}

OnsetDetection::OnsetDetection()
{
  //frequency-range of the bands
  band[0].start =   0;
  band[0].end   =   4;
  band[1].start =   4;
  band[1].end   =   8;
  band[2].start =   8;
  band[2].end   =  16;
  band[3].start =  16;
  band[3].end   =  32;
  band[4].start =  32;
  band[4].end   =  64;
  band[5].start =  64;
  band[5].end   = 128;
  band[6].start = 128;
  band[6].end   = 256;

  for(int i=0;i<NBANDS;i++)
    band[i].nBands = band[i].end - band[i].start;

}

void OnsetDetection::reset()
{
  for(int i=0;i<NBANDS;i++)
  band[i].reset();
}

void OnsetDetection::process(int count,double *freq,double **onset)
{
  int cm1 = count-1; 
  int cm2 = count-2; 

  if(cm1<0) cm1 += NSAMPLES; 
  if(cm2<0) cm2 += NSAMPLES;

  const double a0 = 6.372802E-02,
               a1 = 1.274560E-01,
               a2 = 6.372802E-02,
               b1 = 1.194365E-00,
               b2 =-4.492774E-01;

  const double mu    = 100;
  const double alpha = 0.8;


  for(int i=0;i<NBANDS;i++)
  if(bandSwitch[i])
  {
	double sum = 0;

	for(int k=band[i].start;k<band[i].end;k++)
	  sum += freq[i] * freq[i];

	band[i].rms[count] = sqrt(sum / band[i].nBands);	

	band[i].com[count] = log(1.+mu*band[i].rms[count])/log(1.+mu);     
 
	band[i].low[count] = a0*band[i].com[count]+                        
                         a1*band[i].com[cm1]+
	  	                 a2*band[i].com[cm2]+
		                 b1*band[i].low[cm1]+
		                 b2*band[i].low[cm2];

	band[i].diff[count] = band[i].low[count]-band[i].low[cm1];         

	if(band[i].diff[count]<0) 
	  band[i].diff[count] = 0;                       

	band[i].weight[count] = ((1.0-alpha)*band[i].low[count]) + (alpha*(NRESONANCE/10.)*band[i].diff[count]); 

	onset[i][count] = band[i].weight[count];

  }//end for NBANDS

}