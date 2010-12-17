
using System;
using System.Collections.Generic;
using VVVV.Core.View.Table;

namespace VVVV.HDE.CodeEditor.ErrorView
{
	public class ErrorCollectionColumnProvider : IEnumerable<Column>
	{
		public IEnumerator<Column> GetEnumerator()
		{
			yield return new Column("!");
			yield return new Column("Line");
			yield return new Column("Message", AutoSizeColumnMode.Fill);
			yield return new Column("File", AutoSizeColumnMode.AllCells);
			yield return new Column("Path", AutoSizeColumnMode.None);
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
