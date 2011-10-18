using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.jQuery;
using VVVV.Webinterface;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.IO;


namespace VVVV.Nodes.Http.BaseNodes
{
    public abstract class GuiNodeDynamic : POSTReceiverNode, IHttpGUIIO, IPluginConnections
    {




        #region field Definition


        // Input Pins
        public INodeIn FHttpGuiIn;
        public IHttpGUIIO FUpstreamHttpGuiIn;

		public ITransformIn FTransformIn;
        private IValueIn FSendPolling;

		private IEnumIn FPositionType;
		private IEnumIn FBasingPoint;
        private IEnumIn FUnit;

        public INodeIn FHttpStyleIn;
        public IHttpGUIStyleIO FUpstreamStyle;

		public INodeIn FJQueryNodeInput;
		protected IJQueryIO FUpstreamJQueryNodeInterface;
		protected IValueIn FSavePostedPropertiesValueInput;

		public INodeOut FHttpGuiOut;


        //Required Members
        public List<GuiDataObject> FGuiDataList = new List<GuiDataObject>();
		public List<GuiDataObject> FUpstreamGuiList;
		protected JQueryNodeIOData FUpstreamJQueryNodeData;
        private List<bool> FSendToBrowser = new List<bool>();
		private bool FHttpGuiInConnectedThisFrame = true;
		private bool FGuiListModified = false;
        private bool FDisconnectStyle = false;

        private SortedList<int, SortedList<string, XmlDocument>> FPollingMessages = new SortedList<int, SortedList<string, XmlDocument>>();

        private IStringOut FNodeIdOut;
        private IStringOut FSliceIdOut;
	

        #endregion field Definition





        #region abstract Methods

        protected abstract void OnSetPluginHost();
        protected abstract void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SlideId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser);
		protected abstract bool DynamicPinsAreChanged();


        #endregion abstract Methods




        #region pin creation

        //this method is called by vvvv when the node is created
        protected override void CreateBasePins()
        {
            //Input Pins 

            OnSetPluginHost();

            FHost.CreateNodeInput("GUI", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);

            FHost.CreateValueInput("Send", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FSendPolling);
            FSendPolling.SetSubType(0, 1, 1, 0, true, false, true);

            FHost.CreateNodeInput("CSS", TSliceMode.Dynamic, TPinVisibility.True, out FHttpStyleIn);
            FHttpStyleIn.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);

			FHost.CreateNodeInput("JQuery", TSliceMode.Single, TPinVisibility.True, out FJQueryNodeInput);
			FJQueryNodeInput.SetSubType(new Guid[1] { JQueryIO.GUID }, JQueryIO.FriendlyName);

            FHost.UpdateEnum("PositionType", "absolute", new string[] { "absolute", "fixed ", "relative ", "static " });
            FHost.CreateEnumInput("Positiontype", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FPositionType);
            FPositionType.SetSubType("PositionType");

            FHost.UpdateEnum("BasingPoint", "Center", new string[] { "Center", "TopLeft", "TopRight", "BottomLeft", "BottomRight" });
            FHost.CreateEnumInput("Basing Point", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FBasingPoint);
            FBasingPoint.SetSubType("BasingPoint");

            FHost.UpdateEnum("Unit", "Percent", new string[] { "Percent", "Pixel"});
            FHost.CreateEnumInput("Unit", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FUnit);
            FUnit.SetSubType("Unit");

