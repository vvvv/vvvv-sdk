using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Service
{
    public interface ISourceMapper : ISourceTypeMapper
    {
        object Source { get; }

        ISourceMapper CreateChildMapper(object source);
    }
}
