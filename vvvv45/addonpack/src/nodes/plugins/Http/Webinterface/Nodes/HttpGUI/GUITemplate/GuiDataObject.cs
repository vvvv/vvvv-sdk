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
        }



        private SortedList<string, string> mCssProperties = new SortedList<string, string>();
        private List<GuiDataObject> mGuiUpstreamList;
        private SortedList<string,string> mTransform = new SortedList<string,string>();
        private string mNodeId = "";
        private string mSliceId = "";
        private StringBuilder mHead;
        private StringBuilder mBody;
        private Tag mTag;
        private StringBuilder mJavaScript;



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

        public StringBuilder JavaScript
        {
            get
            {
                return mJavaScript;
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



        public void AddString(string pContent, Position pPosition, bool Reset)
        {
            if (Reset)
            {
				ResetContent(pPosition);
            }

            if (pPosition == Position.Head)
            {
                mHead.Append(pContent);
            }
            else if(pPosition == Position.Body)
            {
                mBody.Append(pContent);
            }
			else if(pPosition == Position.JavaScript)
            {
                mJavaScript.Append(pContent);
            }
        }

		public void ResetContent(Position pPosition)
		{
			if (pPosition == Position.Head)
			{
				mHead = new StringBuilder();
			}
			else if (pPosition == Position.Body)
			{
				mBody = new StringBuilder();
			}
			else if (pPosition == Position.JavaScript)
			{
				mJavaScript = new StringBuilder();
			}
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

			if (mHead != null)
			{
				clonedObject.mHead = new StringBuilder(mHead.ToString());
			}
			else
			{
				clonedObject.mHead = mHead;
			}

			if (mBody != null)
			{
				clonedObject.mBody = new StringBuilder(mBody.ToString());
			}
			else
			{
				clonedObject.mBody = mBody;
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
 
			if (mJavaScript != null)
			{
				clonedObject.mJavaScript = new StringBuilder(mJavaScript.ToString());
			}
			else
			{
				clonedObject.mJavaScript = mJavaScript;
			}

			return clonedObject;
		}

		#endregion
	}

}
