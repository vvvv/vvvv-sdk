using System;
using System.Web;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Text
{
    [PluginInfo(Name = "Encode", 
	            Category = "String", 
	            Version = "HTML",
	            Help = "Converts a string to an HTML-encoded string.",
	            Tags = "web")]
    public class HTMLEncodeNode: IPluginEvaluate
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
            	FOutput[i] = HttpUtility.HtmlEncode(FInput[i]);
        }
    }

    [PluginInfo(Name = "Decode", 
                Category = "String", 
                Version = "HTML",
                Help = "Converts a string that has been HTML-encoded for HTTP transmission into a decoded string.",
                Tags = "web")]
    public class HTMLDecodeNode: IPluginEvaluate
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
            	FOutput[i] = HttpUtility.HtmlDecode(FInput[i]);
        }
    }
    
    [PluginInfo(Name = "Encode", 
	            Category = "String", 
	            Version = "HTML Attribute",
	            Help = "Minimally converts a string to an HTML-encoded string. Converts only quotation marks, ampersands and left angle brackets to equivalent character entities. It is considerably faster than Encode (String HTML).",
	            Tags = "web")]
    public class HTMLAttribteEncodeNode: IPluginEvaluate
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
            	FOutput[i] = HttpUtility.HtmlAttributeEncode(FInput[i]);
        }
    }
}
