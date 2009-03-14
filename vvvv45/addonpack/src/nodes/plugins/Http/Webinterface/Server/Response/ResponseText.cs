using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Webinterface.HttpServer;
using System.Net.Sockets;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{
    class ResponseText:Response
    {
        private string mContent;
        //private Socket mSocket;

        public ResponseText(string pResponseHead, Socket pSocket, string pFilename , string pContent)
        {
            mContent = pResponseHead + pContent;
            mSocket = pSocket;
            Debug.WriteLine("constructor ResponseText");
        }

        public override void Run()
        {
            sendtextWithoutNewLine(mContent);
            mSocket.Close();
        }
    }
}
