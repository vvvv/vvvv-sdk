#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using TriangleNet.Meshing;
using TriangleNet.Geometry;

using maintest;



#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "PolyMap", Category = "2d", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class C2dPolyMapNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;
		
		[Input("Triangles", DefaultValue = 1.0)]
		public ISpread<ITriangle> FTriangle;

		[Output("Output")]
		public ISpread<double> FOutput;
		
		[Output("Output2")]
		public ISpread<int> FOutput2;

		[Import()]
		public ILogger FLogger;
        #endregion fields & pins

        #region dllimport



        #endregion dllimport

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			FOutput2.SliceCount = SpreadMax;
			

//			int test =0;
			
			FinanTestce instance = new FinanTestce();
			
			FOutput2[0]=instance.ComputeBBW(2.0,4);
            

			for (int i = 0; i < SpreadMax; i++){
				
			}
//				FOutput[i] = FInput[i] * 2;

			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
