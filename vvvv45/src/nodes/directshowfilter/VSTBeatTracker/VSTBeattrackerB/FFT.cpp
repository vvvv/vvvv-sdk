#include "FFT.h"

FFT::FFT()
{
  length = NSAMPLES;

  in  = new fftw_complex[NSAMPLES];
  out = new fftw_complex[NSAMPLES]; 

  plan = fftw_plan_dft_1d(NSAMPLES,in, out, FFTW_FORWARD, FFTW_ESTIMATE);

}

void FFT::adjust()
{
  fftw_destroy_plan(plan);

  //delete (in);
  //delete (out);

  //in  = new fftw_complex[length];
  //out = new fftw_complex[length]; 

  plan = fftw_plan_dft_1d(NSAMPLES,in, out, FFTW_FORWARD, FFTW_ESTIMATE);
}

FFT::~FFT()
{
  fftw_destroy_plan(plan); //???
}

void FFT::process(double wave[],double freq[],int length)
{
  if(length != this->length)
  {
   this->length = length;
   adjust();
  }

  for(int i=0;i<NSAMPLES;i++)
  {
   if(i < length)
    in [i][0] = wave[i] * (0.54 - 0.46 * cos(2*PI*(double)i / (double)length)); //Blackman-Window???
   else
    in [i][0] = 0;
   
   in [i][1] = 0;  
  }

  fftw_execute(plan);

  for(int i=0;i<NSAMPLES;i++)
  if(i < length)
   freq[i] = (double)(sqrt(out[i][0] * out[i][0] + out[i][1] * out[i][1])) / length;
  else  
   freq[i] = 0;


}
