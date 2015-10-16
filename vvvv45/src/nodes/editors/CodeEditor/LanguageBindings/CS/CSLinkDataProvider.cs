
using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using VVVV.Core.Model.CS;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
	public class CSLinkDataProvider : ILinkDataProvider
	{
		protected CodeEditor FEditor;
		
		public CSLinkDataProvider(CodeEditor editor)
		{
			FEditor = editor;
		}
		
		public Link GetLink(IDocument doc, TextLocation location)
		{
			if (location.Line >= doc.TotalNumberOfLines)
				return Link.Empty;
			
			var lineSegment = doc.GetLineSegment(location.Line);
			var word = lineSegment.GetWord(location.Column);
			
			if (word != null && !word.IsWhiteSpace)
			{
				var csDoc = FEditor.TextDocument as CSDocument;
				
				int offset = lineSegment.Offset + word.Offset + word.Length;
				var expression = csDoc.FindExpression(offset);
				var region = expression.Region;
				
				if (region != null && !region.IsEmpty)
				{
					var resolveResult = csDoc.Resolve(expression);

					if (resolveResult != null)
					{
						var filePosition = resolveResult.GetDefinitionPosition();
						
						if (filePosition != null && !filePosition.IsEmpty)
						{
							var hoverRegion = region.ToTextRegion();
							var fileName = filePosition.FileName;
							var destination = filePosition.Position.ToTextLocation();
							
							return new Link(hoverRegion, fileName, destination);
						}
					}
				}
			}

			return Link.Empty;
		}
	}
}
