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
		protected Pin<T> FPin;
		
		public Pin<T> Pin
		{
			get
			{
				return FPin;
			}
		}
		
		public T this[int index]
		{
			get
			{
				return FPin[index];
			}
			set
			{
				FPin[index] = value;
			}
		}
		
		public int SliceCount
		{
			get
			{
				return FPin.SliceCount;
			}
			set
			{
				FPin.SliceCount = value;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return FPin.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