			FHost.CreateValueInput("Save Posted Properties", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FSavePostedPropertiesValueInput);
			FSavePostedPropertiesValueInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);

            FHost.CreateNodeOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiOut);
            FHttpGuiOut.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
            FHttpGuiOut.SetInterface(this);
            FHttpGuiOut.Order = -3;

            FHost.CreateStringOutput("NodeId", TSliceMode.Single, TPinVisibility.Hidden, out FNodeIdOut);
            FNodeIdOut.SetSubType("",false);
            FNodeIdOut.Order = -2;

            FHost.CreateStringOutput("SliceIds", TSliceMode.Dynamic, TPinVisibility.Hidden, out FSliceIdOut);
            FSliceIdOut.SetSubType("", false);
            FSliceIdOut.Order = -1;

			
        }


        public virtual bool AutoEvaluate
        {
            get { return false; }
        }

        #endregion pin creation



        #region IMyNodeIO

		public bool PinIsChanged()
		{
			return (FGuiListModified || DynamicPinsAreChanged());
		}

        public void GetDataObject(int Index, out List<GuiDataObject> GuiDaten)
        {
			////Debug.WriteLine("Enter Get daten Object");
            GuiDaten = new List<GuiDataObject>();
            for (int i = 0; i < FGuiDataList.Count; i++)
            {
                GuiDaten.Add((GuiDataObject)(FGuiDataList[i].Clone()));
            }
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
                    FHttpGuiInConnectedThisFrame = true;
                }

            }
            else if (Pin == FHttpStyleIn)
            {
                INodeIOBase usIHttpStyle;
                FHttpStyleIn.GetUpstreamInterface(out usIHttpStyle);
                FUpstreamStyle = usIHttpStyle as IHttpGUIStyleIO;
            }
			else if (Pin == FJQueryNodeInput)
			{
				if (FJQueryNodeInput != null)
				{
					INodeIOBase upstreamInterface;
					FJQueryNodeInput.GetUpstreamInterface(out upstreamInterface);
					FUpstreamJQueryNodeInterface = upstreamInterface as IJQueryIO;
				}
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
                FHttpGuiInConnectedThisFrame = true;

            }
            else if (Pin == FHttpStyleIn)
            {
                for (int i = 0; i < FGuiDataList.Count; i++)
                {
                    FGuiDataList[i].CssProperties.Clear();
                }
                FDisconnectStyle = true;
                FUpstreamStyle = null;
            }
			if (Pin == FJQueryNodeInput)
			{
				FUpstreamJQueryNodeInterface = null;
				FUpstreamJQueryNodeData = null;
			}

        }

        #endregion NodeIO




        #region Configurate

        public void Configurate(IPluginConfig Input)
        {

        }

        #endregion




        #region Evaluate

		protected override void BaseEvaluate(int SpreadMax, bool ReceivedNewString)
        {


            FGuiListModified = ReceivedNewString;

            #region Check Gui List


            #region Upstream Gui Elements

			bool upstreamGuiListChanged = false;

			if (FUpstreamHttpGuiIn != null)
			{
				if (FHttpGuiInConnectedThisFrame || FUpstreamHttpGuiIn.PinIsChanged())
				{
					FUpstreamHttpGuiIn.GetDataObject(0, out FUpstreamGuiList);
					upstreamGuiListChanged = true;
				}
			}
						
			#endregion Upstream Gui Elements
			
			if (FChangedSpreadSize)
            {
				if (FGuiDataList.Count > SpreadMax)
                {
                    FSliceIdOut.SliceCount = SpreadMax;

                    FGuiDataList.RemoveRange(SpreadMax, FGuiDataList.Count - SpreadMax);
                    FGuiDataList.Capacity = SpreadMax;
                }
                else
                {
                    FSliceIdOut.SliceCount = SpreadMax;

                    for (int i = FSpreadMax; i < SpreadMax; i++)
                    {
                        GuiDataObject tObject = new GuiDataObject();
                        FGuiDataList.Insert(i, tObject);

                        FGuiDataList[i].NodeId = FNodeId;
                        FGuiDataList[i].SliceId = FSliceId[i];

                        FSliceIdOut.SetString(i, FSliceId[i]);
                    }


                    FNodeIdOut.SetString(0, FNodeId);
                }
            }

			if (FChangedSpreadSize || upstreamGuiListChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					FGuiDataList[i].GuiUpstreamList = FUpstreamGuiList;
				}
				FGuiListModified = true;
			}

            if (FChangedNodeId)
            {
                for (int i = 0; i < FGuiDataList.Count; i++)
                {
                    FGuiDataList[i].NodeId = FNodeId;
                    FGuiDataList[i].SliceId = FSliceId[i];
                }
            }

            #endregion Check Gui List


			#region JQuery

            try
            {
                if (FJQueryNodeInput.IsConnected && (FJQueryNodeInput.PinIsChanged || FUpstreamJQueryNodeInterface.PinIsChanged))
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        FUpstreamJQueryNodeData = FUpstreamJQueryNodeInterface.GetJQueryData(i);
                        FGuiDataList[i].AddString(JQuery.GenerateDocumentReady(FUpstreamJQueryNodeData.BuildChain().SetSelector(new ClassSelector(FNodeId))).GenerateScript(1, true, true), GuiDataObject.Position.UpstreamJQuery, true);
                    }
                    FGuiListModified = true;
                }
                else if (FJQueryNodeInput.PinIsChanged)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        FGuiDataList[i].ResetContent(GuiDataObject.Position.UpstreamJQuery);
                    }
                    FGuiListModified = true;
                }
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, ex.Message);
            }


			
			#endregion


            #region Transform Pin

            if (FTransformIn.PinIsChanged || FBasingPoint.PinIsChanged || FPositionType.PinIsChanged|| FUnit.PinIsChanged || FSavePostedPropertiesValueInput.PinIsChanged || FChangedSpreadSize)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    FGuiListModified = true;
                    string tBasingPoint;
                    FBasingPoint.GetString(i, out tBasingPoint);

                    string tPositionType;
                    FPositionType.GetString(i, out tPositionType);

                    string Unit;
                    FUnit.GetString(i, out Unit);

                    Matrix4x4 tMatrix;
                    FTransformIn.GetMatrix(i, out tMatrix);

                    // Position Type
                    SortedList<string, string> tTransformSlice = new SortedList<string, string>();
                    tTransformSlice.Add("position", tPositionType);

                    if (Unit == "Percent")
                    {

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

                        tTransformSlice.Add("z-index", Convert.ToString(Math.Round(tMatrix.m43)));

                        FGuiDataList[i].Transform = new SortedList<string, string>(tTransformSlice);
                    }
                    else
                    {
                        //Scale
                        int tWidth = (int) tMatrix.m11;
                        int tHeight = (int) tMatrix.m22;

                        tTransformSlice.Add("width", tWidth.ToString() + "px");
                        tTransformSlice.Add("height", tHeight.ToString() + "px");

                        //X / Y Position
                        int tX;
                        int tY;

                        if (tBasingPoint == "BottomRight")
                        {
                            tX = (int) tMatrix.m42 - (tHeight/2);
                            tY = (int) tMatrix.m41 - (tWidth/2);

                            tTransformSlice.Add("bottom",  tX.ToString() + "px");
                            tTransformSlice.Add("right",  tY.ToString() + "px");
                        }
                        else if (tBasingPoint == "TopRight")
                        {
                            tX = (int)tMatrix.m42 - (tHeight / 2);
                            tY = (int)tMatrix.m41 - (tWidth / 2);

                            tTransformSlice.Add("top", tX.ToString() + "px");
                            tTransformSlice.Add("right", tY.ToString() + "px");
                        }
                        else if (tBasingPoint == "BottomLeft")
                        {
                            tX = (int)tMatrix.m42 - (tHeight / 2);
                            tY = (int)tMatrix.m41 - (tWidth / 2);

                            tTransformSlice.Add("bottom", tX.ToString() + "px");
                            tTransformSlice.Add("left", tY.ToString() + "px");
                        }
                        else if (tBasingPoint == "TopLeft")
                        {
                            tX = (int)tMatrix.m42 - (tHeight / 2);
                            tY = (int)tMatrix.m41 - (tWidth / 2);

                            tTransformSlice.Add("top", tX.ToString() + "px");
                            tTransformSlice.Add("left", tY.ToString() + "px");
                        }
                        else
                        {
                            FHost.Log(TLogType.Message, "CenterTransform is not implementet yet. Please use BottomRight/TopRight/BottomLeft or TopLeft");
                        }

                        tTransformSlice.Add("z-index", Convert.ToString(Math.Round(tMatrix.m43)));

                        FGuiDataList[i].Transform = new SortedList<string, string>(tTransformSlice);

                    }
                }
            }

            #endregion Transform Pin


            # region Upstream Css Properties

            //int usSStyle;
            if (FUpstreamStyle != null)
            {
                if (FHttpStyleIn.IsConnected && (FHttpStyleIn.PinIsChanged || FUpstreamStyle.PinIsChanged() || FChangedSpreadSize))
                {
                    FGuiListModified = true;

                    string NodePath;
                    FHost.GetNodePath(false, out NodePath);
                    ////Debug.WriteLine("Enter Css Upstream Gui Node: " + NodePath);

                    for (int i = 0; i < SpreadMax; i++)
                    {
                        //get upstream slice index

                        //FHttpStyleIn.GetUpsreamSlice(i, out usSStyle);

                        SortedList<string, string> tSliceCssPropertie;
                        FUpstreamStyle.GetCssProperties(i, out tSliceCssPropertie);

                        FGuiDataList[i].CssProperties = tSliceCssPropertie;
                    }
                }
            }
            if (FDisconnectStyle)
            {
                FDisconnectStyle = false;
                FGuiListModified = true;

                for (int i = 0; i < SpreadMax; i++)
                {
                    FGuiDataList[i].CssProperties.Clear();
                }
            }

            #endregion Upstream Css Propeties


            #region Polling

            if (FSendPolling.PinIsChanged)
            {

                FSendToBrowser = new List<bool>();
                FSendToBrowser.Clear();

                for (int i = 0; i < SpreadMax; i++)
                {
                    double currentSendValue = 0;
                    FSendPolling.GetValue(i, out currentSendValue);

                    FSendToBrowser.Insert(i, FSendPolling.PinIsChanged);

                    FReceivedNewString = true;
                    FReceivedString[i] = null;
                    SetPollingMessage(i);
                }
            }

            #endregion 

            this.OnEvaluate(SpreadMax, FChangedSpreadSize, FNodeId, FSliceId, FReceivedNewString,FReceivedString, FSendToBrowser);


			FHttpGuiInConnectedThisFrame = false;
            
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


        public string GetNodeId(int SliceNumber)
        {
            return FGuiDataList[SliceNumber].NodeId;
        }

        #endregion Node Information




        #region Add to GuiDataObject

        public void SetTag(int pSliceIndex, Tag pTag)
        {
            pTag.AddAttribute(new HTMLAttribute("id", FSliceId[pSliceIndex]));
            pTag.AddAttribute(new HTMLAttribute("class", FNodeId + " Slice" +  String.Format("{0:00000}", pSliceIndex))); 
            FGuiDataList[pSliceIndex].Tag = pTag;
        }

        public void SetTag(int pSliceIndex, Tag pTag, string ClassName)
        {
            pTag.AddAttribute(new HTMLAttribute("id", FSliceId[pSliceIndex]));
            pTag.AddAttribute(new HTMLAttribute("class", FNodeId + " Slice" + String.Format("{0:00000}", pSliceIndex) + " " + ClassName)); 
            FGuiDataList[pSliceIndex].Tag = pTag;
        }


        public void AddJavaScript(int pSliceIndex, string pContent, bool reset)
        {
            FGuiDataList[pSliceIndex].AddString(pContent, GuiDataObject.Position.JavaScript, reset);
        }

		public void ResetJavaScript(int pSliceIndex)
		{
			FGuiDataList[pSliceIndex].ResetContent(GuiDataObject.Position.JavaScript);
		}

        #endregion Add to GuiDataObject




        #region Polling

        /// <summary>
        /// Creates an Xml File with is polled by the server
        /// </summary>
        /// <param name="SliceID">the Object which value should be set</param>
        /// <param name="ObjectMethodName">the Name of the Method which is used to set the necessary value</param>
        /// <param name="Elements">the Mehtod parameters to set the value</param>
        public void CreatePollingMessage(int index, string SliceID, string ObjectMethodName,params string[] MethodeParameters)
        {
            SortedList<string, XmlDocument> tPollingValues = new SortedList<string, XmlDocument>();

            //the xml Document to create the XmlNode witch contains theinformation for the Browser
            XmlDocument doc = new XmlDocument();
            XmlNode RootNode, ElementNode;

            RootNode = doc.CreateElement("node");
            doc.AppendChild(RootNode);

            RootNode.Attributes.Append(doc.CreateAttribute("SliceId")).InnerText = "#" + SliceID;
            RootNode.Attributes.Append(doc.CreateAttribute("ObjectMethodName")).InnerText = ObjectMethodName;

            if (MethodeParameters != null)
            {
                for (int i = 0; i < MethodeParameters.Length; i++)
                {
                    ElementNode = doc.CreateElement("MethodParameters");
                    ElementNode.InnerText = MethodeParameters[i];
                    RootNode.AppendChild(ElementNode);
                }

            }
            
            if(FPollingMessages.ContainsKey(index))
            {
                FPollingMessages.TryGetValue(index, out tPollingValues);
                if (tPollingValues.ContainsKey(SliceID) == false)
                {
                    tPollingValues.Add(SliceID, doc);
                }
                else
                {
                    tPollingValues.Remove(SliceID);
                    tPollingValues.Add(SliceID, doc);
                }
                FPollingMessages.Remove(index);
                FPollingMessages.Add(index, tPollingValues);
                
            }else
            {
                tPollingValues.Add(SliceID, doc);
                FPollingMessages.Add(index, tPollingValues);
            }
        }




        private void SetPollingMessage(int index)
        {

            SortedList<string,XmlDocument> tPollingValues;
            FPollingMessages.TryGetValue(index,out tPollingValues);

            if (tPollingValues != null)
            {
                foreach (KeyValuePair<string, XmlDocument> Pair in tPollingValues)
                {
                    FWebinterfaceSingelton.setPollingMessage(Pair.Key, Pair.Value);
                }
            }
            
        }



        #endregion




        #region IHttpGUIIO Members


        public List<string> GetAllNodeIds()
		{
			List<string> allNodeIds = new List<string>();
			allNodeIds.Add(FNodeId);
			return allNodeIds;
		}

		#endregion



        #region AddTexturesToSingelton


        public void AddTextureToMemory(string Path)
        {
            string tSource = new FileInfo(Path).Name;
            FWebinterfaceSingelton.SetFileToStorage(tSource, File.ReadAllBytes(Path));
        }

        public void SetSaveLastResponse(bool State)
        {
            FSaveLastResponse = State;
        }



        #endregion
    }
}
