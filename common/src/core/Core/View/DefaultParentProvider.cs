/*
 * Erstellt mit SharpDevelop.
 * Benutzer: gregsn
 * Datum: 20.12.2010
 * Zeit: 19:25
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections;

using VVVV.Core;

namespace VVVV.Core.View
{
	/// <summary>
	/// Description of DefaultParentProvider.
	/// </summary>
	public class DefaultParentProvider : IParent
	{
		public IEnumerable Childs
		{
			get; private set;			
		}

        public DefaultParentProvider(ModelMapper mapper)
        {
        	if (mapper.CanMap<IEnumerable>())
				Childs = mapper.Map<IEnumerable>(); 
        	else 
        	if (mapper.Model is IEnumerable)
        		Childs = mapper.Model as IEnumerable;
        	else
        		Childs = null; 
        }
	}
}
