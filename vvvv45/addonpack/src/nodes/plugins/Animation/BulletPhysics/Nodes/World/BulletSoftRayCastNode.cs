using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;
using VVVV.Internals.Bullet;
using BulletSharp.SoftBody;


namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "RayCast", Category = "Bullet", Version="SoftBody DX9", Author = "vux")]
    public class BulletSoftRayCastNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        Pin<BulletRigidSoftWorld> FWorld;

        [Input("From")]
        ISpread<Vector3D> FFrom;

        [Input("To")]
        ISpread<Vector3D> FTo;

        [Output("Hit")]
        ISpread<bool> FHit;

        [Output("Hit Fraction")]
        ISpread<double> FHitFraction;

        [Output("Hit Position")]
        ISpread<Vector3D> FHitPosition;

        [Output("Hit Feature")]
        ISpread<EFeature> FHitFeature;

        [Output("Hit Index")]
        ISpread<int> FHitIndex;

        [Output("Query Index")]
        ISpread<int> FQueryIndex;

        [Output("Body")]
        ISpread<SoftBody> FBody;

        [Output("Node")]
        ISpread<Node> FNode;

        [Output("Body Id")]
        ISpread<int> FId;

        public void Evaluate(int SpreadMax)
        {
            if (this.FWorld.PluginIO.IsConnected)
            {
                this.FHit.SliceCount = SpreadMax;

                List<double> fraction = new List<double>();
                List<Vector3D> position = new List<Vector3D>();
                List<Vector3D> normal = new List<Vector3D>();
                List<SoftBody> body = new List<SoftBody>();
                List<int> bodyid = new List<int>();
                List<int> qidx = new List<int>();
                List<int> nidx = new List<int>();
                List<EFeature> feat = new List<EFeature>();
                List<Node> nodes = new List<Node>();


                for (int i = 0; i < SpreadMax; i++)
                {
                    Vector3 from = this.FFrom[i].ToBulletVector();
                    Vector3 to = this.FTo[i].ToBulletVector();

                    List<SoftBody> sbs = this.FWorld[0].SoftBodies;
                    //List<SRayCast> results = new List<SRayCast>();

                    SRayCast closest = null;

                    for (int ib = 0; ib < sbs.Count; ++ib)
                    {
                        SoftBody psb = sbs[ib];
                        SRayCast res = new SRayCast();
                        if (psb.RayTest(from, to, res))
                        {
                            if (closest == null)
                            { closest = res; }
                            else
                            { if (res.Fraction < closest.Fraction) { closest = res; } }
                        }
                    }

                    if (closest != null)
                    {
                        this.FHit[i] = true;
                        fraction.Add(closest.Fraction);

                        Vector3 pos = from + (to - from) * closest.Fraction;
                        position.Add(pos.ToVVVVector());
                        body.Add(closest.Body);
                        qidx.Add(i);
                        SoftBodyCustomData sd = (SoftBodyCustomData)closest.Body.UserObject;
                        bodyid.Add(sd.Id);
                        feat.Add(closest.Feature);

                        int nodeidx = closest.Index;

                        Node fnode = null;

                        if (closest.Feature == EFeature.Face)
                        {
                            Face f = closest.Body.Faces[closest.Index];
                            Node node = f.N[0];
                            for (int ni = 1; ni < 3; ++ni)
                            {
                                if ((node.X - pos).LengthSquared() >
                                    (f.N[ni].X - pos).LengthSquared())
                                {
                                    fnode = f.N[ni];
                                }
                            }
                        }

                        if (closest.Feature == EFeature.Node)
                        {
                            fnode = closest.Body.Nodes[nodeidx];
                        }

                        nidx.Add(nodeidx);
                        nodes.Add(fnode);
                               
                    }
                    else
                    {
                        this.FHit[i] = false;
                    }
                }


                this.FId.AssignFrom(bodyid);
                this.FHitFraction.AssignFrom(fraction);
                this.FHitPosition.AssignFrom(position);
                this.FQueryIndex.AssignFrom(qidx);
                this.FBody.AssignFrom(body);
                this.FHitFeature.AssignFrom(feat);
                this.FHitIndex.AssignFrom(nidx);
                this.FNode.AssignFrom(nodes);
            }
            else
            {
                this.FHit.SliceCount = 0;
                this.FId.SliceCount = 0;
                this.FHitFraction.SliceCount = 0;
                this.FHitPosition.SliceCount = 0;
                this.FHitFeature.SliceCount = 0;
                this.FHitIndex.SliceCount = 0;
                this.FBody.SliceCount = 0;
                this.FNode.SliceCount = 0;
            }

        }
    }
}
