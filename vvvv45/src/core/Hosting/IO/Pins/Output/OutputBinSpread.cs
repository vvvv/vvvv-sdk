using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Output
{
	[ComVisible(false)]
	class OutputBinSpread<T> : BinSpread<T>
	{
		private readonly IOutStream<T> FDataStream;
		private readonly IOutStream<int> FBinSizeStream;
		private readonly T[] FDataBuffer = new T[StreamUtils.BUFFER_SIZE];
		private readonly int[] FBinSizeBuffer = new int[StreamUtils.BUFFER_SIZE];
		
		public OutputBinSpread(IIOFactory ioFactory, OutputAttribute attribute)
			: base(ioFactory, attribute)
		{
			FDataStream = FIOFactory.CreateIO<IOutStream<T>>(attribute);
			FBinSizeStream = FIOFactory.CreateIO<IOutStream<int>>(attribute.GetBinSizeOutputAttribute());
			
			SliceCount = 1;
		}
		
		public override void Flush()
		{
			FBinSizeStream.Length = SliceCount;
			
			int dataStreamLength = 0;
			using (var binSizeWriter = FBinSizeStream.GetWriter())
			{
				foreach (var spread in this)
				{
					var stream = spread.Stream;
					dataStreamLength += stream.Length;
					binSizeWriter.Write(stream.Length);
				}
			}
			
			FDataStream.Length = dataStreamLength;
			
			using (var dataWriter = FDataStream.GetWriter())
			{
				foreach (var spread in this)
				{
					var stream = spread.Stream;
					using (var dataReader = stream.GetReader())
					{
						while (!dataReader.Eos)
						{
							int numSlicesRead = dataReader.Read(FDataBuffer, 0, FDataBuffer.Length);
							dataWriter.Write(FDataBuffer, 0, numSlicesRead);
						}
					}
				}
			}
		}
	}
}
