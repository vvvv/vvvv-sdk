using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "ConvexHull", Category = "Bullet", Version="SoftShape DX9", Author = "vux")]
	public class BulletConvexHullSoftShape : AbstractSoftShapeNode
	{
		[Input("Vertices")]
		IDiffSpread<ISpread<Vector3D>> FVertices;

		[Input("Randomize Contraints")]
		IDiffSpread<bool> FRandomize;
		

		protected override bool SubPinsChanged
		{
			get
			{
				return this.FVertices.IsChanged
					|| this.FRandomize.IsChanged;
			}
		}

		protected override AbstractSoftShapeDefinition GetShapeDefinition(int slice)
		{
			Vector3[] verts = new Vector3[this.FVertices[slice].SliceCount];
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = this.FVertices[slice][i].ToBulletVector();
			}

			return new ConvexHullSoftShapeDefinition(verts,this.FRandomize[slice]);
		}

		protected override int SubPinSpreadMax
		{
			get
			{
				return ArrayMax.Max
					(
						this.FRandomize.SliceCount,
						this.FVertices.SliceCount
					);
			}

		}
	}
}
