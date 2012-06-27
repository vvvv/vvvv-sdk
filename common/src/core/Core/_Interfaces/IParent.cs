/*
 * Erstellt mit SharpDevelop.
 * Benutzer: gregsn
 * Datum: 13.12.2010
 * Zeit: 23:19
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections;

namespace VVVV.Core
{
	/// <summary>
	/// Just to delegate child collection implementation to a field
	/// </summary>
	public interface IParent
	{
		IEnumerable Childs
		{
			get;
		}
	}
}
