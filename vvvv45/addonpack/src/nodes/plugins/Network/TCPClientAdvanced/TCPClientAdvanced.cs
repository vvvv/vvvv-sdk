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
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Drawing;
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
    	private IValueIn FReadGreedyInput;
    	private IValueIn FReceiveBufferSizeInput;
    	private IValueIn FReceiveTimeoutInput;
    	private IValueIn FSendBufferSizeInput;
    	private IValueIn FSendTimeoutInput;
    	
    	//output pin declaration
    	private IStringOut FOutputStringOutput;
    	private IValueOut FConnectedValueOutput;
    	private IValueOut FOnConnectFailOutput;
    	
    	//the maximum spread count of all input spreads, stored so we can check when it changes
    	private int FSpreadMax = 0;
    	
    	//collection of TCPClients to listen and send on each specified host/port combination
    	private List<TV4TcpClient> TClients = new List<TV4TcpClient>();
    	
    	//buffer for reading from the TCP NetworkStream
    	private byte[] readBuffer = null;
    	
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
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Connects to a TCP server, sends and receives data over the TCP connection. Allows setting of properties on the underlying socket connection.";
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
	    	FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FInputStringInput);
	    	FInputStringInput.SetSubType("", false);	
	    	
	    	FHost.CreateValueInput("Enable", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FEnableValueInput);
	    	FEnableValueInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);	
	    	
	    	FHost.CreateValueInput("Do Send", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDoSendValueInput);
	    	FDoSendValueInput.SetSubType(0.0, 1.0, 1.0, 0.0, true, false, false);
	    	
	    	FHost.CreateStringInput("Remote Host", TSliceMode.Dynamic, TPinVisibility.True, out FRemoteHostStringInput);
	    	FRemoteHostStringInput.SetSubType("localhost", false);	
	    	
	    	FHost.CreateValueInput("Remote Port", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRemotePortValueInput);
	    	FRemotePortValueInput.SetSubType(0.0, 65535.0, 1.0, 4444.0, false, false, true);
	    	
	    	FHost.CreateValueInput("Hold Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHoldOutputInput);
	    	FHoldOutputInput.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
	    	
	    	FHost.CreateValueInput("Read Greedy", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReadGreedyInput);
	    	FReadGreedyInput.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false);
	    	
	    	FHost.CreateValueInput("Receive Buffer Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReceiveBufferSizeInput);
	    	FReceiveBufferSizeInput.SetSubType(1.0, (double)int.MaxValue, 1.0, 8192.0, false, false, true);
	    	
	    	FHost.CreateValueInput("Receive Timeout", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReceiveTimeoutInput);
	    	FReceiveTimeoutInput.SetSubType(0.0, int.MaxValue / 1000.0, 1.0, 0.0, false, false, false);
	    	
	    	FHost.CreateValueInput("Send Buffer Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSendBufferSizeInput);
	    	FSendBufferSizeInput.SetSubType(1.0, (double)int.MaxValue, 1.0, 8192.0, false, false, true);
	    	
	    	FHost.CreateValueInput("Send Timeout", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSendTimeoutInput);
	    	FSendTimeoutInput.SetSubType(0.0, int.MaxValue / 1000.0, 1.0, 0.0, false, false, false);
	    	
	    	//create outputs	    	
	    	FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FOutputStringOutput);
	    	FOutputStringOutput.SetSubType("", false);
	    	
	    	FHost.CreateValueOutput("Connected", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FConnectedValueOutput);
	    	FConnectedValueOutput.SetSubType(0.0, 1.0, 1.0, 0.0, false, true, false);
	    	
	    	FHost.CreateValueOutput("On Connect Fail", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOnConnectFailOutput);
	    	FOnConnectFailOutput.SetSubType(0.0, 1.0, 1.0, 0.0, true, false, false);
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
        				TClients[i] = new TV4TcpClient();
        			}
        		}
        	}
        	
        	//store the slice count so we can check next frame if it has changed
        	FSpreadMax = SpreadMax;

        	bool anyInputsChanged = FInputStringInput.PinIsChanged || FEnableValueInput.PinIsChanged || FDoSendValueInput.PinIsChanged ||
        	    FRemoteHostStringInput.PinIsChanged || FRemotePortValueInput.PinIsChanged || FHoldOutputInput.PinIsChanged
        		|| FReadGreedyInput.PinIsChanged || FReceiveBufferSizeInput.PinIsChanged || FReceiveTimeoutInput.PinIsChanged ||
        		FSendBufferSizeInput.PinIsChanged || FSendTimeoutInput.PinIsChanged;
        	
        	//if any of the inputs has changed
        	if (anyInputsChanged)
        	{
        		//set slicecounts for all outputs
        		//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
        		FOutputStringOutput.SliceCount = SpreadMax;
        		FConnectedValueOutput.SliceCount = SpreadMax;
        		FOnConnectFailOutput.SliceCount = SpreadMax;
        		
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
        		
        		//if we don't have it already,
        		//we need to allocate a readBuffer big enough to handle the largest network read buffer specified on the slices
        		if (readBuffer == null || readBuffer.Length < requiredBufferSize)
        		{
        			readBuffer = new byte[requiredBufferSize];
        		}
        	}
        		
        	//since we are always listening for TCP traffic, we have work to do even if none of the inputs have changed
        	for (int i = 0; i < SpreadMax; i++)
        	{	
        		TV4TcpClient tcpClientSlice = TClients[i];
        		
        		//if the connection is enabled
        		if (tcpClientSlice.FEnabled)
        		{
        			//if the connection hasn't been made yet, try to connect asynchronously
        			if (tcpClientSlice.FConnectStatus == TConnectStatus.NeverConnected)
        			{
        				tcpClientSlice.BeginConnectAndTrackStatus();
        			}
        			
        			//if we are connected
        			if (tcpClientSlice.Connected)
        			{
        				bool readAnythingSlice = false;
        				bool connectionLostSlice = false;
        				try
        				{
        					//get the NetworkStream associated with this TcpClient
        					NetworkStream tcpStreamSlice = tcpClientSlice.GetStream();
        					
        					//--send--
        					
        					if (tcpStreamSlice.CanWrite && tcpClientSlice.FInput != null)
        					{
        						if (tcpClientSlice.FDoSend)
        						{
        							//if the send pin is on and we have data to send, send it
        							byte[] sendBuffer = Encoding.ASCII.GetBytes(tcpClientSlice.FInput);
        							tcpStreamSlice.Write(sendBuffer, 0, sendBuffer.Length);
        						}
        					}
        					
        					
        					//--receive--
        					
        					//if we can read
        					if (tcpStreamSlice.CanRead)
        					{
        						bool dataAvailable = true;
        						//if there is data available to read on the tcp socket, read it and update
        						//the output pin
        						dataAvailable = ReadAndProcessDataSlice(ref readAnythingSlice, i);
        						
        						if (!dataAvailable)
        						{
        							//otherwise, if there is no data available we might have lost the connection, so
        							//test	 the connection
        							int nBytesReceived;
        							bool readData;
        							TestConnection(ref connectionLostSlice, out readData, out nBytesReceived, i);
        							//if we received some data during the test
        							if (readData && nBytesReceived > 0) {
        									//we read real data during our test that needs to be shown
        									//on the output pin
        									ReadAndProcessDataSlice(nBytesReceived, ref readAnythingSlice, i);
        							}
        						}
        					}
        				}
        				catch (ObjectDisposedException objectDisposedException)
        				{
        					//if the TcpClient or NetworkStream is closed, we're also not connected anymore
        					connectionLostSlice = true;
        				}
        				catch (InvalidOperationException invalidOperationException)
        				{
        					//if the socket is closed, we're not connected anymore
        					connectionLostSlice = true;
        				}
        				catch (IOException ioException)
        				{
        					//if a network error occured, we're not connected anymore
        					connectionLostSlice = true;
        				}
        				catch (Exception e)
        				{
        				}
        				finally
        				{
        					if(connectionLostSlice)
        					{
        						//the TcpClient object is now useless so we create a new one to try to reconnect next time
        						//we copy all the data values from the old client object, but not the actual tcp stream object
        						TV4TcpClient newClient = new TV4TcpClient(tcpClientSlice);
        						tcpClientSlice.Close(true);
        						tcpClientSlice = TClients[i] = newClient;
        					}
        					
        					//if we didn't read anything and we're not holding the last read value,
        					//clear the output pin
        					if(!tcpClientSlice.FHoldOutput && !readAnythingSlice)
        					{
        						FOutputStringOutput.SetString(i, "");
        					}
        				}
        			}
        		}
        		//update the "connected" output pin to show whether we are connected
        		FConnectedValueOutput.SetValue(i, tcpClientSlice.Connected ? 1.0 : 0.0);
        		//update the On Connect Fail pin to show if there were any failed connection attempts
        		//since the end of the last frame
        		FOnConnectFailOutput.SetValue(i, tcpClientSlice.FConnectFailSinceLastEvaluate ? 1.0 : 0.0);
        		//start tracking connection failures anew in case any occur between now and the next evaluate call
        		tcpClientSlice.FConnectFailSinceLastEvaluate = false;
        	}
        }
        
        #endregion mainloop
        
        #region helper functions
        protected void ProcessInputsSlice(int sliceIndex)
        {
        	String inputStringSlice;
        	double enabledValueSlice;
        	double doSendValueSlice;
        	String remoteHostStringSlice;
        	double remotePortValueSlice;
        	double holdOutputSlice;
        	double readGreedySlice;
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
        	FReadGreedyInput.GetValue(sliceIndex, out readGreedySlice);
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
        	if (tcpClientSlice.FConnectStatus != TConnectStatus.NeverConnected &&
        	    (enabledValueSlice <= 0.5 || remoteHostChangedSlice || remotePortChangedSlice))
        	{
        		//if the TcpClient object ever connected it cannot be used for another connection,
        		//so we create a new one for any new connection that this slice might make
        		tcpClientSlice.Close(true);
        		tcpClientSlice = TClients[sliceIndex] = new TV4TcpClient();
        		FOutputStringOutput.SetString(sliceIndex, "");
        		FOnConnectFailOutput.SetValue(sliceIndex, 0);
        	}
        	
        	//we need the data every frame even if none of it has changed, so we store a persistent copy of everything
        	tcpClientSlice.FInput = inputStringSlice;
        	tcpClientSlice.FEnabled = enabledValueSlice > 0.5;
        	tcpClientSlice.FDoSend = doSendValueSlice > 0.5;
        	tcpClientSlice.FRemoteHost = remoteHostStringSlice;
        	tcpClientSlice.FRemotePort = (int)remotePortValueSlice;
        	tcpClientSlice.FHoldOutput = holdOutputSlice > 0.5;
        	tcpClientSlice.FReadGreedy = readGreedySlice > 0.5;
			tcpClientSlice.ReceiveBufferSize = (int)receiveBufferSizeSlice;
			tcpClientSlice.ReceiveTimeout = (int)(receiveTimeoutSlice * 1000.0);
        	tcpClientSlice.SendBufferSize = (int)sendBufferSizeSlice;
        	tcpClientSlice.SendTimeout = (int)(sendTimeoutSlice * 1000.0);
        }
        
        
        protected bool ReadAndProcessDataSlice(int nBytesAlreadyRead, ref bool readAnything, int sliceIndex)
        {
        	TV4TcpClient tcpClientSlice = TClients[sliceIndex];
        	NetworkStream tcpStreamSlice = tcpClientSlice.GetStream();
        	//we will collect a string of all character data received
        	String dataReceived = "";
        	bool dataAvailable = false;
        	        	        	
        	//if we already read some data, stringify it and add it to the end of our collected string
        	if (readBuffer != null && nBytesAlreadyRead > 0)
        	{
        		dataReceived = String.Concat(dataReceived,
        		                             System.Text.Encoding.ASCII.GetString(readBuffer, 0, nBytesAlreadyRead));
        	    readAnything = true;
        	    dataAvailable = true;
        	}
        	                             
        	int nBytesReceived;
        	
        	//if there is data available, then read it
        	while (tcpStreamSlice.DataAvailable)
        	{
        		dataAvailable = true;
        		nBytesReceived = tcpStreamSlice.Read(readBuffer, 0, readBuffer.Length);
        		if (nBytesReceived > 0)
        		{
        			//note the fact that we read something
        			readAnything = true;
        			//add the data to the end of our collected string
        			dataReceived = String.Concat(dataReceived,
        			                             System.Text.Encoding.ASCII.GetString(readBuffer, 0, nBytesReceived));
        		}
        							
        		//if we're not doing greedy reading, then we either have read everything or our
        		//buffer is full, so we're done reading
        		if (!tcpClientSlice.FReadGreedy) break;
        		//if we are doing greedy reading, the loop will continue until all available data has been read
        	}
        	
        	if (readAnything)
        	{
        		//update the output pin with the string of read data that we collected
        		FOutputStringOutput.SetString(sliceIndex, dataReceived);
        	}
        	
        	return dataAvailable;
        }
        
        protected bool ReadAndProcessDataSlice(ref bool readAnything, int sliceIndex)
        {
        	return ReadAndProcessDataSlice(0, ref readAnything, sliceIndex);
        }
        
        protected void TestConnection(ref bool connectionLost, out bool readData, out int nBytesReceived, int sliceIndex)
        {
        	TV4TcpClient tcpClientSlice = TClients[sliceIndex];	
        	Socket client = tcpClientSlice.Client;
        	readData = true;
        	nBytesReceived = 0;
        	bool blockingState = client.Blocking;
        	
        	try
        	{
        		//we do a read that is guaranteed not to block
        		client.Blocking = false;
        		nBytesReceived = client.Receive(readBuffer);
        	}
        	catch (SocketException socketException)
        	{
        		//the non-blocking read failed
        		readData = false;
        		
        		// 10035 == WSAEWOULDBLOCK
        		//the read may have failed because it WOULD have blocked, ie the
        		//connection is still there, but nothing has been sent
        		if (!(socketException.NativeErrorCode.Equals(10035)))
        		{
        			//however, if there was some other exception, we've lost the connection
        			connectionLost = true;
        		}
        	}
        	finally
        	{
        		client.Blocking = blockingState;
        	}
        	
        	if (readData)
        	{
        		//if we received 0 bytes on a non-blocking read, we've lost the connection
        		if (nBytesReceived == 0)
        		{
        			connectionLost = true;
        		}
        	}
        }
        
        #endregion helper functions
	}    
}
