#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.EX9;

using OpenNI;
using SlimDX;
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
	            Category = "Kinect",
	            Version ="OpenNI",
	            Help = "Returns a 16bit depthmap in two flavors: histogram or depth in mm (where only the first 13bit are being used).",
	            Tags = "EX9, texture",
	            Author = "Phlegma, joreg",
	            AutoEvaluate = true)]
	#endregion PluginInfo
	public class Texture_Depth: DXTextureOutPluginBase, IPluginEvaluate, IPluginConnections, IDisposable
	{
		//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
		static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
		
		//[DllImport("msvcrt.dll", EntryPoint="memcpy", SetLastError=false)]
		//static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
		
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Depth Mode", IsSingle = true)]
		IDiffSpread<DepthMode> FDepthMode;
		
		[Input("Adapt to RGB View", IsSingle = true, DefaultValue = 0)]
		IDiffSpread<bool> FAdaptView;

		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		IDiffSpread<bool> FEnabledIn;
		
		[Output("FOV", Order = int.MaxValue)]
		ISpread<Vector2D> FFov;

		[Import()]
		ILogger FLogger;

		private int[] FHistogram;
		private DepthGenerator FDepthGenerator;
		private ImageGenerator FImageGenerator;
		
		private int FTexWidth;
		private int FTexHeight;
		
		private bool FContextChanged = false;
		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public Texture_Depth(IPluginHost host)
			: base(host)
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
							FDepthGenerator = (DepthGenerator) FContextIn[0].GetProductionNodeByName("Depth1");
							FFov[0] = new Vector2D(FDepthGenerator.FieldOfView.HorizontalAngle, FDepthGenerator.FieldOfView.VerticalAngle);
							
							FHistogram = new int[FDepthGenerator.DeviceMaxDepth];
							
							//Set the resolution of the texture
							var mapMode = FDepthGenerator.MapOutputMode;
							FTexWidth = mapMode.XRes;
							FTexHeight = mapMode.YRes;
							
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
			
			if (FDepthGenerator != null)
			{
				if (FEnabledIn.IsChanged)
					if (FEnabledIn[0])
						FDepthGenerator.StartGenerating();
					else
						FDepthGenerator.StopGenerating();
				
				bool imageGeneratorChanged = false;
				try
				{
					var imageGenerator =  (ImageGenerator) FContextIn[0].GetProductionNodeByName("Image1");
					if (FImageGenerator != imageGenerator)
					{
						FImageGenerator = imageGenerator;
						imageGeneratorChanged = true;
					}
				}
				catch
				{}
				
				if (FAdaptView.IsChanged || imageGeneratorChanged)
				{
					if (FImageGenerator == null || !FAdaptView[0])
						FDepthGenerator.AlternativeViewpointCapability.ResetViewpoint();
					else
						FDepthGenerator.AlternativeViewpointCapability.SetViewpoint(FImageGenerator);
				}
				
				if (FDepthGenerator.IsDataNew)
						Update();
			}
		}
		#endregion
		
		#region Dispose
		public void Dispose()
		{
			CleanUp();
		}
		
		private void CleanUp()
		{
			FDepthGenerator = null;
			FImageGenerator = null;
		}
		#endregion

		#region Helper
		private unsafe void CalculateHistogram(DepthMetaData DepthMD)
		{
			//initialize all slots to 0
			for (int i = 0; i < FHistogram.Length; ++i)
				FHistogram[i] = 0;

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
		#endregion Helper
		
		#region IPluginDXTexture Members
		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Texture CreateTexture(int Slice, SlimDX.Direct3D9.Device device)
		{
			var pool = Pool.Managed;
			var usage = Usage.None;
			if (device is DeviceEx)
			{
				pool = Pool.Default;
				usage = Usage.Dynamic;
			}
			
			return new Texture(device, FTexWidth, FTexHeight, 1, usage, Format.L16, pool);
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe protected override void UpdateTexture(int Slice, Texture texture)
		{
			//lock the vvvv texture
			DataRectangle rect;
			if (texture.Device is DeviceEx)
				rect = texture.LockRectangle(0, LockFlags.None);
			else
				rect = texture.LockRectangle(0, LockFlags.Discard);
			
			try 
			{
				if (FDepthMode[0] == DepthMode.Raw)
					//copy full lines
					for (int i = 0; i < FTexHeight; i++) 
						CopyMemory(rect.Data.DataPointer.Move(rect.Pitch * i), FDepthGenerator.DepthMapPtr.Move(FTexWidth * i * 2), FTexWidth * 2);
				else
				{
					DepthMetaData DepthMD = FDepthGenerator.GetMetaData();
					CalculateHistogram(DepthMD);
	
					ushort* pSrc = (ushort*)FDepthGenerator.DepthMapPtr;
					ushort* pDest = (ushort*)rect.Data.DataPointer;
	
					// write the Depth pointer to Destination pointer
					for (int y = 0; y < FTexHeight; y++)
					{
						var off = 0;
						
						for (int x = 0; x < FTexWidth; x++, pSrc++, pDest++)
						{
							pDest[0] = (ushort)FHistogram[*pSrc];
							off += 2;
						}
						
						//advance dest by rest of pitch
						pDest += (rect.Pitch - off) / 2;
					}
				}
			}
			finally
			{
				//unlock the vvvv texture
				texture.UnlockRectangle(0);
			}
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
