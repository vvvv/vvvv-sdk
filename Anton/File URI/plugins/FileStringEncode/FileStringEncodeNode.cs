#region usings
using System;
using System.ComponentModel.Composition;
using System.Web;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Encode", Category = "String", Version = "File", Help = "Encodes a File path using the file URI scheme.", Tags = "URI, path")]
	#endregion PluginInfo
	public class EncodeStringFileNode : IPluginEvaluate
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
					var input = FInput[i];
					var output = FOutput[i];
									
					if (input.Length > 0)
					{
						try
						{
							var Uri = new Uri(input, UriKind.RelativeOrAbsolute);
							output=Uri.AbsoluteUri;
						}
						catch
						{
							output=String.Empty;
							FError[i]=@"Incorrect Path";
						}
					}
					
					FOutput[i] = output;
					
				}	
			}
			
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Decode", Category = "String", Version = "File", Help = "Decodes a file URI scheme as Local Path.", Tags = "URI, path")]
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
					var input = FInput[i];
					var output = FOutput[i];
									
					if (input.Length > 0)
					{
						try
						{
							var Uri = new Uri(input, UriKind.RelativeOrAbsolute);
							output=Uri.LocalPath;
						}
						catch
						{
							output=String.Empty;
							FError[i]=@"Incorrect file URI";
						}
					}
					
					FOutput[i] = output;
					
				}	
			}
			
		}
	}
}
