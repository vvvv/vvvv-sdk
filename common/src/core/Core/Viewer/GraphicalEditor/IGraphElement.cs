using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using VVVV.Core.View;
using VVVV.Core.View.GraphicalEditor;

namespace VVVV.Core.Viewer.GraphicalEditor
{
    public interface IGraphElement : ICollection<IGraphElement>, IContentBounds
    {
        IGraphElement Parent
        {
            get;
        }

        bool IsSelectable
        {
            get;
        }

        ISelectable Selectable
        {
            get;
        }

        bool IsMovable
        {
            get;
        }

        IMovable Movable
        {
            get;
        }

        bool IsConnectable
        {
            get;
        }

        IConnectable Connectable
        {
            get;
        }

        bool Visible
        {
            get;
            set;
        }

        void BringToFront();
    }        
}
