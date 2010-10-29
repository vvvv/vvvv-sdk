#region licence/info
//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html


//////initial author
//phlegma

#endregion licence/info

//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.Nodes.Http;
using VVVV.Nodes.Http.BaseNodes;

//the vvvv node namespace
namespace VVVV.Nodes.Http.GUI
{
    //class definition
    public class AsString : IPlugin, IDisposable, IHttpGUIIO, IPluginConnections
    {



        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private INodeIn FHttpIn;
        IHttpGUIIO FUpstreamInterface;
        List<GuiDataObject> FGuiDataList = new List<GuiDataObject>();

        bool FPinIsConnectedDisconnected = false;


        private IStringOut FHeadOut;
        private IStringOut FBodyOut;
        private IStringOut FJavaScriptOut;
        private IStringOut FCssOut;


        #endregion field declaration




        #region constructor/destructor

        public AsString()
        {
            //the nodes constructor
            //nothing to declare for this node
        }

        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                FHost.Log(TLogType.Debug, "AsString (HTTP) is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~AsString()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion constructor/destructor





        #region node name and infos

        //provide node infos 
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
                    FPluginInfo.Name = "AsString";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "AsString node for the Renderer (HTTP)";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "Webinterface";

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

        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return false; }
        }

        #endregion node name and infos





        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs
            FHost.CreateNodeInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FHttpIn);
            FHttpIn.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);

            FHost.CreateStringOutput("Head", TSliceMode.Dynamic, TPinVisibility.True, out FHeadOut);
            FHeadOut.SetSubType("", false);

            FHost.CreateStringOutput("Body", TSliceMode.Dynamic, TPinVisibility.True, out FBodyOut);
            FBodyOut.SetSubType("", false);

            FHost.CreateStringOutput("Css", TSliceMode.Dynamic, TPinVisibility.True, out FCssOut);
            FCssOut.SetSubType("", false);

            FHost.CreateStringOutput("JavaScript", TSliceMode.Dynamic, TPinVisibility.True, out FJavaScriptOut);
            FJavaScriptOut.SetSubType("", false);

        }

        #endregion pin creation





        #region IMyNodeIO


        public void GetDataObject(int Index, out List<GuiDataObject> GuiDaten)
        {
            GuiDaten = new List<GuiDataObject>();
            for (int i = 0; i < FGuiDataList.Count; i++)
            {
                GuiDaten.Add((GuiDataObject)(FGuiDataList[i].Clone()));
            }
        }




        public void ConnectPin(IPluginIO Pin)
        {
            FPinIsConnectedDisconnected = true;

            if (Pin == FHttpIn)
            {
                INodeIOBase usI;
                FHttpIn.GetUpstreamInterface(out usI);
                FUpstreamInterface = usI as IHttpGUIIO;
            }
            
        }



        public void DisconnectPin(IPluginIO Pin)
        {
            FPinIsConnectedDisconnected = true;

            if (Pin == FHttpIn)
            {
                FUpstreamInterface = null;
            }
        }

        #endregion





        #region mainloop

        public void Configurate(IPluginConfig Input)
        {

        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {

            if (FHttpIn.IsConnected)
            {
                if (FUpstreamInterface != null)
                {
                    List<GuiDataObject> tGuiData;
                    FUpstreamInterface.GetDataObject(0, out tGuiData);
                    FGuiDataList.AddRange(tGuiData);

                    FHeadOut.SliceCount = tGuiData.Count;
                    FBodyOut.SliceCount = tGuiData.Count;
                    FJavaScriptOut.SliceCount = tGuiData.Count;
                    FCssOut.SliceCount = tGuiData.Count;

                    for (int i = 0; i < tGuiData.Count; i++)
                    {
                        string Head = tGuiData[i].Head;
                        FHeadOut.SetString(i, Head);

                        string Body = tGuiData[i].Tag.Text;
                        FBodyOut.SetString(i, Body);

                        string JavaScript = tGuiData[i].JavaScript;
                        FJavaScriptOut.SetString(i, JavaScript);

                        SortedList<string, string> Css = tGuiData[i].CssProperties;
                        SortedList<string, string> Transform = tGuiData[i].Transform;

                        string CssContent = "";
                        foreach (KeyValuePair<string, string> Pairs in Css)
                        {
                            CssContent += Pairs.Key + ":" + Pairs.Value + Environment.NewLine;
                        }

                        foreach (KeyValuePair<string, string> Pairs in Transform)
                        {
                            CssContent += Pairs.Key + ":" + Pairs.Value + Environment.NewLine;
                        }

                        FCssOut.SetString(i, CssContent);

                    }
                }

            }
            else
            {
                FHeadOut.SliceCount = 0;
                FBodyOut.SliceCount = 0;
                FJavaScriptOut.SliceCount = 0;
                FCssOut.SliceCount = 0;
            }
        }

        #endregion mainloop





        #region IHttpGUIIO Members


        #endregion

        #region IHttpGUIIO Members

        public bool PinIsChanged()
        {
            return false;
        }

        public string GetNodeId(int Index)
        {
            return FGuiDataList[Index].NodeId;
        }

        public string GetSliceId(int Index)
        {
            return FGuiDataList[Index].SliceId;
        }

        public List<string> GetAllNodeIds()
        {
            return null;
        }

        #endregion
    }
}
