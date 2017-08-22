#region licence/info

//////project name
//vvvv TCP Client Advanced

//////description
//Re-implementation and extension of vvvv standard TCP Client node.

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
//iceberg (Joshua Samberg)

#endregion licence/info

//use what you need
using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;


//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class TCPClientAdvanced: IPlugin, IDisposable
    {	   
       	


    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
	
    	//input pin declaration
    	private IStringIn FInputStringInput;
    	private IValueIn FEnableValueInput;
    	private IValueIn FDoSendValueInput;
    	private IStringIn FRemoteHostStringInput;
    	private IValueIn FRemotePortValueInput;
    	private IValueIn FHoldOutputInput;
    	private IValueIn FReceiveBufferSizeInput;
    	private IValueIn FReceiveTimeoutInput;
    	private IValueIn FSendBufferSizeInput;
    	private IValueIn FSendTimeoutInput;
    	
    	//output pin declaration
    	private IStringOut FOutputStringOutput;
        private IValueOut FReceivedNewData;
    	private IValueOut FConnectedValueOutput;
    	
    	//the maximum spread count of all input spreads, stored so we can check when it changes
    	private int FSpreadMax = 0;
    	
    	//collection of TCPClients to listen and send on each specified host/port combination
    	private List<TV4TcpClient> TClients = new List<TV4TcpClient>();
    	//buffer for reading from the TCP NetworkStream
    	
    	#endregion field declaration
       


    	#region constructor/destructor
    	
        public TCPClientAdvanced()
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
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			//close all the open TcpClients
        			for (int i = 0; i < TClients.Count; i++)
        			{
        				TClients[i].Close(true);
        			}
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "TCPDotNet is being deleted");
        		
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
        ~TCPClientAdvanced()
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
					FPluginInfo.Name = "TCP";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Network";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Client Advanced";
					
					//the nodes author: your sign
					FPluginInfo.Author = "phlegma";
					//describe the nodes function
					FPluginInfo.Help = "Connects to a TCP server, sends and receives data over the TCP connection. Allows setting of properties on the underlying socket connection.";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "iceberg";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "Sending is not habdeld in a seperate thread";
					
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
        	get {return true;}
        }
        
        #endregion node name and infos
        


      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
            FHost.CreateStringInput("Remote Host", TSliceMode.Dynamic, TPinVisibility.True, out FRemoteHostStringInput);
            FRemoteHostStringInput.SetSubType("localhost", false);

            FHost.CreateValueInput("Remote Port", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRemotePortValueInput);
            FRemotePortValueInput.SetSubType(0.0, 65535.0, 1.0, 4444.0, false, false, true);

	    	FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FInputStringInput);
	    	FInputStringInput.SetSubType("", false);	
	    	
	    	FHost.CreateValueInput("Send", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDoSendValueInput);
	    	FDoSendValueInput.SetSubType(0.0, 1.0, 1.0, 0.0, true, false, false);
	    	
	    	FHost.CreateValueInput("Hold", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHoldOutputInput);
	    	FHoldOutputInput.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
	    
	    	FHost.CreateValueInput("Receive Buffer Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReceiveBufferSizeInput);
	    	FReceiveBufferSizeInput.SetSubType(1.0, (double)int.MaxValue, 1.0, 8192.0, false, false, true);
	    	
	    	FHost.CreateValueInput("Receive Timeout", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReceiveTimeoutInput);
	    	FReceiveTimeoutInput.SetSubType(0.0, int.MaxValue / 1000.0, 1.0, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Send Buffer Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSendBufferSizeInput);
	    	FSendBufferSizeInput.SetSubType(1.0, (double)int.MaxValue, 1.0, 8192.0, false, false, true);
	    	
	    	FHost.CreateValueInput("Send Timeout", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSendTimeoutInput);
	    	FSendTimeoutInput.SetSubType(0.0, int.MaxValue / 1000.0, 1.0, 0.0, false, false, false);

            FHost.CreateValueInput("Enable", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FEnableValueInput);
            FEnableValueInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);	
	    	 
	    	//create outputs	    	
	    	FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FOutputStringOutput);
	    	FOutputStringOutput.SetSubType("", false);

            FHost.CreateValueOutput("NewData", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReceivedNewData);
            FReceivedNewData.SetSubType(0, 1, 1, 0, true, false, true);

	    	FHost.CreateValueOutput("Connected", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FConnectedValueOutput);
	    	FConnectedValueOutput.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
	    	
        }

        #endregion pin creation


        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            
        	//if the slice count has changed
        	if (SpreadMax != FSpreadMax)
        	{
        		int nClients = TClients.Count;
        		//we need a TcpClient instance for every slice
        		if (SpreadMax > nClients)
        		{
        			for (int i = 0; i < SpreadMax - nClients; i++)
        			{
        				TV4TcpClient newClient = new TV4TcpClient();
        				TClients.Add(newClient);
        			}
        		}
        		else if (SpreadMax < nClients)
        		{
        			//if we have connected TcpClients for extra slices that have been removed, we need to disconnect them
        			for (int i = SpreadMax; i < TClients.Count; i++)
        			{
        				TClients[i].Close(true);
        				//TClients[i] = new TV4TcpClient();
        			}
        		}
            }
            //store the slice count so we can check next frame if it has changed
        	FSpreadMax = SpreadMax;

        	bool anyInputsChanged = FInputStringInput.PinIsChanged || FEnableValueInput.PinIsChanged || FDoSendValueInput.PinIsChanged ||
        	    FRemoteHostStringInput.PinIsChanged || FRemotePortValueInput.PinIsChanged || FHoldOutputInput.PinIsChanged
        		|| FReceiveBufferSizeInput.PinIsChanged || FReceiveTimeoutInput.PinIsChanged ||
        		FSendBufferSizeInput.PinIsChanged || FSendTimeoutInput.PinIsChanged; 
        	
        	//if any of the inputs has changed
        	if (anyInputsChanged)
        	{
        		//set slicecounts for all outputs
        		//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
        		FOutputStringOutput.SliceCount = SpreadMax;
        		FConnectedValueOutput.SliceCount = SpreadMax;
                FReceivedNewData.SliceCount = SpreadMax;
        		
        		int requiredBufferSize = 1;
        		        		
        		//read and process the new input values
        		for (int i = 0; i < SpreadMax; i++)
        		{
        			ProcessInputsSlice(i);
        			//keep track of the largest network buffer size on all of the slices
        			if (TClients[i].ReceiveBufferSize > requiredBufferSize)
        			{
        				requiredBufferSize = TClients[i].ReceiveBufferSize;
        			}
        		}
        	}

	
        	//since we are always listening for TCP traffic, we have work to do even if none of the inputs have changed
        	for (int i = 0; i < SpreadMax; i++)
        	{
                try
                {
                    
                    TV4TcpClient tcpClientSlice = TClients[i];
                    FReceivedNewData.SetValue(i, 0);

                    //if the connection is enabled
                    if (tcpClientSlice.FEnabled)
                    {
                        //if the connection hasn't been made yet, try to connect asynchronously
                        if (tcpClientSlice.ConnectStatus == TConnectStatus.NeverConnected)
                        {
                            tcpClientSlice.BeginConnectAndTrackStatus();
                        }

                        //if we didn't read anything and we're not holding the last read value,
                        //clear the output pin
                        if (!tcpClientSlice.FHoldOutput)
                        {
                            FOutputStringOutput.SetString(i, "");
                        }


                        if (tcpClientSlice.ConnectStatus == TConnectStatus.Connected)
                        {

                            //set ConnectPin to 1
                            FConnectedValueOutput.SetValue(i, 1);

                            //-- send --
                            if (tcpClientSlice.FInput != null)
                            {
                                if (tcpClientSlice.FDoSend)
                                {
                                    tcpClientSlice.Send();
                                }
                            }


                            // -- Read --
                            if (tcpClientSlice.IsReading == false)
                            {
                                string dataReceived = tcpClientSlice.GetReadData();


                                if (dataReceived != null)
                                {
                                    FReceivedNewData.SetValue(i, 1);
                                    FOutputStringOutput.SetString(i, dataReceived);
                                    //Debug.WriteLine("Length: " + dataReceived.Length);
                                }

                                tcpClientSlice.Read();
                            }

                        }

                        if (tcpClientSlice.ConnectStatus == TConnectStatus.ConnectionLost || tcpClientSlice.ConnectStatus == TConnectStatus.Disconnected)
                        {
                            FConnectedValueOutput.SetValue(i, 0);
                            //the TcpClient object is now useless so we create a new one to try to reconnect next time
                            //we copy all the data values from the old client object, but not the actual tcp stream object
                            TV4TcpClient newClient = new TV4TcpClient(tcpClientSlice);
                            tcpClientSlice.Close(true);
                            tcpClientSlice = TClients[i] = newClient;
                        }
                    }
                    //update the "connected" output pin to show whether we are connected
                    FConnectedValueOutput.SetValue(i, tcpClientSlice.Connected ? 1.0 : 0.0);
                    


                    //Reading Error MEssages formt the TCPClient Class
                    List<string> Errors;
                    Errors = tcpClientSlice.GetErrorMessages();
                    if (Errors != null)
                    {
                        foreach (string Error in Errors)
                        {
                            FHost.Log(TLogType.Error, Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    FHost.Log(TLogType.Error, ex.Message);
                }
            }
        }
        
        #endregion mainloop
       
 

        #region Get InputPin Values

        protected void ProcessInputsSlice(int sliceIndex)
        {
        	String inputStringSlice;
        	double enabledValueSlice;
        	double doSendValueSlice;
        	String remoteHostStringSlice;
        	double remotePortValueSlice;
        	double holdOutputSlice;
			double receiveBufferSizeSlice;
			double receiveTimeoutSlice;
        	double sendBufferSizeSlice;
        	double sendTimeoutSlice;
        	bool remoteHostChangedSlice;
        	bool remotePortChangedSlice;
        	
        	TV4TcpClient tcpClientSlice = TClients[sliceIndex];
        	
        	//read data from inputs
        	FInputStringInput.GetString(sliceIndex, out inputStringSlice);
        	FEnableValueInput.GetValue(sliceIndex, out enabledValueSlice);
        	FDoSendValueInput.GetValue(sliceIndex, out doSendValueSlice);
        	FRemoteHostStringInput.GetString(sliceIndex, out remoteHostStringSlice);
        	FRemotePortValueInput.GetValue(sliceIndex, out remotePortValueSlice);
        	FHoldOutputInput.GetValue(sliceIndex, out holdOutputSlice);
			FReceiveBufferSizeInput.GetValue(sliceIndex, out receiveBufferSizeSlice);
			FReceiveTimeoutInput.GetValue(sliceIndex, out receiveTimeoutSlice);
        	FSendBufferSizeInput.GetValue(sliceIndex, out sendBufferSizeSlice);
        	FSendTimeoutInput.GetValue(sliceIndex, out sendTimeoutSlice);
        	
        	//if the remote server or remote port pin was changed on this slice we're going to have to close
        	//the connection so we can create a new one to the new host and port
        	if (tcpClientSlice.FRemoteHost != null && FRemoteHostStringInput.PinIsChanged)
        	{
        		remoteHostChangedSlice = !(tcpClientSlice.FRemoteHost.Equals(remoteHostStringSlice));
        	}
        	else
        	{
        		remoteHostChangedSlice = false;
        	}
        	if (FRemotePortValueInput.PinIsChanged)
        	{
        		remotePortChangedSlice = tcpClientSlice.FRemotePort != remotePortValueSlice;
        	}
        	else
        	{
        		remotePortChangedSlice = false;
        	}
        	
        	//if the enabled value changed to zero, we need to close the connection
        	//if the remote server or port pin was changed, we will need to connect to the new host and port
        	if (tcpClientSlice.ConnectStatus != TConnectStatus.NeverConnected &&
        	    (enabledValueSlice <= 0.5 || remoteHostChangedSlice || remotePortChangedSlice))
        	{
        		//if the TcpClient object ever connected it cannot be used for another connection,
        		//so we create a new one for any new connection that this slice might make
        		tcpClientSlice.Close(true);
        		tcpClientSlice = TClients[sliceIndex] = new TV4TcpClient();
        		FOutputStringOutput.SetString(sliceIndex, "");
                FConnectedValueOutput.SetValue(sliceIndex, 0);
        	}
        	
        	//we need the data every frame even if none of it has changed, so we store a persistent copy of everything
        	tcpClientSlice.FInput = inputStringSlice;
        	tcpClientSlice.FEnabled = enabledValueSlice > 0.5;
        	tcpClientSlice.FDoSend = doSendValueSlice > 0.5;
        	tcpClientSlice.FRemoteHost = remoteHostStringSlice;
        	tcpClientSlice.FRemotePort = (int)remotePortValueSlice;
        	tcpClientSlice.FHoldOutput = holdOutputSlice > 0.5;
			tcpClientSlice.ReceiveBufferSize = (int)receiveBufferSizeSlice;
			tcpClientSlice.ReceiveTimeout = (int)(receiveTimeoutSlice * 1000.0);
        	tcpClientSlice.SendBufferSize = (int)sendBufferSizeSlice;
        	tcpClientSlice.SendTimeout = (int)(sendTimeoutSlice * 1000.0);
        }

        #endregion Get InputPin Values
	}    
}
