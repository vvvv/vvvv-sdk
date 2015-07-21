using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Nodes.Bullet;
using BulletSharp.SoftBody;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Bullet.Nodes.Bodies.Interactions.Soft
{
    [PluginInfo(Name = "ApplyForce", Category = "Bullet", Version = "Node DX9", Author = "vux",
        Help = "Applies a force on a soft body", AutoEvaluate = true)]
    public class BulletSoftNodeApplyForceNode : AbstractBodyInteractionNode<Node>
    {
        [Input("Force")]
        ISpread<Vector3D> FForce;

        protected override void ProcessObject(Node obj, int slice)
        {
            obj.Force += this.FForce[slice].ToBulletVector();
        }
    }
}
