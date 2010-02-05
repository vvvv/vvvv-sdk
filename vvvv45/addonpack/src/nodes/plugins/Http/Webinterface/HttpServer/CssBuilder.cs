using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

using VVVV.Webinterface.Utilities;

namespace VVVV.Webinterface.HttpServer
{

    /// <summary>
    /// Creates an Css File for the requestet Page.
    /// </summary>
    class CssBuilder
    {


        private StringBuilder mCssFile = new StringBuilder();
        private SortedList<string, SortedList<string,string>> mCssSliceLists = new SortedList<string,SortedList<string,string>>();


        private List<string> mNodeIds = new List<string>();
        public List<string> mCssRules = new List<string>();
        public SortedList<string, string> mFirstSlice = new SortedList<string, string>();
        public string mFirstSliceName = "";
        SortedList<string, string> mNodeCss = new SortedList<string, string>();
        private bool mStartFlag = true;


        private string mActuallNodeId;

        public StringBuilder CssFile
        {
            get
            {
                return mCssFile;
            }
        }

        public void Reset()
        {
            mCssFile.Remove(0, mCssFile.Length);
            mCssSliceLists.Clear();
            mNodeIds.Clear();
            mCssRules.Clear();
            mFirstSlice.Clear();
            mFirstSliceName = "";
            mNodeCss.Clear();
            mNodeIds.Clear();
            mStartFlag = true;
        }


        public void AddCssSliceList(string pSliceID, SortedList<string,string> pCssProperties)
        {
            if (mCssSliceLists.ContainsKey(pSliceID) == false)
            {
                mCssSliceLists.Add(pSliceID, pCssProperties);
            }
        }


        public void AddNodeId(string pNodeId)
        {
            if (mNodeIds.Contains(pNodeId) == false)
            {
                mNodeIds.Add(pNodeId);
            }

        }

        public void Build()
        {
           
            
            foreach (KeyValuePair<string, SortedList<string, string>> pKeyPair in mCssSliceLists)
            {
                

                string WholeId = pKeyPair.Key;
                int Index = WholeId.IndexOf("Slice");

                string tNodeId = WholeId.Substring(0, Index); 

                string tSliceId = WholeId.Substring(Index, WholeId.Length - Index);


                SortedList<string, string> tProperties = pKeyPair.Value;
                CreateCssRule(tProperties, tNodeId, tSliceId);
            }



            mCssRules.Add(BuildFirstSlice());
            mCssRules.Add(BuildNodeRule());
            foreach (string pPair in mCssRules)
            {
                mCssFile.Append(pPair + Environment.NewLine);
            }
        }



        private void CreateCssRule(SortedList<string, string> pProperties, string pNodeId, string pSliceID)
        {

            if (pNodeId != mActuallNodeId)
            {
                if (mStartFlag)
                {
                    mStartFlag = false;
                    mActuallNodeId = pNodeId;
                    mFirstSlice = pProperties;
                    mFirstSliceName = pSliceID;
                    return;
                }
                else
                {
                    mCssRules.Add(BuildNodeRule());
                    mCssRules.Add(BuildFirstSlice());
                    mActuallNodeId = pNodeId;
                    mFirstSlice = pProperties;
                    mFirstSliceName = pSliceID;
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


                Rule tSliceCss = new Rule(pNodeId + "." + pSliceID, Rule.SelectorType.Class);

                foreach (KeyValuePair<string, string> pPair in tCleanList)
                {
                    tSliceCss.AddProperty(new Property(pPair.Key, pPair.Value));
                }

                mCssRules.Add(tSliceCss.Text);
            }
        }




        private SortedList<string, string> compareLists(SortedList<string, string> pListToClean, SortedList<string, string> pReferenzList)
        {
            SortedList<string, string> tCleanList = new SortedList<string, string>(pListToClean);

            foreach (KeyValuePair<string, string> pPair in pListToClean)
            {
                if (pReferenzList.ContainsKey(pPair.Key))
                {
                    if (pReferenzList[pPair.Key] == pPair.Value)
                    {
                        if (mNodeCss.ContainsKey(pPair.Key) == false)
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


            if (mNodeCss.Count > 0)
            {
                foreach (KeyValuePair<string, string> pPair in mNodeCss)
                {
                    tNodeRule.AddProperty(new Property(pPair.Key, pPair.Value));

                }
            }
            return tNodeRule.Text;
        }




        private string BuildFirstSlice()
        {
            Rule tFirstSlice = new Rule(mActuallNodeId + "." + mFirstSliceName, Rule.SelectorType.Class);

            SortedList<string, string> tCleanList = compareLists(mFirstSlice, mNodeCss);

            foreach (KeyValuePair<string, string> pPair in tCleanList)
            {
                tFirstSlice.AddProperty(new Property(pPair.Key, pPair.Value));

            }

            return tFirstSlice.Text;
        }
    }
}
