using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.Http.BaseNodes;

namespace VVVV.Nodes.Http.GUI
{
    class Image : GuiNodeDynamic, IPlugin, IDisposable
    {




        #region field declaration


        private IStringIn FSource;
        private IStringIn FAlt;

        private bool FDisposed = false;

        #endregion field declaration






        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Image()
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

                FHost.Log(TLogType.Message, "Image (Http Gui) Node is being deleted");

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
        ~Image()
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
                    FPluginInfo.Name = "Image";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Image node for the Renderer (HTTP)";
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

        protected override void OnSetPluginHost()
        {
            this.FHost.CreateStringInput("Source", TSliceMode.Dynamic, TPinVisibility.True, out FSource);
            FSource.SetSubType("",true);

            this.FHost.CreateStringInput("Alt", TSliceMode.Dynamic, TPinVisibility.True, out FAlt);
            FAlt.SetSubType("", false);
        }

        #endregion pin creation




        #region Main Loop



        protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SliceId, bool ReceivedNewString, List<string> ReceivedString, List<bool> SendToBrowser)
        {


            if (DynamicPinsAreChanged() || changedSpreadSize || ReceivedNewString)
            {

                for (int i = 0; i < SpreadMax; i++)
                {
                    string currentSourceSlice;
                    string currentAltSlice;
                    string tAlt = String.Empty;
                    FSource.GetString(i, out currentSourceSlice);
                    FAlt.GetString(i, out currentAltSlice);

                    string tSource = new FileInfo(currentSourceSlice).Name;

                    bool FileExist = File.Exists(currentSourceSlice);

                    // Source Pins
                    if (FileExist)
                    {
                        FWebinterfaceSingelton.SetFileToStorage(tSource, File.ReadAllBytes(currentSourceSlice));
                    }
                    else
                    {
                        tSource = "No Source";
                        FHost.Log(TLogType.Message, FPluginInfo.Namespace + ": can't load " + currentSourceSlice);
                    }

                    // Alt Pin Input
                    if (currentAltSlice != null || currentAltSlice != "")
                    {
                        Img tImage = new Img(tSource,currentAltSlice);
                        SetTag(i, tImage);
                    }
                    else
                    {
                        Img tImage = new Img(tSource);
                        SetTag(i, tImage);
                    }

                    CreatePollingMessage(i, SliceId[i], "attr","src",tSource);
                }
            }
        }


        #endregion Main Loop

		protected override bool DynamicPinsAreChanged()
		{
			return (FSource.PinIsChanged || FAlt.PinIsChanged);
		}
	}
}
