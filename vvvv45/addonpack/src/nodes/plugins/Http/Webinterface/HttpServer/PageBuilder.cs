using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.Http.BaseNodes;

namespace VVVV.Webinterface.HttpServer
{


    /// <summary>
    /// Builds the HTML Strukt of an requested Page. 
    /// </summary>
    class PageBuilder
    {

        #region field declaration


        private Page mPage;
        List<string> mJsFileList = new List<string>();
        private CssBuilder mCssBuilder = new CssBuilder();
        private Body mBody = new Body();
        private SortedList<string, Tag> mTags = new SortedList<string, Tag>();
        List<GuiDataObject> mGuiElemente  =new List<GuiDataObject>();
        bool mBuildFlag = false;
        private Object _updatelock = new Object();
        private string FBodyExtension = "";

        public Page Page
        {
            get
            {
                Debug.WriteLine("get Page");
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
                Debug.WriteLine("Get HtmlFile");
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
            //Debug.WriteLine("Update Start");
            if (Monitor.TryEnter(_updatelock, 1000))
            {

                try
                {
                    if (mBuildFlag == false)
                    {
                        mPage = pPage;
                        mJsFileList.Clear();
                        mCssBuilder.Reset();
                        mTags.Clear();
                        FBodyExtension = "";

                        //Build
                        mGuiElemente = new List<GuiDataObject>(pGuiElemente);
                    }
                }
                finally
                {
                    Monitor.Exit(_updatelock);
                }
            }
            //Debug.WriteLine("Update Ende");
        }

        public void Build()
        {
            Monitor.Enter(_updatelock);
            mBuildFlag = true;
            //Debug.WriteLine("Start Build Vorgang");
            if (mGuiElemente.Count == 0)
            {
                Debug.WriteLine("count 0");
            }

            mPage.Body.ClearTagsInside();
            mPage.Body = (Body)BuildHtmlFrame(new List<GuiDataObject>(mGuiElemente), mPage.Body);
            mPage.Body.Insert(FBodyExtension);
            mTags.Clear();
            //Debug.WriteLine("Ende Build Vorgang");
            mCssBuilder.Build();
            mBuildFlag = false;
            Monitor.Exit(_updatelock);
        }


        private Tag BuildHtmlFrame(List<GuiDataObject> pGuiObjectIn, Tag tTag)
        {

            ////Debug.WriteLine("------------ Enter BuildHtmlFrame -------------");
            ////Debug.WriteLine("pGuiObjectIn.Count: " + pGuiObjectIn.Count.ToString());
            tTag.ClearTagsInside();

            foreach (GuiDataObject pElement in pGuiObjectIn)
            {
                //Debug.WriteLine("---------------- Enter foreachSchleife-----------------");
                //Debug.WriteLine("SliceId: " + pElement.SliceId);
                //Debug.WriteLine("tTag: " + tTag.Text + Environment.NewLine);
                ////Debug.WriteLine("tTag.Level: " + tTag.Level);
                ////Debug.WriteLine("tTag.Name: " + tTag.Name);

                string javaScript = pElement.JavaScript;
                
                if (javaScript != null && javaScript.ToString() != "")
                {
                    AddJavaScript(javaScript);
                }

                if (pElement.Tag == null)
                {
                    FBodyExtension += pElement.Body + Environment.NewLine;
                    
                }
                else
                {
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
