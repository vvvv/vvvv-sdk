using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Diagnostics;
using System.Timers;

namespace VVVV.Nodes.Animation
{
    public class SimpleStateRemindor<T> //where T : IEquatable<T>
    {
        T LastInput;
        bool FirstFrame;

        public SimpleStateRemindor()
        {
            FirstFrame = true;
            LastInput = default(T);
        }

        [Node]
        public bool Changed(T input)
        {
            var changed = (FirstFrame || !LastInput.Equals(input));

            FirstFrame = false;
            LastInput = input;

            return changed;
        }

        [Node]
        public T GetLast(T input)
        {
            var last = LastInput;
            LastInput = input;
            return last;
        }

        [Node]
        public T Hold(T input, bool sample)
        {
            if (sample)
                LastInput = input;
            return LastInput;
        }
    }

    public class RandomGenerator
    {
        private Random FRandom;

        private static Random FSeed = new Random();

        public RandomGenerator()
        {
            FRandom = new Random(FSeed.Next());
        }

        [Node]
        public int Random() //(int max = 10)
        {
            return FRandom.Next(10);//(max);
        }
    }

    public class FrameCounter
    {
        private int FCount;

        [Node]
        public int FrameNr()
        {
            return FCount++;
        }
    }


    public class ToggleState
    {
        bool State;

        [Node]
        public bool Toggle(bool toggle)
        {
            if (toggle)
                State = !State;
            return State;
        }
    }


    public abstract class Clock
    {
        public double Now { get; protected set; }
        public abstract double FromNow(double inSeconds);
    }


    public class FrameClock : Clock
    {
        public static FrameClock GlobalFrameClock = new FrameClock();
        protected Stopwatch Stopwatch;

        public FrameClock()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public double CheckNow()
        {
            Now = Time();
            return Now;
        }

        [Node]
        public double Time()
        {
            return Stopwatch.Elapsed.TotalSeconds;
        }

        public override double FromNow(double inSeconds)
        {
            return Now + inSeconds;
        }
    }



    public interface ICurve<TOutRoom, TSampleRoom>
    {
        TOutRoom Sample(TSampleRoom samplePos);
    }

    public class Curve<TOutRoom, TSampleRoom> : ICurve<TOutRoom, TSampleRoom>
    {
        protected TSampleRoom Ft0, Ft1;
        protected TOutRoom Fp0, Fp1;

        public Curve(TSampleRoom t0, TOutRoom p0, TSampleRoom t1, TOutRoom p1)
        {
            Ft0 = t0;
            Fp0 = p0;
            Ft1 = t1;
            Fp1 = p1;
        }

        public virtual TOutRoom Sample(TSampleRoom samplePos)
        {
            return Fp0;
        }
    }

    public class Filter
    {
        protected ICurve<double, double> FCurve;

        // change nodes needed to check if new curve has to be evaluated
        protected SimpleStateRemindor<double> FFilterTime = new SimpleStateRemindor<double>();
        protected SimpleStateRemindor<double> FGoal = new SimpleStateRemindor<double>();

        [Node]
        public static double Sample(ICurve<double, double> curve, double samplepos)
        {
            return curve.Sample(samplepos);
        }
    }



    public class Linear : Curve<double, double>
    {
        public Linear(double t0, double p0, double t1, double p1)
            : base(t0, p0, t1, p1) { }

        public override double Sample(double samplePos)
        {
            var x = (samplePos - Ft0) / (Ft1 - Ft0);
            x = System.Math.Min(System.Math.Max(x, 0), 1);
            var y = Fp0 + x * (Fp1 - Fp0);
            return y;
        }
    }

    public class LinearFilterState : Filter
    {
        [Node]
        private static ICurve<double, double> LinearFilterCreate(Clock clock, double startingpos = 0, double goal = 1, double filterTime = 1)
        {
            var t0 = clock.Now;
            var t1 = clock.FromNow(filterTime);
            var p0 = startingpos;
            var p1 = goal;
            return new Linear(t0, p0, t1, p1);
        }

        //[Node]
        public double LinearFilter(Clock clock, double goal = 0, double filterTime = 1, bool restart = false)
        {
            var now = clock.Now;
            var currentpos = restart || (FCurve == null) ? goal : Sample(FCurve, now);
            filterTime = System.Math.Max(filterTime, 1E-9);

            if (restart | FGoal.Changed(goal) | FFilterTime.Changed(filterTime))
                FCurve = LinearFilterCreate(clock, currentpos, goal, filterTime);

            return currentpos;
        }

        [Node]
        public double LinearFilter(double goal = 0, double filterTime = 1, bool restart = false)
        {
            return LinearFilter(FrameClock.GlobalFrameClock, goal, filterTime, restart);
        }
    }



    //public class Oscillator : Curve<double, double>
    //{
    //    double Fv0;
    //    double Fv1;

    //    public Oscillator(double t0, double p0, double t1, double p1, double v0 = 0, double v1 = 0)
    //        : base(t0, p0, t1, p1)
    //    {
    //        Fv0 = v0;
    //        Fv1 = v1;
    //    }
    //}


    //public class Oscillator : Filter
    //{
    //    [Node]
    //    private void SetOscillatingFilterCurve(double goalposition = 0, double filtertime = 1, bool restart = false)
    //    {
    //        if (restart | FGoalPosition.Changed(goalposition) | FFilterTime.Changed(filtertime))
    //        {
    //            restart |= FFirstFrame;
    //            var t0 = (double)DateTime.Now.Ticks;
    //            var t1 = t0 + TimeSpan.FromSeconds(filtertime).Ticks;
    //            var p0 = restart ? goalposition : FPos;
    //            var p1 = goalposition;
    //            var v0 = restart ? 0 : FVel;
    //            var v1 = 0;
    //            FCurve = new Oscillator(t0, p0, t1, p1, v0, v1);
    //        }
    //    }
    //    public double OscillatorFilterAndSample(double goalposition = 0, double filtertime = 1,
    //        bool restart = false)
    //    {
    //        SetOscillatingFilterCurve(goalposition, filtertime, restart);
    //        return Sample((double)DateTime.Now.Ticks);
    //    }
    //}

}
