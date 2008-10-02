#include "Tracker.h"

Tracker::Tracker(AudioEffect* effect)
{
  ctrl   = new Ctrl();
  gui    = new GUI(effect,ctrl);

  signal.init(ctrl,interval.chain);
}

void Tracker::reset()
{
  onset.reset     ();
  resonance.reset ();
  beat.reset      ();

  signal.reset    ();
  ctrl->reset     ();
}

void Tracker::process(double in[NSAMPLES])
{
  signal.in = in;

    fft.process(signal.in,signal.freq);

	onset.process(signal.count,signal.freq,signal.onset);

	resonance.process(signal.count,signal.onset,signal.resonance);

	if(signal.count % (int)FPS == 0)
	interval.process(signal.resonance,signal.interval,signal.targetInterval);

	beat.process(signal);

  signal.process();

  gui->update();

}

