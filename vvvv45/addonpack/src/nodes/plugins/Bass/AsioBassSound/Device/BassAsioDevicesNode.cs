using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.BassAsio;

namespace vvvv.Nodes
{
    public class BassAsioDevicesNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "AsioDevices";
                Info.Category = "BassAsio";
                Info.Version = "";
                Info.Help = "Lists all Asio enabled evices in the system.";
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

        private IValueIn FPinInRefresh;

        private IStringOut FPinOutDevices;
        private IStringOut FPinOutDrivers;

        private bool FFirst = true;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Input
            this.FHost.CreateValueInput("Refresh", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInRefresh);
            this.FPinInRefresh.SetSubType(0, 1, 0, 0, true, false, true);

            //Output
            this.FHost.CreateStringOutput("Devices", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutDevices);
            this.FPinOutDevices.SetSubType("", false);

            this.FHost.CreateStringOutput("Drivers", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out this.FPinOutDrivers);
            this.FPinOutDrivers.SetSubType("", false);
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
            if (this.FPinInRefresh.PinIsChanged || this.FFirst)
            {
                double refresh;
                this.FPinInRefresh.GetValue(0, out refresh);

                if (refresh == 1 || this.FFirst)
                {
                    BASS_ASIO_DEVICEINFO[] devices = BassAsio.BASS_ASIO_GetDeviceInfos();

                    this.FPinOutDevices.SliceCount = devices.Length;
                    this.FPinOutDrivers.SliceCount = devices.Length;

                    int count = 0;
                    foreach (BASS_ASIO_DEVICEINFO device in devices)
                    {
                        this.FPinOutDevices.SetString(count, device.name);
                        this.FPinOutDrivers.SetString(count, device.driver);
                        count++;
                    }
                }
                this.FFirst = false;
            }
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
