#region usings
using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
#endregion usings

namespace VVVV.Nodes
{
	class PatchMessage
	{
		public string Filename;		
		private List<NodeMessage> FNodes = new List<NodeMessage>();
		
		public PatchMessage(string fileName)
		{
			Filename = fileName;
		}
		
		public NodeMessage AddNode(int id)
		{
			var node = new NodeMessage(id);
			FNodes.Add(node);
			return node;
		}
		
		override public string ToString()
		{
			var result = "";
			foreach (var node in FNodes)
				result += node.ToString() + "\n";
			
			return "<PATCH saveme=\"" + Filename + "\">\n\t" + result + "\n</PATCH>";
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
		
		public PinMessage AddPin(string pinName)
		{
			var pin = new PinMessage(pinName);
			FPins.Add(pin);
			return pin;
		}
		
		override public string ToString()
		{
			var result = "";
			foreach (var pin in FPins)
				result += pin.ToString() + "\n";
			
			return "<NODE id=\"" + ID + "\">\n\t\t" + result + "</NODE>";
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
}