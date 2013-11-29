#region usings
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using Phidgets;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Manager",
                Category = "Devices",
                Version = "Phidget",
                Help = "Shows all the Phidgets connected to USB Port",
                Tags = "",
                Author = "Phlegma",
                AutoEvaluate = true
)]
    #endregion PluginInfo

    public class ManagerNode : IPluginEvaluate
    {
        #region fields & pins


        //Input
        [Input("Update", IsBang=true, IsSingle=true)]
        IDiffSpread<bool> FUpdate;

        //Output
        [Output("Device Name")]
        ISpread<string> FName;

        [Output("Label")]
        ISpread<string> FLabel;

        [Output("ID")]
        ISpread<string> FId;

        [Output("Serial")]
        ISpread<string> FSerial;

        [Output("Type")]
        ISpread<string> FType;

        [Output("Version")]
        ISpread<string> FVersion;


        //Logger
        [Import()]
        ILogger FLogger;

        //private Fields
        WrapperManager FManager = new WrapperManager();
        private bool disposed;
        #endregion fields & piins


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            try
            {
                if (FUpdate.IsChanged)
                {
                    FName.SliceCount = FManager.FPhidget.Devices.Count;
                    FLabel.SliceCount = FManager.FPhidget.Devices.Count;
                    FId.SliceCount = FManager.FPhidget.Devices.Count;
                    FSerial.SliceCount = FManager.FPhidget.Devices.Count;
                    FType.SliceCount = FManager.FPhidget.Devices.Count;
                    FVersion.SliceCount = FManager.FPhidget.Devices.Count;


                    for (int i = 0; i < FManager.FPhidget.Devices.Count; i++)
                    {
                        FName[i] = FManager.FPhidget.Devices[i].Name;
                        FLabel[i] = FManager.FPhidget.Devices[i].Label;
                        FId[i] = FManager.FPhidget.Devices[i].ID.ToString();
                        FSerial[i] = FManager.FPhidget.Devices[i].SerialNumber.ToString();
                        FType[i] = FManager.FPhidget.Devices[i].Type;
                        FVersion[i] = FManager.FPhidget.Devices[i].Version.ToString();
                    }
                }

                List<PhidgetException> Exceptions = FManager.Errors;
                if (Exceptions != null)
                {
                    foreach (Exception e in Exceptions)
                        FLogger.Log(e);
                }
            }
            catch (PhidgetException ex)
            {
                FLogger.Log(ex);
            }
        }
    }
}
