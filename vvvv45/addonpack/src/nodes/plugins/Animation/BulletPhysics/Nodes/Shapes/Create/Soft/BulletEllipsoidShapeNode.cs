using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Ellipsoid", Category="Bullet", Version = "DX9", Author="vux")]
	public class BulletEllipsoidShapeNode : AbstractSoftShapeNode
	{
		[Input("Center", DefaultValues = new double[] {0,0,0 })]
		IDiffSpread<Vector3D> FPinInCenter;

		[Input("Radius", DefaultValues = new double[] { 0.5, 0.5, 0.5 })]
		IDiffSpread<Vector3D> FPinInRadius;

		[Input("Resolution",DefaultValue=20)]
		IDiffSpread<int> FPinInRes;

		protected override bool SubPinsChanged
		{
			get 
			{
				return this.FPinInCenter.IsChanged
					|| this.FPinInRadius.IsChanged
					|| this.FPinInRes.IsChanged;
			}
		}

		protected override AbstractSoftShapeDefinition GetShapeDefinition(int slice)
		{
			return new EllipsoidSoftShapeDefnition(
				this.FPinInCenter[slice].ToBulletVector(),
				this.FPinInRadius[slice].ToBulletVector(),
				this.FPinInRes[slice]);
		}

		protected override int SubPinSpreadMax
		{
			get
			{
				return ArrayMax.Max
					(
						this.FPinInCenter.SliceCount,
						this.FPinInRadius.SliceCount,
						this.FPinInRes.SliceCount
					);
			}

		}
	}
}
