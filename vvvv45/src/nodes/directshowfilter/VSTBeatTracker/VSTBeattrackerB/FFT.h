#ifndef FFT_H
#define FFT_H

#include "Global.h"
#include "fftw3.h"

class FFT
{
  private : fftw_plan plan;

			fftw_complex *in;
			fftw_complex *out;

  public  : FFT();
           ~FFT();

		    void process (double wave[],double freq[],int length);
            void adjust  ();


			int length;
			
};

#endif