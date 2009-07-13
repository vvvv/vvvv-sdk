using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.Runtime.Remoting.Messaging;


using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.Data;
using VVVV.Webinterface.HttpServer;



namespace VVVV.Webinterface.HttpServer {


    /// <summary>
    /// Server Class definiton 
    /// Initiates in Server with the Methods GET and Post
    /// Inherite from the Observer
    /// Server listens for all incoming clients on all IP's
    /// </summary>
    /// 
    class Server : Observer
    {





        #region filed declaration 


        //Classes
        private bool FDisposed = false;
        private ConcreteSubject mSubject;
        private WebinterfaceSingelton mWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
     
        // Socket
        private Socket mMainSocket;
        IPEndPoint mIpLocal;
        private int mBacklog;
        private bool mShuttingDown = false;
        //private int convID;
        private Timer mLostTimer;
        private const int mTimerTimeout = 300000;
        //private const int mTimerTimeout = 3000;
        private const int mTimeoutMinutes = 3;
        private const int numThreads = 1;
        protected Hashtable connectedHT = new Hashtable();
        protected ArrayList connectedSocks;
        protected int portNumber;
        protected int maxSockets;
        protected int sockCount = 0;

        //Thread signal.
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private Thread[] serverThread = new Thread[numThreads];
        private AutoResetEvent[] threadEnd = new AutoResetEvent[numThreads];
        private Object thisLock = new Object();


        //Files
        private SortedList<string, byte[]> mHtmlPages = new SortedList<string, byte[]>();
        private SortedList<string,string> mCssFiles;
        private SortedList<string, string> mJsFiles;
        private List<string> mFoldersToServ;

        
        
       

        #region toSort


        //Server(DNS)
        private string mName;
        private ItemsToServ mItemsToServ;

        private List<string> mFileList;
        private List<string> mFileNames;
        private ArrayList mClientSocketList = new ArrayList();
        private SortedList<string, Socket> mIndexSocketList = new SortedList<string, Socket>();
        private SortedList<string, Socket> mDummySocketList = new SortedList<string, Socket>();
        private List<string> mRequestList = new List<string>();
        

        

        #endregion toSort


  

        #endregion field Declaration





        #region properties

        /// <summary>
        /// Name of the Server
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
        }

        public SortedList<string,string> VVVVCssFile
        {
            set
            {
                mCssFiles = value;
            }
        }

        public SortedList<string,string> VVVVJsFile
        {
            set
            {
                mJsFiles = value;
            }
        }

        public List<string> FoldersToServ
        {
            set
            {
                mFoldersToServ = value;
            }
        }

        public SortedList<string, byte[]> HtmlPages
        {
            set
            {
                mHtmlPages = value;
            }
        }

        public bool ShuttingDown
        {
            set
            {
                mShuttingDown = value;
            }
        }



        /// <summary>
        /// sets the port of the Server
        /// </summary>
        public int Port 
        { 
            set 
            {
                mIpLocal.Port = value;
                Debug.WriteLine("set Server to Port: " + value.ToString());
            } 
        }

        public List<string> FileList
        {
            get
            {
                return mFileList;
            }
        }
        /// <summary>
        /// sets the folder to serve
        /// </summary>

        public List<string> FileNames
        {
            get
            {
                return mFileNames;
            }
        }

        #endregion properties





        #region constructor /deconstructor

        /// <summary>
        /// Server constructor
        /// </summary>
        /// <param name="Port">Server Port</param>
        /// <param name="Backlog">Backlog </param>
        /// <param name="pSubject">the subject class which cooperates with the server</param>
        /// <param name="pName">name of the server</param>
        public Server(int pPort, int Backlog, ConcreteSubject pSubject, string pName, string pFolderToServ)
        {

            this.portNumber = pPort;
            this.maxSockets = 10000;
            this.mBacklog = Backlog;
            this.mName = pName;
            this.mSubject = pSubject;

            connectedSocks = new ArrayList(this.maxSockets);

            //Bin ich richtig hier??
            mItemsToServ = new ItemsToServ(pFolderToServ);
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

                Debug.WriteLine("Server is being deleted");
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
            for (int lcv = 0; lcv < numThreads; lcv++)
                threadEnd[lcv] = new AutoResetEvent(false);

            ThreadStart threadStart1 = new ThreadStart(this.StartListening);
            serverThread[0] = new Thread(threadStart1);
            serverThread[0].IsBackground = true;
            serverThread[0].Start();

            
            int tMaxWorkerThreads;
            int tMaxAsynThreads;
            ThreadPool.GetMaxThreads(out tMaxWorkerThreads, out tMaxAsynThreads);
            ThreadPool.SetMaxThreads(tMaxWorkerThreads, tMaxWorkerThreads);
            Debug.WriteLine(String.Format("{0}  Threads  /  {1} Asynthreads in threadspool", tMaxWorkerThreads, tMaxAsynThreads));
            // Create the delegate that invokes methods for the timer.
            TimerCallback timerDelegate = new TimerCallback(this.CheckSockets);
            //Create a timer that waits one minute, then invokes every 5 minutes.
            mLostTimer = new Timer(timerDelegate, null, Server.mTimerTimeout, Server.mTimerTimeout);
        }



