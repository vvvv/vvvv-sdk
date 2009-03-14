using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.HttpGUI.Datenobjekte
{






    class DatenGuiTextfield : BaseDatenObjekt
    {

        private string mLabel ="";
        private string mValue;

        public string Label
        {
            get
            {
                return mLabel;
            }
            set
            {
                mLabel = value;
            }
        }

        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }


        public DatenGuiTextfield(string pId, string pType, int pSliceNumber) 
            : base(pId, pType, pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
            this.SliceNumber = pSliceNumber;
        }




   }








    class DatenGuiText: BaseDatenObjekt
    {

        private string mLabel ="";

        public string Label
        {
            get
            {
                return mLabel;
            }
            set
            {
                mLabel = value;
            }
        }

        public DatenGuiText(string pId, string pType, int pSliceNumber)
            :base(pId,pType,pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
            this.SliceNumber = pSliceNumber;
        }




    }






    class DatenGuiContainer : BaseDatenObjekt
    {
        public DatenGuiContainer(string pId, string pType, int pSliceNumber)
            : base(pId, pType, pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
            this.SliceNumber = pSliceNumber;
        }
    }




    class DatenGuiImage : BaseDatenObjekt
    {
        private string mScr;
        private string mAlt;

        public string Src
        {
            get
            {
                return mScr;
            }
            set
            {
                mScr = value;
            }
        }

        public string Alt
        {
            get
            {
                return mAlt;
            }
            set
            {
                mAlt = value;
            }
        }
        
        
        public DatenGuiImage(string pId, string pType, int pSliceNumber) 
            : base(pId, pType, pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
        }


    }






    class DatenGuiSlider : BaseDatenObjekt
    {

        private string mLabel = "";
        private string mPosition = "";
        private string mOrientation = "";

        public string Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }

        public string Label
        {
            get
            {
                return mLabel;
            }
            set
            {
                mLabel = value;
            }
        }

        public string Orientation
        {
            get
            {
                return mOrientation;
            }
            set
            {
                mOrientation = value;
            }
        }

        public DatenGuiSlider(string pId, string pType, int pSliceNumber)
            : base(pId, pType, pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
        }

    }






    class DatenGuiButton : BaseDatenObjekt
    {

        private string mLabel ="";
        private string mMode;
        private string mState = "0";
        private string mOldState;

        public string Mode
        {
            get
            {
                return mMode;
            }
            set
            {
                mMode = value;
            }
        }

        public string Label
        {
            get
            {
                return mLabel;
            }
            set
            {
                mLabel = value;
            }
        }

        public string State
        {
            get
            {
                return mState;
            }
            set
            {
                mState = value;
            }
        }

        public string OldState
        {
            get
            {
                return mOldState;
            }
            set
            {
                mOldState = value;
            }
        }



        public DatenGuiButton(string pId, string pType , int pSliceNumber)
            : base(pId, pType, pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
            this.SliceNumber = pSliceNumber;
        }


    }






    class DatenGuiTwoPane : BaseDatenObjekt
    {

        private List<BaseDatenObjekt> mFixedPainGuiList;

        public List<BaseDatenObjekt> FixedPainGuiList
        {
            get
            {
                return mFixedPainGuiList;
            }
            set
            {
                mFixedPainGuiList = value;
            }
        }

        public int FixedPainDepth
        {
            get
            {
                return mFixedPainGuiList.Count;
            }
        }

        public DatenGuiTwoPane(string pId, string pType, int pSliceNumber)
            : base(pId, pType, pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
            this.SliceNumber = pSliceNumber;
        }


    }





    class DatenGuiPopUp : BaseDatenObjekt
    {
        private JsFunktion mJsFunktionOpen = new JsFunktion();
        private JsFunktion mJsFunktionClose = new JsFunktion();

        public JsFunktion JsFunktionOpen
        {
            get
            {
                return mJsFunktionOpen;
            }
            set
            {
                mJsFunktionOpen = value;
            }
        }

        public JsFunktion JsFunktionClose
        {
            get
            {
                return mJsFunktionClose;
            }
            set
            {
                mJsFunktionClose = value;
            }
        }
        
        public DatenGuiPopUp(string pId, string pType, int pSliceNumber)
            : base(pId, pType, pSliceNumber)
        {
            this.ID = pId;
            this.Type = pType;
            this.SliceNumber = pSliceNumber;
        }



    }

}
