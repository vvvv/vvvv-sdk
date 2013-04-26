#region usings

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using Tobii.Eyetracking.Sdk;
using Tobii.Eyetracking.Sdk.Exceptions;

#endregion usings


namespace IS1
{
    #region PluginInfo
    [PluginInfo(Name = "Browser", Category = "Devices", Version = "IS1", Help = "Eyetracker IS1 Node", Tags = "tobii,tracking", Author = "niggos, phlegma")]
    #endregion PluginInfo



    public class BrowserNode : IPluginEvaluate
    {

        [Input("Update", IsToggle = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FUpdateIn;


        [Input("Enable", IsToggle = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FEnable;

        [Output("Eyetracker Information")]
        ISpread<EyetrackerInfo> FEyetrackerInfoOut;

        [Output("Model")]
        ISpread<string> FModel;

        [Output("Name")]
        ISpread<string> FName;

        [Output("Product ID")]
        ISpread<string> FID;

        [Output("Gerneration")]
        ISpread<string> FGerneration;

        [Output("Version")]
        ISpread<string> FVersion;

        [Output("Status")]
        ISpread<string> FStatus;


        EyetrackerBrowser FEyetrackerBrowser;
        private List<EyetrackerInfo> FEyetrackerInfo = new List<EyetrackerInfo>();
        private bool FUpdate = false;
        private bool FETUpdatedTriggered = false;
        private bool FLibraryInitialized = false;

        [Import()]
        ILogger FLogger;

        public BrowserNode()
        {
            try
            {
                Library.Init();
                FEyetrackerBrowser = new EyetrackerBrowser();
                FEyetrackerBrowser.EyetrackerFound += _browser_EyetrackerFound;
                FEyetrackerBrowser.EyetrackerRemoved += _browser_EyetrackerRemoved;
                FEyetrackerBrowser.EyetrackerUpdated += _browser_EyetrackerUpdated;
                FLibraryInitialized = true;
            }
            catch (Exception ee)
            {
                FLogger.Log(LogType.Error, "Failed to initialize Tobii library, seems like tobii sdk is not installed ..");
                FLibraryInitialized = false;
            }
        }


        // called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            #region Enable Eyetracker and set handles
            if (FLibraryInitialized)
            {

                if (FEnable.IsChanged && FEnable[0] == true)
                {
                    FEyetrackerBrowser.Start();
                }
                else if (FEnable.IsChanged && FEnable[0] == false)
                {
                    FEyetrackerBrowser.Stop();
                }

                if (FUpdate || FUpdateIn.IsChanged)
                {
                    FModel.SliceCount = FName.SliceCount = FID.SliceCount = FGerneration.SliceCount = FVersion.SliceCount = FStatus.SliceCount = FEyetrackerInfoOut.SliceCount = FEyetrackerInfo.Count;
                    FEyetrackerInfoOut.AssignFrom(FEyetrackerInfo);

                    // int n = FModel.SliceCount;
                    // String s = "";

                    for (int i = 0; i < FEyetrackerInfo.Count; i++)
                    {
                        FModel[i] = FEyetrackerInfo[i].Model;
                        FName[i] = FEyetrackerInfo[i].GivenName;
                        FID[i] = FEyetrackerInfo[i].ProductId;
                        FGerneration[i] = FEyetrackerInfo[i].Generation;
                        FVersion[i] = FEyetrackerInfo[i].Version;
                        FStatus[i] = FEyetrackerInfo[i].Status;

                    }
                    if (FEyetrackerInfo.Count == 0)
                    {

                    }


                    FUpdate = false;
            #endregion Enable Eyetracker and set handles
                }
            }
        }

        private void _browser_EyetrackerFound(object sender, EyetrackerInfoEventArgs e)
        {
            FEyetrackerInfo.Add(e.EyetrackerInfo);
            FUpdate = true;
            FLogger.Log(LogType.Debug, "Eyetracker has been detected");
        }


        // react on EyetrackerRemoved
        private void _browser_EyetrackerRemoved(object sender, EyetrackerInfoEventArgs e)
        {
            if (FEyetrackerInfo.Contains(e.EyetrackerInfo))
                FEyetrackerInfo.Remove(e.EyetrackerInfo);
            FUpdate = true;
            FLogger.Log(LogType.Debug, "Eyetracker has been removed");
        }


        // react on EyetrackerUpdated
        private void _browser_EyetrackerUpdated(object sender, EyetrackerInfoEventArgs e)
        {
            EyetrackerInfo Info = e.EyetrackerInfo;
            if (FEyetrackerInfo.Contains(Info))
            {
                int Index = FEyetrackerInfo.IndexOf(Info);
                FEyetrackerInfo.RemoveAt(Index);
                FEyetrackerInfo.Insert(Index, Info);
            }

            FUpdate = true;
            FLogger.Log(LogType.Debug, "Eyetracker has been updated");
        }
    }
}


