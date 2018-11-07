using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "MouseState", Category = "System", Version = "Split Legacy")]
    public class LegacyMouseStatetSplitNode : IPluginEvaluate, IDisposable
    {
        [Input("Mouse")]
        public ISpread<Mouse> MouseIn;
        [Output("X")]
        public ISpread<double> XOut;
        [Output("Y")]
        public ISpread<double> YOut;
        [Output("Mouse Wheel")]
        public ISpread<int> MouseWheelOut;
        [Output("Left Button")]
        public ISpread<bool> LeftButtonOut;
        [Output("Middle Button")]
        public ISpread<bool> MiddleButtonOut;
        [Output("Right Button")]
        public ISpread<bool> RightButtonOut;
        [Output("X Button 1")]
        public ISpread<bool> X1ButtonOut;
        [Output("X Button 2")]
        public ISpread<bool> X2ButtonOut;

        Spread<Subscription<Mouse, MouseNotification>> FSubscriptions = new Spread<Subscription<Mouse, MouseNotification>>();
        Spread<int> FRawMouseWheel = new Spread<int>(1);

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
            FSubscriptions.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            XOut.SliceCount = spreadMax;
            YOut.SliceCount = spreadMax;
            MouseWheelOut.SliceCount = spreadMax;
            FRawMouseWheel.SliceCount = spreadMax;
            LeftButtonOut.SliceCount = spreadMax;
            MiddleButtonOut.SliceCount = spreadMax;
            RightButtonOut.SliceCount = spreadMax;
            X1ButtonOut.SliceCount = spreadMax;
            X2ButtonOut.SliceCount = spreadMax;
            
            FSubscriptions.ResizeAndDispose(
                spreadMax,
                slice =>
                {
                    return new Subscription<Mouse, MouseNotification>(
                        mouse => mouse.MouseNotifications,
                        (mouse, n) =>
                        {
                            switch (n.Kind)
                            {
                                case MouseNotificationKind.MouseDown:
                                case MouseNotificationKind.MouseUp:
                                    var mouseButton = n as MouseButtonNotification;
                                    var isDown = n.Kind == MouseNotificationKind.MouseDown;
                                    if ((mouseButton.Buttons & MouseButtons.Left) > 0)
                                        LeftButtonOut[slice] = isDown;
                                    if ((mouseButton.Buttons & MouseButtons.Middle) > 0)
                                        MiddleButtonOut[slice] = isDown;
                                    if ((mouseButton.Buttons & MouseButtons.Right) > 0)
                                        RightButtonOut[slice] = isDown;
                                    if ((mouseButton.Buttons & MouseButtons.XButton1) > 0)
                                        X1ButtonOut[slice] = isDown;
                                    if ((mouseButton.Buttons & MouseButtons.XButton2) > 0)
                                        X2ButtonOut[slice] = isDown;
                                    break;
                                case MouseNotificationKind.MouseMove:
                                    var position = new Vector2D(n.Position.X, n.Position.Y);
                                    var clientArea = new Vector2D(n.ClientArea.Width - 1, n.ClientArea.Height - 1);
                                    var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                                    XOut[slice] = normalizedPosition.x;
                                    YOut[slice] = normalizedPosition.y;
                                    break;
                                case MouseNotificationKind.MouseWheel:
                                    var mouseWheel = n as MouseWheelNotification;
                                    FRawMouseWheel[slice] += mouseWheel.WheelDelta;
                                    MouseWheelOut[slice] = (int)Math.Round((float)FRawMouseWheel[slice] / Const.WHEEL_DELTA);
                                    break;
                                case MouseNotificationKind.DeviceLost:
                                    XOut[slice] = 0;
                                    YOut[slice] = 0;
                                    LeftButtonOut[slice] = false;
                                    MiddleButtonOut[slice] = false;
                                    RightButtonOut[slice] = false;
                                    X1ButtonOut[slice] = false;
                                    X2ButtonOut[slice] = false;
                                    FRawMouseWheel[slice] = 0;
                                    MouseWheelOut[slice] = 0;
                                    break;
                                default:
                                    break;
                            }
                        }
                    );
                }
            );

            for (int i = 0; i < spreadMax; i++)
                FSubscriptions[i].Update(MouseIn[i]);
        }
    }
}
