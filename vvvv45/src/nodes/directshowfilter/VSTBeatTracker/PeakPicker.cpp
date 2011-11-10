#include "PeakPicker.h"


PEAKPICKER::PEAKPICKER()
{
  for(int i=0;i<NFIELD;i++)
  {
    signal [i] = 0;
    peak   [i] = 0;
  }
}

//takes signal, detects the peaks and puts them into peak
void PEAKPICKER::process(double _signal[],double _peak[])
{
  for(int i=0;i<NFIELD;i++)
  {
    signal[i] = _signal[i];
    peak  [i] =  0;
  }

  //recursiv function
  clustering(0,NFIELD);

  for(int i=0;i<NFIELD;i++)
  _peak[i] = peak[i];
}

//takes signal, detects the peaks and puts them into peak
//works with variant array-length
void PEAKPICKER::process(double _signal[],double _peak[],int length)
{
  bool zero = true;

  for(int i=0;i<length;i++)
  {
    signal[i] = _signal[i];
    peak  [i] =  0;

	if(_signal[i] > 0) zero = false;
  }

  if(zero) return;

  //recursiv function
  clustering(0,length);

  for(int i=0;i<length;i++)
  _peak[i] = peak[i];
}

//the maximum value in the array between 
//the indices defined by start and end
void PEAKPICKER::getPeak(int start,int end)
{
  int    index=0;
  double value=0;

  for(int i=start;i<end;i++)
  if(signal[i]>value)
  {
    value = signal[i];
	index = i;
  }

  if(start==end) index=start;

  peak[index]=value;

}

//recursiv function
void PEAKPICKER::clustering(int start,int end)
{
  //get the biggest peak
  getPeak(start,end);

  double threshold=0;
  
  //the average of the elements 
  //in the interval defined by start and end
  for(int i=start;i<end;i++)
  threshold+=signal[i];

  threshold/=(double)(end-start);

  //set the elements to zero if they are smaller than the average
  for(int i=start;i<end;i++)
  if(signal[i]<threshold) 
  signal[i]=0;

  //for all elements in the interval
  for(int i=start;i<end;i++)
  {
    if(signal[i]>0)
	{
		//do the same for a new interval
    	for(int k=i;k<end;k++)
		if(signal[k]<=0)
		{
		  clustering(i,k);
		  i=k;
		  break;
		}
	}

  }//end for i

}
