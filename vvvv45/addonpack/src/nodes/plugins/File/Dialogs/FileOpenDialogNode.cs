using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.Windows.Forms;
using System.Threading;

namespace VVVV.Nodes
{  
    public class FileDialogNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "OpenFileDialog";							//use CamelCaps and no spaces
                Info.Category = "File";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Opens a file dialog (without blocking vvvv)";
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

        private IStringIn FPinInDefaultDir;
        private IStringIn FPinInFilter;
        private IValueFastIn FPinInAllowMultiple;
        private IValueFastIn FCheckPathExists;
        private IValueFastIn FPinInOpen;
        private IStringOut FPinOutPath;
        private Thread FThread;

        private OpenFileDialog odlg = new OpenFileDialog();

        private event EventHandler OnSelect;

        private bool FOpened = false;
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

            
            this.FHost.CreateStringInput("Default Directory", TSliceMode.Single, TPinVisibility.True, out this.FPinInDefaultDir);
            this.FPinInDefaultDir.SetSubType2(string.Empty, int.MaxValue, string.Empty, TStringType.Directory);
       
            this.FHost.CreateStringInput("Filter", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilter);
            this.FPinInFilter.SetSubType("All files (*.*)|*.*", false);

            
            this.FHost.CreateValueFastInput("Multi Select", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInAllowMultiple);
            this.FPinInAllowMultiple.SetSubType(0, 1, 1, 0, false, true, false);
            
            this.FHost.CreateValueFastInput("Check Path Exists", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FCheckPathExists);
            this.FCheckPathExists.SetSubType(0, 1, 1, 0, false, true, false);
        
            this.FHost.CreateValueFastInput("Do Open", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInOpen);
            this.FPinInOpen.SetSubType(0, 1, 1, 0, true, false, false);

            //Output 
            this.FHost.CreateStringOutput("Path", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPath);
            this.FPinOutPath.SetSubType2(string.Empty, int.MaxValue, string.Empty, TStringType.Directory);
            
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
            double doopen;
            this.FPinInOpen.GetValue(0, out doopen);

            if (doopen >= 0.5 && !this.FOpened)
            {
                string defaultdir, filter;
                double allowmultiple, checkPath;

                this.FPinInDefaultDir.GetString(0, out defaultdir);
                this.FPinInFilter.GetString(0, out filter);

                this.FPinInAllowMultiple.GetValue(0, out allowmultiple);
                this.FCheckPathExists.GetValue(0, out checkPath);

                odlg.Multiselect = allowmultiple >= 0.5;
                odlg.InitialDirectory = defaultdir;
                odlg.Filter = filter;
                odlg.CheckPathExists = checkPath >= 0.5;
                odlg.CheckFileExists = odlg.CheckPathExists;
                
                this.OnSelect -= new EventHandler(FileDialogNode_OnSelect);
                this.OnSelect += new EventHandler(FileDialogNode_OnSelect);
                this.FOpened = true;
                this.FThread = new Thread(new ThreadStart(this.DoOpen));
                this.FThread.SetApartmentState(ApartmentState.STA);
                this.FThread.Start();
            }

            if (this.FInvalidate)
            {
                this.FPinOutPath.SliceCount = odlg.FileNames.Length;
                for (int i = 0; i < odlg.FileNames.Length; i++)
                {
                    this.FPinOutPath.SetString(i, odlg.FileNames[i]);
                }
                this.FInvalidate = false;
            }
        }

        void FileDialogNode_OnSelect(object sender, EventArgs e)
        {
            this.FInvalidate = true;
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (this.FOpened)
            {
                try
                {
                    this.FThread.Abort();
                }
                catch
                {

                }
            }
        }
        #endregion

        #region Do Open
        private void DoOpen()
        {
            if (odlg.ShowDialog() == DialogResult.OK)
            {
                if (this.OnSelect != null)
                {
                    OnSelect(this, new EventArgs());
                }
            }
            this.FOpened = false;
        }
        #endregion
    }
        
        
}
