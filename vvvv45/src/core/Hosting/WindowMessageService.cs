using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using VVVV.Hosting.Graph;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Win32;
using VVVV.Utils.Win32;

namespace VVVV.Hosting
{
    [ComVisible(false)]
    class WindowMessageService : IWindowMessageService
    {
        readonly List<Subclass> FSubclasses = new List<Subclass>();
        public ReadOnlyCollection<Subclass> Subclasses => FSubclasses.AsReadOnly();
        public event EventHandler SubclassCreated;
        public event EventHandler SubclassDestroyed;

        public IObservable<EventPattern<WMEventArgs>> MessageNotifications { get; }
        IDisposable FWMConnection;

        public WindowMessageService(IHDEHost host)
        {
            var hotObservable = Observable.FromEventPattern<WindowEventArgs>(host, "WindowAdded")
                .Select(p => p.EventArgs.Window)
                .Merge(
                    Observable.FromEventPattern<ComponentModeEventArgs>(host, "AfterComponentModeChange")
                        .Select(p => p.EventArgs.Window)
                )
                .OfType<Window>()
                .Where(w => w.UserInputWindow != null && w.UserInputWindow.InputWindowHandle != IntPtr.Zero)
                .Where(w => !FSubclasses.Any(s => s.HWnd == w.UserInputWindow.InputWindowHandle))
                .Select(w => Subclass.Create(w.UserInputWindow.InputWindowHandle, w.UserInputWindow))
                .Do(s => {
                    s.Disposed += (o, e) =>
                    {
                        if (s != null)
                        {
                            FSubclasses.Remove(s);
                            if (SubclassDestroyed != null)
                                SubclassDestroyed.Invoke(s, EventArgs.Empty);
                        }
                    };
                    FSubclasses.Add(s);
                    if (SubclassCreated != null)
                        SubclassCreated.Invoke(s, EventArgs.Empty);
                })
                .SelectMany(s => Observable.FromEventPattern<WMEventArgs>(s, "WindowMessage"))
                .Publish();
            FWMConnection = hotObservable.Connect();
            MessageNotifications = hotObservable;
        }

        public void Dispose()
        {
            foreach (var subclass in FSubclasses)
                subclass.Dispose();
            FSubclasses.Clear();

            FWMConnection.Dispose();
        }
    }
}
