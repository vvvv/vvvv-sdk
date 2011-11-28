using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2
{
	/// <summary>
	/// The spread data type
	/// </summary>
	[ComVisible(false)]
	public class Spread<T> : ISpread<T>
	{
		private readonly IIOStream<T> FStream;
		
		public Spread(IIOStream<T> stream)
		{
			FStream = stream;
			Sync();
		}
		
		public Spread(int size)
			: this(new ManagedIOStream<T>())
		{
			SliceCount = size;
		}
		
		public Spread()
			: this(0)
		{
		}
		
		public Spread(IList<T> original)
			: this(original.Count)
		{
			var buffer = new T[original.Count];
			original.CopyTo(buffer, 0);
			using (var writer = FStream.GetWriter())
			{
				writer.Write(buffer, 0, buffer.Length);
			}
		}
		
		public virtual bool Sync()
		{
			return FStream.Sync();
		}
		
		public virtual void Flush()
		{
			FStream.Flush();
		}
		
		public IIOStream<T> Stream
		{
			get
			{
				return FStream;
			}
		}
		
		public T this[int index]
		{
			get
			{
				index = VMath.Zmod(index, FStream.Length);
				using (var reader = FStream.GetReader())
				{
					reader.Position = index;
					return reader.Read();
				}
			}
			set
			{
				index = VMath.Zmod(index, FStream.Length);
				using (var writer = FStream.GetWriter())
				{
					writer.Position = index;
					writer.Write(value);
				}
			}
		}
		
		object ISpread.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				this[index] = (T) value;
			}
		}
		
		public virtual int SliceCount
		{
			get
			{
				return FStream.Length;
			}
			set
			{
				if (value != FStream.Length)
				{
					FStream.Length = value >= 0 ? value : 0;
				}
			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			return FStream.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		public object Clone()
		{
			return new Spread<T>(FStream.Clone() as IIOStream<T>);
		}
	}

	[ComVisible(false)]
	public static class SpreadExtensions_
	{
		public static ISpread<T> ToSpread<T>(this List<T> list)
		{
			return new Spread<T>(list);
		}
	}
}