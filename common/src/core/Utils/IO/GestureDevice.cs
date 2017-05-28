using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace VVVV.Utils.IO
{
    public class GestureDevice
    {
        public static readonly GestureDevice Empty = new GestureDevice(Observable.Never<GestureNotification>());

        public readonly IObservable<GestureNotification> Notifications;

        public GestureDevice(IObservable<GestureNotification> notifications)
        {
            Notifications = notifications
                .Publish()
                .RefCount();
        }
    }
}
