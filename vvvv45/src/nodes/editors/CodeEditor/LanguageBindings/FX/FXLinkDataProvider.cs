using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System.Text.RegularExpressions;
using VVVV.Core;

namespace VVVV.HDE.CodeEditor.LanguageBindings.FX
{
	public class FXLinkDataProvider : ILinkDataProvider
	{
		private readonly Regex FLocalIncludePattern = new Regex(@"^\#include\s+""(.+?)""");
		private readonly Regex FGlobalIncludePattern = new Regex(@"^\#include\s+<(.+?)>");
		private readonly string FLocalIncludePath;
		private readonly string FGlobalIncludePath;
		
		public FXLinkDataProvider(string localIncludePath, string globalIncludePath)
		{
			FLocalIncludePath = localIncludePath;
			FGlobalIncludePath = globalIncludePath;
		}
		
		public Link GetLink(IDocument document, TextLocation location)
		{
			if (location.Line >= document.TotalNumberOfLines)
				return Link.Empty;
			
			
			var line = TextUtilities.GetLineAsString(document, location.Line);
			
			string fileName = null;
			int hoverColumn = 0;
			if (FLocalIncludePattern.IsMatch(line))
			{
				fileName = FLocalIncludePattern.Match(line).Groups[1].Value;
				hoverColumn = line.Length - fileName.Length - 1;
				fileName = FLocalIncludePath.ConcatPath(fileName.Replace("/", @"\"));
			}
			else if (FGlobalIncludePattern.IsMatch(line))
			{
				fileName = FGlobalIncludePattern.Match(line).Groups[1].Value;
				hoverColumn = line.Length - fileName.Length - 1;
				fileName = FGlobalIncludePath.ConcatPath(fileName.Replace("/", @"\"));
			}
			
			if (!string.IsNullOrEmpty(fileName))
			{
				var hoverRegion = new TextRegion(location.Line, hoverColumn, location.Line, line.Length - 1);
				
				return new Link(hoverRegion, fileName, new TextLocation(0, 0));
			}
			
			return Link.Empty;
		}
	}
}
