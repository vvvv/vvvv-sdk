using System;
using System.Collections.Generic;

namespace VVVV.PluginInterfaces.V2
{
	public interface ISpread<T> : IEnumerable<T>
	{
		T this[int index]
		{
			get;
			set;
		}
		
		int SliceCount
		{
			get;
			set;
		}
		
		void Update();
		
		bool IsChanged
		{
			get;
		}
	}
}
