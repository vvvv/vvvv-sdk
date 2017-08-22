using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;


namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "HeightField", Category = "Bullet", Version = "DX9", Author = "vux")]
	public class BulletHeightFieldShapeNode : AbstractBulletRigidShapeNode
	{
		private IValueIn FPinInResolution;
		private IPluginHost FHost;

		[Input("Data")]
		IDiffSpread<ISpread<float>> FPinInData;


		[Input("Min Height",DefaultValue=-1.0)]
		IDiffSpread<float> FPinInMinH;
		
		[Input("Max Height", DefaultValue = 1.0)]
		IDiffSpread<float> FPinInMaxH;

		[ImportingConstructor()]
		public BulletHeightFieldShapeNode(IPluginHost host)
		{
			this.FHost = host;

			this.FHost.CreateValueInput("Resolution", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInResolution);
			this.FPinInResolution.SetSubType2D(2, double.MaxValue, 1, 5, 5, false, false, true);
		}


		public override void Evaluate(int SpreadMax)
		{
			int spmax = ArrayMax.Max(
				this.BasePinsSpreadMax,
				this.FPinInMaxH.SliceCount,
				this.FPinInMinH.SliceCount,
				this.FPinInResolution.SliceCount);

			if (this.FPinInData.IsChanged
				|| this.FPinInMaxH.IsChanged
				|| this.FPinInMinH.IsChanged
				|| this.FPinInResolution.PinIsChanged
				|| this.BasePinsChanged)
			{
				this.FShapes.SliceCount = spmax;
				for (int i = 0; i < spmax; i++)
				{
					double resx, resz;
					this.FPinInResolution.GetValue2D(0, out resx, out resz);
					int iresx = Convert.ToInt32(resx);
					int iresz = Convert.ToInt32(resz);

					float[] data = new float[iresx * iresz];
					for (int j = 0; j < iresx * iresz; j++)
					{
						data[j] = this.FPinInData[i][j];
					}

					HeightFieldShapeDefinition hf;
					hf = new HeightFieldShapeDefinition(iresx, iresz, data, this.FPinInMinH[i], this.FPinInMaxH[i]);

					this.SetBaseParams(hf, i);

					this.FShapes[i] = hf;

				}
			}
		}
	}
}
