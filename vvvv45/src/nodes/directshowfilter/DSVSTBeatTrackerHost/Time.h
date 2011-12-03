/*-----------------------------------------------------------------------------------//

   the class Time has acces to the systems ticks 
   and transform them into microseconds

/------------------------------------------------------------------------------------*/


#ifndef TIME_H
#define TIME_H

#include <windows.h>
#include <stdio.h>

/**********************************************************************/

class Time
{
  public :  Time();
		    double displayTime ();
			double start ();
			double startIdle ();
			double idle();
			double stop  ();
			bool   stopInInterval (double interval); 
			double micros();
			double sinceStart();

  private : double ticksPerMicrosecond;
			long   h;
			long   m;
			long   s;
			double microsSinceBoot;
			double microsSinceStart;
			
			LONGLONG startClock;
			LONGLONG stopClock;

			LONGLONG startClockIdle;
			LONGLONG stopClockIdle;

			double microsSinceBootIdle;
            double microsSinceStartIdle;

			double ticksToMicros(LONGLONG ticks);

};

#endif