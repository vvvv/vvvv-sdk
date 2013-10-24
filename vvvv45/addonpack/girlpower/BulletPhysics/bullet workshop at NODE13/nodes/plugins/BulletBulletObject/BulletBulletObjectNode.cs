#region usings
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	// This will be your Custom object
	public class BulletObject : ICloneable
	{
		public double Property;
		public Stopwatch Age;
		
		public object Clone()
		{
			BulletObject o = new BulletObject();
			o.Property = this.Property;
			o.Age = new Stopwatch();
			o.Age.Start();
			return o;
		}
	}
	
	[PluginInfo(Name = "BulletObject", Category = "Bullet",Version="Pack")]
	public class BulletBulletObjectNode : IPluginEvaluate
	{
		[Input("Property", DefaultValue = 1.0)]
		IDiffSpread<double> FInput;

		[Output("Output")]
		ISpread<BulletObject> FOutput;

		public void Evaluate(int SpreadMax)
		{
				FOutput.SliceCount = SpreadMax;
	
				for (int i = 0; i < SpreadMax; i++)
				{
					BulletObject bo = new BulletObject();
					bo.Property = this.FInput[i];
					FOutput[i] = bo;
				}
		}
	}
	
	[PluginInfo(Name = "BulletObject", Category = "Bullet",Version="UnPack", AutoEvaluate = true)]
	public class BulletBulletObjectUnPackNode : IPluginEvaluate
	{
		[Input("Input")]
		IDiffSpread<ICloneable> FInput;
		[Input("Increment")]
		IDiffSpread<double> FIncrement;
		[Input("Add", IsBang = true)]
		IDiffSpread<bool> FBang;

		[Output("Property")]
		ISpread<double> FOutput;
		
		[Output("Age")]
		ISpread<double> FAge;

		public void Evaluate(int SpreadMax)
		{
			if (this.FInput.IsChanged)
			{
				FOutput.SliceCount = SpreadMax;
				FAge.SliceCount = SpreadMax;
	
				for (int i = 0; i < SpreadMax; i++)
				{
					if (this.FInput[i] is BulletObject)
					{
						BulletObject o = (BulletObject)this.FInput[i];
						if(FBang[i]) o.Property += FIncrement[i];
						FOutput[i] = o.Property;
						FAge[i] = (double)o.Age.ElapsedMilliseconds / 1000;
					}
					else
					{
						FOutput[i] = 0;
						FAge[i] = -1;
					}
				}
			}
		}
	}
}
