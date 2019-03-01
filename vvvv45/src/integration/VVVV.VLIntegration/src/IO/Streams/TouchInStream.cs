using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using VL.Lib.IO;
using VL.Lib.IO.Notifications;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VLTouch = VL.Lib.IO.ITouchDevice;
using VVVVTouch = VVVV.Utils.IO.TouchDevice;

namespace VVVV.VL.Hosting.IO.Streams
{
    class TouchInStream : MemoryIOStream<VLTouch>, IDisposable
    {
        readonly IIOContainer<IInStream<VVVVTouch>> FContainer;

        Dictionary<VVVVTouch, VLTouch> map = new Dictionary<VVVVTouch, VLTouch>();

        public TouchInStream(IIOFactory factory, InputAttribute attribute)
        {
            attribute.StringType = StringType.Filename;
            FContainer = factory.CreateIOContainer<IInStream<VVVVTouch>>(attribute, false);
        }

        public void Dispose()
        {
            FContainer.Dispose();
        }

        public override bool Sync()
        {
            var stream = FContainer.IOObject;
            var result = IsChanged = stream.Sync();
            if (result)
            {
                using (var writer = this.GetDynamicWriter())
                    foreach (var s in stream)
                        if (s != null)
                            writer.Write(DictionaryExtensions.EnsureValue(map, s, vvvvK => new MyTouch(vvvvK)));
                        else
                            writer.Write(TouchDevice.Empty);
            }
            return result;
        }

        public class MyTouch : ITouchDevice
        {
            Subject<TouchNotification> m = new Subject<TouchNotification>();
            public IObservable<TouchNotification> Notifications => m;
            IDisposable subscription;

            public MyTouch(VVVVTouch vvvvK)
            {
                subscription = vvvvK.Notifications.Subscribe(notification => {
                    var mn = NotificationConverter.ConvertNotification(notification, DummyNotificationSpaceTransformer.Instance) as TouchNotification;
                    if (mn != null)
                    {
                        m.OnNext(mn);
                    }
                });
            }
        }
    }
}
