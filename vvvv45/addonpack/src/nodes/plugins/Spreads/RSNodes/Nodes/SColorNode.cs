using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using VVVV.Utils.VColor;

namespace VVVV.Nodes
{
    
    public class SColorNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "S";							//use CamelCaps and no spaces
                Info.Category = "Color";						//try to use an existing one
                Info.Version = "Advanced";						//versions are optional. leave blank if not needed
                Info.Help = "LTP version of S (Color)";
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
        private ColorDataHolder FData;

        private IColorIn FPinInput;
        private IStringIn FPinInSendString;
        private string FKey = null;
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

            this.FData = ColorDataHolder.Instance;
     
            this.FHost.CreateColorInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType(VColor.Black,true);

            this.FHost.CreateStringInput("Send String", TSliceMode.Single, TPinVisibility.True, out this.FPinInSendString);
            this.FPinInSendString.SetSubType("send", false); 
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
            bool update = false;
            if (this.FPinInSendString.PinIsChanged)
            {
                string key;
                this.FPinInSendString.GetString(0, out key);
                if (key == null) { key = ""; }

                //First frame
                if (this.FKey == null)
                {
                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }
                else
                {
                    if (this.FKey.Length > 0)
                    {
                        this.FData.RemoveInstance(this.FKey);
                    }
                    
                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }

                update = true;
            }

            if (this.FPinInput.PinIsChanged || update)
            {
                List<RGBAColor> dbl = new List<RGBAColor>();
                for (int i = 0; i < this.FPinInput.SliceCount; i++)
                {
                    RGBAColor d;
                    this.FPinInput.GetColor(i, out d);
                    dbl.Add(d);
                }
                this.FData.UpdateData(this.FKey, dbl);
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (this.FKey != null)
                this.FData.RemoveInstance(this.FKey);
        }
        #endregion
    }
        
        
}
