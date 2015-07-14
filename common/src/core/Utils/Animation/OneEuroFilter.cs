#region usings
using System;
#endregion

namespace VVVV.Utils.Animation
{

	/// <summary>
	/// Adaptive noise filter via http://www.lifl.fr/~casiez/1euro/OneEuroFilter.cs
	/// See http://www.lifl.fr/~casiez/1euro
	/// </summary>
	public class OneEuroFilter
    {
        public OneEuroFilter(double minCutoff, double beta)
        {
            FFirstTime = true;
            this.FMinCutoff = minCutoff;
            this.FBeta = beta;

            FxFilter = new LowpassFilter();
            FdxFilter = new LowpassFilter();
            FdxCutoff = 1;
        }

        protected bool FFirstTime;
        protected double FMinCutoff;
        protected double FBeta;
        protected LowpassFilter FxFilter;
        protected LowpassFilter FdxFilter;
        protected double FdxCutoff;

        public double MinCutoff
        {
            get { return FMinCutoff; }
            set { FMinCutoff = value; }
        }

        public double Beta
        {
            get { return FBeta; }
            set { FBeta = value; }
        }
    	
    	public double CutoffDerivative
        {
            get { return FdxCutoff; }
            set { FdxCutoff = value; }
        }

        public double Filter(double x, double rate)
        {
            double dx;
            if (FFirstTime)
            {
                FFirstTime = false;
                dx = 0;
            }
            else
            {
                dx = (x - FxFilter.Last) * rate;
            }

            var edx = FdxFilter.Filter(dx, Alpha(rate, FdxCutoff));
            var cutoff = FMinCutoff + FBeta * Math.Abs(edx);

            return FxFilter.Filter(x, Alpha(rate, cutoff));
        }

        const double TwoPi = 2*Math.PI;
        protected double Alpha(double rate, double cutoff)
        {
            var twoPiCutoff = TwoPi * cutoff;
            return twoPiCutoff / (twoPiCutoff + rate);
        }
    }
}


