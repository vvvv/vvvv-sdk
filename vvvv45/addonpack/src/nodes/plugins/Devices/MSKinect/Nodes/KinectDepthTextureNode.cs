using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using System.Runtime.InteropServices;
using Microsoft.Research.Kinect.Nui;
using VVVV.MSKinect.Lib;
using System.ComponentModel.Composition;
using SlimDX.Direct3D9;
using SlimDX;

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
        private object m_lock = new object();

        private Dictionary<Device, Texture> FDepthTex = new Dictionary<Device, Texture>();

        [ImportingConstructor()]
        public KinectDepthTextureNode(IPluginHost host)
        {
            //this.depthimage = Marshal.AllocCoTaskMem(320 * 240);
            this.depthimage = new byte[320 * 240];
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
                    this.FInRuntime[0].DepthFrameReady += DepthFrameReady;
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
            if (!this.FDepthTex.ContainsKey(OnDevice))
            {
                Texture t = new Texture(OnDevice, 320, 240, 1, Usage.None, Format.L8, Pool.Managed);
                this.FDepthTex.Add(OnDevice, t);
            }

            if (this.FInvalidate)
            {
                Texture tx = this.FDepthTex[OnDevice];
                Surface srf = tx.GetSurfaceLevel(0);
                DataRectangle rect = srf.LockRectangle(LockFlags.Discard);

                lock (this.m_lock)
                {
                    //rect.Data.WriteRange(this.depthimage, 320 * 240);
                    rect.Data.WriteRange(this.depthimage);
                }
                srf.UnlockRectangle();


                this.FInvalidate = false;
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

        private void DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            bool hasplayer = e.ImageFrame.Type == ImageType.DepthAndPlayerIndex;
            
            byte[] d16 = e.ImageFrame.Image.Bits;
            lock (m_lock)
            {
                if (hasplayer)
                {
                    for (int i = 0, i16 = 0; i < 320 * 240; i++, i16 += 2)
                    {
                        int realDepth = (d16[i16 + 1] << 5) | (d16[i16] >> 3);
                        byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));
                        this.depthimage[i] = intensity;
                    }
                }
                else
                {
                    /*int i16 = 0;
                    for (int i = 0; i < 240; i++)
                    {
                        for (int j = 0; j < 320; j++)
                        {
                            int realDepth = (d16[i16 + 1] << 8) | (d16[i16]);
                            byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));
                            this.depthimage[(319-j)+  i * 240] = intensity;
                            i16 += 2;
                        }
                    }*/
                    for (int i = 0, i16 = 0; i < 320 * 240; i++, i16 += 2)
                    {
                        int realDepth = (d16[i16 + 1] << 8) | (d16[i16]);
                        byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));
                        this.depthimage[i] = intensity;
                    }
                }

                //Marshal.Copy(e.ImageFrame.Image.Bits, 0, this.depthimage, 640 * 480);
            }
            this.FInvalidate = true;
            this.frameindex = e.ImageFrame.FrameNumber;
        }
    }
}
