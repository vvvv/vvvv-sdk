using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using VVVV.Nodes.HttpGUI;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.Http
{
    class PageBuilder
    {

        #region field declaration


        private Page mPage;
        List<string> mJsFileList = new List<string>();
        private CssBuilder mCssBuilder = new CssBuilder();
        private Body mBody = new Body();
        private SortedList<string, Tag> mTags = new SortedList<string, Tag>();
        List<GuiDataObject> mGuiElemente;

        public Page Page
        {
            get
            {
                return mPage;
            }
        }

        public Body Body
        {
            get
            {
                return mPage.Body;
            }
        }

        public Head Head
        {
            get
            {
                return mPage.Head;
            }
        }

        public string HtmlFile
        {
            get
            {
                return mPage.Text;
            }
        }

        public StringBuilder CssFile
        {
            get
            {
                return mCssBuilder.CssFile;
            }
        }

        public StringBuilder JsFile
        {
            get
            {
                StringBuilder tJsFile = new StringBuilder();
                foreach (string pScript in mJsFileList)
                {
                    tJsFile.Append(pScript + Environment.NewLine);
                }
                return tJsFile;
            }
        }

 
        #endregion field declaration


        public PageBuilder()
        {
            
        }


        public void UpdateGuiList(List<GuiDataObject> pGuiElemente, Page pPage)
        {

            // Reset everything
            //mPage.Body = null;
            //mPage.Head = null;
           
            mPage = pPage;
            mJsFileList.Clear();
            mCssBuilder.Reset();
            mTags.Clear();

            //Build
            mGuiElemente = new List<GuiDataObject>(pGuiElemente);
        }

        public void Build()
        {
            mPage.Body = (Body)BuildHtmlFrame(mGuiElemente, mPage.Body);
            mCssBuilder.Build();
        }



        private Tag BuildHtmlFrame(List<GuiDataObject> pGuiObjectIn, Tag tTag)
        {

            ////Debug.WriteLine("------------ Enter BuildHtmlFrame -------------");
            ////Debug.WriteLine("pGuiObjectIn.Count: " + pGuiObjectIn.Count.ToString());
            

            foreach (GuiDataObject pElement in pGuiObjectIn)
            {
                //Debug.WriteLine("---------------- Enter foreachSchleife-----------------");
                //Debug.WriteLine("SliceId: " + pElement.SliceId);
                //Debug.WriteLine("tTag: " + tTag.Text + Environment.NewLine);
                ////Debug.WriteLine("tTag.Level: " + tTag.Level);
                ////Debug.WriteLine("tTag.Name: " + tTag.Name);

                if (pElement.JavaScript != null)
                {
                    if (pElement.JavaScript.ToString() != "")
                    {
                        AddJavaScript(pElement.JavaScript.ToString());
                    }
                }

                Tag mtTag = pElement.Tag;

                if (mTags.ContainsKey(pElement.SliceId) == false)
                {
                    mTags.Add(pElement.SliceId, pElement.Tag);
                    AddCssProperties(pElement);

                    if (pElement.GuiUpstreamList != null)
                    {
                        AddCssProperties(pElement);
                       
                        tTag.Insert(BuildHtmlFrame(pElement.GuiUpstreamList, pElement.Tag));
                    }
                    else
                    {
                            tTag.Insert(pElement.Tag);
                    }
                }
                else
                {
                    Tag tDummyTag;
                    mTags.TryGetValue(pElement.SliceId, out tDummyTag);

                        tTag.Insert(tDummyTag);
                    
                }
            }

            //Debug.WriteLine("---------------- Exit foreachSchleife-----------------");
            //Debug.WriteLine("tTag: " + tTag.Text + Environment.NewLine);
            return tTag;
        }

        public void AddJavaScript(string pScript)
        {
            if(mJsFileList.Contains(pScript) == false)
            {
                mJsFileList.Add(pScript);
            }
        }



        private void AddCssProperties(GuiDataObject pObject)
        {
            SortedList<string, string> tTransform = new SortedList<string, string>();

            if (pObject.Transform != null)
            {
                tTransform = new SortedList<string, string>(pObject.Transform);
            }

            SortedList<string, string> tCssProperties = new SortedList<string, string>();
            if (pObject.CssProperties != null)
            {
                tCssProperties = new SortedList<string, string>(pObject.CssProperties);
            }

            foreach (KeyValuePair<string, string> KeyPair in tTransform)
            {
                tCssProperties.Add(KeyPair.Key, KeyPair.Value);
            }

            mCssBuilder.AddCssSliceList(pObject.SliceId, tCssProperties);
            mCssBuilder.AddNodeId(pObject.NodeId);
        }




  

    }
}
