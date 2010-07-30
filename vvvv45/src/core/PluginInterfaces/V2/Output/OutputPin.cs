using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class OutputPin<T> : Pin<T>
	{
		public OutputPin(OutputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating output pin {0}.", attribute.Name));
		}
		
		public abstract IPluginOut PluginOut
		{
			get;
		}
		
		public override int SliceCount 
		{
			get 
			{
				return PluginOut.SliceCount;
			}
			set 
			{
				PluginOut.SliceCount = value;
			}
		}
	}
}
