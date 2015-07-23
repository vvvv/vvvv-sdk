using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins.Input;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "CompoundShape", Category = "Bullet", Version="Spread DX9", Author = "vux")]
	public class BulletCompoundSpreadNode : IPluginEvaluate
	{
		[Input("Shape")]
		ISpread<ISpread<AbstractRigidShapeDefinition>> FShapesIn;

		[Output("Shape")]
		ISpread<AbstractRigidShapeDefinition> FShapesOut;

		public void Evaluate(int SpreadMax)
		{
			//if (((GenericInputPin<AbstractRigidShapeDefinition>)this.FShapesIn).PluginIO.IsConnected)
			if (FShapesIn[0] != null)
			{
				this.FShapesOut.SliceCount = this.FShapesIn.SliceCount;
				for (int i = 0; i < this.FShapesIn.SliceCount; i++)
				{
					List<AbstractRigidShapeDefinition> childs = new List<AbstractRigidShapeDefinition>();
					for (int j = 0; j < this.FShapesIn[i].SliceCount; j++)
					{
						childs.Add(this.FShapesIn[i][j]);
					}
					CompoundShapeDefinition def = new CompoundShapeDefinition(childs);
					this.FShapesOut[i] = def;
				}
			}
			else
			{
				this.FShapesOut.SliceCount = 0;
			}
		}
	}
}

