using System;
using System.Collections.Generic;
using System.Text;

namespace NTrees.Lib.Base
{
    public interface IBounds<T> where T : IElement<T> 
    {
        //To check if point inside bounds
        bool IsInside(T point);

        //Bounds Center
        T Center { get; }


    }
}
