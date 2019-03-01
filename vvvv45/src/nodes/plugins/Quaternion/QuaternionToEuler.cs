using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Euler",
    Category = "Quaternion",
    Version = "Get Vector",
    Help = "Returns the euler angles of a quaternion",
    Tags = "rotation, split")]
    #endregion PluginInfo
    public class QuaternionToEulerVectorNode : IPluginEvaluate
    {
        #region fields & pins
#pragma warning disable 0649
        [Input("Quaternion ", DefaultValues = new[] { 0.0, 0.0, 0.0, 1.0 })]
        IDiffSpread<Vector4D> FInput;

        [Output("Rotation ")]
        ISpread<Vector3D> FAnglesOut;
        #endregion

        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                double pitch;
                double yaw;
                double roll;

                VMath.QuaternionToEulerYawPitchRoll(FInput[i], out pitch, out yaw, out roll);

                FAnglesOut[i] = new Vector3D(pitch * VMath.RadToCyc, yaw * VMath.RadToCyc, roll * VMath.RadToCyc);
            }
        }
#pragma warning restore

        #region PluginInfo
        [PluginInfo(Name = "Euler",
        Category = "Quaternion",
        Version = "Get",
        Help = "Returns the euler angles of a quaternion",
        Tags = "rotation, split")]
        #endregion PluginInfo
        public class QuaternionToEulerNode : IPluginEvaluate
        {
            #region fields & pins
#pragma warning disable 0649
            [Input("Quaternion ", DefaultValues = new[] { 0.0, 0.0, 0.0, 1.0 })]
            IDiffSpread<Vector4D> FInput;

            [Output("Pitch")]
            ISpread<double> FPitchOut;

            [Output("Yaw")]
            ISpread<double> FYawOut;

            [Output("Roll")]
            ISpread<double> FRollOut;
            #endregion

            public void Evaluate(int SpreadMax)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    double pitch;
                    double yaw;
                    double roll;

                    VMath.QuaternionToEulerYawPitchRoll(FInput[i], out pitch, out yaw, out roll);

                    FPitchOut[i] = pitch * VMath.RadToCyc;
                    FYawOut[i] = yaw * VMath.RadToCyc;
                    FRollOut[i] = roll * VMath.RadToCyc;
                }

#pragma warning restore


            }
        }

    }
}