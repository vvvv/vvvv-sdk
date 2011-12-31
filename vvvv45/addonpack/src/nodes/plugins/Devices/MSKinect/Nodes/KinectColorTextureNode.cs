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
    [PluginInfo(Name = "RGB", Category = "Kinect",Version="Microsoft", Author = "vux",Tags="directx,texture")]
    public class KinectColorTextureNode : IPluginEvaluate, IPluginConnections, IPluginDXTexture
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        [Output("Frame Index",IsSingle=true, Order=10)]
        private ISpread<int> FOutFrameIndex;

        private int frameindex = -1;

        private IDXTextureOut FOutTexture;

        private bool FInvalidateConnect = false;
        private bool FInvalidate = true;

        private KinectRuntime runtime;

        private IntPtr colorimage;
        private object m_colorlock = new object();



        private Dictionary<int, Texture> FColorTex = new Dictionary<int, Texture>();


        [ImportingConstructor()]
        public KinectColorTextureNode(IPluginHost host)
        {
            this.colorimage = Marshal.AllocCoTaskMem(640 * 480 * 4);
            host.CreateTextureOutput("Texture Out", TSliceMode.Single, TPinVisibility.True, out this.FOutTexture);
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.ColorFrameReady -= ColorFrameReady;
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];
                    this.FInRuntime[0].ColorFrameReady += ColorFrameReady;
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

        public void GetTexture(IDXTextureOut ForPin, int OnDevice, out int tex)
        {
            tex = 0;
            if (this.FColorTex.ContainsKey(OnDevice)) { tex = this.FColorTex[OnDevice].ComPointer.ToInt32(); }
        }

        public void UpdateResource(IPluginOut ForPin, int OnDevice)
        {
            if (!this.FColorTex.ContainsKey(OnDevice))
            {
                Texture t = new Texture(Device.FromPointer(new IntPtr(OnDevice)), 640, 480, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);
                this.FColorTex.Add(OnDevice, t);
            }

            if (this.FInvalidate)
            {
                Texture tx = this.FColorTex[OnDevice];
                Surface srf = tx.GetSurfaceLevel(0);
                DataRectangle rect = srf.LockRectangle(LockFlags.Discard);

                lock (this.m_colorlock)
                {
                    rect.Data.WriteRange(this.colorimage, 640 * 480 * 4);
                }
                srf.UnlockRectangle();


                this.FInvalidate = false;
            }
        }

        public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
        {
            if (this.FColorTex.ContainsKey(OnDevice))
            {
                this.FColorTex[OnDevice].Dispose();
                this.FColorTex.Remove(OnDevice);
            }
        }

        private void ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            lock (m_colorlock)
            {
                Marshal.Copy(e.ImageFrame.Image.Bits, 0, this.colorimage, 640 * 480 * 4);
            }
            this.FInvalidate = true;
            this.frameindex = e.ImageFrame.FrameNumber;
        }
    }
}
