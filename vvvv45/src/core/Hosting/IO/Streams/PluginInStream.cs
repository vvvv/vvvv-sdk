
using System;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
	// Slow
	abstract class PluginInStream<T> : IInStream<T>
	{
		class PluginInReader : IStreamReader<T>
		{
			private readonly PluginInStream<T> FStream;
			
			public PluginInReader(PluginInStream<T> stream)
			{
				FStream = stream;
			}
			
			public bool Eos
			{
				get
				{
					return Position >= Length;
				}
			}
			
			public int Position
			{
				get;
				set;
			}
			
			public int Length
			{
				get
				{
					return FStream.Length;
				}
			}
			
			public T Current
			{
				get;
				private set;
			}
			
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}
			
			public bool MoveNext()
			{
				var result = !Eos;
				if (result)
				{
					Current = Read();
				}
				return result;
			}
			
			public T Read(int stride = 1)
			{
				var result = FStream.GetSlice(Position);
				Position += stride;
				return result;
			}
			
			public int Read(T[] buffer, int index, int length, int stride)
			{
				var numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				for (int i = index; i < index + numSlicesToRead; i++)
				{
					buffer[i] = Read(stride);
				}
				return numSlicesToRead;
			}
			
//			public void ReadCyclic(T[] buffer, int index, int length, int stride)
//			{
//				// No need to optimize here as slow as it is already.
//				StreamUtils.ReadCyclic(this, buffer, index, length, stride);
//			}
			
			public void Dispose()
			{
				// Nothing to do
			}
			
			public void Reset()
			{
				Position = 0;
			}
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public abstract int Length
		{
			get;
		}
		
		protected abstract T GetSlice(int index);
		
		public abstract bool Sync();
		
		public IStreamReader<T> GetReader()
		{
			return new PluginInReader(this);
		}
		
		public System.Collections.Generic.IEnumerator<T> GetEnumerator()
		{
			return GetReader();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	
	class StringInStream : ManagedIOStream<string>
	{
		private readonly IStringIn FStringIn;
		
		public StringInStream(IStringIn stringIn)
		{
			FStringIn = stringIn;
		}
		
		public override bool Sync()
		{
			if (FStringIn.Validate())
			{
				Length = FStringIn.SliceCount;
				using (var writer = GetWriter())
				{
					for (int i = 0; i < Length; i++)
					{
						string result;
						FStringIn.GetString(i, out result);
						writer.Write(result ?? string.Empty);
					}
				}
				
				return true;
			}
			
			return false;
		}
	}
	
	class EnumInStream<T> : PluginInStream<T>
	{
		protected readonly IEnumIn FEnumIn;
		
		public EnumInStream(IEnumIn enumIn)
		{
			FEnumIn = enumIn;
		}
		
		public override int Length
		{
			get
			{
				return FEnumIn.SliceCount;
			}
		}
		
		protected override T GetSlice(int index)
		{
			string entry;
			FEnumIn.GetString(index, out entry);
			try
			{
				return (T)Enum.Parse(typeof(T), entry);
			}
			catch (Exception)
			{
				return default(T);
			}
		}
		
		public override bool Sync()
		{
			return FEnumIn.Validate();
		}
	}
	
	class DynamicEnumInStream : EnumInStream<EnumEntry>
	{
		public DynamicEnumInStream(IEnumIn enumIn)
			: base(enumIn)
		{
		}
		
		protected override EnumEntry GetSlice(int index)
		{
			int ord;
			string name;
			FEnumIn.GetOrd(index, out ord);
			// TODO: Was not used. FEnumName.
			FEnumIn.GetString(index, out name);
			return new EnumEntry(name, ord);
		}
	}
	
	class NodeInStream<T> : PluginInStream<T>
	{
		private readonly INodeIn FNodeIn;
		
		public NodeInStream(INodeIn nodeIn, IConnectionHandler handler)
		{
			FNodeIn = nodeIn;
			FNodeIn.SetConnectionHandler(handler, this);
		}
		
		public NodeInStream(INodeIn nodeIn)
		    : this(nodeIn, new DefaultConnectionHandler())
		{
		}
		
		public override int Length
		{
			get
			{
				return FNodeIn.SliceCount;
			}
		}
		
		protected override T GetSlice(int index)
		{
			object usI;
			FNodeIn.GetUpstreamInterface(out usI);
			var upstreamInterface = usI as IGenericIO;
			
			int usS;
			if (upstreamInterface != null)
			{
				FNodeIn.GetUpsreamSlice(index, out usS);
				return (T) upstreamInterface.GetSlice(usS);
			}
			else
			{
				return default(T);
			}
		}
		
		public override bool Sync()
		{
			return FNodeIn.Validate();
		}
	}
}
