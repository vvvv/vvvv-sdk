using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;

using BulletSharp;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	
	[PluginInfo(Name="Rope", Category="Bullet", Version = "DX9", Author="vux")]
	public class BulletRopeShapeNode : AbstractSoftShapeNode
	{
		//Need to declare 4d toggle
		private IPluginHost FHost;

		[Input("From")]
		IDiffSpread<Vector3D> FFrom;

		[Input("To")]
		IDiffSpread<Vector3D> FTo;

		[Input("Resolution",DefaultValue=5)]
		IDiffSpread<int> FRes;

		private IValueIn FPinInFixed;

		[ImportingConstructor()]
		public BulletRopeShapeNode(IPluginHost host)
		{
			this.FHost = host;

			this.FHost.CreateValueInput("Fixed Edges", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInFixed);
			this.FPinInFixed.SetSubType2D(0, 1, 1, 0, 0, false, true, false);
		}

		protected override bool SubPinsChanged
		{
			get
			{
				return this.FFrom.IsChanged
					|| this.FRes.IsChanged
					|| this.FTo.IsChanged
				|| this.FPinInFixed.PinIsChanged;
			}
		}

		protected override AbstractSoftShapeDefinition GetShapeDefinition(int slice)
		{
			double f1, f2;
			this.FPinInFixed.GetValue2D(slice, out f1, out f2);

			int fix = 0;
			fix = f1 > 0.5 ? fix + 1 : fix;
			fix = f2 > 0.5 ? fix + 2 : fix;

			//this.FPinInResolution.GetValue2D(slice, out resx, out resy);
			return new RopeSoftShapeDefinition(this.FFrom[slice].ToBulletVector(), this.FTo[slice].ToBulletVector(),
				this.FRes[slice], fix);
		}

		protected override int SubPinSpreadMax
		{
			get
			{
				return ArrayMax.Max
					(
						this.FFrom.SliceCount,
						this.FPinInFixed.SliceCount,
						this.FRes.SliceCount,
						this.FTo.SliceCount
					);
			}

		}
	}
}
