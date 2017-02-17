using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
    
    public class GetDrivesInfoNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Info";							//use CamelCaps and no spaces
                Info.Category = "System";						//try to use an existing one
                Info.Version = "Drive";						//versions are optional. leave blank if not needed
                Info.Help = "Retrieves info about window drives.";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "windows";

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
        private IStringOut FPinOutType;
        private IStringOut FPinOutFormat;
        private IStringOut FPinOutVolumeName;
        private IValueOut FPinOutAvailableSpace;
        private IValueOut FPinOutFreeSpace;
        private IValueOut FPinOutTotalSpace;
        private IValueOut FPinOutReady;


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

            this.FHost.CreateStringOutput("Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutName);
            this.FPinOutName.SetSubType("", false);

            this.FHost.CreateStringOutput("Type", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutType);
            this.FPinOutType.SetSubType("", false);

            this.FHost.CreateStringOutput("Format", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFormat);
            this.FPinOutFormat.SetSubType("", false);

            this.FHost.CreateStringOutput("Volume Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutVolumeName);
            this.FPinOutVolumeName.SetSubType("", false);
         
            this.FHost.CreateValueOutput("Available Free Space", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutAvailableSpace);
            this.FPinOutAvailableSpace.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);

            this.FHost.CreateValueOutput("Total Free Space", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFreeSpace);
            this.FPinOutFreeSpace.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);

            this.FHost.CreateValueOutput("Total Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutTotalSpace);
            this.FPinOutTotalSpace.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);

            this.FHost.CreateValueOutput("Is Ready", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutReady);
            this.FPinOutReady.SetSubType(0, 1, 1, 0, false, true, false);
        
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
                DriveInfo[] infos = DriveInfo.GetDrives();
                this.FPinOutName.SliceCount = infos.Length;
                this.FPinOutFormat.SliceCount = infos.Length;
                this.FPinOutType.SliceCount = infos.Length;
                this.FPinOutAvailableSpace.SliceCount = infos.Length;
                this.FPinOutFreeSpace.SliceCount = infos.Length;
                this.FPinOutReady.SliceCount = infos.Length;
                this.FPinOutTotalSpace.SliceCount = infos.Length;
                this.FPinOutVolumeName.SliceCount = infos.Length;

                for (int i = 0; i < infos.Length; i++)
                {
                    try
                    {
                        this.FPinOutName.SetString(i, infos[i].Name);
                        this.FPinOutType.SetString(i, infos[i].DriveType.ToString());
                        this.FPinOutFormat.SetString(i, infos[i].DriveFormat);
                        this.FPinOutAvailableSpace.SetValue(i, Convert.ToDouble(infos[i].AvailableFreeSpace));
                        this.FPinOutFreeSpace.SetValue(i, Convert.ToDouble(infos[i].TotalFreeSpace));
                        this.FPinOutTotalSpace.SetValue(i, Convert.ToDouble(infos[i].TotalSize));
                        this.FPinOutVolumeName.SetString(i, infos[i].VolumeLabel);
                        this.FPinOutReady.SetValue(i, Convert.ToDouble(infos[i].IsReady));
                    }
                    catch (IOException e)
                    {
                        this.FPinOutReady.SetValue(i, 0);
                    }
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
