using System;
using System.Collections.Generic;
using System.Text;

namespace BenTools.Mathematics
{
    internal class VDataEvent : VEvent
    {
        public Vector DataPoint;
        public VDataEvent(Vector DP)
        {
            this.DataPoint = DP;
        }
        public override double Y
        {
            get
            {
                return DataPoint[1];
            }
        }

        public override double X
        {
            get
            {
                return DataPoint[0];
            }
        }

    }
}
