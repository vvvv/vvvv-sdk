using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// base class for spread lists
	/// </summary>
	[ComVisible(false)]
	abstract class SpreadList<TSpread> : Spread<TSpread>, IDisposable
	    where TSpread : class, ISynchronizable, IFlushable
	{
	    class SpreadListStream : BufferedIOStream<TSpread>
	    {
            public override bool Sync()
            {
                var isChanged = base.Sync();
                foreach (var spread in this)
                {
                    isChanged |= spread.Sync();
                }
                return isChanged;
            }
            
            public override void Flush()
            {
                foreach (var spread in this)
                {
                    spread.Flush();
                }
                base.Flush();
            }
	    }
	    
		protected readonly IIOFactory FFactory;
		protected readonly IOAttribute FAttribute;
		private readonly List<IIOContainer> FIOHandlers = new List<IIOContainer>();
		protected IDiffSpread<int> FCountSpread;
		protected int FOffsetCounter;
		protected static int FInstanceCounter = 1;
		
		public SpreadList(IIOFactory factory, IOAttribute attribute)
		    : base(new SpreadListStream())
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
					MinValue = 2
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
				var io = FFactory.CreateIOContainer<TSpread>(attribute, false);
				FIOHandlers.Add(io);
			}
			
			for (int i = oldCount - 1; i >= newCount; i--)
			{
				var io = FIOHandlers[i];
				FIOHandlers.Remove(io);
				io.Dispose();
			}
			
			SliceCount = FIOHandlers.Count;
			using (var writer = Stream.GetWriter())
			{
				foreach (var io in FIOHandlers)
				{
					writer.Write(io.RawIOObject as TSpread);
				}
			}
		}
		
		protected abstract IOAttribute CreateAttribute(int position);
	}
}
