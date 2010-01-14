using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;

namespace VVVV.Nodes
{
    
    public class RStringNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "R";							//use CamelCaps and no spaces
                Info.Category = "String";						//try to use an existing one
                Info.Version = "Advanced";						//versions are optional. leave blank if not needed
                Info.Help = "Node template";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";

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
        private StringDataHolder FData;

        private IStringIn FPinInReceiveString;
        private IStringOut FPinOutValue;
        private string FKey = "";

        private bool FInvalidate = false;

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
            this.FData = StringDataHolder.Instance;
            this.FData.OnAdd += FData_OnAdd;
            this.FData.OnRemove += this.FData_OnAdd;
            this.FData.OnUpdate += this.FData_OnAdd;
    
            this.FHost.CreateStringInput("Receive String", TSliceMode.Single, TPinVisibility.True, out this.FPinInReceiveString);
            this.FPinInReceiveString.SetSubType("", false);

            
            this.FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue);
            this.FPinOutValue.SetSubType("", false);
        
        
        }

        void FData_OnAdd(string key)
        {
            if (this.FKey == key)
            {
                this.FInvalidate = true;
            }
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
            if (this.FPinInReceiveString.PinIsChanged)
            {
                this.FPinInReceiveString.GetString(0, out this.FKey);
                if (this.FKey == null)
                {
                    this.FKey = "";
                }
                this.FInvalidate = true;
            }

            if (this.FInvalidate)
            {
                List<string> dbl = this.FData.GetData(this.FKey);
                this.FPinOutValue.SliceCount = dbl.Count;
                for (int i = 0; i < dbl.Count; i++)
                {
                    this.FPinOutValue.SetString(i, dbl[i]);
                }
                this.FInvalidate = false;
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
