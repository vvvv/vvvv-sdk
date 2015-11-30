using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using BassSound.Internals;
using System.IO;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.WaDsp;

namespace BassSound.DSP
{
    public class WinampDSPNode : IPlugin,IDisposable
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Winamp";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Loads a winamp DSP";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound,DSP";

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
        private ChannelInfo FChannel;
        private ChannelsManager FManager;

        private IStringIn FPinInFilename;
        private IValueIn FPinInHandle;
        private IValueIn FPinInPriority;
        private IValueIn FPinInEnabled;
        private IValueIn FPinInShowConfig;

        private IValueOut FPinOutFxHandle;

        protected int FDSPHandle = 0;
        private bool FConnected = false;
        private string FFilename;

        #region SetPluginHost
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;
            this.FManager = ChannelsManager.GetInstance();

            //Input Pins
            this.FHost.CreateValueInput("Handle In", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInHandle);
            this.FPinInHandle.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateValueInput("Priority", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPriority);
            this.FPinInPriority.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);

            this.FHost.CreateStringInput("File Name", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);

            this.FHost.CreateValueInput("Show Config", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInShowConfig);
            this.FPinInShowConfig.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueOutput("FX Handle", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutFxHandle);

            //We put the enabled node at the end
            this.FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEnabled);
            this.FPinInHandle.SetSubType(0, 1, 1, 0, false,true, true);
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool reset = false;

            #region Reset is handle or fconnected change
            if (FConnected != this.FPinInHandle.IsConnected)
            {
                if (this.FPinInHandle.IsConnected)
                {
                    reset = true;
                }
                else
                {
                    this.ClearUp();
                }
                this.FConnected = this.FPinInHandle.IsConnected;
            }
            #endregion

            #region Reset
            if (this.FPinInHandle.PinIsChanged || reset || this.FPinInEnabled.PinIsChanged || this.FPinInFilename.PinIsChanged)
            {
                this.ClearUp();

                if (this.FPinInHandle.IsConnected)
                {
                    if (this.FPinInFilename.SliceCount > 0)
                    {

                        this.FPinInFilename.GetString(0, out this.FFilename);

                        this.FFilename = this.FFilename == null ? "" : this.FFilename;

                        //Just Update the Handle
                        double dhandle;
                        this.FPinInHandle.GetValue(0, out dhandle);
                        int ihandle = Convert.ToInt32(Math.Round(dhandle));


                        if (File.Exists(this.FFilename) && this.FManager.Exists(ihandle))
                        {
                            this.FChannel = this.FManager.GetChannel(ihandle);
                            this.FChannel.OnInit += new EventHandler(FChannel_OnInit);
                            if (this.FChannel.BassHandle.HasValue)
                            {
                                this.AddDSP();
                            }
                        }
                        else
                        {
                            this.FChannel = null;
                            this.FDSPHandle = 0;
                        }
                    }
                }
            }
            #endregion


            double dshow;
            this.FPinInShowConfig.GetValue(0, out dshow);
            if (this.FDSPHandle != 0 && dshow >= 0.5)
            {
                BassWaDsp.BASS_WADSP_Config(this.FDSPHandle, 0);
            }
        }

        void FChannel_OnInit(object sender, EventArgs e)
        {
            this.ClearUp();
            this.AddDSP();
        }
        #endregion

        #region Clear Up
        private void ClearUp()
        {
            
            try
            {
                if (this.FChannel.BassHandle.HasValue && this.FDSPHandle != 0)
                {
                    BassWaDsp.BASS_WADSP_Stop(this.FDSPHandle);
                    Bass.BASS_ChannelRemoveDSP(this.FChannel.BassHandle.Value, this.FDSPHandle);
                    BassWaDsp.BASS_WADSP_FreeDSP(this.FDSPHandle);
                }
            }
            catch
            {

            }
            
            this.FDSPHandle = 0;
        }
        #endregion

        #region Add the DSP
        private void AddDSP()
        {
            if (this.FChannel != null)
            {
                double dpriority;
                this.FPinInPriority.GetValue(0, out dpriority);

                double denabled;
                this.FPinInEnabled.GetValue(0, out denabled);

                if (this.FChannel.BassHandle.HasValue && denabled >= 0.5)
                {
                    this.FDSPHandle = BassWaDsp.BASS_WADSP_Load(this.FFilename, 5, 5, 100, 100, null);
                    
                    string dspDesc = BassWaDsp.BASS_WADSP_GetName(this.FDSPHandle);
                    string moduleName = BassWaDsp.BASS_WADSP_GetModuleName(this.FDSPHandle, 0);

                    BassWaDsp.BASS_WADSP_Start(this.FDSPHandle, 0, this.FChannel.BassHandle.Value);

                    BassWaDsp.BASS_WADSP_ChannelSetDSP(this.FDSPHandle, this.FChannel.BassHandle.Value,Convert.ToInt32(dpriority));

                    this.FPinOutFxHandle.SliceCount = 1;
                    this.FPinOutFxHandle.SetValue(0, this.FDSPHandle);
                }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.ClearUp();
        }
        #endregion


    }
}
