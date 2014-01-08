#region usings

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Drawing;
using System.Drawing.Imaging;

#endregion usings

namespace VVVV.Nodes
{
	public enum UserTexturetMode
	{
		Viewable,
		Raw,
	}
	
	#region PluginInfo
	[PluginInfo(Name = "User",
	            Category = "Kinect",
	            Version = "OpenNI",
	            Help = "Returns a 16bit texture per recognized user with pixels activated for areas occupied by the user plus center of mass and ID per user",
	            Tags = "EX9, texture, tracking, person",
	            Author = "Phlegma, joreg")]
	#endregion PluginInfo
	public class Users: DXTextureOutPluginBase, IPluginEvaluate, IPluginConnections, IDisposable
	{
		//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
		static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
		
		#region fields & pins
		[Input("Context", IsSingle=true)]
		Pin<Context> FContextIn;
		
		[Input("Output Mode", IsSingle = true)]
		IDiffSpread<UserTexturetMode> FOutputMode;
		
		[Input("Viewable User Color", DefaultColor = new double[] {0, 0, 1, 1})]
		ISpread<RGBAColor> FUserColor;
		
		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		IDiffSpread<bool> FEnabledIn;

		[Output("User ID", Order = int.MaxValue-1)]
		ISpread<int> FUserIdOut;
		
		[Output("Position", Order = int.MaxValue)]
		ISpread<Vector3D> FPositionOut;

		[Import()]
		ILogger FLogger;

		private int FTexWidth;
		private int FTexHeight;
		UserGenerator FUserGenerator;
		private bool FContextChanged = false;
		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public Users(IPluginHost host)
			: base(host)
		{}
		
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
							// Creates the User Generator from the Context Object
							FUserGenerator = new UserGenerator(FContextIn[0]);
							
							//Set the resolution of the texture
							FTexWidth = FUserGenerator.GetUserPixels(0).FullXRes;
							FTexHeight = FUserGenerator.GetUserPixels(0).FullYRes;

							//Reinitalie the vvvv texture
							Reinitialize();
							
							FUserGenerator.StartGenerating();
							
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
			
			//create new texture if outputmode changed
			if (FOutputMode.IsChanged)
				Reinitialize();
			
			if (FUserGenerator != null)
			{
				if (FEnabledIn.IsChanged)
					if (FEnabledIn[0])
						FUserGenerator.StartGenerating();
					else
						FUserGenerator.StopGenerating();
				
				if (FUserGenerator.IsDataNew)
				{
					FUserIdOut.SliceCount = FUserGenerator.NumberOfUsers;
					FPositionOut.SliceCount = FUserGenerator.NumberOfUsers;
					FTextureOut.SliceCount = 1;
						
					if (FUserGenerator.NumberOfUsers > 0)
					{
						//copies a list of all users and sort them
						int[] tUsers = FUserGenerator.GetUsers();
						int[] Users = (int[])tUsers.Clone();
						Array.Sort(Users);
						
						for (int i = 0; i < Users.Length; i++)
						{
							FUserIdOut[i] = Users[i];
							try
							{
								//middle point of the User
								Point3D Point = FUserGenerator.GetCoM(Users[i]);
								Vector3D Position = new Vector3D(Point.X, Point.Y, Point.Z);
								
								//map postion values to vvvv coordinates
								FPositionOut[i] = Position / 1000;
							}
							catch (StatusException ex)
							{
								FLogger.Log(ex);
							}
						}
					}

					//update the vvvv texture
					Update();
				}
			}
			else
			{
				FUserIdOut.SliceCount = 0;
				FPositionOut.SliceCount = 0;
				FTextureOut.SliceCount = 0;
			}
		}
		
		#region Dispose
		public void Dispose()
		{
			CleanUp();
		}

		private void CleanUp()
		{
			FUserGenerator = null;
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
			
			if (FOutputMode[0] == UserTexturetMode.Raw)
				return new Texture(device, FTexWidth, FTexHeight, 1, usage, Format.L16, pool);
			else
				return new Texture(device, FTexWidth, FTexHeight, 1, usage, Format.A8R8G8B8, pool);
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its texture, here you fill the texture with the actual data
		//this is called for each renderer, careful here with multiscreen setups, in that case
		//calculate the pixels in evaluate and just copy the data to the device texture here
		unsafe protected override void UpdateTexture(int Slice, Texture texture)
		{
			DataRectangle rect;
			if (texture.Device is DeviceEx)
				rect = texture.LockRectangle(0, LockFlags.None);
			else
				rect = texture.LockRectangle(0, LockFlags.Discard);
			
			try
			{
				if (FOutputMode[0] == UserTexturetMode.Raw)
					//copy full lines
					for (int i = 0; i < FTexHeight; i++) 
						CopyMemory(rect.Data.DataPointer.Move(rect.Pitch * i), FUserGenerator.GetUserPixels(0).LabelMapPtr.Move(FTexWidth * i * 2), FTexWidth * 2);
				else
				{
					ushort* pSrc = (ushort*)FUserGenerator.GetUserPixels(0).LabelMapPtr;
					byte* pDest = (byte*)rect.Data.DataPointer;
	
					// write the Depth pointer to Destination pointer
					for (int y = 0; y < FTexHeight; y++)
					{
						var off = 0;
						for (int x = 0; x < FTexWidth; x++, pSrc++, pDest+=4)
						{
							var color = VColor.Black;
							
							if (*pSrc == 0)
								color.A = 0;
							else
								color = FUserColor[*pSrc - 1];
	
							pDest[0] = (byte) (color.B * 255);
							pDest[1] = (byte) (color.G * 255);
							pDest[2] = (byte) (color.R * 255);
							pDest[3] = (byte) (color.A * 255);
							
							off += 4;
						}
						
						//advance dest by rest of pitch
						pDest += rect.Pitch - off;
					}
				}
			}
			finally
			{
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
