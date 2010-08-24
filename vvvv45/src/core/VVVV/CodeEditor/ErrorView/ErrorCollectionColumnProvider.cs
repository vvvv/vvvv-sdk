
using System;
using System.Collections.Generic;
using VVVV.Core.View.Table;

namespace VVVV.HDE.CodeEditor.ErrorView
{
	public class ErrorCollectionColumnProvider : IEnumerable<IColumn>
	{
		public IEnumerator<IColumn> GetEnumerator()
		{
			yield return new Column("!", 3f);
			yield return new Column("Line", 7f);
			yield return new Column("Message", 55f);
			yield return new Column("File", 15f);
			yield return new Column("Path", 15f);
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
