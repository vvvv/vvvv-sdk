using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.Runtime;
using VVVV.Core.View.Table;

namespace VVVV.HDE.CodeEditor.ErrorView
{
	/// <summary>
	/// Describes a runtime error.
	/// </summary>
	public class RuntimeErrorCellProvider : IEnumerable<ICell>
	{
		protected RuntimeError FError;
		
		public RuntimeErrorCellProvider(RuntimeError error)
		{
			FError = error;
		}
		
		public IEnumerator<ICell> GetEnumerator()
		{
		    yield return new Cell("R", typeof(string));
			yield return new Cell(FError.Line, typeof(int));
			yield return new Cell(FError.ErrorText, typeof(string), true);
			if (FError.FileName != null && FError.FileName.Length > 0)
			{
				yield return new Cell(Path.GetFileName(FError.FileName), typeof(string));
				yield return new Cell(Path.GetDirectoryName(FError.FileName), typeof(string));
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
