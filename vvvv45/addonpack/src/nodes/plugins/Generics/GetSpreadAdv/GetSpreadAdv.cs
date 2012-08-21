#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{

	public class GetSpread<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IInStream<IInStream<T>> FInput;
		
		[Input("Offset")]
		IInStream<int> FOffset;
		
		[Input("Count", DefaultValue = 1, MinValue = 0)]
		IInStream<int> FCount;

		[Output("Output")]
		IOutStream<T> FOutput;
		
		[Output("Output Bin Size")]
		IOutStream<int> FOutBinSize;
		
		[Import]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (FInput.Length == 0 || FOffset.Length == 0 || FCount.Length == 0)
			{
				FOutput.Length=0;
				FOutBinSize.Length = 1;
				FOutBinSize.GetWriter().Write(0);
			}
			else
			{
				spreadMax = Math.Max(FInput.Length,Math.Max(FOffset.Length,FCount.Length));
				FOutBinSize.Length = spreadMax;
				
				var countB = new int[spreadMax];
				FCount.GetCyclicReader().Read(countB, 0, spreadMax);
				
				int sliceCount = 0;
				for (int i=0; i<spreadMax; i++)
					sliceCount+=countB[i];
				FOutput.Length = sliceCount;
				
				var binB = MemoryPool<IInStream<T>>.GetArray();
				var offsetB = MemoryPool<int>.GetArray(binB.Length);
				var sliceB = MemoryPool<T>.GetArray(binB.Length);
				try
				{
					using (var binReader = FInput.GetCyclicReader())
					using (var offsetReader = FOffset.GetCyclicReader())
					using (var sliceWriter = FOutput.GetWriter())
					using (var binSizeWriter = FOutBinSize.GetWriter())
					{
						int binsLeft = spreadMax;
						int binsToRead = 0;
						do
						{
							binsToRead = Math.Min(binsLeft,binB.Length);
							binReader.Read(binB, 0, binsToRead);
							offsetReader.Read(offsetB, 0, binsToRead);
							binsLeft -= binsToRead;
							
							for (int i=0; i<binsToRead; i++)
							{
								binSizeWriter.Write(countB[i]);
								if (binB[i].Length==0)
								{
									FOutput.Length-=countB[i];
								}
								else if (countB[i] != 0)
								{
									using (var sliceReader = binB[i].GetCyclicReader())
									{
										offsetB[i] = VMath.Zmod(offsetB[i],binB[i].Length);
										sliceReader.Position = offsetB[i];
										
										int slicesLeft = countB[i];
										int slicesToRead = 0;
										do
										{
											slicesToRead = Math.Min(slicesLeft,sliceB.Length);
											slicesLeft -= slicesToRead;
											sliceReader.Read(sliceB,0,slicesToRead);
											sliceWriter.Write(sliceB,0,slicesToRead);
											
										} while (slicesLeft >0);
									}
								}
							}
						} while (binsLeft>0);
					}
					
				}
				finally
				{
					MemoryPool<IInStream<T>>.PutArray(binB);
					MemoryPool<int>.PutArray(offsetB);
					MemoryPool<T>.PutArray(sliceB);
				}
			}
		}
	}
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Spreads",
	            Version = "Advanced",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadSpreads : GetSpread<double> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "String",
	            Version = "Advanced",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadString : GetSpread<string> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Color",
	            Version = "Advanced",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadColor : GetSpread<RGBAColor> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Transform",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadTransform : GetSpread<Matrix4x4> {}
	
	#region PluginInfo
	[PluginInfo(Name = "GetSpread",
	            Category = "Enumerations",
	            Help = "returns sub-spreads from the input specified via offset and count, with Bin Size Option",
	            Tags = "",
	            Author = "woei")]
	#endregion PluginInfo
	public class GetSpreadEnum : GetSpread<EnumEntry> {}
	
}
