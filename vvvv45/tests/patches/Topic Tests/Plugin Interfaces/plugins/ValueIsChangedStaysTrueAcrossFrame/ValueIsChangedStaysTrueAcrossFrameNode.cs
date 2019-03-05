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
	[PluginInfo(Name = "IsChangedStaysTrueAcrossFrame", Category = "Value")]
	#endregion PluginInfo
	public class ValueIsChangedStaysTrueAcrossFrameNode : IPluginEvaluate, 
		IPartImportsSatisfiedNotification, IDisposable
	{
		[Import]
		public IMainLoop MainLoop;
		
		[Input("Input")]
		public IDiffSpread<double> DiffInput;

		[Output("Output")]
		public ISpread<bool> EvaluateIsChangedOut;
		
		[Output("Render")]
		public ISpread<bool> RenderIsChangedOut;
		
		public void OnImportsSatisfied()
		{
			MainLoop.OnRender += HandleOnRender;
		}
		
		public void Dispose()
		{
			MainLoop.OnRender -= HandleOnRender;
		}
		
		private void HandleOnRender(object sender, EventArgs args)
		{
			RenderIsChangedOut[0] = DiffInput.IsChanged;
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			EvaluateIsChangedOut[0] = DiffInput.IsChanged;
		}
	}
}
