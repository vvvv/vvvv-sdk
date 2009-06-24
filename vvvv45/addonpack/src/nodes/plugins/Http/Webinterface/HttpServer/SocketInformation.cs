using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace VVVV.Webinterface.HttpServer
{
    class SocketInformation
    {

        
        private Socket mClientSocket;
        private string mSocketId = String.Empty;
        private const int mBufferSize = 1024;
        private byte[] mDataBuffer = new byte[mBufferSize];
        public StringBuilder Request = new StringBuilder();
        private string mResponse;
        private DateTime mTimeStamp;
        private byte[] mResponseAsBytes;
        private SortedList<string, byte[]> mHtmlPages;

        public byte[] Buffer
        {
            get
            {
                return mDataBuffer;
            }
            set
            {
                mDataBuffer = value;
            }
        }

        public string Response
        {
            get
            {
                return mResponse;
            }
            set
            {
                mResponse = value;
            }
        }

        public byte[] ResponseAsBytes
        {
            get
            {
                return mResponseAsBytes;
            }
            set
            {
                mResponseAsBytes = value;
            }
        }

        public Socket ClientSocket
        {
            get
            {
                return mClientSocket;
            }
            set
            {
                mClientSocket = null;
            }
        }

        public string SocketId
        {
            get
            {
                return mSocketId;
            }
        }

        public int BufferSize
        {
            get
            {
                return mBufferSize;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return mTimeStamp;
            }
            set
            {
                mTimeStamp = value;
            }
        }

        public SortedList<string, byte[]> HtmlPages
        {
            get
            {
                return mHtmlPages;
            }
            set
            {
                mHtmlPages = value;
            }
        }
    

        public SocketInformation(Socket pClientSocket, string pSocketId)
        {
            this.mClientSocket = pClientSocket;
            this.mSocketId = pSocketId;
        }
    }
}
