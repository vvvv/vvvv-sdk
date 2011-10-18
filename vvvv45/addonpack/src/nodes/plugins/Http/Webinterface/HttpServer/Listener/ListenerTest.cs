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
        HttpListener FHttpListener = null;
        private static IAsyncResult FResult;
        private WebinterfaceSingelton FWebinterfaceSingelton = WebinterfaceSingelton.getInstance();
        private List<string> FFoldersToServ;
        private SortedList<string, string> mPostMessages = new SortedList<string, string>();
        private int FPortNumber;
        private List<string> FErrorMessages = new List<string>();

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

        public List<string> ErrorMessages
        {
            get
            {
                if (Monitor.TryEnter(FErrorMessages))
                {
                    try
                    {
                        List<string> TempErrorMessages = new List<string>(FErrorMessages);
                        FErrorMessages.Clear();
                        return TempErrorMessages;
                    }
                    finally
                    {
                        Monitor.Exit(FErrorMessages);
                    }
                }
                else
                {
                    return null;
                }
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
            if (FHttpListener == null)
            {
                FHttpListener = new HttpListener();
                if (HttpListener.IsSupported)
                {
                    try
                    {
                        FHttpListener.Prefixes.Add(String.Format("http://*:{0}/", FPortNumber));
                        FHttpListener.Prefixes.Add(String.Format("http://localhost:{0}/", FPortNumber));
                        FHttpListener.Start();

                        ThreadStart threadStart1 = new ThreadStart(StartListening);
                        FServerThread = new Thread(threadStart1);
                        FServerThread.Start();
                        FRunning = true;

                    }
                    catch (HttpListenerException ex)
                    {
                        AddErrorMessage(ex.Message);
                        FHttpListener = null;
                        FRunning = false;
                        //Debug.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                FHttpListener.Start();

                ThreadStart threadStart1 = new ThreadStart(StartListening);
                FServerThread = new Thread(threadStart1);
                FServerThread.Start();
                FRunning = true;
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
                    AddErrorMessage(ex.Message);
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

                RequestListener tRequest = new RequestListener(FFoldersToServ, Request, mPostMessages);

                HttpListenerResponse response = context.Response;
                //response.AddHeader("Cache-Control", "public");
                //response.ContentType = tRequest.Response.ContentType; 
                
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
                AddErrorMessage(Ex.Message);
                //Debug.WriteLine(Ex.Message);
            }
     
        }

        public void Stop()
        {
            try
            {

                if (FHttpListener != null)
                    if (FHttpListener.IsListening)
                        FHttpListener.Stop();
                        

                FRunning = false;
            }
            catch (Exception ex)
            {
                AddErrorMessage(ex.Message);
                //Debug.WriteLine(ex.Message);
            }

            
        }

        public void Close()
        {
            if (FHttpListener != null)
            {
                //FHttpListener.Stop();
                FHttpListener.Close();
                if (FHttpListener.IsListening)
                {
                    FHttpListener.Abort();
                }

                FHttpListener = null;
                FRunning = false;
            }
        }

        private void AddErrorMessage(string Message)
        {
            Monitor.Enter(FErrorMessages);
            try
            {
                FErrorMessages.Add(Message);
            }
            finally
            {
                Monitor.Exit(FErrorMessages);
            }
        }
    }
}
