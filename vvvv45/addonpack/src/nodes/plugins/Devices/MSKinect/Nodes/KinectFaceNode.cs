using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.MSKinect.Lib;

using SlimDX.Direct3D9;
using SlimDX;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Face", 
	            Category = "Kinect", 
	            Version = "Microsoft", 
	            Author = "vux", 
	            Tags = "EX9",
	           	Help = "Returns detailed 2D and 3D data describing the tracked face")]
    public class KinectFaceNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        [Output("Success")]
        private ISpread<bool> FOutOK;

        [Output("Position", Order = 10)]
        private ISpread<Vector3> FOutPosition;

        [Output("Rotation")]
        private ISpread<Vector3> FOutRotation;

        [Output("Face Points")]
        private ISpread<ISpread<Vector3>> FOutPts;
        
        [Output("Face Normals")]
		private ISpread<ISpread<Vector3>> FOutNormals;

        [Output("Projected Face Points")]
        private ISpread<ISpread<Vector2>> FOutPPTs;

        [Output("Indices")]
        private ISpread<int> FOutIndices;

        [Output("Frame Index", IsSingle = true)]
        private ISpread<int> FOutFrameIndex;

        private int frameindex = -1;

       

        private bool FInvalidateConnect = false;
        private bool FInvalidate = true;

        private KinectRuntime runtime;

        private FaceTracker face;

        private byte[] colorImage;
        private short[] depthImage;
        private Skeleton[] skeletonData;
        private readonly Dictionary<int, SkeletonFaceTracker> trackedSkeletons = new Dictionary<int, SkeletonFaceTracker>();

        //private FaceTrackFrame frm;

        [ImportingConstructor()]
        public KinectFaceNode(IPluginHost host)
        {
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.AllFrameReady -= KinectFaceNode_AllFrameReady;
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.FInRuntime[0].AllFrameReady +=KinectFaceNode_AllFrameReady;
                    }

                }

                this.FInvalidateConnect = false;
            }

            if (this.FInvalidate)
            {
                this.FOutOK.SliceCount = this.trackedSkeletons.Count;
                this.FOutPosition.SliceCount = this.trackedSkeletons.Count;
                this.FOutRotation.SliceCount = this.trackedSkeletons.Count;
                this.FOutPts.SliceCount = this.trackedSkeletons.Count;
                this.FOutPPTs.SliceCount = this.trackedSkeletons.Count;

                int cnt = 0;
                foreach (int key in this.trackedSkeletons.Keys)
                {
                    SkeletonFaceTracker sft = this.trackedSkeletons[key];
                    if (sft.frame != null)
                    {
                        this.FOutOK[cnt] = sft.frame.TrackSuccessful;
                        this.FOutPosition[cnt] = new Vector3(sft.frame.Translation.X, sft.frame.Translation.Y, sft.frame.Translation.Z);
                        this.FOutRotation[cnt] = new Vector3(sft.frame.Rotation.X, sft.frame.Rotation.Y, sft.frame.Rotation.Z) * (float)VMath.DegToCyc;

                        EnumIndexableCollection<FeaturePoint, PointF> pp = sft.frame.GetProjected3DShape();
                        EnumIndexableCollection<FeaturePoint, Vector3DF> p = sft.frame.Get3DShape();

                        this.FOutPPTs[cnt].SliceCount = pp.Count;
                        this.FOutPts[cnt].SliceCount = p.Count;
                        this.FOutNormals[cnt].SliceCount = p.Count;
                        
                        //Compute smoothed normals
						Vector3[] norms = new Vector3[p.Count];
						int[] inds = KinectRuntime.FACE_INDICES;
						int tricount = inds.Length / 3;
						for (int j = 0; j < tricount; j++)
						{
							int i1 = inds[j * 3];
							int i2 = inds[j * 3 + 1];
							int i3 = inds[j * 3 + 2];

							Vector3 v1 = new Vector3(p[i1].X, p[i1].Y, p[i1].Z);
							Vector3 v2 = new Vector3(p[i2].X, p[i2].Y, p[i2].Z);
							Vector3 v3 = new Vector3(p[i3].X, p[i3].Y, p[i3].Z);

							Vector3 faceEdgeA = v2 - v1;
							Vector3 faceEdgeB = v1 - v3;
							Vector3 norm = Vector3.Cross(faceEdgeB, faceEdgeA);

							norms[i1] += norm; 
							norms[i2] += norm; 
							norms[i3] += norm;
						}

                        for (int i = 0; i < pp.Count; i++)
                        {
                            this.FOutPPTs[cnt][i] = new Vector2(pp[i].X, pp[i].Y);
                            this.FOutPts[cnt][i] = new Vector3(p[i].X, p[i].Y,p[i].Z);
                            this.FOutNormals[cnt][i] = Vector3.Normalize(norms[i]);
                        }

                        FaceTriangle[] d = sft.frame.GetTriangles();
                        this.FOutIndices.SliceCount = d.Length * 3;
                        for (int i = 0; i < d.Length; i++)
                        {
                            this.FOutIndices[i*3] = d[i].First;
                            this.FOutIndices[i * 3+1] = d[i].Second;
                            this.FOutIndices[i * 3 +2] = d[i].Third;
                        }
                    }
                    else
                    {
                        this.FOutOK[cnt] = false;
                        this.FOutPosition[cnt] = new Vector3(sft.frame.Translation.X, sft.frame.Translation.Y, sft.frame.Translation.Z);
                        this.FOutRotation[cnt] = new Vector3(sft.frame.Rotation.X, sft.frame.Rotation.Y, sft.frame.Rotation.Z) * (float)VMath.DegToCyc;
                        this.FOutIndices.SliceCount = 0;
                        this.FOutPPTs[cnt].SliceCount = 0;
                        this.FOutPts[cnt].SliceCount = 0;
                    }
                    cnt++;
                }

                //this.FOutOK[0] = this.frm.TrackSuccessful;
                //this.FOutPosition[0] = new Vector3(frm.Translation.X, frm.Translation.Y, frm.Translation.Z);
            }

            //this.FOutFrameIndex[0] = this.frameindex;
        }

        void KinectFaceNode_AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;

            if (face == null)
            {
                face = new FaceTracker(this.runtime.Runtime);
            }

            colorImageFrame = e.OpenColorImageFrame();
            depthImageFrame = e.OpenDepthImageFrame();
            skeletonFrame = e.OpenSkeletonFrame();

            if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
            {
                return;
            }

            if (this.depthImage == null)
            {
                this.depthImage = new short[depthImageFrame.PixelDataLength];
            }

            if (this.colorImage == null)
            {
                this.colorImage = new byte[colorImageFrame.PixelDataLength];
            }

            if (this.skeletonData == null || this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
            {
                this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
            }

            colorImageFrame.CopyPixelDataTo(this.colorImage);
            depthImageFrame.CopyPixelDataTo(this.depthImage);
            skeletonFrame.CopySkeletonDataTo(this.skeletonData);

            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                    || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    // We want keep a record of any skeleton, tracked or untracked.
                    if (!this.trackedSkeletons.ContainsKey(skeleton.TrackingId))
                    {
                        this.trackedSkeletons.Add(skeleton.TrackingId, new SkeletonFaceTracker());
                    }

                    // Give each tracker the upated frame.
                    SkeletonFaceTracker skeletonFaceTracker;
                    if (this.trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                    {
                        skeletonFaceTracker.OnFrameReady(this.runtime.Runtime, ColorImageFormat.RgbResolution640x480Fps30, colorImage, DepthImageFormat.Resolution640x480Fps30, depthImage, skeleton);
                        skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;
                    }
                }
            }

            this.RemoveOldTrackers(skeletonFrame.FrameNumber);

            colorImageFrame.Dispose();
            depthImageFrame.Dispose();
            skeletonFrame.Dispose();

            this.FInvalidate = true;
        }

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in this.trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > 50)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                this.RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            this.trackedSkeletons[trackingId].Dispose();
            this.trackedSkeletons.Remove(trackingId);
        }

        private class SkeletonFaceTracker : IDisposable
        {
            //private static FaceTriangle[] faceTriangles;

            //private EnumIndexableCollection<FeaturePoint, PointF> facePoints;

            private FaceTracker faceTracker;

            private bool lastFaceTrackSucceeded;

            private SkeletonTrackingState skeletonTrackingState;

            public int LastTrackedFrame { get; set; }

            public FaceTrackFrame frame;

            public void Dispose()
            {
                if (this.faceTracker != null)
                {
                    this.faceTracker.Dispose();
                    this.faceTracker = null;
                }
            }

            public void DrawFaceModel()
            {
                if (!this.lastFaceTrackSucceeded || this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    return;
                }

                /*
                var faceModelPts = new List<Point>();
                var faceModel = new List<FaceModelTriangle>();

                for (int i = 0; i < this.facePoints.Count; i++)
                {
                    faceModelPts.Add(new Point(this.facePoints[i].X + 0.5f, this.facePoints[i].Y + 0.5f));
                }

                foreach (var t in faceTriangles)
                {
                    var triangle = new FaceModelTriangle();
                    triangle.P1 = faceModelPts[t.First];
                    triangle.P2 = faceModelPts[t.Second];
                    triangle.P3 = faceModelPts[t.Third];
                    faceModel.Add(triangle);
                }

                var faceModelGroup = new GeometryGroup();
                for (int i = 0; i < faceModel.Count; i++)
                {
                    var faceTriangle = new GeometryGroup();
                    faceTriangle.Children.Add(new LineGeometry(faceModel[i].P1, faceModel[i].P2));
                    faceTriangle.Children.Add(new LineGeometry(faceModel[i].P2, faceModel[i].P3));
                    faceTriangle.Children.Add(new LineGeometry(faceModel[i].P3, faceModel[i].P1));
                    faceModelGroup.Children.Add(faceTriangle);
                }

                drawingContext.DrawGeometry(Brushes.LightYellow, new Pen(Brushes.LightYellow, 1.0), faceModelGroup);*/
            }

            /// <summary>
            /// Updates the face tracking information for this skeleton
            /// </summary>
            internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
            {
                this.skeletonTrackingState = skeletonOfInterest.TrackingState;

                if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    // nothing to do with an untracked skeleton.
                    return;
                }

                if (this.faceTracker == null)
                {
                    try
                    {
                        this.faceTracker = new FaceTracker(kinectSensor);
                    }
                    catch (InvalidOperationException)
                    {
                        this.faceTracker = null;
                    }
                }

                if (this.faceTracker != null)
                {
                    frame = this.faceTracker.Track(
                        colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest).Clone() as FaceTrackFrame;

                    this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                    if (this.lastFaceTrackSucceeded)
                    {
                        /*if (faceTriangles == null)
                        {
                            // only need to get this once.  It doesn't change.
                            faceTriangles = frame.GetTriangles();
                        }

                        this.facePoints = frame.GetProjected3DShape();*/
                    }
                }
            }

            private struct FaceModelTriangle
            {
                public Point P1;
                public Point P2;
                public Point P3;
            }
        }

    }
}
