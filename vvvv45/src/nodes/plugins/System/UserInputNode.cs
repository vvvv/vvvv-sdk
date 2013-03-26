using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Win32;
using VVVV.Hosting.Graph;

namespace VVVV.Nodes.Input
{
    public abstract class UserInputNode: IPartImportsSatisfiedNotification, IDisposable
    {
#pragma warning disable 0649
        [Import]
        IHDEHost FHost;
#pragma warning restore 0649

        private readonly List<Subclass> FSubclasses = new List<Subclass>();

        public void OnImportsSatisfied()
        {
            FHost.WindowAdded += HandleWindowAdded;
        }

        public void Dispose()
        {
            FHost.WindowAdded -= HandleWindowAdded;
            foreach (var subclass in FSubclasses)
                subclass.Dispose();
        }

        void HandleWindowAdded(object sender, WindowEventArgs args)
        {
            var window = args.Window as Window;
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

        void HandleSubclassDisposed(object sender, EventArgs e)
        {
            var subclass = sender as Subclass;
            subclass.WindowMessage -= HandleSubclassWindowMessage;
            subclass.Disposed -= HandleSubclassDisposed;
            FSubclasses.Remove(subclass);
        }

        protected abstract void HandleSubclassWindowMessage(object sender, WMEventArgs e);
    }
}
