using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VVVV.Utils.Win32;

namespace VVVV.PluginInterfaces.V2.Win32
{
    [ComVisible(false)]
    public interface IWindowMessageService
    {
        IObservable<EventPattern<WMEventArgs>> MessageNotifications { get; }
        ReadOnlyCollection<Subclass> Subclasses { get; }
        event EventHandler SubclassCreated;
        event EventHandler SubclassDestroyed;
    }
}
