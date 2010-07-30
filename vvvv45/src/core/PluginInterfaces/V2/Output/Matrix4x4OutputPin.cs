
using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class Matrix4x4OutputPin : Pin<Matrix4x4>
	{
		protected ITransformOut FTransformOut;
		
		public Matrix4x4OutputPin(IPluginHost host, OutputAttribute attribute)
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
