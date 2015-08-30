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
	public enum Occurence
	{
		Start, End, Both
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Clean", 
				Category = "String", 
				Version = "Advanced", 
				Help = "Removes given characters from the beginning and/or end of the string", Tags = "trim")]
	#endregion PluginInfo
	public class CharStringCleanNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultString = "")]
		public ISpread<string> FInput;
		
		[Input("Char", DefaultString = ",")]
		public ISpread<string> FChar;
		
		[Input("Occurence", DefaultEnumEntry = "Both")]
        public IDiffSpread<Occurence> FOccurence;

		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
			{
				var chars = FChar[i].ToCharArray();
				switch (FOccurence[i])
				{
					case Occurence.Start: FOutput[i] = FInput[i].TrimStart(chars); break;
					case Occurence.End: FOutput[i] = FInput[i].TrimEnd(chars); break;
					case Occurence.Both: FOutput[i] = FInput[i].Trim(chars); break;
				}
			}

			//FLogger.Log(LogType.Debug, "Logging to Renderer (TTY)");
		}
	}
}
