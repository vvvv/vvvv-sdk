using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VVVV.Core.Viewer.GraphicalEditor
{
    public interface IDot : ISolid
    {
        float DotSize
        {
            get;
            set;
        }

        float Rotation
        {
            get;
            set;
        }
    }
}
