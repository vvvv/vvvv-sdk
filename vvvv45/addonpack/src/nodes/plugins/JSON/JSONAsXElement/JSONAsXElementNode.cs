#region usings
using System;
using System.ComponentModel.Composition;

using System.Xml;
using System.Xml.XPath; 
using System.Xml.Linq;
using System.Runtime.Serialization.Json;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "AsXElement", Category = "JSON", Version = "", Help = "Reads a JSON string as XElement", Tags = "", Author="herbst")]
	#endregion PluginInfo
	public class JSONAsElementNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("JSON", DefaultString = "{\"vvvv\":\"awesome\"}")]
		public IDiffSpread<string> FInput;

		[Output("Element")]
		public ISpread<XElement> RootElement;
		
		[Output("Document")]
		public ISpread<XDocument> Document;
		
		[Output("Success")]
		public ISpread<bool> FSuccess;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(!FInput.IsChanged) return;
			
			RootElement.SliceCount = FInput.SliceCount;
			Document.SliceCount = FInput.SliceCount;
			FSuccess.SliceCount = FInput.SliceCount;

			for (int i = 0; i < SpreadMax; i++)
			{
				XElement result = ConvertToXElement(FInput[i]);
				RootElement[i] = result;
				Document[i] = result.Document;
				FSuccess[i] = result != null;				
			}
		}

		XElement ConvertToXElement(string json)
		{
			System.Text.Encoding enc = new System.Text.UTF8Encoding();
			byte[] buffer  = enc.GetBytes(json);
			try {
				XmlReader reader = JsonReaderWriterFactory.CreateJsonReader(buffer, new XmlDictionaryReaderQuotas());
				XElement root = XElement.Load(reader);
				return root;
			}
			catch(Exception e) {
				FLogger.Log(LogType.Error, "Error in AsXElement (JSON): " + e.ToString());
			}
			return null;
		}
	}
}
