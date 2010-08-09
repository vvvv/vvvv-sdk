using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Config;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// base class for spread lists
	/// </summary>
	public abstract class BinSpread<T> : ISpread<ISpread<T>>
	{
		protected IPluginHost FHost;
		protected PinAttribute FAttribute;
		
		public BinSpread(IPluginHost host, PinAttribute attribute)
		{
			//store fields
			FHost = host;
			FAttribute = attribute; 
		}
		
		public abstract ISpread<T> this[int index]
		{
			get;
			set;
		}
		
		public abstract int SliceCount
		{
			get;
			set;
		}

		public IEnumerator<ISpread<T>> GetEnumerator()
		{
			return null;
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
