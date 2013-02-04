using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;

using Piccolo.NET;
using Piccolo.NET.Event;
using Piccolo.NET.Nodes;
using Piccolo.NET.Util;
using VVVV.Core.View.GraphicalEditor;
using VVVV.Core.Viewer.GraphicalEditor;

namespace VVVV.HDE.GraphicalEditing
{
	/// <summary>
	/// Text class, which holds the CombinedText class as PNode
	/// </summary>
    public class Text : Solid, IText 
    {
        CombinedText Label
        {
            get
            {
                return PNode as CombinedText;
            }
        }

        public Text(IGraphElementHost host, string caption)
            : base(host)
        {
        	if (caption == null) caption = "";
        	
            Caption = caption;
            Brush = null;
        }

        protected override PNode CreatePNode()
        {
        	return new CombinedText();
        }
      
        #region IText Members

        public Font Font
        {
            get
            {
                return Label.Font;               
            }
            set
            {
                Label.Font = value;
                Size = Label.FullBounds.Size;
            }
        }

        public string Caption
        {
            get
            {
                return Label.Text;
            }
            set
            {
                Label.Text = value;
                Size = Label.FullBounds.Size;
            }
        }
        
        public override PositionMode PositionMode {
            get 
            {
                return base.PositionMode;
            }
            set 
            {
                base.PositionMode = value; 
                Label.BuildText();
            }
        }
       
        #endregion IText Members
    }
    
    //simple string class
    internal class CombineString
    {
    	public string Text;
    	public bool IsBold;
    	
    	public CombineString(string text, bool isBold)
    	{
    		Text = text;
    		IsBold = isBold;
    	}
    }
    
    //combines PText nodes to one node
    internal class CombinedText : PNode
    {
        public CombinedText()
            : base()
        {
            FFont = PText.DEFAULT_FONT;
            FPen = Pens.Black;
            BoldTag = "@@";
        }
        
    	protected string FText;
    	public string Text
    	{
    		get
    		{
    			return FText;
    		}
    		set
    		{
    			if(value == null)
    				value = "";
    			
    			if(FText != value)
    			{
    				FText = value;
    				BuildText();
    			}
    		}
    	}
    	
    	internal void BuildText()
    	{
    		var split = ParseText();
    		
    		RemoveAllChildren();
    		
    		var off = 0.0f;
    		var marg = FFont.Size * 0.45f;
    		
    		for (int i=0; i<split.Length; i++) 
			{
    		    var t = new PText();
	    		t.Pickable = false; //do not eat input events
	    		t.ConstrainHeightToTextHeight = true;
	    		t.ConstrainWidthToTextWidth = true;
	    		t.Font = new Font(FFont.FontFamily, FFont.Size, split[i].IsBold ? FontStyle.Bold : FontStyle.Regular);
	    		t.TextBrush = FPen == null ? null : FPen.Brush;
	    		t.Text = split[i].Text;
                t.X = this.X;
                t.Y = this.Y;
	    		t.X += off;
	    		
	    		off += t.Width - marg;
	    		
	    		AddChild(t);
    		}

            this.Width = this.UnionOfChildrenBounds.Width;
            this.Height = this.UnionOfChildrenBounds.Height;

            //position may have changed when in center mode
            foreach (var item in this.AllNodes)
            {
                if (item is PText)
                {
                    (item as PText).X = this.X;
                    (item as PText).Y = this.Y;
                }
            }

    	}
    	
    	protected Font FFont;
    	public Font Font
    	{
    		get
    		{
    			return FFont;
    		}
    		set
    		{
    			FFont = value;
    			BuildText();
    		}
    	}
    	
    	protected Pen FPen;
    	public Pen Pen
    	{
    		get
    		{
    			return FPen;
    		}
    		set
    		{
    			FPen = value;
    			BuildText();
    		}
    	}
    	
    	public string BoldTag
    	{
    		get;
    		set;
    	}
    	
    	protected CombineString[] ParseText()
    	{
    		var l = new List<CombineString>();
    			
			var t = FText.Split(new string[]{BoldTag}, StringSplitOptions.None);
			
//			for (int i=0; i<256; i++) 
//			{
//				System.Diagnostics.Debug.WriteLine("" + i + "sometext" + (char)i);
//			}
			
			for (int i=0; i<t.Length; i++) 
			{
				var s = t[i];
				if(s.EndsWith(" "))
				{
					s.TrimEnd(' ');
					s += (char)128;
				}
				
				l.Add(new CombineString(s, i % 2 == 1));
			}
			
			//remove empty entrys
			for (int i=l.Count-1; i>=0; i--) 
			{
				if(l[i].Text == "") l.Remove(l[i]);
			}

    		return l.ToArray();
    	}
    	
    	//html tag parsing try out
//    	protected CombineString[] ParseText2()
//    	{
//    		var l = new List<CombineString>();
//    		
//    		var settings = new XmlReaderSettings();
//    		
//    		var xmlReader = XmlReader.Create( new StringReader("<MMASTERTAGG>" + FText + "</MMASTERTAGG>"), settings);
//    		
//    		var xElem = XElement.Load(xmlReader);
//    		
//    		var xpath = new XPathDocument(new StringReader("<MMASTERTAGG>" + FText + "</MMASTERTAGG>"));
//    		var nav = xpath.CreateNavigator();
//    		var xpr = nav.Compile("/MMASTERTAGG");
//    		
//    		var iter = nav.Select(xpr);
//    			
//    		while (iter.MoveNext())
//    		{
//    			System.Diagnostics.Debug.WriteLine(iter.Current.Value);
//    		}
//
//    			
//    		//xmlReader.Settings
//    		if(xmlReader.ReadToFollowing(BoldTag))
//    		{
//    			if(xmlReader.HasValue)
//    			{
//    				var s = xmlReader.ReadElementContentAsString();
//    				l.Add(new CombineString(s, true));
//    			}
//    		}
//			
//    		return l.ToArray();
//    	}
    }
}
