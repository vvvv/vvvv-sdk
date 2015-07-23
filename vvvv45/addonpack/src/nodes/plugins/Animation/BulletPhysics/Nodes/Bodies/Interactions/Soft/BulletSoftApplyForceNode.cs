using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp.SoftBody;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "ApplyForce", Category = "Bullet", Version="SoftBody DX9", Author = "vux",
		Help = "Applies a force on a soft body", AutoEvaluate = true)]
	public class BulletSoftApplyForceNode : AbstractBodyInteractionNode<SoftBody>
	{
		[Input("Node Index",DefaultValue=-1)]
		ISpread<int> FNodeIndex;

		[Input("Force")]
		ISpread<Vector3D> FForce;

		protected override void ProcessObject(SoftBody obj, int slice)
		{
			if (this.FNodeIndex[slice] < 0)
			{
				obj.AddForce(this.FForce[slice].ToBulletVector());
			}
			else
			{
				obj.AddForce(this.FForce[slice].ToBulletVector(), this.FNodeIndex[slice] % obj.Nodes.Count);
			}		
		}
	}
}
