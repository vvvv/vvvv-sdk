using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.Runtime.Remoting.Messaging;


using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.HttpServer;



namespace VVVV.Webinterface.HttpServer {


    /// <summary>
    /// Server Class definiton 
    /// Initiates in Server with the Methods GET and Post
    /// Inherite from the Observer
    /// Server listens for all incoming clients on all IP's
    /// </summary>
    /// 
    class Server
    {



        #region filed declaration 


        //Classes
        private bool FDisposed = false;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
     
        // Socket
        private Socket mMainSocket;
        IPEndPoint mIpLocal;
        private int mBacklog;
        private bool mShuttingDown = false;
        private Timer mLostTimer;
        private const int mTimerTimeout = 300000;
        private const int numThreads = 1;
        protected Hashtable connectedHT = new Hashtable();
        protected ArrayList connectedSocks;
        protected int mPortNumber;
        protected int maxSockets;
        protected int sockCount = 0;
        private bool mInit = false;
        private List<int> mBlockedPorts = new List<int>();
        private bool mPortFree = false;
        private bool mRunning = false;

        //Thread signal.
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private Thread serverThread;
        private AutoResetEvent threadEnd;
        private Object thisLock = new Object();


        //Files
        private List<string> mFoldersToServ;
        private SortedList<string, string> mPostMessages = new SortedList<string, string>();

        
        
      

        #region toSort
        private ArrayList mClientSocketList = new ArrayList();
        private SortedList<string, Socket> mIndexSocketList = new SortedList<string, Socket>();
        private SortedList<string, Socket> mDummySocketList = new SortedList<string, Socket>();
        private List<string> mRequestList = new List<string>();
        

        

        #endregion toSort


  

        #endregion field Declaration





        #region properties


        public List<string> FoldersToServ
        {
            set
            {
                mFoldersToServ = value;
            }
        }


        public bool ShuttingDown
        {
            set
            {
                mShuttingDown = value;
            }
        }

        public SortedList<string, string> PostMessages
        {
            set
            {
                mPostMessages = value;
            }
        }

        public bool Init
        {
            get
            {
                return mInit;
            }
        }

        public bool Running
        {
            get
            {
                return mRunning;
            }
        }

        public int Port
        {
            set 
            {
                mPortNumber = value;
            }
        }

        #endregion properties





        #region constructor /deconstructor

        /// <summary>
        /// Server constructor
        /// </summary>
        /// <param name="Port">Server Port</param>
        /// <param name="Backlog">Backlog </param>
        public Server(int pPort, int Backlog)
        {
            this.mPortNumber = pPort;
            this.maxSockets = 10000;
            this.mBacklog = Backlog;
            connectedSocks = new ArrayList(this.maxSockets);
        }


        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time
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
                //mSocket.Close();

                ////Debug.WriteLine("Server is being deleted");
                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        #endregion Dispose


        #endregion constructor /deconstructor


        #region Socket Connection



        public void Start()
        {
            // Clear the thread end events
            if (CheckPorts())
            {
                mRunning = true;
                mShuttingDown = false;
                threadEnd = new AutoResetEvent(false);

                ThreadStart threadStart1 = new ThreadStart(this.StartListening);
                serverThread = new Thread(threadStart1);
                serverThread.IsBackground = true;
                serverThread.Start();

                
                int tMaxWorkerThreads;
                int tMaxAsynThreads;
                ThreadPool.GetMaxThreads(out tMaxWorkerThreads, out tMaxAsynThreads);
                ThreadPool.SetMaxThreads(tMaxWorkerThreads, tMaxWorkerThreads);
                // Create the delegate that invokes methods for the timer.
                TimerCallback timerDelegate = new TimerCallback(this.CheckSockets);
                //Create a timer that waits one minute, then invokes every 5 minutes.
                mLostTimer = new Timer(timerDelegate, null, Server.mTimerTimeout, Server.mTimerTimeout);
            }
        }

