using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;

namespace VVVV.Nodes.Animation
{
    public class ChangedState<T> //where T : IEquatable<T>
    {
        T LastInput;
        bool FirstFrame;

        public ChangedState()
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
    }

    public class RandomGenerator
    {
        private readonly Random FRandom = new Random();

        [Node]
        public int Random()
        {
            return FRandom.Next(10);
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
        public double CheckNow()
        {
            Now = Time();
            return Now;
        }

        [Node]
        public static double Time()
        {
            return (double)DateTime.Now.Ticks;
        }

        public override double FromNow(double inSeconds)
        {
            return Now + TimeSpan.FromSeconds(inSeconds).Ticks;
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
        protected double FPos;
        protected double FVel;
        protected bool FFirstFrame;

        // change nodes needed to check if new curve has to be evaluated
        protected ChangedState<double> FFilterTime = new ChangedState<double>();
        protected ChangedState<double> FGoal = new ChangedState<double>();

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

    public class LinearFilterState: Filter
    {
        [Node]
        private ICurve<double, double> LinearFilterCreate(double goal = 0, double filterTime = 1, bool restart = false)
        {
            if (restart | FGoal.Changed(goal) | FFilterTime.Changed(filterTime))
            {
                restart |= FFirstFrame;
                var t0 = FrameClock.Time();
                var t1 = t0 + TimeSpan.FromSeconds(filterTime).Ticks;
                var p0 = restart ? goal : FPos;
                var p1 = goal;
                FCurve = new Linear(t0, p0, t1, p1);
            }
            FFirstFrame = false;            
            return FCurve;
        }

        [Node]
        public double LinearFilter(int goal = 0, int filtertime = 1,
            bool restart = false)
        {
            LinearFilterCreate(goal, filtertime, restart);
            FPos = Sample(FCurve, FrameClock.Time());
            return FPos;
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
