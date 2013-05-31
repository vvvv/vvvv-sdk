using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Win32;
using VVVV.Hosting.Graph;
using VVVV.Hosting.IO;

namespace VVVV.Nodes.Input
{
    public abstract class WindowInputNode: IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
#pragma warning disable 0649
        [Input("Enabled", IsSingle = true, DefaultBoolean = true, Order = int.MinValue)]
        ISpread<bool> FEnabledIn;

        [Import]
        IHDEHost FHost;

        [Import]
        protected IOFactory FIOFactory;
#pragma warning restore 0649

        private readonly List<Subclass> FSubclasses = new List<Subclass>();

        private bool FEnabled;
        bool Enabled
        {
            get { return FEnabled; }
            set
            {
                if (value != FEnabled)
                {
                    if (FEnabled)
                        Unsubscribe();
                    FEnabled = value;
                    if (FEnabled)
                        Subscribe();
                }
            }
        }

        public virtual void OnImportsSatisfied()
        {
            Enabled = true;
        }

        public virtual void Dispose()
        {
            Enabled = false;
        }

        void Subscribe()
        {
            FHost.WindowAdded += HandleWindowAdded;
            FHost.AfterComponentModeChange += HandleAfterComponentModeChange;
        }

        void Unsubscribe()
        {
            FHost.WindowAdded -= HandleWindowAdded;
            FHost.AfterComponentModeChange -= HandleAfterComponentModeChange;
            foreach (var subclass in FSubclasses.ToArray())
                subclass.Dispose();
        }

        void HandleWindowAdded(object sender, WindowEventArgs args)
        {
            var window = args.Window as Window;
            TrySubclassWindow(window);
        }

        void HandleAfterComponentModeChange(object sender, ComponentModeEventArgs args)
        {
            var window = args.Window as Window;
            TrySubclassWindow(window);
        }

        void HandleSubclassDisposed(object sender, EventArgs e)
        {
            var subclass = sender as Subclass;
            subclass.WindowMessage -= HandleSubclassWindowMessage;
            subclass.Disposed -= HandleSubclassDisposed;
            FSubclasses.Remove(subclass);
        }

        void TrySubclassWindow(Window window)
        {
            var inputWindow = window.UserInputWindow;
            if (inputWindow != null)
            {
                var handle = inputWindow.InputWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    var subclass = Subclass.Create(handle);
                    subclass.WindowMessage += HandleSubclassWindowMessage;
                    subclass.Disposed += HandleSubclassDisposed;
                    FSubclasses.Add(subclass);
                }
            }
        }

        public void Evaluate(int spreadMax)
        {
            Enabled = spreadMax > 0 && FEnabledIn[0];
            Evaluate(spreadMax, Enabled);
        }

        protected abstract void Evaluate(int spreadMax, bool enabled);
        protected abstract void HandleSubclassWindowMessage(object sender, WMEventArgs e);
    }
}
