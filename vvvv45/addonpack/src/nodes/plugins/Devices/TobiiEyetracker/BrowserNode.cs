#region usings

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using Tobii.Eyetracking.Sdk;
using Tobii.Eyetracking.Sdk.Exceptions;

#endregion usings


namespace TobiiEyetracker
{
    #region PluginInfo
    [PluginInfo(Name = "Browser", Category = "Devices", Version = "TobiiEyetracker", Help = "TobiiEyetracker Browser Node", Tags = "", Author = "niggos, phlegma")]
    #endregion PluginInfo


    public class BrowserNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Update", IsBang = true, DefaultValue = 0, IsSingle = true)]
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

        [Output("Initialized")]
        ISpread<bool> FInitialized;

        EyetrackerBrowser FEyetrackerBrowser;
        private List<EyetrackerInfo> FEyetrackerInfo = new List<EyetrackerInfo>();
        private bool FUpdate = false;
        private bool FETUpdatedTriggered = false;
        private bool FLibraryInitialized = false;

        [Import()]
        ILogger FLogger;

        #endregion

        // constructor (init tobii library)
        public BrowserNode()
        {
            
        }

        private void Initialize()
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
                FLogger.Log(LogType.Debug, "Failed to initilize Library: " + ee.Message);
                FLibraryInitialized = false;
            }
        }

        // called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
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

                    for (int i = 0; i < FEyetrackerInfo.Count; i++)
                    {
                        FModel[i] = FEyetrackerInfo[i].Model;
                        FName[i] = FEyetrackerInfo[i].GivenName;
                        FID[i] = FEyetrackerInfo[i].ProductId;
                        FGerneration[i] = FEyetrackerInfo[i].Generation;
                        FVersion[i] = FEyetrackerInfo[i].Version;
                        FStatus[i] = FEyetrackerInfo[i].Status;
                    }
                    FUpdate = false;
                }
            }
            else
            {
                if (FEnable[0] == true)
                    Initialize();
            }
            FInitialized[0] = FLibraryInitialized;
        }

        // Eyetracker found
        private void _browser_EyetrackerFound(object sender, EyetrackerInfoEventArgs e)
        {
            FEyetrackerInfo.Add(e.EyetrackerInfo);
            FUpdate = true;
            FLogger.Log(LogType.Debug, "Eyetracker has been detected");
        }

        // EyetrackerRemoved
        private void _browser_EyetrackerRemoved(object sender, EyetrackerInfoEventArgs e)
        {
            if (FEyetrackerInfo.Contains(e.EyetrackerInfo))
                FEyetrackerInfo.Remove(e.EyetrackerInfo);
            FUpdate = true;
            FLogger.Log(LogType.Debug, "Eyetracker has been removed");
        }

        // EyetrackerUpdated
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


