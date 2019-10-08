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
using VVVV.PluginInterfaces.V2.Win32;
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

        [Import]
        IWindowMessageService FWMService;

        public virtual void OnImportsSatisfied()
        {
            FWMService.SubclassCreated += FWMService_SubclassCreated;
            FWMService.SubclassDestroyed += FWMService_SubclassDestroyed;
            
            var disabled = FEnabledIn.ToObservable(1)
                .DistinctUntilChanged()
                .Where(enabled => !enabled);
            var windowsMessages = FWMService.MessageNotifications
                .Where(_ => FEnabledIn.SliceCount > 0 && FEnabledIn[0]);
            Initialize(windowsMessages, disabled);
        }

        protected abstract void Initialize(IObservable<EventPattern<WMEventArgs>> windowMessages, IObservable<bool> disabled);

        protected virtual void SubclassCreated(Subclass subclass)
        {
        }

        protected virtual void SubclassDestroyed(Subclass subclass)
        {
        }

        protected ReadOnlyCollection<Subclass> Subclasses => FWMService.Subclasses;

        public virtual void Dispose()
        {
            FWMService.SubclassCreated -= FWMService_SubclassCreated;
            FWMService.SubclassDestroyed -= FWMService_SubclassDestroyed;
        }

        private void FWMService_SubclassCreated(object sender, EventArgs e)
        {
            SubclassCreated(sender as Subclass);
        }

        private void FWMService_SubclassDestroyed(object sender, EventArgs e)
        {
            SubclassDestroyed(sender as Subclass);
        }
    }
}
