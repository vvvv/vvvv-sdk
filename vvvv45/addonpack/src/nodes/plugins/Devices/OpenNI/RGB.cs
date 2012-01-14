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
	enum OutputMode {Texture, SharedMemory, Both};
	
	#region PluginInfo
	[PluginInfo(Name = "RGB",
	            Category = "Kinect",
	            Version = "OpenNI",
	            Help = "Returns an X8R8G8B8 formatted texture from the kinects RGB camera",
	            Tags = "ex9, texture",
	            Author = "Phlegma, joreg")]
	#endregion PluginInfo
	public class Texture_Image: DXTextureOutPluginBase, IPluginEvaluate, IPluginConnections, IDisposable
	{
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		ISpread<bool> FEnabledIn;
		
		[Import()]
		ILogger FLogger;

		private ImageGenerator FImageGenerator;
		private ImageMetaData FImageMetaData;

		private int FTexWidth;
		private int FTexHeight;

		private bool FContextChanged = false;
		#endregion fields & pins
		
		// import host and hand it to base constructor
		[ImportingConstructor()]
		public Texture_Image(IPluginHost host): base(host)
		{}
		
		#region Evaluate
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FContextChanged)
			{
				if (FContextIn.PluginIO.IsConnected)
				{
					if (FContextIn[0] != null)
					{
						try
						{
							FImageGenerator = (ImageGenerator) FContextIn[0].GetProductionNodeByName("Image1"); //new ImageGenerator(FContextIn[0]);
							FImageMetaData = FImageGenerator.GetMetaData();
							
							FTexWidth = FImageMetaData.XRes;
							FTexHeight = FImageMetaData.YRes;
							
							//Reinitalie the vvvv texture
							Reinitialize();
							
							FContextChanged = false;
						}
						catch (Exception ex)
						{
							FLogger.Log(ex);
						}
					}
				}
				else
				{
					CleanUp();
					FContextChanged = false;
				}
			}
			
			if (FImageGenerator != null && FEnabledIn[0])
				Update();
		}

		void FImageGenerator_NewDataAvailable(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Dispose
		public void Dispose()
		{
			CleanUp();
		}
		
		private void CleanUp()
		{
			FImageGenerator = null;
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
		
		#region ContextConnect
		public void ConnectPin(IPluginIO pin)
		{
			if (pin == FContextIn.PluginIO)
				FContextChanged = true;
		}

		public void DisconnectPin(IPluginIO pin)
		{
			if (pin == FContextIn.PluginIO)
				FContextChanged = true;
		}
		#endregion
	}
}
