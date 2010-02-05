using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{

    /// <summary>
    /// Carries all Information from an HTTP Request. Is Required for async Socket Handling. 
    /// </summary>
    class SocketInformation
    {

        
        private Socket mClientSocket;
        private string mSocketIP = String.Empty;
        private const int mBufferSize = 1024;
        private byte[] mDataBuffer = new byte[mBufferSize];
        private StringBuilder FRequest = new StringBuilder();
        private string mResponse;
        private DateTime mTimeStamp;
        private byte[] mResponseAsBytes;
        private SortedList<string, byte[]> mHtmlPages;
        private Request mRequestObject;
        private Stopwatch mStopwatch;
        private int FTotalBytes = 0;
        
        

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
                return mSocketIP;
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

        public Request RequestObject
        {
            get
            {
                return mRequestObject;
            }
            set
            {
                mRequestObject = value;
            }
        }

        public Stopwatch StopWatch
        {
            get
            {
                return mStopwatch;
            }
            set
            {
                mStopwatch = value;
            }
        }

        public int TotalBytes
        {
            get
            {
                return FTotalBytes;
            }
        }

        public string Request
        {
            get
            {
               return FRequest.ToString();
            }
        }
    

        public SocketInformation(Socket pClientSocket, string pSocketId)
        {
            this.mClientSocket = pClientSocket;
            this.mSocketIP = pSocketId;
        }

        public void AppendRequest(string Content)
        {
            FRequest.Append(Content);
        }


        public void AddToTotalBytes(int count)
        {
            FTotalBytes += count;
        }
    }
}
