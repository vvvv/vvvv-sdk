#include "Tracker.h"

Tracker::Tracker()
{
  reset();
}

Tracker::~Tracker()
{
  delete signal.freq;
}

void Tracker::checkLength(int length)
{
  if(length == nSamples) return;

  nSamples = length;

  double framesPerSecond = 44100.0 / (double) nSamples;

  double maxBeatsPerSecond = (double) signal.maxBPM / 60.0;

  double minBeatsPerSecond = (double) signal.minBPM / 60.0;

  signal.minInterval = framesPerSecond / maxBeatsPerSecond; //the smallest interval 

  signal.maxInterval = framesPerSecond / minBeatsPerSecond;

}

void Tracker::setBPM()
{
  double framesPerSecond = 44100.0 / (double) nSamples;

  double maxBeatsPerSecond = (double) signal.maxBPM / 60.0;

  double minBeatsPerSecond = (double) signal.minBPM / 60.0;

  signal.minInterval = framesPerSecond / maxBeatsPerSecond; //the smallest interval 

  signal.maxInterval = framesPerSecond / minBeatsPerSecond;

}

void Tracker::reset()
{
  optimize = false;

  nSamples = 0;
  counter  = 0;
  bpm      = 0;

  checkLength(NSAMPLES);

  signal.reset         ();
  fft.reset            ();
  onsetDetection.reset ();
  resonator.reset      ();
  interval.reset       ();
  beatDetection.reset  ();
  
}

void Tracker::setPerformance(int performance)
{
  reset();

  if(performance == 0) optimize = true;
  if(performance == 1) optimize = false;
  if(performance == 2) optimize = true;
  if(performance == 3) optimize = false;

}

void Tracker::process (double in[], int length, float samplerate)  
{ 
  signal.in = in;

  checkLength(length);
    
  fft.process (signal.in, signal.freq, length);

  ////!the size of the bands should change dynamically!---------------//
  //resonator Intervalmax & min should change depending on the resolution
  onsetDetection.process (signal.freq, signal.onset,signal.fx, signal.c,length);

  resonator.process  (signal.onset, signal.resonance,optimize);
 
  if(counter==0) 
  {
    interval.process (signal.resonance, signal.interval,signal.minInterval,signal.maxInterval,optimize);

    bpm = 60.0  / ( ((double)length / samplerate) * signal.interval[beatDetection.channel] );

 
 //   char buffer[128];

 //   sprintf(buffer,"BPM %d Length %d SAMPLERATE %d INTERVAL %d\n", bpm, length, samplerate, signal.interval[beatDetection.channel] );

	//sprintf(buffer,"BPM %d\n", bpm);

 //   OutputDebugString(buffer);

  }

  beatDetection.process  (signal.onset,signal.interval,signal.resonance,signal.beep,signal.fx,optimize);

  ++counter %= INTERVALCYCLE;

}

