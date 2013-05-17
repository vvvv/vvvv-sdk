// standard values of eyetracker device
// 172.68.195.1
// Portnumber 4455
// syncport 5547

#region usings

using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using Tobii.Eyetracking.Sdk;
using Tobii.Eyetracking.Sdk.Exceptions;

#endregion usings


namespace TobiiEyetracker
{
    #region PluginInfo
    [PluginInfo(Name = "Headbox", Category = "Devices", Version = "TobiiEyetracker", Help = "Headbox of a Tobii Eyetracker", Tags = "", Author = "niggos, phlegma")]
    #endregion PluginInfo

    public class HeadboxNode : IPluginEvaluate
    {
        #region fields & pins

        #region Input
        [Input("Device", IsSingle = true)]
        IDiffSpread<IEyetracker> FEyetrackerIn;

        [Input("Enable", IsToggle = true, DefaultValue = 0, IsSingle = true)]
        IDiffSpread<bool> FEnable;

        #endregion Input

        #region Output

        [Output("Output")]
        ISpread<Vector3D> FPointsOut;

        #endregion Output

        [Import()]
        ILogger FLogger;

        private IEyetracker FEyetracker;
        private bool FConnected = false;
        private bool Finit = true;

        #endregion fields & pins

        // called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (Finit)
                FEyetrackerIn.Changed += new SpreadChangedEventHander<IEyetracker>(FEyetrackerIn_Changed);

            if (FEyetracker != null)
            {
                if ((FEnable.IsChanged || FConnected) && FEnable[0] == true)
                {
                    FPointsOut.SliceCount = 8;
                    HeadMovementBox Box = FEyetracker.GetHeadMovementBox();
                    FPointsOut[0] = new Vector3D(Box.Point1.X, Box.Point1.Y, Box.Point1.Z);
                    FPointsOut[1] = new Vector3D(Box.Point2.X, Box.Point2.Y, Box.Point2.Z);
                    FPointsOut[2] = new Vector3D(Box.Point3.X, Box.Point3.Y, Box.Point3.Z);
                    FPointsOut[3] = new Vector3D(Box.Point4.X, Box.Point4.Y, Box.Point4.Z);
                    FPointsOut[4] = new Vector3D(Box.Point5.X, Box.Point5.Y, Box.Point5.Z);
                    FPointsOut[5] = new Vector3D(Box.Point6.X, Box.Point6.Y, Box.Point6.Z);
                    FPointsOut[6] = new Vector3D(Box.Point7.X, Box.Point7.Y, Box.Point7.Z);
                    FPointsOut[7] = new Vector3D(Box.Point8.X, Box.Point8.Y, Box.Point8.Z);
                }
                else if (FEnable.IsChanged && FEnable[0] == false)
                {
                    FPointsOut.SliceCount = 0;
                }
            }
            FConnected = false;
            Finit = false;
        }

        void FEyetracker_HeadMovementBoxChanged(object sender, HeadMovementBoxChangedEventArgs e)
        {
            FConnected = true;
        }

        void FEyetrackerIn_Changed(IDiffSpread<IEyetracker> spread)
        {
            if (spread[0] == null)
            {
                if (FEyetracker != null)
                {
                    FEyetracker.Dispose();
                    FEyetracker = null;
                }

                FPointsOut.SliceCount = 0;
            }
            else
            {
                FEyetracker = spread[0];
                FConnected = true;
            }
        }
    }
}


