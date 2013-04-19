/*
 * Created by SharpDevelop.
 * User: admin
 * Date: 04.09.2009
 * Time: 14:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace VVVV.Nodes
{
	public enum TConnectStatus
	{
			NeverConnected,
			Connecting,
			Connected,
			Disconnected,
            ConnectionLost
	};

	
	/// <summary>
	/// Extends the TcpClient class to allow it to store a copy of the relevant
	/// VVVV node inputs for the slice that the TcpClient is associated with.  Also
	/// implements additional helpful behaviors such as keeping track of when the
	/// TcpClient is in the middle of an ansychronous connection attempt.
	/// </summary>
	public class TV4TcpClient: TcpClient
	{




        #region Field Deglaration
        public static ManualResetEvent allDone = new ManualResetEvent(false);
		//these members match the inputs of the TCPDotNet VVVV node that are not
		//already recorded using the same data type in the TcpClient class
		public String FInput = "";
		public bool FEnabled = true;
		public bool FDoSend = false;
		public String FRemoteHost = null;
		public int FRemotePort = 0;
		//if true, we keep the last read data on the output pin until something new is read
		//if false, on every frame where nothing is read, we clear the output pin
		public bool FHoldOutput = false;	
		//if true, we keep reading in a given frame until all available data has been read
        //if false, we issue only one read command, which will read up to the number of bytes in our buffer
		
		//We need to keep track of the status of this connection a little bit more
		//carefully than the .NET TcpClient does by default
		private TConnectStatus FConnectStatus = TConnectStatus.NeverConnected;

        private Object ReadLock = new Object();
        private Object WriteLock = new Object();
        private Object ErrorLock = new Object();

        private List<string> FDataStorage = new List<string>();
        private string FDataReceived = String.Empty;
        private string FTestReceivedData = String.Empty;

        private Thread FReadThread;

        private int FNewData = 0;

        public TConnectStatus ConnectStatus
        {
            get
            {
                return FConnectStatus;
            }
        }


		//this is where we record if any connection attempts failed since the last Evaluate() call
		//We need to store the information because we can't inform a consumer until the
		//next time the node's Evaluate() function is called
		public bool FConnectFailSinceLastEvaluate = false;

        //a List of Error Messages which are read by vvvv every Frame.
        private List<string> FExceptions = new List<string>();


        private bool FReadData = false;




        /// <summary>
        /// Shows if the TCPCLient is Reading Data from the Socket
        /// </summary>
        public bool IsReading
        {
            get
            {
                return FReadData;
            }
        }


         




        #endregion Field Deglaration




        #region Connection


        //starts an asynchronous attempt to connect to the specified server and port
		public void BeginConnectAndTrackStatus()
		{
			//make sure we have a server string and a valid port number
			if (FRemoteHost != null && FRemotePort >= 0 && FRemotePort <= 65535)
			{
				FConnectStatus = TConnectStatus.Connecting;
				try
        		{
                    base.BeginConnect(FRemoteHost, FRemotePort, new AsyncCallback(this.ConnectCallback), this); 
        		}
        		catch (Exception e)
        		{
        			FConnectStatus = TConnectStatus.NeverConnected;
        			FConnectFailSinceLastEvaluate = true;
                    AddErrorMessage(e.Message);
        		}
				
			}
		}
		
        //Connect Callback
		protected void ConnectCallback(IAsyncResult asyncResult)
		{
			TConnectStatus connectStatus = TConnectStatus.Connected;
			
			try
			{
				EndConnect(asyncResult);

			}
			catch (Exception e)
			{
				connectStatus = TConnectStatus.NeverConnected;
				FConnectFailSinceLastEvaluate = true;
                AddErrorMessage(e.Message);
			}

            FReadData = false;
            FConnectStatus = connectStatus;
		}
		
        //Closes the Connection 
		public void Close(bool trackConnectStatus)
		{
			if (trackConnectStatus)
			{
				FConnectStatus = TConnectStatus.Disconnected;
			}

            FReadData = false;
			base.Close();
        }

        #endregion Connection




        #region Sending / Reading Data


        //Cobverts the given Input string and send it to the Socket
        public void Send()
        {
            try
            {
                byte[] sendBuffer = Encoding.ASCII.GetBytes(FInput);
                this.GetStream().Write(sendBuffer, 0, sendBuffer.Length);
            }
            catch (Exception ex)
            {
                AddErrorMessage(ex.Message);
            }
        }

        //init a new thread to read data from the socket
        public void Read()
        {
            if (FReadThread.IsAlive == false && FConnectStatus == TConnectStatus.Connected)
            {
                FReadThread = new Thread(new ThreadStart(ReadToSocket));
                FReadThread.IsBackground = true;
                FReadThread.Start();
            }
        }

        //Read the data in form the Socket in a seperate Thread
        private void ReadToSocket()
        {

            FReadData = true;
            int nBytesReceived = 0;
            byte[] ReadBuffer = new byte[ReceiveBufferSize];

            try
            {
                NetworkStream Stream = this.GetStream();
                Client.Blocking = true;

                //check if the stream can Read
                if (Stream.CanRead == true)
                {
                    //check if data available on the stream
                    if (Stream.DataAvailable)
                    {
                        do
                        {
                            //Write the data from the Stream to the Buffer and check how many bytes received
                            string ReceivedString = String.Empty;
                            nBytesReceived = Stream.Read(ReadBuffer, 0, ReadBuffer.Length);

                            // if we received some bytes we convert them to a string
                            if (nBytesReceived > 0)
                            {
                                FDataReceived += System.Text.Encoding.ASCII.GetString(ReadBuffer, 0, nBytesReceived);
                            }

                        } while (Stream.DataAvailable);
                    }
                    else
                    {
                        //If there is no data to read we chack if the Server is still availabel;
                        //Try to Read the Socket and Check if the Server is still available
                        TestConnection(ref nBytesReceived, ref ReadBuffer);

                        //If the Server is still availbaleand we received bytes we send them to vvvv
                        if (nBytesReceived > 0)
                        {
                              //If we received data save this and add it to the rest we will received. 
                              FDataReceived += System.Text.Encoding.ASCII.GetString(ReadBuffer, 0, nBytesReceived);
                        }
                    }
                }
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                //if the TcpClient or NetworkStream is closed, we're also not connected anymore
                FConnectStatus = TConnectStatus.ConnectionLost;
                AddErrorMessage(objectDisposedException.Message);

            }
            catch (InvalidOperationException invalidOperationException)
            {
                //if the socket is closed, we're not connected anymore
                FConnectStatus = TConnectStatus.ConnectionLost;
                AddErrorMessage(invalidOperationException.Message);
            }
            catch (IOException ioException)
            {
                //if a network error occured, we're not connected anymore
                FConnectStatus = TConnectStatus.ConnectionLost;
                AddErrorMessage(ioException.Message);
            }
            catch (Exception e)
            {
                AddErrorMessage(e.Message);
            }

            // Add the receive Data to the DataStorage List. 
            Monitor.Enter(ReadLock);
            try
            {
                if (!String.IsNullOrEmpty(FDataReceived))
                {
                    FDataStorage.Add(FDataReceived);
                }
            }
            finally
            {
                Monitor.Exit(ReadLock);
            }

            // Deltet the received data for the next reading cycling
            FDataReceived = String.Empty;

            //set the reading status to false
            FReadData = false;
        }

        // Test the Connection if the Server is still available. 
        private void TestConnection(ref int nTestBytesReceived, ref byte[] ReadBuffer)
        {
            bool blockingState = Client.Blocking;
            bool ReadData = true;

            try
            {
                //we do a read that is guaranteed not to block
                //Client.Blocking = false;
                nTestBytesReceived = Client.Receive(ReadBuffer);

            }
            catch (SocketException socketException)
            {
                //the non-blocking read failed
                ReadData = false;

                // 10035 == WSAEWOULDBLOCK
                //the read may have failed because it WOULD have blocked, ie the
                //connection is still there, but nothing has been sent
                if (!(socketException.NativeErrorCode.Equals(10035)))
                {
                    //however, if there was some other exception, we've lost the connection
                    FConnectStatus = TConnectStatus.ConnectionLost;
                }
            }
            finally
            {
                Client.Blocking = blockingState;
               
            }

            if (ReadData)
            {
                //if we received 0 bytes on a non-blocking read, we've lost the connection
                if (nTestBytesReceived == 0)
                {
                    FConnectStatus = TConnectStatus.ConnectionLost;
                }
            }
        }

        //method that vvvv can pulls the read data
        public string GetReadData()
        {

            Monitor.TryEnter(ReadLock);
            try
            {
                if (FDataStorage.Count > 0)
                {
                    string Data = "";

                    foreach (string Content in FDataStorage)
                    {
                        Data += Content;
                    }
                    FDataStorage.Clear();
                    return Data;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                Monitor.Exit(ReadLock);
            }
        
        }

        //adding a Error Message the the Exeptions List
        private void AddErrorMessage(string Message)
        {
            Monitor.Enter(ErrorLock);
            try
            {
                if (!String.IsNullOrEmpty(Message))
                {
                    FExceptions.Add(Message);
                }
            }
            finally
            {
                Monitor.Exit(ErrorLock);
            }

        }

        //method that vvv can pull the Error Messages
        public List<string> GetErrorMessages()
        {
            Monitor.TryEnter(ErrorLock);
            try
            {

                List<string> ErrorList = new List<string>(FExceptions);
                FExceptions.Clear();

                if (ErrorList.Count > 0)
                {
                    return ErrorList;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                Monitor.Exit(ErrorLock);
            }
        }
        #endregion


        #region Constructor


        public TV4TcpClient() : base()
		{
            InitThread();
		}
		
        //Constructor witch can copy an old Client
		public TV4TcpClient(TV4TcpClient toCopy) : base()
		{
            InitThread();
			FInput = toCopy.FInput;
			FEnabled = toCopy.FEnabled;
			FDoSend = toCopy.FDoSend;
			FRemoteHost = toCopy.FRemoteHost;
			FRemotePort = toCopy.FRemotePort;
			FHoldOutput = toCopy.FHoldOutput;
			ReceiveBufferSize = toCopy.ReceiveBufferSize;
			ReceiveTimeout = toCopy.ReceiveTimeout;
			SendBufferSize = toCopy.SendBufferSize;
			SendTimeout = toCopy.SendTimeout;

        }

        // ints the Reading and Writing threads
        private void InitThread()
        {
            if (FReadThread == null)
            {
                FReadThread = new Thread(new ThreadStart(ReadToSocket));
                FReadThread.IsBackground = true;
            }
        }

        #endregion Constructor 



    }
}
