using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp.SoftBody;
using VVVV.Utils.VMath;
using VVVV.Internals.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="GetSoftBodyDetails", Category="Bullet", Version = "DX9", 
		Help = "Gets some info about a soft body", Author = "vux")]
	public class BulletGetSoftBodyDetailsNode : IPluginEvaluate
	{
		[Input("Bodies")]
		ISpread<SoftBody> FBodies;

		[Output("Nodes")]
		ISpread<ISpread<Vector3D>> FOutNodes;

		[Output("Mass")]
		ISpread<float> FOutMass;

		[Output("Custom")]
		ISpread<string> FOutCustom;

		[Output("Has Custom Object")]
		ISpread<bool> FHasCustomObj;

		[Output("Custom Object")]
		ISpread<ICloneable> FCustomObj;

		[Output("Id")]
		ISpread<int> FOutId;
	
		public void  Evaluate(int SpreadMax)
		{
			this.FOutNodes.SliceCount = this.FBodies.SliceCount;
			this.FOutCustom.SliceCount = SpreadMax;
			this.FOutId.SliceCount = SpreadMax;
			this.FOutMass.SliceCount = SpreadMax;
			
			for (int i = 0; i < SpreadMax; i++)
			{
				
				SoftBody sb = this.FBodies[i];
				this.FOutNodes[i].SliceCount = sb.Nodes.Count;
                //sb.Nodes[0].

				for (int j = 0; j < sb.Nodes.Count; j++)
				{
					this.FOutNodes[i][j] =sb.Nodes[j].X.ToVVVVector();
				}

				this.FOutMass[i] = sb.TotalMass;

				SoftBodyCustomData custom = (SoftBodyCustomData)sb.UserObject;
				this.FOutCustom[i] = custom.Custom;
				this.FOutId[i] = custom.Id;
				
				if (custom.CustomObject != null)
				{
					this.FHasCustomObj[i] = true;
					this.FCustomObj[i] = custom.CustomObject;
				}
				else
				{
					this.FHasCustomObj[i] = false;
					this.FCustomObj[i] = null;
				}
			}
		}
	}
}

