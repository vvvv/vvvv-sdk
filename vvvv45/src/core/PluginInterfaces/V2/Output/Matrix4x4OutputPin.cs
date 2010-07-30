
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public class Matrix4x4OutputPin : OutputPin<Matrix4x4>
	{
		protected ITransformOut FTransformOut;
		
		public Matrix4x4OutputPin(IPluginHost host, OutputAttribute attribute)
			:base(attribute)
		{
			host.CreateTransformOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FTransformOut);
		}
		
		public override IPluginOut PluginOut 
		{
			get 
			{
				return FTransformOut;
			}
		}
		
		public override Matrix4x4 this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FTransformOut.SetMatrix(index, value);
			}
		}
	}
}
