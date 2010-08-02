using System;
using System.Collections;
using System.Collections.Generic;

using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Input;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class WrapperPin<T> : ISpread<T>
	{
		protected ISpread<T> FSpread;
		
		public T this[int index] 
		{
			get 
			{
				return FSpread[index];
			}
			set 
			{
				FSpread[index] = value;
			}
		}
		
		public int SliceCount 
		{
			get 
			{
				return FSpread.SliceCount;
			}
			set 
			{
				FSpread.SliceCount = value;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return FSpread.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
