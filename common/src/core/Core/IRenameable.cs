using System;

namespace VVVV.Core
{
	public interface IRenameable : IIDItem
	{
	    new string Name
	    {
	        get;
	        set;
	    }

        bool CanRenameTo(string value);
	}
}
