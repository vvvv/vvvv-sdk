using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Utils.VMath;
using VVVV.Webinterface.Data;
using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    class Slider : BaseGUINode, IPlugin, IDisposable
    {


        #region field declaration


        private IValueConfig FPosition;
        private List<DatenGuiSlider> mSliderDaten = new List<DatenGuiSlider>();
        private string mNodeId;
        private IEnumConfig FOrientation;

        private IValueOut FResponse;
        private NodeObserver mObserver;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        private bool FDisposed = false;


        private string currentOrientation;
        #endregion field declaration


        


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Slider()
        {
            
        }

        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (FDisposed == false)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                mWebinterfaceSingelton.DeleteNode(mObserver);
                FHost.Log(TLogType.Message, "Slider (Http Gui) Node is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }

            FDisposed = true;
        }


        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in WebTypes derived from this class.
        /// </summary>
        ~Slider()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion constructor/destructor






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
                    FPluginInfo.Name = "Slider";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "GUI";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Textfield node for the Renderer (HTTP)";
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

        #endregion




        #region pin creation 

        protected override void OnPluginHostSet()
        {
            //this.FHost.CreateStringInput("Label", TSliceMode.Dynamic, TPinVisibility.True, out FLabel);
            //FLabel.SetSubType("", false);

            FHost.UpdateEnum("SliderOrientation", "x", new string[] { "x", "y" });
            FHost.CreateEnumConfig("Orientation", TSliceMode.Single, TPinVisibility.True, out FOrientation);
            FOrientation.SetSubType("SliderOrientation");

            this.FHost.CreateValueConfig("StartPosition", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPosition);
            FPosition.SetSubType(0, 1, 0.01, 0, false, false, false);
            
            this.FHost.CreateValueOutput("Response",1,null, TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType(0, 1, 0.01, 0, false, false, false);

            mNodeId = "Slider" + GetNodeID();
            mObserver = mWebinterfaceSingelton.AddNode(mNodeId);
        }


        #endregion pin creation






        #region Node IO

        public override void GetDatenObjekt(int Index, out BaseDatenObjekt GuiDaten)
        {
            GuiDaten = mSliderDaten[Index];
        }

        public override void GetFunktionObjekt(int Index, out JsFunktion FunktionsDaten)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion Node IO






        #region MainLoop


        protected override void OnConfigurate(IPluginConfig Input)
        {
            if (Input == FOrientation)
            {
                
                FOrientation.GetString(0, out currentOrientation);
            }

            //else if (Input == FPosition)
            //{
            //    JavaScript tSetPosition = new JavaScript();
            //    tSetPosition.Insert("$('." + tSliderDatenObjekt.Class + "', parent.document).slider('option', 'value'," + currentPositionSlice + ");");
            //    mWebinterfaceSingelton.NotifyServer(tSetPosition.Text);
            //}
        }


        protected override void OnEvaluate(int SpreadMax)
        {

            if (FTransformIn.PinIsChanged || mChangedStyle)
            {
                mSliderDaten.Clear();
                FHttpGuiOut.SliceCount = SpreadMax;
                FResponse.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    string tSliceId = mNodeId + "/" + i;
                    DatenGuiSlider tSliderDatenObjekt = new DatenGuiSlider(tSliceId, "Slider", i);
                    SortedList<string, string> tHtmlAttr = new SortedList<string, string>();
                    SortedList<string, string> tCssProperties = new SortedList<string, string>();

                    double currentPositionSlice;
                    tSliderDatenObjekt.Class = tSliceId.Replace("/","");


                    FPosition.GetValue(i, out currentPositionSlice);
                    tSliderDatenObjekt.Position = currentPositionSlice.ToString();
                    mWebinterfaceSingelton.setNodeDaten(tSliceId, currentPositionSlice.ToString());


                    //Position Pin
                    //if (FPosition.PinIsChanged)
                    //{
                    //    JavaScript tSetPosition = new JavaScript();
                    //    tSetPosition.Insert("$('." + tSliderDatenObjekt.Class + "', parent.document).slider('option', 'value'," + currentPositionSlice + ");");
                    //    mWebinterfaceSingelton.NotifyServer(tSetPosition.Text);
                    //}

                    
                    //Orientation
                    if (currentOrientation == "x")
                    {
                        tSliderDatenObjekt.Orientation = "";
                    }
                    else
                    {
                        tSliderDatenObjekt.Orientation = "orientation: 'vertical'," + Environment.NewLine;
                    }
                    

                    //Transform Pin
                    Matrix4x4 tMatrix = new Matrix4x4();
                    FTransformIn.GetMatrix(i, out tMatrix);

                    SortedList<string, string> tTransform;
                    GetTransformation(tMatrix, out tTransform);

                    //Css Properteis
                    tCssProperties = tTransform;
                    //tCssProperties.Add("margin", "1%");

                    tSliderDatenObjekt.CssProperties = tCssProperties;
                    mSliderDaten.Add(tSliderDatenObjekt);
                }
            }

            if (mObserver.ToNode != null)
            {
                string SliceId = mObserver.ToNode;
                string tNodeId = GetNodeIdformSliceId(SliceId);


                if ((mObserver.ObserverState == "Server") && (mNodeId == tNodeId))
                {

                    SortedList<string, string> tValuePair = mWebinterfaceSingelton.Daten;
                    string tValue;
                    tValuePair.TryGetValue(SliceId, out tValue);
                    int tSliceIndex = Convert.ToInt16(GetSliceFormSliceId(mObserver.ToNode));
                    
                    FResponse.SetValue(tSliceIndex, Convert.ToDouble(tValue) / 1000);
                    mWebinterfaceSingelton.setSubbjectStateToVVVV();
                }
            }
            else
            {

            }
        }

        #endregion MainLoop


    }
}
