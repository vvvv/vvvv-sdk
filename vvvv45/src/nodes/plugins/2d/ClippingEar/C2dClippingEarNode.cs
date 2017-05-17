#region usings
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using Triangulation;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ClippingEar", 
				Category = "2d", 
				Help = "Simple Polygon Triangulation based on the clipping ear algorithm", 
				Tags = "triangulate",
				Credits = " Bill Overman, https://polygontriangulation.codeplex.com")]
	#endregion PluginInfo
	public class C2dClippingEarNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<ISpread<Vector2D>> FInput;
		
		[Input("Enabled", DefaultValue=1)]
		public ISpread<bool> FEnabled;

		[Output("Output")]
		public ISpread<ISpread<Vector2D>> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			int i = 0;
			FOutput.SliceCount = FInput.SliceCount;
			foreach (var input in FInput)
			{
				if (FEnabled[i])
				{
					if (input.Any())
					{
						var polygon = new Polygon(input.ToList());
		            	var triangles = Triangulation2D.Triangulate(polygon);
						FOutput[i].AssignFrom(triangles.SelectMany(t => t));
					}
				}
				i++;
			}

			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
