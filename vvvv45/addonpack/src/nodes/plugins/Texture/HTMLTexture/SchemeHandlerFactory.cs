using Xilium.CefGlue;

namespace VVVV.Nodes.Texture.HTML
{
    internal sealed class SchemeHandlerFactory : CefSchemeHandlerFactory
    {
        public const string SCHEME_NAME = "cef";

        protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
        {
            if (schemeName == SCHEME_NAME)
                return new SchemeHandler();
            return null;
        }
    }
}
