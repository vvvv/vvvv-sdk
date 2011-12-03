using System;

namespace VVVV.Core.View
{
    public interface ILinkSink
    {
        bool Accepts(ILinkSource source);
    }
}
