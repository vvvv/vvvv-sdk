using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{  
	[PluginInfo(Name = "Dialog",
	            Category = "File",
	            Version = "Save",
	            Help = "Opens a dialog (without blocking vvvv) for chosing a file to save.",
	            Tags = "",
	            AutoEvaluate = true,
	            Author = "vux, vvvv group")]
    public class FileSaveDialogNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins
        private IPluginHost FHost;

        [Input ("Title", IsSingle = true, DefaultString = "Save...")]
        ISpread<string> FPinInTitle;
        
        [Input ("Default Directory", IsSingle = true, StringType = StringType.Directory)]
        ISpread<string> FPinInDefaultDir;
        
        [Input ("Filter", IsSingle = true, DefaultString = "All Files (*.*)|*.*")]
        ISpread<string> FPinInFilter;
        
        [Input ("Check Path Exists", IsSingle = true)]
        ISpread<bool> FCheckPathExists;
        
        [Input ("Save", IsSingle = true, IsBang = true)]
        ISpread<bool> FPinInOpen;
        
        [Output ("Path", StringType = StringType.Filename)]
        ISpread<string> FPinOutPath;
        
        [Output ("Saved", IsSingle = true)]
        ISpread<bool> FBangOut;
        
        private Thread FThread;

        private SaveFileDialog FDialog = new SaveFileDialog();

        private event EventHandler OnSelect;

        private bool FSaved = false;
        private bool FInvalidate = false;
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
        	FBangOut[0] = false;
        	
            if (FPinInOpen[0] && !FSaved)
            {
            	FDialog.Title = FPinInTitle[0];
                FDialog.InitialDirectory = FPinInDefaultDir[0];
                FDialog.Filter = FPinInFilter[0];
                FDialog.CheckPathExists = FCheckPathExists[0];
                FDialog.CheckFileExists = FCheckPathExists[0];
                
                OnSelect -= new EventHandler(FileDialogNode_OnSelect);
                OnSelect += new EventHandler(FileDialogNode_OnSelect);
                FSaved = true;
                FThread = new Thread(new ThreadStart(DoSave));
                FThread.SetApartmentState(ApartmentState.STA);
                FThread.Start();
            }

            if (FInvalidate)
            {
                FPinOutPath.SliceCount = FDialog.FileNames.Length;
                for (int i = 0; i < FDialog.FileNames.Length; i++)
                	FPinOutPath[i] = FDialog.FileNames[i];
                FBangOut[0] = true;
                FInvalidate = false;
            }
        }

        void FileDialogNode_OnSelect(object sender, EventArgs e)
        {
            FInvalidate = true;
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (FSaved)
            {
                try
                {
                    FThread.Abort();
                }
                catch
                {

                }
            }
        }
        #endregion

        #region Do Open
        private void DoSave()
        {
            if (FDialog.ShowDialog() == DialogResult.OK)
            {
                if (OnSelect != null)
                {
                    OnSelect(this, new EventArgs());
                }
            }
            FSaved = false;
        }
        #endregion
    }
}
