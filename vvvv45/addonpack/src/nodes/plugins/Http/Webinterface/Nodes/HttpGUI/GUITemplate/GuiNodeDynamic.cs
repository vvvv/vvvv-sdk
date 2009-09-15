using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI;
using VVVV.Utils.VMath;
using VVVV.Webinterface.Utilities;
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
        private IEnumIn FPositionType;
        private IEnumIn FBasingPoint;
        public INodeOut FHttpGuiOut;

        public INodeIn FHttpGuiIn;
        public IHttpGUIIO FUpstreamHttpGuiIn;

        public INodeIn FHttpStyleIn;
        public IHttpGUIStyleIO FUpstreamStyle;

        public ITransformIn FTransformIn;


        //Required Members
        public List<GuiDataObject> mGuiDataList = new List<GuiDataObject>();
        public int mSpreadMax = 0;
        public bool mChangedSpreadSize = true;
        private string mNodePath;
        private string mActualNodePath;
        private SortedList<int,string> mSavedResponses = new SortedList<int,string>();
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();



        #endregion field Definition








        #region abstract Methods

        protected abstract void OnSetPluginHost();
        //protected abstract void OnConfigurate(IPluginConfig Input);
        protected abstract void OnEvaluate(int SpreadMax);


        #endregion abstract Methods







        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            FHost.GetNodePath(true, out mNodePath);

            this.OnSetPluginHost();

            //Input Pins 
            this.FHost.CreateNodeInput("Input GUI", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);

            FHost.UpdateEnum("PositionType", "absolute", new string[] { "absolute", "fixed ", "relative ", "static " });
            FHost.CreateEnumInput("Positiontype", TSliceMode.Single, TPinVisibility.True, out FPositionType);
            FPositionType.SetSubType("PositionType");

            FHost.UpdateEnum("BasingPoint", "Center", new string[] { "Center", "TopLeft", "TopRight", "BottomLeft", "BottomRight" });
            FHost.CreateEnumInput("Basing Point", TSliceMode.Single, TPinVisibility.True, out FBasingPoint);
            FBasingPoint.SetSubType("BasingPoint");

            FHost.CreateNodeInput("Input CSS", TSliceMode.Dynamic, TPinVisibility.True, out FHttpStyleIn);
            FHttpStyleIn.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);

            FHost.CreateNodeOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiOut);
            FHttpGuiOut.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
            FHttpGuiOut.SetInterface(this);

        }


        public virtual bool AutoEvaluate
        {
            get { return false; }
        }

        #endregion pin creation








        #region IMyNodeIO

        public void GetDatenObjekt(int Index, out List<GuiDataObject> GuiDaten)
        {
            ////Debug.WriteLine("Enter Get daten Object");
            GuiDaten = new List<GuiDataObject>(mGuiDataList);
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
                    mChangedSpreadSize = true;
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

                for (int i = 0; i < mGuiDataList.Count; i++)
                {
                    mGuiDataList[i].GuiUpstreamList = null;
                }
                FUpstreamHttpGuiIn = null;


            }
            else if (Pin == FHttpStyleIn)
            {
                for (int i = 0; i < mGuiDataList.Count; i++)
                {
                    mGuiDataList[i].Transform = null;
                }

                FUpstreamStyle = null;
            }

        }

        #endregion NodeIO







        #region Configurate

        public void Configurate(IPluginConfig Input)
        {

        }

        #endregion







        #region Evaluate



        public void Evaluate(int SpreadMax)
        {

            

            FHost.GetNodePath(true, out mNodePath);
            
            bool ChangedID = false;
            if (mNodePath !=  mActualNodePath)
            {
                mActualNodePath = mNodePath;
                ChangedID = true;
            }
            //Debug.WriteLine(mNodePath);

            #region Check Gui List


            if (mSpreadMax != SpreadMax )
            {
                mChangedSpreadSize = true;
                

                if (mGuiDataList.Count > SpreadMax)
                {
                    mGuiDataList.RemoveRange(SpreadMax, mGuiDataList.Count - SpreadMax);
                    mGuiDataList.Capacity = SpreadMax;
                }
                else
                {
                    for (int i = mSpreadMax; i < SpreadMax; i++)
                    {
                        GuiDataObject tObject = new GuiDataObject();
                        mGuiDataList.Insert(i, tObject);
                        mGuiDataList[i].NodeId = HTMLToolkit.CreatePageID(mNodePath);
                        mGuiDataList[i].SliceId = HTMLToolkit.CreateSliceID(mNodePath, i);
                    }
                }
                mSpreadMax = SpreadMax;
            }

            if (ChangedID)
            {
                for (int i = 0; i < mGuiDataList.Count; i++)
                {
                    mGuiDataList[i].NodeId = HTMLToolkit.CreatePageID(mNodePath);
                    mGuiDataList[i].SliceId = HTMLToolkit.CreateSliceID(mNodePath, i);
                }

                ChangedID = false;
            }





            #endregion Check Gui List



            #region Transform Pin

            if (FTransformIn.PinIsChanged || FBasingPoint.PinIsChanged || FPositionType.PinIsChanged || mChangedSpreadSize)
            {
                string tBasingPoint;
                FBasingPoint.GetString(0, out tBasingPoint);

                string tPositionType;
                FPositionType.GetString(0, out tPositionType);

                for (int i = 0; i < SpreadMax; i++)
                {
                    Matrix4x4 tMatrix;

                    FTransformIn.GetMatrix(i, out tMatrix);


                    // Position Type
                    SortedList<string, string> tTransformSlice = new SortedList<string, string>();
                    tTransformSlice.Add("position", tPositionType);


                    //Scale
                    double tWidth = HTMLToolkit.MapScale(tMatrix.m11, 0, 2, 0, 100);
                    double tHeight = HTMLToolkit.MapScale(tMatrix.m22, 0, 2, 0, 100);

                    tTransformSlice.Add("width", ReplaceComma(string.Format("{0:0.0}", Math.Round(tWidth, 1)) + "%"));
                    tTransformSlice.Add("height", ReplaceComma(string.Format("{0:0.0}", Math.Round(tHeight, 1)) + "%"));

                    //X / Y Position
                    double tX;
                    double tY;

                    if (tBasingPoint == "BottomRight")
                    {
                        tX = HTMLToolkit.MapTransform(tMatrix.m42, 0, 2, 0, 100, tHeight);
                        tY = HTMLToolkit.MapTransform(tMatrix.m41, 0, -2, 0, 100, tWidth);

                        tTransformSlice.Add("bottom", ReplaceComma(string.Format("{0:0.0}", Math.Round(tX, 1)) + "%"));
                        tTransformSlice.Add("right", ReplaceComma(string.Format("{0:0.0}", Math.Round(tY, 1)) + "%"));
                    }
                    else if (tBasingPoint == "TopRight")
                    {
                        tX = HTMLToolkit.MapTransform(tMatrix.m42, 0, -2, 0, 100, tHeight);
                        tY = HTMLToolkit.MapTransform(tMatrix.m41, 0, -2, 0, 100, tWidth);

                        tTransformSlice.Add("top", ReplaceComma(string.Format("{0:0.0}", Math.Round(tX, 1)) + "%"));
                        tTransformSlice.Add("right", ReplaceComma(string.Format("{0:0.0}", Math.Round(tY, 1)) + "%"));
                    }
                    else if (tBasingPoint == "BottomLeft")
                    {
                        tX = HTMLToolkit.MapTransform(tMatrix.m42, 0, 2, 0, 100, tHeight);
                        tY = HTMLToolkit.MapTransform(tMatrix.m41, 0, 2, 0, 100, tWidth);

                        tTransformSlice.Add("bottom", ReplaceComma(string.Format("{0:0.0}", Math.Round(tX, 1)) + "%"));
                        tTransformSlice.Add("left", ReplaceComma(string.Format("{0:0.0}", Math.Round(tY, 1)) + "%"));
                    }
                    else if (tBasingPoint == "TopLeft")
                    {
                        tX = HTMLToolkit.MapTransform(tMatrix.m42, 0, -2, 0, 100, tHeight);
                        tY = HTMLToolkit.MapTransform(tMatrix.m41, 0, 2, 0, 100, tWidth);

                        tTransformSlice.Add("top", ReplaceComma(string.Format("{0:0.0}", Math.Round(tX, 1)) + "%"));
                        tTransformSlice.Add("left", ReplaceComma(string.Format("{0:0.0}", Math.Round(tY, 1)) + "%"));
                    }
                    else
                    {
                        tX = HTMLToolkit.MapTransform(tMatrix.m42, 1, -1, 0, 100, tHeight);
                        tY = HTMLToolkit.MapTransform(tMatrix.m41, -1, 1, 0, 100, tWidth);

                        tTransformSlice.Add("top", ReplaceComma(string.Format("{0:0.0}", Math.Round(tX, 1)) + "%"));
                        tTransformSlice.Add("left", ReplaceComma(string.Format("{0:0.0}", Math.Round(tY, 1)) + "%"));
                    }

                    tTransformSlice.Add("z-index", Convert.ToString(Math.Round(tMatrix.m33)));

                    mGuiDataList[i].Transform = new SortedList<string, string>(tTransformSlice);
                }
            }

            #endregion Transform Pin



            #region Upstream Gui Elements

            if (FUpstreamHttpGuiIn != null)
            {
                if (mChangedSpreadSize)
                {
                    List<GuiDataObject> tGuiList;
                    FUpstreamHttpGuiIn.GetDatenObjekt(0, out tGuiList);

                    for (int i = 0; i < SpreadMax; i++)
                    {
                        mGuiDataList[i].GuiUpstreamList = tGuiList;
                    }
                }
            }

            #endregion Upstream Gui Elements




            # region Upstream Css Properties

            int usSStyle;
            if (FUpstreamStyle != null)
            {
                string NodePath;
                FHost.GetNodePath(false, out NodePath);
                ////Debug.WriteLine("Enter Css Upstream Gui Node: " + NodePath);

                for (int i = 0; i < SpreadMax; i++)
                {
                    //get upstream slice index

                    FHttpStyleIn.GetUpsreamSlice(i, out usSStyle);

                    SortedList<string, string> tSliceCssPropertie;
                    FUpstreamStyle.GetCssProperties(i, out tSliceCssPropertie);

                    mGuiDataList[i].CssProperties = new SortedList<string, string>(tSliceCssPropertie);

                }
            }

            #endregion Upstream Css Propeties




            this.OnEvaluate(SpreadMax);

            //FHttpGuiOut.SliceCount = SpreadMax;

            if (mSpreadMax == SpreadMax)
            {
                mChangedSpreadSize = false;
            }
            ////Debug.WriteLine("Leave Evaluate GUiNodeDynamic");
        }



        #endregion Evaluate







        #region Node Information


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
            string[] Patches = pSliceId.Split('/');
            int tPatchDepth = Patches.Length;
            return Patches[Patches.Length - 1];
        }

        public string GetSliceId(int SliceNumber)
        {
            return mGuiDataList[SliceNumber].SliceId;
        }

        #endregion Node Information







        #region Add to GuiDataObject

        public void SetBodyContent(int pSliceIndex, string pContent)
        {
            mGuiDataList[pSliceIndex].AddString(pContent, GuiDataObject.Position.Body, true);
        }



        public void SetHeadContent(int pSliceIndex, string pContent)
        {
            mGuiDataList[pSliceIndex].AddString(pContent, GuiDataObject.Position.Head, true);
        }


        public void SetTag(int pSliceIndex, Tag pTag)
        {
            pTag.AddAttribute(new HTMLAttribute("id", mGuiDataList[pSliceIndex].SliceId));
            pTag.AddAttribute(new HTMLAttribute("class", mGuiDataList[pSliceIndex].SliceId + " " + mGuiDataList[pSliceIndex].NodeId)); 
            mGuiDataList[pSliceIndex].Tag = pTag;
        }


        public void SetJavaScript(int pSliceIndex, string pContent)
        {
            mGuiDataList[pSliceIndex].AddString(pContent, GuiDataObject.Position.JavaScript, true);
        }

        #endregion Add to GuiDataObject





        #region Get data from WebinterfaceSingelton

        public bool CheckIfNodeReceivedData()
        {
            if (mWebinterfaceSingelton.CheckIfNodeIdReceivedValues(mGuiDataList[0].NodeId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void GetNewDataFromServer(int SliceNumber, out string pContent)
        {
            mWebinterfaceSingelton.getNewBrowserData(mGuiDataList[SliceNumber].SliceId,mNodePath,SliceNumber,out pContent);

            if (mSavedResponses.ContainsKey(SliceNumber))
            {
                mSavedResponses.Remove(SliceNumber);
                mSavedResponses.Add(SliceNumber, pContent);
            }
            else
            {
                mSavedResponses.Add(SliceNumber, pContent);
            }
        }

        #endregion Get data from WebinterfaceSingelton




        #region Saved Responses


        public string GetSavedValue(int SliceNumber)
        {
            string tValue;
            mSavedResponses.TryGetValue(SliceNumber, out tValue);

            mWebinterfaceSingelton.AddListOnDestroy(mNodePath, mSavedResponses);
            return tValue;
        }



        #endregion Saved Responses



    }
}
