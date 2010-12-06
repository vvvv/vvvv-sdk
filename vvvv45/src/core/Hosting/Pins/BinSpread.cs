using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// Base class of 2d spreads.
	/// </summary>
	public abstract class BinSpread<T> : Spread<ISpread<T>>
	{
		protected bool FLazy;
		
		public BinSpread(PinAttribute attribute)
			: base(1)
		{
			FLazy = attribute.Lazy;
			BufferIncreased(new ISpread<T>[0], FBuffer);
		}
		
		protected override void BufferIncreased(ISpread<T>[] oldBuffer, ISpread<T>[] newBuffer)
		{
			for (int i = 0; i < oldBuffer.Length; i++)
				newBuffer[i] = oldBuffer[i];
			
			for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
				newBuffer[i] = new Spread<T>(0);
		}
	}
}
