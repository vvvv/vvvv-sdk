#include <stdio.h>
#include <math.h>
#include <windows.h>

#ifndef GLOBAL_H
#define GLOBAL_H

#define SAMPLERATE       44100
#define FPS        43.06640625
#define FPSINT              43   
#define NSAMPLES          1024
#define NBANDS               7
#define NRESONANCE         256
#define NINTERVAL            2
#define DELAY               23
#define AREA                 3
#define MININTERVAL         16 
#define MAXINTERVAL         60 
#define PI        3.1415926535
#define MINIMUMONSET       0.1
#define STRLENGTH          512
#define WEIGHTFACTOR      1.25
#define NOISE               15
#define NOISECYCLE          64
#define SECPERFRAME 0.02321995
#define MINBPM              44
#define MAXBPM             144

#define VENDORVERSION        1
#define NUMPROGRAMS          1
#define NUMPARAMS           14

#define PARAM_BEAT           0
#define PARAM_PHASE          1
#define PARAM_BPM            2
#define PARAM_PROBABILITY    3
#define PARAM_SILENCE        4
#define PARAM_SIGNAL         5
#define PARAM_DELAY          6
#define PARAM_ADJUST         7
#define PARAM_TARGETBPM      8
#define PARAM_RESET          9
#define PARAM_BAND0         10
#define PARAM_BAND1         11
#define PARAM_BAND2         12
#define PARAM_BAND3         13

const bool bandSwitch[] = { true,
                            true,
   					        true,
						    true,
						    true,
						    true,
						    true };

#endif