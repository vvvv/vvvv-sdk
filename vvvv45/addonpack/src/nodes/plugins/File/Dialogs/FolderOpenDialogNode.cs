using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace VVVV.Nodes
{
    public class FolderOpenDialogNode : IPlugin,IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "FolderBrowserDialog";							//use CamelCaps and no spaces
                Info.Category = "File";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Opens a folder browser dialog (non blocking)";
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

        private IStringIn FPinInDefaultDir;
        private IValueFastIn FPinInOpen;
        private IValueFastIn FPinInAllowCreate;
        private IStringOut FPinOutPath;
        private FolderBrowserDialog odlg = new FolderBrowserDialog();

        private event EventHandler OnSelect;

        private bool FOpened = false;
        private bool FInvalidate = false;
        private Thread FThread;
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
            this.FPinInDefaultDir.SetSubType("", false);


            this.FHost.CreateValueFastInput("Allow Create", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInAllowCreate);
            this.FPinInAllowCreate.SetSubType(0, 1, 1, 0, false, true, true);
        
            this.FHost.CreateValueFastInput("Do Open", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInOpen);
            this.FPinInOpen.SetSubType(0, 1, 1, 0, true, false, true);

            //Output 
            this.FHost.CreateStringOutput("FPinOutPath", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPath);
            this.FPinOutPath.SetSubType("", true);

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
                string defaultdir;
                double allowcreate;

                this.FPinInDefaultDir.GetString(0, out defaultdir);

                this.FPinInAllowCreate.GetValue(0, out allowcreate);

                odlg.ShowNewFolderButton = allowcreate >= 0.5;
                odlg.RootFolder = Environment.SpecialFolder.MyComputer;

                try
                {
                    if (Directory.Exists(defaultdir))
                    {
                        odlg.SelectedPath = defaultdir;
                    }
                    else
                    {
                        odlg.SelectedPath = "";
                    }
                }
                catch
                {
                    odlg.SelectedPath = "";
                }

                this.OnSelect -= new EventHandler(FileDialogNode_OnSelect);
                this.OnSelect += new EventHandler(FileDialogNode_OnSelect);
                this.FOpened = true;
                FThread = new Thread(new ThreadStart(this.DoOpen));
                FThread.Start();
            }

            if (this.FInvalidate)
            {
                this.FPinOutPath.SliceCount = 1;
                this.FPinOutPath.SetString(0,odlg.SelectedPath);
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
