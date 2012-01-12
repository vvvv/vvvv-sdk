#region usings

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.EX9;

using OpenNI;
using SlimDX.Direct3D9;
using System.Drawing;
using System.Drawing.Imaging;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "User",
	            Category = "Kinect",
	            Version = "OpenNI",
	            Help = "Returns a 16bit texture per recognized user with pixels activated for areas occupied by the user plus center of mass and ID per user",
	            Tags = "ex9, texture, tracking, person, people",
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
		
		[Input("Enabled", IsSingle = true, DefaultValue = 1)]
		ISpread<bool> FEnabledIn;

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
							
							//start generating data
							FUserGenerator.StartGenerating();
							
							FContextChanged = false;
						}
						catch (StatusException ex)
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
			
			if (FUserGenerator != null && FEnabledIn[0])
			{
				if (FUserGenerator.NumberOfUsers > 0)
				{
					//copies a list of all users and sort them
					int[] tUsers = FUserGenerator.GetUsers();
					int[] Users = (int[])tUsers.Clone();
					Array.Sort(Users);
					
					FUserIdOut.SliceCount = Users.Length;
					FPositionOut.SliceCount = Users.Length;
					FTextureOut.SliceCount = Users.Length;
					
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
					
					//update the vvvv texture
					Update();
				}
			}
			else
			{
				FUserIdOut.SliceCount = 0;
				FPositionOut.SliceCount = 0;
			}
		}
		
		#region Dispose
		public void Dispose()
		{
			CleanUp();
		}

		private void CleanUp()
		{
			if (FUserGenerator != null)
			{
				FUserGenerator.Dispose();
				FUserGenerator = null;
			}
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
			CopyMemory(rect.DataPointer, FUserGenerator.GetUserPixels(Slice).LabelMapPtr, FTexHeight * FTexWidth * 2);
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
