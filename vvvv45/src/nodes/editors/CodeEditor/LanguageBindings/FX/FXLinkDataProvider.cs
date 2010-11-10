using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor.LanguageBindings.FX
{
	public class FXLinkDataProvider : ILinkDataProvider
	{
		public Link GetLink(IDocument document, TextLocation location)
		{
			if (location.Line >= document.TotalNumberOfLines)
				return Link.Empty;
			
			var line = TextUtilities.GetLineAsString(document, location.Line);
			if (line.StartsWith("#include"))
			{
				var fileName = line.Replace("#include", "").Trim(new char[]{'"', ' ', '\t'});
				
				var hoverColumn = line.Length - fileName.Length - 1;
				var hoverRegion = new TextRegion(location.Line, hoverColumn, location.Line, line.Length - 1);
				
				return new Link(hoverRegion, fileName, new TextLocation(0, 0));
			}
			
			return Link.Empty;
		}
	}
}
