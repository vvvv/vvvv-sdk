using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Utils.IO
{
    public class Mouse
    {
        public static readonly Mouse Empty = new Mouse(Observable.Never<MouseNotification>());

        public readonly IObservable<MouseNotification> MouseNotifications;
        private MouseButtons FPressedButtons;

        public Mouse(IObservable<MouseNotification> mouseNotifications)
        {
            mouseNotifications = mouseNotifications
                .Do(n =>
                    {
                        switch (n.Kind)
                        {
                            case MouseNotificationKind.MouseDown:
                                var mouseDown = n as MouseDownNotification;
                                FPressedButtons |= mouseDown.Buttons;
                                break;
                            case MouseNotificationKind.MouseUp:
                                var mouseUp = n as MouseUpNotification;
                                FPressedButtons &= ~mouseUp.Buttons;
                                break;
                            default:
                                break;
                        }
                    });
            MouseNotifications = mouseNotifications.Publish().RefCount();
        }

        /// <summary>
        /// The currently pressed mouse buttons.
        /// </summary>
        public MouseButtons PressedButtons
        {
            get { return FPressedButtons; }
        }
    }
}
