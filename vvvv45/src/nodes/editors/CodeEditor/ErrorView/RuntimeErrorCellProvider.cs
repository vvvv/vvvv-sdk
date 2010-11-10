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
			yield return new Cell("R");
			yield return new Cell(FError.Line);
			yield return new Cell(FError.ErrorText);
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
