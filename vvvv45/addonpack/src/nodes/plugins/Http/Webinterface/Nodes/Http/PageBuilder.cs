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
        public SortedList<string,string> mFirstSlice = new SortedList<string,string>();
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
                mCssRules.Add(BuildFirstSlice());
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
            mActuallNodeId = "";
            mNodeCss.Clear();
            mCssRules.Clear();
            mJsFile.Clear();
            Build(pGuiElemente);
        }


        public void Build(List<GuiDataObject> pGuiElemente)
        {

            bool LastSliceFlag = false;
            int LastSliceCounter = 0;

            foreach (GuiDataObject pElement in pGuiElemente)
            {
                LastSliceCounter++;
                mPage.Head.Insert(pElement.Head);
                mPage.Body.Insert(pElement.Body);

                SortedList<string, string> tTransform = pElement.Transform;
                SortedList<string, string> tCssProperties = pElement.CssProperties;

                foreach (KeyValuePair<string, string> KeyPair in tTransform)
                {
                    tCssProperties.Add(KeyPair.Key, KeyPair.Value);

                }


                if (LastSliceCounter == pGuiElemente.Count)
                {
                    LastSliceFlag = true;
                }

                CreateCssRule(tCssProperties,pElement.NodeId, pElement.SliceId, LastSliceFlag);



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


        private void CreateCssRule(SortedList<string,string> pProperties, string pNodeId,string pSliceID, bool LastSliceFlag)
        {

            if (pNodeId != mActuallNodeId)
            {
                if (mNodeCss.Count == 0)
                {
                    mActuallNodeId = pNodeId;
                    mFirstSlice = pProperties;
                    mFirstSliceName = pSliceID;
                    return;
                }
            }
            else
            {
                SortedList<string, string> tCleanList;

                if (mNodeCss.Count == 0)
                {
                    tCleanList = compareLists(pProperties, mFirstSlice);
                }
                else
                {
                    tCleanList = compareLists(pProperties, mNodeCss);
                }


                Rule tSliceCss = new Rule(pSliceID + "." + pNodeId, Rule.SelectorType.Class);

                foreach (KeyValuePair<string, string> pPair in tCleanList)
                {
                    tSliceCss.AddProperty(new Property(pPair.Key, pPair.Value));
                }

                mCssRules.Add(tSliceCss.Text);
            }



            
        }


        private SortedList<string,string> compareLists(SortedList<string,string> pListToClean, SortedList<string,string> pReferenzList)
        {
            SortedList<string, string> tCleanList = new SortedList<string, string>(pListToClean);

            foreach (KeyValuePair<string, string> pPair in pListToClean)
            {
                if (pReferenzList.ContainsKey(pPair.Key))
                {
                    if (pReferenzList[pPair.Key] == pPair.Value)
                    {
                        if(mNodeCss.ContainsKey(pPair.Key) == false)
                        {
                           mNodeCss.Add(pPair.Key, pPair.Value);
                        }

                        tCleanList.Remove(pPair.Key);
                    }
                }
                else
                {
                    
                }
            }
            return tCleanList;
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


        private string BuildFirstSlice()
        {
             Rule tFirstSlice = new Rule(mFirstSliceName + "." + mActuallNodeId, Rule.SelectorType.Class);

            SortedList<string, string> tCleanList = compareLists(mFirstSlice, mNodeCss);

            foreach (KeyValuePair<string, string> pPair in tCleanList)
            {
                tFirstSlice.AddProperty(new Property(pPair.Key, pPair.Value));

            }

            return tFirstSlice.Text;
        }

    }
}
