using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using vvvv.Utils;

namespace vvvv.Nodes
{
    public class EyesWebScalarDecoderNode : IPlugin 
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "ScalarDecoder";
                Info.Category = "Network";
                Info.Version = "EyesWeb";
                Info.Help = "Read scalar values from Eyesweb";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "";

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

        private IPluginHost FHost;

        private IStringIn FPinInValue;
        private IValueIn FPinIsFloat;

        private IValueOut FPinOutValue;
        private IValueOut FPinOutBang;

        private bool FIsFloat = false;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInValue);

            this.FHost.CreateValueInput("Is Float", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinIsFloat);

            this.FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue);

            this.FHost.CreateValueOutput("Bang", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutBang);
            this.FPinOutBang.SetSubType(0, 1, 0, 0, true, false, true);
            
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
            if (this.FPinIsFloat.PinIsChanged)
            {
                double isfloat;
                this.FPinIsFloat.GetValue(0, out isfloat);
                this.FIsFloat = (isfloat == 1);
            }

            if (this.FPinInValue.PinIsChanged)
            {
                string val;
                this.FPinInValue.GetString(0, out val);

                if (val.Length > 0)
                {
                    try
                    {
                        double output;

                        //First 4 characters are packet length
                        string head = val.Substring(0, 4);

                        //Remainder is data
                        string tail = val.Substring(4);

                        TScalarRawData data = TTypeConverter.GetStructure<TScalarRawData>(tail);
                        if (this.FIsFloat)
                        {
                            output = data.d;
                        }
                        else
                        {
                            output = data.i;
                        }
                        this.FPinOutValue.SetValue(0, output);
                        this.FPinOutBang.SetValue(0, 1);
                    }
                    catch
                    {
                        this.FPinOutValue.SetValue(0, double.NaN);
                        this.FPinOutBang.SetValue(0, 0);
                    }
                }
                else
                {
                    this.FPinOutValue.SetValue(0, double.NaN);
                    this.FPinOutBang.SetValue(0, 0);
                }
            }
            else
            {
                this.FPinOutValue.SetValue(0, double.NaN);
                this.FPinOutBang.SetValue(0, 0);
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
