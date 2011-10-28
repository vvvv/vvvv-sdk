//if this line is uncomment the debug information will be written to the output view an will be include to the compiling
//#define Debug

#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Diagnostics;
using System.IO;
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


#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Depth",
                Category = "EX9.Texture",
                Version ="Kinect Pure",
                Help = "Returns a 16bit depthmap of which only the first 13bit are being used to specify depth in mm.",
                Tags = "OpenNI",
                Author = "joreg")]
    #endregion PluginInfo
   
    public class Texture_DepthPure : DXTextureOutPluginBase, IPluginEvaluate, IDisposable
    {
    	//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
    	static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
        
		//[DllImport("msvcrt.dll", EntryPoint="memcpy", SetLastError=false)]
		//static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
    	
        #region fields & pins
        [Input("Context", IsSingle = true)]
        ISpread<Context> FContext;

        [Input("Enable", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FEnableIn;
        
        [Output("FOV")]
        ISpread<Vector2D> FFov;

        [Import()]
        ILogger FLogger;

        private int FTexWidth;
        private int FTexHeight;
        private DepthGenerator FDepthGenerator; 
        private bool FInit = true;

        private bool disposed = false;
        IPluginHost FHost;

        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
        public Texture_DepthPure(IPluginHost host)
            : base(host)
        {
            FHost = host;
        }

        #region Evaluate

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FContext[0] != null)
            {
                if (FInit == true)
                {
                    if (FDepthGenerator == null)
                    {
                        FDepthGenerator = new DepthGenerator(FContext[0]);

                        MapOutputMode MapMode = FDepthGenerator.MapOutputMode;
                        MapMetaData MetaData = FDepthGenerator.GetMetaData();
                        FFov[0] = new Vector2D(FDepthGenerator.FieldOfView.HorizontalAngle, FDepthGenerator.FieldOfView.VerticalAngle);

                        //Set the resolution of the texture
                        FTexWidth = MapMode.XRes;
                        FTexHeight = MapMode.YRes;

                        //Reinitalie the vvvv texture
                        Reinitialize();

                        FDepthGenerator.StartGenerating();
                        FInit = false;
                    }
                }
                else
                {
                    //update the vvvv texture
                    if (FEnableIn[0])
                        Update();
                }
            }
            else
            {
                if (FDepthGenerator != null)
                {
                    //FDepthGenerator.StopGenerating();
                    FDepthGenerator = null;
                    FInit = true;
                }
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.

                disposed = true;
            }
        }

        ~Texture_DepthPure()
        {
            Dispose(false);
        }

        #endregion

        #region IPluginDXTexture Members

        //this method gets called, when Reinitialize() was called in evaluate,
        //or a graphics device asks for its data
        protected override Texture CreateTexture(int Slice, SlimDX.Direct3D9.Device device)
        {
            return new Texture(device, FTexWidth, FTexHeight, 1, Usage.None, Format.L16, Pool.Managed);
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its texture, here you fill the texture with the actual data
        //this is called for each renderer, careful here with multiscreen setups, in that case
        //calculate the pixels in evaluate and just copy the data to the device texture here
        unsafe protected override void UpdateTexture(int Slice, Texture texture)
        {
	        var rect = texture.LockRectangle(0, LockFlags.Discard).Data;
			CopyMemory(rect.DataPointer, FDepthGenerator.DepthMapPtr, FTexHeight * FTexWidth * 2);
			texture.UnlockRectangle(0);
        }

        #endregion IPluginDXResource Members
    }
}
