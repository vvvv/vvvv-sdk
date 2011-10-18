using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    class ParentChildSelector:Selector
    {

        string FParent;
        string FChild;

        public ParentChildSelector(string PParent, string PChild)
        {
            Parent = PParent;
            Child = PChild;
		}

        public string Parent
        {
            set
            {
                FParent = value;
                PValue = FParent + " > " + FChild;
            }
                
        }

        public string Child
        {
            set
            {
                FChild = value;
                PValue = FParent + " > " + FChild;
            }
        }
    }
}
