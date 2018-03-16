#region usings
using System;
using System.Xml;
using System.Drawing;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
#endregion usings

namespace VVVV.Hosting
{
	public class PatchMessage
	{
		private XmlDocument FPatch;
		public XmlElement XML
		{
			get {return FPatch.DocumentElement;}
		}
		
		public PatchMessage(string fileName)
		{
			FPatch = new XmlDocument();
			var patch = FPatch.CreateElement("PATCH");
			FPatch.AppendChild(patch);
			Filename = fileName;
		}

        private string FFileName;
		public string Filename
		{
			get {return FFileName;}
			set 
			{
                if (FFileName != value)
                {
                    FPatch.DocumentElement.SetAttribute("nodename", value);
                    FFileName = value;
                }
			}
        }

        public void AddSaveMe()
        {
            FPatch.DocumentElement.SetAttribute("saveme", FFileName);
        }

		public NodeMessage AddNode(int id)
		{
			var node = new NodeMessage(FPatch, id);
			FPatch.DocumentElement.AppendChild(node);
			return node;
		}
		
		public NodeMessage AddNode(string systemName)
		{
			var node = new NodeMessage(FPatch, -1);
			node.SystemName = systemName;
			FPatch.DocumentElement.AppendChild(node);
			return node;
		}
		
		public LinkMessage AddLink(int fromID, string fromName, int toID, string toName)
		{
			var link = new LinkMessage(FPatch, fromID, fromName, toID, toName);
			FPatch.DocumentElement.AppendChild(link);
			return link;
		}
		
		override public string ToString()
		{
			return FPatch.OuterXml;
		}
	}
	
	public class NodeMessage: XmlElement
	{
		public NodeMessage(XmlDocument patch, int id): base(String.Empty, "NODE", String.Empty, patch)
		{
			ID = id;
		}
		
		public int ID
		{
			get {return int.Parse(GetAttribute("id"));}
			set {SetAttribute("id", value.ToString());}
		}
		
		public string SystemName
		{
			get {return GetAttribute("systemname");}
			set {SetAttribute("systemname", value);}
		}

        public string Filename
        {
            get { return GetAttribute("filename"); }
            set { SetAttribute("filename", value); }
        }

        public ComponentMode ComponentMode
		{
			//get {return ComponentMode.Parse(GetAttribute("componentmode"));}
			set {SetAttribute("componentmode", value.ToString());}
		}

        public bool CreateMe
        {
            get 
            {
                var a = GetAttribute("createme");
                if (a != null)
                    return bool.Parse(a);
                return false;
            }
            set { SetAttribute("createme", value.ToString()); }
        }
		
		public bool DeleteMe 
		{
			get 
            {
                var a = GetAttribute("deleteme");
                if (a != null)
                    return bool.Parse(a);
                return false;
            }
			set { SetAttribute("deleteme", value.ToString()); }
		}
					
		public PinMessage AddPin(string pinName)
		{
			var pin = new PinMessage(OwnerDocument, pinName);
			AppendChild(pin);
			return pin;
		}
		
		public BoundsMessage AddBounds(BoundsType boundsType)
		{
			var bounds = new BoundsMessage(OwnerDocument, boundsType);
			AppendChild(bounds);
			return bounds;
		}
		
		private string RectToStr(Rectangle rect)
		{
			return "left=\"" + rect.Left.ToString() + "\" " + "top=\"" + rect.Top.ToString() + "\" " + "width=\"" + rect.Width.ToString() + "\" " + "height=\"" + rect.Height.ToString() + "\"";
		}
		
		override public string ToString()
		{
			return OuterXml;
		}
	}
	
	public class PinMessage: XmlElement
	{
		public PinMessage(XmlDocument patch, string pinName): base(String.Empty, "PIN", String.Empty, patch)
		{
			PinName = pinName;
		}
		
		public string PinName	
		{
			get {return GetAttribute("pinname");}
			set {SetAttribute("pinname", value);}
		}
		
		public string Spread	
		{
			get {return GetAttribute("values");}
			set {SetAttribute("values", value);}
		}
		
		override public string ToString()
		{
			return OuterXml;
		}
	}
	
	public class BoundsMessage: XmlElement
	{
		public BoundsMessage(XmlDocument patch, BoundsType boundsType): base(String.Empty, "BOUNDS", String.Empty, patch)
		{
			SetAttribute("type", boundsType.ToString());
		}
		
		private Rectangle FRectangle;
		public Rectangle Rectangle
		{
			get 
			{
				return FRectangle;
			}
		
			set 
			{
				FRectangle = value;
				SetAttribute("left", value.Left.ToString());
				SetAttribute("top", value.Top.ToString());
				SetAttribute("width", value.Width.ToString());
				SetAttribute("height", value.Height.ToString());
			}
		}
		
		override public string ToString()
		{
			return OuterXml;
		}
	}
	
	public class LinkMessage: XmlElement
	{
		public LinkMessage(XmlDocument patch, int fromID, string fromName, int toID, string toName): base(String.Empty, "LINK", String.Empty, patch)
		{
			FromID = fromID;
			FromName = fromName;
			ToID = toID;
			ToName = toName;
		}
		
		public int FromID 
		{
			get {return int.Parse(GetAttribute("srcnodeid"));}
			set {SetAttribute("srcnodeid", value.ToString());}
		}
		
		public string FromName
		{
			get {return GetAttribute("srcpinname");}
			set {SetAttribute("srcpinname", value);}
		}
		
		public int ToID
		{
			get {return int.Parse(GetAttribute("dstnodeid"));}
			set {SetAttribute("dstnodeid", value.ToString());}
		}
		
		public string ToName
		{
			get {return GetAttribute("dstpinname");}
			set {SetAttribute("dstpinname", value);}
		}
		
		override public string ToString()
		{
			return OuterXml;
		}
	}
}