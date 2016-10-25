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
	abstract class SpreadList<TSpread> : Spread<TSpread>, IIOMultiPin, IDisposable 
	    where TSpread : class, ISynchronizable, IFlushable
	{
	    class SpreadListStream : MemoryIOStream<TSpread>
	    {
            public override bool Sync()
            {
                IsChanged = false;
                foreach (var spread in this)
                {
                    IsChanged |= spread.Sync();
                }
                return base.Sync();
            }

            public override void Flush(bool force = false)
            {
                foreach (var spread in this)
                {
                    spread.Flush(force);
                }
                base.Flush(force);
            }
	    }
	    
		protected readonly IIOFactory FFactory;
		protected readonly IOAttribute FAttribute;
		private readonly List<IIOContainer> FIOContainers = new List<IIOContainer>();
		protected IDiffSpread<int> FCountSpread;
		protected int FOffsetCounter;
		protected static int FInstanceCounter = 1;
        private bool FForceOnNextFlush;

        public IIOContainer BaseContainer
        {
            get
            {
                return FCountSpread as IIOContainer;
            }
        }

        public IIOContainer[] AssociatedContainers
        {
            get
            {
                return FIOContainers.ToArray();
            }
        }

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
		    foreach (var container in FIOContainers)
		    {
		        container.Dispose();
		    }
		    FIOContainers.Clear();
		}
		
		//pin management
		void HandleCountSpreadChanged(IDiffSpread<int> spread)
		{
			int oldCount = FIOContainers.Count;
			int newCount = Math.Max(spread[0], 0);
			
			for (int i = oldCount; i < newCount; i++)
			{
				var attribute = CreateAttribute(i + 1);
				attribute.IsPinGroup = false;
                var order = FAttribute.Order + FOffsetCounter * 1000 + 2 * i;
                attribute.Order = order;
                attribute.BinOrder = order + 1;
				var io = FFactory.CreateIOContainer<TSpread>(attribute, false);
				FIOContainers.Add(io);
			}
			
			for (int i = oldCount - 1; i >= newCount; i--)
			{
				var io = FIOContainers[i];
				FIOContainers.Remove(io);
				io.Dispose();
			}
			
			SliceCount = FIOContainers.Count;
			using (var writer = Stream.GetWriter())
			{
				foreach (var io in FIOContainers)
				{
					writer.Write(io.RawIOObject as TSpread);
				}
            }

            FForceOnNextFlush = true;
		}

        public override void Flush(bool force = false)
        {
            force |= FForceOnNextFlush;
            FForceOnNextFlush = false;
            base.Flush(force);
        }
		
		protected abstract IOAttribute CreateAttribute(int position);
	}
}
