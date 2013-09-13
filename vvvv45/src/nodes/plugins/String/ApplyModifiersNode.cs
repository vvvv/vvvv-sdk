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
#pragma warning disable 0649
        [Input("Input")]
        IDiffSpread<KeyboardState> FInput;

        [Output("Output")]
        ISpread<string> FOutput; 
#pragma warning restore
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            if (!FInput.IsChanged) return;

			FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var input = FInput[i];
                if (input != null)
                {
                    FOutput[i] = string.Join(string.Empty, input.KeyChars);
                }
                else
                    FOutput[i] = string.Empty;
            }
		}
	}
}
