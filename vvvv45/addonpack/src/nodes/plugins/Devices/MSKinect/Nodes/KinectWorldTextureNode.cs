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
    [PluginInfo(Name = "World", Category = "Kinect", Version = "Microsoft", Author = "vux", Tags = "EX9, texture")]
    public unsafe class KinectWorldTextureNode : IPluginEvaluate, IPluginConnections, IPluginDXTexture2
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

        private float[] color0;
        private float[] color1;

        private object m_lock = new object();

        private Dictionary<Device, Texture> FDepthTex = new Dictionary<Device, Texture>();

        [ImportingConstructor()]
        public KinectWorldTextureNode(IPluginHost host)
        {
            this.color0 = new float[320 * 240*4];
            this.color1 = new float[320 * 240*4];
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
                        t = new Texture(OnDevice, 320, 240, 1, Usage.Dynamic, Format.A32B32G32R32F, Pool.Default);
                    }
                    else
                    {
                       t = new Texture(OnDevice, 320, 240, 1, Usage.None, Format.A32B32G32R32F, Pool.Managed);
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
                        rect.Data.WriteRange(this.color1);
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
                if (frame.FrameNumber != this.frameindex)
                {

                    this.frameindex = frame.FrameNumber;

                    int cnt = 0;
                    for (int h = 0; h < 240; h++)
                    {
                        for (int w = 0; w < 320; w++)
                        {
                            SkeletonPoint sp = frame.MapToSkeletonPoint(w, h);
                            this.color0[cnt] = sp.X;
                            this.color0[cnt + 1] = sp.Y;
                            this.color0[cnt + 2] = sp.Z;
                            this.color0[cnt + 3] = 1.0f;
                            cnt += 4;
                        }
                    }

                    frame.Dispose();

                    lock (m_lock)
                    {
                        float[] tmp = this.color0;
                        this.color0 = this.color1;
                        this.color1 = tmp;
                    }
                }
            }  
        }
    }
}
