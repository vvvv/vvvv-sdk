using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    public class GuiDataObject : ICloneable
    {

        public enum Position
        {
            Head = 0,
            Body = 1,
            JavaScript = 2,
			UpstreamJQuery = 3
        }



        private SortedList<string, string> mCssProperties = new SortedList<string, string>();
        private List<GuiDataObject> mGuiUpstreamList;
        private SortedList<string,string> mTransform = new SortedList<string,string>();
        private string mNodeId = "";
        private string mSliceId = "";
		private SortedList<Position, StringBuilder> FContent = new SortedList<Position, StringBuilder>();
        private Tag mTag;


		public GuiDataObject()
		{
			for (Position p = Position.Head; p <= Position.UpstreamJQuery; p++)
			{
				FContent[p] = new StringBuilder();
			}
		}

		public Tag Tag
        {
            get
            {
                return mTag;
            }
            set
            {
                mTag = value;
            }
        }



        public string Head
        {
            get
            {
                if (FContent[Position.Head] != null)
                {
                    return FContent[Position.Head].ToString();
                }
                else
                {
                    return ""; 
                }
            }
        }

        public StringBuilder JavaScript
        {
            get
            {
                return FContent[Position.JavaScript].Append(FContent[Position.UpstreamJQuery]);
            }
        }


        public string Body
        {
            get
            {
                if (FContent[Position.Body] != null)
                {
                    return FContent[Position.Body].ToString();
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



        public void AddString(string pContent, Position pPosition, bool Reset)
        {
            if (Reset)
            {
				ResetContent(pPosition);
            }

			FContent[pPosition].Append(pContent);
        }

		public void ResetContent(Position pPosition)
		{
			FContent[pPosition].Length = 0;
		}

		#region ICloneable Members

		public object Clone()
		{
			GuiDataObject clonedObject = new GuiDataObject();
			foreach (KeyValuePair<string, string> kvp in mCssProperties)
			{
				clonedObject.mCssProperties.Add(System.String.Copy(kvp.Key), System.String.Copy(kvp.Value));
			}

			clonedObject.mGuiUpstreamList = mGuiUpstreamList;

			foreach (KeyValuePair<string, string> kvp in mTransform)
			{
				clonedObject.mTransform.Add(System.String.Copy(kvp.Key), System.String.Copy(kvp.Value));
			}

			
			clonedObject.mNodeId = System.String.Copy(mNodeId);
			clonedObject.mSliceId = System.String.Copy(mSliceId);

			for (Position p = Position.Head; p <= Position.UpstreamJQuery; p++)
			{
				clonedObject.FContent[p] = new StringBuilder(FContent[p].ToString());
			}
			

			if (mTag != null)
			{
				clonedObject.mTag = (Tag)(mTag.Clone());
			}
			else
			{
				clonedObject.mTag = mTag;
			}
			
			clonedObject.mTag = (Tag)(mTag.Clone());

			return clonedObject;
		}

		#endregion
	}

}
