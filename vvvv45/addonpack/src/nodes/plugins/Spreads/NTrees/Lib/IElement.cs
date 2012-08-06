using System;
using System.Collections.Generic;
using System.Text;

namespace NTrees.Lib.Base
{
    public interface IElement<T> : IEquals<T>,ICloneable<T>
    {
        //bool IsEqual<E>(E element) where E : IElement;
    }
}
