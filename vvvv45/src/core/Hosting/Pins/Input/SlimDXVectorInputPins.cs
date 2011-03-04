using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class Vector2InputPin : VectorInputPin<Vector2>
	{
		public Vector2InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 2, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector2[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector2* destination = buffer)
			{
				Vector2* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / FDimension; i++)
				{
					dst->X = (float) *src;
					src++;
					dst->Y = (float) *src;
					src++;
					dst++;
				}
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->X = (float) *(source + i++ % length);
					dst->Y = (float) *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector3InputPin : VectorInputPin<Vector3>
	{
		public Vector3InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 3, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector3[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector3* destination = buffer)
			{
				Vector3* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / FDimension; i++)
				{
					dst->X = (float) *src;
					src++;
					dst->Y = (float) *src;
					src++;
					dst->Z = (float) *src;
					src++;
					dst++;
				}
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->X = (float) *(source + i++ % length);
					dst->Y = (float) *(source + i++ % length);
					dst->Z = (float) *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class Vector4InputPin : VectorInputPin<Vector4>
	{
		public Vector4InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 4, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector4[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector4* destination = buffer)
			{
				Vector4* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / FDimension; i++)
				{
					dst->X = (float) *src;
					src++;
					dst->Y = (float) *src;
					src++;
					dst->Z = (float) *src;
					src++;
					dst->W = (float) *src;
					src++;
					dst++;
				}
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->X = (float) *(source + i++ % length);
					dst->Y = (float) *(source + i++ % length);
					dst->Z = (float) *(source + i++ % length);
					dst->W = (float) *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffVector2InputPin : DiffVectorInputPin<Vector2>
	{
		public DiffVector2InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 2, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector2[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector2* destination = buffer)
			{
				Vector2* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / FDimension; i++)
				{
					dst->X = (float) *src;
					src++;
					dst->Y = (float) *src;
					src++;
					dst++;
				}
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->X = (float) *(source + i++ % length);
					dst->Y = (float) *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffVector3InputPin : DiffVectorInputPin<Vector3>
	{
		public DiffVector3InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 3, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector3[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector3* destination = buffer)
			{
				Vector3* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / FDimension; i++)
				{
					dst->X = (float) *src;
					src++;
					dst->Y = (float) *src;
					src++;
					dst->Z = (float) *src;
					src++;
					dst++;
				}
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->X = (float) *(source + i++ % length);
					dst->Y = (float) *(source + i++ % length);
					dst->Z = (float) *(source + i++ % length);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffVector4InputPin : DiffVectorInputPin<Vector4>
	{
		public DiffVector4InputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 4, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(Vector4[] buffer, double* source, int length, int underFlow)
		{
			fixed (Vector4* destination = buffer)
			{
				Vector4* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / FDimension; i++)
				{
					dst->X = (float) *src;
					src++;
					dst->Y = (float) *src;
					src++;
					dst->Z = (float) *src;
					src++;
					dst->W = (float) *src;
					src++;
					dst++;
				}
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->X = (float) *(source + i++ % length);
					dst->Y = (float) *(source + i++ % length);
					dst->Z = (float) *(source + i++ % length);
					dst->W = (float) *(source + i++ % length);
				}
			}
		}
	}
}
