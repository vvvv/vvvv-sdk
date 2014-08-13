#include "OnsetDetection.h"

BAND::BAND()
{
  reset();
}

void BAND::reset()
{
  for(int i=0;i<NFIELD;i++)
  rms[i] = com[i] = low[i] = diff [i] = weight[i] = 0;
}

OnsetDetection::OnsetDetection()
{
  /*
   //the indices on the signal.freq-array for the subbands--------//
   int index[24][2] =   { {  0,  1},{  2,  3},{  4,  5},{  6,  7},
                          {  8, 10},{ 11, 13},{ 14, 16},{ 17, 19},
                          { 20, 22},{ 23, 27},{ 28, 32},{ 33, 38},
                          { 39, 45},{ 46, 52},{ 53, 61},{ 62, 71},
                          { 72, 84},{ 85,101},{102,121},{122,147},
                          {148,177},{178,219},{220,277},{278,512} };
  */

   int index[24][2] =   { {  0,  1},{  2,  3},{  4,  5},{  6,  7},
                          {  8, 10},{ 11, 13},{ 14, 16},{ 17, 19},
                          { 20, 23},{ 24, 27},{ 28, 31},{ 32, 35},
                          { 36, 40},{ 41, 45},{ 46, 50},{ 51, 55},
                          { 56, 61},{ 62, 67},{ 68, 73},{ 74, 79},
                          { 80, 86},{ 92, 98},{104,110},{111,127} };

  for(int i=0;i<NBAND;i++)
  { 
    band[i].start = index[i][0];
    band[i].end   = index[i][1]; 
  } 

  reset();

}

void OnsetDetection::reset()
{
  for(int c=0;c<NCHANNEL;c++)
  for(int i=0;i<NFIELD;i++)
  signal[c][i]=0;

  for(int i=0;i<NBAND;i++)
  band[i].reset();

}

void OnsetDetection::process (double freq[],double onset[][NFIELD],double fx[], int &c, int length) 
{
  int cm1 = c-1; //one element before the actual element
  int cm2 = c-2; //two elements before the actual element

  if(cm1<0) cm1+=NFIELD;  //if c==0 cm1=1023
  if(cm2<0) cm2+=NFIELD;

  //lowpassfilter coefficients----------------------------------------------------------------//
  double a0 = 6.372802E-02,
         a1 = 1.274560E-01,
         a2 = 6.372802E-02,
         b1 = 1.194365E-00,
         b2 =-4.492774E-01;

  double mu    = 100;
  double alpha = 0.8;

  //------------------------------------------------------------------------------------------//

  for(int i=0;i<NBAND;i++)
  {
	  double sum=0;

      for(int k=band[i].start;k<=band[i].end;k++)                    //sum up the energy in the frequency bands 
      sum  += freq[k] * freq[k];                                     //which belong to the subband


	  //sum *= 1.+max;


	  band[i].rms[c] = (sqrt(sum/(band[i].end-band[i].start+1)));    //real-mean-square

	  band[i].com[c] = log(1.+mu*band[i].rms[c])/log(1.+mu);         //compression
 
	  band[i].low[c] = a0*band[i].com[c]+                            //lowpass-filtering with a checbyshev filter
                       a1*band[i].com[cm1]+
	  	               a2*band[i].com[cm2]+
		               b1*band[i].low[cm1]+
		               b2*band[i].low[cm2];

	  band[i].diff[c]=band[i].low[c]-band[i].low[cm1];               //the difference from the previous to the actual frame

	  if(band[i].diff[c]<0) band[i].diff[c]=0;                       //set negative values to zero(half-wave-rectifier)

	  band[i].weight[c] = ((1.0-alpha)*band[i].low[c]) + (alpha*(NRESONANCE/10.)*band[i].diff[c]); //weighting

  }
  
  //-------------------------------------------------------------------------------------------//

  //6 adjecent subbands are summed into one channel
  for(int i=0;i<NCHANNEL;i++)                  
  {
     int inc=i*(NBAND/NCHANNEL);

	 signal[i][c]= (band[inc  ].weight[c]+
                    band[inc+1].weight[c]+
	                band[inc+2].weight[c]+
	                band[inc+3].weight[c]+
	                band[inc+4].weight[c]+
	                band[inc+5].weight[c]);
  }

  //------------------------------------------------------------------------------------------//

  //signal[0][c] = band[ 0].weight[c]*6;
  //signal[3][c] = band[23].weight[c]*6;

  for(int h=0;h<NCHANNEL;h++)
  for(int i=0;i<NFIELD;i++)
  {
   int shift = c-i;

   if(shift<0) shift+=NFIELD;

   onset[h][i] = signal[h][shift];
  }

  for(int h=0;h<NCHANNEL;h++)
  fx[h] = signal[h][c];


  //increment of the count variable: 0,1,..,1023,0,1,...
  ++c %= NFIELD;
 
}