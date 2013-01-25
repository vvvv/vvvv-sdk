using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VVVV.Core.View.GraphicalEditor
{
    public interface IMovable
    {
        void UpdateBounds(RectangleF bounds);
    }
}