        private bool CheckPorts()
        {
            mPortFree = false;
            mBlockedPorts.Clear();

            IPGlobalProperties GlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            //TcpConnectionInformation[] TCPConnections = GlobalProperties.GetActiveTcpConnections();
            IPEndPoint[] TcpEndPoints = GlobalProperties.GetActiveTcpListeners();
            //IPEndPoint[] UdpEndpoints = GlobalProperties.GetActiveUdpListeners();

            //foreach (TcpConnectionInformation TcpInformation in TCPConnections)
            //{
            //    mBlockedPorts.Add(TcpInformation.LocalEndPoint.Port);
            //}

            foreach (IPEndPoint TcpEndPoint in TcpEndPoints)
            {
                mBlockedPorts.Add(TcpEndPoint.Port);
            }

            //foreach (IPEndPoint UdpEndPoint in UdpEndpoints)
            //{
            //    mBlockedPorts.Add(UdpEndPoint.Port);
            //}

            if (mBlockedPorts.Contains(this.mPortNumber) == false)
            {
                mPortFree = true;
                return mPortFree;
            }

            
            return mPortFree;
        }



        public void StartListening()
        {
            try
            {
                mIpLocal = new IPEndPoint(IPAddress.Any, mPortNumber);
                mMainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mMainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //mMainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                mMainSocket.Bind(mIpLocal);
                // Start listening...<
                mMainSocket.Listen(100);
                // Create the call back for any client connections...

                while (mShuttingDown == false)
                {
                    allDone.Reset();
                    mMainSocket.BeginAccept(new AsyncCallback(OnClientConnectCallback), mMainSocket);
                    allDone.WaitOne();
                }

                if (mMainSocket.Connected == true)
                {
                    Debug.WriteLine(String.Format("Socket {0} still alive", mPortNumber));
                }

            }
            catch (SocketException se)
            {
                string ErrorMessage = se.Message;
                mInit = false;
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message;
                mInit = false;
            }


            Debug.WriteLine(serverThread.IsAlive.ToString());
            mMainSocket.Close();
            mMainSocket = null;
        }



        public void Stop()
        {
            mShuttingDown = true;
            threadEnd.Set();

            // Create a connection to the port to unblock the listener thread
            try
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, this.mPortNumber);
                sock.Connect(endPoint);
                sock = null;
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message;
            }

