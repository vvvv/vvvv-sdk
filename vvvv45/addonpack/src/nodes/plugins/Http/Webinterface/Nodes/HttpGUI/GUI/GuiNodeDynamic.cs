using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI;
using VVVV.Utils.VMath;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Webinterface;
using System.Diagnostics;
using System.Globalization;


namespace VVVV.Nodes.HttpGUI
{
    public abstract class GuiNodeDynamic : IHttpGUIIO, IPluginConnections
    {



        #region field Definition


        //Host
        protected IPluginHost FHost;

        // Input Pins
        public INodeOut FHttpGuiOut;

        public INodeIn FHttpGuiIn;
        public IHttpGUIIO FUpstreamHttpGuiIn;

        public INodeIn FHttpStyleIn;
        public IHttpGUIStyleIO FUpstreamStyle;

        public ITransformIn FTransformIn;


        //Required Members
        SortedList<int, SortedList<string, string>> mCssPropertiesSpread = new SortedList<int, SortedList<string, string>>();
        SortedList<int, SortedList<string, string>> mCssTransformSpread = new SortedList<int, SortedList<string, string>>();
        List<GuiDataObject> mGuiDataList = new List<GuiDataObject>();


        #endregion field Definition






        #region abstract Methods

        protected abstract void OnSetPluginHost();
        protected abstract void OnConfigurate(IPluginConfig Input);
        protected abstract void OnEvaluate(int SpreadMax);
        
        
        #endregion abstract Methods






        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            this.FHost.CreateNodeInput("Input GUI", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);

            FHost.CreateNodeOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiOut);
            FHttpGuiOut.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
            FHttpGuiOut.SetInterface(this);

