#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using System.IO;
using System.Text;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Reader", Category = "String", Version = "Advanced", Help = "Returns specified parts of a file", Tags = "file", Author = "woei")]
	#endregion PluginInfo
	public class ReaderFileAdvancedNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 649, 169
		[Input("Filename", StringType = StringType.Filename)]
		ISpread<string> FInput;
		
		[Input("Encoding", EnumName = "CodePages")]
		IDiffSpread<EnumEntry> FEncoding;
		
		[Input("Toggle line-wise", Visibility = PinVisibility.Hidden)]
		ISpread<bool> FLineWise;

		[Input("Startindex", MinValue = 0)]
		IDiffSpread<int> FIndex;
		
		[Input("Count", MinValue = 0, DefaultValue = 1)]
		IDiffSpread<int> FCount;
		
		[Input("Read", IsBang = true)]
		ISpread<bool> FRead;
		
		[Output("Content")]
		ISpread<string> FContent;
		
		[Output("End Of Stream")]
		ISpread<bool> FEOS;
		
		Spread<string> path = new Spread<string>(0);
		Spread<bool> lineWise = new Spread<bool>(0);
		Spread<StreamReader> streamReader = new Spread<StreamReader>(0);
		#pragma warning restore 649, 169
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			FContent.SliceCount = spreadMax;
			FEOS.SliceCount = spreadMax;
			
			path.ResizeAndDismiss(spreadMax, () => string.Empty);
			lineWise.ResizeAndDismiss(spreadMax, (i) => FLineWise[i]);
			streamReader.Resize(spreadMax, (i) => new StreamReader(FInput[i]), (t) => t.Dispose());
			
			for (int i = 0; i < spreadMax; i++)
			{
				//check if encoding has changed
				bool encodingChanged = false;
				System.Text.Encoding enc = System.Text.Encoding.Default;
				if (FEncoding.IsChanged)
				{
					if (FEncoding[i].Index>0)
						enc = Encoding.GetEncoding(FEncoding[i].Name);
					encodingChanged = !(streamReader[i].CurrentEncoding != enc);
				}
				
				//initialize stream reader
				bool update = false;
				bool isValid = true;
				if (path[i] != FInput[i] || encodingChanged || FRead[i])
				{
					try
					{
						streamReader[i].Dispose();
						streamReader[i] = new StreamReader(FInput[i], enc);
						path[i] = FInput[i];
						update = true;
					}
					catch
					{
						FContent[i]=string.Empty;
						FEOS[i] = true;
						isValid = false;
					}
				}
				
				//do the reading part
				if ((lineWise[i]!=FLineWise[i] || FIndex.IsChanged || FCount.IsChanged || update) && isValid)
				{
					streamReader[i].DiscardBufferedData();
					if (FLineWise[i])
					{
						streamReader[i].BaseStream.Seek(0, SeekOrigin.Begin);
						int incr = 0;
						FContent[i] = string.Empty;
						while (incr < FIndex[i]+FCount[i] && !streamReader[i].EndOfStream)
						{
							string line = streamReader[i].ReadLine();
							if (incr >= FIndex[i])
								FContent[i] += line + Environment.NewLine;
							
							incr++;
						}
						if (incr >0) //remove last \r\n
							FContent[i] = FContent[i].Substring(0, FContent[i].Length-Environment.NewLine.Length);
					}
					else
					{
						streamReader[i].BaseStream.Seek(FIndex[i], SeekOrigin.Begin);
						
						char[] buffer = new char[Math.Max(FCount[i],0)];
						int read = streamReader[i].ReadBlock(buffer, 0, buffer.Length);
						FContent[i] = new string(buffer);
					}
					FEOS[i] = streamReader[i].EndOfStream;
				}
			}
		}
	}
}