            //mLostTimer.Dispose();
            //mLostTimer = null;
            mRunning = false;

        }



        private void CheckSockets(object eventState)
        {
            mLostTimer.Change(System.Threading.Timeout.Infinite,
                System.Threading.Timeout.Infinite);
            
            if ( connectedSocks.Count != 0)
            {
                Monitor.Enter(connectedSocks);
                foreach (SocketInformation state in connectedSocks)
                {
                    //if (state.ClientSocket == null)
                    //{    // Remove invalid state object
                    //    if (connectedSocks.Contains(state))
                    //    {
                    //        connectedSocks.Remove(state);
                    //        Interlocked.Decrement(ref sockCount);
                    //    }
                    //}
                    //else
                    //{
                    //    if (DateTime.Now.AddTicks(-state.TimeStamp.Ticks).Minute > mTimeoutMinutes)
                    //    {
                    //        RemoveSocket(state);
                    //        Interlocked.Decrement(ref sockCount);
                        //}
                        //else if(state.RequestObject.FileName == "dummy.html" && state.ClientSocket.Connected == false)
                        //{
                        //    RemoveSocket(state);
                        //    Interlocked.Decrement(ref sockCount);
                        //}
                    //}
                }
                Monitor.Exit(connectedSocks);
            }

            mLostTimer.Change(Server.mTimerTimeout, Server.mTimerTimeout);
            
        }


        virtual protected void RemoveSocket(SocketInformation state)
        {
            Socket sock = state.ClientSocket;
            Monitor.Enter(connectedSocks);
            if (connectedSocks.Contains(state))
            {
                connectedSocks.Remove(state);
                Interlocked.Decrement(ref sockCount);
            }
            Monitor.Exit(connectedSocks);
            Monitor.Enter(connectedHT);

            if ((sock != null) && (connectedHT.ContainsKey(sock)))
            {
                object sockTemp = connectedHT[sock];
                if (connectedHT.ContainsKey(sockTemp))
                {
                    if (connectedHT.ContainsKey(connectedHT[sockTemp]))
                    {
                        connectedHT.Remove(sock);
                        if (sock.Equals(connectedHT[sockTemp]))
                        {
                            connectedHT.Remove(sockTemp);
                        }
                        else
                        {
                            object val, key = sockTemp;
                            while (true)
                            {
                                val = connectedHT[key];
                                if (sock.Equals(val))
                                {
                                    connectedHT[key] = sockTemp;
                                    break;
                                }
                                else if (connectedHT.ContainsKey(val))
                                    key = val;
                                else    // The chain is broken
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("Socket is not in the connected hash table!"));
                    }
                }
            }
            Monitor.Exit(connectedHT);

            if (sock != null)
            {
                if (sock.Connected)
                {
                    sock.Shutdown(SocketShutdown.Both);
                }
                sock.Disconnect(true);
                sock.Close();
                sock = null;
                state.ClientSocket = null;
                state = null;
            }
        }


        #endregion Socket Connection





        #region request handle

        /// <summary>
        /// Callback function with is calles if an Client connects to the Server
        /// </summary>
        /// <param name="asynConnect"> Contains the SocketInformationObjekt</param>
        public void OnClientConnectCallback(IAsyncResult asynConnect)
        {
                allDone.Set();

                //The Socket witch connects to the Server
                Socket tClientSocket = mMainSocket.EndAccept(asynConnect);

                //Adding the Socket into the Socketlist of the Server 
                mClientSocketList.Add(tClientSocket);
                
                //Adding Time and Socket to the SocketInformation Object 
                SocketInformation tSocketInformations = new SocketInformation(tClientSocket, tClientSocket.RemoteEndPoint.ToString());


                Stopwatch myStop = new Stopwatch();
                myStop.Start();

                tSocketInformations.StopWatch = myStop;
                

                //Shows if the Socket is stille connected and begins to receive data in calling the ReceiveSocketDataCallback function
                try
                {
                    Interlocked.Increment(ref sockCount);
                    Monitor.Enter(connectedSocks);
                    connectedSocks.Add(tSocketInformations);
                    Monitor.Exit(connectedSocks);

                    tClientSocket.BeginReceive(tSocketInformations.Buffer, 0, tSocketInformations.BufferSize, 0, new AsyncCallback(ReceiveSocketDataCallback), tSocketInformations);


                    if (sockCount > this.maxSockets)
                    {
                        RemoveSocket(tSocketInformations);
                        tClientSocket.Shutdown(SocketShutdown.Both);
                        tClientSocket.Close();
                        tClientSocket = null;
                        tSocketInformations = null;
                    }

                }

                catch (SocketException se)
                {
                    string ErrorMessage = se.Message;
                    RemoveSocket(tSocketInformations);
                    ////Debug.WriteLine("Socket Error " + es.Message.ToString());
                }
                catch (Exception ex)
                {
                    string ErrorMessage = ex.Message;
                    RemoveSocket(tSocketInformations);
                    ////Debug.WriteLine("Any Error " + e.Message.ToString());
                }

        }


        /// <summary>
        /// callback function with receives Data from the Socket 
        /// </summary>
        /// <param name="asynReceive">Contains the SocketInformationObjekt</param>
        public void ReceiveSocketDataCallback(IAsyncResult asynReceive)
        {
            String Content = String.Empty;
            SocketInformation tSocketInformation = (SocketInformation)asynReceive.AsyncState;

            try
            {
                //////Debug.WriteLine("-------------ReceiveSocketData----------------" + Environment.NewLine);
                
                //gets the SocketInformatinObject form the IAsyncResult Object
                

                //Looks how many bytes are to read from the socket
                int bytesRead = tSocketInformation.ClientSocket.EndReceive(asynReceive);

                //Looks if all Data is received from the Socket
                if (bytesRead > 0)
                {
                    //Adding the received data to the SocketInformation Object 
                    Monitor.Enter(tSocketInformation);
                    tSocketInformation.AppendRequest(Encoding.UTF8.GetString(tSocketInformation.Buffer, 0, bytesRead));
                    Monitor.Exit(tSocketInformation);


                    
                    Content = tSocketInformation.Request.ToString();

                    //looks if alle data is received from the Socket
                    // ?? sind damit alle Fälle abgedeckt (//Debuggen);
                    if ((Content.Length > 0) && ((Content[0] != '<') ||
                        ((Content[0] == '<') && (Content.IndexOf("</message>") > -1))))
                    {

                        tSocketInformation.TimeStamp = DateTime.Now;
                        try
                        {
                            //Request tRequest = new Request(tSocketInformation.Request.ToString(), mFoldersToServ, tSocketInformation, mPostMessages);
                            //tSocketInformation.RequestObject = tRequest;
                            //tSocketInformation.ResponseAsBytes = tRequest.Response.TextInBytes;

                        }
                        catch (Exception ex)
                        {
                            string ErrorMessage = ex.Message;
                            Response ErrorResponse = new Response("unkown.html", Encoding.UTF8.GetBytes("Error by creating an Response." + Environment.NewLine + ErrorMessage), new HTTPStatusCode("").Code500);
                            tSocketInformation.ResponseAsBytes = ErrorResponse.TextInBytes;
                        }
                        
                        //SendData(tSocketInformation);
                        tSocketInformation.ClientSocket.BeginSend(tSocketInformation.ResponseAsBytes, 0, tSocketInformation.ResponseAsBytes.Length, 0, new AsyncCallback(SendDataCallback), tSocketInformation);
                    }
                    else
                    {
                        //if not all Data is received read next block
                       tSocketInformation.ClientSocket.BeginReceive(tSocketInformation.Buffer, 0, tSocketInformation.BufferSize, 0, 
                       new AsyncCallback(ReceiveSocketDataCallback), tSocketInformation);
                    }
                }
             }
            catch (System.Net.Sockets.SocketException es)
            {
                RemoveSocket(tSocketInformation);
                if (es.ErrorCode != 64)
                {
                    ////Debug.WriteLine(string.Format("ReadCallback Socket Exception: {0}, {1}.", es.ErrorCode, es.ToString()));
                }

            }
            catch (Exception e)
            {
                RemoveSocket(tSocketInformation);
                if (e.GetType().FullName != "System.ObjectDisposedException")
                {
                    Console.WriteLine(string.Format("ReadCallback Exception: {0}.", e.ToString()));
                }
            }

        }

        private void SendData(SocketInformation pSocketInformations)
        {
            
        }


        private void SendDataCallback(IAsyncResult asynSendData)
        {

            SocketInformation tSocketIformations = (SocketInformation)asynSendData.AsyncState;

            try
            {
                int bytesSent = tSocketIformations.ClientSocket.EndSend(asynSendData);
                if (tSocketIformations.RequestObject != null)
                {
                    if (tSocketIformations.RequestObject.FileName == "dummy.html")
                    {

                    }
                    else
                    {
                        Stopwatch myStop = new Stopwatch();
                        myStop = tSocketIformations.StopWatch;
                        myStop.Stop();
                        Debug.WriteLine("Request time in ticks: " + myStop.ElapsedTicks.ToString());
                        Debug.WriteLine("Request time in milliseconds: " + myStop.ElapsedMilliseconds.ToString());
                        RemoveSocket(tSocketIformations);
                    }
                }

                ////Debug.WriteLine(String.Format("Sent {0} bytes to client.",  bytesSent));
           }
            catch ( Exception ex )
            {

                string ErrorMessage = ex.Message;
            }
        }


        #endregion request Handle
       
    }
}
