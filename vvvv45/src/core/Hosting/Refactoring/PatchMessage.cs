#region usings
using System;
using System.Drawing;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
#endregion usings

namespace VVVV.Hosting
{
	class PatchMessage
	{
		public string Filename;		
		private List<NodeMessage> FNodes = new List<NodeMessage>();
		private List<LinkMessage> FLinks = new List<LinkMessage>();
		public List<LinkMessage> Links
		{
			get
			{
				return FLinks;
			}
		}
		
		public PatchMessage(string fileName)
		{
			Filename = fileName;
		}
		
		public bool SaveMe {get; set;}
		
		public NodeMessage AddNode(int id)
		{
			var node = new NodeMessage(id);
			FNodes.Add(node);
			return node;
		}
		
		public NodeMessage AddNode(string systemName)
		{
			var node = new NodeMessage(-1);
			node.SystemName = systemName;
			FNodes.Add(node);
			return node;
		}
		
		public LinkMessage AddLink(int fromID, string fromName, int toID, string toName)
		{
			var link = new LinkMessage(fromID, fromName, toID, toName);
			FLinks.Add(link);
			return link;
		}
		
		override public string ToString()
		{
			var nodes = "";
			foreach (var node in FNodes)
				nodes += node.ToString() + "\n\t";
			
			var links = "";
			foreach (var link in FLinks)
				links += link.ToString() + "\n\t";
			
			if (SaveMe)
				return "<PATCH saveme=\"" + Filename + "\">\n\t" + nodes + "\n\t" + links +"\n</PATCH>";
			else
				return "<PATCH>\n\t" + nodes + "\n\t" + links +"\n</PATCH>";
		}
	}
	
	class NodeMessage
	{
		public int ID;		
		private List<PinMessage> FPins = new List<PinMessage>();
		
		public NodeMessage(int id)
		{
			ID = id;
		}
		
		public string SystemName {get; set;}
		private bool FHasNodeBounds;
		private Rectangle FNodeBounds;
		public Rectangle NodeBounds 
		{
			get
			{
				return FNodeBounds;
			}
				
			set
			{
				FHasNodeBounds = true;
				FNodeBounds = value;
			}
		}
		
		private bool FHasBoxBounds;
		private Rectangle FBoxBounds;
		public Rectangle BoxBounds 
		{
			get
			{
				return FBoxBounds;
			}
				
			set
			{
				FHasBoxBounds = true;
				FBoxBounds = value;
			}
		}
		
		private bool FHasWindowBounds;
		private Rectangle FWindowBounds;
		public Rectangle WindowBounds 
		{
			get
			{
				return FWindowBounds;
			}
				
			set
			{
				FHasWindowBounds = true;
				FWindowBounds = value;
			}
		}
		
		public bool DeleteMe {get; set;}		
			
		public PinMessage AddPin(string pinName)
		{
			var pin = new PinMessage(pinName);
			FPins.Add(pin);
			return pin;
		}
		
		private string RectToStr(Rectangle rect)
		{
			return "left=\"" + rect.Left.ToString() + "\" " + "top=\"" + rect.Top.ToString() + "\" " + "width=\"" + rect.Width.ToString() + "\" " + "height=\"" + rect.Height.ToString() + "\"";
		}
		
		override public string ToString()
		{
			var pins = "";
			foreach (var pin in FPins)
				pins += "\n\t\t" + pin.ToString();
			
			var bounds = "";
			if (FHasNodeBounds)
				bounds = "<BOUNDS type=\"Node\" " + RectToStr(NodeBounds) + "/>";
			
			if (FHasBoxBounds)
				bounds += "\n\t\t<BOUNDS type=\"Box\" " + RectToStr(BoxBounds) + "/>";
			
			if (FHasWindowBounds)
				bounds += "\n\t\t<BOUNDS type=\"Window\" " + RectToStr(WindowBounds) + "/>";
			
			if (DeleteMe)
				return "<NODE id=\"" + ID + "\" deleteme=\"true\"/>";
			else
				return "<NODE id=\"" + ID + "\" systemname=\"" + SystemName + "\">\n\t\t" + bounds + "\n\t\t" + pins + "\n\t</NODE>";
		}
	}
	
	class PinMessage
	{
		public string PinName;		
		private Dictionary<string, string> FAttributes = new Dictionary<string, string>();
		
		public PinMessage(string pinName)
		{
			PinName = pinName;
		}
		
		public void AddAttribute(string name, string value)
		{
			FAttributes.Add(name, value);
		}
		
		override public string ToString()
		{
			var result = "";
			foreach (var attribute in FAttributes)
				result += attribute.Key + "=\"" + attribute.Value + "\"";
			
			return "<PIN pinname=\"" + PinName + "\" " + result + " />";
		}
	}
	
	class LinkMessage
	{
		public int FromID {get; set;}
		public string FromName {get; set;}
		public int ToID {get; set;}
		public string ToName {get; set;}
		
		public LinkMessage(int fromID, string fromName, int toID, string toName)
		{
			FromID = fromID;
			FromName = fromName;
			ToID = toID;
			ToName = toName;
		}
		
		override public string ToString()
		{
			return "<LINK dstnodeid=\"" + ToID.ToString() + "\" dstpinname=\"" + ToName + "\" srcnodeid=\"" + FromID.ToString() + "\" srcpinname=\"" + FromName + "\"/>";
		}
	}
}