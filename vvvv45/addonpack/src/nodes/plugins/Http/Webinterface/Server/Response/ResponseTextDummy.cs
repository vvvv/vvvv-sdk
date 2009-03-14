using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;

namespace VVVV.Webinterface.HttpServer
{
    class ResponseTextDummy:Response
    {
         private string mContent;
        //private Socket mSocket;

        public ResponseTextDummy(string pResponseHead, Socket pSocket, string pFilename , string pContent)
        {
            mContent = pResponseHead + pContent;
            
            this.mSocket = pSocket;
            Debug.WriteLine("constructor ResponseTextDummy");
        }

        public override void Run()
        {
            sendText(mContent);
        }
    }
}
