using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VVVV.Core.Viewer.GraphicalEditor
{
	public struct StyledString
	{
		public string Text;
		public FontStyle FontStyle;
	}
	
    public interface IText : ISolid
    {
        string Caption
        {
            get;
            set;
        }
        
        Font Font
        {
            get;
            set;
        }
    }
}
