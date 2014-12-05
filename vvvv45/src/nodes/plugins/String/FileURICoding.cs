using System;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Encode", Category = "String", Version = "File", Help = "Encodes a file path using the file URI scheme.", Tags = "URI, path")]
	#endregion PluginInfo
	public class EncodeStringFileNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", StringType = StringType.Filename)]
		public IDiffSpread<string> FInput;
			
		[Output("Output")]
		public ISpread<string> FOutput;

		[Output("Error")]
		public ISpread<string> FError;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			if (FInput.IsChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					var input = FInput[i].Trim();
					
					if (!string.IsNullOrEmpty(input))
					{
						try
						{
							var Uri = new Uri(input, UriKind.RelativeOrAbsolute);
							FOutput[i] = Uri.AbsoluteUri;
							FError[i] = string.Empty;
						}
						catch (Exception e)
						{
							FOutput[i] = string.Empty;
							FError[i] = e.Message;
						}
					}
				}	
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Decode", Category = "String", Version = "File", Help = "Decodes a file URI scheme as local path.", Tags = "URI, path")]
	#endregion PluginInfo
	public class DecodeStringFileNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public IDiffSpread<string> FInput;
			
		[Output("Output")]
		public ISpread<string> FOutput;

		[Output("Error")]
		public ISpread<string> FError;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;

			if (FInput.IsChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					var input = FInput[i].Trim();
					
					if (!string.IsNullOrEmpty(input))
					{
						try
						{
							var Uri = new Uri(input, UriKind.RelativeOrAbsolute);
							FOutput[i] = Uri.LocalPath;
							FError[i] = string.Empty;
						}
						catch (Exception e)
						{
							FOutput[i] = string.Empty;
							FError[i] = e.Message;
						}
					}
				}	
			}
		}
	}
	
}

