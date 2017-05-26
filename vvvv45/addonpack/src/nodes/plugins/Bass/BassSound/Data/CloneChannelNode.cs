using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass.Misc;
using BassSound.Internals;

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
                Info.Author = "vux";
                Info.Tags = "Audio,Sound";

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
        private ChannelsManager FManager;
        private CloneChannelInfo FChannel = new CloneChannelInfo();
        //private int FHandle;

        private IValueIn FPinInHandle;
        private IValueOut FPinOutHandle;

        private DSP_BufferStream bufferStream;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            //Input Pins
            this.FHost.CreateValueInput("Handle In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue,1, 0, false, false, true);

            this.FHost.CreateValueOutput("Handle Out", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutHandle);
            this.FPinOutHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

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
                this.ClearUp();

                if (this.FChannel.InternalHandle != 0)
                {
                    this.FManager.Delete(this.FChannel.InternalHandle);
                }

                //Just Update the Handle
                double dhandle;
                this.FPinInHandle.GetValue(0, out dhandle);
                int ihandle = Convert.ToInt32(Math.Round(dhandle));

                if (this.FManager.Exists(ihandle))
                {
                    this.FManager.CreateChannel(this.FChannel);
                    this.FChannel.Parent = this.FManager.GetChannel(ihandle);
                    this.FChannel.Parent.OnInit += new EventHandler(FChannel_OnInit);

                    if (this.FChannel.Parent.BassHandle.HasValue)
                    {
                        this.AddDSP();
                    }
                    else
                    {
                        this.FChannel.Parent = null;
                    }
                }
                else
                {
                    this.FChannel.Parent = null;
                }

                if (this.FChannel.Parent != null)
                {
                    this.FPinOutHandle.SetValue(0, this.FChannel.InternalHandle);
                }
            }
        }

        void FChannel_OnInit(object sender, EventArgs e)
        {
            this.AddDSP();
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
            this.ClearUp();
        }

        #endregion

        #region Add the DSP
        private void AddDSP()
        {
            // create a buffer of the source stream
            //We can't get it from the main stream otherwise it would interfere with the asio buffering
            bufferStream = new DSP_BufferStream();
            bufferStream.ChannelHandle = this.FChannel.Parent.BassHandle.Value; // the stream to copy
            bufferStream.DSPPriority = -4000;
            bufferStream.Start();
            this.FChannel.BassHandle = bufferStream.BufferStream;
        }
        #endregion

        #region Clear Up
        private void ClearUp()
        {
            try
            {
                this.bufferStream.Stop();
                this.bufferStream.Dispose();
            }
            catch
            {

            }
        }
        #endregion
    }
}
