using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "Mouse", Category = "System", Version = "Global New")]
    public class GlobalMouseNode : IPluginEvaluate
    {
        [Output("Mouse State")]
        ISpread<MouseState> FMouseOut;

        public void Evaluate(int SpreadMax)
        {
            FMouseOut[0] = MouseState.Current;
        }
    }
}
