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
        public List<GuiDataObject> FGuiDataList = new List<GuiDataObject>();
        public int FSpreadMax = 0;
        public bool FChangedSpreadSize = true;
        private string FNodePath;
        private string FActualNodePath;
        private SortedList<int,string> FSavedResponses = new SortedList<int,string>();
        private WebinterfaceSingelton FWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        private string FNodeId;
        private List<string> FSliceId = new List<string>();
        private List<string> ReceivedString = new List<string>();


        #endregion field Definition





        #region abstract Methods

        protected abstract void OnSetPluginHost();
        protected abstract void OnEvaluate(int SpreadMax, string NodeId, List<string> SlideId, bool ReceivedNewString, List<string> ReceivedString);


        #endregion abstract Methods







        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            FHost.GetNodePath(true, out FNodePath);

            try
            {
                FWebinterfaceSingelton.HostPath = FNodePath;

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
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, "Error in GUINodeDynamic (Http) by Pin Initialisation" + Environment.NewLine + ex.Message);
            }
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
            GuiDaten = new List<GuiDataObject>(FGuiDataList);
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
                    FChangedSpreadSize = true;
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

                for (int i = 0; i < FGuiDataList.Count; i++)
                {
                    FGuiDataList[i].GuiUpstreamList = null;
                }
                FUpstreamHttpGuiIn = null;


            }
            else if (Pin == FHttpStyleIn)
            {
                for (int i = 0; i < FGuiDataList.Count; i++)
                {
                    FGuiDataList[i].Transform = null;
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

            

            try
            {


                FHost.GetNodePath(true, out FNodePath);

                bool ChangedID = false;
                if (FNodePath != FActualNodePath)
                {
                    FActualNodePath = FNodePath;
                    ChangedID = true;
                }

                #region Check Gui List


                if (FSpreadMax != SpreadMax)
                {
                    FChangedSpreadSize = true;


                    if (FGuiDataList.Count > SpreadMax)
                    {
                        FGuiDataList.RemoveRange(SpreadMax, FGuiDataList.Count - SpreadMax);
                        FGuiDataList.Capacity = SpreadMax;

                        FSliceId.RemoveRange(SpreadMax, FGuiDataList.Count - SpreadMax);
                        FSliceId.Capacity = SpreadMax;
                    }
                    else
                    {
                        for (int i = FSpreadMax; i < SpreadMax; i++)
                        {
                            GuiDataObject tObject = new GuiDataObject();
                            FGuiDataList.Insert(i, tObject);

                            FNodeId = HTMLToolkit.CreatePageID(FNodePath);
                            FGuiDataList[i].NodeId = FNodeId;

                            FSliceId[i] = HTMLToolkit.CreateSliceID(FNodePath, i);
                            FGuiDataList[i].SliceId = FSliceId[i];

                        }
                    }
                    FSpreadMax = SpreadMax;
                }

                if (ChangedID)
                {
                    for (int i = 0; i < FGuiDataList.Count; i++)
                    {
                        FNodeId = HTMLToolkit.CreatePageID(FNodePath);
                        FGuiDataList[i].NodeId = FNodeId;

                        FSliceId[i] = HTMLToolkit.CreateSliceID(FNodePath, i);
                        FGuiDataList[i].SliceId = FSliceId[i];
                    }

                    ChangedID = false;
                }





                #endregion Check Gui List



                #region Transform Pin

                if (FTransformIn.PinIsChanged || FBasingPoint.PinIsChanged || FPositionType.PinIsChanged || FChangedSpreadSize)
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

                        FGuiDataList[i].Transform = new SortedList<string, string>(tTransformSlice);
                    }
                }

                #endregion Transform Pin



                #region Upstream Gui Elements

                if (FUpstreamHttpGuiIn != null)
                {
                    if (FChangedSpreadSize)
                    {
                        List<GuiDataObject> tGuiList;
                        FUpstreamHttpGuiIn.GetDatenObjekt(0, out tGuiList);

                        for (int i = 0; i < SpreadMax; i++)
                        {
                            FGuiDataList[i].GuiUpstreamList = tGuiList;
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

                        FGuiDataList[i].CssProperties = new SortedList<string, string>(tSliceCssPropertie);

                    }
                }

                #endregion Upstream Css Propeties




                #region ReceivedData

                bool ReceivedNewString = CheckIfNodeReceivedData();
                ReceivedString.Capacity = SpreadMax;

                if(ReceivedNewString)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        ReceivedString[i] = GetNewDataFromServer(i);
                    }
                }

                #endregion ReceivedData



                this.OnEvaluate(SpreadMax, FNodeId, FSliceId, ReceivedNewString,ReceivedString);


                if (FSpreadMax == SpreadMax)
                {
                    FChangedSpreadSize = false;
                }
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, "in Node with Id: " + FNodePath + Environment.NewLine + ex.Message);
            }
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
            return FGuiDataList[SliceNumber].SliceId;
        }


        public string GetNodeID(int SliceNumber)
        {
            return FGuiDataList[SliceNumber].NodeId;
        }

        #endregion Node Information




        #region Add to GuiDataObject

        public void SetBodyContent(int pSliceIndex, string pContent)
        {
            FGuiDataList[pSliceIndex].AddString(pContent, GuiDataObject.Position.Body, true);
        }



        public void SetHeadContent(int pSliceIndex, string pContent)
        {
            FGuiDataList[pSliceIndex].AddString(pContent, GuiDataObject.Position.Head, true);
        }


        public void SetTag(int pSliceIndex, Tag pTag)
        {
            pTag.AddAttribute(new HTMLAttribute("id", FGuiDataList[pSliceIndex].SliceId));
            pTag.AddAttribute(new HTMLAttribute("class", FGuiDataList[pSliceIndex].SliceId + " " + FGuiDataList[pSliceIndex].NodeId)); 
            FGuiDataList[pSliceIndex].Tag = pTag;
        }

        public void SetTag(int pSliceIndex, Tag pTag, string ClassName)
        {
            pTag.AddAttribute(new HTMLAttribute("id", FGuiDataList[pSliceIndex].SliceId));
            pTag.AddAttribute(new HTMLAttribute("class", FGuiDataList[pSliceIndex].SliceId + " " + FGuiDataList[pSliceIndex].NodeId + " " + ClassName ));
            FGuiDataList[pSliceIndex].Tag = pTag;
        }


        public void SetJavaScript(int pSliceIndex, string pContent)
        {
            FGuiDataList[pSliceIndex].AddString(pContent, GuiDataObject.Position.JavaScript, true);
        }

        #endregion Add to GuiDataObject




        #region Get data from WebinterfaceSingelton

        private bool CheckIfNodeReceivedData()
        {
            if (FWebinterfaceSingelton.CheckIfNodeIdReceivedValues(FGuiDataList[0].NodeId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private string GetNewDataFromServer(int SliceNumber)
        {

            string tContent = null;

            FWebinterfaceSingelton.getNewBrowserData(FGuiDataList[SliceNumber].SliceId,FNodePath,SliceNumber,out tContent);

            if (tContent == null && FSavedResponses.ContainsKey(SliceNumber))
            {
                FSavedResponses.TryGetValue(SliceNumber, out tContent);
            }

            if (FSavedResponses.ContainsKey(SliceNumber))
            {
                FSavedResponses.Remove(SliceNumber);
                FSavedResponses.Add(SliceNumber, tContent);
            }
            else
            {
                FSavedResponses.Add(SliceNumber, tContent);
            }

            FWebinterfaceSingelton.AddListToSave(FNodePath, FSavedResponses);

            return tContent;
        }

        #endregion Get data from WebinterfaceSingelton




        #region Saved Responses






        #endregion Saved Responses



    }
}
