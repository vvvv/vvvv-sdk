using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{

    /// <summary>
    /// Implements the HttpListener Class which is used by the Render(HTTP) Node
    /// </summary>
    class ListenerTest
    {

        //Thread signal.
        private ManualResetEvent FAllDone = new ManualResetEvent(false);
        private Thread FServerThread;
        private AutoResetEvent FThreadEnd = new AutoResetEvent(false);
        private ManualResetEvent FManualReset = new ManualResetEvent(false);
        volatile bool FRunning = false;
        private static System.Threading.AutoResetEvent FListenForNextRequest = new System.Threading.AutoResetEvent(false);

        //Listener
        HttpListener FHttpListener = new HttpListener();
        private static IAsyncResult FResult;
        private WebinterfaceSingelton FWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        private List<string> FFoldersToServ;
        private SortedList<string, string> mPostMessages = new SortedList<string, string>();
        private int FPortNumber;


        #region Properties

        public List<string> FoldersToServ
        {
            set
            {
                FFoldersToServ = value;
            }
        }

        public SortedList<string, string> PostMessages
        {
            set
            {
                mPostMessages = value;
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


        #endregion


        #region Construtor


        public ListenerTest(int PortNumber)
        {
            FPortNumber = PortNumber;
        }

        #endregion



        public void Start()
        {
            
            if (FHttpListener.IsListening)
            {
                Debug.WriteLine("WHY? ...");
            }
            if(HttpListener.IsSupported)
            {
                try
                {
                    FRunning = true;
                    FHttpListener.Prefixes.Add(String.Format("http://*:{0}/", FPortNumber));
                    FHttpListener.Prefixes.Add(String.Format("http://localhost:{0}/", FPortNumber));
                    FHttpListener.Start();


                    ThreadStart threadStart1 = new ThreadStart(StartListening);
                    FServerThread = new Thread(threadStart1);
                    FServerThread.Start();
                }
                catch (HttpListenerException ex)
                {
                    //Debug.WriteLine(ex.Message);
                }
                
            }else
            {

            }

        }

        private void StartListening()
        {
            while (FRunning)
            {
                try
                {
                    FAllDone.Reset();
                    FHttpListener.BeginGetContext(new AsyncCallback(ListenerCallback), FHttpListener);
                    FAllDone.WaitOne();
                }
                catch(Exception ex)
                {
                    //Debug.WriteLine(ex.Message);
                }
            }

        }


        private void ListenerCallback(IAsyncResult result)
        {

            try
            {
                FAllDone.Set();
                HttpListener listener = (HttpListener)result.AsyncState;
                // Call EndGetContext to complete the asynchronous operation.
                HttpListenerContext context = listener.EndGetContext(result);
                HttpListenerRequest Request = context.Request;

                Request tRequest = new Request(FFoldersToServ, Request, mPostMessages);

                HttpListenerResponse response = context.Response;
                response.AddHeader("Cache-Control", "no-store");
                response.ContentType = tRequest.Response.ContentType;
                
                // Construct a response.
                byte[] buffer = tRequest.Response.TextInBytes;
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
            catch (Exception Ex)
            {
                //Debug.WriteLine(Ex.Message);
            }
     
        }

        public void Stop()
        {
            try
            {
                FRunning = false;
                FHttpListener.Prefixes.Clear();
                FHttpListener.Stop();
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
            }

            
        }

        public void Close()
        {
            FHttpListener.Abort();
        }
    }
}
