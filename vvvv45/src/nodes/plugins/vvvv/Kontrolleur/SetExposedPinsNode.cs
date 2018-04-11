#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SetExposedPin", 
				Category = "VVVV", 
				Help = "Allows writing data directly to exposed pins.", 
				Tags = "",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class SetExposedPinNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		[Input("Address")]
		public ISpread<string> FAddress;
		
		[Input("Spread")]
		public ISpread<string> FInput;

        [Input("Enabled")]
        public ISpread<bool> FEnabled;

        [Import()] 
		#pragma warning restore
		IHDEHost FHDEHost;
		
		private bool FDisposed;
		private Dictionary<string, IPin2> FExposedPins = new Dictionary<string, IPin2>();
		#endregion fields & pins
		
		#region constructor/destructor
		[ImportingConstructor]
		public SetExposedPinNode(IHDEHost host)
		{
			FHDEHost = host;
			FHDEHost.ExposedNodeService.NodeAdded += NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved += NodeRemovedCB;
			
			//get initial list of exposed ioboxes
			foreach (var node in FHDEHost.ExposedNodeService.Nodes)
			{
				var pinName = PinNameFromNode(node);
				var pin = node.FindPin(pinName);
				FExposedPins.Add(node.GetNodePath(false) + "/" + pinName, pin);
			}
		}
		
		~SetExposedPinNode()
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
					FHDEHost.ExposedNodeService.NodeAdded -= NodeAddedCB;
					FHDEHost.ExposedNodeService.NodeRemoved -= NodeRemovedCB;

                    FExposedPins.Clear();
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
			}
			FDisposed = true;
		}
		#endregion
		
		private string PinNameFromNode(INode2 node)
		{
			string pinName = "";
			if (node.NodeInfo.Systemname == "IOBox (Value Advanced)")
				pinName = "Y Input Value";
			else if (node.NodeInfo.Systemname == "IOBox (String)")
				pinName = "Input String";
			else if (node.NodeInfo.Systemname == "IOBox (Color)")
				pinName = "Color Input";
			else if (node.NodeInfo.Systemname == "IOBox (Enumerations)")
				pinName = "Input Enum";
			else if (node.NodeInfo.Systemname == "IOBox (Node)")
				pinName = "Input Node";
			
			return pinName;
		}
		
		private void NodeAddedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			
			FExposedPins.Add(node.GetNodePath(false) + "/" + pinName, pin);
		}
		
		private void NodeRemovedCB(INode2 node)
		{
			var pinName = PinNameFromNode(node);
			var pin = node.FindPin(pinName);
			
			FExposedPins.Remove(node.GetNodePath(false) + "/" + pinName);
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			for (int i = 0; i < SpreadMax; i++)
				if (FEnabled[i] && FExposedPins.ContainsKey(FAddress[i]))
				{
					var pin = FExposedPins[FAddress[i]];
					pin.Spread = FInput[i];
				}	
		}
	}
}
