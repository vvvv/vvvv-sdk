using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Webinterface.HttpServer;
using System.Net.Sockets;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{
    class ResponsePic : Response
    {
        private byte[] mContentArray;
        private string mHeader;
        //private Socket mSocket;

        public ResponsePic(string pResponseHead, Socket pSocket, string pFilename,  byte[] pContentArray)
        {
            mHeader = pResponseHead;
            mContentArray = pContentArray;

            mSocket = pSocket;
            Debug.WriteLine("constructor ResponsePic");
        }

        public override void Run()
        {
            
            sendFile(mHeader);
            sendByteArray(mContentArray);
            mSocket.Close();
        }
    }
}
