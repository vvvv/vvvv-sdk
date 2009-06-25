using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI.Datenobjekte
{
    abstract public class BaseDatenObjekt
    {



        #region Field Declaration

        private SortedList<int, SortedList<string, string>> mHtmlAttribute = new SortedList<int, SortedList<string, string>>();
        private SortedList<string, string> mCssProperties = new SortedList<string, string>();
        private List<BaseDatenObjekt> mGuiElementListe = new List<BaseDatenObjekt>();
        private string mID;
        private string mType;
        private Tag mGuiType;
        private int mSliceCount  = 1;
        private int mSliceNumber;
        private string mClass = "";
        private JsFunktion mJsFunktion = new JsFunktion();
        private bool mCreatJsFunktion = true;

        #endregion Filed Declaration




        #region Properties


        public int SliceCount
        {
            get
            {
                return mSliceCount;
            }
            set
            {
                mSliceCount = value;
            }
        }

        public int GuiDepth
        {
            get
            {
                return mGuiElementListe.Count;
            }
        }

        public string ID
        {
            get
            {
                return mID;
            }
            set
            {
                mID = value;
            }
        }

        public string Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
            }
        }

        public Tag GuiType
        {
            get
            {
                return mGuiType;
            }
            set
            {
                mGuiType = value;
            }
        }


        public List<BaseDatenObjekt> GuiObjektListe
        {
            get
            {
                return mGuiElementListe;
            }
            set
            {
                mGuiElementListe = value;
            }
        }

        public int SliceNumber
        {
            get
            {
                return mSliceNumber;
            }
            set
            {
                mSliceNumber = value;
            }
        }

        public string Class
        {
            get
            {
                return mClass;
            }
            set
            {
                mClass = value;
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

        public JsFunktion JsFunktion
        {
            get
            {
                return mJsFunktion;
            }
            set
            {
                mJsFunktion = value;
            }
        }

        public bool CreateJsFuntkion
        {
            get
            {
                return mCreatJsFunktion;
            }
            set
            {
                mCreatJsFunktion = value;
            }
        }

        #endregion Properties



        public BaseDatenObjekt(string pId, string pType, int SliceNumber)
        {

        }


        #region public methods



        public void AddHtmlAttribute(int pIndex, SortedList<string,string> pHtmlAttr)
        {
            if (mHtmlAttribute.ContainsKey(pIndex))
            {
                mHtmlAttribute.Remove(pIndex);
            }
            else
            {
                mHtmlAttribute.Add(pIndex, pHtmlAttr);
            }
            
        }

        public void GetHtmlAttribute(int pIndex, out SortedList<string, string> pHtmlAttr)
        {
            try
            {
                SortedList<string, string> tHtmlAttr;
                mHtmlAttribute.TryGetValue(pIndex, out tHtmlAttr);
                pHtmlAttr = tHtmlAttr;
            }
            catch
            {
                SortedList<string, string> tHtmlAttrEmpty  = new SortedList<string,string>();
                pHtmlAttr = tHtmlAttrEmpty;
                //Debug.WriteLine( String.Format("Index {0} not found Method GetHTMLAttribute Class BAseDatenObjket",pIndex.ToString()));
            }
        }






        public void AddGuiObjekt(int Index, BaseDatenObjekt pGuiObjekt)
        {
                mGuiElementListe.Add(pGuiObjekt);
        
        }

        public void ClearGuiElementenListe()
        {
            mGuiElementListe.Clear();
        }

        #endregion public Methods


    }
}
