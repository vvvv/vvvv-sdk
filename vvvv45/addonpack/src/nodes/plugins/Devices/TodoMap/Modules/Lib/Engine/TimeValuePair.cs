using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace VVVV.Lib
{
    public class TimeValuePair
    {
        private double time;
        private double val;

        public double Time
        {
            get { return time; }
            set { time = value; }
        }

        public double Value
        {
            get { return val; }
            set { val = value; }
        }
    }

    public class TimeLine : List<TimeValuePair> { }
}
