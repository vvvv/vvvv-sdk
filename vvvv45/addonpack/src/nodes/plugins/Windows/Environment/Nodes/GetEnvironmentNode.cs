using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using VVVV.PluginInterfaces.V1;


namespace VVVV.Nodes
{
    
    public class GetEnvironmentVariableNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "GetEnvironmentVariable";							//use CamelCaps and no spaces
                Info.Category = "Windows";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Retrieves windows environment variables";
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
        private IEnumIn FPinInType;

        private IStringOut FPinOutName;
        private IStringOut FPinOutValue;
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

            this.FHost.UpdateEnum("Environment Variables Targets", "Process", Enum.GetNames(typeof(System.EnvironmentVariableTarget)));
    
            this.FHost.CreateValueInput("Update", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoRefresh);
            this.FPinInDoRefresh.SetSubType(0, 1, 1, 0, true, false, false);

            this.FHost.CreateEnumInput("Target", TSliceMode.Single, TPinVisibility.True, out this.FPinInType);
            this.FPinInType.SetSubType("Environment Variables Targets");
           
            this.FHost.CreateStringOutput("Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutName);
            this.FPinOutName.SetSubType("", false);
    
            this.FHost.CreateStringOutput("Value", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue);
            this.FPinOutValue.SetSubType("", false);
           
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
                EnvironmentVariableTarget target;
                string starget;
                this.FPinInType.GetString(0, out starget);
                target = (EnvironmentVariableTarget)Enum.Parse(typeof(EnvironmentVariableTarget), starget);

                IDictionary vars = System.Environment.GetEnvironmentVariables(target);
                this.FPinOutName.SliceCount = vars.Count;
                this.FPinOutValue.SliceCount = vars.Count;

                int cnt = 0;
                foreach (object o in vars.Keys)
                {
                    this.FPinOutName.SetString(cnt, o.ToString());
                    this.FPinOutValue.SetString(cnt, vars[o].ToString());
                    cnt++;
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
