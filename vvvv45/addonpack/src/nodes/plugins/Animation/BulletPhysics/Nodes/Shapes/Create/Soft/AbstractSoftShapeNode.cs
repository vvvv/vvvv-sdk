using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp.SoftBody;
using VVVV.Utils.VColor;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	public abstract class AbstractSoftShapeNode : IPluginEvaluate
	{
		[Input("Aero Model",DefaultEnumEntry="VPoint")]
		IDiffSpread<AeroModel> FPinInAeroModel;

		[Input("Is Volume Mass", DefaultValue = 0.0)]
		IDiffSpread<bool> FPinInIsVolumeMass;

		[Input("Mass", DefaultValue=1.0)]
		IDiffSpread<float> FPinInMass;

		[Input("Damping Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInDP;

		[Input("Drag Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInDG;

		[Input("Dynamic Friction Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInDF;

		[Input("Pressure Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInPR;

		[Input("Volume Conservation Coefficient", DefaultValue = 0.0)]
		IDiffSpread<float> FPinInVC;

		[Input("Lift Coefficient", DefaultValue = 1.0)]
		IDiffSpread<float> FPinInLF;

		[Input("Rigid Contact Hardness", DefaultValue = 1.0)]
		IDiffSpread<float> FPinInCHR;

		[Input("Soft Contact Hardness", DefaultValue = 1.0)]
		IDiffSpread<float> FPinInSHR;

		[Input("Anchor Hardness", DefaultValue = 0.4)]
		IDiffSpread<float> FPinInAHR;

		[Input("Generate Bending Constraints", DefaultValue = 0.0)]
		IDiffSpread<bool> FPinInGenBend;

		[Input("Bending Constraints Distance", DefaultValue = 1.0)]
		IDiffSpread<int> FPinInBendDist;

		[Output("Shape")]
		ISpread<AbstractSoftShapeDefinition> FPinOutShapes;

		protected abstract bool SubPinsChanged { get; }
		protected abstract AbstractSoftShapeDefinition GetShapeDefinition(int slice);
		protected abstract int SubPinSpreadMax { get; }

		public void Evaluate(int SpreadMax)
		{
			if (this.FPinInMass.IsChanged
				|| this.FPinInDF.IsChanged
				|| this.FPinInDP.IsChanged
				|| this.FPinInPR.IsChanged
				|| this.FPinInAeroModel.IsChanged
				|| this.FPinInBendDist.IsChanged
				|| this.FPinInGenBend.IsChanged
				|| this.FPinInCHR.IsChanged
				|| this.FPinInDG.IsChanged
				|| this.FPinInAHR.IsChanged
				|| this.SubPinsChanged
				|| this.FPinInIsVolumeMass.IsChanged
				|| this.FPinInSHR.IsChanged
				|| this.FPinInVC.IsChanged)
			{
				this.FPinOutShapes.SliceCount =
					ArrayMax.Max(
					this.FPinInAeroModel.SliceCount,
					this.FPinInBendDist.SliceCount,
					this.FPinInCHR.SliceCount,
					this.FPinInDF.SliceCount,
					this.FPinInDP.SliceCount,
					this.FPinInGenBend.SliceCount,
					this.FPinInLF.SliceCount,
					this.FPinInMass.SliceCount,
					this.FPinInPR.SliceCount,
					this.FPinInDG.SliceCount,
					this.FPinInAHR.SliceCount,
					this.FPinInIsVolumeMass.SliceCount,
					this.FPinInSHR.SliceCount,
				    this.FPinInVC.SliceCount,
					this.SubPinSpreadMax
					);

				for (int i = 0; i < SpreadMax; i++)
				{
					AbstractSoftShapeDefinition shape = this.GetShapeDefinition(i);
					shape.Mass = this.FPinInMass[i];
					shape.DampingCoefficient = this.FPinInDP[i];
					shape.DynamicFrictionCoefficient = this.FPinInDF[i];
					shape.PressureCoefficient = this.FPinInPR[i];
					shape.LiftCoefficient = this.FPinInLF[i];
					shape.AeroModel = this.FPinInAeroModel[i];
					shape.GenerateBendingConstraints = this.FPinInGenBend[i];
					shape.BendingDistance = this.FPinInBendDist[i];
					shape.RigidContactHardness = this.FPinInCHR[i];
					shape.DragCoefficient = this.FPinInDG[i];
					shape.AnchorHardness = this.FPinInAHR[i];
					shape.IsVolumeMass = this.FPinInIsVolumeMass[i];
					shape.SoftContactHardness = this.FPinInSHR[i];
					shape.VolumeConservation = this.FPinInVC[i];
					this.FPinOutShapes[i] = shape;
				}
			}
			
		}
	}
}
