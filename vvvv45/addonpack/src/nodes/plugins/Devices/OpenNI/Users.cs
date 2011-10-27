#region usings

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;


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
                Author = "Phlegma")]
    #endregion PluginInfo


    public class Users : IPluginEvaluate
    {
        #region fields & pins
        [Input("Context", IsSingle=true)]
        ISpread<Context> FContextIn;

        [Input("Map Values", Visibility = PinVisibility.OnlyInspector, DefaultValues = new double[] { 3812, 2764, 3500 })]
        IDiffSpread<Vector3D> FMappingIn;

        [Output("Users", IsSingle = true)]
        ISpread<UserGenerator> FUserOut;

        [Output("Position")]
        ISpread<Vector3D> FMappedPositionOut;

        [Output("Position (m) ")]
        ISpread<Vector3D> FPositionOut;

        [Output("User ID")]
        ISpread<int> FUserIdOut;

        [Import()]
        ILogger FLogger;

        UserGenerator FUsers;

        private bool FInit = true;
        private SortedList<int,Vector3D> FPositions = new SortedList<int,Vector3D>();

        #endregion fields & pins

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
                        FUsers = new UserGenerator(FContextIn[0]);

                        //start generating data
                        FUsers.StartGenerating();

                        // add the Callback function to the Events
                        FUsers.NewUser += new EventHandler<NewUserEventArgs>(FUsers_NewUser);
                        FUsers.LostUser += new EventHandler<UserLostEventArgs>(FUsers_LostUser);
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
                if (FInit == false)
                {
                    FUserOut[0] = FUsers;


                    //write the joint position and orientation to the output
                    if (FUsers.NumberOfUsers > 0 && FInit == false)
                    {
                        //copies a list of all users and sort them
                        int[] tUsers = FUsers.GetUsers();
                        int[] Users = (int[])tUsers.Clone();
                        Array.Sort(Users);

                        FUserIdOut.SliceCount = Users.Length;
                        FPositionOut.SliceCount = Users.Length;
                        FMappedPositionOut.SliceCount = Users.Length;

                        for (int i = 0; i < Users.Length; i++)
                        {
                            FUserIdOut[i] = Users[i];
                            try
                            {
                                //middle point of the User
                                Point3D Point = FUsers.GetCoM(Users[i]);
                                Vector3D Position = new Vector3D(Point.X, Point.Y, Point.Z);
                                FPositionOut[i] = Position / 1000;

                                //map postion values to vvvv coordinates
                                FMappedPositionOut[i] = VMath.Map(Position, new Vector3D((FMappingIn[0].x / 2) * -1, (FMappingIn[0].y / 2), 0), new Vector3D(FMappingIn[0].x / 2, (FMappingIn[0].y / 2) * -1, FMappingIn[0].z), new Vector3D(-1, 1, 0), new Vector3D(1, -1, 1), TMapMode.Float);
                                FPositionOut[i] = Position / 1000;
                            }
                            catch (StatusException ex)
                            {
                                FLogger.Log(ex);
                            }
                        }
                    }
                    else
                    {
                        FUserIdOut.SliceCount = 0;
                        FPositionOut.SliceCount = 0;
                        FMappedPositionOut.SliceCount = 0;
                    }
                }

                
            }
            else
            {
                FUserOut.SliceCount = 0;
                FUserIdOut.SliceCount = 0;
                FPositionOut.SliceCount = 0;
                FMappedPositionOut.SliceCount = 0;

                FUsers = null;
                FInit = true;
            }
        }

        /// <summary>
        /// Callback function for LostUser Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FUsers_LostUser(object sender, UserLostEventArgs e)
        {
            //FPoseDecetionCapability.StopPoseDetection(e.ID);
            Debug.WriteLine(String.Format("User deleted. ID: {0}", e.ID));
        }


        /// <summary>
        /// callback function for the NewUser Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FUsers_NewUser(object sender, NewUserEventArgs e)
        {
            //FPoseDecetionCapability.StartPoseDetection("Psi", e.ID);
            Debug.WriteLine(String.Format("User found. ID: {0}", e.ID));
        }
    }
}
