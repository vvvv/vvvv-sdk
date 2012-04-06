#region usings
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Security;

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
		ILogger FLogger;
		
		INodeInfoFactory FNodeInfoFactory;
		private XmlDocument FDocument;
		
		private Dictionary<int, int> FOldID2NewID = new Dictionary<int, int>();
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
			
			var CBorder = 1500;
			int CPinOffset = CBorder / 3;
			x /= selectedNodes.Length;
			y /= selectedNodes.Length;
			var selectionCenter = new Point (x, y);
			var selectionSize = new Size((maxX - minX) + CBorder * 2, (maxY - minY) + CBorder * 2);
			
			//create new nodinfo for subpatch
			var patchPath = Path.GetDirectoryName(hdeHost.ActivePatchWindow.Node.NodeInfo.Filename);
			var patchName = GetUniquePatchName(hdeHost.ActivePatchWindow.Node.NodeInfo.Filename);
			
			patchPath = Path.Combine(patchPath, patchName) + ".v4p";
			
			var ni = FNodeInfoFactory.CreateNodeInfo(patchName, "", "", patchPath, true);
			ni.InitialComponentMode = TComponentMode.Hidden;
			ni.Type = NodeType.Patch;
			ni.CommitUpdate();
			
			//modify the current selection XML
			FDocument = new XmlDocument();
			FDocument.LoadXml(hdeHost.GetXMLSnippetFromSelection());
			
			//create new subpatch
			var subID = nodeBrowserHost.CreateNode(ni, selectionCenter);
			var patch = new PatchMessage(patchPath);

			//in the new subpatch nodes will have new IDs and bounds
			FOldID2NewID.Clear();
			var newNodeID = 0;
			var origNodes = FDocument.SelectNodes("/PATCH/NODE");
			foreach (XmlNode node in origNodes)
			{
				//modify the ID
				var idAttribute = node.Attributes.GetNamedItem("id");
				var oldID = int.Parse(idAttribute.Value);
				idAttribute.Value = newNodeID.ToString();
				FOldID2NewID.Add(oldID, newNodeID);
				newNodeID++;
				
				//modify the bounds
				var bounds = node.SelectNodes("BOUNDS");
				foreach (XmlElement bound in bounds)
				{
					if ((bound.GetAttribute("type") == "Node") || (bound.GetAttribute("type") == "Box"))
					{
						var top = int.Parse(bound.GetAttribute("top"));
						var left = int.Parse(bound.GetAttribute("left"));
						
						bound.SetAttribute("top", (top - minY + CBorder).ToString());
						bound.SetAttribute("left", (left - minX + CBorder).ToString());
					}
				}
			}
			
			//offset linkpoints
			var origLinks = FDocument.SelectNodes("/PATCH/LINK");
			foreach (XmlElement link in origLinks)
				foreach (XmlElement point in link)
				{
					var px = int.Parse(point.GetAttribute("x"));
					var py = int.Parse(point.GetAttribute("y"));
					
					point.SetAttribute("x", (px - minX + CBorder).ToString());
					point.SetAttribute("y", (py - minY + CBorder).ToString());	
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
						if (FOldID2NewID.ContainsKey(cpin.ParentNodeByPatch(node.Parent).ID))
			{
				//this needs only be done for inputs
				if (pin.Direction == PinDirection.Input)
				{
					var parent = cpin.ParentNodeByPatch(node.Parent);
					var fromID = parent.ID;
					var toID = pin.ParentNodeByPatch(node.Parent).ID;
					var fromName = cpin.NameByParent(parent);
					var toName = pin.NameByParent(node);
					var link = patch.AddLink(FOldID2NewID[fromID], fromName, FOldID2NewID[toID], toName);
				
					//copy linkpoints from selectionXML
					var origLink = (from XmlElement l in origLinks where 
					         (l.GetAttribute("srcnodeid") == fromID.ToString() 
					          && l.GetAttribute("dstnodeid") == toID.ToString()
					          && l.GetAttribute("srcpinname") == fromName
					          && l.GetAttribute("dstpinname") == toName) select l).First();
					foreach (XmlElement point in origLink)
						link.AppendChild(link.OwnerDocument.ImportNode(point, true));
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
				
				if (!IOpins.ContainsKey(ident))
				{
					var pinType = pin.Type;
					//create an iobox of the right type
					var iobox = CreateIOBox(patch, pinType);
					
					iobox.ID = newNodeID;
					var bounds = node.GetBounds(BoundsType.Node);
					
					//name the iobox
					var labelPin = iobox.AddPin("Descriptive Name");
					var boxBounds = iobox.AddBounds(BoundsType.Box);
					
					if (pin.Direction == PinDirection.Input)
					{
						boxBounds.Rectangle = new Rectangle(Math.Max(minInputX, bounds.X - minX + CBorder), CPinOffset, 750, 225);
						
						//an input-pin may be connected to an output-pin
						//that in turn is connected to multiple inputs
						//in those cases name the iobox by concatenating the names of all those pins (which are in the selection!)
						//but leave out duplicates
						var pinName = GetNameForInput(node.Parent, cpin);
						pinName = GetUniqueInputName(pinName);
						oldPinToNewPin.Add(ident, pinName);
						labelPin.SetAttribute("values", "|" + pinName + "|");
						
						//save it for reference
						var parent = cpin.ParentNodeByPatch(node.Parent);
						IOpins.Add(parent.ID.ToString() + cpin.NameByParent(parent), newNodeID);
						var ioboxOutput = GetIOBoxPinName(pinType, false);
						patch.AddLink(newNodeID, ioboxOutput, FOldID2NewID[pin.ParentNodeByPatch(node.Parent).ID], pin.NameByParent(node));
						
						minInputX = boxBounds.Rectangle.X + boxBounds.Rectangle.Width + 150;
					}
					else if (pin.Direction == PinDirection.Output)
					{
						
						boxBounds.Rectangle = new Rectangle(Math.Max(minOutputX, bounds.X - minX + CBorder), (maxY  -minY) + CPinOffset + CBorder, 750, 225);
						var origName = pin.NameByParent(node);
						var pinName = GetUniqueOutputName(origName);
						oldPinToNewPin.Add(ident, pinName);
						labelPin.SetAttribute("values", "|" + pinName + "|");
						
						//save it for reference
						IOpins.Add(pin.ParentNodeByPatch(node.Parent).ID.ToString() + origName, newNodeID);
						var ioboxInput = GetIOBoxPinName(pinType, true);
						patch.AddLink(FOldID2NewID[node.ID], origName, newNodeID, ioboxInput);
						
						minOutputX = boxBounds.Rectangle.X + boxBounds.Rectangle.Width + 150;
					}
					
					var nodeBounds = iobox.AddBounds(BoundsType.Node);
					nodeBounds.Rectangle = boxBounds.Rectangle;
					newNodeID++;
				}
				else //IOpin already exists
				{
					var srcID = IOpins[ident];
					//this needs only be done for inputs
					if (pin.Direction == PinDirection.Input)
						patch.AddLink(srcID, GetIOBoxPinName(cpin.Type, false), FOldID2NewID[pin.ParentNodeByPatch(node.Parent).ID], pin.NameByParent(node));
				}
			}
			
			//remove superfluous links from origXML
			var linksToRemove = FDocument.DocumentElement.SelectNodes("/PATCH/LINK");
			foreach (XmlElement link in linksToRemove)
				FDocument.DocumentElement.RemoveChild(link);
			
			foreach (XmlElement node in patch.XML)
			{
				var n = FDocument.ImportNode(node, true);
				FDocument.DocumentElement.AppendChild(n);
			}
			hdeHost.SendXMLSnippet(patchPath, FDocument.OuterXml, true);

			//make connections to new subpatch
			patch = new PatchMessage("");
			
			foreach (var node in selectedNodes)
				foreach (var pin in node.Pins)
					foreach (var cpin in pin.ConnectedPins)
						//..if there is a connection to a not selected node
						if (!FOldID2NewID.ContainsKey(cpin.ParentNodeByPatch(node.Parent).ID))
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
//			var nodeB = nodeMsg.AddBounds(BoundsType.Node);
//			nodeB.Rectangle = new Rectangle(selectionCenter.X, selectionCenter.Y, 0, 0);
//			var boxB = nodeMsg.AddBounds(BoundsType.Node);
//			boxB.Rectangle = new Rectangle(selectionCenter.X - selectionSize.Width / 2, selectionCenter.Y - selectionSize.Height / 2, selectionSize.Width, selectionSize.Height);
			var windowB = nodeMsg.AddBounds(BoundsType.Window);
			windowB.Rectangle = new Rectangle(300 + selectionCenter.X + hdeHost.ActivePatchWindow.Bounds.X * 15, 300 + selectionCenter.Y + hdeHost.ActivePatchWindow.Bounds.Y * 15, selectionSize.Width, selectionSize.Height);
			
			hdeHost.SendXMLSnippet(hdeHost.ActivePatchWindow.Node.NodeInfo.Filename, patch.ToString(), true);
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
			//concatenate the names of all pins connected to this output (which are in the selection)
			var names = new List<string>();
			foreach(var cpin in pin.ConnectedPins)
			{
				var parent = cpin.ParentNodeByPatch(patch);
				if (FOldID2NewID.ContainsKey(parent.ID))
				{
					var pinName = cpin.NameByParent(parent);
					if (!names.Contains(pinName))
						names.Add(pinName);
				}
			}
			
			return names.Aggregate((i, j) => i + " - " + j);
		}
	}
}