using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VVVV.Core.Viewer.GraphicalEditor
{
    public interface IRectangle : ISolid
    {
        new SizeF Size
        {
            get;
            set;
        }
    }
}
