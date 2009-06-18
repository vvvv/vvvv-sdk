using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;

using VVVV.Webinterface;
using VVVV.Webinterface.Data;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;




namespace VVVV.Webinterface.HttpServer {

    class GetHandling
    {


    #region field declaration
        private Socket mSock;
        private string mFilename;
        private string mResponse;
        private bool FDisposed = false;
    
    #endregion field declaration



   #region properties
        public Socket Socket
        {
            set
            {
                mSock = value;
            }
        }

        public string Filename
        {
            set
            {
                mFilename = value;
            }
        }

        public string Response
        {
            set
            {
                mResponse = value;
            }
        }



        #endregion properies



    #region Constructor /Deconstructor

        public GetHandling() 
        {
       
        }

        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time
            GC.SuppressFinalize(this);
        }

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
                //mWebinterfaceSingelton.DeleteServhandling(this);
                Debug.WriteLine("Handling is being deleted");
                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

     #endregion Constructor /Deconstructor



    #region DataHandling

        public void Run(){

          try 
          {
              sendFile(mResponse, mFilename);
          }
          catch 
          {
            sendResponse("400 BAD REQUEST", "Protokollfehler");
          }
          
          mSock.Close();
          this.Dispose();
        }        
        
        private void sendResponse(String status, String tosend) {

          LogConsole(status);
          sendText("HTTP/1.1 " + status);
          sendText("Server: WebInterface 0.1");
          sendText("Connection: close");
          sendText("Content-Type: text/html");
          sendText("Content-Length: " + (tosend.Length + Environment.NewLine.Length));
          sendText("");
          sendText(tosend);
        }

        private void sendText(String text) {
          mSock.Send(Encoding.UTF8.GetBytes(text + Environment.NewLine));
          //Debug.WriteLine("Socket send: " + text + "/n");
          //Debug.WriteLine("sock.Available: " + sock.Available.ToString());
        }

        private void sendtextWithoutNewLine(string text)
        {
          mSock.Send(Encoding.UTF8.GetBytes(text));
          //Debug.WriteLine("Socket send: " + text);
          //Debug.WriteLine("sock.Available: " + sock.Available.ToString());
        }

        private void sendFile(String page,String filename ) 
        {
            LogConsole("200 OK [ " + filename + " ]");
            sendText("HTTP/1.1 200 OK");
            sendText("Server: VVVVVVVV.Webinterface.Webserver 1beta1");
            sendText("Connection: keep-alive");
            sendText("Content-Type: text/html");
            sendText("Content-Length: " + page.Length);
            sendText("");


            long read = 0;
            int tStartSubstring = 0;
            while (read < page.Length)
            {
                int buffersize;

                if (page.Length - read > 1024)
                {
                  buffersize = 1024;
                  read += 1024;
                }
                else
                {
                  buffersize = (int)(page.Length - read);
                  read = page.Length;
                }

                string tPartofPage = page.Substring(tStartSubstring, buffersize);
                tStartSubstring = Convert.ToInt32(read);
                sendtextWithoutNewLine(tPartofPage);
                Thread.Sleep(10);
            }

       }

      private void LogConsole(String Message) {
          Debug.WriteLine(((IPEndPoint)mSock.RemoteEndPoint).Address.ToString() + " - " + Message);
        }
       
    #endregion Datahandling 
    }
}
