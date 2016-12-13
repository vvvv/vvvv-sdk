using System;

namespace VVVV.Core
{
	public interface IRenameable
	{
	    string Name
	    {
	        get;
	        set;
	    }

        bool CanRenameTo(string value);
	}
}
