using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;

namespace VVVV.Nodes.Animation
{
    public class ChangeDetector<T> //where T : IEquatable<T>
    {
        T LastInput;
        bool FirstFrame;

        public ChangeDetector()
        {
            FirstFrame = true;
            LastInput = default(T);
        }

        [Node]
        public bool IsChanged(T input)
        {
            var changed = (FirstFrame || !LastInput.Equals(input));

            FirstFrame = false;
            LastInput = input;

            return changed;
        }
    }




    public class Toggle
    {
        bool State;

        [Node]
        public bool DoToggle(bool toggle)
        {
            if (toggle)
                State = !State;
            return State;
        }
    }





    public enum FilterType { Linear, Oscillator };

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


    public class Linear : Curve<double, double>
    {
        public Linear(double t0, double p0, double t1, double p1)
            : base(t0, p0, t1, p1) { }

        public override double Sample(double samplePos)
        {
            var x = (samplePos - Ft0) / (Ft1 - Ft0);
            x = Math.Min(Math.Max(x, 0), 1);
            var y = Fp0 + x * (Fp1 - Fp0);
            return y;
        }
    }

    public class Oscillator : Curve<double, double>
    {
        double Fv0;
        double Fv1;

        public Oscillator(double t0, double p0, double t1, double p1, double v0 = 0, double v1 = 0)
            : base(t0, p0, t1, p1)
        {
            Fv0 = v0;
            Fv1 = v1;
        }
    }



    public interface IClock
    {
        double Now { get; }
        double FromNow(double inSeconds);
    }

    public class FrameClock : IClock
    {
        public double Now { get; private set; }

        public void CheckNow()
        {
            Now = (double)DateTime.Now.Ticks;
        }

        public double Time()
        {
            CheckNow();
            return Now;
        }

        public double FromNow(double inSeconds)
        {
            return Now + TimeSpan.FromSeconds(inSeconds).Ticks;
        }
    }

    public class FilterNode
    {
        ICurve<double, double> FCurve;
        double FPos;
        double FVel;
        bool FFirstFrame;

        // change nodes needed to check if new curve has to be evaluated
        ChangeDetector<double> FFilterTime = new ChangeDetector<double>();
        ChangeDetector<double> FGoalPosition = new ChangeDetector<double>();
        ChangeDetector<FilterType> FFilterType = new ChangeDetector<FilterType>();

        [Node]
        private void SetLinearFilterCurve(double goalposition = 0, double filtertime = 1, bool restart = false)
        {
            if (restart | FGoalPosition.IsChanged(goalposition) | FFilterTime.IsChanged(filtertime))
            {
                restart |= FFirstFrame;
                var t0 = (double)DateTime.Now.Ticks;
                var t1 = t0 + TimeSpan.FromSeconds(filtertime).Ticks;
                var p0 = restart ? goalposition : FPos;
                var p1 = goalposition;
                FCurve = new Linear(t0, p0, t1, p1);
            }
        }

        [Node]
        private void SetOscillatingFilterCurve(double goalposition = 0, double filtertime = 1, bool restart = false)
        {
            if (restart | FGoalPosition.IsChanged(goalposition) | FFilterTime.IsChanged(filtertime))
            {
                restart |= FFirstFrame;
                var t0 = (double)DateTime.Now.Ticks;
                var t1 = t0 + TimeSpan.FromSeconds(filtertime).Ticks;
                var p0 = restart ? goalposition : FPos;
                var p1 = goalposition;
                var v0 = restart ? 0 : FVel;
                var v1 = 0;
                FCurve = new Oscillator(t0, p0, t1, p1, v0, v1);
            }
        }

        public void SetFilterCurve(double goalposition = 0, double filtertime = 1, FilterType filtertype = FilterType.Linear,
            bool restart = false)
        {
            restart |= FFilterType.IsChanged(filtertype);

            switch (filtertype)
            {
                case FilterType.Linear:
                    SetLinearFilterCurve(goalposition, filtertime, restart);
                    break;
                case FilterType.Oscillator:
                    SetOscillatingFilterCurve(goalposition, filtertime, restart);
                    break;
                default:
                    break;
            }
        }

        [Node]
        public double Sample(double samplepos)
        {
            FPos = FCurve.Sample(samplepos);
            FFirstFrame = false;
            return FPos;
        }

        public double LinearFilterAndSample(double goalposition = 0, double filtertime = 1,
            bool restart = false)
        {
            SetLinearFilterCurve(goalposition, filtertime, restart);
            return Sample((double)DateTime.Now.Ticks);
        }

        public double OscillatorFilterAndSample(double goalposition = 0, double filtertime = 1,
            bool restart = false)
        {
            SetOscillatingFilterCurve(goalposition, filtertime, restart);
            return Sample((double)DateTime.Now.Ticks);
        }

        public double FilterAndSample(double goalposition = 0, double filtertime = 1, FilterType filtertype = FilterType.Linear,
            bool restart = false)
        {
            SetFilterCurve(goalposition, filtertime, filtertype, restart);
            return Sample((double)DateTime.Now.Ticks);
        }
    }
}
