using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Core.Viewer.GraphicalEditor
{
    public interface ICircle : ISolid
    {
        float Radius
        {
            get;
            set;
        }
    }
}
