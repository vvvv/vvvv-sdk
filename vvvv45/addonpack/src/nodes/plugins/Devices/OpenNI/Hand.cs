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
    [PluginInfo(Name = "Hand",
                Category = "Kinect",
                Version = "OpenNI",
                Help = "Hand recognition from the Kinect",
                Tags = "Kinect, OpenNI, Hand")]
    #endregion PluginInfo
    public class HandOpenNI: IPluginEvaluate, IDisposable
    {
        #region fields & pins
        [Input("Context", IsSingle=true)]
        ISpread<Context> FContextIn;

        [Input("Track Start Position")]
        ISpread<Vector3D> FTrackStartIn;
        
        [Input("Enable", IsSingle = true, DefaultValue=1)]
        IDiffSpread<bool> FEnableIn;

        [Output("Hand Position")]
        ISpread<Vector3D> FHandPositionOut;
        
        [Output("Hand ID")]
        ISpread<int> FHandIdOut;

        [Import()]
        ILogger FLogger;

        HandsGenerator FHandGenerator;

        private bool FInit = true;
        private Dictionary<int, Vector3D> FTrackedHands = new Dictionary<int, Vector3D>();
        private Dictionary<int, Point3D> FTrackedStarts = new Dictionary<int, Point3D>();
        //private List<Vector3D> FTrackedStarts = new List<Vector3D>();
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
                        // Create and Hands generator
                        FHandGenerator = new HandsGenerator(FContextIn[0]);
                        FHandGenerator.HandCreate += new EventHandler<HandCreateEventArgs>(FHands_HandCreate);
                        FHandGenerator.HandDestroy += new EventHandler<HandDestroyEventArgs>(FHands_HandDestroy);
                        FHandGenerator.HandUpdate += new EventHandler<HandUpdateEventArgs>(FHands_HandUpdate);

                        //Start the Nodes to generate data
                        FHandGenerator.StartGenerating();
                    }
                    catch (StatusException ex)
                    {
                        FLogger.Log(ex);
                    }
                    catch (GeneralException e)
                    {
                        FLogger.Log(e);
                    }
                    
                    FInit = false;
                }
                
                for (int i = 0; i < FTrackStartIn.SliceCount; i++)
                {
                	var p = new Point3D((float)(FTrackStartIn[i].x * 1000), (float)(FTrackStartIn[i].y * 1000), (float)(FTrackStartIn[i].z * 1000));
                	if (!FTrackedStarts.ContainsValue(p))
                		FHandGenerator.StartTracking(p);
                }
                
               // if (FTrackIn[0] && (FTrackedHands.Count < 2))
               // 	FHandGenerator.StartTracking(new Point3D(128, 31, 773));
               
                // Write the received Gestures with their endposition to the output
                FHandIdOut.SliceCount = FHandPositionOut.SliceCount = FTrackedHands.Count;
                int Slice = 0;
                lock(FTrackedHands)
	                foreach (var item in FTrackedHands)
	                {
	                    FHandIdOut[Slice] = item.Key;
	                    FHandPositionOut[Slice] = item.Value;
	                    Slice++;
	                }
            }
            else
            {
                FHandIdOut.SliceCount = FHandPositionOut.SliceCount = 0;
            }
        }

        void FHands_HandUpdate(object sender, HandUpdateEventArgs e)
        {
        	lock(FTrackedHands)
	    		if (FTrackedHands.ContainsKey(e.UserID))
	    			FTrackedHands[e.UserID] = new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000);
        }

        void FHands_HandDestroy(object sender, HandDestroyEventArgs e)
        {
            // Debug.WriteLine(String.Format("Hand ID: {0} destroyed", e.UserID));
            if (FTrackedHands.ContainsKey(e.UserID))
            {
            	lock(FTrackedHands)
            		FTrackedHands.Remove(e.UserID);
           		FTrackedStarts.Remove(e.UserID);
            }
        }

        void FHands_HandCreate(object sender, HandCreateEventArgs e)
        {
        	lock(FTrackedHands)
        		FTrackedHands.Add(e.UserID, new Vector3D(e.Position.X / 1000, e.Position.Y / 1000, e.Position.Z / 1000));
        	FTrackedStarts.Add(e.UserID, e.Position);
        }
        
        #region Dispose

        public void Dispose()
        {
        	if (FHandGenerator != null)
        	{
        		FHandGenerator.HandCreate -= new EventHandler<HandCreateEventArgs>(FHands_HandCreate);
                FHandGenerator.HandDestroy -= new EventHandler<HandDestroyEventArgs>(FHands_HandDestroy);
                FHandGenerator.HandUpdate -= new EventHandler<HandUpdateEventArgs>(FHands_HandUpdate);
                FHandGenerator.Dispose();
        	}
        }

        #endregion 
    }
}
