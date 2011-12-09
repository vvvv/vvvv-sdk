using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.Parser;

namespace VVVV.Core.Runtime.CS
{
	public class CSParserResults
	{
		public CompilationUnit CompilationUnit
		{
			get;
			private set;
		}
		
		public bool HasErrors
		{
			get;
			private set;
		}
		
		public List<ISpecial> Specials
		{
			get;
			private set;
		}
		
		public List<TagComment> TagComments
		{
			get;
			private set;
		}
		
		public CSParserResults(IParser parser)
			: this(parser.CompilationUnit, parser.Errors.Count > 0, parser.Lexer.SpecialTracker.CurrentSpecials, parser.Lexer.TagComments)
		{
		}
		
		public CSParserResults(CompilationUnit compilationUnit, bool hasErrors, List<ISpecial> specials, List<TagComment> tagComments)
		{
			CompilationUnit = compilationUnit;
			HasErrors = hasErrors;
			Specials = specials;
			TagComments = tagComments;
		}
	}
}
