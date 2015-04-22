using System;

namespace VVVV.Utils.Animation
{
    /// <summary>
    /// One pole IIR filter
    /// </summary>
	public class LowpassFilter
    {
        /// <summary>
        /// Initialize the filter
        /// </summary>
        public LowpassFilter()
        {
            FFirstTime = true;
        }

        protected bool FFirstTime;
        protected double FLastFilterValue;

        /// <summary>
        /// Last filter value
        /// </summary>
        public double Last
        {
            get
            {
                return FLastFilterValue;
            }
        }

        /// <summary>
        /// Gets the next filter value, applies <c>alpha * value + (1 - alpha) * lastValue</c>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public double Filter(double value, double alpha = 1)
        {
            double filterValue = 0;
            if (FFirstTime)
            {
                FFirstTime = false;
                filterValue = value;
            }
            else
            {
                filterValue = alpha * value + (1 - alpha) * FLastFilterValue;
            }

            FLastFilterValue = filterValue;

            return filterValue;
        }
    }
}


