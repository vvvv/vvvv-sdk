
using System;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
	// Slow
	abstract class PluginOutStream<T> : IOutStream<T>
	{
		class PluginOutWriter : IStreamWriter<T>
		{
			private readonly PluginOutStream<T> FStream;
			
			public PluginOutWriter(PluginOutStream<T> stream)
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
				set
				{
					FStream.Length = value;
				}
			}
			
			public void Write(T value, int stride)
			{
				FStream.SetSlice(Position, value);
				Position += stride;
			}
			
			public int Write(T[] buffer, int index, int length, int stride)
			{
				var numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
				for (int i = index; i < index + numSlicesToWrite; i++)
				{
					Write(buffer[i], stride);
				}
				return numSlicesToWrite;
			}
			
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
			set;
		}
		
		protected abstract void SetSlice(int index, T value);
		
		public void Flush()
		{
			// Nothing to do
		}
		
		public IStreamWriter<T> GetWriter()
		{
			return new PluginOutWriter(this);
		}
	}
	
	class StringOutStream : ManagedIOStream<string>
	{
		private readonly IStringOut FStringOut;
		
		public StringOutStream(IStringOut stringOut)
		{
			FStringOut = stringOut;
		}
		
		public override void Flush()
		{
			if (FChanged)
			{
				FStringOut.SliceCount = Length;
				
				int i = 0;
				using (var reader = GetReader())
				{
					while (!reader.Eos)
					{
						FStringOut.SetString(i++, reader.Read());
					}
				}
			}
			
			base.Flush();
		}
	}
	
	class EnumOutStream<T> : PluginOutStream<T>
	{
		protected readonly IEnumOut FEnumOut;
		
		public EnumOutStream(IEnumOut enumOut)
		{
			FEnumOut = enumOut;
		}
		
		protected override void SetSlice(int index, T value)
		{
			FEnumOut.SetString(index, value.ToString());
		}
		
		public override int Length
		{
			get
			{
				return FEnumOut.SliceCount;
			}
			set
			{
				FEnumOut.SliceCount = value;
			}
		}
	}
	
	class DynamicEnumOutStream : EnumOutStream<EnumEntry>
	{
		public DynamicEnumOutStream(IEnumOut enumOut)
			: base(enumOut)
		{
		}
		
		protected override void SetSlice(int index, EnumEntry value)
		{
			FEnumOut.SetOrd(index, value.Index);
		}
	}
	
	class NodeOutStream<T> : PluginOutStream<T>, IGenericIO
	{
		private readonly INodeOut FNodeOut;
		private readonly ISpread<T> FDataStore;
		
		public NodeOutStream(INodeOut nodeOut)
			: this(nodeOut, new Spread<T>())
		{
			
		}
		
		private NodeOutStream(INodeOut nodeOut, ISpread<T> dataStore)
		{
			FNodeOut = nodeOut;
			FNodeOut.SetInterface(this);
			FDataStore = dataStore;
		}
		
		object IGenericIO.GetSlice(int index)
		{
			return FDataStore[index];
		}
		
		protected override void SetSlice(int index, T value)
		{
			FDataStore[index] = value;
		}
		
		public override int Length
		{
			get
			{
				return FNodeOut.SliceCount;
			}
			set
			{
				FNodeOut.SliceCount = value;
				FDataStore.SliceCount = value;
			}
		}
	}
}
