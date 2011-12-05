using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Input
{
	[ComVisible(false)]
	class InputBinSpread<T> : BinSpread<T>
	{
		private readonly IInStream<int> FBinSizeStream;
		protected readonly IInStream<T> FDataStream;
		protected readonly IIOStream<int> FNormBinSizeStream;
		protected readonly int[] FBinSizeBuffer = new int[StreamUtils.BUFFER_SIZE];
		protected readonly T[] FDataBuffer = new T[StreamUtils.BUFFER_SIZE];
		
		public InputBinSpread(IIOFactory ioFactory, InputAttribute attribute)
			: base(ioFactory, attribute)
		{
			attribute = ManipulateAttribute(attribute);
			
			attribute.AutoValidate = false;
			FDataStream = FIOFactory.CreateIO<IInStream<T>>(attribute);
			
			var att = new InputAttribute(attribute.Name + " Bin Size")
			{
				AutoValidate = false,
				DefaultValue = attribute.BinSize,
				Order = attribute.Order + 1,
				CheckIfChanged = attribute.CheckIfChanged
			};
			FBinSizeStream = FIOFactory.CreateIO<IInStream<int>>(att);
			FNormBinSizeStream = new ManagedIOStream<int>();
		}
		
		protected virtual InputAttribute ManipulateAttribute(InputAttribute attribute)
		{
			// Do nothing by default
			return attribute;
		}
		
		public override bool Sync()
		{
			// Sync source
			bool binSizeChanged = FBinSizeStream.Sync();
			bool dataChanged = FDataStream.Sync();
			
			if (!(binSizeChanged || dataChanged))
			{
				return false;
			}
			
			// Normalize bin size and compute sum
			int dataStreamLength = FDataStream.Length;
			int binSizeSum = 0;
			
			FNormBinSizeStream.Length = FBinSizeStream.Length;
			using (var binSizeReader = FBinSizeStream.GetReader())
			{
				using (var binSizeWriter = FNormBinSizeStream.GetWriter())
				{
					while (!binSizeReader.Eos)
					{
						int binSizeCount = binSizeReader.Read(FBinSizeBuffer, 0, FBinSizeBuffer.Length);
						for (int i = 0; i < binSizeCount; i++)
						{
							FBinSizeBuffer[i] = SpreadUtils.NormalizeBinSize(dataStreamLength, FBinSizeBuffer[i]);
							binSizeSum += FBinSizeBuffer[i];
						}
						
						binSizeWriter.Write(FBinSizeBuffer, 0, binSizeCount);
					}
				}
			}
			

			int binTimes = SpreadUtils.DivByBinSize(dataStreamLength, binSizeSum);
			binTimes = binTimes > 0 ? binTimes : 1;
			SliceCount = binTimes * FBinSizeStream.Length;
			
			using (var binSizeReader = FNormBinSizeStream.GetCyclicReader())
			{
				using (var dataReader = FDataStream.GetCyclicReader())
				{
					foreach (var spread in this)
					{
						spread.SliceCount = binSizeReader.Read();
						
						var stream = spread.Stream;
						using (var writer = stream.GetWriter())
						{
							while (!writer.Eos)
							{
								// Since we're using cyclic readers we need to limit the amount 
								// of data we request.
								int numSlicesRead = dataReader.Read(FDataBuffer, 0, Math.Min(FDataBuffer.Length, writer.Length));
								writer.Write(FDataBuffer, 0, numSlicesRead);
							}
						}
					}
				}
			}
			
			return base.Sync();
		}
	}
}
