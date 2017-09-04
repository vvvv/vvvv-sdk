#region usings
using System;
using System.ComponentModel.Composition;

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
	[PluginInfo(Name = "TestMouseAndKeyboard", Category = "Test", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class TestTestMouseAndKeyboardNode : IPluginEvaluate
	{
		[Input("Mouse State")]
		public IDiffSpread<MouseState> FMouseIn;
		
		[Input("Keyboard State")]
		public IDiffSpread<KeyboardState> FKeyboardIn;

		[Output("Mouse Is Changed")]
		public ISpread<bool> FMouseChangedOut;
		
		[Output("Keyboard Is Changed")]
		public ISpread<bool> FKeyboardChangedOut;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FMouseChangedOut[0] = FMouseIn.IsChanged;
			FKeyboardChangedOut[0] = FKeyboardIn.IsChanged;
		}
	}
}
