﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Bullet;
using VVVV.Utils.VMath;
using BulletSharp;
using VVVV.Bullet.DataTypes.Shapes.Rigid;

namespace VVVV.Bullet.Nodes.Shapes.Create.Rigid
{
    [PluginInfo(Name = "Bvh", Category = "Bullet", Author = "vux")]
    public class BulletBvhShapeNode : AbstractBulletRigidShapeNode
    {
        [Input("Vertices")]
        ISpread<ISpread<Vector3D>> FVertices;

        [Input("Indices")]
        ISpread<ISpread<int>> FIndices;

        [Input("Apply",IsBang=true)]
        ISpread<bool> FApply;

        public override void Evaluate(int SpreadMax)
        {
            int spmax = ArrayMax.Max(FVertices.SliceCount,
                FIndices.SliceCount,
                this.BasePinsSpreadMax);

            if (this.FApply[0])
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

                    BvhShapeDefinition chull = new BvhShapeDefinition(vertices, inds);
                    chull.Mass = this.FMass[i];
                    this.SetBaseParams(chull, i);

                    this.FShapes[i] = chull;
                }
            }
        }
    }
}
