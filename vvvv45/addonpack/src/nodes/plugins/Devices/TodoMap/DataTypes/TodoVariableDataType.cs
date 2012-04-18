using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.TodoMap.DataTypes
{
    public class TodoVariableDataType
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public double Default { get; set; }
        public bool AllowFeedBack { get; set; }

        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public bool Reverse { get; set; }
        public eTweenEaseMode EaseMode { get; set; }
        public eTweenMode TweenMode { get; set; }
   
    }
}
