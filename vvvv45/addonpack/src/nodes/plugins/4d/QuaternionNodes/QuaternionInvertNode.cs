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
	#region PluginInfo
	[PluginInfo(Name = "Invert", Category = "Quaternion", Help = "", Tags = "",Author="vux")]
	#endregion PluginInfo
	public class QuaternionInvertNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValues = new double[] { 0,0,0,1})]
		ISpread<Quaternion> FInput;

		[Output("Output")]
		ISpread<Quaternion> FOutput;
		#endregion fields & pins

		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
			{
				Quaternion q = FInput[i];
				q.Invert();
				FOutput[i] = q;
			}
		}
	}
}
