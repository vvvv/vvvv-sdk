//@author: motzi
//@help: creates a tube-like strip of triangles with adjustable thickness

#region usings
using System;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

//using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Rope", Category = "2D", Help = "Creates rope-like triangle strip vertices with adjustable thickness from given path coordinates", Tags = "", Author = "motzi")]
	#endregion PluginInfo
	public class Rope2d : IPluginEvaluate
	{
		#region fields & pins
		[Input("Points", DefaultValue = 1.0)]
		public ISpread<Vector2D> FInput;
		
		[Input("Thickness", DefaultValue = 0.2)]
		public ISpread<double> FWidth;
		
		[Output("Vertices")]
		public ISpread<Vector2D> FOutput;
		
		[Output("Texture Coordinates")]
		public ISpread<Vector2D> FTexCoords;
		
		[Output("Factor", Visibility = PinVisibility.OnlyInspector)]
		public ISpread<double> IFactor;

		//[Import()]
		//public ILogger FLogger;

		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			// no vertices
			if(FInput.SliceCount < 1)
			{
				return;
			}
			// Only one vertex
			else if(FInput.SliceCount == 1)
			{
				FOutput.SliceCount = FInput.SliceCount;
				FOutput[0] = FInput[0];
				//FLogger.Log(LogType.Debug, "Single");
				
				IFactor[0] = 1;
				return;
			}

			IFactor[0] = 1.0 /(FInput.SliceCount-1);
			
			FOutput.SliceCount = FInput.SliceCount * 2;
			FTexCoords.SliceCount = FInput.SliceCount * 2;
			
			Vector2D hn;
			int i, inIndex, outIndex, outTexCoord = 0;
			
			
			for (i = 1; i < FInput.SliceCount; i++)
			{
				inIndex = FInput.SliceCount - i;
				outIndex = (i-1) * 2;
				outTexCoord = i-1;
				hn = GetHalfNormal(FInput[inIndex], FInput[inIndex-1], FWidth[inIndex-1]);
				FOutput[outIndex] = FInput[inIndex] - hn;
				FTexCoords[outIndex] = new Vector2D(IFactor[0] * outTexCoord,1);
				
				FOutput[outIndex + 1] = FInput[inIndex] + hn;
				FTexCoords[outIndex + 1] = new Vector2D(IFactor[0] * outTexCoord,0);
			}
			
			// last point
			--i;
			inIndex = 1;
			outIndex = i*2;
			hn = GetHalfNormal(FInput[inIndex], FInput[inIndex-1], FWidth[inIndex-1]);
			FOutput[outIndex] = FInput[inIndex - 1] - hn;
			FTexCoords[outIndex] = new Vector2D(1,1);
			
			FOutput[outIndex+1] = FInput[inIndex - 1] + hn;
			FTexCoords[outIndex+1] = new Vector2D(1,0);
		}
			
		
		
		private Vector2D GetHalfNormal(Vector2D vertexOne, Vector2D vertexTwo, double width)
		{
			Vector2D halfnormal = vertexTwo - vertexOne;
			halfnormal = ~halfnormal;
			halfnormal = new Vector2D(-halfnormal.y  * width * 0.5, halfnormal.x * width * 0.5);
			
			return halfnormal;
		}
	}
}
