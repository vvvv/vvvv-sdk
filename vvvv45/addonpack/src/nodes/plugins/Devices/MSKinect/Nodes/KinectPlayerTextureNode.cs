using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using System.Runtime.InteropServices;
using VVVV.MSKinect.Lib;
using System.ComponentModel.Composition;
using SlimDX.Direct3D9;
using SlimDX;
using Microsoft.Kinect;
using VVVV.Utils.VColor;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Player", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "EX9, texture",
	            Help = "?")]
    public class KinectPlayeTextureNode : IPluginEvaluate, IPluginConnections, IPluginDXTexture2
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        [Input("Back Color", DefaultColor=new double[] {0,0,0,1})]
        private IDiffSpread<RGBAColor> FInBgColor;

        [Input("Player Color", DefaultColor = new double[] { 1, 0, 0, 1 })]
        private IDiffSpread<RGBAColor> FInPlayerColor;

        private IDXTextureOut FOutTexture;

        [Output("Found Id", Order=9)]
        private ISpread<bool> FOutFoundId;

        [Output("Frame Index", IsSingle = true, Order = 10)]
        private ISpread<int> FOutFrameIndex;

        private int frameindex = -1;

        private bool FInvalidateConnect = false;
        private bool FInvalidate = true;

        private KinectRuntime runtime;

        private int[] playerimage;
        private short[] rawdepth;

        private object m_lock = new object();

        private int[] colors = new int[9];

        private bool[] found = new bool[9];

        private Dictionary<Device, Texture> FDepthTex = new Dictionary<Device, Texture>();

        [ImportingConstructor()]
        public KinectPlayeTextureNode(IPluginHost host)
        {
            this.playerimage = new int[640 * 480];
            this.rawdepth = new short[640 * 480];
            host.CreateTextureOutput("Texture Out", TSliceMode.Single, TPinVisibility.True, out this.FOutTexture);
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInBgColor.IsChanged)
            {
                this.colors[0] = this.FInBgColor[0].Color.ToArgb();
                this.FInvalidate = true;
            }

            if (this.FInPlayerColor.IsChanged)
            {
                for (int i = 0; i < 8; i++)
                {
                    this.colors[i + 1] = this.FInPlayerColor[i].Color.ToArgb();
                }

                this.FInvalidate = true;
            }

            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.DepthFrameReady -= DepthFrameReady;
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.FInRuntime[0].DepthFrameReady += DepthFrameReady;
                    }
                    
                }

                this.FInvalidateConnect = false;
            }

            this.FOutFrameIndex[0] = this.frameindex;

            this.FOutFoundId.SliceCount = this.found.Length;
            for (int i = 0; i < this.found.Length; i++)
            {
                this.FOutFoundId[i] = this.found[i];
            }
        }

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public Texture GetTexture(IDXTextureOut ForPin, Device OnDevice, int Slice)
        {
            if (this.FDepthTex.ContainsKey(OnDevice))
            {
                return this.FDepthTex[OnDevice];
            }
            else
                return null;
        }

        public void UpdateResource(IPluginOut ForPin, Device OnDevice)
        {
            if (this.runtime != null)
            {

                if (!this.FDepthTex.ContainsKey(OnDevice))
                {
                    Texture t = null;
                    if (OnDevice is DeviceEx)
                    {
                        t = new Texture(OnDevice, 640, 480, 1, Usage.Dynamic, Format.X8R8G8B8, Pool.Default);
                    }
                    else
                    {
                        t = new Texture(OnDevice, 640, 480, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);
                    }

                    this.FDepthTex.Add(OnDevice, t);
                }

                if (this.FInvalidate)
                {
                    Texture tx = this.FDepthTex[OnDevice];
                    Surface srf = tx.GetSurfaceLevel(0);
                    DataRectangle rect = srf.LockRectangle(LockFlags.Discard);

                    lock (this.m_lock)
                    {
                        rect.Data.WriteRange(this.playerimage);
                    }
                    srf.UnlockRectangle();


                    this.FInvalidate = false;
                }
            }
        }

        public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
        {
            if (this.FDepthTex.ContainsKey(OnDevice))
            {
                this.FDepthTex[OnDevice].Dispose();
                this.FDepthTex.Remove(OnDevice);
            }
        }

        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame frame = e.OpenDepthImageFrame();

            if (frame != null)
            {
                this.FInvalidate = true;
                this.frameindex = frame.FrameNumber;

                bool[] fnd = new bool[9];

                lock (m_lock)
                {

                    frame.CopyPixelDataTo(this.rawdepth);
                    for (int i16 = 0; i16 < 640 * 480; i16++)
                    {
                        int player = rawdepth[i16] & DepthImageFrame.PlayerIndexBitmask;

                        this.playerimage[i16] = this.colors[player];
                        fnd[player] = true;
                    }
                }

                this.found = fnd;
                
                frame.Dispose();
            }
        }
    }
}
