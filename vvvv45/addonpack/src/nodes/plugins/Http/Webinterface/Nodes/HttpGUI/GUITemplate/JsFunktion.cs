using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.HttpGUI.Datenobjekte
{
    public class JsFunktion
    {

        private string mFunktinName = "";
        private string mContent = "";
        private string mFunktionParameter = "";

        public string Name
        {
            get
            {
                return mFunktinName;
            }
            set
            {
                mFunktinName = value;
            }
        }

        public string Content
        {
            get
            {
                return mContent;
            }
            set
            {
                mContent  = value;
            }
        }

        public string Parameter
        {
            get
            {
                return mFunktionParameter;
            }
            set
            {
                mFunktionParameter = value;
            }
        }

        public JsFunktion()
        {
        }

        public JsFunktion(string pFunktionName, string pContent, string pFunktionParameter)
        {
            this.mFunktinName = pFunktionName;
            this.mContent = pContent;
            this.mFunktionParameter = pFunktionParameter;
        }

        
    }
}
