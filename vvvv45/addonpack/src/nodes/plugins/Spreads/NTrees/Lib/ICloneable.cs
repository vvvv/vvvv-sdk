using System;
using System.Collections.Generic;
using System.Text;
using NTrees.Lib.Base;

namespace NTrees.Lib.Base
{
    public interface ICloneable<T>
    {
        T Clone();
    }

    public interface ISpliteable<T>
    {
        T[] Split();
    }

    public interface IEquals<T>
    {
        bool IsEquals(T element);
    }
}
