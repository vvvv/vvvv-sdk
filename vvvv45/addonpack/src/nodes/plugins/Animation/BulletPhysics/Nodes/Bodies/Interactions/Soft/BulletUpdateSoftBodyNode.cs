using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp.SoftBody;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "UpdateSoftBodyConfig", Category = "Bullet", Version = "DX9", Author = "vux",
		Help = "Updates soft body settings", AutoEvaluate = true)]
	public class BulletUpdateSoftBodyNode : IPluginEvaluate
	{
		[Input("Bodies", Order = 0)]
		ISpread<SoftBody> FInput;

		[Input("Damping Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInDP;

		[Input("Update Damping Coefficient", IsBang = true)]
		ISpread<bool> FPinInUpdateDP;

		[Input("Drag Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInDG;

		[Input("Drag Damping Coefficient", IsBang = true)]
		ISpread<bool> FPinInUpdateDG;

		[Input("Dynamic Friction Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInDF;

		[Input("Update Dynamic Friction Coefficient", IsBang = true)]
		ISpread<bool> FPinInUpdateDF;

		[Input("Pressure Coefficient", DefaultValue = 0.0)]
		ISpread<float> FPinInPR;

		[Input("Update Pressure Coefficient", IsBang=true)]
		ISpread<bool> FPinInUpdatePR;

		[Input("Lift Coefficient", DefaultValue = 1.0)]
		IDiffSpread<float> FPinInLF;

		[Input("Update Lift Coefficient", IsBang = true)]
		ISpread<bool> FPinInUpdateLF;

		[Input("Rigid Constat Hardness", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInCHR;

		[Input("Update Rigid Constat Hardness", IsBang = true)]
		ISpread<bool> FPinInUpdateCHR;

		[Input("Anchor Hardness", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInAHR;

		[Input("Update Anchor Hardness", IsBang = true)]
		ISpread<bool> FPinInUpdateAHR;


		public void Evaluate(int SpreadMax)
		{
			for (int i = 0; i < SpreadMax; i++)
			{
				SoftBody sb = this.FInput[i];

				if (this.FPinInUpdateDP[i]) { sb.Cfg.DP = this.FPinInDP[i]; }
				if (this.FPinInUpdateDF[i]) { sb.Cfg.DF = this.FPinInDF[i]; }
				if (this.FPinInUpdatePR[i]) { sb.Cfg.PR = this.FPinInPR[i]; }
				if (this.FPinInUpdateLF[i]) { sb.Cfg.LF = this.FPinInLF[i]; }
				if (this.FPinInUpdateCHR[i]) { sb.Cfg.Chr = this.FPinInCHR[i]; }
				if (this.FPinInUpdateAHR[i]) { sb.Cfg.Ahr = this.FPinInCHR[i]; }
				if (this.FPinInUpdateDG[i]) { sb.Cfg.DG = this.FPinInDG[i]; }
			}
		}
	}
}
