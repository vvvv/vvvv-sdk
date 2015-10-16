using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using VVVV.DataTypes.Bullet;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Cylinder", Category="Bullet", Version = "DX9", Author="vux")]
	public class BulletCylinderShapeNode : AbstractBulletRigidShapeNode
	{
		[Input("Radius", DefaultValue = 0.5)]
		IDiffSpread<float> FRadius;
		
		[Input("Length", DefaultValue = 1.0)]
		IDiffSpread<float> FLength;

		[Input("Resolution X", DefaultValue = 10)]
		IDiffSpread<int> FResX;

		[Input("Resolution Y", DefaultValue = 10)]
		IDiffSpread<int> FResY;


		public override void Evaluate(int SpreadMax)
		{
			if (this.BasePinsChanged
				|| this.FRadius.IsChanged
				|| this.FLength.IsChanged
				|| this.FResX.IsChanged
				|| this.FResY.IsChanged
				)
			{
				this.FShapes.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax; i++)
				{
					CylinderShapeDefinition cyl = new CylinderShapeDefinition(Math.Abs(this.FRadius[i]), Math.Abs(this.FRadius[i]), Math.Abs(this.FLength[i]),this.FResX[i],this.FResY[i]);
					cyl.Mass = this.FMass[i];
					this.SetBaseParams(cyl, i);
					this.FShapes[i] = cyl;
				}
			}			
		}
	}
}
