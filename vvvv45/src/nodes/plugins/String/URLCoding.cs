using System;
using System.Web;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Text
{
    [PluginInfo(Name = "Encode", 
	            Category = "String", 
	            Version = "URL",
	            Help = "Encodes a URL string.",
	            Tags = "web")]
    public class URLEncodeNode: IPluginEvaluate
    {
        [Input("Input")]
        public IDiffSpread<string> FInput;

        [Output("Output")]
        public ISpread<string> FOutput;

        public void Evaluate(int spreadMax)
        {
            if (!FInput.IsChanged) return;

            FOutput.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            	FOutput[i] = HttpUtility.UrlEncode(FInput[i]);
        }
    }

    [PluginInfo(Name = "Decode", 
                Category = "String", 
                Version = "URL",
	            Help = "Converts a string that has been encoded for transmission in a URL into a decoded string.",
	            Tags = "web")]
    public class URLDecodeNode: IPluginEvaluate
    {
        [Input("Input")]
        public IDiffSpread<string> FInput;

        [Output("Output")]
        public ISpread<string> FOutput;

        public void Evaluate(int spreadMax)
        {
            if (!FInput.IsChanged) return;

            FOutput.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            	FOutput[i] = HttpUtility.UrlDecode(FInput[i]);
        }
    }
}
