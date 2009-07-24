using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using VVVV.Utils;
using VVVV.Utils.VColor;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Utils.VMath;
using VVVV.Webinterface.Data;
using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    class Popup : BaseGUINode, IPlugin
    {


        #region field declaration

        private IColorIn FBackgroundColor;
        private List<DatenGuiPopUp> mPopUpDaten = new List<DatenGuiPopUp>();
        private List<BaseDatenObjekt> mGuiInDaten = new List<BaseDatenObjekt>();
        private List<JsFunktion> mJsFunktionOpenDaten = new List<JsFunktion>();
        private List<JsFunktion> mJsFunktionCloseDaten = new List<JsFunktion>();
        private string mNodeId;

        #endregion field declaration




        #region Plugin Information


        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Popup";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "GUI";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Popup node for the Renderer (HTTP)";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }


        #endregion Plugin Information






        #region pin creation


        protected override void OnPluginHostSet()
        {
            FHost.CreateColorInput("Background", TSliceMode.Dynamic, TPinVisibility.True, out FBackgroundColor);
            FBackgroundColor.SetSubType(VColor.Green, false);

            FHost.CreateNodeInput("Open", TSliceMode.Dynamic, TPinVisibility.True, out FFunktionOpen);
            FFunktionOpen.SetSubType(new Guid[1] { HttpGUIFunktionIO.GUID }, HttpGUIFunktionIO.FriendlyName);

            FHost.CreateNodeInput("Close", TSliceMode.Dynamic, TPinVisibility.True, out FFunkttionClose);
            FFunkttionClose.SetSubType(new Guid[1] { HttpGUIFunktionIO.GUID }, HttpGUIFunktionIO.FriendlyName);

            FHost.CreateNodeInput("Http GUI", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            mNodeId = "Popup" + GetNodeID();
        }


        #endregion pin creation






        #region Node IO


        public override void GetDatenObjekt(int Index, out BaseDatenObjekt GuiDaten)
        {
            ////Debug.WriteLine("Enter GetdatenObjekt" + mNodeId);
            GuiDaten = mPopUpDaten[Index];
        }

        public override void GetFunktionObjekt(int Index, out JsFunktion FunktionsDaten)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion Node IO





        #region Main Loop


        protected override void OnConfigurate(IPluginConfig Input)
        {
            //
        }

        protected override void OnEvaluate(int SpreadMax)
        {
            ////Debug.WriteLine("Enter OnEvaluate");

            int[] tSliceCount = { FBackgroundColor.SliceCount, FTransformIn.SliceCount};
            Array.Sort(tSliceCount);
            int ArrayLength = tSliceCount.Length - 1;
            FHttpGuiOut.SliceCount = tSliceCount[ArrayLength];
            FHttpGuiOut.SliceCount = SpreadMax;



            # region Upstream

            //Upstream to Gui Elemente
            int usS;

            if (FUpstreamInterface != null)
            {
                FHttpGuiOut.SliceCount = tSliceCount[ArrayLength];
                mGuiInDaten.Clear();

                for (int i = 0; i < FHttpGuiIn.SliceCount; i++)
                {
                    //get upstream slice index
                    
                    FHttpGuiIn.GetUpsreamSlice(i, out usS);

                    BaseDatenObjekt tGuiDaten;
                    FUpstreamInterface.GetDatenObjekt(i, out tGuiDaten);

                    if (tGuiDaten != null)
                    {
                        mGuiInDaten.Add(tGuiDaten);
                    }
                }
            }

            int usSFunktionOpen;

            if (FUpstreamFunktionOpen != null)
            {
                mJsFunktionOpenDaten.Clear();
                for (int i = 0; i < tSliceCount[ArrayLength]; i++)
                {
                    FFunktionOpen.GetUpsreamSlice(i, out usSFunktionOpen);

                    JsFunktion tJsFunktion;
                    FUpstreamFunktionOpen.GetFunktionObjekt(i, out tJsFunktion);

                    if (tJsFunktion != null)
                    {
                        mJsFunktionOpenDaten.Add(tJsFunktion);
                    }
                    else
                    {
                        mJsFunktionOpenDaten.Add(new JsFunktion());
                    }
                }
            }

            int usSFunktionClose;
            if (FUpstreamFunktionClose != null)
            {
                mJsFunktionCloseDaten.Clear();

                for (int i = 0; i < tSliceCount[ArrayLength]; i++)
                {
                    FFunkttionClose.GetUpsreamSlice(i, out usSFunktionClose);

                    JsFunktion tJsFunktion;
                    FUpstreamFunktionClose.GetFunktionObjekt(i, out tJsFunktion);

                    if (tJsFunktion != null)
                    {
                        mJsFunktionCloseDaten.Add(tJsFunktion);
                    }
                    else
                    {
                        mJsFunktionCloseDaten.Add(new JsFunktion());
                    }
                }
            }




            #endregion Upstream







            if (FBackgroundColor.PinIsChanged || FTransformIn.PinIsChanged)
            {

                mPopUpDaten.Clear();


                //FButtonMode.GetString(0,out currentSelection);
                //mButtonDaten.Mode = currentSelection;

                for (int i = 0; i < tSliceCount[ArrayLength]; i++)
                {

                    string tSliceId = mNodeId + "/" + i;
                    DatenGuiPopUp tPopupDatenObjekt = new DatenGuiPopUp(tSliceId, "PopUp", i);
                    tPopupDatenObjekt.Class = tSliceId.Replace("/", "");
                    tPopupDatenObjekt.SliceNumber = i;


                    //State Pin
                    RGBAColor currentColorSlice;
                    FBackgroundColor.GetColor(i, out currentColorSlice);
                    

                    // Transform Pin
                    Matrix4x4 tMatrix = new Matrix4x4();
                    FTransformIn.GetMatrix(i, out tMatrix);

                    SortedList<string, string> tTransform;
                    GetTransformation(tMatrix, out tTransform);
                    
                    
                    //Css Properties
                    SortedList<string, string> tStyleProperties;
                    tStyleProperties = tTransform;
                    tStyleProperties.Add("background-color", "rgb(" + Math.Round(currentColorSlice.R * 100) + "%," + Math.Round(currentColorSlice.G * 100) + "%," + Math.Round(currentColorSlice.B * 100) + "%)");
                    tStyleProperties.Add("display", "none");

                    tPopupDatenObjekt.CssProperties = tStyleProperties;
                    tPopupDatenObjekt.GuiObjektListe = mGuiInDaten;

                    if (FUpstreamFunktionOpen!= null)
                    {
                        tPopupDatenObjekt.JsFunktionOpen = mJsFunktionOpenDaten[i];
                    }
                    if (FUpstreamFunktionClose != null)
                    {
                        tPopupDatenObjekt.JsFunktionClose = mJsFunktionCloseDaten[i];
                    }
                    
                    mPopUpDaten.Add(tPopupDatenObjekt);
                }
            }
        }

        #endregion Main Loop


    }
}
