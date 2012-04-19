using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using SlimDX;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1;
using Microsoft.Kinect;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name="Skeleton",Category="Kinect",Version="Microsoft",Author="vux")]
    public class KinectSkeletonNode : IPluginEvaluate, IPluginConnections
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        [Output("Skeleton Count",IsSingle = true)]
        private ISpread<int> FOutCount;

        [Output("User Index")]
        private ISpread<int> FOutUserIndex;

        [Output("Position")]
        private ISpread<Vector3> FOutPosition;

        [Output("Clipping")]
        private ISpread<Vector4> FOutClipped;

        [Output("Joint ID")]
        private ISpread<string> FOutJointID;

        [Output("Joint Position")]
        private ISpread<Vector3> FOutJointPosition;

        [Output("Joint State")]
        private ISpread<string> FOutJointState;

        [Output("Frame Number", IsSingle = true)]
        private ISpread<int> FOutFrameNumber;


        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private bool FInvalidate = true;

        private Skeleton[] lastframe = null;
        private object m_lock = new object();
        private int frameid = -1;

       

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.SkeletonFrameReady -= SkeletonReady;
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];

                    if (runtime != null)
                    {
                        this.FInRuntime[0].SkeletonFrameReady += SkeletonReady;
                    }
                    
                }

                this.FInvalidateConnect = false;
            }

            if (this.FInvalidate)
            {
                if (this.lastframe != null)
                {
                    List<Skeleton> skels = new List<Skeleton>();
                    float z = float.MaxValue;
                    int id = -1;

                    lock (m_lock)
                    {

                        foreach (Skeleton sk in this.lastframe)
                        {
                            if (sk.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                skels.Add(sk);
                            }
                        }
                    }

                    int cnt = skels.Count;
                    this.FOutCount[0] = cnt;

                    this.FOutPosition.SliceCount = cnt;
                    this.FOutUserIndex.SliceCount = cnt;
                    this.FOutClipped.SliceCount = cnt;
                    this.FOutJointPosition.SliceCount = cnt * 20;
                    this.FOutJointState.SliceCount = cnt * 20;
                    this.FOutJointID.SliceCount = cnt * 20;
                    this.FOutFrameNumber[0] = this.frameid;


                    int jc = 0;
                    for (int i = 0; i < cnt; i++)
                    {
                        Skeleton sk = skels[i];
                        this.FOutPosition[i] = new Vector3(sk.Position.X, sk.Position.Y, sk.Position.Z);
                        this.FOutUserIndex[i] = sk.TrackingId;
                        
                        Vector4 clip = Vector4.Zero;
                        clip.X = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Left));
                        clip.Y = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Right));
                        clip.Z = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Top));
                        clip.W = Convert.ToSingle(sk.ClippedEdges.HasFlag(FrameEdges.Bottom));

                        this.FOutClipped[i] = clip;

                        foreach (Joint joint in sk.Joints)
                        {
                            this.FOutJointID[jc] = joint.JointType.ToString();
                            this.FOutJointPosition[jc] = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            this.FOutJointState[jc] = joint.TrackingState.ToString();

                            jc++;
                        }
                    }
                }
                else
                {
                    this.FOutCount[0] = 0;
                    this.FOutPosition.SliceCount = 0;
                    this.FOutUserIndex.SliceCount = 0;
                    this.FOutJointID.SliceCount = 0;
                    this.FOutJointPosition.SliceCount = 0;
                    this.FOutJointState.SliceCount = 0;
                    this.FOutFrameNumber[0] = 0;
                }
                this.FInvalidate = false;
            }
        }

        private void SkeletonReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    this.lastframe = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    lock (m_lock)
                    {
                        skeletonFrame.CopySkeletonDataTo(this.lastframe);
                    }
                    this.frameid = skeletonFrame.FrameNumber;
                }
            }
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
    }
}
