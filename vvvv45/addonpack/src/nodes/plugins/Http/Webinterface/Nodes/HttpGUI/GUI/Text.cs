using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.HttpGUI.Datenobjekte;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.HttpGUI
{
    class Text:BaseGUINode, IPlugin, IDisposable 
    {
    	
    	

        #region field declaration

        private IStringIn FTextInput;
        private List<DatenGuiText> mTextDaten = new List<DatenGuiText>();
        private List<BaseDatenObjekt> mGuiInDaten = new List<BaseDatenObjekt>();
        private string mNodeId;

        private bool FDisposed = false;

        #endregion field declaration

        
        
        
        
         #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Text()
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

                FHost.Log(TLogType.Message, "Text (Http Gui) Node is being deleted");

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
        ~Text()
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
                    FPluginInfo.Name = "Text";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "GUI";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Text node for the Renderer (HTTP)";
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






        #region Pin creation

        protected override void OnPluginHostSet()
        {
            this.FHost.CreateStringInput("Text", TSliceMode.Dynamic, TPinVisibility.True, out FTextInput);
            FTextInput.SetSubType("", false);

            this.FHost.CreateNodeInput("Input GUI", TSliceMode.Dynamic, TPinVisibility.True, out FHttpGuiIn);
            FHttpGuiIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            mNodeId = "Text" + GetNodeID();
        }

        #endregion Pin creation







        # region NodeIO


        public override void GetDatenObjekt(int Index, out BaseDatenObjekt GuiDaten)
        {
            GuiDaten = mTextDaten[Index];
        }

        public override void GetFunktionObjekt(int Index, out JsFunktion FunktionsDaten)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion NodeIO






        #region Mainloop

        protected override void OnConfigurate(IPluginConfig Input)
        {
            
        }

        protected override void OnEvaluate(int SpreadMax)
        {

            int[] tSliceCount = { FTextInput.SliceCount, FTransformIn.SliceCount, FHttpStyleIn.SliceCount };
            Array.Sort(tSliceCount);
            int ArrayLength = tSliceCount.Length - 1;
            FHttpGuiOut.SliceCount = tSliceCount[ArrayLength];
            //TextInput
            if (FTextInput.PinIsChanged || FTransformIn.PinIsChanged || mChangedStyle)
            {

                mTextDaten.Clear();

                for (int i = 0; i < tSliceCount[ArrayLength]; i++)
                {

                    FHttpGuiOut.SliceCount = SpreadMax;
                    DatenGuiText tTextDaten = new DatenGuiText(GetNodeID() + "/" + i , "Text", i);
                    string tSliceId = mNodeId + "/" + i;
                    tTextDaten.Class = tSliceId.Replace("/", "");
                    //read Text Data from input
                    string currentInputSlice;
                    FTextInput.GetString(i, out currentInputSlice);
                    
                    if (currentInputSlice != null)
                    {
                        tTextDaten.Label = currentInputSlice;
                    }
                    else
                    {
                        tTextDaten.Label = "No Text Input";
                    }


                    // Transform In Pin
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

                    //if (tCssPropertiesIn.ContainsKey("overflow") == false)
                    //{
                    //    tCssPropertiesIn.Add("overflow", "scroll");
                    //}

                    //if (tCssProperties.ContainsKey("text-align") == false)
                    //{
                    //    tCssProperties.Add("text-align", "center");
                    //}
                    
                    //if(tCssProperties.ContainsKey("vertical-align") == false)
                    //{
                    //    tCssProperties.Add("vertical-align", "middle");
                    //}
                        


                    tTextDaten.CssProperties = tCssProperties;
                    tTextDaten.GuiObjektListe =  mGuiInDaten;
                    mTextDaten.Add(tTextDaten);
                }
            }


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
                    FUpstreamInterface.GetDatenObjekt(usS, out tGuiDaten);

                    if (tGuiDaten != null)
                    {
                        mGuiInDaten.Add(tGuiDaten);
                    }
                }
            }


        }

        #endregion Mainloop
    }
}
