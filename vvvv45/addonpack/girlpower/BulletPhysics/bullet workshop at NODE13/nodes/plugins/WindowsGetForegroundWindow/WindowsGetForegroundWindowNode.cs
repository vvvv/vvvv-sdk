#region usings
using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.Collections;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "GetForegroundWindow", Category = "Windows", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class WindowsGetForegroundWindowNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Update", DefaultValue = 0)]
		ISpread<bool> FUpdate;

		[Output("Handle Out")]
		ISpread<int> FOutput;
		[Output("Title")]
		ISpread<string> FTitle;
		#endregion fields & pins

		[DllImport("C:\\Windows\\System32\\user32.dll")]
		public static extern IntPtr GetForegroundWindow();
		
		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			FTitle.SliceCount = SpreadMax;
			const int nChars = 256;
			IntPtr handle = IntPtr.Zero;
			if(FUpdate[0]) {
				for (int i = 0; i < SpreadMax; i++) {
					handle = GetForegroundWindow();
					StringBuilder Buff = new StringBuilder(nChars);
					FOutput[i] = handle.ToInt32();
					if(GetWindowText(handle, Buff, nChars) > 0) FTitle[i] = Buff.ToString();
				}	
			}
		}
	}
}
