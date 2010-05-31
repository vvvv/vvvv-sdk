using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using ICSharpCode.TextEditor.Document;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactory = ICSharpCode.NRefactory;

namespace VVVV.Nodes
{
	public class BackgroundCodeParser
	{
		private BackgroundWorker FBackgroundWorker;
		private Dom.ICompilationUnit FLastCompilationUnit;
		
		private Dom.DefaultProjectContent FProjectContent;
		private Dom.ParseInformation FParseInfo;
		private string FFilename;
		
		public IDocument Document { get; set; }
		
		public BackgroundCodeParser(Dom.DefaultProjectContent projectContent, IDocument document, string filename, Dom.ParseInformation parseInfo)
		{
			FBackgroundWorker = new BackgroundWorker();
			FBackgroundWorker.WorkerReportsProgress = false;
			FBackgroundWorker.WorkerSupportsCancellation = false;
			
			FBackgroundWorker.DoWork += new DoWorkEventHandler(DoWorkCB);
			FBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedCB);
			
			FProjectContent = projectContent;
			Document = document;
			FParseInfo = parseInfo;
			FFilename = filename;
		}
		
		public void RunParserAsync(string code)
		{
			if (!FBackgroundWorker.IsBusy)
				FBackgroundWorker.RunWorkerAsync(code);
		}
		
		private void DoWorkCB(object sender, DoWorkEventArgs args)
		{
			string code = args.Argument as string;
			TextReader textReader = new StringReader(code);
			Dom.ICompilationUnit newCompilationUnit;
			using (NRefactory.IParser p = NRefactory.ParserFactory.CreateParser(NRefactory.SupportedLanguage.CSharp, textReader)) {
				// we only need to parse types and method definitions, no method bodies
				// so speed up the parser and make it more resistent to syntax
				// errors in methods
				p.ParseMethodBodies = false;
				p.Parse();
				
				newCompilationUnit = ConvertCompilationUnit(p.CompilationUnit);
				RetrieveRegions(ref newCompilationUnit, p.Lexer.SpecialTracker);
			}
			// Remove information from lastCompilationUnit and add information from newCompilationUnit.
			FProjectContent.UpdateCompilationUnit(FLastCompilationUnit, newCompilationUnit, FFilename);
			FLastCompilationUnit = newCompilationUnit;
			FParseInfo.SetCompilationUnit(newCompilationUnit);
			// OR (like in SD trunk):
			// FParseInfo = new Dom.ParseInformation(newCompilationUnit);
		}
		
		private void RunWorkerCompletedCB(object sender, RunWorkerCompletedEventArgs args)
		{
			Document.FoldingManager.UpdateFoldings(FFilename, FParseInfo);
		}
		
		private Dom.ICompilationUnit ConvertCompilationUnit(NRefactory.Ast.CompilationUnit cu)
		{
			var visitor = new Dom.NRefactoryResolver.NRefactoryASTConvertVisitor(FProjectContent);
			cu.AcceptVisitor(visitor, null);
			return visitor.Cu;
		}
		
		private void RetrieveRegions(ref Dom.ICompilationUnit cu, ICSharpCode.NRefactory.Parser.SpecialTracker tracker)
		{
			var directives = new Stack<NRefactory.PreprocessingDirective>();
			var foldingRegions = cu.FoldingRegions;
			
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
