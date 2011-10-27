
using System;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	// Slow
	abstract class PluginInStream<T> : IInStream<T>
	{
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public abstract int Length
		{
			get;
		}
		
		protected abstract T GetSlice(int index);
		
		public T Read(int stepSize)
		{
			var result = GetSlice(ReadPosition);
			ReadPosition += stepSize;
			return result;
		}
		
		public int Read(T[] buffer, int index, int length, int stepSize)
		{
			var numSlicesToRead = StreamUtils.GetNumSlicesToRead(this, index, length, stepSize);
			for (int i = index; i < index + numSlicesToRead; i++)
			{
				buffer[i] = Read(stepSize);
			}
			return numSlicesToRead;
		}
		
		public void ReadCyclic(T[] buffer, int index, int length, int stepSize)
		{
			// No need to optimize here as slow as it is already.
			StreamUtils.ReadCyclic(this, buffer, index, length, stepSize);
		}
		
		public void Reset()
		{
			ReadPosition = 0;
		}
		
		public int ReadPosition 
		{
			get;
			set;
		}
		
		public bool Eof 
		{
			get 
			{
				return ReadPosition >= Length;
			}
		}
		
		public void Sync()
		{
			// Nothing to do
		}
	}
	
	class StringInStream : PluginInStream<string>
	{
		private readonly IStringIn FStringIn;
		
		public StringInStream(IStringIn stringIn)
		{
			FStringIn = stringIn;
		}
		
		protected override string GetSlice(int index)
		{
			string result;
			FStringIn.GetString(index, out result);
			result = result ?? string.Empty;
			return result;
		}
		
		public override int Length
		{
			get
			{
				return FStringIn.SliceCount;
			}
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
		
		public NodeInStream(INodeIn nodeIn)
		{
			FNodeIn = nodeIn;
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
			INodeIOBase usI;
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
	}
}
