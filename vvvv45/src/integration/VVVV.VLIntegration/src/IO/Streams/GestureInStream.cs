using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using VL.Lib.IO;
using VL.Lib.IO.Notifications;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VLGesture = VL.Lib.IO.IGestureDevice;
using VVVVGesture = VVVV.Utils.IO.GestureDevice;

namespace VVVV.VL.Hosting.IO.Streams
{
    class GestureInStream : MemoryIOStream<VLGesture>, IDisposable
    {
        readonly IIOContainer<IInStream<VVVVGesture>> FContainer;

        Dictionary<VVVVGesture, VLGesture> map = new Dictionary<VVVVGesture, VLGesture>();

        public GestureInStream(IIOFactory factory, InputAttribute attribute)
        {
            attribute.StringType = StringType.Filename;
            FContainer = factory.CreateIOContainer<IInStream<VVVVGesture>>(attribute, false);
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
                            writer.Write(DictionaryExtensions.EnsureValue(map, s, vvvvK => new MyGesture(vvvvK)));
                        else
                            writer.Write(GestureDevice.Empty);
            }
            return result;
        }

        public class MyGesture : IGestureDevice
        {
            Subject<GestureNotification> m = new Subject<GestureNotification>();
            public IObservable<GestureNotification> Notifications => m;
            IDisposable subscription;

            public MyGesture(VVVVGesture vvvvK)
            {
                subscription = vvvvK.Notifications.Subscribe(notification => {
                    var mn = NotificationConverter.ConvertNotification(notification, DummyNotificationSpaceTransformer.Instance) as GestureNotification;
                    if (mn != null)
                    {
                        m.OnNext(mn);
                    }
                });
            }
        }
    }
}
