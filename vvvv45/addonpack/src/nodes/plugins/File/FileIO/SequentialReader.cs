using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.IO;

namespace VVVV.Nodes
{

    public class SequentialReaderNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SequentialReader";							//use CamelCaps and no spaces
                Info.Category = "File";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Reads a file line by line";
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
        private IStringIn FPinInFile;
        private IValueIn FPinInReset;
        private IValueIn FPinInDoRead;
        private IValueIn FPinInSpreadCount;

        private IStringOut FPinOutLine;
        private IValueOut FPinOutEndFile;

        private StreamReader FReader;
        private int FSpreadcount = 1;
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

            
            this.FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out this.FPinInFile);
            this.FPinInFile.SetSubType("", true);

            this.FHost.CreateValueInput("Do Read", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoRead);
            this.FPinInDoRead.SetSubType(0, 1, 1, 0, true, false, true);
        
            this.FHost.CreateValueInput("Reset", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInReset);
            this.FPinInReset.SetSubType(0, 1, 1, 0, true, false, true);

            
            this.FHost.CreateValueInput("Spread Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSpreadCount);
            this.FPinInSpreadCount.SetSubType(1, double.MaxValue, 1, 1, false, false, true);
        

            //Outputs
            
            this.FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutLine);
            this.FPinOutLine.SetSubType("", false);
         
            this.FHost.CreateValueOutput("End of File", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutEndFile);
            this.FPinOutEndFile.SetSubType(0, 1, 1, 0, true, false, true);
             
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
            double spreadcount;
            this.FPinInSpreadCount.GetValue(0, out spreadcount);
            this.FSpreadcount = Convert.ToInt32(spreadcount);

            double reset, next;
            this.FPinInReset.GetValue(0, out reset);

            #region Reset
            if (this.FPinInFile.PinIsChanged || reset >= 0.5)
            {
                this.Clear();
                string path;
                this.FPinInFile.GetString(0, out path);

                if (File.Exists(path))
                {
                    this.FReader = new StreamReader(path);
                    this.FPinOutEndFile.SliceCount = 1;
                    this.FPinOutLine.SliceCount = 1;
                    this.FPinOutEndFile.SetValue(0,0);

                }
                else
                {
                    this.FPinOutEndFile.SliceCount = 0;
                    this.FPinOutLine.SliceCount = 0;
                }
            }
            #endregion

            this.FPinInDoRead.GetValue(0,out next);

            #region Read Line
            if (this.FReader != null && next >= 0.5)
            {
                this.FPinOutLine.SliceCount = this.FSpreadcount;

                for (int i = 0; i < this.FSpreadcount; i++)
                {
                    int end = 0;
                    if (!this.FReader.EndOfStream)
                    {
                        string line = this.FReader.ReadLine();
                        this.FPinOutLine.SetString(i, line);
                    }
                    else
                    {
                        end = 1;
                        this.FPinOutLine.SetString(i, "");
                    }

                    this.FPinOutEndFile.SetValue(0, end);
                }
            }
            #endregion

        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            this.Clear();
        }
        #endregion

        private void Clear()
        {
            if (this.FReader != null)
            {
                this.FReader.Close();
                this.FReader.Dispose();
            }
        }
    }
        
        
}
