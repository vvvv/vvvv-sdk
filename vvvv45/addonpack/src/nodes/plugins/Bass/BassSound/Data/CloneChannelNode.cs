using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass.Misc;

namespace vvvv.Nodes
{
    /// <summary>
    /// Clone a channel, so you can output it to multiple devices 
    /// (Note: There is a slight delay involved).
    /// </summary>
    public class CloneChannelNode : IPlugin, IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Clone";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Clones a channel, so we can output it in another device";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }
        #endregion

        private IPluginHost FHost;
        private int FHandle;

        private IValueIn FPinInHandle;
        private IValueOut FPinOutHandle;

        private DSP_BufferStream bufferStream;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Input Pins
            this.FHost.CreateValueInput("Handle In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

            this.FHost.CreateValueOutput("Handle Out", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);

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
            if (this.FPinInHandle.PinIsChanged)
            {
                if (bufferStream != null)
                {
                    bufferStream.Stop();
                }

                //Just Update the Handle
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                this.FHandle = Convert.ToInt32(Math.Round(dhandle));

                // create a buffer of the source stream (note: if the source stops playing, the clone stops as well)
                bufferStream = new DSP_BufferStream();
                bufferStream.ChannelHandle = this.FHandle; // the stream to copy
                bufferStream.DSPPriority = -4000;
                bufferStream.Start();

                this.FPinOutHandle.SetValue(0, bufferStream.BufferStream);

            }
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion


        #region IDisposable Members

        public virtual void Dispose()
        {
            this.bufferStream.Stop();
            this.bufferStream.Dispose();
        }

        #endregion
    }
}
