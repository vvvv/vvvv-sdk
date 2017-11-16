using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
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

        public virtual void OnImportsSatisfied()
        {
            var windowMessages = Observable.FromEventPattern<WindowEventArgs>(FHost, "WindowAdded")
                .Select(p => p.EventArgs.Window)
                .Merge(
                    Observable.FromEventPattern<ComponentModeEventArgs>(FHost, "AfterComponentModeChange")
                        .Select(p => p.EventArgs.Window)
                )
                .OfType<Window>()
                .Where(w => w.UserInputWindow != null && w.UserInputWindow.InputWindowHandle != IntPtr.Zero)
                .Where(w => !FSubclasses.Any(s => s.HWnd == w.UserInputWindow.InputWindowHandle))
                .Select(w => new { subclass = Subclass.Create(w.UserInputWindow.InputWindowHandle), sender = w.UserInputWindow })
                .Do(_ =>
                    {
                        _.subclass.Disposed += HandleSubclassDisposed;
                        FSubclasses.Add(_.subclass);
                        SubclassCreated(_.subclass);
                    }
                )
                .SelectMany(_ => Observable.FromEventPattern<WMEventArgs>(_.subclass, "WindowMessage"), 
                (_, evp) => new EventPattern<WMEventArgs>(_.sender, evp.EventArgs))
                .Where(_ => FEnabledIn.SliceCount > 0 && FEnabledIn[0]);
            var disabled = FEnabledIn.ToObservable(1)
                .DistinctUntilChanged()
                .Where(enabled => !enabled);
            Initialize(windowMessages, disabled);
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

        protected abstract void Initialize(IObservable<EventPattern<WMEventArgs>> windowMessages, IObservable<bool> disabled);

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
