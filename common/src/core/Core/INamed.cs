using System;

namespace VVVV.Core
{
    public delegate void RenamedHandler(INamed sender, string newName);

    public interface INamed 
    {
        string Name
        {
            get;
        }
        
        event RenamedHandler Renamed;
    }
}
