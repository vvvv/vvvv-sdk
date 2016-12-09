using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.Security.Cryptography;
using System.IO;

namespace VVVV.Nodes
{
    public class SHAStringNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SHA1";
                Info.Category = "String";
                Info.Version = "";
                Info.Help = "Calculate the SHA-1 hash of a string";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "hash,cryptography";

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

        private IStringIn FPinInString;
        private IStringOut FPinOutHashes;
        private IValueOut FPinOutValid;

        private SHA1CryptoServiceProvider FSha = new SHA1CryptoServiceProvider();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInString);
            this.FPinInString.SetSubType("", false);

            this.FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutHashes);
            this.FPinOutHashes.SetSubType("", false);

            this.FHost.CreateValueOutput("Is Valid", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValid);
            this.FPinOutValid.SetSubType(0, 1, 1, 0, false, true, true);
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
            if (this.FPinInString.PinIsChanged)
            {
                this.FPinOutHashes.SliceCount = this.FPinInString.SliceCount;
                this.FPinOutValid.SliceCount = this.FPinInString.SliceCount;

                #region Compute Hash for each File
                for (int i = 0; i < this.FPinInString.SliceCount; i++)
                {
                    string str;
                    this.FPinInString.GetString(i, out str);

                    try
                    {
                        byte[] strarray = Encoding.Default.GetBytes(str);

                        //Compute Hash
                        byte[] hash = this.FSha.ComputeHash(strarray);

                        StringBuilder sb = new StringBuilder();
                        foreach (byte hex in hash)
                        {
                            sb.Append(hex.ToString("x2"));
                        }

                        this.FPinOutHashes.SetString(i, sb.ToString());
                        this.FPinOutValid.SetValue(i, 1);
                    }
                    catch
                    {
                        this.FPinOutHashes.SetString(i, "");
                        this.FPinOutValid.SetValue(i, 0);
                    }
                }
                #endregion
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
