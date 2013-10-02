using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.Hosting;
using VVVV.Hosting.Graph;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.Input
{
    public abstract class WindowMessageNode : IPluginBase, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Enabled", IsSingle = true, DefaultBoolean = true, Order = int.MinValue)]
        public IDiffSpread<bool> FEnabledIn;

        [Import]
        protected IHDEHost FHost;

        [Import]
        protected IOFactory FIOFactory;

        //IObservable<KeyNotification> FKeyNotifications;
        private readonly List<Subclass> FSubclasses = new List<Subclass>();

        public void OnImportsSatisfied()
        {
            var windowMessages = Observable.FromEventPattern<WindowEventArgs>(FHost, "WindowAdded")
                .Select(p => p.EventArgs.Window)
                .Merge(
                    Observable.FromEventPattern<ComponentModeEventArgs>(FHost, "AfterComponentModeChange")
                        .Select(p => p.EventArgs.Window)
                )
                .OfType<Window>()
                .Where(w => w.UserInputWindow != null && w.UserInputWindow.InputWindowHandle != IntPtr.Zero)
                .Select(w => Subclass.Create(w.UserInputWindow.InputWindowHandle))
                .Do(s => FSubclasses.Add(s))
                .SelectMany(s => Observable.FromEventPattern<WMEventArgs>(s, "WindowMessage"))
                .Select(p => p.EventArgs)
                .Where(_ => FEnabledIn.SliceCount > 0 && FEnabledIn[0]);
                //.Where(e => e.Message >= WM.KEYFIRST && e.Message <= WM.KEYLAST)
                //.Select<WMEventArgs, KeyNotification>(e =>
                //    {
                //        switch (e.Message)
                //        {
                //            case WM.KEYDOWN:
                //            case WM.SYSKEYDOWN:
                //                    return new KeyDownNotification(e);
                //            case WM.CHAR:
                //            case WM.SYSCHAR:
                //                    return new KeyPressNotification(e);
                //            case WM.KEYUP:
                //            case WM.SYSKEYUP:
                //                    return new KeyUpNotification(e);
                //        }
                //        return null;
                //    }
                //);
            Initialize(windowMessages);
        }

        protected abstract void Initialize(IObservable<WMEventArgs> windowMessages);

        public virtual void Dispose()
        {
            foreach (var subclass in FSubclasses)
                subclass.Dispose();
            FSubclasses.Clear();
        }
    }
}
