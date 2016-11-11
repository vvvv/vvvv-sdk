using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using WindowsInput;
using WindowsInput.Native;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "SendKeyboard",
                Category = "System",
                Help = "Inserts the key events serially into the keyboard input stream of the system.",
                AutoEvaluate = true)]
    public class SendKeyboardNode : IPluginEvaluate
    {
        private static readonly InputSimulator InputSimulator = new InputSimulator();

        [Input("Keyboard")]
        public ISpread<Keyboard> KeyboardIn;

        [Input("Send Key Codes", DefaultBoolean = true)]
        public ISpread<bool> SendKeyCodesIn;

        [Input("Send Key Chars", DefaultBoolean = true)]
        public ISpread<bool> SendKeyCharsIn;

        Spread<Subscription<Keyboard, KeyNotification>> FSubscriptions = new Spread<Subscription<Keyboard, KeyNotification>>();

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
            FSubscriptions.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            FSubscriptions.ResizeAndDispose(
                spreadMax,
                i =>
                {
                    return new Subscription<Keyboard, KeyNotification>(
                        k => k.KeyNotifications,
                        (k, n) =>
                        {
                            switch (n.Kind)
                            {
                                case KeyNotificationKind.KeyDown:
                                    if (SendKeyCodesIn[i])
                                        InputSimulator.Keyboard.KeyDown((VirtualKeyCode)((KeyDownNotification)n).KeyCode);
                                    break;
                                case KeyNotificationKind.KeyPress:
                                    if (SendKeyCharsIn[i])
                                        InputSimulator.Keyboard.TextEntry(((KeyPressNotification)n).KeyChar);
                                    break;
                                case KeyNotificationKind.KeyUp:
                                    if (SendKeyCodesIn[i])
                                        InputSimulator.Keyboard.KeyUp((VirtualKeyCode)((KeyUpNotification)n).KeyCode);
                                    break;
                                case KeyNotificationKind.DeviceLost:
                                    break;
                                default:
                                    break;
                            }
                        }
                    );
                }
            );
            for (int i = 0; i < spreadMax; i++)
            {
                // Resubsribe if necessary
                FSubscriptions[i].Update(KeyboardIn[i]);
            }
        }
    }
}
