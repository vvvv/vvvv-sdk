using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "GetContactPointDetails", Category = "Bullet", Version = "DX9", Author = "vux")]
	public class BulletGetContactPointDetails : IPluginEvaluate
	{
		[Input("Contact Points")]
		Pin<ManifoldPoint> FContactPoints;

		[Output("World Point 1")]
		ISpread<Vector3D> FPointWorld1;

		[Output("World Point 2")]
		ISpread<Vector3D> FPointWorld2;

		[Output("Applied Impulse")]
		ISpread<double> FImpulse;

		[Output("LifeTime")]
		ISpread<int> FLifeTime;


		public void Evaluate(int SpreadMax)
		{
			if (this.FContactPoints.PluginIO.IsConnected)
			{
				this.FPointWorld1.SliceCount = SpreadMax;
				this.FPointWorld2.SliceCount = SpreadMax;
				this.FLifeTime.SliceCount = SpreadMax;
				this.FImpulse.SliceCount = SpreadMax;

				for (int i = 0; i < SpreadMax;i++)
				{
					ManifoldPoint pt = this.FContactPoints[i];

					this.FPointWorld1[i] = pt.PositionWorldOnA.ToVVVVector();
					this.FPointWorld2[i] = pt.PositionWorldOnB.ToVVVVector();
					this.FLifeTime[i] = pt.LifeTime;
					this.FImpulse[i] = pt.AppliedImpulse;	
				}
			}
			else
			{
				this.FPointWorld1.SliceCount = 0;
				this.FPointWorld2.SliceCount = 0;
				this.FLifeTime.SliceCount = 0;
				this.FImpulse.SliceCount = 0;
			}
		}
	}
}
