using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{
    abstract class Response
    {

        #region field declaration
        public Socket mSocket;
        #endregion field declaration




        public virtual void Run()
        {
        }



        public void sendText(String text)
        {
            try
            {
                if (mSocket.Connected)
                {
                    mSocket.Send(Encoding.UTF8.GetBytes(text + Environment.NewLine));
                }
                else
                {
                    Debug.WriteLine("Connection Dropped....");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void sendtextWithoutNewLine(string text)
        {
            int numBytes = 0;
            
            if (mSocket.Connected)
            {
                if ((numBytes = mSocket.Send(Encoding.UTF8.GetBytes(text), text.Length, 0)) == -1)
                    Debug.WriteLine("Socket Error cannot Send Packet");
                else
                {
                    //mSocket.Send(Encoding.UTF8.GetBytes(text));
                    Debug.WriteLine(String.Format("No. of bytes send {0}", numBytes));
                }
            }
            else
                Debug.WriteLine("Connection Dropped....");
        }



        public void sendFile(String page)
        {
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



        public void sendByteArray(byte[] pContent)
        {

            int numBytes = 0;
            try
            {
                if (mSocket.Connected)
                {
                    if ((numBytes = mSocket.Send(pContent, pContent.Length, 0)) == -1)
                        Console.WriteLine("Socket Error cannot Send Packet");
                    else
                    {
                        Console.WriteLine("No. of bytes send {0}", numBytes);
                    }
                }
                else
                    Console.WriteLine("Connection Dropped....");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Occurred : {0} ", e);
            }

        }

        
        public void LogConsole(String Message)
        {
            Debug.WriteLine(((IPEndPoint)mSocket.RemoteEndPoint).Address.ToString() + " - " + Message);
        }

    }
}
