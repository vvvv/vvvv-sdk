#region usings
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Animation;
using VVVV.Utils.Network;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Sync",
    Category = "Network",
    Version = "FileStream",
    Help = "Syncronizes a FileStream node over network in a boygroup setup",
    Tags = "",
    AutoEvaluate = true)]
    #endregion PluginInfo
    public class FileStreamNetworkSyncNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins
#pragma warning disable 0649
        [Input("Duration", IsSingle = true)]
        ISpread<double> FLength;

        [Input("Position", IsSingle = true)]
        ISpread<double> FTime;
        
        [Input("Seek Threshold", DefaultValue = 1, IsSingle = true)]
        ISpread<double> FSeekThreshold;

        [Input("Fine Offset (ms)", IsSingle = true)]
        ISpread<int> FFineOffset;

        [Input("Is Client", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        ISpread<bool> FIsLocalClient;

        [Input("Server IP", DefaultString = "127.0.0.1", IsSingle = true, StringType = StringType.IP, Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FServerIP;

        [Input("Port", DefaultValue = 3336)]
        IDiffSpread<int> FPort;

        [Output("Do Seek")]
        ISpread<bool> FDoSeekOut;

        [Output("Seek Position")]
        ISpread<double> FSeekTimeOut;

        [Output("Adjust System Time")]
        ISpread<double> FAdjustTimeOut;

        [Output("Offset")]
        ISpread<double> FOffsetOut;

        [Output("Stream Offset")]
        ISpread<double> FStreamOffsetOut;

        [Import]
        IHDEHost FHost;
#pragma warning restore

        object FLock = new object();
        double FStreamTime;
        double FTimeStamp;
        double FReceivedStreamTime;
        double FReceivedTimeStamp;
        double FRequestSentTimeStamp;
        double FHalfRoundTripDelay;
        int FFrameCounter;
        IIRFilter FStreamDiffFilter;
        Spread<UDPServer> FServer;
        IPEndPoint FRemoteServer;
        Timer FTimer;
        #endregion fields & pins

        public FileStreamNetworkSyncNode()
        {
            FStreamDiffFilter.Value = 0;
            FStreamDiffFilter.Thresh = 1;
            FStreamDiffFilter.Alpha = 0.95;
            FServer = new Spread<UDPServer>(0);
            FTimer = new Timer(500);
            FTimer.Elapsed += FTimer_Elapsed;
            FTimer.Start();
        }

        //request video time
        void FTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(FIsLocalClient[0] || FHost.IsBoygroupClient)
            {
                //request sync data
                FRequestSentTimeStamp = FHost.RealTime;
                FServer[0].Send(Encoding.ASCII.GetBytes("videosync"), FRemoteServer);
            }
        }
        
        UDPServer CreateServer(int slice)
        {
            var server = (FIsLocalClient[0] || FHost.IsBoygroupClient) ? new UDPServer(FPort[slice] + 1) : new UDPServer(FPort[slice]);
            server.MessageReceived += FServer_MessageReceived;
            server.Start();
            return server;
        }

        void DeleteServer(UDPServer server)
        {
            server.MessageReceived -= FServer_MessageReceived;
            server.Close();
            server = null;
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            var isClient = FIsLocalClient[0] || FHost.IsBoygroupClient;
            
            //set server and port
            if (FPort.IsChanged)
            {
                FServer.Resize(FPort.SliceCount, CreateServer, DeleteServer);
                 
                for (int i = 0; i < FServer.SliceCount; i++) 
                {
                    FServer[i].Port = isClient ? FPort[i] + 1 : FPort[i];
                }                

                if (isClient) // store the time server
                    FRemoteServer = new IPEndPoint(IPAddress.Parse(FIsLocalClient[0] ? FServerIP[0] : FHost.BoygroupServerIP), FPort[0]);
            }

            //read stream time
            lock (FLock)
            {
                FStreamTime = FTime[0];
                FTimeStamp = FHost.RealTime;
            }

            //do the evaluation for client or server
            if (isClient)
            {
                ClientEvaluate();
            }
            else
            {
                ServerEvaluate();
            }
        }

        //respond to udp message
        void FServer_MessageReceived(object sender, UDPReceivedEventArgs e)
        {
            var server = (UDPServer)sender;
            if (FIsLocalClient[0] || FHost.IsBoygroupClient)
            {
                ReceiveServerAnswer(e.Data);
            }
            else //server code
            {
                lock (FLock)
                {
                    using (var stream = new MemoryStream())
                    {
                        stream.Write(BitConverter.GetBytes(FStreamTime), 0, 8);
                        stream.Write(BitConverter.GetBytes(FTimeStamp), 0, 8);
                        server.Send(stream.ToArray(), e.RemoteSender);
                    }

                    //FLogger.Log(LogType.Debug, FStreamTime.ToString() + ";" + FHost.RealTime.ToString());
                }
            }
        }

        void ReceiveServerAnswer(byte[] data)
        {
            FHalfRoundTripDelay = (FHost.RealTime - FRequestSentTimeStamp) * 0.5;

            lock (FLock)
            {
                FReceivedStreamTime = BitConverter.ToDouble(data, 0);
                FReceivedTimeStamp = BitConverter.ToDouble(data, 8);
            }
        }

        protected void ClientEvaluate()
        {
            var fCount = 5;
            lock (FLock)
            {
                var offset = FIsLocalClient[0] ? FHalfRoundTripDelay : FTimeStamp - FReceivedTimeStamp;
                var streamDiff = FReceivedStreamTime - FStreamTime + offset + FFineOffset[0] * 0.001;
                streamDiff = Math.Abs(streamDiff) > FLength[0] * 0.5 ? streamDiff - FLength[0] * Math.Sign(streamDiff) : streamDiff;
                var doSeek = Math.Abs(streamDiff) > FSeekThreshold[0];

                FStreamDiffFilter.Update(streamDiff);

                FDoSeekOut[0] = doSeek;
                FSeekTimeOut[0] = FReceivedStreamTime + offset + 0.05;

                var doAdjust = Math.Abs(FStreamDiffFilter.Value) > 0.001;

                if (!doSeek && FFrameCounter == 0 && doAdjust)
                {
                    FAdjustTimeOut[0] = FStreamDiffFilter.Value * 50;
                }
                else
                {
                    FAdjustTimeOut[0] = 0;
                }

                FStreamOffsetOut[0] = FStreamDiffFilter.Value;
                FOffsetOut[0] = offset;

                FFrameCounter = (++FFrameCounter) % fCount;
            }
        }

        protected void ServerEvaluate()
        {
        }

        public void Dispose()
        {
            if (FServer != null)
            {
                foreach (var server in FServer) {
                    DeleteServer(server);
                }
            }

            if (FTimer != null)
            {
                FTimer.Elapsed -= FTimer_Elapsed;
                FTimer.Dispose();
                FTimer = null;
            }
        }
    }
}
