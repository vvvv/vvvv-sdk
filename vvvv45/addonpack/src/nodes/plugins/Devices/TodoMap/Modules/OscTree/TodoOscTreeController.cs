using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.TodoMap.Lib;
using VVVV.TodoMap.Lib.Modules.Osc;
using VVVV.Utils.OSC;

namespace VVVV.TodoMap.Modules.OscTree
{
    public class TodoOscTreeController
    {
        private TodoEngine engine;

        public TodoOscTreeController(TodoEngine engine)
        {
            this.engine = engine;
            engine.Osc.OscDataReceived += new OscReceivedDelegate(Osc_OscDataReceived);
        }

        private void Osc_OscDataReceived(OSCMessage msg)
        {
            this.ProcessSetMessage(msg);
        }

        private void ProcessSetMessage(OSCMessage msg)
        {
            string[] tester = msg.Address.Split('/');

            if (tester.Length == 5 && msg.Values.Count > 0)
            {

                if (tester[1] == "todomap")
                {
                    if (tester[2] == "variable")
                    {
                        TodoVariable var = this.engine.GetVariableByName(tester[3]);

                        if (var != null)
                        {
                            if (tester[4] == "set")
                            {
                                if (msg.Values[0] is float)
                                {
                                    var.SetValue(null, (float)msg.Values[0]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
