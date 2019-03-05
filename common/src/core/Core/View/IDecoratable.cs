using System;
using System.Drawing;

namespace VVVV.Core.View
{
    public delegate void DecorationChangedHandler();
    
    public enum NodeIcon {None, GUI, Code, Patch, GUICode, GUIPatch, Comment, IONode, SRNode};
	/// <summary>
	/// Provides decorations.
	/// </summary>		
	public interface IDecoratable
	{
	    Pen TextColor{get;}
	    Pen TextHoverColor{get;}
	    Brush BackColor{get;}
	    Brush BackHoverColor{get;}
	    Pen OutlineColor{get;}
	    string Text{get;}
	    NodeIcon Icon{get;}
	    
	    event DecorationChangedHandler DecorationChanged;
	}
}
