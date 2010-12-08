using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.View.Table;

namespace VVVV.HDE.CodeEditor.ErrorView
{
	/// <summary>
	/// Describes a compiler error.
	/// </summary>
	public class ErrorCellProvider : IEnumerable<Cell>
	{
		protected CompilerError FError;
		
		public ErrorCellProvider(CompilerError error)
		{
			FError = error;
		}
		
		public IEnumerator<Cell> GetEnumerator()
		{
			yield return new Cell(FError.IsWarning ? "W" : "E");
			yield return new Cell(FError.Line);
			yield return new Cell(FError.ErrorText, true);
			if (FError.FileName != null && FError.FileName.Length > 0)
			{
				yield return new Cell(Path.GetFileName(FError.FileName));
				yield return new Cell(Path.GetDirectoryName(FError.FileName));
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
