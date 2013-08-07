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
using VVVV.Utils.VMath;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "RGBDepth", Category = "Kinect",Version="Microsoft", Author = "vux", Tags = "EX9, texture")]
    public class KinectColorDepthTextureNode : IPluginEvaluate, IPluginConnections, IPluginDXTexture2
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

        private float[] colorimage;
        private short[] depthimage;
        private ColorImagePoint[] cp;
        private object m_colorlock = new object();



        private Dictionary<Device, Texture> FColorTex = new Dictionary<Device, Texture>();


        [ImportingConstructor()]
        public KinectColorDepthTextureNode(IPluginHost host)
        {
            this.colorimage = new float[320 * 320 *2];
            this.depthimage = new short[320 * 240];
            this.cp = new ColorImagePoint[320 * 240];
            host.CreateTextureOutput("Texture Out", TSliceMode.Single, TPinVisibility.True, out this.FOutTexture);
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.AllFrameReady -= AllFrameReady;
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.FInRuntime[0].AllFrameReady += AllFrameReady;
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
            if (this.FColorTex.ContainsKey(OnDevice)) 
            { 
            	return this.FColorTex[OnDevice]; 
            }
            else
            	return null;
        }

        public void UpdateResource(IPluginOut ForPin, Device OnDevice)
        {
            if (this.runtime != null)
            {

                if (!this.FColorTex.ContainsKey(OnDevice))
                {
                    Texture t;
                    if (OnDevice is DeviceEx)
                    {
                        t = new Texture(OnDevice, 320, 240, 1, Usage.Dynamic, Format.G32R32F, Pool.Default);
                    }
                    else
                    {
                        t = new Texture(OnDevice, 320, 240, 1, Usage.None, Format.G32R32F, Pool.Managed);
                    }
                    this.FColorTex.Add(OnDevice, t);
                }

                if (this.FInvalidate)
                {
                    Texture tx = this.FColorTex[OnDevice];
                    Surface srf = tx.GetSurfaceLevel(0);
                    DataRectangle rect = srf.LockRectangle(LockFlags.Discard);

                    lock (this.m_colorlock)
                    {
                        rect.Data.WriteRange<float>(this.colorimage, 0, 320*240*2);
                    }
                    srf.UnlockRectangle();


                    this.FInvalidate = false;
                }
            }
        }

        public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
        {
            if (this.FColorTex.ContainsKey(OnDevice))
            {
                this.FColorTex[OnDevice].Dispose();
                this.FColorTex.Remove(OnDevice);
            }
        }

        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            //ColorImageFrame frame = e.OpenColorImageFrame();
            DepthImageFrame df = e.OpenDepthImageFrame();


            

            if (df != null)
            {
                this.FInvalidate = true;
                //this.frameindex = frame.FrameNumber;

                lock (m_colorlock)
                {
                    df.CopyPixelDataTo(this.depthimage);
                    //frame.CopyPixelDataTo(this.colorimage);
                    //Marshal.Copy(frame..Image.Bits, 0, this.colorimage, 640 * 480 * 4);
                }

                this.runtime.Runtime.MapDepthFrameToColorFrame(DepthImageFormat.Resolution320x240Fps30, this.depthimage, ColorImageFormat.RgbResolution640x480Fps30, this.cp);

                lock (m_colorlock)
                {
                    for (int i = 0; i < this.cp.Length; i++)
                    {
                        this.colorimage[i * 2] = (float)VMath.Map(cp[i].X, 0, 640, 0, 1,TMapMode.Clamp);
                        this.colorimage[i * 2 +1] = (float)VMath.Map(cp[i].Y, 0, 480, 0, 1, TMapMode.Clamp);
                    }
                }


                this.FInvalidate = true;
                this.frameindex = df.FrameNumber;

                df.Dispose();
            }  
        }
    }
}
