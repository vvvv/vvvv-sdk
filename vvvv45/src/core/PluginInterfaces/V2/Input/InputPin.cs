using System;
using System.Diagnostics;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class InputPin<T> : Pin<T>
	{
		public InputPin(InputAttribute attribute)
		{
			Debug.WriteLine(string.Format("Creating input pin {0}.", attribute.Name));
		}
		
		public abstract IPluginIn PluginIn
		{
			get;
		}
		
		public override int SliceCount 
		{
			get 
			{
				return PluginIn.SliceCount;
			}
			set {
				throw new NotImplementedException();
			}
		}
	}
}
