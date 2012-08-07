#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ApplyModifiers", 
	            Category = "String",
	            Help = "Returns the actual string representation of a given keyboardstate.",
	            AutoEvaluate = true,
				Tags = "keyboard, convert")]
	#endregion PluginInfo
	public class ApplyModifiersNode: IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IDiffSpread<KeyboardState> FInput;

		[Output("Output")]
		ISpread<string> FOutput;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			
			if (FInput[0] != null && FInput.IsChanged)
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FInput[i].KeyCodes.Count > 0)
					{
						FOutput[i] = string.Join(string.Empty, FInput[i].KeyChars);
					}
					else
						FOutput[i] = "";
				}
		}
	}
}
