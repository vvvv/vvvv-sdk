using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public interface IStartableStatus
    {
        bool Success { get;  }
        string Message { get; }
        string Name { get; }
        string TypeName { get; }
    }

    [ComVisible(false)]
    public interface IStartableRegistry
    {
        bool ProcessType(Type type, Assembly assembly);
        void ProcessAssembly(Assembly assembly);
        void ShutDown();
        void AddFromXml(string startablelist);
        List<IStartableStatus> Status { get; }
    }

}
