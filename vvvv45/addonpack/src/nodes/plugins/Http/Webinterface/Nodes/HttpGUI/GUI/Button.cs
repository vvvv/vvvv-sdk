using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using VVVV.Utils;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Utils.VMath;
using VVVV.Webinterface.Data;
using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    class Button : BaseGUINode, IPlugin, IDisposable
    {


        #region field declaration

        private bool FDisposed = false;

        private IValueIn FState;
        private IEnumConfig FButtonMode;
        private List<DatenGuiButton> mButtonDaten = new List<DatenGuiButton>();
        private List<BaseDatenObjekt> mGuiInDaten = new List<BaseDatenObjekt>();
        private List<JsFunktion> mFunktionDaten = new List<JsFunktion>();

        private IValueOut FValueOut;
        private NodeObserver mObserver;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();

        private string mNodeId;
        private string mButtonMode;
        private bool mChangedConfig = false;
        #endregion field declaration





        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Button()
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
                FHost.Log(TLogType.Message, "Button (Http Gui) Node is being deleted");

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
        ~Button()
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
                    FPluginInfo.Name = "Button";
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


        #endregion Plugin Information






        #region pin creation


        protected override void OnPluginHostSet()
        {
            //this.FHost.CreateStringInput("Label", TSliceMode.Dynamic, TPinVisibility.True, out FLabel);
            //FLabel.SetSubType("", false);

            FHost.UpdateEnum("ButtonMode", "Bang", new string[] { "Bang", "Toggle" });
            FHost.CreateEnumConfig("Mode", TSliceMode.Single, TPinVisibility.True, out FButtonMode);
            FButtonMode.SetSubType("ButtonMode");


            this.FHost.CreateNodeInput("Input GUI", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
		
            
            this.FHost.CreateValueOutput("Response",1,null,TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
            FValueOut.SetSubType(0,1,1,0,true,true,true);

            //FHost.CreateNodeOutput("JsFunktion", TSliceMode.Dynamic, TPinVisibility.True, out FFunktionOut);
            //FFunktionOut.SetSubType(new Guid[1] { HttpGUIFunktionIO.GUID }, HttpGUIFunktionIO.FriendlyName);
            //FFunktionOut.SetInterface(this);
            
            
            mNodeId = "button" + GetNodeID();
            mObserver = mWebinterfaceSingelton.AddNode(mNodeId);
           
        }


        #endregion pin creation






        #region Node IO

        public override void GetDatenObjekt(int Index, out BaseDatenObjekt GuiDaten)
        {
                ////Debug.WriteLine("Enter GetdatenObjekt" + mNodeId);
                GuiDaten = mButtonDaten[Index];
        }
        public override void GetFunktionObjekt(int Index, out JsFunktion FunktionsDaten)
        {
            FunktionsDaten = mFunktionDaten[Index];
        }


        #endregion Node IO





        #region Main Loop


        protected override void OnConfigurate(IPluginConfig Input)
        {
            if (Input == FButtonMode)
            {
                string tButtonselection = "";
                FButtonMode.GetString(0, out tButtonselection);
                mButtonMode = tButtonselection;
                mChangedConfig = true;
                if (tButtonselection == "Toggle")
                {
                    this.FHost.CreateValueInput("ChangeState", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FState);
                    FState.SetSubType(0, 1, 1, 0, false, true, true);
                    mWebinterfaceSingelton.Reload();
                }
                else if(FState != null)
                {
                    FHost.DeletePin(FState);
                    FState = null;
                    mWebinterfaceSingelton.Reload();
                }
            }
        }

        protected override void OnEvaluate(int SpreadMax)
        {
            ////Debug.WriteLine("Enter OnEvaluate");

            int[] tSliceCount = {FTransformIn.SliceCount};

            Array.Sort(tSliceCount);
            int ArrayLength = tSliceCount.Length -1;
            FHttpGuiOut.SliceCount = tSliceCount[ArrayLength];


            
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

            #endregion Upstream


            bool tChangedPin = false;
            if (FState != null)
            {
                tChangedPin = FState.PinIsChanged;
            }


            if ( FTransformIn.PinIsChanged || tChangedPin ||  mChangedConfig)
            {
                
                mButtonDaten.Clear();
                FValueOut.SliceCount = tSliceCount[ArrayLength];
                mChangedConfig = false;

                //FButtonMode.GetString(0,out currentSelection);
                //mButtonDaten.Mode = currentSelection;

                for (int i = 0; i < tSliceCount[ArrayLength]; i++)
                {

                    string tSliceId = mNodeId + "/" + i;
                    DatenGuiButton tButtonDatenObjekt = new DatenGuiButton(tSliceId , "Button", i);
                    tButtonDatenObjekt.Class =  tSliceId.Replace("/", "");
                    tButtonDatenObjekt.SliceNumber = i;

                    
                    
                    
                    mFunktionDaten.Clear();
                  
                    //State Pin
                    if (FState != null)
                    {
                        double currentStateSlice;
                        FState.GetValue(i, out currentStateSlice);

                        if (currentStateSlice > 0.5)
                        {

                            

                            //update Browser
                            if (tButtonDatenObjekt.State != currentStateSlice.ToString())
                            {
                                //FValueOut.SetValue(i, currentStateSlice);
                                JavaScript tJava = new JavaScript();
                                tJava.Insert("parent.window." + tButtonDatenObjekt.Class + "('" + tSliceId + "');");
                                mWebinterfaceSingelton.NotifyServer(tJava.Text);
                            }

                            tButtonDatenObjekt.State = Convert.ToInt16(currentStateSlice).ToString();
                        }
                    }


                    //Get Button Mode


                    //if (FFunktionOut.IsConnected == true)
                    //{
                    //     //Funktions Out
                         
                    //     tButtonDatenObjekt.CreateJsFuntkion = false;
                    //     tButtonDatenObjekt.JsFunktion.Name = tSliceId.Replace("/", "");
                    //     tButtonDatenObjekt.JsFunktion.Parameter = tSliceId;
                    //     if (tButtonselection == "Bang")
                    //     {
                    //         tButtonDatenObjekt.Mode = "Bang";
                    //         tButtonDatenObjekt.JsFunktion.Content = JSToolkit.ButtonBang();
                    //     }
                    //     else
                    //     {
                    //         tButtonDatenObjekt.Mode = "Toggle";
                    //         tButtonDatenObjekt.JsFunktion.Content = JSToolkit.ButtonToggle();
                    //     }

                    //     FFunktionOut.SliceCount = tSliceCount[ArrayLength];
                    //     mFunktionDaten.Add(tButtonDatenObjekt.JsFunktion);
                    //}
                    //else
                    //{
                        tButtonDatenObjekt.CreateJsFuntkion = true;
                        if (mButtonMode == "Bang")
                        {
                            tButtonDatenObjekt.Mode = "Bang";
                            tButtonDatenObjekt.JsFunktion.Content = JSToolkit.ButtonBang();
                        }
                        else
                        {
                            tButtonDatenObjekt.Mode = "Toggle";
                            tButtonDatenObjekt.JsFunktion.Content = JSToolkit.ButtonToggle();
                        }
                    //}

                    //FFunktionOut.SliceCount = tSliceCount[ArrayLength];
                    mFunktionDaten.Add(tButtonDatenObjekt.JsFunktion);


                    SortedList<string, string> tHtmlAttr = new SortedList<string, string>();
                    //Label Pin

                    


                    // Transform Pin                   
                    Matrix4x4 tMatrix = new Matrix4x4();
                    FTransformIn.GetMatrix(i, out tMatrix);

                    SortedList<string, string> tTransform;
                    GetTransformation(tMatrix, out tTransform);


                    //check incoming Style
                    SortedList<string, string> tCssProperties;
                    tCssProperties = tTransform;


                    SortedList<string, string> tCssPropertiesIn;
                    mStyles.TryGetValue(i, out tCssPropertiesIn);

                    if (tCssPropertiesIn != null)
                    {
                        foreach (KeyValuePair<string, string> pValuePair in tCssPropertiesIn)
                        {
                            if (tCssProperties.ContainsKey(pValuePair.Key))
                            {
                                tCssProperties.Remove(pValuePair.Key);
                                tCssProperties.Add(pValuePair.Key, pValuePair.Value);
                            }
                            else
                            {
                                tCssProperties.Add(pValuePair.Key, pValuePair.Value);
                            }

                        }
                    }

                    if (tCssProperties.ContainsKey("background-color"))
                    {

                    }
                    else
                    {
                        tCssProperties.Add("background-color", "#cccccc");
                    }






                    //CSS
                    

                    tButtonDatenObjekt.CssProperties = tCssProperties;
                    tButtonDatenObjekt.GuiObjektListe = mGuiInDaten;
                    
                    mButtonDaten.Add(tButtonDatenObjekt);
                }
            }



            if (mButtonMode  == "Bang")
            {
                for (int i = 0; i < tSliceCount[ArrayLength]; i++)
                {
                    FValueOut.SetValue(i,0);
                }

            }

            if (mObserver.ToNode != null)
            {
                string SliceId = mObserver.ToNode;
                string tNodeId = GetNodeIdformSliceId(SliceId);
                

                if (mObserver.ObserverState =="Server" && (mNodeId == tNodeId))
                {

                    SortedList<string, string> tValuePair = mWebinterfaceSingelton.Daten;
                    string tValue;
                    tValuePair.TryGetValue(SliceId, out tValue);
                    int tSliceIndex = Convert.ToInt16(GetSliceFormSliceId(mObserver.ToNode));


                    FValueOut.SetValue(tSliceIndex,Convert.ToDouble(tValue));
                    mWebinterfaceSingelton.setSubbjectStateToVVVV();
                }
            }

            
           

        }

        #endregion Main Loop


    }
}
