
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class Matrix4x4InputPin : ObservablePin<Matrix4x4>
	{
		protected ITransformIn FTransformIn;
		
		public Matrix4x4InputPin(IPluginHost host, InputAttribute attribute)
		{
			host.CreateTransformInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FTransformIn);
		}
		
		public override bool IsChanged 
		{
			get 
			{
				return FTransformIn.PinIsChanged;
			}
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FTransformIn.SliceCount;
			}
			set 
			{
				throw new NotImplementedException();
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
