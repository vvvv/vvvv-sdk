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
        public SortedList<string, string> mJsFile = new SortedList<string, string>();
        private CssBuilder mCssBuilder = new CssBuilder();
        private List<string> TestList = new List<string>();
        private Body mBody = new Body();
        private SortedList<string, Tag> mTags = new SortedList<string, Tag>();

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

        public StringBuilder CssMainFile
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
                foreach (KeyValuePair<string, string> pPair in mJsFile)
                {
                    tJsFile.Append(pPair.Value + Environment.NewLine);
                }
                return tJsFile;
            }
        }

 
        #endregion field declaration





        public PageBuilder()
        {
            
        }


        public void UpdateGuiList(List<GuiDataObject> pGuiElemente)
        {

            // Reset everything
            //mPage.Body = null;
            //mPage.Head = null;
            mPage = null;
            mPage = new Page(true);

            mJsFile.Clear();
            mCssBuilder.Reset();
            TestList.Clear();
            mTags.Clear();

            //Build
            mPage.Body  = (Body) BuildHtmlFrame(pGuiElemente, mPage.Body);
            mCssBuilder.Build();
        }


        public void Build(List<GuiDataObject> pGuiElemente)
        {


            
            //foreach (GuiDataObject pElement in pGuiElemente)
            //{
            //    mPage.Body.Insert(BuildHtmlFrame(pElement));
            //    //CreateCssRule(tCssProperties,pElement.NodeId, pElement.SliceId);
            //}


        }


        public Tag BuildHtmlFrame(List<GuiDataObject> pGuiObjectIn, Tag tTag)
        {

            Debug.WriteLine("------------ Enter BuildHtmlFrame -------------");
            Debug.WriteLine("pGuiObjectIn.Count: " + pGuiObjectIn.Count.ToString());
            

            foreach (GuiDataObject pElement in pGuiObjectIn)
            {
                Debug.WriteLine("---------------- Enter foreachSchleife-----------------");
                Debug.WriteLine("SliceId: " + pElement.SliceId);
                Debug.WriteLine("tTag.Level: " + tTag.Level);
                Debug.WriteLine("tTag.Name: " + tTag.Name);

                
                if (mTags.ContainsKey(pElement.SliceId) == false)
                {
                    mTags.Add(pElement.SliceId, pElement.Tag);
                    AddCssPropertiesToBuilder(pElement);

                    if (pElement.GuiUpstreamList != null)
                    {
                        AddCssPropertiesToBuilder(pElement);
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

            Debug.WriteLine("tTag: " + tTag.Text + Environment.NewLine);
            return tTag;
        }

        private void AddCssPropertiesToBuilder(GuiDataObject pObject)
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
            TestList.Add(pObject.SliceId);
            mCssBuilder.AddNodeId(pObject.NodeId);
        }


  

    }
}
