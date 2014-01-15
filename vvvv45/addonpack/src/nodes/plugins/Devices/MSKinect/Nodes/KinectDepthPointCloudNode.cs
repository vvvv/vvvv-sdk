using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Kinect;
using SlimDX.Direct3D9;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "DepthPointCloud", Category = "Kinect", Version = "Microsoft", Author = "rvazquez", Tags = "kinect,pointcloud")]
    public unsafe class KinectDepthPointCloudNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        [Input("Resolution", DefaultValue = 10)]
        private Pin<int> FInResolution;

        [Input("MinTreshold", DefaultValue = 1)]
        private Pin<int> FInMinTreshold;
        
        [Input("MaxTreshold", DefaultValue = 4000)]
        private Pin<int> FInMaxTreshold;

        [Output("X")]
        private ISpread<int> FOutX;

        [Output("Y")]
        private ISpread<int> FOutY;

        [Output("Z")]
        private ISpread<int> FOutZ;

        [Output("MinZ")]
        private ISpread<int> FOutMinZ;

        [Output("MaxZ")]
        private ISpread<int> FOutMaxZ;

        private bool FInvalidateConnect = false;
        private bool FInvalidate = true;

        private KinectRuntime runtime;

        private DepthImagePixel[] depthpixels;
        private short[] rawdepth;

        private object m_lock = new object();

        private Dictionary<Device, Texture> FDepthTex = new Dictionary<Device, Texture>();

        [ImportingConstructor()]
        public KinectDepthPointCloudNode(IPluginHost host)
        {
            this.depthpixels = new DepthImagePixel[320 * 240];
            this.rawdepth = new short[320 * 240];
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

        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (FInResolution[0] <= 0)
                FInResolution[0] = 10;
            int s = FInResolution[0];

            int loTreshold = FInMinTreshold[0];
            int hiTreshold = FInMaxTreshold[0];

            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    this.FInvalidate = true;
                    lock (m_lock)
                    {
                        short[] pixelData = new short[frame.PixelDataLength];
                        frame.CopyPixelDataTo(pixelData);
                        int temp = 0;
                        int i = 0;
                        int j = 0;
                        for (int y = 0; y < frame.Height; y += s)
                            for (int x = 0; x < frame.Width; x += s)
                            {
                                temp = ((ushort)pixelData[x + y * frame.Width]) >> DepthImageFrame.PlayerIndexBitmaskWidth;
                                FOutX[i] = x;
                                FOutY[i] = y;
                                if (temp < loTreshold || temp > hiTreshold)
                                {
                                    FOutZ[i] = 0;
                                }
                                else
                                {
                                    FOutZ[i] = temp;
                                }
                                i++;
                            }
                        FOutX.SliceCount = i;
                        FOutY.SliceCount = i;
                        FOutZ.SliceCount = i;
                        FOutMinZ.SliceCount = 1;
                        FOutMaxZ.SliceCount = 1;
                        FOutMinZ[0] = frame.MinDepth;
                        FOutMaxZ[0] = frame.MaxDepth;
                    }
                }
            }
        }
    }
}
