using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "KeyboardState", Category = "System", Version = "Split")]
    public class KeyStateSplitNode : IPluginEvaluate
    {
        [Input("Keyboard")]
        public ISpread<KeyboardState> FInput;

        [Output("Key")]
        public ISpread<string> FKeyOut;
        
        [Output("Key Code")]
        public ISpread<ISpread<int>> FKeyCodeOut;
        
        [Output("Caps Lock")]
        public ISpread<bool> FCapsOut;

        [Output("Time")]
        public ISpread<int> FTimeOut;

        public void Evaluate(int spreadMax)
        {
            FTimeOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var keyEvent = FInput[i];
                ISpread<int> keyCode;
                int time;
                bool capsLock;

                if (keyEvent != null)
                    KeyboardStateNodes.Split(keyEvent, out keyCode, out time, out capsLock);
                else
                {
                    keyCode = new Spread<int>();
                    time = 0;
                    capsLock = false;
                }        

                FKeyCodeOut[i] = keyCode;
                FCapsOut[i] = capsLock;
                FTimeOut[i] = time;
            }
            
            //KeyOut returns the keycodes symbolic names
            //it is a spread parallel to the keycodes 
            //didn't want to create an extra binsize output, so...
            var keys = new List<string>();
            foreach (var bin in FKeyCodeOut)
            	foreach (var slice in bin)
            {
            	var key = (Keys)slice;
            	keys.Add(key.ToString());
            }
            FKeyOut.AssignFrom(keys);
        }
    }
}
