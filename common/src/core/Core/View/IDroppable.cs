using System;
using System.Collections.Generic;
using System.Drawing;

namespace VVVV.Core.View
{
    /// <summary>
    /// Participates in a drop of a Drag'n Drop Operation.
    /// </summary>
    public interface IDroppable
    {
        bool AllowDrop(Dictionary<string, object> items);
        void DropItems(Dictionary<string, object> items, Point pt);
    }
}
