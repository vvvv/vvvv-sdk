using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VL.Lang.Model;
using VL.Lang.Symbols;
using VVVV.PluginInterfaces.V1;

namespace VVVV.VL.Factories
{
    public class Node : IPlugin, IDisposable
    {
        private readonly VLType FType;
        private readonly IRuntimeHost FRuntimeHost;

        public Node(VLType type, IRuntimeHost runtimeHost)
        {
            FType = type;
            FRuntimeHost = runtimeHost;
            FRuntimeHost.Updated += HandleRuntimeHostUpdated;
        }

        public void Dispose()
        {
            FRuntimeHost.Updated -= HandleRuntimeHostUpdated;
        }

        void HandleRuntimeHostUpdated(object sender, RuntimeUpdatedEventArgs e)
        {
            //var compilation = FRuntimeHost.
        }

        public void SetPluginHost(IPluginHost host)
        {
            
        }

        public void Configurate(IPluginConfig input)
        {
        }

        public void Evaluate(int spreadMax)
        {
        }

        public bool AutoEvaluate
        {
            get { return false; }
        }
    }
}
