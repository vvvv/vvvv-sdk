using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
	class GroupInStream<T> : IInStream<IInStream<T>>//, IDisposable
	{
		private readonly MemoryIOStream<IInStream<T>> FStreams = new MemoryIOStream<IInStream<T>>(2);
		private readonly List<IIOContainer> FIOContainers = new List<IIOContainer>();
		private readonly IDiffSpread<int> FCountSpread;
		private readonly IIOFactory FFactory;
		private readonly InputAttribute FInputAttribute;
		private readonly int FOffsetCounter;
		private static int FInstanceCounter = 1;
		
		public GroupInStream(IIOFactory factory, InputAttribute attribute)
		{
			FFactory = factory;
			FInputAttribute = attribute;
			//increment instance Counter and store it as pin offset
			FOffsetCounter = FInstanceCounter++;
			
			FCountSpread = factory.CreateIO<IDiffSpread<int>>(
				new ConfigAttribute(FInputAttribute.Name + " Count")
				{
					DefaultValue = 2,
					MinValue = 2
				}
			);
			
			FCountSpread.Changed += HandleCountSpreadChanged;
			FCountSpread.Sync();
		}

		void HandleCountSpreadChanged(IDiffSpread<int> spread)
		{
			int oldCount = FIOContainers.Count;
			int newCount = Math.Max(spread[0], 0);
			
			for (int i = oldCount; i < newCount; i++)
			{
				var attribute = new InputAttribute(string.Format("{0} {1}", FInputAttribute.Name, i + 1))
				{
					IsPinGroup = false,
					Order = FInputAttribute.Order + FOffsetCounter * 1000 + i,
					BinOrder = FInputAttribute.Order + FOffsetCounter * 1000 + i,
					AutoValidate = FInputAttribute.AutoValidate
				};
				var io = FFactory.CreateIOContainer(typeof(IInStream<T>), attribute);
				FIOContainers.Add(io);
			}
			
			for (int i = oldCount - 1; i >= newCount; i--)
			{
				var io = FIOContainers[i];
				FIOContainers.Remove(io);
				io.Dispose();
			}
			
			FStreams.Length = FIOContainers.Count;
			using (var writer = FStreams.GetWriter())
			{
				foreach (var io in FIOContainers)
				{
					writer.Write(io.RawIOObject as IInStream<T>);
				}
			}
		}
		
		public int Length
		{
			get
			{
				return FStreams.Length;
			}
		}
		
		public IStreamReader<IInStream<T>> GetReader()
		{
			return FStreams.GetReader();
		}
		
		public bool Sync()
		{
			IsChanged = false;
			foreach (var stream in FStreams)
			{
				IsChanged |= stream.Sync();
			}
			return IsChanged;
		}
		
		public bool IsChanged { get; private set; }
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public System.Collections.Generic.IEnumerator<IInStream<T>> GetEnumerator()
		{
			return GetReader();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	    
//        public void Dispose()
//        {
//            FCountSpread.Changed -= HandleCountSpreadChanged;
//            foreach (var container in FIOContainers)
//            {
//                container.Dispose();
//            }
//            FIOContainers.Clear();
//        }
	}
}
