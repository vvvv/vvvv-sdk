using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
//	class MyPlugin<T>
//	{
//		IInStream<IInStream<T>> InStreams;
//		IIOStream<IOutStream<T>> OutStreams;
//
//		public void Evaluate()
//		{
//			OutStreams.Length = InStreams.Length;
//			while (!OutStreams.Eof)
//			{
//				var inStream = InStreams.Read();
//				var outStream = OutStreams.Read();
//
//				OutStreams.Write(outStream);
//			}
//		}
//	}
	
	public class MultiDimOutStream<T> : ManagedIOStream<ManagedIOStream<T>>
	{
		private readonly IOutStream<T> FDataStream;
		private readonly IOutStream<int> FBinSizeStream;
		private readonly T[] FBuffer = new T[StreamUtils.BUFFER_SIZE];
		
		public MultiDimOutStream(IOFactory ioFactory, OutputAttribute attribute)
		{
			FDataStream = ioFactory.CreateIO<IOutStream<T>>(attribute);
			FBinSizeStream = ioFactory.CreateIO<IOutStream<int>>(
				new OutputAttribute(string.Format("{0} Bin Size", attribute.Name))
			);
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
		
		protected override void BufferIncreased(ManagedIOStream<T>[] oldBuffer, ManagedIOStream<T>[] newBuffer)
		{
			Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
			if (oldBuffer.Length > 0)
			{
				for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
					newBuffer[i] = new ManagedIOStream<T>();
			}
		}
	}
}
