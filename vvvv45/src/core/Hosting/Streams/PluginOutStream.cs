
using System;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	// Slow
	abstract class PluginOutStream<T> : IOutStream<T>
	{
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
		
		public void Write(T value, int stepSize)
		{
			SetSlice(WritePosition, value);
			WritePosition += stepSize;
		}
		
		public int Write(T[] buffer, int index, int length, int stepSize)
		{
			var numSlicesToWrite = StreamUtils.GetNumSlicesToWrite(this, index, length, stepSize);
			for (int i = index; i < index + numSlicesToWrite; i++)
			{
				Write(buffer[i], stepSize);
			}
			return numSlicesToWrite;
		}
		
		public void Reset()
		{
			WritePosition = 0;
		}
		
		public bool Eof 
		{
			get 
			{
				return WritePosition >= Length;
			}
		}
		
		public int WritePosition 
		{
			get;
			set;
		}
		
		public void Flush()
		{
			// Nothing to do
		}
	}
	
	class StringOutStream : PluginOutStream<string>
	{
		private readonly IStringOut FStringOut;
		
		public StringOutStream(IStringOut stringOut)
		{
			FStringOut = stringOut;
		}
		
		protected override void SetSlice(int index, string value)
		{
			FStringOut.SetString(index, value);
		}
		
		public override int Length
		{
			get
			{
				return FStringOut.SliceCount;
			}
			set
			{
				FStringOut.SliceCount = value;
			}
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
