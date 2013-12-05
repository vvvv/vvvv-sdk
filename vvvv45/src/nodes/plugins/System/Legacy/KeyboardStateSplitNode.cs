using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "KeyboardState", Category = "System", Version = "Split Legacy")]
    public class LegacyKeyStateSplitNode : IPluginEvaluate
    {
        [Input("Keyboard")]
        public ISpread<Keyboard> FInput;

        [Output("Key")]
        public ISpread<string> FKeyOut;
        
        [Output("Key Code")]
        public ISpread<ISpread<int>> FKeyCodeOut;
        
        [Output("Caps Lock")]
        public ISpread<bool> FCapsOut;

        [Output("Time")]
        public ISpread<int> FTimeOut;

        Spread<Subscription<Keyboard, KeyCodeNotification>> FSubscriptions = new Spread<Subscription<Keyboard, KeyCodeNotification>>();

        public void Evaluate(int spreadMax)
        {
            FSubscriptions.ResizeAndDispose(
                spreadMax,
                slice =>
                {
                    return new Subscription<Keyboard, KeyCodeNotification>(
                        keyboard => keyboard.KeyNotifications.OfType<KeyCodeNotification>(),
                        (keyboard, notification) =>
                        {
                            var keyCodeOut = FKeyCodeOut[slice];
                            var keyCodeValue = (int)notification.KeyCode;
                            switch (notification.Kind)
                            {
                                case KeyNotificationKind.KeyDown:
                                    if (!keyCodeOut.Contains(keyCodeValue))
                                        keyCodeOut.Add(keyCodeValue);
                                    break;
                                case KeyNotificationKind.KeyUp:
                                    keyCodeOut.Remove(keyCodeValue);
                                    break;
                            }
                            FCapsOut[slice] = keyboard.CapsLock;
                            FTimeOut[slice] = FTimeOut[slice] + 1;
                        }
                    );
                }
            );

            FKeyCodeOut.SliceCount = spreadMax;
            FCapsOut.SliceCount = spreadMax;
            FTimeOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                FSubscriptions[i].Update(FInput[i]);
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
