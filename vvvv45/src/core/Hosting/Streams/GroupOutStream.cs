using System;
using System.Collections.Generic;
using VVVV.Hosting.Streams.Registry;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	public class GroupOutStream<T> : IInStream<IOutStream<T>>
	{
		private readonly ManagedIOStream<IOutStream<T>> FStreams = new ManagedIOStream<IOutStream<T>>();
		private readonly List<IOHandler> FIOHandlers = new List<IOHandler>();
		private readonly IDiffSpread<int> FCountSpread;
		private readonly IOFactory FFactory;
		private readonly OutputAttribute FOutputAttribute;
		
		public GroupOutStream(IOFactory factory, OutputAttribute attribute)
		{
			FFactory = factory;
			FOutputAttribute = attribute;
			
			FCountSpread = factory.CreateIO<IDiffSpread<int>>(
				new ConfigAttribute(FOutputAttribute.Name + " Count")
				{
					DefaultValue = 2,
					MinValue = 0
				}
			);
			
			FCountSpread.Changed += HandleCountSpreadChanged;
			FCountSpread.Sync();
		}

		void HandleCountSpreadChanged(IDiffSpread<int> spread)
		{
			int oldCount = FIOHandlers.Count;
			int newCount = Math.Max(spread[0], 0);
			
			for (int i = oldCount; i < newCount; i++)
			{
				var attribute = new OutputAttribute(string.Format("{0} {1}", FOutputAttribute.Name, i + 1))
				{
					IsPinGroup = false,
					Order = i
				};
				var io = FFactory.CreateIOHandler<IOutStream<T>>(attribute);
				FIOHandlers.Add(io);
			}
			
			for (int i = oldCount - 1; i >= newCount; i--)
			{
				var io = FIOHandlers[i];
				FFactory.DestroyIOHandler(io);
				FIOHandlers.Remove(io);
			}
			
			FStreams.Length = FIOHandlers.Count;
			using (var writer = FStreams.GetWriter())
			{
				foreach (var io in FIOHandlers)
				{
					writer.Write(io.RawIOObject as IOutStream<T>);
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
		
		public IStreamReader<IOutStream<T>> GetReader()
		{
			return FStreams.GetReader();
		}
		
		public bool Sync()
		{
			return true;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public System.Collections.Generic.IEnumerator<IOutStream<T>> GetEnumerator()
		{
			return GetReader();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
