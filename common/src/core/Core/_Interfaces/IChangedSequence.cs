using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Collections
{
    public interface IChangedSequence<out T> : IEnumerable<T>
    {
        bool Changed { get; }
    }
}
