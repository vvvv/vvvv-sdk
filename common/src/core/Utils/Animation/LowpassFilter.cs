using System;

namespace VVVV.Utils.Animation
{
    /// <summary>
    /// One pole IIR filter
    /// </summary>
	public class LowpassFilter
    {
        public LowpassFilter()
        {
            FFirstTime = true;
        }

        protected bool FFirstTime;
        protected double FLastFilterValue;

        public double Last
        {
            get
            {
                return FLastFilterValue;
            }
        }

        public double Filter(double x, double alpha)
        {
            double filterValue = 0;
            if (FFirstTime)
            {
                FFirstTime = false;
                filterValue = x;
            }
            else
            {
                filterValue = alpha * x + (1 - alpha) * FLastFilterValue;
            }

            FLastFilterValue = filterValue;

            return filterValue;
        }
    }
}


