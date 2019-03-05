using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Windows.Forms;
using VL.Lib.IO;
using VL.Lib.IO.Notifications;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VLMouse = VL.Lib.IO.IMouse;
using VVVVMouse = VVVV.Utils.IO.IMouse;

namespace VVVV.VL.Hosting.IO.Streams
{
    class MouseInStream : MemoryIOStream<VLMouse>, IDisposable
    {
        readonly IIOContainer<IInStream<VVVVMouse>> FContainer;

        Dictionary<VVVVMouse, VLMouse> map = new Dictionary<VVVVMouse, VLMouse>();

        public MouseInStream(IIOFactory factory, InputAttribute attribute)
        {
            attribute.StringType = StringType.Filename;
            FContainer = factory.CreateIOContainer<IInStream<VVVVMouse>>(attribute, false);
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
                            writer.Write(DictionaryExtensions.EnsureValue(map, s, vvvvM => new MyMouse(vvvvM)));
                        else
                            writer.Write(Mouse.Empty);
            }
            return result;
        }

        public class MyMouse : VLMouse
        {
            VVVVMouse VVVVM;
            Subject<MouseNotification> m = new Subject<MouseNotification>();
            public IObservable<MouseNotification> Notifications => m;
            public MouseButtons PressedButtons => VVVVM.PressedButtons;
            IDisposable subscription;

            public MyMouse(VVVVMouse vvvvM)
            {
                VVVVM = vvvvM;
                subscription = vvvvM.MouseNotifications.Subscribe(notification => {
                    var mn = NotificationConverter.ConvertNotification(notification, DummyNotificationSpaceTransformer.Instance) as MouseNotification;
                    if (mn != null)
                    {
                        m.OnNext(mn);
                    }
                });
            }
        }
    }
}
