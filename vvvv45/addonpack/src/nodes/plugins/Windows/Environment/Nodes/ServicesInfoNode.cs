using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using VVVV.PluginInterfaces.V1;
using System.ServiceProcess;

namespace VVVV.Nodes
{

    public class GetServicesInfoNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Info";							//use CamelCaps and no spaces
                Info.Category = "Windows";						//try to use an existing one
                Info.Version = "Services";						//versions are optional. leave blank if not needed
                Info.Help = "Retrieves Windows Services list and status";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        private IPluginHost FHost;

        private IValueIn FPinInDoRefresh;

        private IStringOut FPinOutName;
        private IStringOut FPinOutServiceName;
        private IStringOut FPinOutStatus;
        private IValueOut FPinOutCanStop;



        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.FHost.CreateValueInput("Update", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoRefresh);
            this.FPinInDoRefresh.SetSubType(0, 1, 1, 0, true, false, false);

            this.FHost.CreateStringOutput("Display Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutName);
            this.FPinOutName.SetSubType("", false);

            this.FHost.CreateStringOutput("Service Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutServiceName);
            this.FPinOutServiceName.SetSubType("", false);

            this.FHost.CreateStringOutput("Status", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutStatus);
            this.FPinOutStatus.SetSubType("", false);

            this.FHost.CreateValueOutput("Can Stop", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutCanStop);
            this.FPinOutCanStop.SetSubType(0, 1, 1, 0, false, true, false);

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
            double dorefresh;
            this.FPinInDoRefresh.GetValue(0, out dorefresh);

            if (dorefresh >= 0.5)
            {
                ServiceController[] infos = ServiceController.GetServices();
                this.FPinOutName.SliceCount = infos.Length;
                this.FPinOutStatus.SliceCount = infos.Length;
                this.FPinOutCanStop.SliceCount = infos.Length;
                this.FPinOutServiceName.SliceCount = infos.Length;

                for (int i = 0; i < infos.Length; i++)
                {
                    this.FPinOutName.SetString(i, infos[i].DisplayName);
                    this.FPinOutStatus.SetString(i, infos[i].Status.ToString());
                    this.FPinOutCanStop.SetValue(i, Convert.ToDouble(infos[i].CanStop));
                    this.FPinOutServiceName.SetString(i, infos[i].ServiceName);
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }


}
