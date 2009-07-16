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
                //mCssRules.Add(BuildFirstSlice());
                //StringBuilder tCssFile = new StringBuilder();
                //mCssRules.Add(BuildNodeRule());

                //foreach (string pPair in mCssRules)
                //{
                //    tCssFile.Append(pPair + Environment.NewLine);
                //}

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
            mPage = new Page(true);
            mJsFile.Clear();
            mCssBuilder.Reset();

            //Build
            Build(pGuiElemente);
        }


        public void Build(List<GuiDataObject> pGuiElemente)
        {

            foreach (GuiDataObject pElement in pGuiElemente)
            {
                Tag tBody = BuildHtmlFrame(pElement);
                mPage.Body.Insert(tBody);
                //CreateCssRule(tCssProperties,pElement.NodeId, pElement.SliceId);
            }


        }


        public Tag BuildHtmlFrame(GuiDataObject pGuiObject)
        {

            SortedList<string, string> tTransform = new SortedList<string, string>(pGuiObject.Transform);
            SortedList<string, string> tCssProperties = new SortedList<string, string>(pGuiObject.CssProperties);

            foreach (KeyValuePair<string, string> KeyPair in tTransform)
            {
                tCssProperties.Add(KeyPair.Key, KeyPair.Value);
            }

            mCssBuilder.AddCssSliceList(pGuiObject.SliceId, tCssProperties);
            mCssBuilder.AddNodeId(pGuiObject.NodeId);

            if(pGuiObject.GuiUpstreamList != null)
            {
                

                foreach(GuiDataObject pObjekt in pGuiObject.GuiUpstreamList)
                {
                    Tag tTag = BuildHtmlFrame(pObjekt);
                    pGuiObject.Tag.Insert(tTag);
                }

                
            }

            return pGuiObject.Tag;
        }


  

    }
}
