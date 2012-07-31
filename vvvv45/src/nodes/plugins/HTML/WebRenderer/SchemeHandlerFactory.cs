using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CefGlue;

namespace VVVV.Nodes.HTML
{
    internal sealed class SchemeHandlerFactory : CefSchemeHandlerFactory
    {
        protected override CefSchemeHandler Create(CefBrowser browser, string schemeName, CefRequest request)
        {
            return new SchemeHandler();
        }
    }
}
