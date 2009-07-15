using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    public class GuiDataObject
    {

        public enum Position
        {
            Head = 0,
            Body = 0,
        }



        private SortedList<string, string> mCssProperties = new SortedList<string, string>();
        private List<GuiDataObject> mGuiUpstreamList = new List<GuiDataObject>();
        private SortedList<string,string> mTransform = new SortedList<string,string>();
        private string mNodeId = "";
        private string mSliceId = "";
        private StringBuilder mHead;
        private StringBuilder mBody;



        public string Head
        {
            get
            {
                if (mHead != null)
                {
                    return mHead.ToString();
                }
                else
                {
                    return ""; 
                }
            }

        }


        public string Body
        {
            get
            {
                if (mBody != null)
                {
                    return mBody.ToString();
                }
                else
                {
                    return "";
                }
            }
        }

        public string NodeId
        {
            get
            {
                return mNodeId;
            }
            set
            {
                mNodeId = value;
            }
        }

        public string SliceId
        {
            get
            {
                return mSliceId;
            }
            set
            {
                mSliceId = value;
            }
        }

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


        public void AddTag(string pContent, Position pPosition)
        {
            if (pPosition == Position.Head)
            {
                mHead = new StringBuilder();
                mHead.Append(pContent);
            }
            else
            {
                mBody = new StringBuilder();
                mBody.Append(pContent);
            }
        }

       
    }

}
