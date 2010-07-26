using System;
using System.IO;
using System.Collections.Generic;
using VVVV.Core.Model;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactory = ICSharpCode.NRefactory;

namespace VVVV.HDE.CodeEditor
{
	public class CodeParser
	{
		protected IParseInfoProvider FParseInfoProvider;
		
		public CodeParser(IParseInfoProvider parseInfoProvider)
		{
			FParseInfoProvider = parseInfoProvider;
		}
		
		public Dom.ParseInformation Parse(ITextDocument document)
		{
			var parseInfo = FParseInfoProvider.GetParseInfo(document);
			var projectContent = FParseInfoProvider.GetProjectContent(document.Project);
			
			var code = document.TextContent;
			var filename = document.Location.LocalPath;
			var oldCompilationUnit = parseInfo.MostRecentCompilationUnit;
			var textReader = new StringReader(code);
			
			using (var parser = NRefactory.ParserFactory.CreateParser(NRefactory.SupportedLanguage.CSharp, textReader)) {
				// we only need to parse types and method definitions, no method bodies
				// so speed up the parser and make it more resistent to syntax
				// errors in methods
				parser.ParseMethodBodies = false;
				parser.Parse();
				
				var newCompilationUnit = ConvertCompilationUnit(parser, projectContent);
				RetrieveRegions(ref newCompilationUnit, parser.Lexer.SpecialTracker);
				
				// Remove information from lastCompilationUnit and add information from newCompilationUnit.
				projectContent.UpdateCompilationUnit(oldCompilationUnit, newCompilationUnit, filename);
				parseInfo.SetCompilationUnit(newCompilationUnit);
			}
			
			return parseInfo;
		}
		
		private Dom.ICompilationUnit ConvertCompilationUnit(NRefactory.IParser parser, Dom.IProjectContent projectContent)
		{
			var visitor = new Dom.NRefactoryResolver.NRefactoryASTConvertVisitor(projectContent);
			visitor.Specials = parser.Lexer.SpecialTracker.CurrentSpecials;
			visitor.VisitCompilationUnit(parser.CompilationUnit, null);
			visitor.Cu.ErrorsDuringCompile = parser.Errors.Count > 0;
			return visitor.Cu;
		}
		
		private void RetrieveRegions(ref Dom.ICompilationUnit cu, NRefactory.Parser.SpecialTracker tracker)
		{
			var directives = new Stack<NRefactory.PreprocessingDirective>();
			var foldingRegions = cu.FoldingRegions;
			var hasErrors = cu.ErrorsDuringCompile;
			// Collect all #region and #endregion directives and push them on a stack.
			foreach (var special in tracker.CurrentSpecials)
			{
				var directive = special as NRefactory.PreprocessingDirective;
				if (directive != null)
				{
					if (directive.Cmd == "#region")
						directives.Push(directive);
					else if (directive.Cmd == "#endregion")
					{
						if (directives.Count > 0)
						{
							var o = directives.Pop();
							var l = Dom.DomRegion.FromLocation(o.StartPosition, directive.EndPosition);
							foldingRegions.Add(new Dom.FoldingRegion(o.Arg.Trim(), l));
						}
					}
				}
			}
		}
	}
}
