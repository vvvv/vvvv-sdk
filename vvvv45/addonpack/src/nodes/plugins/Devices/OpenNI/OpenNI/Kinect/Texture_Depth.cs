#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading;


using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.EX9;

using OpenNI;
using SlimDX.Direct3D9;
using VVVV.Utils.SlimDX;
using System.Drawing;
using System.Drawing.Imaging;


#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Depth",
                Category = "EX9.Texture",
                Version ="Kinect",
                Help = "Gesture recognition from the Kinect",
                Tags = "Kinect, OpenNI,",
                Author = "Phlegma")]
    #endregion PluginInfo


    public class Texture_Depth : DXTextureOutPluginBase, IPluginEvaluate
    {
        #region fields & pins
        [Input("Context", IsSingle = true)]
        ISpread<Context> FInput;

        [Input("Enable", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FEnableIn;

        [Import()]
        ILogger FLogger;


        private int FTexWidth = 640;
        private int FTexHeight = 480;

        private DepthGenerator FDepthGenerator; 
        private Bitmap FDepthTexture;
        private IntPtr FDepthPointer;
        private int[] FHistogram;
        private int FDataSize;

        private Thread FReadThread;
        private bool FInit = true;
        private int FCurrentSlice;

        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
        public Texture_Depth(IPluginHost host)
            : base(host)
        {
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput[0] != null)
            {
                if (FInit == true)
                {
                    if (FDepthGenerator == null)
                    {
                        FDepthGenerator = new DepthGenerator(FInput[0]);

                        FHistogram = new int[FDepthGenerator.DeviceMaxDepth];
                        MapOutputMode MapMode = FDepthGenerator.MapOutputMode;
                        MapMetaData MetaData = FDepthGenerator.GetMetaData();
                        FDataSize = MetaData.DataSize;

                        //Set the resolution of the texture
                        FTexWidth = MapMode.XRes;
                        FTexHeight = MapMode.YRes;
                        

                        //create an texture where the kinect image is written to
                        FDepthTexture = new System.Drawing.Bitmap(FTexWidth, FTexHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        //Reinitalie the vvvv texture
                        Reinitialize();

                        //start generating data
                        FDepthGenerator.StartGenerating();

                        //create a image reader thread
                        FReadThread = new Thread(ReadDepthImage);
                        FReadThread.Start();
                    }
                }
                else
                {
                    //update the vvvv texture
                    if(FEnableIn[0] == true)
                        Update();
                }

                FInit = false;
            }
        }

        //reade the kinect image data and write it to the holder FDepthTexture
        private unsafe void ReadDepthImage()
        {

            while (true)
            {
                if (FInit == false || FDepthGenerator.IsDataNew)
                {
                    lock (this)
                    {
                        BitmapData data = null;
                        try
                        {
                            DepthMetaData DepthMD = FDepthGenerator.GetMetaData();


                            CalculateHistogram(DepthMD);

                            //lock the FDepth texture to wrtie the depth image data to the texture
                            Rectangle rect = new Rectangle(0, 0, FDepthTexture.Width, FDepthTexture.Height);
                            data = FDepthTexture.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                            // get the Pointer to the depth image

                            ushort* pDepth = (ushort*)FDepthGenerator.DepthMapPtr;

                            // write the Depth pointer to Destination pointer
                            for (int y = 0; y < FTexHeight; ++y)
                            {

                                byte* pDest = (byte*)data.Scan0.ToPointer() + y * data.Stride;

                                for (int x = 0; x < FTexWidth; ++x, ++pDepth, pDest += 4)
                                {
                                    pDest[0] = pDest[1] = pDest[2] = 128;
                                    pDest[3] = 255;

                                    byte pixel = (byte)FHistogram[*pDepth];
                                    pDest[0] = (byte)(pixel);
                                    pDest[1] = (byte)(pixel);
                                    pDest[2] = (byte)(pixel);
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            return;
                        }
                        catch (AccessViolationException)
                        {
                            return;
                        }
                        finally
                        {
                            if (data != null)
                            {
                                //Unlock the Depthtexture
                                FDepthTexture.UnlockBits(data);

                                //set the pointer for vvvv
                                FDepthPointer = data.Scan0;
                            }
                        }

                    }
                }
            }
        }

        private unsafe void CalculateHistogram(DepthMetaData DepthMD)
        {
            for (int i = 0; i < FHistogram.Length; ++i)
                FHistogram[i] = 0;

            try
            {
                ushort* pDepth = (ushort*)DepthMD.DepthMapPtr.ToPointer();

                int points = 0;
                for (int y = 0; y < DepthMD.YRes; ++y)
                {
                    for (int x = 0; x < DepthMD.XRes; ++x, ++pDepth)
                    {
                        if (FInput[0] != null)
                        {
                            ushort depthVal = *pDepth;
                            if (depthVal != 0)
                            {
                                FHistogram[depthVal]++;
                                points++;
                            }
                        }
                    }
                }


                for (int i = 1; i < FHistogram.Length; i++)
                {
                    FHistogram[i] += FHistogram[i - 1];
                }

                if (points > 0)
                {
                    for (int i = 1; i < FHistogram.Length; i++)
                    {
                        FHistogram[i] = (int)(256 * (1.0f - (FHistogram[i] / (float)points)));
                    }
                }
            }
            catch (AccessViolationException)
            {
                //FLogger.Log(ex);
            }
            catch (IndexOutOfRangeException)
            {
                //FLogger.Log(ex);
            }
        }



        #region IPluginDXTexture Members

        //this method gets called, when Reinitialize() was called in evaluate,
        //or a graphics device asks for its data
        protected override Texture CreateTexture(int Slice, SlimDX.Direct3D9.Device device)
        {
            //FLogger.Log(LogType.Debug, "Creating new texture at slice: " + Slice + " -> W: " + FTexWidth + "H: " + FTexHeight);
            return TextureUtils.CreateTexture(device, FTexWidth, FTexHeight);
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its texture, here you fill the texture with the actual data
        //this is called for each renderer, careful here with multiscreen setups, in that case
        //calculate the pixels in evaluate and just copy the data to the device texture here
        unsafe protected override void UpdateTexture(int Slice, Texture texture)
        {

            FCurrentSlice = Slice;
            try
            {
                if (FEnableIn[0] == true && FInput[0] != null)
                {
                    //lock the vvvv texture
                    var rect = texture.LockRectangle(0, LockFlags.Discard).Data;

                    // tell the vvvv texture where the pointer to the depth image lies
                    if (FDepthPointer.ToInt32() != 0)
                        rect.WriteRange(FDepthPointer,FTexHeight * FTexWidth * 4);
                    //unlock the vvvv texture
                    texture.UnlockRectangle(0);
                }
            }
            catch (Direct3D9Exception ex)
            {
                FLogger.Log(ex);
            }
            
        }

        #endregion IPluginDXResource Members
    }
}
