using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Service
{
    public interface ISourceTypeMapper : IMapper
    {
        Type SourceType { get; }
    }
}
