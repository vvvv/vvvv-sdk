using System;
using System.Collections.Generic;
using VVVV.Hosting.Streams.Registry;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	public class GroupInStream<T> : IInStream<IInStream<T>>
	{
		private readonly ManagedIOStream<IInStream<T>> FStreams = new ManagedIOStream<IInStream<T>>();
		private readonly List<IOHandler> FIOHandlers = new List<IOHandler>();
		private readonly IDiffSpread<int> FCountSpread;
		private readonly IOFactory FFactory;
		private readonly InputAttribute FInputAttribute;
		
		public GroupInStream(IOFactory factory, InputAttribute attribute)
		{
			FFactory = factory;
			FInputAttribute = attribute;
			
			FCountSpread = factory.CreateIO<IDiffSpread<int>>(
				new ConfigAttribute(FInputAttribute.Name + " Count")
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
				var attribute = new InputAttribute(string.Format("{0} {1}", FInputAttribute.Name, i + 1))
				{
					IsPinGroup = false,
					Order = i,
					AutoValidate = false // We validate them if necessary
				};
				var io = FFactory.CreateIOHandler<IInStream<T>>(attribute);
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
			var changed = false;
			foreach (var stream in FStreams)
			{
				changed |= stream.Sync();
			}
			return changed;
		}
		
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
	}
}
