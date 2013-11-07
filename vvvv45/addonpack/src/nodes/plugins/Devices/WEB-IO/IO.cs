#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;


using System.Net;
using System.Net.Sockets;

//using System.Text;
using System.Text.RegularExpressions;

using System.Threading;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "IO",
	Category = "Devices",
    Version = "Wiesemann+Theis Web-IO",
	Help = "alternative Node for the Wiesemann & Theis 12x Web-IO Digital E/A",
	Tags = "IO, Devices",
    Author = "sebl",
    Credits = " Wiesemann & Theis (http://www.wut.de/e-5763w-13-inde-000.php)")]
	#endregion PluginInfo
	public class IO : IPluginEvaluate, IDisposable
	{
		#pragma warning disable 649
		#region fields & pins
		[Input("Output")]
		public IDiffSpread<bool> FPinsIn;
		
		[Input("Enabled", IsSingle = true, DefaultBoolean = false)]
		public IDiffSpread<bool> FEnable;
		
		[Input("Get Pin States", IsSingle = true, IsBang = true)]
		public IDiffSpread<bool> FGetState;
		
		[Input("Remote Host", IsSingle = true, StringType = StringType.IP, DefaultString = "192.168.1.1")]
		public IDiffSpread<string> FIp;
		
		[Input("Remote Port", IsSingle = true, DefaultValue = 49153)]
		public IDiffSpread<int> FPort;
		
		[Output("Actual Output")]
		public ISpread<bool> FOutput;
		
		[Output("Input")]
		public ISpread<bool> FInputs;
		
		[Output("Status")]
		public ISpread<string> FStatus;
		
		[Output("Connected", DefaultBoolean = false)]
		public ISpread<bool> FConnected;
		
		[Import()]
		public ILogger FLogger;
		
		
		//-------------------------------------
        //[ImportingConstructor]
        //public IO()
        //{
        private Socket TCP_Client;

        private byte[] receiveBuffer = new byte[512];
        private byte[] lastReceive = new byte[512];

        private bool[] lastOutputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        private bool[] lastInputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        //}

        private bool disposed = false;
        private bool firstConnect = true;

		#endregion fields & pins
		#pragma warning restore
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

            FOutput.SliceCount = 12;
            FInputs.SliceCount = 12;

			// Enabled changed
            if (FEnable.IsChanged || FIp.IsChanged || FPort.IsChanged)
			{
                FConnected[0] = false;

				if(FEnable[0] == true)
				{
                    if (FConnected[0])
                        closeConnection(true);
					connect();
				}
				else
				{
					closeConnection(false);
					firstConnect = true;
				}
			}

			// on first connect
			if (firstConnect && FEnable[0] && FConnected[0])
			{
				getPins();
				FOutput.AssignFrom(lastOutputPinState);
				FInputs.AssignFrom(lastInputPinState);
				firstConnect = false;
			}
			
			// getpinState manually
			if (FGetState[0] && FEnable[0] && FConnected[0])
			{
				getPins();
				FOutput.AssignFrom(lastOutputPinState);
				FInputs.AssignFrom(lastInputPinState);
			}
			
            // set Output Pins
			if(FPinsIn.IsChanged && FConnected[0])
			{
				for (int i = 0; i < 12; i++)
				{
                    //if (FConnected[0])
                    //{
						if (FPinsIn[i] != lastOutputPinState[i])
						{
							if(FPinsIn[i])
							{
								setPins(i, (UInt16)1);
							}
							else
							{
								setPins(i, (UInt16)0);
							}
						}
					//}
					lastOutputPinState[i] = FPinsIn[i];
				}
				getPins();
			}
		}
	

		//--------------------------------------------------------------------------------------------
		// METHODS
		//--------------------------------------------------------------------------------------------
		
		
		private void setPins(int outputPin, UInt16 state)
		{
			int CurOutputNo = outputPin;
			
			structs.setBit cmd = new structs.setBit();
			cmd.Start_1 = 0;
			cmd.Start_2 = 0;
			cmd.StructType = 0x9;
			cmd.StructLength = 12;
			cmd.Mask =  (UInt16)Math.Pow(2, CurOutputNo);
			
			if (state == 1)
			{
				cmd.Value = (UInt16)Math.Pow(2, CurOutputNo);
			}
			else
			{
				cmd.Value = 0;
			}
			
			byte[] sendCmd = ByteConvert.ToBytes(cmd, typeof(structs.setBit));
			TCP_Client.Send(sendCmd, sendCmd.Length, SocketFlags.None);
		}
		
		
		private void getPins()
		{
			structs.RegisterRequest readCmd = new structs.RegisterRequest();
			readCmd.Start_1 = 0;
			readCmd.Start_2 = 0;
			readCmd.StructType = 0x21;
			readCmd.StructLength = 8;
			
			byte[] sendCmd = ByteConvert.ToBytes(readCmd, typeof(structs.RegisterRequest));

            if (FConnected[0] && TCP_Client.Connected)
            {
                TCP_Client.Send(sendCmd, sendCmd.Length, SocketFlags.None);
                TCP_Client.BeginReceive(receiveBuffer, 0, 512, SocketFlags.None, new AsyncCallback(callback_receive), TCP_Client);
            }
		}
		
		
		private void connect()
		{
            if (IsValidIP(FIp[0]) && FPort[0] > 0 && FPort[0] < 65535)
			{
				try
				{
                    //FLogger.Log(LogType.Debug, "connecting to " + FIp[0] + ":" + FPort[0]);
					IPEndPoint ClientEP = new IPEndPoint(IPAddress.Parse(FIp[0]), FPort[0]);                    
                    TCP_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    TCP_Client.BeginConnect(ClientEP, new AsyncCallback(callback_connect), TCP_Client);

				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Debug, "exception in connect(): " + e);
					closeConnection(true);
				}
			}
			else
			{
				FStatus[0] = "IP and Port needed or in wrong format";
			}
			
		}
		
		
		private void callback_connect(IAsyncResult ar)
		{
			try
			{
                bool complete = ar.IsCompleted;

                if (complete)
                {
                    Socket socket = (Socket)ar.AsyncState;
                    socket.EndConnect(ar);

                    FStatus[0] = "connected";
                    FConnected[0] = true;

                }
                else
                {
                    //didn't connect > retry
                    FLogger.Log(LogType.Debug, "connection not established");
                    closeConnection(true);
                }

			}
			catch (Exception e)
			{
                FLogger.Log(LogType.Debug, "exception in callback_connect(): " + e);
                FStatus[0] = "connection error (refused)";
				closeConnection(true);
			}		
		}
		
		
		private void callback_receive(IAsyncResult ar)
		{
			int receiveCount = 0;
			
			try
			{
                if (TCP_Client != null)
				{
                    if (TCP_Client.Connected)
					{
                        receiveCount = TCP_Client.EndReceive(ar);
						Array.Copy(receiveBuffer, lastReceive, 511);
						Array.Clear(receiveBuffer, 0, 512);
						
						evaluateResponse();
					}
				}
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Debug, "Error while receiving!" + e);
			}
		}
		
		
		private void evaluateResponse()
		{
			
			# region test response
			/*
			switch(lastReceive[4])
			{
				case 8:
				FLogger.Log(LogType.Debug, "case 8");
				break;
				
				case 0x31:
				FLogger.Log(LogType.Debug, "case 0x31");
				break;
				
				case 0xB4:
				FLogger.Log(LogType.Debug, "case 0xB4");
				break;
				
				case 0xB5:
				FLogger.Log(LogType.Debug, "case 0xB5");
				break;
				
			}
			*/
			#endregion
			
			try
			{
				structs.registerState RegisterState = (structs.registerState)ByteConvert.ToStruct(lastReceive, typeof(structs.registerState));
				
				for(int port = 0; port<12; port++)
				{
					bool outputPinstatus = (RegisterState.OutputValue & (UInt16)Math.Pow(2, port)) == Math.Pow(2, port);
					//FLogger.Log(LogType.Debug, "Output Pinstatus " + port + " is: " + outputPinstatus);
					
					if (outputPinstatus)
					{
						lastOutputPinState[port] = true;
					}
					else
					{
						lastOutputPinState[port] = false;
					}
					
					
					bool inputPinstatus = (RegisterState.InputValue & (UInt16)Math.Pow(2, port)) == Math.Pow(2, port);
					//FLogger.Log(LogType.Debug, "Input  Pinstatus " + port + " is: " + inputPinstatus);
					
					if (inputPinstatus)
					{
						lastInputPinState[port] = true;
					}
					else
					{
						lastInputPinState[port] = false;
					}
				}

				FOutput.AssignFrom(lastOutputPinState);
				FInputs.AssignFrom(lastInputPinState);

			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Debug, "Error while parsing return data: " + e);
			}
			
		}
		
		
		private void closeConnection(bool reconnect)
		{
			try
			{
                if (TCP_Client != null)
				{
                    if (TCP_Client.Connected)
					{
                        if (reconnect)
                            TCP_Client.Disconnect(true);
                        else
                            TCP_Client.Disconnect(false);

                        TCP_Client.Shutdown(SocketShutdown.Both);
                        TCP_Client.Close();
                        TCP_Client.Dispose();
                        //TCP_Client = null;
                        
					}
				}

				FConnected[0] = false;
                FStatus[0] = "disconnected";

                if (reconnect && FEnable[0])
                {
                    FLogger.Log(LogType.Debug, "reconnect in 2 seconds...");
                    Thread.Sleep(2000);
                    connect();
                }

			}
			catch (Exception e)
			{
				FStatus[0] = "Error while disconnecting: " + e;
			}
		}

        //--------------------------------------------------------------------------------------------
        // Helpers
        //--------------------------------------------------------------------------------------------


        public bool IsValidIP(string addr)
        {
            string pattern = "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$";
            Regex check = new Regex(pattern);

            bool valid = false;

            if (addr == "")
            {
                valid = false;
                //FLogger.Log(LogType.Debug, "no address provided");
            }
            else
            {
                valid = check.IsMatch(addr, 0);
            }
            return valid;
        }

		
		//--------------------------------------------------------------------------------------------
		// DISPOSE
		//--------------------------------------------------------------------------------------------
			
		public void Dispose()
		{
            FLogger.Log(LogType.Debug, "Dispose...");
            GC.SuppressFinalize(this);
			Dispose(true);
            GC.Collect();
            FLogger.Log(LogType.Debug, "disposed");
			
		}
		
		
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!this.disposed)
			{
				/*
				if(disposing)
				{

                    if (TCP_Client != null)
					{

                        if (TCP_Client.Connected)
						{
                            TCP_Client.Shutdown(SocketShutdown.Both);
                            TCP_Client.Close();
						}
                        this.TCP_Client = null;
					}
				}*/
                //Thread.Sleep(3000);
                closeConnection(false);
				// Note disposing has been done.
				disposed = true;
			}
		}


        ~IO()
        {
            /*
            if (TCP_Client != null)
            {

                if (TCP_Client.Connected)
                {
                    TCP_Client.Shutdown(SocketShutdown.Both);
                    TCP_Client.Close();
                }
                TCP_Client = null;
            }*/
            closeConnection(false);
        }
		
		
	}
}
