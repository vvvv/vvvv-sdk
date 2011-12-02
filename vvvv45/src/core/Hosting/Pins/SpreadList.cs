using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Streams;
using VVVV.Hosting.Streams.Registry;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// base class for spread lists
	/// </summary>
	[ComVisible(false)]
	public abstract class SpreadList<T> : Spread<ISpread<T>>, IDisposable
	{
		protected readonly IOFactory FFactory;
		protected readonly IOAttribute FAttribute;
		private readonly List<IOHandler> FIOHandlers = new List<IOHandler>();
		protected IDiffSpread<int> FCountSpread;
		protected int FOffsetCounter;
		protected static int FInstanceCounter = 1;
		
		public SpreadList(IOFactory factory, IOAttribute attribute)
			: base(0)
		{
			//store fields
			FFactory = factory;
			FAttribute = attribute;
			
			//increment instance Counter and store it as pin offset
			FOffsetCounter = FInstanceCounter++;
			
			//create config pin
			FCountSpread = factory.CreateIO<IDiffSpread<int>>(
				new ConfigAttribute(FAttribute.Name + " Count")
				{
					DefaultValue = 2,
					MinValue = 0
				}
			);
			FCountSpread.Changed += HandleCountSpreadChanged;
			FCountSpread.Sync();
		}
		
		public virtual void Dispose()
		{
			FCountSpread.Changed -= HandleCountSpreadChanged;
			SliceCount = 0;
		}
		
		//pin management
		void HandleCountSpreadChanged(IDiffSpread<int> spread)
		{
			int oldCount = FIOHandlers.Count;
			int newCount = Math.Max(spread[0], 0);
			
			for (int i = oldCount; i < newCount; i++)
			{
				var attribute = CreateAttribute(i + 1);
				attribute.IsPinGroup = false;
				attribute.Order = FAttribute.Order + FOffsetCounter * 1000 + i;
				var io = FFactory.CreateIOHandler<ISpread<T>>(attribute);
				FIOHandlers.Add(io);
			}
			
			for (int i = oldCount - 1; i >= newCount; i--)
			{
				var io = FIOHandlers[i];
				FFactory.DestroyIOHandler(io);
				FIOHandlers.Remove(io);
			}
			
			SliceCount = FIOHandlers.Count;
			using (var writer = Stream.GetWriter())
			{
				foreach (var io in FIOHandlers)
				{
					writer.Write(io.RawIOObject as ISpread<T>);
				}
			}
		}
		
		public override bool Sync()
		{
			var changed = base.Sync();
			foreach (var spread in Stream)
			{
				changed |= spread.Sync();
			}
			return changed;
		}
		
		public override void Flush()
		{
			foreach (var spread in Stream)
			{
				spread.Flush();
			}
			base.Flush();
		}
		
		protected abstract IOAttribute CreateAttribute(int position);
	}
}
