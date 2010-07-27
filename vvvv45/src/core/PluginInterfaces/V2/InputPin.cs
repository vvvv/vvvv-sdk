using System;
using System.Collections;
using System.Collections.Generic;

namespace VVVV.PluginInterfaces.V2
{
	abstract class InputPin<T> : ISpread<T>
	{
		protected T[] FData;
		
		public InputPin()
		{
			FData = new T[0];
		}
		
		public abstract T this[int index] 
		{
			get;
			set;
		}
		
		public abstract int SliceCount 
		{
			get;
			set;
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
