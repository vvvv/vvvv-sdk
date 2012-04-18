using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{

    public class SetEnvironmentVariableNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SetEnvironmentVariable";							//use CamelCaps and no spaces
                Info.Category = "Windows";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "sets windows environment variable";
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

        private IStringIn FPinOutName;
        private IStringIn FPinOutValue;
        private IEnumIn FPinInType;

        private IValueIn FPinInDoUpdate;

        private IValueOut FPinOutResult;
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.FHost.UpdateEnum("Environment Variables Targets", "Process", Enum.GetNames(typeof(System.EnvironmentVariableTarget)));

            this.FHost.CreateStringInput("Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutName);
            this.FPinOutName.SetSubType("", false);

            this.FHost.CreateStringInput("Value", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue);
            this.FPinOutValue.SetSubType("", false);

            this.FHost.CreateEnumInput("Target", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInType);
            this.FPinInType.SetSubType("Environment Variables Targets");

            this.FHost.CreateValueInput("Update", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoUpdate);
            this.FPinInDoUpdate.SetSubType(0, 1, 1, 0, true, false, false);

            this.FHost.CreateValueOutput("Result", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutResult);
            this.FPinOutResult.SetSubType(0, 1, 1, 0, true, false, false);

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
            this.FPinInDoUpdate.GetValue(0, out dorefresh);

            if (dorefresh >= 0.5)
            {
                for (int i = 0; i < SpreadMax; i++)
                {

                    EnvironmentVariableTarget target;
                    string starget, name, value;

                    this.FPinInType.GetString(i, out starget);
                    target = (EnvironmentVariableTarget)Enum.Parse(typeof(EnvironmentVariableTarget), starget);

                    this.FPinOutName.GetString(i, out name);
                    this.FPinOutValue.GetString(i, out value);

                    Environment.SetEnvironmentVariable(name, value, target);
                    this.FPinOutResult.SetValue(0, 1);
                }
            }
            else
            {
                this.FPinOutResult.SetValue(0, 0);
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
