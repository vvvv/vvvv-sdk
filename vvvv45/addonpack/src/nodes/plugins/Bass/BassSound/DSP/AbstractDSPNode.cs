using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace vvvv.Nodes
{
    /// <summary>
    /// Abstract Type for all DSPs, so we can have a common beahaviour.
    /// Basically DSP Node does nothing by themselves, and return an auto generated ID.
    /// All the effects are applied in the setDSP node, this will just return the right structure.
    /// </summary>
    /// <typeparam name="T">Effect Structure to send to bass.</typeparam>
    public abstract class AbstractDSPNode<T>
    {
        private IPluginHost FHost;

        private IValueIn FPinInPriority;

        #region SetPluginHost
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateValueInput("Priority", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPriority);
            this.FPinInPriority.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {

        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

    }
}
