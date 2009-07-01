using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.Http
{
    class PageBuilder
    {

        #region field declaration


        protected SortedList<int, BaseDatenObjekt> mGuiDatenObjekte;
        protected Page mPage = new Page(true);


        public SortedList<string, string> mCssFile = new SortedList<string, string>();
        public SortedList<string, string> mJsFile = new SortedList<string, string>();
        public SortedList<string, string> mDocumentReady = new SortedList<string, string>();

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
                StringBuilder tCssFile = new StringBuilder();
                foreach (KeyValuePair<string, string> pPair in mCssFile)
                {
                    tCssFile.Append(pPair.Value + Environment.NewLine);
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

        public PageBuilder(SortedList<int,BaseDatenObjekt> pGuiElemente)
        {
            this.mGuiDatenObjekte = pGuiElemente;
            buildHead();
        }


        public void buildHead()
        {
            foreach (KeyValuePair<int,BaseDatenObjekt> pKeyPair in mGuiDatenObjekte)
            {
                BaseDatenObjekt pObject = pKeyPair.Value;
                Type myObject = pObject.GetType();
                MethodInfo[] myMethodInfo = myObject.GetMethods();
            }
        }
    }
}
