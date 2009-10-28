#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Nodes.HttpGUI;
//the vvvv node namespace
namespace VVVV.Nodes.HttpGUI
{
    //class definition
    public class Group : IPlugin, IDisposable, IHttpGUIIO, IPluginConnections
    {



        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        

        //Config Pin
        private IValueConfig FInputCount;


        //input pin declaration
        private INodeIn FInput1;
        private INodeIn FInput2;

        //output pin declaration
        private INodeOut FMyNodeOutput;
        

        private List<INodeIn> FInputPinList;
        private List<INodeIOBase> FUpstreamInterfaceList;
        private SortedList<string, IHttpGUIIO> FNodeUpstream;
        private bool FPinIsChanged = false;
        private bool FPinIsConnectedDisconnected = false;


        List<GuiDataObject> mGuiDataList = new List<GuiDataObject>();
        private int mInputCount;

        #endregion field declaration




        #region constructor/destructor

        public Group()
        {
            
            FInputPinList = new List<INodeIn>();
            FInputPinList.Capacity = 2;
            FUpstreamInterfaceList = new List<INodeIOBase>();
            FUpstreamInterfaceList.Capacity = 2;

            FNodeUpstream = new SortedList<string, IHttpGUIIO>();
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

                FHost.Log(TLogType.Debug, "Group (HTTP GUI) is being deleted");

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
        ~Group()
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
                    FPluginInfo.Name = "Group";
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
            FHost.CreateNodeInput("Input1", TSliceMode.Dynamic, TPinVisibility.True, out FInput1);
            FInput1.SetSubType(new Guid[1] { HttpGUIIO.GUID}, HttpGUIIO.FriendlyName);
            FInputPinList.Add(FInput1);

            FHost.CreateNodeInput("Input2", TSliceMode.Dynamic, TPinVisibility.True, out FInput2);
            FInput2.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
            FInputPinList.Add(FInput2);

            //create outputs	    	
            FHost.CreateNodeOutput("NodePin Out", TSliceMode.Dynamic, TPinVisibility.True, out FMyNodeOutput);
            FMyNodeOutput.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
            FMyNodeOutput.SetInterface(this);

            //config pin
            FHost.CreateValueConfig("Pin Count",1,null, TSliceMode.Single, TPinVisibility.True, out FInputCount);
            FInputCount.SetSubType(2, double.MaxValue, 1, 2, false, false, true);
        }

        #endregion pin creation





        #region IMyNodeIO


        public void GetDataObject(int Index, out List<GuiDataObject> GuiDaten)
        {
			GuiDaten = new List<GuiDataObject>(mGuiDataList);
        }




        public void ConnectPin(IPluginIO Pin)
        {
            mInputCount = FInputPinList.Count;
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            FPinIsConnectedDisconnected = true;

             foreach (INodeIn pNodeIn in FInputPinList)
             {
                 if (Pin == pNodeIn)
                    {
                        INodeIOBase usI;
                        pNodeIn.GetUpstreamInterface(out usI);
                        FUpstreamInterfaceList.Add(usI);

                        IHttpGUIIO FUpstreamInterface;
                        FUpstreamInterface = usI as IHttpGUIIO;
                        
                        
                        string tNodeName = pNodeIn.Name;
                        int tNumber = Convert.ToInt16(tNodeName.Replace("Input", "")) - 1;

                        FNodeUpstream.Add(pNodeIn.Name, FUpstreamInterface);
                        //FUpstremaListSort.Add(FUpstreamInterface);
                    }
               }
        }



        public void DisconnectPin(IPluginIO Pin)
        {
            FPinIsConnectedDisconnected = true;

            foreach (INodeIn pNodeIn in FInputPinList)
            {
                if (Pin == pNodeIn)
                {
                    string tNodeName = pNodeIn.Name;

                    FNodeUpstream.Remove(pNodeIn.Name);
                    
                }
            }
        }

        #endregion





        #region mainloop

        public void Configurate(IPluginConfig Input)
        {
            if (Input == FInputCount)
            {
                double count;
                FInputCount.GetValue(0, out count);

                int diff = FInputPinList.Count - (int)Math.Round(count);

                if (diff > 0) //delete pins
                {
                    for (int i = 0; i < diff; i++)
                    {
                        INodeIn pinToDelete = FInputPinList[FInputPinList.Count - 1];
                        FInputPinList.Remove(pinToDelete);
                        FNodeUpstream.Remove(pinToDelete.Name);

                        FHost.DeletePin(pinToDelete);
                        pinToDelete = null;
                    }

                }
                else if (diff < 0) //create pins
                {
                    for (int i = 0; i > diff; i--)
                    {
                        INodeIn newPin;

                        FHost.CreateNodeInput("Input" + (FInputPinList.Count + 1), TSliceMode.Dynamic, TPinVisibility.True, out newPin);
                        newPin.SetSubType(new Guid[1] { HttpGUIIO.GUID }, HttpGUIIO.FriendlyName);
                        FInputPinList.Add(newPin);
                    }
                }
            }
        }



        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {

            int TSliceMax = 0;
            for (int i = 0; i < FInputPinList.Count; i++)
            {
                TSliceMax += FInputPinList[i].SliceCount;
            }

            foreach (INodeIn pNodeIn in FInputPinList)
            {
                string tNodeName = pNodeIn.Name;

                IHttpGUIIO FUpstream;
                FNodeUpstream.TryGetValue(pNodeIn.Name, out FUpstream);

                if (FUpstream != null)
                {
                    if (FUpstream.PinIsChanged())
                    {
                        FPinIsChanged = true;
                    }
                }
            }

            if (PinIsChanged())
            {
                mGuiDataList.Clear();
                foreach (INodeIn pNodeIn in FInputPinList)
                {
                    string tNodeName = pNodeIn.Name;

                    IHttpGUIIO FUpstream;
                    FNodeUpstream.TryGetValue(pNodeIn.Name, out FUpstream);

                    if (FUpstream != null)
                    {
                        List<GuiDataObject> tGuiDaten;
                        FUpstream.GetDataObject(0, out tGuiDaten);
                        mGuiDataList.AddRange(tGuiDaten);

                    }
                }
            }

            FPinIsChanged = false;
            FPinIsConnectedDisconnected = false;
        }

        #endregion mainloop





		#region IHttpGUIIO Members


		public bool PinIsChanged()
		{
			return FPinIsChanged ||FPinIsConnectedDisconnected;
		}

		#endregion
	}
}
