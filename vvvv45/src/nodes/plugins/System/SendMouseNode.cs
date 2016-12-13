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
    [PluginInfo(Name = "SendMouse",
                Category = "System",
                Help = "Inserts the mouse events serially into the mouse input stream of the system.",
                AutoEvaluate = true)]
    public class SendMouseNode : IPluginEvaluate, IDisposable
    {
        private static readonly InputSimulator InputSimulator = new InputSimulator();

        [Input("Mouse")]
        public ISpread<Mouse> MouseIn;

        Spread<Subscription<Mouse, MouseNotification>> FSubscriptions = new Spread<Subscription<Mouse, MouseNotification>>();

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
                    return new Subscription<Mouse, MouseNotification>(
                        k => k.MouseNotifications,
                        (k, n) =>
                        {
                            switch (n.Kind)
                            {
                                case MouseNotificationKind.MouseDown:
                                    var downNotification = n as MouseDownNotification;
                                    switch (downNotification.Buttons)
	                                {
                                        case System.Windows.Forms.MouseButtons.Left:
                                            InputSimulator.Mouse.LeftButtonDown();
                                            break;
                                        case System.Windows.Forms.MouseButtons.Middle:
                                            // Function missing
                                            break;
                                        case System.Windows.Forms.MouseButtons.Right:
                                            InputSimulator.Mouse.RightButtonDown();
                                            break;
                                        case System.Windows.Forms.MouseButtons.XButton1:
                                            InputSimulator.Mouse.XButtonDown(1);
                                            break;
                                        case System.Windows.Forms.MouseButtons.XButton2:
                                            InputSimulator.Mouse.XButtonDown(2);
                                            break;
                                        default:
                                            break;
	                                }
                                    break;
                                case MouseNotificationKind.MouseUp:
                                    var upNotification = n as MouseUpNotification;
                                    switch (upNotification.Buttons)
	                                {
                                        case System.Windows.Forms.MouseButtons.Left:
                                            InputSimulator.Mouse.LeftButtonUp();
                                            break;
                                        case System.Windows.Forms.MouseButtons.Middle:
                                            // Function missing
                                            break;
                                        case System.Windows.Forms.MouseButtons.Right:
                                            InputSimulator.Mouse.RightButtonUp();
                                            break;
                                        case System.Windows.Forms.MouseButtons.XButton1:
                                            InputSimulator.Mouse.XButtonUp(1);
                                            break;
                                        case System.Windows.Forms.MouseButtons.XButton2:
                                            InputSimulator.Mouse.XButtonUp(2);
                                            break;
                                        default:
                                            break;
	                                }
                                    break;
                                case MouseNotificationKind.MouseMove:
                                    double x = ((double)n.Position.X / n.ClientArea.Width) * ushort.MaxValue;
                                    double y = ((double)n.Position.Y / n.ClientArea.Height) * ushort.MaxValue;
                                    InputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(x, y);
                                    break;
                                case MouseNotificationKind.MouseWheel:
                                    var wheelNotification = n as MouseWheelNotification;
                                    InputSimulator.Mouse.VerticalScroll(wheelNotification.WheelDelta);
                                    break;
                                case MouseNotificationKind.MouseHorizontalWheel:
                                    var hwheelNotification = n as MouseHorizontalWheelNotification;
                                    InputSimulator.Mouse.HorizontalScroll(hwheelNotification.WheelDelta);
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
                FSubscriptions[i].Update(MouseIn[i]);
            }
        }
    }
}
