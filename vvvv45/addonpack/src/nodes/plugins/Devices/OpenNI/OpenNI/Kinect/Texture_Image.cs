#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Runtime.InteropServices;


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
using System.Drawing.Drawing2D;




#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "RGB",
                Category = "EX9.Texture",
                Version = "Kinect",
                Help = "Gesture recognition from the Kinect",
                Tags = "Kinect, OpenNI,",
                Author = "Phlegma")]
    #endregion PluginInfo


    public class Texture_Image : DXTextureOutPluginBase, IPluginEvaluate
    {
        #region fields & pins
        [Input("Context", IsSingle = true)]
        ISpread<Context> FContextIn;

        [Input("Enable", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FEnableIn;

        [Import()]
        ILogger FLogger;

     
        private int FCurrentSlice;
        private ImageGenerator FImageGenerator;
        private ImageMetaData FImageMetaData;

        private int FTexWidth = 640;
        private int FTexHeight = 480;

        private IntPtr FDataRGB = new IntPtr();

        private Thread FReadThread;
        private bool FInit = true;


        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
        public Texture_Image(IPluginHost host)
            : base(host)
        {
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FContextIn[0] != null)
            {
                if (FInit == true)
                {
                    //Create an Image Generator
                    FImageGenerator = new ImageGenerator(FContextIn[0]);
                    FImageMetaData = FImageGenerator.GetMetaData();

                    FTexWidth = FImageMetaData.XRes;
                    FTexHeight = FImageMetaData.YRes;

                    //allocate data for the RGB Image
                    this.FDataRGB = Marshal.AllocCoTaskMem(FTexWidth * FTexHeight * 4);

                    //Start the Thread for reading the Rgb Imag
                    FReadThread = new Thread(ReadRGBImage);
                    FReadThread.Start();
                }
                else
                {
                    if (FEnableIn[0] == true)
                        Update();
                }

                FInit = false;
            }


            Update();
        }



        private unsafe void ReadRGBImage()
        {

            while (true)
            {
                lock (this)
                {
                    if (FInit == false && FImageGenerator.IsDataNew)
                    {
                        //create a pointer where the information for the vvvv texture lies
                        byte* RGB32 = (byte*)FDataRGB.ToPointer();

                        //get the pointer to the Rgb Image
                        byte* RGB24 = (byte*)FImageGenerator.ImageMapPtr;

                        try
                        {
                            //write the pixels
                            for (int i = 0; i < FTexWidth * FTexHeight; i++, RGB24 += 3, RGB32 += 4)
                            {
                                RGB32[0] = RGB24[2];
                                RGB32[1] = RGB24[1];
                                RGB32[2] = RGB24[0];
                            }
                        }
                        catch (AccessViolationException)
                        { }
                    }
                }
            }
        }

      


        #region IPluginDXTexture Members

        //this method gets called, when Reinitialize() was called in evaluate,
        //or a graphics device asks for its data
        protected override Texture CreateTexture(int Slice, SlimDX.Direct3D9.Device device)
        {
            FLogger.Log(LogType.Debug, "Creating new texture at slice: " + Slice + " -> W: " + FTexWidth + "H: " + FTexHeight);
            Texture tex = new Texture(device, FTexWidth, FTexHeight, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);

            return tex;
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its texture, here you fill the texture with the actual data
        //this is called for each renderer, careful here with multiscreen setups, in that case
        //calculate the pixels in evaluate and just copy the data to the device texture here
        unsafe protected override void UpdateTexture(int Slice, Texture texture)
        {

            FCurrentSlice = Slice;

            if (FDataRGB != null)
            {
                if (FDataRGB.ToInt32() != 0 && FInit == false)
                {
                    //lock the vvv texture
                    var rect = texture.LockRectangle(0, LockFlags.Discard).Data;

                    // tell the vvvv texture where the pointer to the depth image lies
                    rect.WriteRange(FDataRGB, FTexHeight * FTexWidth * 4);
                    texture.UnlockRectangle(0);
                }
            }
        }

        #endregion IPluginDXResource Members
    }
}
