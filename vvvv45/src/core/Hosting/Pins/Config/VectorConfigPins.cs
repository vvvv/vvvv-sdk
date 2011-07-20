using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public abstract class VectorConfigPin<T> : DiffVectorPin<T> where T: struct
	{
		protected IValueConfig FValueConfig;
		
		public VectorConfigPin(IPluginHost host, ConfigAttribute attribute, int dimension, double minValue, double maxValue, double stepSize)
			: base(host, attribute, dimension, minValue, maxValue, stepSize)
		{
			host.CreateValueConfig(FName, FDimension, FDimensionNames, FSliceMode, FVisibility, out FValueConfig);
			switch (FDimension)
			{
				case 2:
					FValueConfig.SetSubType2D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FIsBang, FIsToggle, FIsInteger);
					break;
				case 3:
					FValueConfig.SetSubType3D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FIsBang, FIsToggle, FIsInteger);
					break;
				case 4:
					FValueConfig.SetSubType4D(FMinValue, FMaxValue, FStepSize, FDefaultValues[0], FDefaultValues[1], FDefaultValues[2], FDefaultValues[3], FIsBang, FIsToggle, FIsInteger);
					break;
			}
			
			base.InitializeInternalPin(FValueConfig);
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FValueConfig.SliceCount;
			}
			set 
			{
				base.SliceCount = value;
				
				if (FAttribute.SliceMode != SliceMode.Single)
					FValueConfig.SliceCount = FSliceCount;
			}
		}
		
		public override bool IsChanged 
        {
            get 
            { 
                return FValueConfig.PinIsChanged; 
            }
        }
		
		// Only called by ConfigurateCB
		protected override void DoUpdate()
		{
			// Config pins read from internal pin directly
		}
		
		protected override bool IsInternalPinChanged
		{
			get 
			{
				return FValueConfig.PinIsChanged;
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector2DConfigPin : VectorConfigPin<Vector2D>
	{
		public Vector2DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, 2, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		public override Vector2D this[int index] 
		{
			get
			{
				double value1, value2;
				FValueConfig.GetValue2D(index, out value1, out value2);
				return new Vector2D(value1, value2);
			}
			set
			{
				FValueConfig.SetValue2D(index, value.x, value.y);
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector3DConfigPin : VectorConfigPin<Vector3D>
	{
		public Vector3DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, 3, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		public override Vector3D this[int index] 
		{
			get
			{
				double value1, value2, value3;
				FValueConfig.GetValue3D(index, out value1, out value2, out value3);
				return new Vector3D(value1, value2, value3);
			}
			set
			{
				FValueConfig.SetValue3D(index, value.x, value.y, value.z);
			}
		}
	}
		
	[ComVisible(false)]
	public class Vector4DConfigPin : VectorConfigPin<Vector4D>
	{
		public Vector4DConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, 4, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		public override Vector4D this[int index] 
		{
			get
			{
				double value1, value2, value3, value4;
				FValueConfig.GetValue4D(index, out value1, out value2, out value3, out value4);
				return new Vector4D(value1, value2, value3, value4);
			}
			set
			{
				FValueConfig.SetValue4D(index, value.x, value.y, value.z, value.w);
			}
		}
	}
}
