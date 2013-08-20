/*-----------------------------------------------------------------------------------//

  FFT takes the time-domain data in the 1024 elements long wavedata
  and outputs the frequency-transformed data. therefore it uses the 
  FFTW-bibliotheque:  www.fftw.org

/------------------------------------------------------------------------------------*/


#ifndef FFT_H
#define FFT_H

#include <stdio.h>
#include <math.h>
#include <windows.h>

#include "Define.h"
#include "fftw3.h"

class FFT 
{

 private :  fftw_plan plan;           

			fftw_complex *in;
			fftw_complex *out;

			int nSamples;

 		    //fftw_complex in  [NSAMPLES];
			//fftw_complex out [NSAMPLES];

  public :  FFT(); 
	       ~FFT(); 
	        
		    void reset       ();
		    void process     (double wave[],double frequency[],int length);
			void checkLength (int length);

};


#endif