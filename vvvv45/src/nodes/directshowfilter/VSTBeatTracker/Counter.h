#ifndef COUNTER_H
#define COUNTER_H

#include <windows.h>
#include <stdio.h>
#include <math.h>

#include "Define.h"

typedef struct COUNTER
{
  int length;

  int minus1;
  int minus2;
  int minus3;

  int plus1;
  int plus2;
  int plus3;

  int value;

  COUNTER     ();
  void init   (int _length);
  void inc    ();
  void inc    (int a);
  int  shiftR (int x);
  int  shiftL (int x);
  void shiftL (double in[],double out[]);

  void set    ();
  
}_COUNTER;

#endif COUNTER_H
