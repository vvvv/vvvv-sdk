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
		protected readonly IIOStream<T> FStream;
		
		public Spread(IIOStream<T> stream)
		{
			FStream = stream;
		}
		
		public Spread(int size)
			: this(new ManagedIOStream<T>())
		{
			FStream.Length = size;
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
			FStream.Write(buffer, 0, buffer.Length);
		}
		
		public Spread(ISpread<T> original)
			: this(original.SliceCount)
		{
			var srcStream = original.GetStream();
			var buffer = srcStream.CreateReadBuffer();
			while (!srcStream.Eof)
			{
				int n = srcStream.Read(buffer, 0, buffer.Length);
				FStream.Write(buffer, 0, n);
			}
		}
		
		public IIOStream<T> GetStream()
		{
			FStream.Reset();
			return FStream;
		}
		
		public T this[int index]
		{
			get
			{
				// TODO: Try to make this faster
				FStream.ReadPosition = VMath.Zmod(index, FStream.Length);
				return FStream.Read();
			}
			set
			{
				// TODO: Try to make this faster
				FStream.WritePosition = VMath.Zmod(index, FStream.Length);
				FStream.Write(value);
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
				FStream.Length = value >= 0 ? value : 0;
			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			// TODO: implement this
			//var stream = FStream.Clone() as IInStream<T>;
			var stream = FStream;
			
			stream.Reset();
			
			while (!stream.Eof)
			{
				yield return stream.Read();
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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