#region usings
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Hosting
{
	public class PatchRefactorer
	{
		#region fields
		[Import()]
		ILogger FLogger;
		
		INodeInfoFactory FNodeInfoFactory;
		
		private List<string> FInputNames = new List<string>();
		private List<string> FOutputNames = new List<string>();
		#endregion fields & pins
		
		public PatchRefactorer(IHDEHost hdeHost, INode2[] selectedNodes, INodeBrowserHost nodeBrowserHost, INodeInfoFactory nodeInfoFactory)
		{
			FNodeInfoFactory = nodeInfoFactory;
			
			int x = 0;
			int y = 0;
			int maxX = 0;
			int maxY = 0;
			
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			
			//check selection bounds
			foreach (var node in selectedNodes)
			{
				var bounds = node.GetBounds(BoundsType.Node);
				x += bounds.X;
				y += bounds.Y;
				
				maxX = Math.Max(maxX, bounds.X + bounds.Width);
				maxY = Math.Max(maxY, bounds.Y + bounds.Height);
				
				minX = Math.Min(minX, bounds.X);
				minY = Math.Min(minY, bounds.Y);
			}
			
			var border = 1500;
			int pinOffset = border / 3;
			x /= selectedNodes.Length;
			y /= selectedNodes.Length;
			var selectionCenter = new Point (x, y);
			var selectionSize = new Size((maxX - minX) + border * 2, (maxY - minY) + border * 2);
			
			var patchPath = Path.GetDirectoryName(hdeHost.ActivePatchWindow.Node.NodeInfo.Filename);
			var patchName = GetUniquePatchName(hdeHost.ActivePatchWindow.Node.NodeInfo.Filename);
			
			patchPath = Path.Combine(patchPath, patchName) + ".v4p";
			
			var ni = FNodeInfoFactory.CreateNodeInfo(patchName, "", "", patchPath, true);
			ni.InitialComponentMode = TComponentMode.Hidden;
			ni.Type = NodeType.Patch;
			ni.CommitUpdate();
			
			var subID = nodeBrowserHost.CreateNode(ni, selectionCenter);
			var patch = new PatchMessage(patchPath);
			
			var newNodeID = 0;
			var oldIDToNewID = new Dictionary<int, int>();
			
			//take over all selected nodes to the subpatch
			foreach (var node in selectedNodes)
			{
				var nodeMessage = patch.AddNode(node.NodeInfo.Systemname);
				
				if (node.Name.StartsWith("IOBox ("))
				{
					var bounds = node.GetBounds(BoundsType.Box);
					nodeMessage.BoxBounds = new Rectangle(bounds.X - minX + border, bounds.Y - minY + border, bounds.Width, bounds.Height);
					nodeMessage.NodeBounds = nodeMessage.BoxBounds;
				}
				else if (node.HasPatch || node.HasGUI)
				{
					var bounds = node.GetBounds(BoundsType.Node);
					nodeMessage.NodeBounds = new Rectangle(bounds.X - minX + border, bounds.Y - minY + border, bounds.Width, bounds.Height);
					bounds = node.GetBounds(BoundsType.Box);
					nodeMessage.BoxBounds = new Rectangle(bounds.X - minX + border, bounds.Y - minY + border, bounds.Width, bounds.Height);
					bounds = node.GetBounds(BoundsType.Window);
					nodeMessage.WindowBounds = new Rectangle(bounds.X - minX + border, bounds.Y - minY + border, bounds.Width, bounds.Height);
					
					//nodeMessage.ComponentMode = node.Window
				}
				else
				{
					var bounds = node.GetBounds(BoundsType.Node);
					nodeMessage.NodeBounds = new Rectangle(bounds.X - minX + border, bounds.Y - minY + border, bounds.Width, bounds.Height);
				}
				
				foreach (var pin in node.Pins)
				{
					if (!pin.IsConnected())
					{
						var def = pin.SubType.Split(',');
						//only write pinmessage if value != default
						
						if ((pin.Type == "Value" && pin.Spread != def[2].Trim())
						    || (pin.Type == "String" && pin.Spread != def[1].Trim())
						    || (pin.Type == "Enumeration" && pin.Spread != def[1].Trim())
						    || (pin.Type == "Color"))
						{
							var p = nodeMessage.AddPin(pin.NameByParent(node));
							p.AddAttribute("values", pin.Spread);
						}
					}
				}
				
				nodeMessage.ID = newNodeID;
				oldIDToNewID.Add(node.ID, newNodeID);
				newNodeID++;
			}
			
			var IOpins = new Dictionary<string, int>();
			var minInputX = 0;
			var minOutputX = 0;
			
			FInputNames.Clear();
			FOutputNames.Clear();
			
			//now sort the list of nodes in X from left to right
			//in order to get IOBoxes of leftmost nodes also leftmost
			var nodes = (from n in selectedNodes orderby n.GetBounds(BoundsType.Node).X select n).ToList();
			var oldPinToNewPin = new Dictionary<string, string>();
			
			//make connections in the selection
			//for each selected nodes input pin...
			foreach (var node in nodes)
				foreach (var pin in node.Pins)
					foreach (var cpin in pin.ConnectedPins)
						//..if there is a connection to another selected node
						if (oldIDToNewID.ContainsKey(cpin.ParentNodeByPatch(node.Parent).ID))
			{
				//this needs only be done for inputs
				if (pin.Direction == PinDirection.Input)
				{
					var parent = cpin.ParentNodeByPatch(node.Parent);
					patch.AddLink(oldIDToNewID[parent.ID], cpin.NameByParent(parent), oldIDToNewID[pin.ParentNodeByPatch(node.Parent).ID], pin.NameByParent(node));
				}
			}
			//..if there is a connection to a not selected node
			else
			{
				//an IO pin needs to be created
				//if it doesn't exist yet
				//(multiple inputs may connect to an upstream pin and an IO pin may alread exist now)
				string ident = "";
				if (pin.Direction == PinDirection.Input)
				{
					var parent = cpin.ParentNodeByPatch(node.Parent);
					ident = parent.ID.ToString() + cpin.NameByParent(parent);
				}
				else if (pin.Direction == PinDirection.Output)
					ident = node.ID.ToString() + pin.NameByParent(node);
				
//				FLogger.Log(LogType.Debug, ident);
				
				if (!IOpins.ContainsKey(ident))
				{
					var pinType = pin.Type;
					//create an iobox of the right type
					var iobox = CreateIOBox(patch, pinType);
					
					iobox.ID = newNodeID;
					var bounds = node.GetBounds(BoundsType.Node);
					
					//name the iobox
					var labelPin = iobox.AddPin("Descriptive Name");
					
					if (pin.Direction == PinDirection.Input)
					{
						iobox.BoxBounds = new Rectangle(Math.Max(minInputX, bounds.X - minX + border), pinOffset, 750, 225);
						
						//an input-pin may be connected to an output-pin
						//that in turn is connected to multiple inputs
						//in those cases name the iobox by concatenating the names of all those pins
						//but leave out duplicates
						var pinName = GetNameForInput(node.Parent, cpin);
//						FLogger.Log(LogType.Debug, pinName);
						pinName = GetUniqueInputName(pinName);
						oldPinToNewPin.Add(ident, pinName);
						labelPin.AddAttribute("values", "|" + pinName + "|");
						
						//save it for reference
						var parent = cpin.ParentNodeByPatch(node.Parent);
						IOpins.Add(parent.ID.ToString() + cpin.NameByParent(parent), newNodeID);
						var ioboxOutput = GetIOBoxPinName(pinType, false);
						patch.AddLink(newNodeID, ioboxOutput, oldIDToNewID[pin.ParentNodeByPatch(node.Parent).ID], pin.NameByParent(node));
						
						minInputX = iobox.BoxBounds.X + iobox.BoxBounds.Width + 150;
					}
					else if (pin.Direction == PinDirection.Output)
					{
						iobox.BoxBounds = new Rectangle(Math.Max(minOutputX, bounds.X - minX + border), (maxY  -minY) + pinOffset + border, 750, 225);
						var origName = pin.NameByParent(node);
						var pinName = GetUniqueOutputName(origName);
						oldPinToNewPin.Add(ident, pinName);
						labelPin.AddAttribute("values", "|" + pinName + "|");
						
						//save it for reference
						IOpins.Add(pin.ParentNodeByPatch(node.Parent).ID.ToString() + origName, newNodeID);
						var ioboxInput = GetIOBoxPinName(pinType, true);
						patch.AddLink(oldIDToNewID[node.ID], origName, newNodeID, ioboxInput);
						
						minOutputX = iobox.BoxBounds.X + iobox.BoxBounds.Width + 150;
					}
					
					iobox.NodeBounds = iobox.BoxBounds;
					newNodeID++;
				}
				else //IOpin already exists
				{
					var srcID = IOpins[ident];
					//this needs only be done for inputs
					if (pin.Direction == PinDirection.Input)
						patch.AddLink(srcID, GetIOBoxPinName(cpin.Type, false), oldIDToNewID[pin.ParentNodeByPatch(node.Parent).ID], pin.NameByParent(node));
				}
			}
			
			hdeHost.SendPatchMessage(patchPath, patch.ToString(), true);

			//make connections to new subpatch
			patch = new PatchMessage("");
			
			foreach (var node in selectedNodes)
				foreach (var pin in node.Pins)
					foreach (var cpin in pin.ConnectedPins)
						//..if there is a connection to a not selected node
						if (!oldIDToNewID.ContainsKey(cpin.ParentNodeByPatch(node.Parent).ID))
							if (pin.Direction == PinDirection.Input)
			{
				var parent = cpin.ParentNodeByPatch(node.Parent);
				patch.AddLink(parent.ID, cpin.NameByParent(parent), subID, oldPinToNewPin[parent.ID.ToString() + cpin.NameByParent(parent)]);
			}
			else if (pin.Direction == PinDirection.Output)
			{
				var parent = cpin.ParentNodeByPatch(node.Parent);
				patch.AddLink(subID, oldPinToNewPin[pin.ParentNodeByPatch(node.Parent).ID.ToString() + pin.NameByParent(node)], parent.ID, cpin.NameByParent(parent));
			}
			
			//..and remove selected nodes
			foreach (var node in selectedNodes)
			{
				var nodeMessage = patch.AddNode(node.ID);
				nodeMessage.DeleteMe = true;
			}
			
			var nodeMsg = patch.AddNode(subID);
			nodeMsg.WindowBounds = new Rectangle(300 + selectionCenter.X + hdeHost.ActivePatchWindow.Bounds.X * 15, 300 + selectionCenter.Y + hdeHost.ActivePatchWindow.Bounds.Y * 15, selectionSize.Width, selectionSize.Height);
			nodeMsg.BoxBounds = new Rectangle(selectionCenter.X - selectionSize.Width / 2, selectionCenter.Y - selectionSize.Height / 2, selectionSize.Width, selectionSize.Height);
			
			hdeHost.SendPatchMessage(hdeHost.ActivePatchWindow.Node.NodeInfo.Filename, patch.ToString(), true);
		}
		//FLogger.Log(LogType.Debug, "hi tty!");
		
		private string GetIOBoxPinName(string pinType, bool input)
		{
			if (pinType == "String")
			{	if (input)
					return "Input String";
				else
					return "Output String";}
			else if (pinType == "Value")
			{	if (input)
					return "Y Input Value";
				else
					return "Y Output Value";}
			else if (pinType == "Color")
			{	if (input)
					return "Color Input";
				else
					return "Color Output";}
			else if (pinType == "Enumeration")
			{	if (input)
					return "Input Enum";
				else
					return "Output Enum";}
			else //assume node
			{	if (input)
					return "Input Node";
				else
					return "Output Node";}
		}
		
		private NodeMessage CreateIOBox(PatchMessage patch, string pinType)
		{
			if (pinType == "String")
				return patch.AddNode("IOBox (String)");
			else if (pinType == "Value")
				return patch.AddNode("IOBox (Value Advanced)");
			else if (pinType == "Color")
				return patch.AddNode("IOBox (Color)");
			else if (pinType == "Enumeration")
				return patch.AddNode("IOBox (Enumerations)");
			else //assume node
				return patch.AddNode("IOBox (Node)");
		}
		
		private string GetUniqueInputName(string pinName)
		{
			if (FInputNames.Contains(pinName))
			{
				var id = 1;
				while (FInputNames.Contains(pinName + " " + id.ToString()))
					id++;
				
				pinName += " " + id.ToString();
			}
			
			FInputNames.Add(pinName);
			return pinName;
		}
		
		private string GetUniqueOutputName(string pinName)
		{
			if (FOutputNames.Contains(pinName))
			{
				var id = 1;
				while (FOutputNames.Contains(pinName + " " + id.ToString()))
					id++;
				
				pinName += " " + id.ToString();
			}
			
			FOutputNames.Add(pinName);
			return pinName;
		}
		
		private string GetUniquePatchName(string patchPath)
		{
			var path = Path.GetDirectoryName(patchPath);
			var filename = Path.GetFileNameWithoutExtension(patchPath);
			var id = 1;
			var uniqueName = "";
			var uniquePath = "";
			
			do
			{
				uniqueName = filename + "-" + id.ToString();
				uniquePath = Path.Combine(path, uniqueName) + ".v4p";
				id++;
			}
			while (FNodeInfoFactory.ContainsKey(uniqueName, "", "", uniquePath));
			
			return uniqueName;
		}
		
		private string GetNameForInput(INode2 patch, IPin2 pin)
		{
			//concatenate the names of all pins connected to this output
			var names = new List<string>();
			foreach(var cpin in pin.ConnectedPins)
			{
				var parent = cpin.ParentNodeByPatch(patch);
				var pinName = cpin.NameByParent(parent);
				if (!names.Contains(pinName))
					names.Add(pinName);
			}
			
			return names.Aggregate((i, j) => i + " - " + j);
		}
	}
}