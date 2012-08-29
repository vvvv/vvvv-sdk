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

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Depth", Category = "Kinect", Version = "Microsoft", Author = "vux", Tags = "directx,texture")]
    public class KinectDepthTextureNode : IPluginEvaluate, IPluginConnections, IPluginDXTexture2
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        private IDXTextureOut FOutTexture;

        [Output("Frame Index", IsSingle = true, Order = 10)]
        private ISpread<int> FOutFrameIndex;

        private int frameindex = -1;

        private bool FInvalidateConnect = false;
        private bool FInvalidate = true;

        private KinectRuntime runtime;

        private byte[] depthimage;
        private short[] rawdepth;

        private object m_lock = new object();

        private Dictionary<Device, Texture> FDepthTex = new Dictionary<Device, Texture>();

        [ImportingConstructor()]
        public KinectDepthTextureNode(IPluginHost host)
        {
            this.depthimage = new byte[320 * 240];
            this.rawdepth = new short[320 * 240];
            host.CreateTextureOutput("Texture Out", TSliceMode.Single, TPinVisibility.True, out this.FOutTexture);
        }

        public void Evaluate(int SpreadMax)
        {
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
                        t = new Texture(OnDevice, 320, 240, 1, Usage.None, Format.L8, Pool.Default);
                    }
                    else
                    {
                        t = new Texture(OnDevice, 320, 240, 1, Usage.None, Format.L8, Pool.Managed);
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
                        rect.Data.WriteRange(this.depthimage);
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
                lock (m_lock)
                {
                    frame.CopyPixelDataTo(this.rawdepth);
                    for (int i16 = 0; i16 < 320 * 240; i16++)
                    {
                        int realDepth = rawdepth[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                        byte intensity = (byte)(~(realDepth >> 4));
                        this.depthimage[i16] = intensity;
                    }
                }
            }  
        }
    }
}
