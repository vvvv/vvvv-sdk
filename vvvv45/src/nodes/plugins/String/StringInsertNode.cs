#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Insert", 
				Category = "String", 
				Help = "Inserts a specified string at a specified index position in the input string.")]
	#endregion PluginInfo
	public class StringInsertNode: IPluginEvaluate
	{
		#region fields & pins
#pragma warning disable 0649
        [Input("Input")]
        ISpread<string> FInput;

        [Input("Position", MinValue = 0)]
        ISpread<int> FPosition;

        [Input("Text to Insert")]
        ISpread<string> FInsert;

        [Output("Output")]
        ISpread<string> FOutput;
#pragma warning restore
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
				FOutput[i] = FInput[i].Insert(FPosition[i] % (FInput[i].Length + 1), FInsert[i]);
		}
	}
}
