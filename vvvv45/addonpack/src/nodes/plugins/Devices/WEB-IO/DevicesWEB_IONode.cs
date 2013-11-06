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
//using System.Windows.Forms;

using System.Threading;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "WEB-IO",
	Category = "Devices",
	//Version = " Wiesemann & Theis Web-IO Digital E/A",
	Help = "alternate Node for the Wiesemann & Theis Web-IO Digital E/A",
	Tags = "")]
	#endregion PluginInfo
	public class DevicesWEB_IONode : IPluginEvaluate, IDisposable
	{
		#pragma warning disable 649
		#region fields & pins
		[Input("Input")]
		public IDiffSpread<bool> FPinsIn;
		
		[Input("Enable", IsSingle = true)]
		public IDiffSpread<bool> FEnable;
		
		[Input("Get Pin States", IsSingle = true, IsBang = true)]
		public IDiffSpread<bool> FGetState;
		
		[Input("Remote Host", IsSingle = true, StringType = StringType.IP)]
		public IDiffSpread<string> FIp;
		
		[Input("Remote Port", IsSingle = true)]
		public IDiffSpread<int> FPort;
		
		[Output("Outputs ")]
		public ISpread<bool> FOutput;
		
		[Output("Inputs")]
		public ISpread<bool> FInputs;
		
		[Output("Status")]
		public ISpread<string> FStatus;
		
		[Output("Connected", DefaultBoolean = false)]
		public ISpread<bool> FConnected;
		
		[Import()]
		public ILogger FLogger;
		
		
		//-------------------------------------
		Socket TCP_Client;
		
		byte[] receiveBuffer = new byte[512];
		byte[] lastReceive = new byte[512];
		
		private bool disposed = false;
		private bool firstConnect = true;
		
		bool[] lastOutputPinState = new bool[12]{false, false, false, false, false, false, false, false, false, false, false, false};
		bool[] lastInputPinState = new bool[12]{false, false, false, false, false, false, false, false, false, false, false, false};
		
		#endregion fields & pins
		#pragma warning restore
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			// Enabled changed
			if (FEnable.IsChanged)
			{
				
				if(FEnable[0] == true)
				{
					connect();
				}
				else
				{
					closeConnection();
					firstConnect = true;
				}
			}
			
			// IP or Port changed
			if (FIp.IsChanged || FPort.IsChanged)
			{
				if (FConnected[0])
				{
					closeConnection();
					connect();
				}
				else
				{
					connect();
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
			
			if(FPinsIn.IsChanged && FConnected[0])
			{
				for (int i = 0; i < 12; i++)
				{
					if(FConnected[0])
					{
						
						if(FPinsIn[i] != lastOutputPinState[i])
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
					}
					lastOutputPinState[i] = FPinsIn[i];
				}
				//FOutput.AssignFrom(lastOutputPinState);
				getPins();
			}
		}
		
		//--------------------------------------------------------------------------------------------
		// METHODS
		//--------------------------------------------------------------------------------------------
		
		
		private void setPins(int outputPin, UInt16 state)
		{
			//this.FLogger.Log(LogType.Debug, "setting Pin " + outputPin + " to " + state);
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
			//this.FLogger.Log(LogType.Debug, "get Pin states");
			structs.RegisterRequest readCmd = new structs.RegisterRequest();
			readCmd.Start_1 = 0;
			readCmd.Start_2 = 0;
			readCmd.StructType = 0x21;
			readCmd.StructLength = 8;
			
			byte[] sendCmd = ByteConvert.ToBytes(readCmd, typeof(structs.RegisterRequest));
			TCP_Client.Send(sendCmd, sendCmd.Length, SocketFlags.None);
			TCP_Client.BeginReceive(receiveBuffer, 0, 512, SocketFlags.None, new AsyncCallback(callback_receive), TCP_Client);
		}
		
		
		private void connect()
		{
			if (FIp[0] != "" && FPort[0] > 0)
			{
				try
				{
					IPEndPoint ClientEP = new IPEndPoint(IPAddress.Parse(FIp[0]), FPort[0]);
                    
                    TCP_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

					TCP_Client.BeginConnect(ClientEP, new AsyncCallback(callback_connect), TCP_Client);
					
				}
				catch (Exception e)
				{
					FLogger.Log(LogType.Debug, "exception while connect: " + e);
					closeConnection();
				}
			}
			else
			{
				FStatus[0] = "IP and Port needed!";
			}
			
		}
		
		
		private void callback_connect(IAsyncResult ar)
		{
			//FLogger.Log(LogType.Debug, "connect callback: " + TCP_Client.ToString() );
			try
			{
				
				TCP_Client.EndConnect(ar);
				
				FStatus[0] = "connected";
				FConnected[0] = true;
								
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Debug, "exception while connect: " + e);
				closeConnection();
                //connect();
			}
			
		}
		
		
		private void callback_receive(IAsyncResult ar)
		{
			//FLogger.Log(LogType.Debug, "receive callback...");
			int receiveCount = 0;
			
			try
			{
				if(TCP_Client != null)
				{
					if (TCP_Client.Connected)
					{
						receiveCount = TCP_Client.EndReceive(ar);
						Array.Copy(receiveBuffer, this.lastReceive, 511);
						Array.Clear(receiveBuffer, 0, 512);
						
						evaluateResponse();
					}
				}
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Debug, "Error while receiving!" + e);
			}
			
			if (receiveCount == 0)
			{
                // why should it close the connection, when nothing comes back?
				//closeConnection();
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
		
		
		private void closeConnection()
		{
			try
			{
				if (TCP_Client != null)
				{
					//FLogger.Log(LogType.Debug, "closing connection to " + TCP_Client.RemoteEndPoint.ToString());
					if (TCP_Client.Connected)
					{
						TCP_Client.Shutdown(SocketShutdown.Both);
						TCP_Client.Close();
						TCP_Client = null;
					}
					TCP_Client = null;
				}
				FStatus[0] = "connection closed";
				FConnected[0] = false;
				
			}
			catch (Exception e)
			{
				FStatus[0] = "Error while disconnecting: " + e;
			}
			
		}
		
		//--------------------------------------------------------------------------------------------
		// DISPOSE
		//--------------------------------------------------------------------------------------------
		
		
		public void Dispose()
		{
			Dispose(true);
			FLogger.Log(LogType.Debug, "GarbageCollector SuppressFinalize");
			GC.SuppressFinalize(this);
			
		}
		
		
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!this.disposed)
			{
				
				if(disposing)
				{
					//this.FLogger.Log(LogType.Debug, "Disposing TCP Client");
					
					if (TCP_Client != null)
					{
						
						if (TCP_Client.Connected)
						{
							//this.FLogger.Log(LogType.Debug, "shutdown TCP Client");
							TCP_Client.Shutdown(SocketShutdown.Both);
							TCP_Client.Close();
							TCP_Client = null;
						}
					}
					
					TCP_Client.Dispose();
				}
				
				// Note disposing has been done.
				disposed = true;
				
			}
		}
		
		
	}
}
