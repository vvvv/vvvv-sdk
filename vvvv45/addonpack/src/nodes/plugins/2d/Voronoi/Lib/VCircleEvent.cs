using System;
using System.Collections.Generic;
using System.Text;

namespace BenTools.Mathematics
{
    internal class VCircleEvent : VEvent
    {
        public VDataNode NodeN, NodeL, NodeR;
        public Vector Center;
        public override double Y
        {
            get
            {
                return Math.Round(Center[1] + MathTools.Dist(NodeN.DataPoint[0], NodeN.DataPoint[1], Center[0], Center[1]), 10);
            }
        }

        public override double X
        {
            get
            {
                return Center[0];
            }
        }

        public bool Valid = true;
    }
}
