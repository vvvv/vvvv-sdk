using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI;
using VVVV.Utils.VMath;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.jQuery;
using VVVV.Webinterface;
using System.Diagnostics;
using System.Globalization;
using System.Xml;


namespace VVVV.Nodes.Http.BaseNodes
{
    public abstract class GuiNodeDynamic : POSTReceiverNode, IHttpGUIIO, IPluginConnections
    {




        #region field Definition


        

        // Input Pins
        public INodeIn FHttpGuiIn;
        public IHttpGUIIO FUpstreamHttpGuiIn;

		public ITransformIn FTransformIn;

		private IEnumIn FPositionType;
		private IEnumIn FBasingPoint;

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
		private bool FHttpGuiInConnectedThisFrame = true;
		private bool FGuiListModified = false;
        private bool FDisconnectStyle = false;
	

        #endregion field Definition





        #region abstract Methods

        protected abstract void OnSetPluginHost();
        protected abstract void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SlideId, bool ReceivedNewString, List<string> ReceivedString);
		protected abstract bool DynamicPinsAreChanged();


        #endregion abstract Methods







        #region pin creation

        //this method is called by vvvv when the node is created
        protected override void CreateBasePins()
        {
            //Input Pins 
            FHost.CreateNodeInput("Input GUI", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
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

			FHost.CreateNodeInput("JQuery", TSliceMode.Single, TPinVisibility.True, out FJQueryNodeInput);
			FJQueryNodeInput.SetSubType(new Guid[1] { JQueryIO.GUID }, JQueryIO.FriendlyName);

			FHost.CreateValueInput("Save Posted Properties", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSavePostedPropertiesValueInput);
			FSavePostedPropertiesValueInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);

            FHost.CreateNodeOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiOut);
            FHttpGuiOut.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
            FHttpGuiOut.SetInterface(this);

			OnSetPluginHost();
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
                    FGuiDataList.RemoveRange(SpreadMax, FGuiDataList.Count - SpreadMax);
                    FGuiDataList.Capacity = SpreadMax;
                }
                else
                {
                    for (int i = FSpreadMax; i < SpreadMax; i++)
                    {
                        GuiDataObject tObject = new GuiDataObject();
                        FGuiDataList.Insert(i, tObject);

                        FGuiDataList[i].NodeId = FNodeId;
                        FGuiDataList[i].SliceId = FSliceId[i];
                    }
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

            if (FTransformIn.PinIsChanged || FBasingPoint.PinIsChanged || FPositionType.PinIsChanged || FSavePostedPropertiesValueInput.PinIsChanged || FChangedSpreadSize)
            {
				FGuiListModified = true;
				
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


            # region Upstream Css Properties

            int usSStyle;
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

                        FHttpStyleIn.GetUpsreamSlice(i, out usSStyle);

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


            this.OnEvaluate(SpreadMax, FChangedSpreadSize, FNodeId, FSliceId, FReceivedNewString,FReceivedString);


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
        public void SetPollingData(int index, string SliceID, string ObjectMethodName, string[] MethodeParameters)
        {
            //the xml Document to create the XmlNode witch contains theinformation for the Browser
            XmlDocument doc = new XmlDocument();
            XmlNode RootNode, ElementNode;

            RootNode = doc.CreateElement("node");
            doc.AppendChild(RootNode);

            RootNode.Attributes.Append(doc.CreateAttribute("SliceId")).InnerText = "#" + SliceID;
            RootNode.Attributes.Append(doc.CreateAttribute("ObjectMethodName")).InnerText = ObjectMethodName; 

            for (int i = 0; i < MethodeParameters.Length; i++)
			{
                ElementNode = doc.CreateElement("MethodParameters");
                ElementNode.InnerText = MethodeParameters[i];
                RootNode.AppendChild(ElementNode);
			}

            FWebinterfaceSingelton.setPollingMessage(SliceID, doc);
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
	}
}
