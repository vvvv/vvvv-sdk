#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.SharedMemory;

using OpenNI;
using SlimDX.Direct3D9;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "RGB",
                Category = "Kinect",
                Version = "OpenNI",
                Help = "Returns an X8R8G8B8 formatted texture from the kinects RGB camera",
                Tags = "ex9, texture",
                Author = "Phlegma, joreg")]
    #endregion PluginInfo
    public class Texture_Image: DXTextureOutPluginBase, IPluginEvaluate, IDisposable
    {
        #region fields & pins
        [Input("Context", IsSingle = true)]
        ISpread<Context> FContextIn;

        [Input("Enabled", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FEnabledIn;
        
        [Input("Shared Name", IsSingle = true, DefaultString = "#vvvv")]
        IDiffSpread<string> FSharedNamePin;        

        [Import()]
        ILogger FLogger;

        private ImageGenerator FImageGenerator;
        private ImageMetaData FImageMetaData;

        private int FTexWidth;
        private int FTexHeight;

        private bool FInit = true;
        private Segment FSegment;
        private IntPtr FImage;
        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
        public Texture_Image(IPluginHost host): base(host)
        {
        }
        
        ~Texture_Image()
        {
        	if (FSegment != null)
				FSegment.Dispose();
        }

        #region Evaluate

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FContextIn[0] != null)
            {
                if (FInit == true)
                {
                    //Create an Image Generator
                    try
                    {
                        FImageGenerator = new ImageGenerator(FContextIn[0]);
                        FImageMetaData = FImageGenerator.GetMetaData();

                        FTexWidth = FImageMetaData.XRes;
                        FTexHeight = FImageMetaData.YRes;
                        
                        //Reinitalie the vvvv texture
                        Reinitialize();

                        FInit = false;
                        
                       // if (FSharedNamePin.IsChanged)
						{
							if (FSegment != null)
								FSegment.Dispose();
			
							int byteCount = FTexWidth*FTexHeight*3;
							FSegment = new Segment(FSharedNamePin[0], SharedMemoryCreationFlag.Create, byteCount);
							FImage = System.Runtime.InteropServices.Marshal.AllocHGlobal(byteCount);
						}
                    }
                    catch (StatusException ex)
                    {
                        FLogger.Log(ex);
                    }
                }
            }
            else
            {
                FInit = true;
            }

            if (FEnabledIn[0])
                Update();
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
        	if (FImageGenerator != null)
        		FImageGenerator.Dispose();
        }

        #endregion 

        #region IPluginDXTexture Members

        //this method gets called, when Reinitialize() was called in evaluate,
        //or a graphics device asks for its data
        protected override Texture CreateTexture(int Slice, SlimDX.Direct3D9.Device device)
        {
            return new Texture(device, FTexWidth, FTexHeight, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its texture, here you fill the texture with the actual data
        //this is called for each renderer, careful here with multiscreen setups, in that case
        //calculate the pixels in evaluate and just copy the data to the device texture here
        unsafe protected override void UpdateTexture(int Slice, Texture texture)
        {
        	//get the pointer to the Rgb Image
            byte* src24 = (byte*)FImageGenerator.ImageMapPtr;

            //lock the vvvv texture
            byte* dest32 = (byte*)texture.LockRectangle(0, LockFlags.Discard).Data.DataPointer;
            
            if (FSegment != null)
            {
            	byte* share24 = (byte*) FImage;
        		
	            //write the pixels
	            for (int i = 0; i < FTexWidth * FTexHeight; i++, src24 += 3, dest32 += 4, share24 += 3)
	            {
	                dest32[0] = src24[2];
	                dest32[1] = src24[1];
	                dest32[2] = src24[0];
	                dest32[3] = 255;
	                
	                share24[0] = src24[2];
	                share24[1] = src24[1];
	                share24[2] = src24[0];
	            }
	            
  				FSegment.Lock();
  				FSegment.CopyByteArrayToSharedMemory(FImage, FTexWidth * FTexHeight * 3);
				FSegment.Unlock();

            }
            else //write the pixels         
	            for (int i = 0; i < FTexWidth * FTexHeight; i++, src24 += 3, dest32 += 4)
	            {
	                dest32[0] = src24[2];
	                dest32[1] = src24[1];
	                dest32[2] = src24[0];
	                dest32[3] = 255;
	            }
            
            texture.UnlockRectangle(0);
        }

        #endregion IPluginDXResource Members
    }
}
