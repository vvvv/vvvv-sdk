using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
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
            mPage = null;
            mBody = null;
            mPage = new Page(false);
            mBody = new Body();

            mJsFile.Clear();
            mCssBuilder.Reset();
            TestList.Clear();

            //Build
            mPage.Insert((Body) BuildHtmlFrame(pGuiElemente, mBody));
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

            foreach (GuiDataObject pElement in pGuiObjectIn)
            {
                AddCssPropertiesToBuilder(pElement);
                tTag.Insert(pElement.Tag);

                if (pElement.GuiUpstreamList != null)
                {
                    BuildHtmlFrame(pElement.GuiUpstreamList, pElement.Tag);
                }



            }

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
