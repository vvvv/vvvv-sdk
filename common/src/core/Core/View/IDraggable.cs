using System;

namespace VVVV.Core.View
{
    /// <summary>
    /// Participates in a drag of a Drag'n Drop operation.
    /// </summary>
    public interface IDraggable
    {
        bool AllowDrag();
        object ItemToDrag();
    }
}
