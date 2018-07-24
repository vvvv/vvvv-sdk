using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Input;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "CompoundShape", Category = "Bullet", Version="Group DX9", Author = "vux")]
	public class BulletCompoundShapeGroupNode : IPluginEvaluate
	{
		[Input("Shape",IsPinGroup=true)]
		ISpread<ISpread<AbstractRigidShapeDefinition>> FShapesIn;

		[Output("Shape")]
		ISpread<AbstractRigidShapeDefinition> FShapesOut;

		public void Evaluate(int SpreadMax)
		{
			//int i = this.FShapesIn.SliceCount;
//			bool allconnected = true;
//			int maxslice = int.MinValue;
//			for (int i = 0; i < this.FShapesIn.SliceCount; i++)
//			{
//				if (((InputPin<AbstractRigidShapeDefinition>)this.FShapesIn[0]).PluginIO.IsConnected)
//				{
//					maxslice = this.FShapesIn[i].SliceCount > maxslice ? this.FShapesIn[i].SliceCount : maxslice;
//					if (this.FShapesIn[i].SliceCount == 0) { allconnected = false; }
//				}
//				else
//				{
//					allconnected = false;
//				}
//			}
			this.FShapesOut.SliceCount = FShapesIn.GetMaxSliceCount();

//			if (allconnected)
//			{
//				this.FShapesOut.SliceCount = maxslice;

				for (int i = 0; i < this.FShapesOut.SliceCount; i++)
				{
					List<AbstractRigidShapeDefinition> childs = new List<AbstractRigidShapeDefinition>();

					for (int j = 0; j < this.FShapesIn.SliceCount; j++)
					{

						childs.Add(this.FShapesIn[j][i]);
						//childs.Add(this.FShapes2[i]);
					}

					CompoundShapeDefinition def = new CompoundShapeDefinition(childs);
					this.FShapesOut[i] = def;
				}
//			}
//			else
//			{
//				this.FShapesOut.SliceCount = 0;
//			}
		}
	}
}
