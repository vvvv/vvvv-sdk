using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace VVVV.Utils.IO
{
    public class TouchDevice
    {
        public static readonly TouchDevice Empty = new TouchDevice(Observable.Never<TouchNotification>());

        public readonly IObservable<TouchNotification> Notifications;

        public TouchDevice(IObservable<TouchNotification> notifications)
        {
            Notifications = notifications
                .Publish()
                .RefCount();
        }
    }
}
