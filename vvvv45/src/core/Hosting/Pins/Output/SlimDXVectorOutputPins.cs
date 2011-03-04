using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class Vector2OutputPin : VectorOutputPin<Vector2>
	{
		public Vector2OutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, 2, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(Vector2[] buffer, double* dst, int length)
		{
			fixed (Vector2* source = buffer)
			{
				Vector2* src = source;
				for (int i = 0; i < length / FDimension; i++)
				{
					*dst = (double) src->X;
					dst++;
					*dst = (double) src->Y;
					dst++;
					src++;
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector3OutputPin : VectorOutputPin<Vector3>
	{
		public Vector3OutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, 3, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(Vector3[] buffer, double* dst, int length)
		{
			fixed (Vector3* source = buffer)
			{
				Vector3* src = source;
				for (int i = 0; i < length / FDimension; i++)
				{
					*dst = (double) src->X;
					dst++;
					*dst = (double) src->Y;
					dst++;
					*dst = (double) src->Z;
					dst++;
					src++;
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector4OutputPin : VectorOutputPin<Vector4>
	{
		public Vector4OutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, 4, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(Vector4[] buffer, double* dst, int length)
		{
			fixed (Vector4* source = buffer)
			{
				Vector4* src = source;
				for (int i = 0; i < length / FDimension; i++)
				{
					*dst = (double) src->X;
					dst++;
					*dst = (double) src->Y;
					dst++;
					*dst = (double) src->Z;
					dst++;
					*dst = (double) src->W;
					dst++;
					src++;
				}
			}
		}
	}
}