            //Input Pins 
            FHost.CreateNodeInput("Input CSS", TSliceMode.Dynamic, TPinVisibility.True, out FHttpStyleIn);
            FHttpStyleIn.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);

            this.OnSetPluginHost();	    	
        }


        public virtual bool AutoEvaluate
        {
            get { return false; }
        }

        #endregion pin creation







        #region IMyNodeIO

        public void GetDatenObjekt(int Index, out List<GuiDataObject> GuiDaten)
        {
            GuiDaten = mGuiDataList;
        }


        public void ConnectPin(IPluginIO Pin)
        {
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            if (Pin == FHttpGuiIn)
            {
                if (FHttpGuiIn != null)
                {
                    INodeIOBase usI;
                    FHttpGuiIn.GetUpstreamInterface(out usI);
                    FUpstreamHttpGuiIn = usI as IHttpGUIIO;
                }

            }
            else if (Pin == FHttpStyleIn)
            {
                INodeIOBase usIHttpStyle;
                FHttpStyleIn.GetUpstreamInterface(out usIHttpStyle);
                FUpstreamStyle = usIHttpStyle as IHttpGUIStyleIO;
            }
        }



        public void DisconnectPin(IPluginIO Pin)
        {
            //reset the cached reference to the upstream interface when the NodeInput is being disconnected
            if (Pin == FHttpGuiIn)
            {
                FUpstreamHttpGuiIn = null;
            }
            else if (Pin == FHttpStyleIn)
            {
                FUpstreamStyle = null;
            }

        }

        #endregion NodeIO






        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            this.OnConfigurate(Input);
        }

        #endregion







        #region Evaluate

        public void Evaluate(int SpreadMax)
        {

            #region Check Gui List
            if (mGuiDataList.Count > SpreadMax)
            {
                mGuiDataList.RemoveRange(SpreadMax, mGuiDataList.Count - SpreadMax);
                mGuiDataList.Capacity = SpreadMax;
            }
            else
            {

                for (int i = 0; i < SpreadMax; i++)
                {
                    if (mGuiDataList.Count == i)
                    {
                        mGuiDataList.Insert(i,new GuiDataObject());
                    }
                }
            }

            mGuiDataList.TrimExcess();


            #endregion Check Gui List



            #region Upstream Gui Elements

            int usSGuiIn;
            if (FUpstreamHttpGuiIn != null)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    //get upstream slice index
                    FHttpGuiIn.GetUpsreamSlice(i, out usSGuiIn);

                    List<GuiDataObject> tGuiList;
                    FUpstreamHttpGuiIn.GetDatenObjekt(usSGuiIn, out tGuiList);
                    mGuiDataList[i].GuiUpstreamList = tGuiList;
                }
            }

            #endregion Upstream Gui Elements



            # region Upstream Css Properties

            int usSStyle;
            if (FUpstreamStyle != null)
            {
                string NodePath;
                FHost.GetNodePath(false, out NodePath);
                Debug.WriteLine("Enter Css Upstream Gui Node: " + NodePath);

                mCssPropertiesSpread.Clear();
                for (int i = 0; i < SpreadMax; i++)
                {
                    //get upstream slice index

                    FHttpStyleIn.GetUpsreamSlice(i, out usSStyle);

                    SortedList<string, string> tSliceCssPropertie;
                    FUpstreamStyle.GetCssProperties(usSStyle, out tSliceCssPropertie);

                    mGuiDataList[i].CssProperties = tSliceCssPropertie;
                    
                }
            }

            #endregion Upstream Css Propeties



            #region Transform Pin

            if (FTransformIn.PinIsChanged)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    Matrix4x4 tMatrix;

                    FTransformIn.GetMatrix(i, out tMatrix);

                    SortedList<string, string> tTransformSlice = new SortedList<string, string>();
                    tTransformSlice.Add("position", "absolute");

                    double tWidth = HTMLToolkit.MapScale(tMatrix.m11, 0, 2, 0, 100);
                    double tHeight = HTMLToolkit.MapScale(tMatrix.m22, 0, 2, 0, 100);

                    tTransformSlice.Add("width", ReplaceComma(string.Format("{0:0.0}", Math.Round(tWidth, 1)) + "%"));
                    tTransformSlice.Add("height", ReplaceComma(string.Format("{0:0.0}", Math.Round(tHeight, 1)) + "%"));

                    double tTop = HTMLToolkit.MapTransform(tMatrix.m42, 1, -1, 0, 100, tHeight);
                    double tLeft = HTMLToolkit.MapTransform(tMatrix.m41, -1, 1, 0, 100, tWidth);

                    tTransformSlice.Add("top", ReplaceComma(string.Format("{0:0.0}", Math.Round(tTop, 1)) + "%"));
                    tTransformSlice.Add("left", ReplaceComma(string.Format("{0:0.0}", Math.Round(tLeft, 1)) + "%"));


                    tTransformSlice.Add("z-index", Convert.ToString(Math.Round(tMatrix.m43)));

                    mGuiDataList[i].Transform = tTransformSlice;
                }
            }

            #endregion Transform Pin



            this.OnEvaluate(SpreadMax);

        }

        #endregion Evaluate






        #region Node Information

        public string GetNodeID()
        {
            string tPath;
            FHost.GetNodePath(true, out tPath);
            return tPath;
        }

        public void GetTransformation(Matrix4x4 pMatrix, out SortedList<string, string> pTransform)
        {


            SortedList<string, string> tStyles = new SortedList<string, string>();
            tStyles.Add("position", "absolute");

            double tWidth = HTMLToolkit.MapScale(pMatrix.m11, 0, 2, 0, 100);
            double tHeight = HTMLToolkit.MapScale(pMatrix.m22, 0, 2, 0, 100);

            tStyles.Add("width", ReplaceComma(string.Format("{0:0.0}", Math.Round(tWidth, 1)) + "%"));
            tStyles.Add("height", ReplaceComma(string.Format("{0:0.0}", Math.Round(tHeight, 1)) + "%"));

            double tTop = HTMLToolkit.MapTransform(pMatrix.m42, 1, -1, 0, 100, tHeight);
            double tLeft = HTMLToolkit.MapTransform(pMatrix.m41, -1, 1, 0, 100, tWidth);

            tStyles.Add("top", ReplaceComma(string.Format("{0:0.0}", Math.Round(tTop, 1)) + "%"));
            tStyles.Add("left", ReplaceComma(string.Format("{0:0.0}", Math.Round(tLeft, 1)) + "%"));


            tStyles.Add("z-index", Convert.ToString(Math.Round(pMatrix.m43)));
            pTransform = tStyles;

        }

        public string ReplaceComma(string tParameter)
        {
            return tParameter.Replace(",", ".");
        }

        public string GetNodeIdformSliceId(string pSliceId)
        {
            char[] delimiter ={ '/' };
            string[] Patches = pSliceId.Split(delimiter);
            int tPatchDepth = Patches.Length;

            int LengthLastPatch = Patches[tPatchDepth - 1].Length;
            return pSliceId.Substring(0, pSliceId.Length - LengthLastPatch - 1);
        }


        public string GetSliceFormSliceId(string pSliceId)
        {
            char[] delimiter ={ '/' };
            string[] Patches = pSliceId.Split(delimiter);
            int tPatchDepth = Patches.Length;
            return Patches[Patches.Length - 1];
        }

        #endregion Node Information






    }
}
