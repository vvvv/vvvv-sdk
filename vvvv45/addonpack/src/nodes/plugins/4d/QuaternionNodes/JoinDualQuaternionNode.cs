#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using SlimDX;
#endregion usings

namespace VVVV.Nodes
{
    [PluginInfo(Name = "DualQuaternion", Category = "Quaternion", Version = "Join", 
        Help = "Transforms a Translate/Rotate into a dual quaternion representation", Tags = "3d,skinning",
        Author="vux")]
    public class JoinDualQuaternionNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Translate", DefaultValue = 0.0)]
        ISpread<Vector3> FTranslate;

        [Input("Rotate", DefaultValues = new double[] { 0.0, 0, 0, 1 })]
        ISpread<Quaternion> FRotate;

        [Output("Output 1")]
        ISpread<Quaternion> FOutput1;

        [Output("Output 2")]
        ISpread<Quaternion> FOutput2;
        #endregion fields & pins

        public void Evaluate(int SpreadMax)
        {
            FOutput1.SliceCount = SpreadMax;
            FOutput2.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                Quaternion r1 = FRotate[i];
                FOutput1[i] = r1;

                Vector3 tr = FTranslate[i];
                Quaternion quat = FRotate[i];

                Quaternion r2 = new Quaternion();
                r2.X = 0.5f * (tr.X * quat.W + tr.Y * quat.Z - tr.Z * quat.Y);
                r2.Y = 0.5f * (-tr.X * quat.Z + tr.Y * quat.W + tr.Z * quat.X);
                r2.Z = 0.5f * (tr.X * quat.Y - tr.Y * quat.X + tr.Z * quat.W);
                r2.W = -0.5f * (tr.X * quat.X + tr.Y * quat.Y + tr.Z * quat.Z);

                FOutput2[i] = r2;

            }
        }
    }
}
