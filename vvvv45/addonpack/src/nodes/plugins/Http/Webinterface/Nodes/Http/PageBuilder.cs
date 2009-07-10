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


        public List<string> mCssRules = new List<string>();
        public SortedList<string,string> mSliceListBefor = new SortedList<string,string>();
        public string mFirstSliceName = "";
        SortedList<string, string> mNodeCss = new SortedList<string, string>();
        public SortedList<string, string> mJsFile = new SortedList<string, string>();
       
        private string mActuallNodeId;

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
                Rule tFirstSlice = new Rule(mActuallNodeId, Rule.SelectorType.Class);
                

                foreach (KeyValuePair<string, string> pPair in mNodeCss)
                {
                    tNodeRule.AddProperty(new Property(pPair.Key, pPair.Value));

                }


                StringBuilder tCssFile = new StringBuilder();
                mCssRules.Add(BuildNodeRule());

                foreach (string pPair in mCssRules)
                {
                    tCssFile.Append(pPair + Environment.NewLine);
                }
                return tCssFile;
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
            mPage = new Page(true);
            mNodeCss.Clear();
            mCssRules.Clear();
            mJsFile.Clear();
            Build(pGuiElemente);
        }


        public void Build(List<GuiDataObject> pGuiElemente)
        {
            foreach (GuiDataObject pElement in pGuiElemente)
            {


                mPage.Head.Insert(pElement.Head);
                mPage.Body.Insert(pElement.Body);

                CreateCssRule(pElement.CssProperties,pElement.NodeId, pElement.SliceId);

                //if (pElement.GuiUpstreamList == null)
                //{
                //    Type myObject = pElement.GetType();
                //    MethodInfo[] myMethodInfo = myObject.GetMethods();
                //}
                //else
                //{
                //    Build(pElement.GuiUpstreamList);
                //}
                
            }
        }


        private void CreateCssRule(SortedList<string,string> pProperties, string pNodeId,string pSliceID)
        {

            SortedList<string, string> tCleanList = new SortedList<string, string>(pProperties);

            if (pNodeId != mActuallNodeId)
            {
                

                if (mNodeCss.Count == 0)
                {
                    Rule tNodeRule = new Rule(pNodeId, Rule.SelectorType.Class);
                    mActuallNodeId = pNodeId;
                    mSliceListBefor = pProperties;
                    mFirstSliceName = pSliceID;
                    return;
                }
                else
                {
                    mCssRules.Add(BuildNodeRule());
                    mActuallNodeId = pNodeId;
                    mNodeCss.Clear();
                }
            }
            else
            {
                foreach (KeyValuePair<string, string> pPair in pProperties)
                {
                    if(mSliceListBefor.ContainsKey(pPair.Key))
                    {
                        if(mSliceListBefor[pPair.Key] == pPair.Value)
                        {
                            if (mNodeCss.ContainsKey(pPair.Key) == false)
                            {
                                mNodeCss.Add(pPair.Key, pPair.Value);
                            }
                            tCleanList.Remove(pPair.Key);
                        }
                    }
                }
            }
            
            Rule tSliceCss = new Rule(pSliceID, Rule.SelectorType.Class);

            foreach (KeyValuePair<string, string> pPair in tCleanList)
            {
                tSliceCss.AddProperty(new Property(pPair.Key, pPair.Value));
            }

            mCssRules.Add(tSliceCss.Text);
        }


        private string BuildNodeRule()
        {
            Rule tNodeRule = new Rule(mActuallNodeId, Rule.SelectorType.Class);

            foreach (KeyValuePair<string, string> pPair in mNodeCss)
            {
                tNodeRule.AddProperty(new Property(pPair.Key, pPair.Value));

            }

            return tNodeRule.Text;
        }

    }
}
