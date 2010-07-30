using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class SlimDXMatrixOutputPin : OutputPin<Matrix>
	{
		protected ITransformOut FTransformOut;
		
		public SlimDXMatrixOutputPin(IPluginHost host, OutputAttribute attribute)
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
		
		public override Matrix this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FTransformOut.SetMatrix(index, value.ToMatrix4x4());
			}
		}
	}
}
