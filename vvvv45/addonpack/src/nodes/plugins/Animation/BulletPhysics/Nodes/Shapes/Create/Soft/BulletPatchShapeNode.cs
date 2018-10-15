using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;


namespace VVVV.Nodes.Bullet
{
	
	[PluginInfo(Name="Patch", Category="Bullet", Version = "DX9", Author="vux")]
	public class BulletPatchShapeNode : AbstractSoftShapeNode
	{
		//Need to declare 4d toggle
		private IPluginHost FHost;

		[Input("Transform In")]
		IDiffSpread<Matrix4x4> FTransformIn;

		[Input("Generate Diagonals")]
		IDiffSpread<bool> FPinInDiagonals;

		private IValueIn FPinInResolution;
		private IValueIn FPinInFixed;

		[ImportingConstructor()]
		public BulletPatchShapeNode(IPluginHost host)
		{
			this.FHost = host;

			this.FHost.CreateValueInput("Fixed Corners", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInFixed);
			this.FPinInFixed.SetSubType4D(0, 1, 1, 0, 0, 0, 0, false, true, false);

			this.FHost.CreateValueInput("Resolution", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInResolution);
			this.FPinInResolution.SetSubType2D(2, double.MaxValue, 1, 5, 5, false, false, true);
		}

		protected override bool SubPinsChanged
		{
			get
			{
				return this.FTransformIn.IsChanged
				|| this.FPinInResolution.PinIsChanged
				|| this.FPinInDiagonals.IsChanged
				|| this.FPinInFixed.PinIsChanged;
			}
		}

		protected override AbstractSoftShapeDefinition GetShapeDefinition(int slice)
		{
			double resx, resy;

			Matrix4x4 transform = this.FTransformIn[slice];

			Vector3D corner00d = new Vector3D(-1, 1, 0);
			Vector3D corner01d = new Vector3D(1, 1, 0);
			Vector3D corner10d = new Vector3D(-1, -1, 0);
			Vector3D corner11d = new Vector3D(1, -1, 0);

			corner00d = transform * corner00d;
			corner01d = transform * corner01d;
			corner10d = transform * corner10d;
			corner11d = transform * corner11d;

				/*  corner00     -->   +1
	*  corner01     -->   +2
	*  corner10     -->   +4
	*  corner11     -->   +8
	*  upper middle -->  +16
	*  left middle  -->  +32
	*  right middle -->  +64
	*  lower middle --> +128
	*  center       --> +256 */

			double f1, f2, f3, f4;
			this.FPinInFixed.GetValue4D(slice, out f1, out f2, out f3, out f4);

			int fix = 0;
			fix = f1 > 0.5 ? fix + 1 : fix;
			fix = f2 > 0.5 ? fix + 2 : fix;
			fix = f3 > 0.5 ? fix + 4 : fix;
			fix = f4 > 0.5 ? fix + 8 : fix;

			this.FPinInResolution.GetValue2D(slice, out resx, out resy);
			return new PatchSoftShapeDefinition(corner00d.ToBulletVector(), 
				corner10d.ToBulletVector(), 
				corner01d.ToBulletVector(), 
				corner11d.ToBulletVector(), (int)resx, (int)resy, this.FPinInDiagonals[slice], fix);
		}

		protected override int SubPinSpreadMax
		{
			get
			{
				return ArrayMax.Max
					(
						this.FTransformIn.SliceCount,
						this.FPinInDiagonals.SliceCount,
						this.FPinInFixed.SliceCount,
						this.FPinInResolution.SliceCount
					);
			}

		}
	}
}
