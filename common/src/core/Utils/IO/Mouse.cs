using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace VVVV.Utils.IO
{
    public class Mouse
    {
        public static readonly Mouse Empty = new Mouse(Observable.Never<MouseNotification>());

        public readonly IObservable<MouseNotification> MouseNotifications;

        public Mouse(IObservable<MouseNotification> mouseNotifications)
        {
            MouseNotifications = mouseNotifications.Publish().RefCount();
        }
    }
}
