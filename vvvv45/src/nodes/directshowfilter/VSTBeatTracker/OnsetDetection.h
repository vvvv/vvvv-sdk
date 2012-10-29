/*-----------------------------------------------------------------------------------//
  
  the OnsetDetection-Class takes the frequency data of one frame of audio and divides 
  it into 24 subbands. then it updates the data in the 24 band-objects. per frame
  one element in the different arrays is updated. which element is updated is 
  controlled by a counter-variable which goes from 0 to 1023 and then starts with 0

/------------------------------------------------------------------------------------*/

#ifndef ONSETDETECTION_H
#define ONSETDETECTION_H

#include <stdio.h>
#include <math.h>
#include <windows.h>
#include "Define.h"

//structure for the subband data----------------------------------//
//one element in each array belongs to the information about 
//one frame
typedef struct BAND
{
  int start,end;

  double rms    [NFIELD];  //real-mean-square
  double com    [NFIELD];  //compression (µ-law)
  double low    [NFIELD];  //lowpass filtered signal
  double diff   [NFIELD];  //the difference to the previous frame
  double weight [NFIELD];  //weighted signal

  BAND ();
  void reset ();

}_BAND;

class OnsetDetection
{
 private : BAND band[NBAND];
		   double signal[NCHANNEL][NFIELD];

 public  : OnsetDetection    (); 
	       void process      (double freq[],double onset[][NFIELD]);
		   void process      (double freq[],double onset[][NFIELD],double fx[], int &c,int length);
		   void reset        ();
		   
};

#endif 

