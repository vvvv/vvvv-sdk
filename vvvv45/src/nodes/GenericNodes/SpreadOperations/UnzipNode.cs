using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Generic
{

	public abstract class UnzipNode<T> : IPluginEvaluate
	{
		[Input("Input", BinSize = -2)]
		protected IInStream<T> FInputStream;

		[Output("Output", IsPinGroup = true)]
		protected IInStream<IOutStream<T>> FOutputStreams;
		
		public void Evaluate(int SpreadMax)
		{
			FOutputStreams.SetLengthBy(FInputStream);
	
			var buffer = MemoryPool<T>.GetArray();			
			try
			{
				var outputStreamsLength = FOutputStreams.Length;
				
				using (var reader = FInputStream.GetCyclicReader())
				{
					int i = 0;
					foreach (var outputStream in FOutputStreams)
					{
						int numSlicesToWrite = Math.Min(outputStream.Length, buffer.Length);
						
						reader.Position = i++;
						using (var writer = outputStream.GetWriter())
						{
							while (!writer.Eos)
							{
								reader.Read(buffer, 0, numSlicesToWrite, outputStreamsLength);
								writer.Write(buffer, 0, numSlicesToWrite);
							}
						}
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
