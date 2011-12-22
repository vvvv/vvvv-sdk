using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO.Streams
{
	class MultiDimOutStream<T> : ManagedIOStream<IInStream<T>>
	{
		private readonly IOutStream<T> FDataStream;
		private readonly IOutStream<int> FBinSizeStream;
		private readonly T[] FBuffer = new T[StreamUtils.BUFFER_SIZE];
		
		public MultiDimOutStream(IIOFactory ioFactory, OutputAttribute attribute)
		{
			FDataStream = ioFactory.CreateIO<IOutStream<T>>(attribute);
			FBinSizeStream = ioFactory.CreateIO<IOutStream<int>>(attribute.GetBinSizeOutputAttribute());
			Length = 1;
		}
		
		public override void Flush()
		{
			FBinSizeStream.Length = Length;
			
			int binSizeSum = 0;
			using (var binSizeWriter = FBinSizeStream.GetWriter())
			{
				foreach (var outputStream in this)
				{
					binSizeWriter.Write(outputStream.Length);
					binSizeSum += outputStream.Length;
				}
			}
			
			FDataStream.Length = binSizeSum;
			using (var dataWriter = FDataStream.GetWriter())
			{
				foreach (var outputStream in this)
				{
					using (var reader = outputStream.GetReader())
					{
						int numSlicesRead = reader.Read(FBuffer, 0, FBuffer.Length);
						dataWriter.Write(FBuffer, 0, numSlicesRead);
					}
				}
			}
			
			base.Flush();
		}
	}
}
