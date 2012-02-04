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
			
			var buffer = MemoryPool<T>.GetArray();
			try 
			{
			    using (var dataWriter = FDataStream.GetWriter())
    			{
    				foreach (var spread in this)
    				{
    					dataWriter.Write(spread.Stream, buffer);
    				}
    			}
			} 
			finally 
			{
			    MemoryPool<T>.PutArray(buffer);
			}
		}
	}
}
