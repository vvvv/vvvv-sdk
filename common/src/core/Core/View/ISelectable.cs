using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Core.View
{
    public delegate void SelectionChangedHandler(ISelectable sender, EventArgs args);
    
    public interface ISelectable
    {
        bool Selected
        {
            get;
            set;
        }
        
        event SelectionChangedHandler SelectionChanged;
    }
}
