#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Fly", Category = "Sandbox", 
	Tags = "demo, filter, transform", AutoEvaluate = true)]

	public class Fly : IPluginEvaluate
	{
		[Input("Transform In")]
		ISpread<Matrix4x4> FTransformIn;

		[Input("Start", IsSingle = true)]
		ISpread<bool> FStart;

		[Output("Position")]
		ISpread<Vector3D> FOutput;

		[Output("Velocity")]
		ISpread<Vector3D> FFrameVelocity;

		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			FFrameVelocity.SliceCount = SpreadMax;
            
            // when flying we just increment the 
            // ouput position by last captured frame velocity 
			if (FStart[0]) 
				for (int i = 0; i < SpreadMax; i++) 
					FOutput[i] += FFrameVelocity[i];
				
	        // otherwise just update output according to transform in
	        // and compute how much the output changed in the last frame
			else 
				for (int i = 0; i < SpreadMax; i++) 
				{
	                // get new transform			
					var matrix = FTransformIn[i];

					// get old position
					var oldOutput = FOutput[i];
					
					// set new position
					FOutput[i] = matrix.row4.xyz;

					// get difference per frame = frame velocity
					FFrameVelocity[i] = FOutput[i] - oldOutput;
				}
		}
	}
}