        public void StartListening()
        {
            try
            {
                mIpLocal = new IPEndPoint(IPAddress.Any, portNumber);
                mMainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
                


                Debug.WriteLine("-------------- Stop Listing to Socket --------------");
            }
            catch (SocketException se)
            {
                Debug.WriteLine(se.Message.ToString());
            }
            catch (Exception ex)
            {
                threadEnd[0].Set();
                Debug.WriteLine("Server Constructor: \n" + ex.ToString());
            }
        }



        public void Stop()
        {
            int lcv;
            mLostTimer.Dispose();
            mLostTimer = null;

            for (lcv = 0; lcv < numThreads; lcv++)
            {
                if (!serverThread[lcv].IsAlive)
                    threadEnd[lcv].Set();    // Set event if thread is already dead
            }
            ShuttingDown = true;

            if (connectedSocks.Count != 0)
            {

            }
            

            // Create a connection to the port to unblock the listener thread
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, this.portNumber);
            sock.Connect(endPoint);
            //sock.Close();
            sock = null;

            // Check thread end events and wait for up to 5 seconds.
            for (lcv = 0; lcv < numThreads; lcv++)
                threadEnd[lcv].WaitOne(5000, false);

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
                    if (state.ClientSocket == null)
                    {    // Remove invalid state object
                        if (connectedSocks.Contains(state))
                        {
                            connectedSocks.Remove(state);
                            Interlocked.Decrement(ref sockCount);
                        }
                    }
                    else
                    {
                        if (DateTime.Now.AddTicks(-state.TimeStamp.Ticks).Minute > mTimeoutMinutes && state.RequestObject.FileName != "dummy.html")
                        {
                            RemoveSocket(state);
                            Interlocked.Decrement(ref sockCount);
                        }
                        else if(state.RequestObject.FileName == "dummy.html" && state.ClientSocket.Connected == false)
                        {
                            RemoveSocket(state);
                            Interlocked.Decrement(ref sockCount);
                        }
                    }
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
                        Console.WriteLine(string.Format("Socket is not in the {0}  connected hash table!", this.Name));
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
                //Debug.WriteLine("-------------onClientConnect----------------" + Environment.NewLine);

                allDone.Set();

                //The Socket witch connects to the Server
                Socket tClientSocket = mMainSocket.EndAccept(asynConnect);

                //Adding the Socket into the Socketlist of the Server 
                mClientSocketList.Add(tClientSocket);
                
                //Adding Time and Socket to the SocketInformation Object 
                SocketInformation tSocketInformations = new SocketInformation(tClientSocket, "Id");
                tSocketInformations.HtmlPages = mHtmlPages;
                

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

                catch (SocketException es)
                {
                    RemoveSocket(tSocketInformations);
                    Debug.WriteLine("Socket Error " + es.Message.ToString());
                }
                catch (Exception e)
                {
                    RemoveSocket(tSocketInformations);
                    Debug.WriteLine("Any Error " + e.Message.ToString());
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
                //Debug.WriteLine("-------------ReceiveSocketData----------------" + Environment.NewLine);
                
                //gets the SocketInformatinObject form the IAsyncResult Object
                

                //Looks how many bytes are to read from the socket
                int bytesRead = tSocketInformation.ClientSocket.EndReceive(asynReceive);

                //Looks if all Data is received from the Socket
                if (bytesRead > 0)
                {
                    //Adding the received data to the SocketInformation Object 
                    Monitor.Enter(tSocketInformation);
                    tSocketInformation.Request.Append(Encoding.UTF8.GetString(tSocketInformation.Buffer, 0, bytesRead));
                    Monitor.Exit(tSocketInformation);


                    
                    Content = tSocketInformation.Request.ToString();

                    //looks if alle data is received from the Socket
                    // ?? sind damit alle Fälle abgedeckt (debuggen);
                    if ((Content.Length > 0) && ((Content[0] != '<') ||
                        ((Content[0] == '<') && (Content.IndexOf("</message>") > -1))))
                    {

                        tSocketInformation.TimeStamp = DateTime.Now;
                        
                        try
                        {
                            Request tRequest = new Request(tSocketInformation.Request.ToString(), mFoldersToServ, tSocketInformation.HtmlPages);
                            tSocketInformation.RequestObject = tRequest;
                            tSocketInformation.ResponseAsBytes = tRequest.Response.TextInBytes;

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(String.Format("Error in RequestHandling: {0}", ex.Message));
                            tSocketInformation.ResponseAsBytes = Encoding.UTF8.GetBytes(new ResponseHeader(new HTTPStatusCode("").Code200).Text);
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
                    Debug.WriteLine(string.Format("ReadCallback Socket Exception: {0}, {1}.", es.ErrorCode, es.ToString()));
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
                        RemoveSocket(tSocketIformations);
                    }
                }

                Debug.WriteLine(String.Format("Sent {0} bytes to client.",  bytesSent));
           }
            catch ( Exception e )
            {
                Debug.WriteLine( e.ToString() );
            }
        }


