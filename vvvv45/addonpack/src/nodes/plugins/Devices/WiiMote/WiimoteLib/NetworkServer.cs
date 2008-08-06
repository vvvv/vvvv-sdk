using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace WiimoteLib
{
    public delegate void OnDataRecievedEventHandler(object sender, DataRecievedEventArgs args);
    public delegate void OnClientConnectEventHandler(object sender, ClientConnectEventArgs args);
    public delegate void OnClientDisconnectEventHandler(object sender, ClientDisconnectEventArgs args);
    
    /// <summary>
    /// Argument sent through the OnDataRecievedEvent
    /// </summary>
    public class DataRecievedEventArgs : EventArgs
    {
        public char[] data;
        public DataRecievedEventArgs(char[] d)
        {
            data = d;
        }
    }
    public class ClientConnectEventArgs : EventArgs
    {
        //should hold client network info
        public ClientConnectEventArgs()
        {

        }
    }
    public class ClientDisconnectEventArgs : EventArgs
    {
        //should hold client network info
        public ClientDisconnectEventArgs()
        {

        }
    }


    public class NetworkServer
    {
        public event OnDataRecievedEventHandler OnDataRecievedEventHandlers;
        public event OnClientConnectEventHandler OnClientConnectEventHandlers;
        public event OnClientDisconnectEventHandler OnClientDisconnectEventHandlers;

        public AsyncCallback pfnWorkerCallBack;
        public Socket m_socListener;
        public Socket m_socWorker;
        private bool isListening = false;
        private int port = 8221;

        public NetworkServer(){}
        public void startListening(int port)
        {
            this.port = port;
            //create the listening socket...
            m_socListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, port);
            //bind to local IP Address...
            m_socListener.Bind(ipLocal);
            //start listening...
            m_socListener.Listen(4);
            // create the call back for any client connections...
            m_socListener.BeginAccept(new AsyncCallback(OnClientConnect), null);
            isListening = true;
        }

        public bool IsListening()
        {
            return isListening;
        }

        public void Stop()
        {
            isListening = false;
        }

        [DllImport("Kernel32.dll")]
        public static extern bool Beep(UInt32 frequency, UInt32 duration);

        private void ClientDisconnect()
        {
            if (OnClientDisconnectEventHandlers != null)
                OnClientDisconnectEventHandlers(this, new ClientDisconnectEventArgs());

            //setup callback for another connection
            if(isListening)
                m_socListener.BeginAccept(new AsyncCallback(OnClientConnect), null);
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                m_socWorker = m_socListener.EndAccept(asyn);
                WaitForData(m_socWorker);
                if (OnClientConnectEventHandlers != null)
                    OnClientConnectEventHandlers(this, new ClientConnectEventArgs());
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
                ClientDisconnect();
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n " + se.Message+ "\n");
                ClientDisconnect();
            }

        }
        public class CSocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[1];
        }

        public void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                CSocketPacket theSocPkt = new CSocketPacket();
                theSocPkt.thisSocket = soc;
                // now start to listen for any data...
                soc.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, pfnWorkerCallBack, theSocPkt);
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n " + se.Message + "\n");
                ClientDisconnect();
            }

        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
                //end receive...
                int iRx = 0;
                iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
         
                if (OnDataRecievedEventHandlers != null)
                    OnDataRecievedEventHandlers(this, new DataRecievedEventArgs(chars));
                WaitForData(m_socWorker);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
                ClientDisconnect();
            }
            catch (SocketException se)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n " + se.Message + "\n");
                ClientDisconnect();
            }
        }
        public void Send(byte[] data)
        {
            if (m_socWorker == null)
                return;

            if (m_socWorker.Connected)
            {
                try
                {
                    m_socWorker.Send(data);
                }
                catch (SocketException se)
                {
                    System.Diagnostics.Debugger.Log(0, "1", "\n " + se.Message + "\n");
                    ClientDisconnect();
                }
            }
        }
    }
}
