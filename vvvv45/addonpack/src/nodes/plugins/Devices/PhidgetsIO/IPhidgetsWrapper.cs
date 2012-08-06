using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phidgets;
using Phidgets.Events;


namespace VVVV.Nodes
{
    interface IPhidgetsWrapper
    {
        int Count { get; }
        bool Changed { get; }
        void AddChangedHandler();
        void RemoveChangedHandler();
    }
}
