using System;
using System.Collections.Generic;
using System.IO;

using ICSharpCode.NRefactory.Ast;
using VVVV.Core.Model.CS;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactory = ICSharpCode.NRefactory;

namespace VVVV.Core.Runtime.CS
{
	public static class CSParser
	{
		/// <summary>
		/// Parses C# code and returns the CSParserResults.
		/// </summary>
		/// <param name="document">The C# code to parse.</param>
		public static CSParserResults Parse(string code, bool parseMethodBodies)
		{
			var textReader = new StringReader(code);
			
			using (var parser = NRefactory.ParserFactory.CreateParser(NRefactory.SupportedLanguage.CSharp, textReader)) {
				// we only need to parse types and method definitions, no method bodies
				// so speed up the parser and make it more resistent to syntax
				// errors in methods
				parser.ParseMethodBodies = parseMethodBodies;
				parser.Parse();
				
				return new CSParserResults(parser);
			}
		}
	}
}
