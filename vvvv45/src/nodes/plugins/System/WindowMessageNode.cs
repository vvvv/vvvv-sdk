using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                .Do(s =>
                    {
                        s.Disposed += HandleSubclassDisposed;
                        FSubclasses.Add(s);
                        SubclassCreated(s);
                    }
                )
                .SelectMany(s => Observable.FromEventPattern<WMEventArgs>(s, "WindowMessage"))
                .Select(p => p.EventArgs)
                .Where(_ => FEnabledIn.SliceCount > 0 && FEnabledIn[0]);
            Initialize(windowMessages);
        }

        void HandleSubclassDisposed(object sender, EventArgs e)
        {
            var subclass = sender as Subclass;
            if (subclass != null)
            {
                FSubclasses.Remove(subclass);
                SubclassDestroyed(subclass);
            }
        }

        protected abstract void Initialize(IObservable<WMEventArgs> windowMessages);

        protected virtual void SubclassCreated(Subclass subclass)
        {
        }

        protected virtual void SubclassDestroyed(Subclass subclass)
        {
        }

        protected ReadOnlyCollection<Subclass> Subclasses
        {
            get { return FSubclasses.AsReadOnly(); }
        }

        public virtual void Dispose()
        {
            foreach (var subclass in FSubclasses.ToArray())
                subclass.Dispose();
            FSubclasses.Clear();
        }
    }
}
