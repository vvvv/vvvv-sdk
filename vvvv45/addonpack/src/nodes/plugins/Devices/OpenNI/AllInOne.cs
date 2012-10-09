#region usings
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

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
	/*
	[PluginInfo(Name = "AllInOne",
	            Category = "Kinect",
	            Version ="OpenNI",
	            Help = "Depth + Handtracker",
	            Tags = "ex9, texture")]
	*/
	#endregion PluginInfo
	public class AllInOne: DXTextureOutPluginBase, IPluginEvaluate, IDisposable
	{
		//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
		static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
		
		//[DllImport("msvcrt.dll", EntryPoint="memcpy", SetLastError=false)]
		//static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
		
		#region fields & pins
		[Input("Mirrored", IsSingle = true, DefaultValue = 1)]
		IDiffSpread<bool> FMirrored;
		
		[Input("Depth Mode")]
		IDiffSpread<DepthMode> FDepthMode;

		[Input("Start Position")]
		ISpread<Vector3D> FStartPositionIn;
		
		[Input("Track")]
		ISpread<bool> FDoTrackStartPosition;
		
		[Output("Position")]
		ISpread<Vector3D> FHandPositionOut;
		
		[Output("Start Position Is Tracked")]
		ISpread<bool> FIsTrackedOut;
		
		[Output("ID")]
		ISpread<int> FHandIdOut;
		
		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		ISpread<bool> FEnabledIn;
		
		[Output("FOV", Order = int.MaxValue)]
		ISpread<Vector2D> FFov;

		[Import()]
		ILogger FLogger;

		private int[] FHistogram;
		private DepthGenerator FDepthGenerator;
		private ImageGenerator FImageGenerator;
		
		private int FTexWidth;
		private int FTexHeight;
		
		private Context FContext;
		private IntPtr FBufferedImage = new IntPtr();
		//private IntPtr FBufferedDepth = new IntPtr();
		private Thread FUpdater;
		private bool FRunning = false;
		
		HandsGenerator FHandGenerator;

		private bool FContextChanged = false;
		private readonly object FHandTrackerLock = new object();
		private readonly object FBufferedImageLock = new object();
		private Dictionary<int, Vector3D> FTrackedHands = new Dictionary<int, Vector3D>();
		private Dictionary<int, Vector3D> FTrackedStartPositions = new Dictionary<int, Vector3D>();
		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public AllInOne(IPluginHost host)
			: base(host)
		{
			OpenContext();
		}

		#region Evaluate
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FContext != null)
			{
				if (FDepthGenerator != null && FEnabledIn[0])
				{
					FFov[0] = new Vector2D(FDepthGenerator.FieldOfView.HorizontalAngle, FDepthGenerator.FieldOfView.VerticalAngle);
					Update();
				}
				
				if (FHandGenerator != null && FEnabledIn[0])
				{
					FIsTrackedOut.SliceCount = FHandIdOut.SliceCount = FHandPositionOut.SliceCount = FStartPositionIn.SliceCount;
					//for every given StartPosition check if it is currently tracked
					lock(FHandTrackerLock)
						for (int i = 0; i < FStartPositionIn.SliceCount; i++)
					{
						if (FDoTrackStartPosition[i])
						{
							//find userID in FTrackedStartPositions
							int userID = -1;
							foreach (var tracker in FTrackedStartPositions)
								if (tracker.Value == FStartPositionIn[i])
							{
								userID = tracker.Key;
								break;
							}
							
							//if present return tracking info
							if (userID > -1)
							{
								FIsTrackedOut[i] = true;
								FHandIdOut[i] = userID;
								FHandPositionOut[i] = FTrackedHands[userID];
							}
							//else start tracking
							else
							{
								FIsTrackedOut[i] = false;
								FHandIdOut[i] = -1;
								FHandPositionOut[i] = FStartPositionIn[i];
								
								var p = new Point3D((float)(FStartPositionIn[i].x * 1000), (float)(FStartPositionIn[i].y * 1000), (float)(FStartPositionIn[i].z * 1000));
								FHandGenerator.StartTracking(p);
							}
						}
						else
						{
							//find the userID corresponding to the StartPosition
							//and stop tracking it
							int userID = -1;
							foreach (var tracker in FTrackedStartPositions)
								if (tracker.Value == FStartPositionIn[i])
							{
								userID = tracker.Key;
								break;
							}
							
							if (userID > -1)
								FHandGenerator.StopTracking(userID);
							
							FIsTrackedOut[i] = false;
							FHandIdOut[i] = -1;
							FHandPositionOut[i] = FStartPositionIn[i];
						}
					}
				}
				else
				{
					FIsTrackedOut.SliceCount = FHandIdOut.SliceCount = FHandPositionOut.SliceCount = 0;
				}
			}
		}
		#endregion
		
		#region HandEvents
		void FHands_HandUpdate(object sender, HandUpdateEventArgs e)
		{
			//if this hand is updated for the first time
			//add it to TrckedStartPositions
			//with the original position of this hand which is found in FTrackedHands[e.UserID]
			//before this is updated!
			lock(FHandTrackerLock)
			{
				if (FTrackedHands.ContainsKey(e.UserID) && !FTrackedStartPositions.ContainsKey(e.UserID))
					FTrackedStartPositions.Add(e.UserID, FTrackedHands[e.UserID]);
				
				if (FTrackedHands.ContainsKey(e.UserID))
					FTrackedHands[e.UserID] = new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000);
			}
		}

		void FHands_HandDestroy(object sender, HandDestroyEventArgs e)
		{
			lock(FHandTrackerLock)
			{
				if (FTrackedHands.ContainsKey(e.UserID))
					FTrackedHands.Remove(e.UserID);
				
				if (FTrackedStartPositions.ContainsKey(e.UserID))
					FTrackedStartPositions.Remove(e.UserID);
			}
		}

		void FHands_HandCreate(object sender, HandCreateEventArgs e)
		{
			var v = new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000);
			lock(FHandTrackerLock)
				if (!FTrackedHands.ContainsValue(v))
					FTrackedHands.Add(e.UserID, v);
		}
		#endregion

		#region Open and close Context
		private void OpenContext()
		{
			//try to open Kinect Context
			try
			{
				FContext = new Context();
				FContext.ErrorStateChanged += FContext_ErrorStateChanged;
				
//				FImageGenerator = (ImageGenerator) FContext.CreateAnyProductionTree(OpenNI.NodeType.Image, null);
				FDepthGenerator = (DepthGenerator) FContext.CreateAnyProductionTree(OpenNI.NodeType.Depth, null);
//				FDepthGenerator.AlternativeViewpointCapability.SetViewpoint(FImageGenerator);
				
				// Create and Hands generator
				FHandGenerator = new HandsGenerator(FContext);
				FHandGenerator.HandCreate += FHands_HandCreate;
				FHandGenerator.HandDestroy += FHands_HandDestroy;
				FHandGenerator.HandUpdate += FHands_HandUpdate;

				FHistogram = new int[FDepthGenerator.DeviceMaxDepth];
				
				//Set the resolution of the texture
				var mapMode = FDepthGenerator.MapOutputMode;
				FTexWidth = mapMode.XRes;
				FTexHeight = mapMode.YRes;
				
				//allocate data for the Depth Image
				FBufferedImage = Marshal.AllocCoTaskMem(FTexWidth * FTexHeight * 2);
				
				//Reinitalie the vvvv texture
				Reinitialize();
				
				FContext.StartGeneratingAll();
				
				FUpdater = new Thread(ReadImageData);
				FRunning = true;
				FUpdater.Start();
			}
			catch (Exception e)
			{
				FLogger.Log(e);
			}
		}

		private void CloseContext()
		{
			if (FUpdater != null && FUpdater.IsAlive)
			{
				//wait for threadloop to exit
				FRunning = false;
				FUpdater.Join();
			}

			if (FContext != null)
			{
				//FContext.WaitAndUpdateAll();
				FContext.StopGeneratingAll();
				FContext.ErrorStateChanged -= FContext_ErrorStateChanged;
				
				if (FHandGenerator != null)
				{
					FHandGenerator.HandCreate -= FHands_HandCreate;
					FHandGenerator.HandDestroy -= FHands_HandDestroy;
					FHandGenerator.HandUpdate -= FHands_HandUpdate;
					
					FHandGenerator.Dispose();
					FHandGenerator = null;
				}
				
				if (FDepthGenerator != null)
					FDepthGenerator.Dispose();
				
				if (FImageGenerator != null)
					FImageGenerator.Dispose();
				
				FTrackedStartPositions.Clear();
				FTrackedHands.Clear();
				
				FContext.Release();
				FContext = null;
			}
			
			Marshal.FreeCoTaskMem(FBufferedImage);
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

		#region UpdateThread
		private unsafe void ReadImageData()
		{
			var depthMD = new DepthMetaData();
			
			while (FRunning)
			{
				try
				{
					FContext.GlobalMirror = FMirrored[0];
					FContext.WaitOneUpdateAll(FDepthGenerator);
				}
				catch (Exception)
				{}

				if (FDepthMode[0] == DepthMode.Histogram)
				{
					FDepthGenerator.GetMetaData(depthMD);
					CalculateHistogram(depthMD);
				}
				
				lock(FBufferedImageLock)
				{
					if (FDepthGenerator.IsDataNew)
					{
						try
						{
							if (FDepthMode[0] == DepthMode.Raw)
								CopyMemory(FBufferedImage, FDepthGenerator.DepthMapPtr, FTexHeight * FTexWidth * 2);
							else
							{
								ushort* pSrc = (ushort*)FDepthGenerator.DepthMapPtr.ToPointer();
								ushort* pDest = (ushort*)FBufferedImage.ToPointer();

								//write the Depth pointer to Destination pointer
								for (int y = 0; y < FTexHeight; y++)
								{
									for (int x = 0; x < FTexWidth; x++, pSrc++, pDest++)
										*pDest = (ushort)FHistogram[*pSrc];
								}
							}
						}
						catch (Exception)
						{ }
					}
				}
			}
		}
		#endregion

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
			var rect = texture.LockRectangle(0, LockFlags.Discard).Data;
			
			lock(FBufferedImageLock)
				//write the image buffer data to the texture
				rect.WriteRange(FBufferedImage, FTexHeight * FTexWidth * 2);

			texture.UnlockRectangle(0);
		}
		#endregion IPluginDXResource Members

		#region Error Event
		void FContext_ErrorStateChanged(object sender, ErrorStateEventArgs e)
		{
			FLogger.Log(LogType.Error, "Global Kinect Error: " + e.CurrentError);
		}
		#endregion

		#region Dispose
		public void Dispose()
		{
			CloseContext();
		}
		#endregion
	}
}
