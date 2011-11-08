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
	public enum DepthMode
	{
		Histogram,
		Raw,
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Depth",
	            Category = "EX9.Texture",
	            Version ="Kinect Pure",
	            Help = "Returns a 16bit depthmap in two flavors: histogram or depth in mm (where only the first 13bit are being used).",
	            Tags = "OpenNI",
	            Author = "joreg")]
	#endregion PluginInfo
	
	public class Texture_Depth: DXTextureOutPluginBase, IPluginEvaluate, IDisposable
	{
		//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
		static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
		
		//[DllImport("msvcrt.dll", EntryPoint="memcpy", SetLastError=false)]
		//static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
		
		#region fields & pins
		[Input("Context", IsSingle = true)]
		ISpread<Context> FContext;
		
		[Input("Depth Mode")]
		IDiffSpread<DepthMode> FDepthMode;

		[Input("Enable", IsSingle = true, DefaultValue = 1)]
		ISpread<bool> FEnableIn;
		
		[Output("FOV", Order = int.MaxValue)]
		ISpread<Vector2D> FFov;

		[Import()]
		ILogger FLogger;

		private int[] FHistogram;
		private int FTexWidth;
		private int FTexHeight;
		private DepthGenerator FDepthGenerator;
		private bool FInit = true;

		private bool disposed = false;
		IPluginHost FHost;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public Texture_Depth(IPluginHost host)
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
						FFov[0] = new Vector2D(FDepthGenerator.FieldOfView.HorizontalAngle, FDepthGenerator.FieldOfView.VerticalAngle);
						FHistogram = new int[FDepthGenerator.DeviceMaxDepth];
						
						//Set the resolution of the texture
						MapOutputMode MapMode = FDepthGenerator.MapOutputMode;
						FTexWidth = MapMode.XRes;
						FTexHeight = MapMode.YRes;

						FDepthGenerator.StartGenerating();
						
						//Reinitalie the vvvv texture
						Reinitialize();
						
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

		~Texture_Depth()
		{
			Dispose(false);
		}

		#endregion

		#region Helper
		private unsafe void CalculateHistogram(DepthMetaData DepthMD)
		{
			//initialize all slots to 0
			for (int i = 0; i < FHistogram.Length; ++i)
				FHistogram[i] = 0;

			try
			{
				ushort* pDepth = (ushort*)DepthMD.DepthMapPtr;

				int points = 0;
				for (int y = 0; y < DepthMD.YRes; y++)
					for (int x = 0; x < DepthMD.XRes; x++, pDepth++)
					{
						ushort depthVal = *pDepth;
						if (depthVal != 0)
						{
							FHistogram[depthVal]++;
							points++;
						}
					}

				for (int i = 1; i < FHistogram.Length; i++)
					FHistogram[i] += FHistogram[i - 1];

				if (points > 0)
					for (int i = 1; i < FHistogram.Length; i++)
						FHistogram[i] = (ushort)(ushort.MaxValue * (1.0f - (FHistogram[i] / (float)points)));
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
		#endregion Helper
		
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
			//lock the vvvv texture
			var rect = texture.LockRectangle(0, LockFlags.Discard).Data;
			
			if (FDepthMode[0] == DepthMode.Raw)
				CopyMemory(rect.DataPointer, FDepthGenerator.DepthMapPtr, FTexHeight * FTexWidth * 2);
			else
			{
				DepthMetaData DepthMD = FDepthGenerator.GetMetaData();
				CalculateHistogram(DepthMD);
				
				ushort* pSrc = (ushort*)FDepthGenerator.DepthMapPtr;
				ushort* pDest = (ushort*)rect.DataPointer;

				// write the Depth pointer to Destination pointer
				for (int y = 0; y < FTexHeight; y++)
				{
					for (int x = 0; x < FTexWidth; x++, pSrc++, pDest++)
						pDest[0] = (ushort)FHistogram[*pSrc];
				}
			}
			
			//unlock the vvvv texture
			texture.UnlockRectangle(0);
		}

		#endregion IPluginDXResource Members
	}
}
