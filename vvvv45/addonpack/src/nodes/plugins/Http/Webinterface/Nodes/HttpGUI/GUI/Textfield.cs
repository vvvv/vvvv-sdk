using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Utils.VMath;
using VVVV.Webinterface;
using VVVV.Webinterface.Data;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    class Textfield:BaseGUINode,IPlugin, IDisposable
    {




        #region field declaration

        private IStringIn FValue;
        private IStringIn FLabel;
        private IStringOut FResponse;

        private List<DatenGuiTextfield> mTextfieldGuiDaten = new List<DatenGuiTextfield>();
        private string mNodeId;

        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        private NodeObserver mObserver;
        private bool FDisposed = false;
        private string[] mOldSpreadValues;

        #endregion field declaration





        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Textfield()
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
                FHost.Log(TLogType.Message, "Textfield (Http Gui) Node is being deleted");

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
        ~Textfield()
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
                    FPluginInfo.Name = "Textfield";
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
            
            this.FHost.CreateStringInput("Value", TSliceMode.Dynamic, TPinVisibility.True, out FValue);
            FValue.SetSubType("", false);

            this.FHost.CreateStringOutput("Response", TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType("", false);

            mNodeId = "Textfield" + GetNodeID();
            mObserver = mWebinterfaceSingelton.AddNode(mNodeId);
        }

        #endregion pin creation







        #region Node IO



        public override void GetDatenObjekt(int Index, out BaseDatenObjekt GuiDaten)
        {
            GuiDaten = mTextfieldGuiDaten[Index];
        }

        public override void GetFunktionObjekt(int Index, out JsFunktion FunktionsDaten)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion Node IO








        #region MainLoop



        protected override void OnConfigurate(IPluginConfig Input)
        {
           
        }


        protected override void OnEvaluate(int SpreadMax)
        {
            int[] tSliceCount = { FTransformIn.SliceCount, FValue.SliceCount, FHttpStyleIn.SliceCount };
            Array.Sort(tSliceCount);
            int ArrayLength = tSliceCount.Length - 1;
            FHttpGuiOut.SliceCount = tSliceCount[ArrayLength];





            if (FValue.PinIsChanged || FTransformIn.PinIsChanged || mChangedStyle)
            {
                mTextfieldGuiDaten.Clear();


                //Look witch slice changed
                string SpreadAsString = FValue.SpreadAsString;
                string[] tSpreadValues = SpreadAsString.Split(new char[] { ',' });
                bool[] tChangedSlice = new bool[tSliceCount[ArrayLength]];


                if (mOldSpreadValues != null)
                {
                    if (mOldSpreadValues.Length <= tSpreadValues.Length)
                    {
                        for (int j = 0; j < mOldSpreadValues.Length; j++)
                        {
                            if (mOldSpreadValues[j] == tSpreadValues[j])
                            {
                                tChangedSlice[j] = false;
                            }
                            else
                            {
                                tChangedSlice[j] = true;
                            }
                        }

                        for (int i = mOldSpreadValues.Length; i < tSliceCount[ArrayLength]; i++)
                        {
                            tChangedSlice[i] = true;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < tSpreadValues.Length; j++)
                        {
                            if (mOldSpreadValues[j] == tSpreadValues[j])
                            {
                                tChangedSlice[j] = false;
                            }
                            else
                            {
                                tChangedSlice[j] = true;
                            }
                        }
                        for (int i = tSpreadValues.Length; i < tSliceCount[ArrayLength]; i++)
                        {
                            tChangedSlice[i] = true;
                        }
                    }
                }


                mOldSpreadValues = tSpreadValues;






                for (int i = 0; i < tSliceCount[ArrayLength]; i++)
                {

                   FHttpGuiOut.SliceCount = SpreadMax;
                   FResponse.SliceCount = SpreadMax;
                   string tSliceId = mNodeId + "/" + i;
                   DatenGuiTextfield tTextfieldGuiDaten = new DatenGuiTextfield(tSliceId, "Textfield", i);
                   
                   tTextfieldGuiDaten.Class = tSliceId.Replace("/", "");



                    //Label
                    //string currentLabelSlice;
                    //FLabel.GetString(i, out currentLabelSlice);


                    
                        //if (currentLabelSlice != null)
                        //{
                        //    tTextfieldGuiDaten.Label = currentLabelSlice;
                        //}
                        //else
                        //{
                        //    tTextfieldGuiDaten.Label = "";
                        //}
                    


                    //Value Pin
                    




                    if (tChangedSlice[i] == true && FValue.PinIsChanged)
                    {
                        string currentValueSlice;
                        FValue.GetString(i, out currentValueSlice);
                        FResponse.SetString(i, currentValueSlice);

                        if (currentValueSlice != null)
                        {
                            JavaScript tJava = new JavaScript();
                            tJava.Insert("parent.window.setNewDaten('" + tSliceId + "','" + currentValueSlice + "');");
                            mWebinterfaceSingelton.NotifyServer(tJava.Text);
                            tTextfieldGuiDaten.Value = currentValueSlice;
                            mWebinterfaceSingelton.setNodeDaten(tSliceId, currentValueSlice.ToString());
                        }
                        else
                        {
                            JavaScript tJava = new JavaScript();
                            tJava.Insert("parent.window.setNewDaten('" + tSliceId + "','" + currentValueSlice + "');");
                            mWebinterfaceSingelton.NotifyServer(tJava.Text);
                            tTextfieldGuiDaten.Value = "";
                            mWebinterfaceSingelton.setNodeDaten(tSliceId, "");
                        }
                    }
                    
                   

                    //Function 
                    tTextfieldGuiDaten.JsFunktion.Name = tSliceId.Replace("/", "");
                    tTextfieldGuiDaten.JsFunktion.Parameter = tSliceId;
                    tTextfieldGuiDaten.JsFunktion.Content = JSToolkit.TextfieldSendData();

                    // Tansform Pin
                    Matrix4x4 tMatrix = new Matrix4x4();
                    FTransformIn.GetMatrix(i, out tMatrix);

                    SortedList<string, string> tTransform;
                    GetTransformation(tMatrix, out tTransform);

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

                    tTextfieldGuiDaten.CssProperties = tCssProperties;
                    mTextfieldGuiDaten.Add(tTextfieldGuiDaten);
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

                    FResponse.SetString(tSliceIndex, tValue);
                    //mLogger.log(mLogger.LogType.Debug, "Node: " + mNodeId + " changed Value: " + tSliceKeys.Value + " at Index: " + tSliceIndex + " from Server");


                    //FLastStateOut.SetString(0, "Daten vom Server");
                    //mWebinterfaceSingelton.SubjectState = "VVVV";

                    mWebinterfaceSingelton.setSubbjectStateToVVVV();
                }
            }
            else
            {

            }
        }



       #endregion Main Loop

    }
}
