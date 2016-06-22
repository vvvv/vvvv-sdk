#include "Counter.h"

/********************************************************************************/

COUNTER::COUNTER()
{
  length = FIELDLENGTH;

  value  = 0;
  
  minus1 = length-1;
  minus2 = length-2;
  minus3 = length-3;

  plus1  = 1;
  plus2  = 2;
  plus3  = 3;
}

void COUNTER::init(int _length)
{
  length = _length;

  value  = 0;
  
  minus1 = length-1;
  minus2 = length-2;
  minus3 = length-3;

  plus1  = 1;
  plus2  = 2;
  plus3  = 3;
}

/*
COUNTER::COUNTER(int _value)
{
  value = _value;

}
*/

/********************************************************************************/

void COUNTER::inc()
{
  value++;

  if(value==length) value = 0;

  set();
}

/********************************************************************************/

void COUNTER::inc(int a)
{
  value+=a;

  if(value>=length) value-=length;

  set();

}

int COUNTER::shiftL(int x)
{
  int shift = value-x;
  
  if(shift<0) shift=length+shift;

  return shift;
}

int COUNTER::shiftR(int x)
{
  int shift = value+x;

  if(shift>=length) shift=x-length;

  printf("LENGTH %d VALUE %d X %d SHIFT %d\n",length,value,x,shift);

  return shift;
}

void COUNTER::shiftL(double in[],double out[])
{
  for(int i=0;i<length;i++)
  out[i]=in[shiftL(i)];
}

/********************************************************************************/

void COUNTER::set()
{
   minus3 = value-3;
   minus2 = value-2;
   minus1 = value-1;
				
   plus1  = value+1;
   plus2  = value+2;
   plus3  = value+3;

   /*
   switch(value)
   {
	  case 0 :  minus3 = length-3;
		        minus2 = length-2;
				minus1 = length-1;
				break;

	  case 1 :  minus3 = length-2;
		        minus2 = length-1;
		        break;

	  case 2 :  minus3 = length-1;
		        break;

	  case length-3 : plus3 = 0; 
		              break;

	  case length-2 : plus3 = 1;
		              plus2 = 0; 
					  break;

	  case length-1 : plus3 = 2;
		              plus2 = 1;
					  plus1 = 0;
					  break;
   }
   */

   if(value==0)
   {
     minus3 = length-3;
	 minus2 = length-2;
	 minus1 = length-1;
   }
   else
   if(value==1)
   {
     minus3 = length-2;
	 minus2 = length-1;
   }
   else
   if(value==2)
   {
     minus3 = length-1;
   }
   else
   if(length-3)
   {
     plus3 = 0;
   }
   else
   if(length-2)
   {
     plus3 = 1;
     plus2 = 0;   
   }
   else
   if(length-1)
   {
     plus3 = 2;
	 plus2 = 1;
	 plus1 = 0;
   }

}

/********************************************************************************/
