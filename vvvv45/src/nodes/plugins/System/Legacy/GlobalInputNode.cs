using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Input
{
    public abstract class GlobalInputNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
#pragma warning disable 0649
        [Input("Enabled", IsSingle = true, DefaultBoolean = true, Order = int.MinValue)]
        ISpread<bool> FEnabledIn;

        [Import]
        protected IOFactory FIOFactory;
#pragma warning restore 0649

        public virtual void OnImportsSatisfied()
        {
        }

        public virtual void Dispose()
        {
        }

        public void Evaluate(int spreadMax)
        {
            var enabled = spreadMax > 0 && FEnabledIn[0];
            Evaluate(spreadMax, enabled);
        }

        protected abstract void Evaluate(int spreadMax, bool enabled);
    }
}
