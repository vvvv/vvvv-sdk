#include "Time.h"

Time::Time ()
{
   LONGLONG ticksPerSecond;

   QueryPerformanceFrequency((LARGE_INTEGER*)&ticksPerSecond);

   ticksPerMicrosecond = (double) ticksPerSecond /1000000.0;

}

//starts the stopwatch
double Time::start()
{
   QueryPerformanceCounter((LARGE_INTEGER*)&startClock);

   double microsecondsSinceBoot = (double) startClock / ticksPerMicrosecond;
   
   microsSinceBoot = microsecondsSinceBoot;

   return microsSinceBoot;
}

double Time::startIdle()
{
   QueryPerformanceCounter((LARGE_INTEGER*)&startClockIdle);

   microsSinceBootIdle = (double) startClockIdle / ticksPerMicrosecond;

   return microsSinceBootIdle;
}

//stops the stopwatch
double Time::stop()
{
   QueryPerformanceCounter((LARGE_INTEGER*)&stopClock);

   microsSinceStart = ticksToMicros (stopClock - startClock);

   return microsSinceStart;

}

//if the time which is passed since the start of the stopwatch
//is still in the interval-time defined by the parameter
//return true. if the pased time is lnger: return false
bool Time::stopInInterval(double interval)
{
   QueryPerformanceCounter((LARGE_INTEGER*)&stopClock);
   microsSinceStart = ticksToMicros (stopClock - startClock);

   if(microsSinceStart<interval) 
     return true; 

   return false;
}

double Time::sinceStart()
{
   QueryPerformanceCounter((LARGE_INTEGER*)&stopClock);
   microsSinceStart = ticksToMicros (stopClock - startClock);

   return microsSinceStart;
}

double Time::idle()
{
   QueryPerformanceCounter((LARGE_INTEGER*)&stopClockIdle);
   return (ticksToMicros (stopClockIdle - startClockIdle)) / 1000.;

}

//transform system.-ticks into microseconds
double Time::ticksToMicros(LONGLONG ticks)
{
   return ((double) ticks / ticksPerMicrosecond );
}

double Time::displayTime()
{
   LONGLONG ticksSinceBoot;

   QueryPerformanceCounter((LARGE_INTEGER*)&ticksSinceBoot);

   double microsecondsSinceBoot = (double) ticksSinceBoot / ticksPerMicrosecond;
   double seconds =  microsecondsSinceBoot / 1000000.0 ;
   
   h   = (long)seconds / 3600;
   m   = ((long)seconds - (h*3600)) / 60;
   s   = ((long)seconds - (h*3600) - (m*60)); 
   microsSinceBoot = microsecondsSinceBoot;

   //hours minutes seconds since boot-time
   //printf("H:%3u M:%3u S:%3u MicrosSinceBoot: %12.0f\n",h,m,s,microsSinceBoot);

   return microsSinceBoot;
   
}

//Micros since boot
double Time::micros()
{
   LONGLONG ticksSinceBoot;

   QueryPerformanceCounter((LARGE_INTEGER*)&ticksSinceBoot);

   double microsecondsSinceBoot = (double) ticksSinceBoot / ticksPerMicrosecond;
   
   microsSinceBoot = microsecondsSinceBoot;

   return microsSinceBoot;
}