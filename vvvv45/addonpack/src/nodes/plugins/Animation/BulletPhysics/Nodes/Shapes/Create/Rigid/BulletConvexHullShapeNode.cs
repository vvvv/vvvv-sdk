using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using BulletSharp;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="ConvexHull", Category="Bullet", Version = "DX9", Author="vux")]
	public class BulletConvexHullShapeNode : AbstractBulletRigidShapeNode
	{
		[Input("Vertices")]
		IDiffSpread<ISpread<Vector3D>> FVertices;

		[Input("Indices")]
		IDiffSpread<ISpread<int>> FIndices;

		public override void Evaluate(int SpreadMax)
		{
			int spmax = ArrayMax.Max(FVertices.SliceCount,
				FIndices.SliceCount,
				this.BasePinsSpreadMax);

			if (this.FVertices.IsChanged
				|| this.FIndices.IsChanged
				|| this.BasePinsChanged)
			{
				this.FShapes.SliceCount = spmax;

				for (int i = 0; i < spmax; i++)
				{
					//Vector3D size = this.FSize[i];
					Vector3[] vertices = new Vector3[this.FVertices[i].SliceCount];
					int[] inds = new int[this.FIndices[i].SliceCount];

					for (int j = 0; j < this.FVertices[i].SliceCount; j++)
					{
						vertices[j] = this.FVertices[i][j].ToBulletVector();
					}

					for (int j = 0; j < this.FIndices[i].SliceCount; j++)
					{
						inds[j] = Convert.ToInt16(this.FIndices[i][j]);
					}

					ConvexHullShapeDefinition chull = new ConvexHullShapeDefinition(vertices, inds);
					chull.Mass = this.FMass[i];
					this.SetBaseParams(chull, i);

					this.FShapes[i] = chull;
				}
			}
		}
	}
}

