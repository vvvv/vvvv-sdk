using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using Microsoft.Research.Kinect.Nui;
using SlimDX;
using VVVV.MSKinect.Lib;
using VVVV.PluginInterfaces.V1;

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

        private bool FInvalidateConnect = false;

        private KinectRuntime runtime;

        private SkeletonFrame lastframe = null;
        private bool FInvalidate = true;

       

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (runtime != null)
                {
                    this.runtime.SkeletonFrameReady -= KinectSkeletonNode_SkeletonFrameReady;
                }

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    //Cache runtime node
                    this.runtime = this.FInRuntime[0];
                    this.FInRuntime[0].SkeletonFrameReady += KinectSkeletonNode_SkeletonFrameReady;
                }

                this.FInvalidateConnect = false;
            }

            if (this.FInvalidate)
            {
                if (this.lastframe != null)
                {
                    List<SkeletonData> skels = new List<SkeletonData>();
                    foreach (SkeletonData sk in this.lastframe.Skeletons)
                    {
                        if (sk.TrackingState != SkeletonTrackingState.NotTracked)
                        {
                            skels.Add(sk);
                        }
                    }

                    int cnt = skels.Count;
                    this.FOutCount[0] = cnt;

                    this.FOutPosition.SliceCount = cnt;
                    this.FOutUserIndex.SliceCount = cnt;
                    this.FOutClipped.SliceCount = cnt;
                    this.FOutJointPosition.SliceCount = cnt * (int)JointID.Count;
                    this.FOutJointState.SliceCount = cnt * (int)JointID.Count;
                    this.FOutJointID.SliceCount = cnt * (int)JointID.Count;


                    int jc = 0;
                    for (int i = 0; i < cnt; i++)
                    {
                        SkeletonData sk = skels[i];
                        this.FOutPosition[i] = new Vector3(sk.Position.X, sk.Position.Y, sk.Position.Z);
                        this.FOutUserIndex[i] = sk.UserIndex;

                        Vector4 clip = Vector4.Zero;
                        clip.X = Convert.ToSingle(sk.Quality.HasFlag(SkeletonQuality.ClippedLeft));
                        clip.Y = Convert.ToSingle(sk.Quality.HasFlag(SkeletonQuality.ClippedRight));
                        clip.Z = Convert.ToSingle(sk.Quality.HasFlag(SkeletonQuality.ClippedTop));
                        clip.W = Convert.ToSingle(sk.Quality.HasFlag(SkeletonQuality.ClippedBottom));

                        this.FOutClipped[i] = clip;

                        foreach (Joint joint in sk.Joints)
                        {
                            this.FOutJointID[jc] = joint.ID.ToString();
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
                }
                this.FInvalidate = false;
            }
        }

        void KinectSkeletonNode_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            this.lastframe = e.SkeletonFrame;
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
