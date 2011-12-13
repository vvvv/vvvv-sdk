using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Service
{
    class SourceTypeMapper : Mapper, ISourceTypeMapper
    {
        public SourceTypeMapper(Type sourceType)
        {
            SourceType = sourceType;
        }

        #region ISourceTypeMapper Members

        public Type SourceType { get; private set; }

        #endregion
    }
}
