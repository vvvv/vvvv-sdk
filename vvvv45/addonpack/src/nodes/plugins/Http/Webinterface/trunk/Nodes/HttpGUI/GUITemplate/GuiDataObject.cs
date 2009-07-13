using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    public class GuiDataObject
    {


        private SortedList<string, string> mCssProperties = new SortedList<string, string>();
        private List<GuiDataObject> mGuiUpstreamList = new List<GuiDataObject>();
        private SortedList<string,string> mTransform = new SortedList<string,string>();

        public SortedList<string, string> CssProperties
        {
            get
            {
                return mCssProperties;
            }
            set
            {
                mCssProperties = value;
            }
        }

        public List<GuiDataObject> GuiUpstreamList
        {
            get
            {
                return mGuiUpstreamList;
            }
            set
            {
                mGuiUpstreamList = value;
            }
        }

        public SortedList<string, string> Transform
        {
            get
            {
                return mTransform;
            }
            set
            {
                mTransform = value;
            }
        }


       
    }

}
