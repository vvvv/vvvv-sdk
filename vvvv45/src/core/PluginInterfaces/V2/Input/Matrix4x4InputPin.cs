
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class Matrix4x4InputPin : InputPin<Matrix4x4>
	{
		protected ITransformIn FTransformIn;
		
		public Matrix4x4InputPin(IPluginHost host, InputAttribute attribute)
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
		
		public override Matrix4x4 this[int index] 
		{
			get 
			{
				Matrix4x4 result;
				FTransformIn.GetMatrix(index, out result);
				return result;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
