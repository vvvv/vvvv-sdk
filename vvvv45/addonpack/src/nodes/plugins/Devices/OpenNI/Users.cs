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
    [PluginInfo(Name = "Users",
                Category = "Kinect",
                Version = "OpenNI",
                Help = "Person recognition from the Kinect",
                Tags = "Kinect, OpenNI, Person",
                Author = "Phlegma, joreg")]
    #endregion PluginInfo


    public class Users: DXTextureOutPluginBase, IPluginEvaluate
    {
    	//memcopy method
		[DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]
    	static extern void CopyMemory(IntPtr dest, IntPtr src, int size);
    	
        #region fields & pins
        [Input("Context", IsSingle=true)]
        ISpread<Context> FContextIn;

        [Input("Enable", IsSingle = true, DefaultValue = 1)]
        ISpread<bool> FEnableIn;

        [Output("Users", IsSingle = true)]
        ISpread<UserGenerator> FUserOut;

        [Output("Position")]
        ISpread<Vector3D> FPositionOut;

        [Output("User ID")]
        ISpread<int> FUserIdOut;

        [Import()]
        ILogger FLogger;

        private int FTexWidth;
        private int FTexHeight;
        UserGenerator FUserGenerator;
        IPluginHost FHost;
        private bool FInit = true;

        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
        public Users(IPluginHost host)
            : base(host)
        {
            FHost = host;
        }
        
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FContextIn[0] != null)
            {
                if (FInit == true)
                {

                    // Create Gesture and Hands generator
                    try
                    {
                        // Creates the User Generator from the Context Object
                        FUserGenerator = new UserGenerator(FContextIn[0]);
                        // add the Callback function to the Events
                        FUserGenerator.NewUser += new EventHandler<NewUserEventArgs>(FUsers_NewUser);
                        FUserGenerator.LostUser += new EventHandler<UserLostEventArgs>(FUsers_LostUser);

                        //Set the resolution of the texture
                        FTexWidth = FUserGenerator.GetUserPixels(0).FullXRes;
                        FTexHeight = FUserGenerator.GetUserPixels(0).FullYRes;

                        //Reinitalie the vvvv texture
                        Reinitialize();
                        
                        //start generating data
                        FUserGenerator.StartGenerating();
                        
                        FInit = false;
                    }
                    catch (StatusException ex)
                    {
                        FLogger.Log(ex);
                        return;
                    }
                    catch (GeneralException e)
                    {
                        FLogger.Log(e);
                        return;
                    } 
                }
                //writes the user Object to the ouputs
                //this is required for the skeleton node
                else
                {
                    FUserOut[0] = FUserGenerator;

                    //write the joint position and orientation to the output
                    if (FUserGenerator.NumberOfUsers > 0)
                    {
                    	if (FEnableIn[0])
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
            }
            else
            {
                FUserOut.SliceCount = 0;
                FUserIdOut.SliceCount = 0;
                FPositionOut.SliceCount = 0;

                FUserGenerator = null;
                FInit = true;
            }
        }

        void FUsers_LostUser(object sender, UserLostEventArgs e)
        {
            Debug.WriteLine(String.Format("User deleted. ID: {0}", e.ID));
        }

        void FUsers_NewUser(object sender, NewUserEventArgs e)
        {
            Debug.WriteLine(String.Format("User found. ID: {0}", e.ID));
        }
        
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
    }
}
