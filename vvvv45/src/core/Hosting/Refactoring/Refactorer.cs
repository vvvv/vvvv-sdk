#region usings
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Hosting
{
	public class PatchRefactorer
	{
		#region fields
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

            var basePatchPath = hdeHost.ActivePatchWindow.Node.NodeInfo.Filename;
            //create new nodinfo for subpatch
            var patchPath = Path.GetDirectoryName(basePatchPath);
			if (!Path.IsPathRooted(patchPath))
				patchPath = hdeHost.ExePath;
			var patchName = GetUniquePatchName(hdeHost.ActivePatchWindow.Node.NodeInfo.Filename);
			
			patchPath = Path.Combine(patchPath, patchName) + ".v4p";
			
			var ni = FNodeInfoFactory.CreateNodeInfo(patchName, "", "", patchPath, true);
			ni.InitialComponentMode = TComponentMode.Hidden;
			ni.Type = NodeType.Patch;
			ni.CommitUpdate();
			
			//modify the current selection XML
			FDocument = new XmlDocument();
			var snippet = hdeHost.GetXMLSnippetFromSelection();
			try 
			{
				//doctype path may include utf8 encoded characters
				var doctype = snippet.Split(new Char[] {'>'}, 2);
				//so decode those..
				snippet = UTF8toUnicode(doctype[0]) + ">" + doctype[1];
				FDocument.LoadXml(snippet);
			} 
			catch (Exception e)
			{
                //for debugging
				//System.Windows.Forms.MessageBox.Show(e.Message);
			}
			
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
			
			FInputNames.Clear();
			FOutputNames.Clear();
			
			//extract all existing iobox names, as those must not be reused
			foreach (var node in selectedNodes)
			{
				if (node.NodeInfo.Name == "IOBox")
				{
					var inputConnected = node.FindPin(GetIOBoxPinName(node.NodeInfo.Category, true)).IsConnected();
					var outputConnected = node.FindPin(GetIOBoxPinName(node.NodeInfo.Category, false)).IsConnected();
					if (inputConnected && !outputConnected)
						FOutputNames.Add(node.FindPin("Descriptive Name").Spread.Trim('|'));
					else if (!inputConnected && outputConnected)
						FInputNames.Add(node.FindPin("Descriptive Name").Spread.Trim('|'));
				}
			}
			
			//now sort the list of nodes in X from left to right
			//in order to get IOBoxes of leftmost nodes also leftmost
			var nodes = (from n in selectedNodes orderby n.GetBounds(BoundsType.Node).X select n).ToList();
			var oldPinToNewPin = new Dictionary<string, string>();
			
			var IOpins = new Dictionary<string, int>();
			var minInputX = 0;
			var minOutputX = 0;
			
			//make connections in the selection
			//for each selected nodes input pin...
			foreach (var node in nodes)
				foreach (var pin in node.Pins.Where(p => p.Direction != PinDirection.Configuration))
					foreach (var cPin in pin.ConnectedPins)
						if (!cPin.Name.Contains("ROUTER DON'T USE")) //hack for S/R nodes 
			{
				//..if there is a connection to another selected node in the same patch
				//(pins of IOboxes can also be connected to nodes in parentpatches!)
				var cNode = cPin.ParentNodeByPatch(node.Parent);
				if (cNode != null)
					if (FOldID2NewID.ContainsKey(cNode.ID))
				{
					//this needs only be done for inputs
					if (pin.Direction == PinDirection.Input)
					{
						var fromID = cNode.ID;
						var toID = pin.ParentNodeByPatch(node.Parent).ID;
						var fromName = cPin.NameByParent(cNode);
						var toName = pin.NameByParent(node);
						
						//copy over complete link (including linkpoints)
						var link = (from XmlElement l in origLinks where
						            (l.GetAttribute("srcnodeid") == fromID.ToString()
						             && l.GetAttribute("dstnodeid") == toID.ToString()
						             && l.GetAttribute("srcpinname") == fromName
						             && l.GetAttribute("dstpinname") == toName) select l).First() as XmlElement;
						link.SetAttribute("srcnodeid", FOldID2NewID[fromID].ToString());
						link.SetAttribute("dstnodeid", FOldID2NewID[toID].ToString());
						
						patch.XML.AppendChild(patch.XML.OwnerDocument.ImportNode(link, true));
					}
				}
				//..if there is a connection to a not selected node
				else
				{
					//an IO pin needs to be created
					//- if it doesn't exist yet (multiple inputs may connect to an upstream pin and an IO pin may already exist now)
					//- if the connected pin belongs to a (preexisting) labeled iobox
					string ident = "";
					if (pin.Direction == PinDirection.Input)
						ident = cNode.ID.ToString() + cPin.NameByParent(cNode);
					else if (pin.Direction == PinDirection.Output)
						ident = node.ID.ToString() + pin.NameByParent(node);

					if ((node.NodeInfo.Name == "IOBox") && (!string.IsNullOrEmpty(node.LabelPin[0])))
					{
						if (!IOpins.ContainsKey(ident))
						{
							IOpins.Add(ident, FOldID2NewID[node.ID]);
							oldPinToNewPin.Add(ident, node.LabelPin[0]);
						}
					}
					
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
							boxBounds.Rectangle = new Rectangle(Math.Max(minInputX, bounds.X - minX + CBorder), CPinOffset, 750, 240);
							
							//an input-pin may be connected to an output-pin
							//that in turn is connected to multiple inputs
							//in those cases name the iobox by concatenating the names of all those pins (which are in the selection!)
							//but leave out duplicates
							var pinName = GetNameForInput(node.Parent, cPin);
							pinName = GetUniqueInputName(pinName);
							oldPinToNewPin.Add(ident, pinName);
							labelPin.SetAttribute("values", "|" + pinName + "|");
							
							//save it for reference
							IOpins.Add(ident, newNodeID);
							var ioboxOutput = GetIOBoxPinName(pinType, false);
							patch.AddLink(newNodeID, ioboxOutput, FOldID2NewID[pin.ParentNodeByPatch(node.Parent).ID], pin.NameByParent(node));
							
							minInputX = boxBounds.Rectangle.X + boxBounds.Rectangle.Width + 150;
						}
						else if (pin.Direction == PinDirection.Output)
						{
							boxBounds.Rectangle = new Rectangle(Math.Max(minOutputX, bounds.X - minX + CBorder), (maxY  -minY) + CPinOffset + CBorder, 750, 240);
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
							patch.AddLink(srcID, GetIOBoxPinName(cPin.Type, false), FOldID2NewID[pin.ParentNodeByPatch(node.Parent).ID], pin.NameByParent(node));
					}
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
				foreach (var pin in node.Pins.Where(p => p.Direction != PinDirection.Configuration))
					foreach (var cpin in pin.ConnectedPins)
						if (!cpin.Name.Contains("ROUTER DON'T USE"))  //hack for S/R nodes 
						//..if there is a connection to a not selected node..
			{
				var parent = cpin.ParentNodeByPatch(node.Parent);
				//..in the same patch..
				if (parent != null)
				{
					if (!FOldID2NewID.ContainsKey(parent.ID))
						if (pin.Direction == PinDirection.Input)
					{
						patch.AddLink(parent.ID, cpin.NameByParent(parent), subID, oldPinToNewPin[parent.ID.ToString() + cpin.NameByParent(parent)]);
					}
					else if (pin.Direction == PinDirection.Output)
						patch.AddLink(subID, oldPinToNewPin[pin.ParentNodeByPatch(node.Parent).ID.ToString() + pin.NameByParent(node)], parent.ID, cpin.NameByParent(parent));
				}
				else //..in the parentpatch
				{
					if (pin.Direction == PinDirection.Input)
						patch.AddLink(node.ID, cpin.Name, subID, node.LabelPin.Spread.Trim('|'));
					else if (pin.Direction == PinDirection.Output)
						patch.AddLink(subID, node.LabelPin.Spread.Trim('|'), node.ID, cpin.Name);
				}
			}
			
			//..and remove selected nodes
			//if they are not labeled ioboxes that are connected in a parentpatch!
			foreach (var node in selectedNodes)
			{
				var delete = true;
				//if this is a labeled iobox check if any of its pins is connected in a parentpatch to not delete it in that case
				if ((node.NodeInfo.Name == "IOBox")
				    && (!string.IsNullOrEmpty(node.LabelPin.Spread)))
				{
					foreach (var pin in node.Pins)
						foreach (var cpin in pin.ConnectedPins)
							if (cpin.ParentNodeByPatch(node.Parent) == null)
					{
						delete  = false;
						break;
					}
				}

				if (delete)
				{
					var nodeMessage = patch.AddNode(node.ID);
					nodeMessage.DeleteMe = true;
				}
			}
			
			var nodeMsg = patch.AddNode(subID);
			nodeMsg.ComponentMode = ComponentMode.Hidden;
			//enabling this fukcs it up:
			var nodeB = nodeMsg.AddBounds(BoundsType.Node);
			nodeB.Rectangle = new Rectangle(selectionCenter.X, selectionCenter.Y, 0, 0);
			var boxB = nodeMsg.AddBounds(BoundsType.Box);
			boxB.Rectangle = new Rectangle(selectionCenter.X - selectionSize.Width / 2, selectionCenter.Y - selectionSize.Height / 2, selectionSize.Width, selectionSize.Height);
			
			//make window-pos -1/-1 so on popup it opens at mousecursor
			var windowB = nodeMsg.AddBounds(BoundsType.Window);
			windowB.Rectangle = new Rectangle(-1, -1, selectionSize.Width, selectionSize.Height);
			
			hdeHost.SendXMLSnippet(basePatchPath, patch.ToString(), true);
		}
		//FLogger.Log(LogType.Debug, "hi tty!");
		
		string UTF8toUnicode(string input)
        {
            var utf8Bytes = Encoding.Default.GetBytes(input);
            var unicodeBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8Bytes);
            return Encoding.Unicode.GetString(unicodeBytes);
        }
		
		private string GetIOBoxPinName(string pinType, bool input)
		{
			if (pinType == "String")
			{	
                return input ? "Input String" : "Output String";
			}
			else if (pinType == "Value")
			{
                return input ? "Y Input Value" : "Y Output Value";
			}
			else if (pinType == "Color")
			{	
                return input ? "Color Input" : "Color Output";
			}
			else if (pinType.StartsWith("Enumeration")) //beware: category is named: Enumerations (plural) while pintype is named: Enumeration (singular). 
			{	
                return input ? "Input Enum" : "Output Enum";
			}
			else //assume node
			{
                return input ? "Input Node" : "Output Node";
			}
		}
		
		private NodeMessage CreateIOBox(PatchMessage patch, string pinType)
		{
			NodeMessage node;
			if (pinType == "String")
				node = patch.AddNode("IOBox (String)");
			else if (pinType == "Value")
				node = patch.AddNode("IOBox (Value Advanced)");
			else if (pinType == "Color")
				node = patch.AddNode("IOBox (Color)");
			else if (pinType == "Enumeration")
				node = patch.AddNode("IOBox (Enumerations)");
			else //assume node
				node = patch.AddNode("IOBox (Node)");
			
			node.ComponentMode = ComponentMode.InABox;
			return node;
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