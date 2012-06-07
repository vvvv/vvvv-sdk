#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "ActivePatch", 
				Category = "VVVV", 
				Help = "Returns the filename of the last active patch",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class VVVVActivePatchNode: IPluginEvaluate, IDisposable
	{
		#region fields & pins
		[Output("Path")]
		ISpread<string> FOutput;
		
		[Output("Contains Problem")]
		ISpread<bool> FContainsProblem;
		
		[Output("Contains Missing Nodes")]
		ISpread<bool> FContainsMissing;
		
		[Output("Is Boygrouped")]
		ISpread<bool> FIsBoygrouped;
		
		[Output("Contains Boygrouped Nodes")]
		ISpread<bool> FContainsBoygrouped;
		
		[Output("Contains Exposed Nodes")]
		ISpread<bool> FContainsExposed;
		
		private IHDEHost FHDEHost;
		private INode2 FActivePatch;
		
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		#endregion fields & pins

		#region constructor/destructor
		[ImportingConstructor]
		public VVVVActivePatchNode(IHDEHost host)
		{
			FHDEHost = host;
			FHDEHost.WindowSelectionChanged += WindowSelectionChangedCB;
			
			FActivePatch = FHDEHost.ActivePatchWindow.Node;
		}
		
		~VVVVActivePatchNode()
		{
			Dispose(false);
		}
		
		public void Dispose()
		{
			Dispose(true);
		}
		
		protected void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
					FHDEHost.WindowSelectionChanged -= WindowSelectionChangedCB;
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
			}
			FDisposed = true;
		}
		#endregion constructor/destructor
		
		private void WindowSelectionChangedCB(Object sender, WindowEventArgs args)
		{
			if (args.Window.WindowType == WindowType.Patch)
				FActivePatch = args.Window.Node;
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput[0] = FActivePatch.NodeInfo.Filename;
			
			FContainsProblem[0] = FActivePatch.ContainsProblem();
			FContainsMissing[0] = FActivePatch.ContainsMissingNodes();
			FIsBoygrouped[0] = FActivePatch.IsBoygrouped();
			FContainsBoygrouped[0] = FActivePatch.ContainsBoygroupedNodes();
			FContainsExposed[0] = FActivePatch.ContainsExposedNodes();
		}
	}
}
