#region usings

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
    [PluginInfo(Name = "Skeleton",
                Category = "Kinect",
                Version = "OpenNI",
                Help = "Skelet recognition from the Kinect",
                Tags = "Kinect, OpenNI, Person",
                Author = "Phlegma")]
    #endregion PluginInfo


    public class Skeleton : IPluginEvaluate
    {
        #region fields & pins
        [Import()]
        ILogger FLogger;

        [Import()]
        IHDEHost FHost;

        [Input("Users", IsSingle = true)]
        ISpread<UserGenerator> FUsersIn;

        [Input("Joint", DefaultEnumEntry = "Head")]
        ISpread<ISpread<SkeletonJoint>> FJointIn;

        [Input("Confidence", DefaultValue=1)]
        ISpread<bool> FConfidenceIn;

        [Input("Smoothing", DefaultValue = 0.5, MinValue = 0, MaxValue = 1, IsSingle = true)]
        IDiffSpread<float> FSmoothingIn;

        [Input("File Path", StringType = StringType.Filename, DefaultString = "Skeleton")]
        IDiffSpread<string> FFilePathIn;

        [Input("Save Skeleton", IsBang = true)]
        IDiffSpread<bool> FSaveIn;

        [Input("Use Saved Skeleton")]
        IDiffSpread<bool> FLoadIn;

        [Input("Skeleton Profile", DefaultEnumEntry = "All", IsSingle = true)]
        IDiffSpread<SkeletonProfile> FSkeletonProfileIn;

        [Input("Reset", IsBang = true)]
        IDiffSpread<bool> FResetIn;

        [Input("Enable", IsSingle = true, DefaultValue = 1)]
        ISpread<bool> FEnableIn;

        [Input("Map Values", Visibility = PinVisibility.OnlyInspector, DefaultValues = new double[] { 3812, 2764, 3500 })]
        IDiffSpread<Vector3D> FMappingIn;

        [Output("User ID")]
        ISpread<int> FUserIdOut;

        [Output("Joint")]
        ISpread<ISpread<string>> FJointOut;

        [Output("Position ")]
        ISpread<ISpread<Vector3D>> FJointsPositionMappedOut;

        [Output("Confidence")]
        ISpread<ISpread<float>> FConfidenceOut;

        [Output("Position (m) ")]
        ISpread<ISpread<Vector3D>> FJointsPositionOut;

        [Output("Orientation X ")]
        ISpread<ISpread<Vector3D>> FJointOrientationXOut;

        [Output("Orientation Y ")]
        ISpread<ISpread<Vector3D>> FJointOrientationYOut;

        [Output("Orientation Z ")]
        ISpread<ISpread<Vector3D>> FJointOrientationZOut;

        [Output("Status")]
        ISpread<string> FStatusOut;



        PoseDetectionCapability FPoseDecetionCapability;
        SkeletonCapability FSkeletonCapability;

        private bool FInit = true;
        private SortedList<int, BackgroundWorker> FWorkerList = new SortedList<int, BackgroundWorker>();
        private List<string> FErrorMessages = new List<string>();

        private Object LockObject = new Object();


        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FUsersIn[0] != null)
            {
                if (FInit == true)
                {
                    try
                    {

                        //Get Pose and Skeleton Capabilities from the Users Generator
                        FPoseDecetionCapability = FUsersIn[0].PoseDetectionCapability;
                        FSkeletonCapability = FUsersIn[0].SkeletonCapability;


                        //add the Callback function to the Events
                        FSkeletonCapability.CalibrationStart += new EventHandler<CalibrationStartEventArgs>(FSkeletonCapability_CalibrationStart);
                        FSkeletonCapability.CalibrationEnd += new EventHandler<CalibrationEndEventArgs>(FSkeletonCapability_CalibrationEnd);

                        FPoseDecetionCapability.PoseDetected += new EventHandler<PoseDetectedEventArgs>(FPoseDecetionCapability_PoseDetected);
                        FPoseDecetionCapability.PoseEnded += new EventHandler<PoseEndedEventArgs>(FPoseDecetionCapability_PoseEnded);

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
                    catch (ObjectDisposedException ex2)
                    {
                        FLogger.Log(ex2);
                        return;
                    }
                }

                lock (FSkeletonCapability)
                {
                    try
                    {
                        //set the Skeleton Profil
                        if (FSkeletonProfileIn.IsChanged)
                            FSkeletonCapability.SetSkeletonProfile(FSkeletonProfileIn[0]);

                        //set the smoothing value
                        if (FSmoothingIn.IsChanged)
                            FSkeletonCapability.SetSmoothing(FSmoothingIn[0]);


                        if (FInit == false && FEnableIn[0] == true)
                        {
                            //get all Users and sort them
                            int[] tUsers = FUsersIn[0].GetUsers();
                            int[] Users = (int[])tUsers.Clone();
                            Array.Sort(Users);

                            //set the SliceCount of the binsize
                            FUserIdOut.SliceCount = Users.Length;
                            FJointOut.SliceCount = Users.Length;
                            FJointsPositionOut.SliceCount = Users.Length;
                            FJointsPositionMappedOut.SliceCount = Users.Length;
                            FJointOrientationXOut.SliceCount = Users.Length;
                            FJointOrientationYOut.SliceCount = Users.Length;
                            FJointOrientationZOut.SliceCount = Users.Length;
                            FStatusOut.SliceCount = Users.Length;
                            FConfidenceOut.SliceCount = Users.Length;


                            if (Users.Length > 0)
                            {
                                for (int i = 0; i < Users.Length; i++)
                                {
                                    //Rest the User
                                    if (FResetIn.IsChanged)
                                    {
                                        if (FResetIn[0] == true)
                                        {
                                            FSkeletonCapability.Reset(Users[i]);
                                            FWorkerList.Clear();
                                        }
                                    }

                                    //write the User ID
                                    FUserIdOut[i] = Users[i];

                                    //Check the state of the skeleton
                                    if (FSkeletonCapability.IsTracking(Users[i]))
                                    {
                                        FStatusOut[i] = "Tracking";
                                        //when the user is tracked stop pose detection
                                        FPoseDecetionCapability.StopPoseDetection(Users[i]);

                                        //write all requested joint to the ouput
                                        WriteJointValuesToOutput(i, Users);

                                        //save the user to file and to an memory slot
                                        if (FSaveIn.IsChanged)
                                        {
                                            if (FSaveIn[0] == true)
                                            {
                                                FSkeletonCapability.SaveCalibrationDataToFile(Users[i], FFilePathIn[i]);
                                                FSkeletonCapability.SaveCalibrationData(Users[i], i);
                                            }
                                        }
                                    }
                                    else if (FSkeletonCapability.IsCalibrated(Users[i]))
                                    {
                                        FStatusOut[i] = "Clibrated";
                                        //if a skeleton is loaded to a user start tracking and stop pose detection
                                        FSkeletonCapability.StartTracking(Users[i]);
                                        FPoseDecetionCapability.StopPoseDetection(Users[i]);
                                    }
                                    else if (FSkeletonCapability.IsCalibrating(Users[i]))
                                    {
                                        FStatusOut[i] = "Calibrating";
                                    }
                                    else
                                    {
                                        //if there is a users but he is not tracked delete the position data
                                        FJointOut[i].SliceCount = FJointsPositionOut[i].SliceCount = FJointsPositionMappedOut[i].SliceCount = 0;
                                        FJointOrientationXOut[i].SliceCount = FJointOrientationYOut[i].SliceCount = FJointOrientationZOut[i].SliceCount = 0;
                                        FConfidenceOut[i].SliceCount = 0;

                                        //check if a user should calibrate or load an skeleton to the users

                                        if (FLoadIn[0] == true)
                                        {
                                            //check if the user is loading the skelton
                                            if (!FWorkerList.ContainsKey(Users[i]))
                                            {
                                                FStatusOut[i] = "Load Skeleton";

                                                //load the skelton in a backgroundthread s that it does not interrup vvvv
                                                BackgroundWorker Worker = new BackgroundWorker();
                                                Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
                                                Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);

                                                FWorkerList.Add(Users[i], Worker);

                                                LoadingValues Values = new LoadingValues();
                                                Values.Slice = 1;
                                                Values.UserID = Users[i];

                                                Worker.RunWorkerAsync(Values);

                                            }
                                            else
                                            {
                                                FLogger.Log(LogType.Message, "Load Skeleton");
                                            }
                                        }
                                        else
                                        {
                                            //start the pose detection if a user should calibrate 
                                            FStatusOut[i] = "Pose Detection";
                                            FPoseDecetionCapability.StartPoseDetection("Psi", Users[i]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //reset the outputpins
                                FJointOut.SliceCount = 0;
                                FUserIdOut.SliceCount = 0;
                                FJointsPositionOut.SliceCount = 0;
                                FJointsPositionMappedOut.SliceCount = 0;
                                FJointOrientationXOut.SliceCount = 0;
                                FJointOrientationYOut.SliceCount = 0;
                                FJointOrientationZOut.SliceCount = 0;
                                FConfidenceOut.SliceCount = 0;


                                FStatusOut.SliceCount = 1;
                                FStatusOut[0] = "No User found";
                            }

                            if (FErrorMessages.Count > 0)
                            {
                                foreach (string Message in FErrorMessages)
                                {
                                    FLogger.Log(LogType.Error, Message);
                                }

                                FErrorMessages.Clear();
                            }
                        }
                        else
                        {
                            //reset the outputpins
                            FJointOut.SliceCount = 0;
                            FUserIdOut.SliceCount = 0;
                            FJointsPositionOut.SliceCount = 0;
                            FJointsPositionMappedOut.SliceCount = 0;
                            FJointOrientationXOut.SliceCount = 0;
                            FJointOrientationYOut.SliceCount = 0;
                            FJointOrientationZOut.SliceCount = 0;
                            FConfidenceOut.SliceCount = 0;

                            FStatusOut.SliceCount = 1;
                            FStatusOut[0] = "Disabled";
                        }
                    }
                    catch(ObjectDisposedException ex)
                    {
                        FLogger.Log(LogType.Warning,"Reinit Openni Plugin");
                        FPoseDecetionCapability = null;
                        FSkeletonCapability = null;
                        FInit = true;
                    }
                }
            }
        }
         
        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                lock (FErrorMessages)
                {
                    FErrorMessages.Add("Skeleton loading failed: " + e.Error.Message.ToString());
                }
            }
            else
            {
                //if the loading succed remove the worker from the list
                if (FWorkerList.ContainsKey(((LoadingValues)e.Result).UserID))
                    FWorkerList.Remove(((LoadingValues)e.Result).UserID);
            }
        }

        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (LockObject)
            {
                try
                {
                    LoadingValues Value = (LoadingValues)e.Argument;
                    e.Result = Value;
                    int i = Value.Slice;
                    int UserID = Value.UserID;

                    //try to load an skeleton from memory or file
                    if (FSkeletonCapability.IsCalibrationData(i))
                    {
                        FSkeletonCapability.LoadCalibrationData(UserID, i);
                        Debug.WriteLine(String.Format("Load Memory User {0} on Slice {1}", UserID, i));
                    }
                    else
                    { 
                        if ((FFilePathIn.SliceCount - 1) <= i)
                        {
                            //if there is file for the skeleton load it from file and write it to a slot
                            FSkeletonCapability.LoadCalibrationDataFromFile(UserID, FFilePathIn[i]);
                            FSkeletonCapability.SaveCalibrationData(UserID, i);
                            Debug.WriteLine(String.Format("Load File User {0} on Slice {1}", UserID, i));
                        }
                        else
                        {
                            int Slice = i % FFilePathIn.SliceCount;
                            FSkeletonCapability.LoadCalibrationData(UserID, Slice);
                            Debug.WriteLine(String.Format("Load Pre Memory User {0} on Slice {1}", UserID, Slice));
                        }
                    }
                }
                catch (StatusException ex)
                {
                    FErrorMessages.Add(ex.Message);
                    //Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Reads the Joint Position and writes it to the Output Pins
        /// </summary>
        /// <param name="Index">Index of Users</param>
        /// <param name="Users">Array of all Users</param>
        private void WriteJointValuesToOutput(int Index, int[] Users)
        {

            int BinSize = FJointIn[Index].SliceCount;

            FJointOut[Index].SliceCount = FJointsPositionOut[Index].SliceCount = FJointsPositionMappedOut[Index].SliceCount = BinSize;
            FJointOrientationXOut[Index].SliceCount = FJointOrientationYOut[Index].SliceCount = FJointOrientationZOut[Index].SliceCount = BinSize;
            FConfidenceOut[Index].SliceCount = BinSize;


            for (int i = 0; i < BinSize; i++)
            {
                FJointOut[Index][i] = FJointIn[Index][i].ToString();

                Point3D Point = GetJointPoint(Users[Index], FJointIn[Index][i]);
                
                
                float Confidence = 0;
                if (FConfidenceIn[Index] == true)
                    Confidence = FSkeletonCapability.GetSkeletonJointPosition(Users[Index], FJointIn[Index][i]).Confidence;
                else
                    Confidence = -1;
                
//                Debug.WriteLine(String.Format("Confidence {0} for User {1} and Joint {2}", Confidence, Users[Index], FJointIn[Index][i]));

                if (Confidence != 0 || Confidence == -1)
                {
                    FConfidenceOut[Index][i] = Confidence;
                    Vector3D Position = new Vector3D(Point.X, Point.Y, Point.Z);
                    FJointsPositionOut[Index][i] = Position / 1000;
                    FJointsPositionMappedOut[Index][i] = VMath.Map(Position, new Vector3D((FMappingIn[0].x / 2) * -1, (FMappingIn[0].y / 2), 0), new Vector3D(FMappingIn[0].x / 2, (FMappingIn[0].y / 2) * -1, FMappingIn[0].z), new Vector3D(-1, 1, 0), new Vector3D(1, -1, 1), TMapMode.Float);

                    SkeletonJointOrientation Orientation = GetJointOrientation(Users[Index], FJointIn[Index][i]);
                    FJointOrientationXOut[Index][i] = new Vector3D(Orientation.X1, Orientation.Y1, Orientation.Z1);
                    FJointOrientationYOut[Index][i] = new Vector3D(Orientation.X2, Orientation.Y2, Orientation.Z2);
                    FJointOrientationZOut[Index][i] = new Vector3D(Orientation.X3, Orientation.Y3, Orientation.Z3);
                }
                else
                {
                    FConfidenceOut[Index][i] = Confidence;
//                    Debug.WriteLine("Out of Confidence");
                }
            }

        }


        private Point3D GetJointPoint(int User, SkeletonJoint Joint)
        {
            Point3D Point = new Point3D();
            if (FSkeletonCapability.IsJointAvailable(Joint))
            {
                if (!FSkeletonCapability.IsJointActive(Joint))
                    FSkeletonCapability.SetJointActive(Joint, true);

                Point = FSkeletonCapability.GetSkeletonJointPosition(User, Joint).Position;
            }

            return Point;
        }

        /// <summary>
        /// get and orientation of an specific joint 
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Joint"></param>
        /// <returns></returns>
        private SkeletonJointOrientation GetJointOrientation(int User, SkeletonJoint Joint)
        {
            SkeletonJointOrientation Orientation = new SkeletonJointOrientation();
            if (FSkeletonCapability.IsJointAvailable(Joint))
            {
                if (!FSkeletonCapability.IsJointActive(Joint))
                    FSkeletonCapability.SetJointActive(Joint, true);

                Orientation = FSkeletonCapability.GetSkeletonJointOrientation(User, Joint);
            }

            return Orientation;
        }



        void FPoseDecetionCapability_PoseEnded(object sender, PoseEndedEventArgs e)
        {
            lock (FErrorMessages)
                FErrorMessages.Add(String.Format("Pose Ended. ID: {0}; Pose:{1}", e.ID, e.Pose));

            //Debug.WriteLine(String.Format("Pose Ended. ID: {0}; Pose:{1}", e.ID, e.Pose));
        }

        void FPoseDecetionCapability_PoseDetected(object sender, PoseDetectedEventArgs e)
        {
            FSkeletonCapability.RequestCalibration(e.ID, true);

            lock (FErrorMessages)
                FErrorMessages.Add(String.Format("Pose for User {0} detected", e.ID));
        }

        void FSkeletonCapability_CalibrationEnd(object sender, CalibrationEndEventArgs e)
        {
            if (e.Success)
                FSkeletonCapability.StartTracking(e.ID);
            else
            {
                FPoseDecetionCapability.StartPoseDetection("Psi", e.ID);

                lock (FErrorMessages)
                    FErrorMessages.Add(String.Format("Calibration for User {0} failed", e.ID));
            }

            //Debug.WriteLine(String.Format("Calibration End. ID: {0}; Success:{1}", e.ID, e.Success));
        }

        void FSkeletonCapability_CalibrationStart(object sender, CalibrationStartEventArgs e)
        {
            FPoseDecetionCapability.StopPoseDetection(e.ID);

            lock (FErrorMessages)
                FErrorMessages.Add(String.Format("Calibration for User {0} started", e.ID));
            //Debug.WriteLine(String.Format("Calibration Start. ID: {0} ", e.ID));
        }
    }






    class LoadingValues
    {
        public int UserID;
        public int Slice;
    }
}
