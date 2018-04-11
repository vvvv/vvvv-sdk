#include "FFT.h"

FFT::FFT()
{
  reset();
}

void FFT::reset()
{
  nSamples = 0;

  checkLength(NSAMPLES);
}

FFT::~FFT()
{
  //fftw_destroy_plan(plan);
}

//delete & destroy plan? 
void FFT::checkLength(int length)
{
  if(nSamples == length) return;

  nSamples = length;

  in  = new fftw_complex[nSamples];
  out = new fftw_complex[nSamples]; 

  for(int i=0;i<nSamples;i++)
  {
    in [i][0] = 0;
	in [i][1] = 0;
	out[i][0] = 0;
	out[i][1] = 0;
  }

  //the plan is an object that contains the data that FFTW needs to compute the FFT
  plan = fftw_plan_dft_1d(nSamples, in, out, FFTW_FORWARD, FFTW_ESTIMATE);

}


//takes the waveform in the time-domain as an input and returns the signal in the frequency-domain 
void FFT::process      (double wave[],double freq[],int length)  
{
  checkLength(length);

  for(int i=0;i<nSamples;i++)
  in [i][0] = ((double)wave[i] * (0.54-0.46 * cos(2*PI*(double)i/(double)NSAMPLES)));

  //compute the actual transform
  fftw_execute(plan);

  //transform the complex number arrays into the spectral-density
  for(int i=0;i<nSamples;i++)
  freq[i] = (double)(sqrt(out[i][0]*out[i][0] + out[i][1]*out[i][1]))/NSAMPLES;

}
