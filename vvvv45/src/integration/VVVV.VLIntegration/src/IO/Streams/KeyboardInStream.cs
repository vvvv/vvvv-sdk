using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Windows.Forms;
using VL.Lib.IO;
using VL.Lib.IO.Notifications;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VLKeyboard = VL.Lib.IO.IKeyboard;
using VVVVKeyboard = VVVV.Utils.IO.IKeyboard;

namespace VVVV.VL.Hosting.IO.Streams
{
    class KeyboardInStream : MemoryIOStream<VLKeyboard>, IDisposable
    {
        readonly IIOContainer<IInStream<VVVVKeyboard>> FContainer;

        Dictionary<VVVVKeyboard, VLKeyboard> map = new Dictionary<VVVVKeyboard, VLKeyboard>();

        public KeyboardInStream(IIOFactory factory, InputAttribute attribute)
        {
            attribute.StringType = StringType.Filename;
            FContainer = factory.CreateIOContainer<IInStream<VVVVKeyboard>>(attribute, false);
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
                            writer.Write(DictionaryExtensions.EnsureValue(map, s, vvvvK => new MyKeyboard(vvvvK)));
                        else
                            writer.Write(Keyboard.Empty);
            }
            return result;
        }

        public class MyKeyboard : VLKeyboard
        {
            VVVVKeyboard VVVVK;
            Subject<KeyNotification> m = new Subject<KeyNotification>();
            public IObservable<KeyNotification> Notifications => m;
            public Keys Modifiers => VVVVK.Modifiers;
            public bool CapsLock => VVVVK.CapsLock;
            IDisposable subscription;

            public MyKeyboard(VVVVKeyboard vvvvK)
            {
                VVVVK = vvvvK;
                subscription = vvvvK.KeyNotifications.Subscribe(notification => {
                    var mn = NotificationConverter.ConvertNotification(notification, DummyNotificationSpaceTransformer.Instance) as KeyNotification;
                    if (mn != null)
                    {
                        m.OnNext(mn);
                    }
                });
            }
        }
    }
}
