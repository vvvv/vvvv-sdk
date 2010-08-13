using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using VVVV.Core.Logging;
using VVVV.Core.Model.FX;

namespace VVVV.HDE.CodeEditor.LanguageBindings.FX
{
	public class FXCompletionProvider : DefaultCompletionDataProvider
	{
		protected IDocumentLocator FDocumentLocator;
		protected ILogger FLogger;
		protected Dictionary<string, string> FHLSLReference;
		protected Dictionary<string, string> FTypeReference;
		
		public FXCompletionProvider(IDocumentLocator documentLocator, ILogger logger)
		{
			FDocumentLocator = documentLocator;
			FLogger = logger;
			
			FHLSLReference = new Dictionary<string, string>();
			FTypeReference = new Dictionary<string, string>();
			
			ParseHLSLFunctionReference();
			
			AddTypeToReference("float");
			AddTypeToReference("int");
			AddTypeToReference("bool");
			FTypeReference.Add("float3x4", "");
			FTypeReference.Add("float4x3", "");
		}
		
		private void AddTypeToReference(string type)
		{
			for (int i = 0; i <= 4; i++)
			{
				if (i == 0)
					FTypeReference.Add(type, "");
				else if (i > 1)
					FTypeReference.Add(type + i.ToString(), "");
			}
		}
		
		private void ParseHLSLFunctionReference()
		{
			try
			{
				var functionReference = new XmlDocument();
				var filename = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), @"..\bin\hlsl.fnr"));
				functionReference.Load(filename);
				foreach (XmlNode function in functionReference.DocumentElement.ChildNodes)
				{
					var item = new List<string>();
					foreach (XmlNode cell in function)
						item.Add(cell.InnerText);
					
					//only take functions for shadermodels < 3
					int sm = Convert.ToInt32(item[2][0].ToString());
					if (sm < 4)
						FHLSLReference.Add(item[0], item[1] + "\nMinimum required ShaderModel: " + item[2]);
				}
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Error, @"Error parsing HLSL function reference \effects\hlsl.fnr");
				FLogger.Log(e);
			}
		}
		
		public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
		{
			// We can return code-completion items like this:
			
			//return new ICompletionData[] {
			//	new DefaultCompletionData("Text", "Description", 1)
			//};
			
			//types: everywhere
			//variable names: only inside of single {}
			//hlsl reference: only inside of single {}
			//semantics: only directly after :
			
			//parse ParameterDescription
			var doc = FDocumentLocator.GetVDocument(fileName);
			if (doc == null)
				return new ICompletionData[0];
			
			var inputs = (doc.Project as FXProject).ParameterDescription.Split(new char[1]{'|'});
			int i = 0;
			ICompletionData[] cData = new ICompletionData[FHLSLReference.Count + FTypeReference.Count + inputs.Length-1];
			foreach (var input in inputs)
			{
				if (!string.IsNullOrEmpty(input))
				{
					var desc = input.Split(new char[1]{','});
					string name = "";
					if (desc[0] != desc[1])
						name = desc[1] + "\n";
					
					string tooltip = name + desc[2] + "\n";
					if (Convert.ToInt32(desc[3]) > 1)
						tooltip += desc[6].Replace("(Rows)", "") + desc[3] + "x" + desc[4];
					else
						tooltip += desc[7] + desc[4].Replace("1", "").Replace("0", "");
					cData[i] = new DefaultCompletionData(desc[0], tooltip, 3);
					i++;
				}
			}
			
			foreach(var function in FHLSLReference)
			{
				cData[i] = new DefaultCompletionData(function.Key, function.Value, 1);
				i++;
			}
			
			foreach(var type in FTypeReference)
			{
				cData[i] = new DefaultCompletionData(type.Key, type.Value, 0);
				i++;
			}
			
			//set preselection to "" for popup to sort by first character
			PreSelection = "";
			
			return cData;
		}
	}
}
