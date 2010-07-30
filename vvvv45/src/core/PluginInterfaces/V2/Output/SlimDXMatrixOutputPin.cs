using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class SlimDXMatrixOutputPin : Pin<Matrix>
	{
		protected ITransformOut FTransformOut;
		
		public SlimDXMatrixOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateTransformOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FTransformOut);
		}
		
		public override int SliceCount 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FTransformOut.SliceCount = value;
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
