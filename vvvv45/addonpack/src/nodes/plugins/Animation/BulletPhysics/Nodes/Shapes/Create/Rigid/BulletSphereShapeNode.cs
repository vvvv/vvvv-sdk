using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Sphere", Category="Bullet", Version = "DX9", Author="vux")]
	public class BulletSphereShapeNode : AbstractBulletRigidShapeNode
	{
		[Input("Radius", DefaultValue = 0.5)]
		IDiffSpread<float> FRadius;

		[Input("Resolution X", DefaultValue = 10)]
		IDiffSpread<int> FResX;

		[Input("Resolution Y", DefaultValue = 10)]
		IDiffSpread<int> FResY;

		public override void Evaluate(int SpreadMax)
		{
			if (this.BasePinsChanged
				|| this.FRadius.IsChanged
				|| this.FResX.IsChanged 
				|| this.FResY.IsChanged
				)
			{
				this.FShapes.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
					SphereShapeDefinition sphere = new SphereShapeDefinition(Math.Abs(this.FRadius[i]),this.FResX[i],this.FResY[i]);
					sphere.Mass = this.FMass[i];
					this.SetBaseParams(sphere, i);
					this.FShapes[i] = sphere;
				}
			}			
		}
	}
}
