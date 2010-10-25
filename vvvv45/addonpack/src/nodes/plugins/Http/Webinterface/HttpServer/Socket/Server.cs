using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
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
        private WebinterfaceSingelton FWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
     

        //Async Socket Handling
        private Socket FMainSocket;
        private TcpListener FTcpListen;
        IPEndPoint FIpLocal;
        private int FBacklog;
        private bool FShuttingDown = false;
        private Timer FLostTimer;
        private const int FTimerTimeout = 300000;
        private const int FNumThreads = 1;
        protected Hashtable FConnectedHT = new Hashtable();
        protected ArrayList ConnectedSockets;
        protected int FPortNumber;
        protected int FMaxSockets;
        protected int FSockCount = 0;
        private bool FInit = false;
        private List<int> FBlockedPorts = new List<int>();
        private bool FPortFree = false;
        private bool FRunning = false;

        //Thread signal.
        private ManualResetEvent FAllDone = new ManualResetEvent(false);
        private Thread FServerThread;
        private AutoResetEvent FThreadEnd;
        private Object FThisLock = new Object();


        //Files
        private List<string> FFoldersToServ;
        private SortedList<string, string> FPostMessages = new SortedList<string, string>();

        
        
      

        #region toSort
        private ArrayList FClientSocketList = new ArrayList();
        private SortedList<string, Socket> FIndexSocketList = new SortedList<string, Socket>();
        private SortedList<string, Socket> FDummySocketList = new SortedList<string, Socket>();
        private List<string> FRequestList = new List<string>();
        

        

        #endregion toSort


  

        #endregion field Declaration


        #region properties


        public List<string> FoldersToServ
        {
            set
            {
                FFoldersToServ = value;
            }
        }


        public bool ShuttingDown
        {
            set
            {
                FShuttingDown = value;
            }
        }

        public SortedList<string, string> PostMessages
        {
            set
            {
                FPostMessages = value;
            }
        }

        public bool Init
        {
            get
            {
                return FInit;
            }
        }

        public bool Running
        {
            get
            {
                return FRunning;
            }
        }

        public int Port
        {
            set 
            {
                FPortNumber = value;
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
            FPortNumber = pPort;
            FMaxSockets = 10000;
            this.FBacklog = Backlog;
            ConnectedSockets = new ArrayList(FMaxSockets);
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
                FRunning = true;
                FShuttingDown = false;
                FThreadEnd = new AutoResetEvent(false);

                ThreadStart threadStart1 = new ThreadStart(this.StartListening);
                FServerThread = new Thread(threadStart1);
                FServerThread.IsBackground = true;
                FServerThread.Start();

                
                int tMaxWorkerThreads;
                int tMaxAsynThreads;
                ThreadPool.GetMaxThreads(out tMaxWorkerThreads, out tMaxAsynThreads);
                ThreadPool.SetMaxThreads(tMaxWorkerThreads, tMaxWorkerThreads);
                // Create the delegate that invokes methods for the timer.
                TimerCallback timerDelegate = new TimerCallback(this.CheckSockets);
                //Create a timer that waits one minute, then invokes every 5 minutes.
                FLostTimer = new Timer(timerDelegate, null, Server.FTimerTimeout, Server.FTimerTimeout);
            }
        }

        private bool CheckPorts()
        {
            FPortFree = false;
            FBlockedPorts.Clear();

            IPGlobalProperties GlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] TcpEndPoints = GlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint TcpEndPoint in TcpEndPoints)
            {
                FBlockedPorts.Add(TcpEndPoint.Port);
            }

            if (FBlockedPorts.Contains(this.FPortNumber) == false)
            {
                FPortFree = true;
                return FPortFree;
            }

            
            return FPortFree;
        }



        public void StartListening()
        {
            try
            {
                FIpLocal = new IPEndPoint(IPAddress.Any, FPortNumber);
                FMainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                FMainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                FMainSocket.Bind(FIpLocal);
                // Start listening...<
                FMainSocket.Listen(10000);
                // Create the call back for any client connections...
                //FTcpListen = new TcpListener(IPAddress .Any,FPortNumber);
                //FTcpListen.Start();


                while (FShuttingDown == false)
                {
                    FAllDone.Reset();
                    FMainSocket.BeginAccept(new AsyncCallback(OnClientConnectCallback), FMainSocket);
                    FAllDone.WaitOne();
                }

                if (FMainSocket.Connected == true)
                {
                    Debug.WriteLine(String.Format("Socket {0} still alive", FPortNumber));
                }

            }
            catch (SocketException se)
            {
                string ErrorMessage = se.Message;
                FInit = false;
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message;
                FInit = false;
            }


            Debug.WriteLine(FServerThread.IsAlive.ToString());
            FMainSocket.Close();
            FMainSocket = null;
        }


        public void Stop()
        {
            FShuttingDown = true;
            FThreadEnd.Set();

            // Create a connection to the port to unblock the listener thread
            try
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, this.FPortNumber);
                sock.Connect(endPoint);
                sock = null;
            }
            catch (Exception ex)
            {
                string ErrorMessage = ex.Message;
            }

            //mLostTimer.Dispose();
            //mLostTimer = null;
            FRunning = false;

        }

        private void CheckSockets(object eventState)
        {
            FLostTimer.Change(System.Threading.Timeout.Infinite,
                System.Threading.Timeout.Infinite);
            
            if ( ConnectedSockets.Count != 0)
            {
                Monitor.Enter(ConnectedSockets);
                foreach (SocketInformation state in ConnectedSockets)
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
                Monitor.Exit(ConnectedSockets);
            }

            FLostTimer.Change(Server.FTimerTimeout, Server.FTimerTimeout);
            
        }


        virtual protected void RemoveSocket(SocketInformation state)
        {
            Socket sock = state.ClientSocket;
            Monitor.Enter(ConnectedSockets);
            if (ConnectedSockets.Contains(state))
            {
                ConnectedSockets.Remove(state);
                Interlocked.Decrement(ref FSockCount);
            }
            Monitor.Exit(ConnectedSockets);
            Monitor.Enter(FConnectedHT);

            if ((sock != null) && (FConnectedHT.ContainsKey(sock)))
            {
                object sockTemp = FConnectedHT[sock];
                if (FConnectedHT.ContainsKey(sockTemp))
                {
                    if (FConnectedHT.ContainsKey(FConnectedHT[sockTemp]))
                    {
                        FConnectedHT.Remove(sock);
                        if (sock.Equals(FConnectedHT[sockTemp]))
                        {
                            FConnectedHT.Remove(sockTemp);
                        }
                        else
                        {
                            object val, key = sockTemp;
                            while (true)
                            {
                                val = FConnectedHT[key];
                                if (sock.Equals(val))
                                {
                                    FConnectedHT[key] = sockTemp;
                                    break;
                                }
                                else if (FConnectedHT.ContainsKey(val))
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
            Monitor.Exit(FConnectedHT);

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
                FAllDone.Set();

                //The Socket witch connects to the Server
                Socket ClientSocket = FMainSocket.EndAccept(asynConnect);

                //Adding the Socket into the Socketlist of the Server 
                FClientSocketList.Add(ClientSocket);
                
                //Adding Time and Socket to the SocketInformation Object 
                SocketInformation tSocketInformations = new SocketInformation(ClientSocket, ClientSocket.RemoteEndPoint.ToString());


                Stopwatch ProcessingTime = new Stopwatch();
                ProcessingTime.Start();

                tSocketInformations.StopWatch = ProcessingTime;
                

                //Shows if the Socket is stille connected and begins to receive data in calling the ReceiveSocketDataCallback function
                try
                {
                    Interlocked.Increment(ref FSockCount);
                    Monitor.Enter(ConnectedSockets);
                    ConnectedSockets.Add(tSocketInformations);
                    Monitor.Exit(ConnectedSockets);

                    ClientSocket.BeginReceive(tSocketInformations.Buffer, 0, tSocketInformations.BufferSize, 0, new AsyncCallback(ReceiveSocketDataCallback), tSocketInformations);


                    if (FSockCount > FMaxSockets)
                    {
                        RemoveSocket(tSocketInformations);
                        ClientSocket.Shutdown(SocketShutdown.Both); 
                        ClientSocket.Close();
                        ClientSocket = null;
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
            Debug.WriteLine("-------------ReceiveSocketData----------------" + Environment.NewLine);
            //gets the SocketInformatinObject form the IAsyncResult Object
            SocketInformation SocketInformation = (SocketInformation)asynReceive.AsyncState;
            try
            {
                //Looks how many bytes are to read from the socket
                int bytesToRead = SocketInformation.ClientSocket.EndReceive(asynReceive);
                //Hold the Received Bytes as string
                string ReceivedString = Encoding.UTF8.GetString(SocketInformation.Buffer, 0, bytesToRead);
                SocketInformation.AppendRequest(ReceivedString);

                if (bytesToRead > 0 && bytesToRead == SocketInformation.BufferSize)
                {

                    SocketInformation.ClientSocket.BeginReceive( SocketInformation.Buffer, 0, SocketInformation.BufferSize, 0, new AsyncCallback(ReceiveSocketDataCallback), SocketInformation);
                    
                }
                else
                {
                    ProccessRequest(SocketInformation);   
                }


                #region Outdated

                ////Looks if all Data is received from the Socket
                //if (bytesToRead > 0)
                //{
                //    //Adding the received data to the SocketInformation Object 
                //    Monitor.Enter(tSocketInformation);
                //    tSocketInformation.AppendRequest(Encoding.UTF8.GetString(tSocketInformation.Buffer, 0, bytesToRead));
                //    Monitor.Exit(tSocketInformation);


                    
                //    Content = tSocketInformation.Request.ToString();

                //    //looks if alle data is received from the Socket
                //    // ?? sind damit alle Fälle abgedeckt (//Debuggen);
                //    if ((Content.Length > 0) && ((Content[0] != '<') ||
                //        ((Content[0] == '<') && (Content.IndexOf("</message>") > -1))))
                //    {

                //        tSocketInformation.TimeStamp = DateTime.Now;
                //        try
                //        {
                //            RequestSocket tRequest = new RequestSocket(FFoldersToServ, tSocketInformation, FPostMessages);
                //            tSocketInformation.RequestObject = tRequest;
                //            tSocketInformation.ResponseAsBytes = tRequest.Response.TextInBytes;

                //        }
                //        catch (Exception ex)
                //        {
                //            string ErrorMessage = ex.Message;
                //            Response ErrorResponse = new Response("unkown.html", Encoding.UTF8.GetBytes("Error by creating an Response." + Environment.NewLine + ErrorMessage), new HTTPStatusCode("").Code500);
                //            tSocketInformation.ResponseAsBytes = ErrorResponse.TextInBytes;
                //        }
                        
                //        //SendData(tSocketInformation);
                //        tSocketInformation.ClientSocket.BeginSend(tSocketInformation.ResponseAsBytes, 0, tSocketInformation.ResponseAsBytes.Length, 0, new AsyncCallback(SendDataCallback), tSocketInformation);
                //    }
                //    else
                //    {
                //        //if not all Data is received read next block
                //       tSocketInformation.ClientSocket.BeginReceive(tSocketInformation.Buffer, 0, tSocketInformation.BufferSize, 0, 
                //       new AsyncCallback(ReceiveSocketDataCallback), tSocketInformation);
                //    }
                //}

                #endregion Outdated


            }
            catch (System.Net.Sockets.SocketException es)
            {
                RemoveSocket(SocketInformation);
                if (es.ErrorCode != 64)
                {
                    ////Debug.WriteLine(string.Format("ReadCallback Socket Exception: {0}, {1}.", es.ErrorCode, es.ToString()));
                }

            }
            catch (Exception e)
            {
                RemoveSocket(SocketInformation);
                if (e.GetType().FullName != "System.ObjectDisposedException")
                {
                    Console.WriteLine(string.Format("ReadCallback Exception: {0}.", e.ToString()));
                }
            }

        }

        private void ProccessRequest(SocketInformation SocketInformation)
        {
                                //check if received string is valid HTTP head
                    Regex HttpHeadRegex = new Regex("\r\n\r\n");
                    MatchCollection Matches = HttpHeadRegex.Matches(SocketInformation.Request);
                    if (Matches.Count > 0)
                    {
                        string[] HttpParts = HttpHeadRegex.Split(SocketInformation.Request);
                        string HttpHead = HttpParts[0];
                        string HttpBody = HttpParts[1];

                        Debug.WriteLine(HttpHead);
                        Debug.WriteLine(HttpBody);




                        RequestSocket tRequest = new RequestSocket(FFoldersToServ, SocketInformation, FPostMessages);
                        SocketInformation.RequestObject = tRequest;
                        SocketInformation.ResponseAsBytes = tRequest.Response.TextInBytes;
                        SocketInformation.ClientSocket.BeginSend(SocketInformation.ResponseAsBytes, 0, SocketInformation.ResponseAsBytes.Length, 0, new AsyncCallback(SendDataCallback), SocketInformation);
                    }
                    else
                    {
                        Debug.WriteLine("Error");
                    }
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


        private void AddErrorMessage(string Message)
        {
        }
       
    }
}
