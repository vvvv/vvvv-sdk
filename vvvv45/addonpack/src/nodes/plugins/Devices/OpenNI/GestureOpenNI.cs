#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using OpenNI;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Gesture",
                Category = "Kinect",
                Version = "OpenNI",
                Help = "Gesture recognition from the Kinect",
                Tags = "Kinect, OpenNI, Hand",
                Author = "Phlegma")]
    #endregion PluginInfo


    public class GestureOpenNI : IPluginEvaluate
    {
        #region fields & pins
        [Input("Context", IsSingle=true)]
        ISpread<Context> FContextIn;

        [Input("Gestures Names", DefaultString = "RaiseHand")]
        IDiffSpread<string> FGestureNamesIn;

        [Input("Smoothing", IsSingle = true, DefaultValue = 0.5, MaxValue=1, MinValue=0)]
        IDiffSpread<float> FSmoothingIn;

        [Input("Reset", IsSingle = true, IsBang = true)]
        IDiffSpread<bool> FStopTrackingIn;

        [Input("Enable", IsSingle = true, DefaultValue=1)]
        IDiffSpread<bool> FEnableIn;

        [Input("Map Values", Visibility = PinVisibility.OnlyInspector, DefaultValues = new double[]{3812,2764,3500})]
        IDiffSpread<Vector3D> FMappingIn;

        [Output("Possible Gestures")]
        ISpread<string> FGesturesPosOut;

        [Output("Active Gestures")]
        ISpread<string> FGestureActOut;

        [Output("Recognized Gestures")]
        ISpread<string> FGesturesRegOut;

        [Output("Gesture Position")]
        ISpread<Vector3D> FGesturePositionMappedOut;

        [Output("Gesture Position (m)")]
        ISpread<Vector3D> FGesturePositionOut;

        [Output("Hand ID")]
        ISpread<int> FUserIdOut;

        [Output("Tracking Position")]
        ISpread<Vector3D> FMappedPositionOut;

        [Output("Tracking Position (m) ")]
        ISpread<Vector3D> FPositionOut;

        [Output("Position Moving Hands")]
        ISpread<Vector3D> FMovingHandsOut;

        [Import()]
        ILogger FLogger;

        GestureGenerator FGesture;
        HandsGenerator FHands;

        private bool FInit = true;
        private SortedList<int,Vector3D> FPositions = new SortedList<int,Vector3D>();
        private Dictionary<Vector3D,string> FGestureRecognized = new Dictionary<Vector3D,string>();
        private List<Vector3D> FMovingHands = new List<Vector3D>();


        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FContextIn[0] != null)
            {

                // Initialization
                if (FInit == true)
                {
                    
                    try
                    {
                        // Create Gesture and Hands generator
                        FGesture = new GestureGenerator(FContextIn[0]);
                        FHands = new HandsGenerator(FContextIn[0]);

                        //Write all possibel Gesture to the Output Pin
                        string[] Enum = FGesture.EnumerateAllGestures();
                        FGesturesPosOut.SliceCount = Enum.Length;
                        for (int i = 0; i < Enum.Length; i++)
                        {
                            FGesturesPosOut[i] = Enum[i];
                        }

                        //Start the Nodes to generate data
                        FHands.StartGenerating();
                        FGesture.StartGenerating();
                    }
                    catch (StatusException ex)
                    {
                        FLogger.Log(ex);
                    }
                    catch (GeneralException e)
                    {
                        FLogger.Log(e);
                    }
                }

                // Handle the Input Pins
                if (FEnableIn.IsChanged || FInit)
                {
                    if (FEnableIn[0] == true)
                    {
                        //Add Event Handler for the Gesture Generator
                        FGesture.GestureRecognized += new EventHandler<GestureRecognizedEventArgs>(FGesture_GestureRecognized);
                        FGesture.GestureProgress += new EventHandler<GestureProgressEventArgs>(FGesture_GestureProgress);

                        //Add Event Handlers for Hand Generator
                        FHands.HandCreate += new EventHandler<HandCreateEventArgs>(FHands_HandCreate);
                        FHands.HandDestroy += new EventHandler<HandDestroyEventArgs>(FHands_HandDestroy);
                        FHands.HandUpdate += new EventHandler<HandUpdateEventArgs>(FHands_HandUpdate);

                    }
                    else if(FEnableIn[0] == false && FInit == false)
                    {
                        //Add Event Handler for the Gesture Generator
                        FGesture.GestureRecognized -= new EventHandler<GestureRecognizedEventArgs>(FGesture_GestureRecognized);
                        FGesture.GestureProgress -= new EventHandler<GestureProgressEventArgs>(FGesture_GestureProgress);

                        //Add Event Handlers for Hand Generator
                        FHands.HandCreate -= new EventHandler<HandCreateEventArgs>(FHands_HandCreate);
                        FHands.HandDestroy -= new EventHandler<HandDestroyEventArgs>(FHands_HandDestroy);
                        FHands.HandUpdate -= new EventHandler<HandUpdateEventArgs>(FHands_HandUpdate);
                    }
                }

                //Check the Incomming gestures and try to add them to the Gesture generator
                if (FGestureNamesIn.IsChanged || FEnableIn.IsChanged || FInit)
                {
                    //check which getsures are active and delete them
                    string[] ActiveGesture = FGesture.GetAllActiveGestures();
                    if (ActiveGesture.Length > 0)
                    {
                        foreach (string Gesture in ActiveGesture)
                        {
                            try
                            {
                                FGesture.RemoveGesture(Gesture);
                            }
                            catch (StatusException ex)
                            {
                                FLogger.Log(LogType.Error, String.Format("Cannot Remove Gesture: {0}", Gesture));
                                FLogger.Log(LogType.Message, ex.Message);
                            }
                        }
                    }

                    //add all gestures to the Gesture Generator
                    foreach (string Gesture in FGestureNamesIn)
                    {
                        try
                        {
                            FGesture.AddGesture(Gesture);
                        }
                        catch (StatusException ex)
                        {
                            FLogger.Log(LogType.Error, String.Format("Cannot Add Gesture: {0}", Gesture));
                            FLogger.Log(LogType.Message, ex.Message);
                        }
                    }

                    //check with gestures are active and write them to the output
                    ActiveGesture = null;
                    ActiveGesture = FGesture.GetAllActiveGestures();
                    FGestureActOut.SliceCount = ActiveGesture.Length;
                    for (int i = 0; i < ActiveGesture.Length; i++)
                    {
                        FGestureActOut[i] = ActiveGesture[i];
                    }
                }

                //set smoothing
                if (FSmoothingIn.IsChanged)
                    FHands.SetSmoothing(FSmoothingIn[0]);

                //stop traking
                if (FStopTrackingIn.IsChanged && FStopTrackingIn[0] == true)
                    FHands.StopTrackingAll();

                // Handle the Output Pins
                if (FInit == false && FEnableIn[0] == true)
                {
                    if (FGestureRecognized.Count > 0)
                    {
                        //lock the List that it its not changed  by the OpenNI Thread
                        lock (FGestureRecognized)
                        {
                            FGesturesRegOut.SliceCount = FGestureRecognized.Count;
                            int Slice = 0;

                            // Write the received Gestures with their endposition to the output
                            foreach (KeyValuePair<Vector3D, string> item in FGestureRecognized)
                            {
                                FGesturesRegOut[Slice] = item.Value;
                                FGesturePositionOut[Slice] = item.Key / 1000;

                                //Map the Position from milimeter to vvvv coordinates 
                                FGesturePositionMappedOut[Slice] = VMath.Map(item.Key, new Vector3D((FMappingIn[0].x / 2) * -1, (FMappingIn[0].y / 2), 0), new Vector3D(FMappingIn[0].x / 2, (FMappingIn[0].y / 2) * -1, FMappingIn[0].z), new Vector3D(-1, 1, 0), new Vector3D(1, -1, 1), TMapMode.Float);
                                Slice++;
                            }

                            FGestureRecognized.Clear();
                        }
                    }
                    else
                    {
                        FGesturesRegOut.SliceCount = 0;
                    }

                    //write tracked hand id and position to the output 
                    if (FPositions.Count > 0)
                    {
                        lock (FPositions)
                        {
                            IList<Vector3D> Vectors = FPositions.Values;
                            IList<int> UserIds = FPositions.Keys;

                            FPositionOut.SliceCount = FMappedPositionOut.SliceCount = Vectors.Count;
                            FUserIdOut.SliceCount = UserIds.Count;

                            for (int i = 0; i < Vectors.Count; i++)
                            {
                                try
                                {
                                    FPositionOut[i] = Vectors[i] / 1000;
                                    //Map the Position from milimeter to vvvv coordinates 
                                    FMappedPositionOut[i] = VMath.Map(Vectors[i], new Vector3D((FMappingIn[0].x / 2) * -1, (FMappingIn[0].y / 2), 0), new Vector3D(FMappingIn[0].x / 2, (FMappingIn[0].y / 2) * -1, FMappingIn[0].z), new Vector3D(-1, 1, 0), new Vector3D(1, -1, 1), TMapMode.Float);
                                    FUserIdOut[i] = UserIds[i];
                                }
                                catch (ArgumentOutOfRangeException Exception)
                                {
                                    FLogger.Log(Exception);
                                }
                            }
                        }
                    }
                    else
                    {
                        FUserIdOut.SliceCount = 0;
                        FPositionOut.SliceCount = 0;
                        FMappedPositionOut.SliceCount = 0;
                    }

                    //write the position from moving hands to the output
                    if (FMovingHands.Count > 0)
                    {
                        FMovingHandsOut.SliceCount = FMovingHands.Count;
                        lock (FMovingHands)
                        {
                            for (int i = 0; i < FMovingHands.Count; i++)
                                FMovingHandsOut[i] = FMovingHands[i];
                        }
                        FMovingHands.Clear();
                    }
                    else
                        FMovingHandsOut.SliceCount = 0;
                }

                FInit = false;
            }
            else
            {
                FGesturesRegOut.SliceCount = FPositionOut.SliceCount = FMappedPositionOut.SliceCount = FUserIdOut.SliceCount = 0;
            }
        }

        void FHands_HandDestroy(ProductionNode node, uint id, float fTime)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Callback for Gesture Prograss Event. Regonized Moving Hands a store theire Position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FGesture_GestureProgress(object sender, GestureProgressEventArgs e)
        {
            if (e.Gesture == "MovingHand")
                FMovingHands.Add(new Vector3D(e.Position.X, e.Position.Y, e.Position.Z));
        }

 
        /// <summary>
        /// Callback function for the HandUpdate Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FHands_HandUpdate(object sender, HandUpdateEventArgs e)
        {
            //Debug.WriteLine(String.Format("Hand ID: {0} updated : {1} y: {2} z: {3}", e.UserID, e.Position.X, e.Position.Y, e.Position.Z));

            Vector3D Vector;
            Vector.x = e.Position.X;
            Vector.y = e.Position.Y;
            Vector.z = e.Position.Z;

            if (FPositions.ContainsKey(e.UserID))
            {
                FPositions.Remove(e.UserID);
                FPositions.Add(e.UserID, Vector);
            }
            else
                FPositions.Add(e.UserID, Vector);

        }

        /// <summary>
        /// Callback for the Hand Destroy Event. Delete ID form Postion List.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FHands_HandDestroy(object sender, HandDestroyEventArgs e)
        {
           // Debug.WriteLine(String.Format("Hand ID: {0} destroyed", e.UserID));
            if (FPositions.ContainsKey(e.UserID))
                FPositions.Remove(e.UserID);
        }

        /// <summary>
        /// Callback for the Hand Create Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FHands_HandCreate(object sender, HandCreateEventArgs e)
        {
            if (!FPositions.Keys.Contains(e.UserID))
                FPositions.Add(e.UserID, new Vector3D(e.Position.X, e.Position.Y, e.Position.Z));
            else
            {
                FPositions.Remove(e.UserID);
                FPositions.Add(e.UserID, new Vector3D(e.Position.X, e.Position.Y, e.Position.Z));
            }
            //Debug.WriteLine(String.Format("Hand ID: {0} updated x: {1} y: {2} z: {3}", e.UserID, e.Position.X, e.Position.Y, e.Position.Z));

        }


        /// <summary>
        /// Callback function for the Gesture Recognized Event. 
        /// Takes the endposition of the gesture and try to track the hand from 
        /// this position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FGesture_GestureRecognized(object sender, GestureRecognizedEventArgs e)
        {
            try
            {
                lock (FGestureRecognized)
                {
                    Vector3D Vector = new Vector3D(e.EndPosition.X, e.EndPosition.Y, e.EndPosition.Z);
                    FGestureRecognized.Add(Vector, e.Gesture);
                }
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }

            FHands.StartTracking(e.EndPosition);
        }
    }
}
