using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;
using VVVV.DataTypes.Bullet;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	public abstract class AbstractBulletRigidShapeNode : IPluginEvaluate
	{
		#region Pins
		[Input("Position",DefaultValues=new double[] { 0,0,0})]
		protected IDiffSpread<Vector3D> FTranslate;

		[Input("Rotation", DefaultValues = new double[] { 0, 0, 0,1 })]
		protected IDiffSpread<Vector4D> FRotate;

		[Input("Scaling", DefaultValues = new double[] { 1.0, 1.0,1.0 })]
		protected IDiffSpread<Vector3D> FScaling;

		[Input("Mass", DefaultValue=1.0)]
		protected IDiffSpread<float> FMass;

		[Input("Custom")]
		protected IDiffSpread<string> FCustom;

		[Input("Custom Object")]
		protected IDiffSpread<ICloneable> FCustomObj;

		[Output("Shape")]
		protected ISpread<AbstractRigidShapeDefinition> FShapes;
		#endregion

		#region Evaluate

		public abstract void Evaluate(int SpreadMax);

		protected bool BasePinsChanged
		{
			get
			{
				return this.FCustom.IsChanged
					|| this.FCustomObj.IsChanged
					|| this.FMass.IsChanged
					|| this.FRotate.IsChanged
					|| this.FScaling.IsChanged;
			}
		}

		protected int BasePinsSpreadMax
		{
			get
			{
				return ArrayMax.Max(this.FCustom.SliceCount,
					this.FCustomObj.SliceCount,
					this.FMass.SliceCount,
					this.FRotate.SliceCount,
					this.FScaling.SliceCount);
			}
		}
		#endregion

		#region Set Local Transform
		protected void SetBaseParams(AbstractRigidShapeDefinition sd, int sliceindex)
		{
			sd.Translation = this.FTranslate[sliceindex].ToBulletVector();
			sd.Rotation = this.FRotate[sliceindex].ToBulletQuaternion();
			sd.Scaling = this.FScaling[sliceindex].Abs().ToBulletVector();
			sd.CustomString = this.FCustom[sliceindex];
			sd.CustomObject = this.FCustomObj[sliceindex];
		}
		#endregion
	}
}
