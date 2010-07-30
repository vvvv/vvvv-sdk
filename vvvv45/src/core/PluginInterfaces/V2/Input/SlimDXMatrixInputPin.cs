using System;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public class SlimDXMatrixInputPin : InputPin<Matrix>
	{
		protected ITransformIn FTransformIn;
		
		public SlimDXMatrixInputPin(IPluginHost host, InputAttribute attribute)
			:base(attribute)
		{
			host.CreateTransformInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FTransformIn);
		}
		
		public override IPluginIn PluginIn 
		{
			get 
			{
				return FTransformIn;
			}
		}
		
		public override Matrix this[int index] 
		{
			get 
			{
				Matrix4x4 result;
				FTransformIn.GetMatrix(index, out result);
				return result.ToSlimDXMatrix();
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
