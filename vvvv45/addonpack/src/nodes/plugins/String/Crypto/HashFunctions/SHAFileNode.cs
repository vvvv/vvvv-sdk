using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.Security.Cryptography;
using System.IO;

namespace VVVV.Nodes
{
    public class Sha1FileNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SHA1";
                Info.Category = "File";
                Info.Version = "";
                Info.Help = "Calculate the SHA1 hash of a file";
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

        private IStringIn FPinInFiles;
        private IStringOut FPinOutHashes;
        private IValueOut FPinOutValid;

        private SHA1CryptoServiceProvider FSha1 = new SHA1CryptoServiceProvider();

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Filename", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInFiles);
            this.FPinInFiles.SetSubType("", true);

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
            if (this.FPinInFiles.PinIsChanged)
            {
                this.FPinOutHashes.SliceCount = this.FPinInFiles.SliceCount;
                this.FPinOutValid.SliceCount = this.FPinInFiles.SliceCount;

                #region Compute Hash for each File
                for (int i = 0; i < this.FPinInFiles.SliceCount; i++)
                {
                    string path;
                    this.FPinInFiles.GetString(i, out path);

                    if (File.Exists(path))
                    {
                        try
                        {
                            //Load file
                            FileStream fs = new FileStream(path, FileMode.Open);
                            
                            //Compute Hash
                            byte[] hash = this.FSha1.ComputeHash(fs);
                            fs.Close();

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
                    else
                    {

                        //File does not exist, invalid
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
