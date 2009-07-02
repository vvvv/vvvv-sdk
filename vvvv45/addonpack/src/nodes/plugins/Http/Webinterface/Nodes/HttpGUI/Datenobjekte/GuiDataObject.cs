using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI.Datenobjekte
{
    public class GuiDataObject
    {


        private int mSliceCount;
        

        public int SliceCount
        {
            get
            {
                return mSliceCount;
            }
            set
            {
                mSliceCount = value;
            }
        }

        public GuiDataObject()
        {

        }

        public void AddTag(Tag pTag)
        {

        }

        public void AddCssPropertie(string pName, string pValue)
        {

        }


    }

}
