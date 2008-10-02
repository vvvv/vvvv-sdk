#include "FFT.h"

FFT::FFT()
{
  in  = new fftw_complex[NSAMPLES];
  out = new fftw_complex[NSAMPLES]; 

  plan = fftw_plan_dft_1d(NSAMPLES,in, out, FFTW_FORWARD, FFTW_ESTIMATE);

}

FFT::~FFT()
{
  fftw_destroy_plan(plan); //???
}

void FFT::process(double wave[],double freq[])
{
  for(int i=0;i<NSAMPLES;i++)
  {
   in [i][0] = wave[i] * (0.54 - 0.46 * cos(2*PI*(double)i / (double)NSAMPLES)); //Blackman-Window???
   in [i][1] = 0;  
  }

  fftw_execute(plan);

  for(int i=0;i<NSAMPLES;i++)
   freq[i] = (double)(sqrt(out[i][0] * out[i][0] + out[i][1] * out[i][1])) / NSAMPLES;

}
