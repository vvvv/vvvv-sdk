using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Core.View.GraphicalEditor
{
    public interface IDeletable
    {
        bool AllowDelete();
        void Delete();
    }
}
