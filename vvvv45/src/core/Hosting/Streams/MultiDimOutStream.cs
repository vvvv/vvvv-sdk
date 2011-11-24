using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	public class MultiDimOutStream<T> : IOutStream<IOutStream<T>>
	{
		private readonly IOutStream<T> FDataStream;
		private readonly IOutStream<int> FBinSizeStream;
		
		public MultiDimOutStream(IOFactory ioFactory, OutputAttribute attribute)
		{
			FDataStream = ioFactory.Create<IOutStream<T>>(attribute);
			FBinSizeStream = ioFactory.CreateIO<IOutStream<int>>(
				new OutputAttribute(string.Format("{0} Bin Size", attribute.Name))
			);
		}
		
		public int WritePosition 
		{
			get;
			set;
		}
		
		public int Length 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
		
		public bool Eof 
		{
			get 
			{
				return WritePosition >= Length;
			}
		}
		
		public void Write(IOutStream<T> value, int stride)
		{
			throw new NotImplementedException();
		}
		
		public int Write(IOutStream<T>[] buffer, int index, int length, int stride)
		{
			throw new NotImplementedException();
		}
		
		public void Flush()
		{
			throw new NotImplementedException();
		}
		
		public void Reset()
		{
			WritePosition = 0;
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
	}
}