        #endregion request Handle





        #region ServeData

        public void ServeFolder(string pPath)
        {
            
            Debug.WriteLine("serve Folder: " + pPath);
            mItemsToServ.ReadServerFolder(pPath);

            mFileList = mItemsToServ.FileListVVVV;
            mFileNames = mItemsToServ.FileListNameVVVV;
        }

        #endregion ServeData






        #region Updated Handling
        /// <summary>
        /// sends the new data to the browser
        /// </summary>
        public override void Updated()
        {
            Debug.WriteLine("Updated Server");
           
            SortedList<string, string> tNewServerhandlingDaten = mSubject.NewServerDaten;

            foreach (KeyValuePair<string, string> tSlice in tNewServerhandlingDaten)
            {

                if (tSlice.Key.Contains("Checkbox"))
                {
                    Debug.WriteLine("Checkbox Update");
                    JavaScript tJava = new JavaScript();
                    tJava.Insert("parent.window.setCheckbox('" + tSlice.Key + "','" + tSlice.Value + "');");
                    JavaScript tJava2 = new JavaScript();
                    tJava2.Insert("parent.window.changeValue('" + tSlice.Key + "');");

                    foreach (KeyValuePair<string, Socket> pKey in mDummySocketList)
                    {

                        Socket pSocket = pKey.Value;
                        ResponseHeader tHeader = new ResponseHeader(new HTTPStatusCode("").Code200, "dummy.html");
                        //ResponseUpdate tResponse = new ResponseUpdate(tHeader.HeaderText, pSocket, "dummy.html", tJava.Text + Environment.NewLine + tJava2.Text);
                        
                   }

                }else if(tSlice.Key.Contains("Button"))
                {

                }
                else
                {
                    Debug.WriteLine(" send: " + tSlice.Value + " from VVVV to Browser Element: " + tSlice.Key);
                  
                    JavaScript tJava = new JavaScript();
                    tJava.Insert("parent.window.setNewDaten('" + tSlice.Key + "','" + tSlice.Value + "');");

                    foreach (KeyValuePair<string, Socket> pKey in mDummySocketList)
                    {

                        Socket pSocket = pKey.Value;
                        if (pSocket.Connected)
                        {
                            pSocket.Send(Encoding.UTF8.GetBytes(tJava.Text + Environment.NewLine));
                        }
                        
                        //ResponseHeader tHeader = new ResponseHeader();
                        //ResponseUpdate tResponse = new ResponseUpdate(tHeader.HeaderText, pSocket, "dummy.html", tJava.Text);

                    }


                }
            }
        }

        public override void UpdatedBrowser(string pData)
        {
            foreach (KeyValuePair<string, Socket> pKey in mDummySocketList)
            {
                Socket pSocket = pKey.Value;
                ResponseHeader tHeader = new ResponseHeader(new HTTPStatusCode("").Code200, "dummy.html");

                //ResponseTextDummy tResponse = new ResponseTextDummy("", pSocket, "dummy.html", pData);

                //Thread tGetHandlingThread = new Thread(new ThreadStart(tResponse.Run));
                //tGetHandlingThread.Start();
            }
        }

        /// <summary>
        /// send data to browser to reload the whole HTML page
        /// </summary>
        public override void Reload()
        {

                //foreach (KeyValuePair<string, Socket> pKey in mDummySocketList)
                //{
                //    Socket pSocket = pKey.Value;
                //    pSocket.Close();
                //}

                JavaScript tJava = new JavaScript();
                tJava.Insert("parent.window.Reload();");


                foreach (KeyValuePair<string, Socket> pKey in mDummySocketList)
                {

                    Socket pSocket = pKey.Value;
                    ResponseHeader tHeader = new ResponseHeader(new HTTPStatusCode("").Code200, "dummy.html");

                    //ResponseTextDummy tResponse = new ResponseTextDummy("", pSocket, "dummy.html", tJava.Text);

                    //Thread tGetHandlingThread = new Thread(new ThreadStart(tResponse.Run));
                    //tGetHandlingThread.Start();
                    //pSocket.Send(Encoding.UTF8.GetBytes(pText + Environment.NewLine));
                }
                //sendText(tJava.Text);
                Debug.WriteLine("Reload Server");

            }

         #endregion Updated Handling
            


        
    }
}
