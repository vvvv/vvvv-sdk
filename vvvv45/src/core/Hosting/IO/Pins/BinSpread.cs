using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// Base class of 2d spreads.
	/// </summary>
	[ComVisible(false)]
	public abstract class BinSpread<T> : Spread<ISpread<T>>, IIOMultiPin
	{
		public class BinSpreadStream : MemoryIOStream<ISpread<T>>
		{
			protected override void BufferIncreased(ISpread<T>[] oldBuffer, ISpread<T>[] newBuffer)
			{
			    if (oldBuffer != null && oldBuffer.Length > 0)
			    {
			        Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
					for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
					{
						var spread = oldBuffer[i & (oldBuffer.Length - 1)];
						if (spread != null)
						    newBuffer[i] = (Spread<T>) spread.Clone();
						else
							newBuffer[i] = new Spread<T>(0);
					}
			    }
				else
				{
					for (int i = 0; i < newBuffer.Length; i++)
					{
						newBuffer[i] = new Spread<T>(0);
					}
				}
			}
		}
		
		protected readonly IIOFactory FIOFactory;

        public abstract IIOContainer BaseContainer { get; }

        public abstract IIOContainer[] AssociatedContainers { get; }

        public BinSpread(IIOFactory ioFactory, IOAttribute attribute, BinSpreadStream stream)
			: base(stream)
		{
			FIOFactory = ioFactory;
		}
	}
}
